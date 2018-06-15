/* Copyright (C) 2017 by John Cronin
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

namespace libtysila5.cil
{
    class CilParser
    {
        static internal Code ParseCIL(metadata.DataInterface di,
            metadata.MethodSpec ms, int boffset, int length,
            long lvar_sig_tok, bool has_exceptions = false,
            List<metadata.ExceptionHeader> ehdrs = null)
        {
            Code ret = new Code();
            ret.cil = new List<CilNode>();
            ret.ms = ms;

            int table_id;
            int row;
            ms.m.InterpretToken((uint)lvar_sig_tok,
                out table_id, out row);
            int idx = (int)ms.m.GetIntEntry(table_id, row, 0);

            var rtsig = ms.m.GetMethodDefSigRetTypeIndex(ms.msig);
            ret.ret_ts = ms.m.GetTypeSpec(ref rtsig, ms.gtparams, ms.gmparams);

            ret.lvar_sig_tok = idx;

            Dictionary<int, List<int>> offsets_before =
                new Dictionary<int, List<int>>(new GenericEqualityComparer<int>());

            // First, generate CilNodes for each instruction
            int offset = 0;
            while (offset < length)
            {
                CilNode n = new CilNode(ms, offset);

                /* Determine try block starts */
                if (ehdrs != null)
                {
                    foreach (var ehdr in ehdrs)
                    {
                        if (ehdr.TryILOffset == offset)
                        {
                            n.try_starts.Insert(0, ehdr);
                        }
                        if (ehdr.HandlerILOffset == offset ||
                            (ehdr.EType == metadata.ExceptionHeader.ExceptionHeaderType.Filter && ehdr.FilterOffset == offset))
                        {
                            n.handler_starts.Add(ehdr);
                        }
                        if(offset >= ehdr.HandlerILOffset &&
                            offset < ehdr.HandlerILOffset + ehdr.HandlerLength)
                        {
                            n.is_in_excpt_handler = true;
                        }
                    }
                }

                /* Parse prefixes */
                bool cont = true;
                while (cont)
                {
                    if (di.ReadByte(offset + boffset) == 0xfe)
                    {
                        switch (di.ReadByte(offset + boffset + 1))
                        {
                            case 0x16:
                                n.constrained = true;
                                offset += 2;
                                n.constrained_tok = di.ReadUInt(offset + boffset);
                                offset += 4;
                                break;
                            case 0x19:
                                if ((di.ReadByte(offset + boffset + 2) & 0x01) == 0x01)
                                    n.no_typecheck = true;
                                if ((di.ReadByte(offset + boffset + 2) & 0x02) == 0x02)
                                    n.no_rangecheck = true;
                                if ((di.ReadByte(offset + boffset + 2) & 0x04) == 0x04)
                                    n.no_nullcheck = true;
                                offset += 3;
                                break;
                            case 0x1e:
                                n.read_only = true;
                                offset += 2;
                                break;
                            case 0x14:
                                n.tail = true;
                                offset += 2;
                                break;
                            case 0x12:
                                n.unaligned = true;
                                n.unaligned_alignment = di.ReadByte(offset + boffset + 2);
                                offset += 3;
                                break;
                            case 0x13:
                                n.volatile_ = true;
                                offset += 2;
                                break;
                            default:
                                cont = false;
                                break;
                        }
                    }
                    else
                        cont = false;
                }

                /* Parse opcode */
                if (di.ReadByte(offset + boffset) == (int)Opcode.SingleOpcodes.double_)
                {
                    offset++;
                    n.opcode = OpcodeList.Opcodes[0xfe00 + di.ReadByte(offset + boffset)];
                }
                else if (di.ReadByte(offset + boffset) == (int)Opcode.SingleOpcodes.tysila)
                {
                    //if (opts.AllowTysilaOpcodes)
                    //{
                    offset++;
                    n.opcode = OpcodeList.Opcodes[0xfd00 + di.ReadByte(offset + boffset)];
                    //}
                    //else
                    //    throw new UnauthorizedAccessException("Opcodes in the range 0xfd00 - 0xfdff are not allowed in user code");
                }
                else
                    n.opcode = OpcodeList.Opcodes[di.ReadByte(offset + boffset)];
                offset++;

                /* Parse immediate operands */
                switch (n.opcode.inline)
                {
                    case Opcode.InlineVar.InlineBrTarget:
                    case Opcode.InlineVar.InlineI:
                    case Opcode.InlineVar.InlineField:
                    case Opcode.InlineVar.InlineMethod:
                    case Opcode.InlineVar.InlineSig:
                    case Opcode.InlineVar.InlineString:
                    case Opcode.InlineVar.InlineTok:
                    case Opcode.InlineVar.InlineType:
                    case Opcode.InlineVar.ShortInlineR:
                        n.inline_int = di.ReadInt(offset + boffset);
                        n.inline_uint = di.ReadUInt(offset + boffset);
                        n.inline_long = n.inline_int;
                        n.inline_val = new byte[4];
                        for (int i = 0; i < 4; i++)
                            n.inline_val[i] = di.ReadByte(offset + boffset + i);
                        offset += 4;

                        if (n.opcode.inline == Opcode.InlineVar.ShortInlineR)
                        {
                            unsafe
                            {
                                fixed (int* ii = &n.inline_int)
                                {
                                    fixed (float* ifl = &n.inline_float)
                                    {
                                        *(int*)ifl = *ii;
                                    }
                                }
                            }
                            n.inline_double = n.inline_float;
                        }
                        break;
                    case Opcode.InlineVar.InlineI8:
                    case Opcode.InlineVar.InlineR:
                        n.inline_int = di.ReadInt(offset + boffset);
                        n.inline_uint = di.ReadUInt(offset + boffset);
                        n.inline_long = di.ReadLong(offset + boffset);
                        n.inline_val = new byte[8];
                        for (int i = 0; i < 8; i++)
                            n.inline_val[i] = di.ReadByte(offset + boffset + i);
                        offset += 8;

                        if (n.opcode.inline == Opcode.InlineVar.InlineR)
                        {
                            unsafe
                            {
                                fixed (long* ii = &n.inline_long)
                                {
                                    fixed (double* ifl = &n.inline_double)
                                    {
                                        *(long*)ifl = *ii;
                                    }
                                }
                            }
                            n.inline_float = (float)n.inline_double;
                        }

                        break;
                    case Opcode.InlineVar.InlineVar:
                        //line.inline_int = LSB_Assembler.FromByteArrayI2S(code, offset);
                        //line.inline_uint = LSB_Assembler.FromByteArrayU2S(code, offset);
                        //line.inline_val = new byte[2];
                        //LSB_Assembler.SetByteArrayS(line.inline_val, 0, code, offset, 2);
                        throw new NotImplementedException();
                        offset += 2;
                        break;
                    case Opcode.InlineVar.ShortInlineBrTarget:
                    case Opcode.InlineVar.ShortInlineI:
                    case Opcode.InlineVar.ShortInlineVar:
                        n.inline_int = di.ReadSByte(offset + boffset);
                        n.inline_uint = di.ReadByte(offset + boffset);
                        n.inline_long = n.inline_int;
                        n.inline_val = new byte[1];
                        n.inline_val[0] = di.ReadByte(offset + boffset);
                        offset += 1;
                        break;
                    case Opcode.InlineVar.InlineSwitch:
                        uint switch_len = di.ReadUInt(offset + boffset);
                        n.inline_int = (int)switch_len;
                        n.inline_long = n.inline_int;
                        offset += 4;
                        n.inline_array = new List<int>();
                        for (uint switch_it = 0; switch_it < switch_len; switch_it++)
                        {
                            n.inline_array.Add(di.ReadInt(offset + boffset));
                            offset += 4;
                        }
                        break;
                }

                /* Determine the next instruction in the stream */
                switch (n.opcode.ctrl)
                {
                    case Opcode.ControlFlow.BRANCH:
                        n.il_offsets_after.Add(offset + n.inline_int);
                        break;

                    case Opcode.ControlFlow.COND_BRANCH:
                        if (n.opcode.opcode1 == Opcode.SingleOpcodes.switch_)
                        {
                            foreach (int jmp_target in n.inline_array)
                                n.il_offsets_after.Add(offset + jmp_target);
                            n.il_offsets_after.Add(offset);
                        }
                        else
                        {
                            n.il_offsets_after.Add(offset);
                            n.il_offsets_after.Add(offset + n.inline_int);
                        }
                        break;

                    case Opcode.ControlFlow.NEXT:
                    case Opcode.ControlFlow.CALL:
                    case Opcode.ControlFlow.BREAK:
                        n.il_offsets_after.Add(offset);
                        break;
                }

                n.il_offset_after = offset;
                ret.offset_map[n.il_offset] = n;
                ret.offset_order.Add(n.il_offset);

                // Store this node as the offset_before whatever it
                //  references
                foreach (var offset_after in n.il_offsets_after)
                {
                    List<int> after_list;
                    if (!offsets_before.TryGetValue(offset_after,
                        out after_list))
                    {
                        after_list = new List<int>();
                        offsets_before[offset_after] = after_list;
                    }
                    if (!after_list.Contains(n.il_offset))
                        after_list.Add(n.il_offset);
                }

                ret.cil.Add(n);
            }

            /* Now determine which instructions are branch targets
             * so we don't coalesce a block containing
             * a branch into a single instruction */
            foreach(var n in ret.cil)
            {
                if (n.opcode.ctrl == Opcode.ControlFlow.BRANCH)
                    ret.offset_map[n.il_offsets_after[0]].is_block_start = true;
                else if(n.opcode.ctrl == Opcode.ControlFlow.COND_BRANCH)
                    ret.offset_map[n.il_offsets_after[1]].is_block_start = true;
            }

            ret.starts = new List<CilNode>();

            if (ret.cil.Count > 0)
                ret.starts.Add(ret.cil[0]);

            if (ehdrs != null)
            {
                foreach (var e in ehdrs)
                {
                    ret.starts.Add(ret.offset_map[e.HandlerILOffset]);

                    if (e.EType == metadata.ExceptionHeader.ExceptionHeaderType.Filter)
                    {
                        var filter_start = ret.offset_map[e.FilterOffset];
                        ret.starts.Add(filter_start);
                        filter_start.is_filter_start = true;
                    }
                }
                
            }

            ret.ehdrs = ehdrs;

            foreach(var n in ret.cil)
            {
                foreach(var next in n.il_offsets_after)
                {
                    var next_n = ret.offset_map[next];
                    next_n.prev.Add(n);
                }
            }

            return ret;
        }
    }
}
