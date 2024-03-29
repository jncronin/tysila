﻿/* Copyright (C) 2016 by John Cronin
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using libtysila5.util;
using metadata;

namespace libtysila5
{
    public partial class libtysila
    {
        public static bool AssembleMethod(metadata.MethodSpec ms,
            binary_library.IBinaryFile bf, target.Target t,
            TysilaState s,
            StringBuilder debug_passes = null,
            MetadataStream base_m = null,
            Code code_override = null,
            binary_library.ISection ts = null,
            binary_library.ISection data_sect = null,
            dwarf.DwarfCU dcu = null)
        {
            if(ms.ret_type_needs_boxing)
            {
                throw new Exception("AssembleMethod called for box rettype stub - use AssembleBoxedMethod instead");
            }
            if (ms.is_boxed)
            {
                throw new Exception("AssembleMethod called for boxed method - use AssembleBoxedMethod instead");
            }

            if (ts == null)
            {
                ts = bf.GetTextSection();
            }
            s.bf = bf;
            s.text_section = ts;
            binary_library.SymbolType sym_st = binary_library.SymbolType.Global;

            var csite = ms.msig;
            var mdef = ms.mdrow;
            var m = ms.m;

            /* Don't compile if not for this architecture */
            if (!t.IsMethodValid(ms))
                return false;

            // Get method RVA, don't compile if no body
            var rva = m.GetIntEntry(metadata.MetadataStream.tid_MethodDef,
                mdef, 0);

            // New signature table
            s.sigt = new SignatureTable(ms.MangleMethod());

            /* Is this an array method? */
            if (rva == 0 &&
                ms.type != null &&
                ms.type.stype == TypeSpec.SpecialType.Array &&
                code_override == null)
            {
                if (ms.name_override == "Get")
                {
                    code_override = ir.ConvertToIR.CreateArrayGet(ms, t, s);
                }
                else if (ms.name_override == ".ctor")
                {
                    // there are two constructors, choose the correct one
                    var pcount = ms.m.GetMethodDefSigParamCount(ms.msig);

                    if (pcount == ms.type.arr_rank)
                        code_override = ir.ConvertToIR.CreateArrayCtor1(ms, t, s);
                    else if (pcount == 2 * ms.type.arr_rank)
                        code_override = ir.ConvertToIR.CreateArrayCtor2(ms, t, s);
                    else
                        throw new NotSupportedException("Invalid number of parameters to " + ms.MangleMethod() + " for array of rank " + ms.type.arr_rank.ToString());
                }
                else if (ms.name_override == "Set")
                    code_override = ir.ConvertToIR.CreateArraySet(ms, t, s);
                else if (ms.name_override == "Address")
                    code_override = ir.ConvertToIR.CreateArrayAddress(ms, t, s);
                else
                    throw new NotImplementedException(ms.name_override);

                sym_st = binary_library.SymbolType.Weak;
            }

            /* Is this a vector method? */
            if (rva == 0 &&
                ms.type != null &&
                ms.type.stype == TypeSpec.SpecialType.SzArray &&
                code_override == null)
            {
                if (ms.Name == "IndexOf")
                    code_override = ir.ConvertToIR.CreateVectorIndexOf(ms, t, s);
                else if (ms.Name == "Insert")
                    code_override = ir.ConvertToIR.CreateVectorInsert(ms, t, s);
                else if (ms.Name == "RemoveAt")
                    code_override = ir.ConvertToIR.CreateVectorRemoveAt(ms, t, s);
                else if (ms.Name == "get_Item")
                    code_override = ir.ConvertToIR.CreateVectorget_Item(ms, t, s);
                else if (ms.Name == "set_Item")
                    code_override = ir.ConvertToIR.CreateVectorset_Item(ms, t, s);
                else if (ms.Name == "get_Count")
                    code_override = ir.ConvertToIR.CreateVectorget_Count(ms, t, s);
                else if (ms.Name == "CopyTo")
                    code_override = ir.ConvertToIR.CreateVectorCopyTo(ms, t, s);
                /*else if (ms.Name == "GetEnumerator")
                    code_override = ir.ConvertToIR.CreateVectorGetEnumerator(ms, t);*/
                else if (ms.Name == "Add" ||
                    ms.Name == "Clear" ||
                    ms.Name == "Contains" ||
                    ms.Name == "Remove" ||
                    ms.Name == "get_IsReadOnly" ||
                    ms.Name == "get_IsSynchronized" ||
                    ms.Name == "get_SyncRoot" ||
                    ms.Name == "get_IsFixedSize" ||
                    ms.Name == "Clone" ||
                    ms.Name == "CompareTo" ||
                    ms.Name == "GetHashCode" ||
                    ms.Name == "Equals")
                    code_override = ir.ConvertToIR.CreateVectorUnimplemented(ms, t, s);
                else
                    return false;

                sym_st = binary_library.SymbolType.Weak;
            }

            if (rva == 0 && code_override == null)
                return false;

            // Get mangled name for defining a symbol
            List<binary_library.ISymbol> meth_syms = new List<binary_library.ISymbol>();
            var mangled_name = m.MangleMethod(ms);
            var meth_sym = bf.CreateSymbol();
            meth_sym.Name = mangled_name;
            meth_sym.ObjectType = binary_library.SymbolObjectType.Function;
            meth_sym.Offset = (ulong)ts.Data.Count;
            meth_sym.Type = binary_library.SymbolType.Global;
            ts.AddSymbol(meth_sym);
            meth_syms.Add(meth_sym);

            foreach (var alias in ms.MethodAliases)
            {
                var alias_sym = bf.CreateSymbol();
                alias_sym.Name = alias;
                alias_sym.ObjectType = binary_library.SymbolObjectType.Function;
                alias_sym.Offset = (ulong)ts.Data.Count;
                alias_sym.Type = binary_library.SymbolType.Global;
                ts.AddSymbol(alias_sym);
                meth_syms.Add(alias_sym);
            }

            if (ms.HasCustomAttribute("_ZN14libsupcs#2Edll8libsupcs20WeakLinkageAttribute_7#2Ector_Rv_P1u1t"))
                sym_st = binary_library.SymbolType.Weak;
            if (base_m != null && ms.m != base_m)
                sym_st = binary_library.SymbolType.Weak;
            foreach (var sym in meth_syms)
                sym.Type = sym_st;

            // Get signature if not specified
            if (csite == 0)
            {
                csite = (int)m.GetIntEntry(metadata.MetadataStream.tid_MethodDef,
                    mdef, 4);
            }

            Code cil;
            if (code_override == null)
            {
                var meth = m.GetRVA(rva);

                var flags = meth.ReadByte(0);
                int max_stack = 0;
                long code_size = 0;
                long lvar_sig_tok = 0;
                int boffset = 0;
                List<metadata.ExceptionHeader> ehdrs = null;
                bool has_exceptions = false;

                if ((flags & 0x3) == 0x2)
                {
                    // Tiny header
                    code_size = flags >> 2;
                    max_stack = 8;
                    boffset = 1;
                }
                else if ((flags & 0x3) == 0x3)
                {
                    // Fat header
                    uint fat_flags = meth.ReadUShort(0) & 0xfffU;
                    int fat_hdr_len = (meth.ReadUShort(0) >> 12) * 4;
                    max_stack = meth.ReadUShort(2);
                    code_size = meth.ReadUInt(4);
                    lvar_sig_tok = meth.ReadUInt(8);
                    boffset = fat_hdr_len;

                    if ((flags & 0x8) == 0x8)
                    {
                        has_exceptions = true;

                        ehdrs = new List<metadata.ExceptionHeader>();

                        int ehdr_offset = boffset + (int)code_size;
                        ehdr_offset = util.util.align(ehdr_offset, 4);

                        while (true)
                        {
                            int kind = meth.ReadByte(ehdr_offset);

                            if ((kind & 0x1) != 0x1)
                                throw new Exception("Invalid exception header");

                            bool is_fat = false;
                            if ((kind & 0x40) == 0x40)
                                is_fat = true;

                            int data_size = meth.ReadInt(ehdr_offset);
                            data_size >>= 8;
                            if (is_fat)
                                data_size &= 0xffffff;
                            else
                                data_size &= 0xff;

                            int clause_count;
                            if (is_fat)
                                clause_count = (data_size - 4) / 24;
                            else
                                clause_count = (data_size - 4) / 12;

                            ehdr_offset += 4;
                            for (int i = 0; i < clause_count; i++)
                            {
                                var ehdr = ParseExceptionHeader(meth,
                                    ref ehdr_offset, is_fat, ms);
                                ehdr.EhdrIdx = i;
                                ehdrs.Add(ehdr);
                            }

                            if ((kind & 0x80) != 0x80)
                                break;
                        }
                    }
                }
                else
                    throw new Exception("Invalid method header flags");

                /* Parse CIL code */
                cil = libtysila5.cil.CilParser.ParseCIL(meth,
                    ms, boffset, (int)code_size, lvar_sig_tok,
                    has_exceptions, ehdrs);
                cil.s = s;

                /* Allocate local vars and args */
                t.AllocateLocalVarsArgs(cil);

                /* Convert to IR */
                cil.t = t;
                ir.ConvertToIR.DoConversion(cil);
            }
            else
                cil = code_override;


            /* Allocate registers */
            ir.AllocRegs.DoAllocation(cil);


            /* Choose instructions */
            target.ChooseInstructions.DoChoosing(cil);
            t.AssemblePass(cil);
            //((target.x86.x86_Assembler)cil.t).AssemblePass(cil);

            foreach (var sym in meth_syms)
                sym.Size = ts.Data.Count - (int)sym.Offset;

            foreach (var extra_sym in cil.extra_labels)
            {
                var esym = bf.CreateSymbol();
                esym.Name = extra_sym.Name;
                esym.ObjectType = binary_library.SymbolObjectType.Function;
                esym.Offset = (ulong)extra_sym.Offset;
                esym.Type = sym_st;
                ts.AddSymbol(esym);
            }

            /* Dump debug */
            DumpDebug(debug_passes, meth_syms, cil);

            /* Signature table */
            if (data_sect == null)
                data_sect = bf.GetDataSection();
            s.sigt.WriteToOutput(bf, ms.m, t, data_sect);

            /* DWARF output */
            if (dcu != null)
            {
                var ddie = dcu.GetMethodDie(ms);
                ddie.sym = meth_syms[0];
                ddie.cil = cil;

                var ddts = dcu.GetTypeDie(ms.type) as dwarf.DwarfParentDIE;
                ddts.Children.Add(ddie);

                if(ms.ReturnType != null)
                {
                    dcu.GetTypeDie(ms.ReturnType);
                    if (ms.ReturnType.stype == TypeSpec.SpecialType.None && !ms.ReturnType.IsValueType)
                        dcu.GetTypeDie(ms.ReturnType.Pointer);
                }
                foreach(var la in cil.la_types)
                {
                    dcu.GetTypeDie(la);
                    if (la.stype == TypeSpec.SpecialType.None && !la.IsValueType)
                        dcu.GetTypeDie(la.Pointer);
                }
                foreach(var lv in cil.lv_types)
                {
                    dcu.GetTypeDie(lv);
                    if (lv.stype == TypeSpec.SpecialType.None && !lv.IsValueType)
                        dcu.GetTypeDie(lv.Pointer);
                }

                if(!ms.IsStatic)
                {
                    dcu.GetTypeDie(ms.type.Pointer);
                }

                // do we have source lines?
                if(ms.m.pdb != null && ms.mdrow > 0)
                {
                    var sps = ms.m.pdb.DebugGetSeqPtForMDRow(ms.mdrow);
                    if (sps != null && cil.cil != null && cil.cil.Count > 0)
                    {
                        /* Start building a Line Number Program
                         * 
                         * We follow the example in DWARF4 D.5 (p251), with a few changes
                         * 
                         * header - defined per CU
                         *   - add this/these source files to it if necessary
                         *                          
                         * DW_LNE_set_address - psize 0s, reloc to mangled name
                         * DW_LNS_advance_pc - mc_offset of first cil instruction
                         * DW_LNS_set_prologue_end
                         * DW_LNS_set_file - source file
                         * SPECIAL(0,0) - add first opcode
                         * SPECIAL(x,y) - add rest of opcodes
                         * DW_LNE_end_sequence
                         * 
                         */

                        string cur_file = null;
                        int cur_line = 1;
                        int cur_mc = 0;
                        int cur_col = 0;

                        var lnp = dcu.lnp;

                        // DW_LNE_set_address
                        lnp.AddRange(new byte[] { 0x00, (byte)(t.psize + 1), 0x02 });
                        dcu.lnp_relocs.Add(lnp.Count, meth_syms[0]);
                        for (int i = 0; i < t.psize; i++)
                            lnp.Add(0);

                        // DW_LNS_advance_pc
                        lnp.Add(0x02);
                        dwarf.DwarfDIE.w(lnp, (uint)cil.cil[0].mc_offset);
                        cur_mc = cil.cil[0].mc_offset;

                        // DW_LNS_set_prologue_end
                        lnp.Add(0x0a);
                        
                        foreach(var cn in cil.cil)
                        {
                            var mc_advance = cn.mc_offset - cur_mc;

                            // get current line number
                            MetadataStream.SeqPt csp = null;
                            foreach(var sp in sps)
                            {
                                if(sp.IlOffset == cn.il_offset &
                                    !sp.IsHidden)
                                {
                                    csp = sp;
                                    break;
                                }
                            }
                            if(csp != null)
                            {
                                var line_advance = csp.StartLine - cur_line;

                                if (ddie.StartLine == 0)
                                    ddie.StartLine = csp.StartLine;
                                if (ddie.StartColumn == 0)
                                    ddie.StartColumn = csp.StartCol;

                                if(csp.DocName != cur_file)
                                {
                                    // DW_LNS_set_file
                                    lnp.Add(0x04);

                                    uint file_no;
                                    if(!dcu.lnp_files.TryGetValue(csp.DocName, out file_no))
                                    {
                                        file_no = (uint)(dcu.lnp_files.Count + 1);
                                        dcu.lnp_files[csp.DocName] = file_no;
                                        dcu.lnp_fnames.Add(csp.DocName);
                                    }
                                    if (ddie.SourceFileId == 0)
                                        ddie.SourceFileId = (int)file_no;

                                    dwarf.DwarfDIE.w(lnp, file_no);

                                    cur_file = csp.DocName;
                                }

                                if(csp.StartCol != cur_col)
                                {
                                    // DW_LNS_set_column
                                    lnp.Add(0x05);
                                    dwarf.DwarfDIE.w(lnp, (uint)csp.StartCol);
                                    cur_col = csp.StartCol;
                                }

                                /* SPECIAL if possible
                                 *  Use example on DWARF4 p132:
                                 *      opcode_base = 13, line_base = -3, line_range = 12
                                 *  
                                 *  Gives a line advance range of [-3,8] and op_advance [0,19]
                                */

                                if((line_advance >= dcu.line_base && line_advance < (dcu.line_base + dcu.line_range)) &&
                                    (mc_advance >= 0 && mc_advance < dcu.mc_advance_max))
                                {
                                    var spec_opcode = (line_advance - dcu.line_base) +
                                        (dcu.line_range * mc_advance) + dcu.opcode_base;
                                    lnp.Add((byte)spec_opcode);
                                }
                                else
                                {
                                    // DW_LNS_advance_pc
                                    lnp.Add(0x02);
                                    dwarf.DwarfDIE.w(lnp, (uint)mc_advance);

                                    // DW_LNS_advance_line
                                    lnp.Add(0x03);
                                    dwarf.DwarfDIE.w(lnp, line_advance);

                                    // SPECIAL(0,0)
                                    var spec_opcode = (0 - dcu.line_base) + dcu.opcode_base;
                                    lnp.Add((byte)spec_opcode);
                                }

                                cur_line += line_advance;
                                cur_mc += mc_advance;


                            }
                        }

                        // Advance to end
                        var to_advance = (int)meth_syms[0].Size - cur_mc;
                        lnp.Add(0x02);
                        dwarf.DwarfDIE.w(lnp, to_advance);

                        // DW_LNE_end_sequence
                        lnp.Add(0x00);
                        lnp.Add(0x01);
                        lnp.Add(0x01);

                    }
                }
            }

            return true;
        }

        private static void DumpDebug(StringBuilder debug_passes,
            List<binary_library.ISymbol> meth_syms,
            Code cil)
        {
            if (debug_passes != null)
            {
                libtysila5.cil.CilNode cur_cil_node = null;
                libtysila5.cil.CilNode.IRNode cur_ir_node = null;

                foreach (var sym in meth_syms)
                    debug_passes.AppendLine(sym.Name + ":");

                foreach (var mc in cil.mc)
                {
                    if (mc.parent != cur_ir_node)
                    {
                        cur_ir_node = mc.parent;

                        if (cur_ir_node.parent != cur_cil_node)
                        {
                            cur_cil_node = cur_ir_node.parent;

                            debug_passes.Append("  IL");
                            debug_passes.Append(cur_cil_node.il_offset.ToString("X4"));
                            debug_passes.Append(": ");
                            debug_passes.AppendLine(cur_cil_node.ToString());
                        }

                        debug_passes.Append("    ");
                        debug_passes.AppendLine(cur_ir_node.ToString());
                    }
                    debug_passes.Append("      ");
                    debug_passes.Append(mc.offset.ToString("X8"));
                    debug_passes.Append(": ");
                    debug_passes.AppendLine(cil.t.MCInstToDebug(mc));
                    if (cil.s != null && cil.s.text_section != null && cil.s.text_section.Data != null)
                    {
                        debug_passes.Append("        ");
                        int idx = 0;
                        for (int i = mc.addr; i < mc.end_addr; i++, idx++)
                        {
                            if (idx > 0)
                                debug_passes.Append(", ");
                            debug_passes.Append(cil.s.text_section.Data[i].ToString("X2"));
                        }
                        debug_passes.AppendLine();
                    }
                }
                debug_passes.AppendLine();
                debug_passes.AppendLine();
            }

        }

        private static metadata.ExceptionHeader ParseExceptionHeader(metadata.DataInterface di,
            ref int ehdr_offset, bool is_fat,
            metadata.MethodSpec ms)
        {
            metadata.ExceptionHeader ehdr = new metadata.ExceptionHeader();
            int flags = 0;
            if (is_fat)
            {
                flags = di.ReadInt(ehdr_offset);
                ehdr.TryILOffset = di.ReadInt(ehdr_offset + 4);
                ehdr.TryLength = di.ReadInt(ehdr_offset + 8);
                ehdr.HandlerILOffset = di.ReadInt(ehdr_offset + 12);
                ehdr.HandlerLength = di.ReadInt(ehdr_offset + 16);
            }
            else
            {
                flags = di.ReadShort(ehdr_offset);
                ehdr.TryILOffset = di.ReadShort(ehdr_offset + 2);
                ehdr.TryLength = di.ReadByte(ehdr_offset + 4);
                ehdr.HandlerILOffset = di.ReadShort(ehdr_offset + 5);
                ehdr.HandlerLength = di.ReadByte(ehdr_offset + 7);
            }

            switch (flags)
            {
                case 0:
                    ehdr.EType = metadata.ExceptionHeader.ExceptionHeaderType.Catch;
                    uint mtoken;
                    if (is_fat)
                        mtoken = di.ReadUInt(ehdr_offset + 20);
                    else
                        mtoken = di.ReadUInt(ehdr_offset + 8);

                    int table_id, row;
                    ms.m.InterpretToken(mtoken, out table_id, out row);
                    ehdr.ClassToken = ms.m.GetTypeSpec(table_id, row,
                        ms.gtparams, ms.gmparams);
                    break;
                case 1:
                    ehdr.EType = metadata.ExceptionHeader.ExceptionHeaderType.Filter;
                    if (is_fat)
                        ehdr.FilterOffset = di.ReadInt(ehdr_offset + 20);
                    else
                        ehdr.FilterOffset = di.ReadInt(ehdr_offset + 8);
                    break;
                case 2:
                    ehdr.EType = metadata.ExceptionHeader.ExceptionHeaderType.Finally;
                    break;
                case 4:
                    ehdr.EType = metadata.ExceptionHeader.ExceptionHeaderType.Fault;
                    break;
                default:
                    throw new Exception("Invalid exception type: " + flags.ToString());
            }

            if (is_fat)
                ehdr_offset += 24;
            else
                ehdr_offset += 12;

            return ehdr;
        }

        public static bool AssembleBoxedMethod(metadata.MethodSpec ms,
            binary_library.IBinaryFile bf, target.Target t,
            TysilaState s,
            StringBuilder debug_passes = null,
            binary_library.ISection ts = null)
        {
            if(ts == null)
                ts = bf.GetTextSection();
            s.bf = bf;
            s.text_section = ts;

            // Symbol
            List<binary_library.ISymbol> meth_syms = new List<binary_library.ISymbol>();
            var mangled_name = ms.MangleMethod();
            var meth_sym = bf.CreateSymbol();
            meth_sym.Name = mangled_name;
            meth_sym.ObjectType = binary_library.SymbolObjectType.Function;
            meth_sym.Offset = (ulong)ts.Data.Count;
            meth_sym.Type = binary_library.SymbolType.Weak;
            ts.AddSymbol(meth_sym);
            meth_syms.Add(meth_sym);

            Code c = ms.ret_type_needs_boxing ? t.AssembleBoxRetTypeMethod(ms, s) : t.AssembleBoxedMethod(ms, s);
            c.t = t;
            t.AssemblePass(c);

            foreach (var sym in meth_syms)
                sym.Size = ts.Data.Count - (int)sym.Offset;

            DumpDebug(debug_passes, meth_syms, c);

            return true;
        }
    }
}
