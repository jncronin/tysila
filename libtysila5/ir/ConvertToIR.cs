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
using System.Text;
using libtysila5.cil;
using metadata;
using libtysila5.util;

namespace libtysila5.ir
{
    public class StackItem
    {
        public TypeSpec ts;
        public int _ct = 0;

        public MethodSpec ms;

        public Spec.FullySpecSignature fss;

        public int ct { get { if (_ct == Opcode.ct_tls_int32 || _ct == Opcode.ct_tls_int64 || _ct == Opcode.ct_tls_intptr || ts == null) return _ct; return Opcode.GetCTFromType(ts); } }

        public long min_l = long.MinValue;
        public long max_l = long.MaxValue;

        public ulong min_ul = ulong.MinValue;
        public ulong max_ul = ulong.MaxValue;

        public string str_val = null;

        public target.Target.Reg reg;

        public bool has_address_taken = false;

        public override string ToString()
        {
            if (ms != null)
                return "fnptr: " + ms.MangleMethod();

            StringBuilder sb = new StringBuilder();

            if (ts != null)
            {
                sb.Append(ts.ToString());
                sb.Append(" ");
            }
            sb.Append("(");
            sb.Append(ir.Opcode.ct_names[ct]);
            sb.Append(")");
            return sb.ToString();
        }

        internal StackItem Clone()
        {
            return new StackItem
            {
                max_l = max_l,
                max_ul = max_ul,
                min_l = min_l,
                min_ul = min_ul,
                reg = reg,
                str_val = str_val,
                ts = ts,
                _ct = _ct,
                ms = ms,
                has_address_taken = has_address_taken
            };
        }
    }

    public partial class ConvertToIR
    {
        static ConvertToIR()
        {
            init_intcalls();
        }
        
        public static void DoConversion(Code c)
        {
            foreach (var n in c.starts)
            {
                if (n.il_offset == 0)
                    n.is_meth_start = true;
                else if(n.is_filter_start == false)
                    n.is_eh_start = true;
            }

            int unconverted = 0;
            do
            {
                unconverted = 0;

                foreach(var n in c.cil)
                {
                    if (n.is_meth_start)
                        DoConversion(n, c, new Stack<StackItem>(c.lv_types.Length));
                    else if(n.is_filter_start)
                    {
                        var stack = new Stack<StackItem>(c.lv_types.Length);
                        stack.Push(new StackItem { ts = c.ms.m.SystemObject });
                        DoConversion(n, c, stack);
                    }
                    else if (n.is_eh_start)
                    {
                        if (n.is_filter_start)
                            System.Diagnostics.Debugger.Break();
                        var stack = new Stack<StackItem>(c.lv_types.Length);
                        if (n.handler_starts.Count != 0)
                        {
                            if (n.handler_starts.Count > 1)
                                throw new Exception("too many catch handlers");
                            var ts = n.handler_starts[0].ClassToken;
                            if (ts != null)
                            {
                                stack.Push(new StackItem
                                {
                                    ts = ts
                                });
                            }
                        }
                        DoConversion(n, c, stack);
                    }
                    else if (n.try_starts.Count > 0)
                        DoConversion(n, c, new Stack<StackItem>(c.lv_types.Length));
                    else
                    {
                        bool any_prev_visited = false;
                        Stack<StackItem> prev_stack = null;
                        foreach (var prev in n.prev)
                        {
                            if(prev.opcode.opcode1 == cil.Opcode.SingleOpcodes.leave ||
                                prev.opcode.opcode1 == cil.Opcode.SingleOpcodes.leave_s)
                            {
                                // leave empties stack
                                any_prev_visited = true;
                                prev_stack = new Stack<StackItem>(c.lv_types.Length);
                                break;
                            }
                            else if (prev.visited)
                            {
                                any_prev_visited = true;
                                prev_stack = prev.stack_after;
                                break;
                            }
                        }
                        if (any_prev_visited)
                        {
                            if (prev_stack == null)
                                throw new Exception("no stack defined");
                            DoConversion(n, c, prev_stack);
                        }
                        else
                            unconverted++;
                    }
                }
            } while (unconverted != 0);

            // Determine if this method is a static constructor
            if(MetadataStream.CompareSignature(c.ms.m, c.ms.msig, c.ms.gtparams, c.ms.gmparams,
                c.special_meths, c.special_meths.static_Rv_P0, null, null))
            {
                var meth_name = c.ms.name_override;
                if(meth_name == null)
                {
                    meth_name = c.ms.m.GetStringEntry(MetadataStream.tid_MethodDef,
                        c.ms.mdrow, 3);
                }
                if(meth_name != null && meth_name == ".cctor")
                    c.is_cctor = true;
            }

            // Insert special code 
            if (c.static_types_referenced.Count > 0 || c.is_cctor)
            {
                foreach (var n in c.cil)
                {
                    if (n.is_meth_start)
                    {
                        // If this is not a static constructor, call others we may have referenced
                        if (c.is_cctor == false)
                        {
                            foreach (var static_type in c.static_types_referenced)
                            {
                                var cctor = static_type.m.GetMethodSpec(static_type, ".cctor", c.special_meths.static_Rv_P0, c.special_meths, false);

                                if (cctor != null)
                                {
                                    n.irnodes.Insert(1,
                                        new CilNode.IRNode { parent = n, opcode = Opcode.oc_call, imm_ms = cctor, stack_before = n.irnodes[0].stack_after, stack_after = n.irnodes[0].stack_after, ignore_for_mcoffset = true });
                                    c.t.r.MethodRequestor.Request(cctor);
                                }
                            }
                        }
                        else
                        {
                            // This is a static constructor, so we need to put some special code to
                            //  ensure it is only run once
                            n.irnodes.Insert(1,
                                new CilNode.IRNode { parent = n, opcode = Opcode.oc_cctor_runonce, imm_ts = c.ms.type, stack_before = n.irnodes[0].stack_after, stack_after = n.irnodes[0].stack_after, ignore_for_mcoffset = true });

                        }
                    }
                }
            }

            c.ir = new System.Collections.Generic.List<CilNode.IRNode>();
            foreach (var n in c.cil)
                c.ir.AddRange(n.irnodes);

        }

        private static void DoConversion(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            // TODO ensure stack integrity
            if (n.visited)
                return;

            Stack<StackItem> stack_after = null;
            StackItem si = null;
            long imm = 0;
            TypeSpec ts = null;

            if (n.is_meth_start)
                n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_enter, stack_before = stack_before, stack_after = stack_before });

            foreach(var ehdr in n.try_starts)
                ehdr_trycatch_start(n, c, stack_before, ehdr.EhdrIdx);
            foreach (var ehdr in n.handler_starts)
            {
                ehdr_trycatch_start(n, c, stack_before, ehdr.EhdrIdx, true, n.is_filter_start);
            }

            switch (n.opcode.opcode1)
            {
                case cil.Opcode.SingleOpcodes.nop:
                    stack_after = stack_before;
                    break;

                case cil.Opcode.SingleOpcodes.ldc_i4_0:
                case cil.Opcode.SingleOpcodes.ldc_i4_1:
                case cil.Opcode.SingleOpcodes.ldc_i4_2:
                case cil.Opcode.SingleOpcodes.ldc_i4_3:
                case cil.Opcode.SingleOpcodes.ldc_i4_4:
                case cil.Opcode.SingleOpcodes.ldc_i4_5:
                case cil.Opcode.SingleOpcodes.ldc_i4_6:
                case cil.Opcode.SingleOpcodes.ldc_i4_7:
                case cil.Opcode.SingleOpcodes.ldc_i4_8:
                case cil.Opcode.SingleOpcodes.ldc_i4_m1:
                case cil.Opcode.SingleOpcodes.ldc_i4_s:
                case cil.Opcode.SingleOpcodes.ldc_i4:
                    stack_after = new Stack<StackItem>(stack_before);
                    si = new StackItem();
                    si.ts = c.ms.m.GetSimpleTypeSpec(0x08);

                    stack_after.Add(si);

                    switch(n.opcode.opcode1)
                    {
                        case cil.Opcode.SingleOpcodes.ldc_i4:
                        case cil.Opcode.SingleOpcodes.ldc_i4_s:
                            imm = n.inline_long;
                            break;
                        case cil.Opcode.SingleOpcodes.ldc_i4_m1:
                            imm = -1;
                            break;
                        default:
                            imm = n.opcode.opcode1 - cil.Opcode.SingleOpcodes.ldc_i4_0;
                            break;
                    }

                    n.irnodes.Add(new CilNode.IRNode { parent = n, imm_l = imm, imm_val = n.inline_val, opcode = Opcode.oc_ldc, ctret = Opcode.ct_int32, vt_size = 4, stack_after = stack_after, stack_before = stack_before });
                    break;

                case cil.Opcode.SingleOpcodes.ldc_i8:
                    stack_after = ldc(n, c, stack_before, n.inline_long, n.inline_val, 0x0a);
                    break;

                case cil.Opcode.SingleOpcodes.ldc_r4:
                    stack_after = ldc(n, c, stack_before, n.inline_long, n.inline_val, 0x0c);
                    break;

                case cil.Opcode.SingleOpcodes.ldc_r8:
                    stack_after = ldc(n, c, stack_before, n.inline_long, n.inline_val, 0x0d);
                    break;

                case cil.Opcode.SingleOpcodes.ldnull:
                    stack_after = new Stack<StackItem>(stack_before);
                    si = new StackItem();
                    si.ts = c.ms.m.GetSimpleTypeSpec(0x1c);

                    stack_after.Add(si);
                    imm = 0;
                    n.irnodes.Add(new CilNode.IRNode { parent = n, imm_l = imm, opcode = Opcode.oc_ldc, ctret = Opcode.ct_object, vt_size = c.t.GetPointerSize(), stack_after = stack_after, stack_before = stack_before });
                    break;

                case cil.Opcode.SingleOpcodes.stloc_0:
                    stack_after = stloc(n, c, stack_before, 0);
                    break;
                case cil.Opcode.SingleOpcodes.stloc_1:
                    stack_after = stloc(n, c, stack_before, 1);
                    break;
                case cil.Opcode.SingleOpcodes.stloc_2:
                    stack_after = stloc(n, c, stack_before, 2);
                    break;
                case cil.Opcode.SingleOpcodes.stloc_3:
                    stack_after = stloc(n, c, stack_before, 3);
                    break;
                case cil.Opcode.SingleOpcodes.stloc_s:
                    stack_after = stloc(n, c, stack_before, (int)n.inline_uint);
                    break;

                case cil.Opcode.SingleOpcodes.ldloc_0:
                    stack_after = ldloc(n, c, stack_before, 0);
                    break;
                case cil.Opcode.SingleOpcodes.ldloc_1:
                    stack_after = ldloc(n, c, stack_before, 1);
                    break;
                case cil.Opcode.SingleOpcodes.ldloc_2:
                    stack_after = ldloc(n, c, stack_before, 2);
                    break;
                case cil.Opcode.SingleOpcodes.ldloc_3:
                    stack_after = ldloc(n, c, stack_before, 3);
                    break;
                case cil.Opcode.SingleOpcodes.ldloc_s:
                    stack_after = ldloc(n, c, stack_before, (int)n.inline_uint);
                    break;

                case cil.Opcode.SingleOpcodes.ldarg_s:
                    stack_after = ldarg(n, c, stack_before, n.inline_int);
                    break;
                case cil.Opcode.SingleOpcodes.ldarg_0:
                    stack_after = ldarg(n, c, stack_before, 0);
                    break;
                case cil.Opcode.SingleOpcodes.ldarg_1:
                    stack_after = ldarg(n, c, stack_before, 1);
                    break;
                case cil.Opcode.SingleOpcodes.ldarg_2:
                    stack_after = ldarg(n, c, stack_before, 2);
                    break;
                case cil.Opcode.SingleOpcodes.ldarg_3:
                    stack_after = ldarg(n, c, stack_before, 3);
                    break;

                case cil.Opcode.SingleOpcodes.ldarga_s:
                    stack_after = ldarga(n, c, stack_before, n.inline_int);
                    break;

                case cil.Opcode.SingleOpcodes.starg_s:
                    stack_after = starg(n, c, stack_before, n.inline_int);
                    break;

                case cil.Opcode.SingleOpcodes.ldloca_s:
                    stack_after = ldloca(n, c, stack_before, n.inline_int);
                    break;

                case cil.Opcode.SingleOpcodes.ldsfld:
                    stack_after = ldflda(n, c, stack_before, true, out ts);
                    stack_after = ldind(n, c, stack_after, ts);
                    break;

                case cil.Opcode.SingleOpcodes.ldfld:
                    stack_after = ldvtaddr(n, c, stack_before);
                    stack_after = ldflda(n, c, stack_after, false, out ts);
                    stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);
                    stack_after = ldind(n, c, stack_after, ts);
                    break;

                case cil.Opcode.SingleOpcodes.ldsflda:
                    stack_after = ldflda(n, c, stack_before, true, out ts);
                    break;

                case cil.Opcode.SingleOpcodes.ldflda:
                    stack_after = ldvtaddr(n, c, stack_before);
                    stack_after = ldflda(n, c, stack_after, false, out ts);
                    stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);
                    stack_after.Peek().ts = ts.ManagedPointer;
                    break;

                case cil.Opcode.SingleOpcodes.stfld:
                    //stack_after = copy_to_front(n, c, stack_before, 1);
                    stack_after = ldvtaddr(n, c, stack_before, 1, 1);
                    stack_after = ldflda(n, c, stack_after, false, out ts, 1);
                    stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr, 2);
                    stack_after = stind(n, c, stack_after, c.t.GetSize(ts), 1, 0);
                    stack_after.Pop();
                    break;

                case cil.Opcode.SingleOpcodes.stsfld:
                    stack_after = ldflda(n, c, stack_before, true, out ts);
                    stack_after = stind(n, c, stack_after, c.t.GetSize(ts), 1, 0);
                    break;

                case cil.Opcode.SingleOpcodes.ldelem:
                    stack_after = ldelem(n, c, stack_before, n.GetTokenAsTypeSpec(c));
                    break;
                case cil.Opcode.SingleOpcodes.ldelem_i:
                    stack_after = ldelem(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x18));
                    break;
                case cil.Opcode.SingleOpcodes.ldelem_i1:
                    stack_after = ldelem(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x04));
                    break;
                case cil.Opcode.SingleOpcodes.ldelem_i2:
                    stack_after = ldelem(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x06));
                    break;
                case cil.Opcode.SingleOpcodes.ldelem_i4:
                    stack_after = ldelem(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x08));
                    break;
                case cil.Opcode.SingleOpcodes.ldelem_i8:
                    stack_after = ldelem(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x0a));
                    break;
                case cil.Opcode.SingleOpcodes.ldelem_r4:
                    stack_after = ldelem(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x0c));
                    break;
                case cil.Opcode.SingleOpcodes.ldelem_r8:
                    stack_after = ldelem(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x0d));
                    break;
                case cil.Opcode.SingleOpcodes.ldelem_ref:
                    stack_after = ldelem(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x1c));
                    break;
                case cil.Opcode.SingleOpcodes.ldelem_u1:
                    stack_after = ldelem(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x05));
                    break;
                case cil.Opcode.SingleOpcodes.ldelem_u2:
                    stack_after = ldelem(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x07));
                    break;
                case cil.Opcode.SingleOpcodes.ldelem_u4:
                    stack_after = ldelem(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x09));
                    break;

                case cil.Opcode.SingleOpcodes.ldelema:
                    stack_after = ldelema(n, c, stack_before, n.GetTokenAsTypeSpec(c));
                    break;

                case cil.Opcode.SingleOpcodes.stelem:
                    stack_after = stelem(n, c, stack_before, n.GetTokenAsTypeSpec(c));
                    break;
                case cil.Opcode.SingleOpcodes.stelem_i:
                    stack_after = stelem(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x18));
                    break;
                case cil.Opcode.SingleOpcodes.stelem_i1:
                    stack_after = stelem(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x04));
                    break;
                case cil.Opcode.SingleOpcodes.stelem_i2:
                    stack_after = stelem(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x06));
                    break;
                case cil.Opcode.SingleOpcodes.stelem_i4:
                    stack_after = stelem(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x08));
                    break;
                case cil.Opcode.SingleOpcodes.stelem_i8:
                    stack_after = stelem(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x0a));
                    break;
                case cil.Opcode.SingleOpcodes.stelem_r4:
                    stack_after = stelem(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x0c));
                    break;
                case cil.Opcode.SingleOpcodes.stelem_r8:
                    stack_after = stelem(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x0d));
                    break;
                case cil.Opcode.SingleOpcodes.stelem_ref:
                    stack_after = stelem(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x1c));
                    break;


                case cil.Opcode.SingleOpcodes.br:
                case cil.Opcode.SingleOpcodes.br_s:
                    stack_after = br(n, c, stack_before);
                    break;

                case cil.Opcode.SingleOpcodes.brtrue:
                case cil.Opcode.SingleOpcodes.brtrue_s:
                case cil.Opcode.SingleOpcodes.brfalse:
                case cil.Opcode.SingleOpcodes.brfalse_s:
                    // first push zero
                    stack_after = new Stack<StackItem>(stack_before);
                    var brtf_ct = get_brtf_type(n.opcode, stack_before.Peek().ct);
                    stack_after.Push(new StackItem { ts = ir.Opcode.GetTypeFromCT(brtf_ct, c.ms.m), min_l = 0, max_l = 0, min_ul = 0, max_ul = 0 });
                    n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_ldc, ctret = brtf_ct, imm_l = 0, imm_ul = 0, stack_before = stack_before, stack_after = stack_after });

                    switch(n.opcode.opcode1)
                    {
                        case cil.Opcode.SingleOpcodes.brtrue:
                        case cil.Opcode.SingleOpcodes.brtrue_s:
                            stack_after = brif(n, c, stack_after, Opcode.cc_ne);
                            break;
                        case cil.Opcode.SingleOpcodes.brfalse:
                        case cil.Opcode.SingleOpcodes.brfalse_s:
                            stack_after = brif(n, c, stack_after, Opcode.cc_eq);
                            break;
                    }
                    break;

                case cil.Opcode.SingleOpcodes.beq:
                case cil.Opcode.SingleOpcodes.beq_s:
                    stack_after = brif(n, c, stack_before, Opcode.cc_eq);
                    break;

                case cil.Opcode.SingleOpcodes.bge:
                case cil.Opcode.SingleOpcodes.bge_s:
                    stack_after = brif(n, c, stack_before, Opcode.cc_ge);
                    break;

                case cil.Opcode.SingleOpcodes.bge_un:
                case cil.Opcode.SingleOpcodes.bge_un_s:
                    stack_after = brif(n, c, stack_before, Opcode.cc_ae);
                    break;

                case cil.Opcode.SingleOpcodes.bgt:
                case cil.Opcode.SingleOpcodes.bgt_s:
                    stack_after = brif(n, c, stack_before, Opcode.cc_gt);
                    break;

                case cil.Opcode.SingleOpcodes.bgt_un:
                case cil.Opcode.SingleOpcodes.bgt_un_s:
                    stack_after = brif(n, c, stack_before, Opcode.cc_a);
                    break;

                case cil.Opcode.SingleOpcodes.ble:
                case cil.Opcode.SingleOpcodes.ble_s:
                    stack_after = brif(n, c, stack_before, Opcode.cc_le);
                    break;

                case cil.Opcode.SingleOpcodes.ble_un:
                case cil.Opcode.SingleOpcodes.ble_un_s:
                    stack_after = brif(n, c, stack_before, Opcode.cc_be);
                    break;

                case cil.Opcode.SingleOpcodes.blt:
                case cil.Opcode.SingleOpcodes.blt_s:
                    stack_after = brif(n, c, stack_before, Opcode.cc_lt);
                    break;

                case cil.Opcode.SingleOpcodes.blt_un:
                case cil.Opcode.SingleOpcodes.blt_un_s:
                    stack_after = brif(n, c, stack_before, Opcode.cc_b);
                    break;

                case cil.Opcode.SingleOpcodes.bne_un:
                case cil.Opcode.SingleOpcodes.bne_un_s:
                    stack_after = brif(n, c, stack_before, Opcode.cc_ne);
                    break;

                case cil.Opcode.SingleOpcodes.ldstr:
                    stack_after = ldstr(n, c, stack_before);
                    break;

                case cil.Opcode.SingleOpcodes.call:
                    stack_after = call(n, c, stack_before);
                    break;

                case cil.Opcode.SingleOpcodes.calli:
                    stack_after = call(n, c, stack_before, true);
                    break;

                case cil.Opcode.SingleOpcodes.callvirt:
                    {
                        var call_ms = c.ms.m.GetMethodSpec(n.inline_uint, c.ms.gtparams, c.ms.gmparams);
                        var call_ms_flags = call_ms.m.GetIntEntry(MetadataStream.tid_MethodDef,
                            call_ms.mdrow, 2);
                        uint sig_idx = call_ms.m.GetIntEntry(MetadataStream.tid_MethodDef, call_ms.mdrow,
                            4);

                        var pc = call_ms.m.GetMethodDefSigParamCountIncludeThis((int)sig_idx);

                        stack_after = stack_before;

                        if(n.constrained)
                        {
                            var cts = c.ms.m.GetTypeSpec(n.constrained_tok, c.ms.gtparams, c.ms.gmparams);

                            if (!cts.ManagedPointer.Equals(stack_before.Peek(pc - 1).ts))
                                throw new Exception("Invalid constrained prefix: " +
                                    cts.ManagedPointer.MangleType() + " vs " + 
                                    stack_before.Peek(pc - 1).ts);

                            if (cts.IsValueType == false)
                            {
                                // dereference ptr
                                stack_after = ldind(n, c, stack_before, cts, pc - 1);
                                // copy value at box entry in stack to ptr entry and remove boxed type from stack
                                stack_after = stackcopy(n, c, stack_after, 0, pc);
                                stack_after = new Stack<StackItem>(stack_after);
                                stack_after.Pop();
                            }
                            else if (!call_ms.type.Equals(cts))
                            {
                                // dereference ptr then box it

                                // create a new top of stack object that is the new boxed object
                                stack_after = newobj(n, c, stack_after, null, cts.Box);

                                // get the address of its data member
                                stack_after = copy_to_front(n, c, stack_after);
                                stack_after = ldc(n, c, stack_after, layout.Layout.GetTypeSize(cts.m.SystemObject, c.t), 0x18);
                                stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);

                                // copy from the managed pointer address to the boxed type address
                                stack_after = ldc(n, c, stack_after, c.t.GetSize(cts));
                                stack_after = memcpy(n, c, stack_after, 1, pc + 2, 0);

                                // remove count and data member ptr entries
                                stack_after = new Stack<StackItem>(stack_after);
                                stack_after.Pop();
                                stack_after.Pop();

                                // copy value at box entry in stack to ptr entry and remove boxed type from stack
                                stack_after = stackcopy(n, c, stack_after, 0, pc);
                                stack_after = new Stack<StackItem>(stack_after);
                                stack_after.Pop();
                            }
                            // else do nothing if this is a value type which implements call_ms
                        }                       

                        if((call_ms_flags & 0x40) == 0x40)
                        {
                            // Calling a virtual function
                            stack_after = get_virt_ftn_ptr(n, c, stack_after, pc - 1);
                            stack_after = call(n, c, stack_after, true);
                        }
                        else
                        {
                            // Calling an instance function
                            stack_after = call(n, c, stack_after);
                        }
                    }                    
                    break;

                case cil.Opcode.SingleOpcodes.ret:
                    stack_after = ret(n, c, stack_before);
                    break;

                case cil.Opcode.SingleOpcodes.add:
                case cil.Opcode.SingleOpcodes.add_ovf:
                case cil.Opcode.SingleOpcodes.add_ovf_un:
                case cil.Opcode.SingleOpcodes.sub:
                case cil.Opcode.SingleOpcodes.sub_ovf:
                case cil.Opcode.SingleOpcodes.sub_ovf_un:
                case cil.Opcode.SingleOpcodes.mul:
                case cil.Opcode.SingleOpcodes.mul_ovf:
                case cil.Opcode.SingleOpcodes.mul_ovf_un:
                case cil.Opcode.SingleOpcodes.div:
                case cil.Opcode.SingleOpcodes.div_un:
                case cil.Opcode.SingleOpcodes.rem:
                case cil.Opcode.SingleOpcodes.rem_un:
                case cil.Opcode.SingleOpcodes.and:
                case cil.Opcode.SingleOpcodes.or:
                case cil.Opcode.SingleOpcodes.xor:
                    stack_after = binnumop(n, c, stack_before, n.opcode.opcode1);
                    break;

                case cil.Opcode.SingleOpcodes.neg:
                case cil.Opcode.SingleOpcodes.not:
                    stack_after = unnumop(n, c, stack_before, n.opcode.opcode1);
                    break;

                case cil.Opcode.SingleOpcodes.shl:
                case cil.Opcode.SingleOpcodes.shr:
                case cil.Opcode.SingleOpcodes.shr_un:
                    stack_after = shiftop(n, c, stack_before, n.opcode.opcode1);
                    break;

                case cil.Opcode.SingleOpcodes.conv_i:
                    stack_after = conv(n, c, stack_before, 0x18);
                    break;
                case cil.Opcode.SingleOpcodes.conv_i1:
                    stack_after = conv(n, c, stack_before, 0x04);
                    break;
                case cil.Opcode.SingleOpcodes.conv_i2:
                    stack_after = conv(n, c, stack_before, 0x06);
                    break;
                case cil.Opcode.SingleOpcodes.conv_i4:
                    stack_after = conv(n, c, stack_before, 0x08);
                    break;
                case cil.Opcode.SingleOpcodes.conv_i8:
                    stack_after = conv(n, c, stack_before, 0x0a);
                    break;
                case cil.Opcode.SingleOpcodes.conv_ovf_i:
                    stack_after = conv(n, c, stack_before, 0x18, true);
                    break;
                case cil.Opcode.SingleOpcodes.conv_ovf_i1:
                    stack_after = conv(n, c, stack_before, 0x04, true);
                    break;
                case cil.Opcode.SingleOpcodes.conv_ovf_i1_un:
                    stack_after = conv(n, c, stack_before, 0x04, true, true);
                    break;
                case cil.Opcode.SingleOpcodes.conv_ovf_i2:
                    stack_after = conv(n, c, stack_before, 0x06, true);
                    break;
                case cil.Opcode.SingleOpcodes.conv_ovf_i2_un:
                    stack_after = conv(n, c, stack_before, 0x06, true, true);
                    break;
                case cil.Opcode.SingleOpcodes.conv_ovf_i4:
                    stack_after = conv(n, c, stack_before, 0x08, true);
                    break;
                case cil.Opcode.SingleOpcodes.conv_ovf_i4_un:
                    stack_after = conv(n, c, stack_before, 0x08, true, true);
                    break;
                case cil.Opcode.SingleOpcodes.conv_ovf_i8:
                    stack_after = conv(n, c, stack_before, 0x0a, true);
                    break;
                case cil.Opcode.SingleOpcodes.conv_ovf_i8_un:
                    stack_after = conv(n, c, stack_before, 0x0a, true, true);
                    break;
                case cil.Opcode.SingleOpcodes.conv_ovf_i_un:
                    stack_after = conv(n, c, stack_before, 0x18, true, true);
                    break;
                case cil.Opcode.SingleOpcodes.conv_ovf_u:
                    stack_after = conv(n, c, stack_before, 0x19, true);
                    break;
                case cil.Opcode.SingleOpcodes.conv_ovf_u1:
                    stack_after = conv(n, c, stack_before, 0x05, true);
                    break;
                case cil.Opcode.SingleOpcodes.conv_ovf_u1_un:
                    stack_after = conv(n, c, stack_before, 0x05, true, true);
                    break;
                case cil.Opcode.SingleOpcodes.conv_ovf_u2:
                    stack_after = conv(n, c, stack_before, 0x07, true);
                    break;
                case cil.Opcode.SingleOpcodes.conv_ovf_u2_un:
                    stack_after = conv(n, c, stack_before, 0x07, true, true);
                    break;
                case cil.Opcode.SingleOpcodes.conv_ovf_u4:
                    stack_after = conv(n, c, stack_before, 0x09, true);
                    break;
                case cil.Opcode.SingleOpcodes.conv_ovf_u4_un:
                    stack_after = conv(n, c, stack_before, 0x09, true, true);
                    break;
                case cil.Opcode.SingleOpcodes.conv_ovf_u8:
                    stack_after = conv(n, c, stack_before, 0x0b, true);
                    break;
                case cil.Opcode.SingleOpcodes.conv_ovf_u8_un:
                    stack_after = conv(n, c, stack_before, 0x0b, true, true);
                    break;
                case cil.Opcode.SingleOpcodes.conv_ovf_u_un:
                    stack_after = conv(n, c, stack_before, 0x19, true, true);
                    break;
                case cil.Opcode.SingleOpcodes.conv_r4:
                    stack_after = conv(n, c, stack_before, 0x0c);
                    break;
                case cil.Opcode.SingleOpcodes.conv_r8:
                    stack_after = conv(n, c, stack_before, 0x0d);
                    break;
                case cil.Opcode.SingleOpcodes.conv_r_un:
                    stack_after = conv(n, c, stack_before, 0x0d, false, true);
                    break;
                case cil.Opcode.SingleOpcodes.conv_u:
                    stack_after = conv(n, c, stack_before, 0x19);
                    break;
                case cil.Opcode.SingleOpcodes.conv_u1:
                    stack_after = conv(n, c, stack_before, 0x05);
                    break;
                case cil.Opcode.SingleOpcodes.conv_u2:
                    stack_after = conv(n, c, stack_before, 0x07);
                    break;
                case cil.Opcode.SingleOpcodes.conv_u4:
                    stack_after = conv(n, c, stack_before, 0x09);
                    break;
                case cil.Opcode.SingleOpcodes.conv_u8:
                    stack_after = conv(n, c, stack_before, 0x0b);
                    break;

                case cil.Opcode.SingleOpcodes.stind_i:
                case cil.Opcode.SingleOpcodes.stind_ref:
                    stack_after = stind(n, c, stack_before, c.t.GetPointerSize());
                    break;
                case cil.Opcode.SingleOpcodes.stind_i1:
                    stack_after = stind(n, c, stack_before, 1);
                    break;
                case cil.Opcode.SingleOpcodes.stind_i2:
                    stack_after = stind(n, c, stack_before, 2);
                    break;
                case cil.Opcode.SingleOpcodes.stind_i4:
                case cil.Opcode.SingleOpcodes.stind_r4:
                    stack_after = stind(n, c, stack_before, 4);
                    break;
                case cil.Opcode.SingleOpcodes.stind_i8:
                case cil.Opcode.SingleOpcodes.stind_r8:
                    stack_after = stind(n, c, stack_before, 8);
                    break;

                case cil.Opcode.SingleOpcodes.stobj:
                    stack_after = stind(n, c, stack_before, c.t.GetSize(n.GetTokenAsTypeSpec(c)));
                    break;

                case cil.Opcode.SingleOpcodes.ldind_i:
                    stack_after = ldind(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x18));
                    break;
                case cil.Opcode.SingleOpcodes.ldind_ref:
                    stack_after = ldind(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x1c));
                    break;
                case cil.Opcode.SingleOpcodes.ldind_i1:
                    stack_after = ldind(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x04));
                    break;
                case cil.Opcode.SingleOpcodes.ldind_i2:
                    stack_after = ldind(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x06));
                    break;
                case cil.Opcode.SingleOpcodes.ldind_i4:
                    stack_after = ldind(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x08));
                    break;
                case cil.Opcode.SingleOpcodes.ldind_i8:
                    stack_after = ldind(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x0a));
                    break;
                case cil.Opcode.SingleOpcodes.ldind_r4:
                    stack_after = ldind(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x0c));
                    break;
                case cil.Opcode.SingleOpcodes.ldind_r8:
                    stack_after = ldind(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x0d));
                    break;
                case cil.Opcode.SingleOpcodes.ldind_u1:
                    stack_after = ldind(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x05));
                    break;
                case cil.Opcode.SingleOpcodes.ldind_u2:
                    stack_after = ldind(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x07));
                    break;
                case cil.Opcode.SingleOpcodes.ldind_u4:
                    stack_after = ldind(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x09));
                    break;

                case cil.Opcode.SingleOpcodes.ldobj:
                    stack_after = ldobj(n, c, stack_before, n.GetTokenAsTypeSpec(c));
                    break;

                case cil.Opcode.SingleOpcodes.isinst:
                    stack_after = castclass(n, c, stack_before, true);
                    break;

                case cil.Opcode.SingleOpcodes.castclass:
                    stack_after = castclass(n, c, stack_before, false);
                    break;

                case cil.Opcode.SingleOpcodes.pop:
                    stack_after = pop(n, c, stack_before);
                    break;

                case cil.Opcode.SingleOpcodes.newarr:
                    stack_after = newarr(n, c, stack_before);
                    break;

                case cil.Opcode.SingleOpcodes.newobj:
                    stack_after = newobj(n, c, stack_before);
                    break;

                case cil.Opcode.SingleOpcodes.throw_:
                    stack_after = throw_(n, c, stack_before);
                    break;

                case cil.Opcode.SingleOpcodes.switch_:
                    stack_after = switch_(n, c, stack_before);
                    break;

                case cil.Opcode.SingleOpcodes.unbox_any:
                    stack_after = unbox_any(n, c, stack_before);
                    break;

                case cil.Opcode.SingleOpcodes.unbox:
                    stack_after = unbox(n, c, stack_before);
                    break;

                case cil.Opcode.SingleOpcodes.box:
                    stack_after = box(n, c, stack_before);
                    break;

                case cil.Opcode.SingleOpcodes.ldlen:
                    stack_after = ldlen(n, c, stack_before);
                    break;

                case cil.Opcode.SingleOpcodes.dup:
                    stack_after = copy_to_front(n, c, stack_before);
                    break;

                case cil.Opcode.SingleOpcodes.ldtoken:
                    stack_after = ldtoken(n, c, stack_before);
                    break;

                case cil.Opcode.SingleOpcodes.leave:
                case cil.Opcode.SingleOpcodes.leave_s:
                    stack_after = leave(n, c, stack_before);
                    break;

                case cil.Opcode.SingleOpcodes.endfinally:
                    stack_after = endfinally(n, c, stack_before);
                    break;

                case cil.Opcode.SingleOpcodes.mkrefany:
                    stack_after = mkrefany(n, c, stack_before);
                    break;

                case cil.Opcode.SingleOpcodes.double_:
                    switch(n.opcode.opcode2)
                    {
                        case cil.Opcode.DoubleOpcodes.ceq:
                            stack_after = cmp(n, c, stack_before, Opcode.cc_eq);
                            break;
                        case cil.Opcode.DoubleOpcodes.cgt:
                            stack_after = cmp(n, c, stack_before, Opcode.cc_gt);
                            break;
                        case cil.Opcode.DoubleOpcodes.cgt_un:
                            stack_after = cmp(n, c, stack_before, Opcode.cc_a);
                            break;
                        case cil.Opcode.DoubleOpcodes.clt:
                            stack_after = cmp(n, c, stack_before, Opcode.cc_lt);
                            break;
                        case cil.Opcode.DoubleOpcodes.clt_un:
                            stack_after = cmp(n, c, stack_before, Opcode.cc_b);
                            break;
                        case cil.Opcode.DoubleOpcodes.localloc:
                            {
                                stack_after = new Stack<StackItem>(stack_before);
                                stack_after.Pop();

                                si = new StackItem { ts = c.ms.m.GetSimpleTypeSpec(0x18) };
                                stack_after.Push(si);

                                // don't allow localloc in exception handlers
                                if (n.is_in_excpt_handler)
                                    throw new NotSupportedException("localloc in exception handler");

                                n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = ir.Opcode.oc_localloc, stack_before = stack_before, stack_after = stack_after });

                                // TODO: if localsinit set then initialize to zero

                                break;
                            }
                        case cil.Opcode.DoubleOpcodes._sizeof:
                            stack_after = ldc(n, c, stack_before, c.t.GetSize(n.GetTokenAsTypeSpec(c)));
                            break;

                        case cil.Opcode.DoubleOpcodes.ldftn:
                            stack_after = ldftn(n, c, stack_before);
                            break;

                        case cil.Opcode.DoubleOpcodes.initobj:
                            stack_after = initobj(n, c, stack_before);
                            break;

                        case cil.Opcode.DoubleOpcodes.rethrow:
                            stack_after = rethrow(n, c, stack_before);
                            break;

                        case cil.Opcode.DoubleOpcodes.arglist:
                            stack_after = arglist(n, c, stack_before);
                            break;

                        case cil.Opcode.DoubleOpcodes.ldvirtftn:
                            stack_after = get_virt_ftn_ptr(n, c, stack_before);
                            break;

                        case cil.Opcode.DoubleOpcodes.endfilter:
                            stack_after = endfilter(n, c, stack_before);
                            break;

                        case cil.Opcode.DoubleOpcodes.refanytype:
                            stack_after = refanytype(n, c, stack_before);
                            break;

                        default:
                            throw new NotImplementedException(n.ToString());
                    }
                    break;

                default:
                    throw new NotImplementedException(n.ToString());
            }

            n.visited = true;
            n.stack_after = stack_after;

            //foreach (var after in n.il_offsets_after)
            //    DoConversion(c.offset_map[after], c, stack_after);
        }

        private static Stack<StackItem> refanytype(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            // extract the 'Type' member as a RuntimeTypeHandle
            var typed_ref = c.ms.m.al.GetAssembly("mscorlib").GetTypeSpec("System", "TypedReference");

            var stack_after = ldc(n, c, stack_before, layout.Layout.GetFieldOffset(typed_ref, "Type", c.t, out var is_tls), 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);
            stack_after = ldind(n, c, stack_after, c.ms.m.SystemRuntimeTypeHandle);

            return stack_after;
        }

        private static Stack<StackItem> mkrefany(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            var ts = n.GetTokenAsTypeSpec(c);

            Stack<StackItem> stack_after = new Stack<StackItem>(stack_before);

            c.t.r.VTableRequestor.Request(ts.Box);

            // Ensure the stack type is a managed pointer to push_ts
            var stack_type = stack_after.Peek().ts;
            if (!stack_type.Equals(ts.ManagedPointer))
                throw new Exception("mkrefany ptr is not managed pointer to " + ts.MangleType());

            // Build the new System.TypedReference object on the stack
            var typed_ref = ts.m.al.GetAssembly("mscorlib").GetTypeSpec("System", "TypedReference");
            stack_after.Push(new StackItem { ts = typed_ref });

            // Save the 'Type' member
            stack_after = copy_to_front(n, c, stack_after);
            stack_after = ldc(n, c, stack_after, layout.Layout.GetFieldOffset(typed_ref, "Type", c.t, out var is_tls), 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);
            stack_after = ldlab(n, c, stack_after, ts.MangleType());
            stack_after = call(n, c, stack_after, false, "__type_from_vtbl", c.special_meths, c.special_meths.type_from_vtbl);
            stack_after = stind(n, c, stack_after, c.t.GetPointerSize());

            // Save the 'Value' member
            stack_after = copy_to_front(n, c, stack_after);
            stack_after = ldc(n, c, stack_after, layout.Layout.GetFieldOffset(typed_ref, "Value", c.t, out is_tls), 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);
            stack_after = copy_to_front(n, c, stack_after, 2);
            stack_after = stind(n, c, stack_after, c.t.psize);

            // Rearrange stack ..., ptr, typedRef.  -> ..., typedRef
            var stack_after2 = new Stack<StackItem>(stack_after);
            stack_after2.Pop();
            stack_after2.Pop();
            stack_after2.Push(new StackItem { ts = typed_ref });
            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_stackcopy, arg_a = 0, res_a = 0, stack_before = stack_after, stack_after = stack_after2 });

            return stack_after2;
        }

        private static Stack<StackItem> memcpy(CilNode n, Code c, Stack<StackItem> stack_before, int dest = 2, int src = 1, int count = 0)
        {
            var stack_after = new Stack<StackItem>(stack_before);

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_memcpy, stack_before = stack_before, stack_after = stack_after, arg_a = dest, arg_b = src, arg_c = count });

            return stack_after;
        }

        private static Stack<StackItem> memset(CilNode n, Code c, Stack<StackItem> stack_before, int dest = 1, int length = 0)
        {
            var stack_after = new Stack<StackItem>(stack_before);

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_memset, stack_before = stack_before, stack_after = stack_after, arg_a = dest, arg_b = length });

            return stack_after;
        }

        private static Stack<StackItem> pop(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            var stack_after = new Stack<StackItem>(stack_before);
            stack_after.Pop();
            return stack_after;
        }

        /* Gets the address of a stack entry that is a value type.
         * Used by ldfld, ldflda, stfld to calculate the address of
         * a field in a value type.
         * If the stack object is not a value type it does nothing. */
        private static Stack<StackItem> ldvtaddr(CilNode n, Code c, Stack<StackItem> stack_before, int arg_a = -1, int res_a = -1)
        {
            var act_arg_a = arg_a;
            if (arg_a == -1)
                arg_a = 0;
            var si = stack_before.Peek(arg_a);

            if (si.ct != ir.Opcode.ct_vt)
                return stack_before;

            var stack_after = new Stack<StackItem>(stack_before);

            si.has_address_taken = true;
            var si_r = new StackItem { ts = si.ts.ManagedPointer };
            if (act_arg_a == -1)
                stack_after.Pop();
            if (res_a == -1)
            {
                stack_after.Push(si_r);
                res_a = 0;
            }
            else
                stack_after[stack_after.Count - 1 - res_a] = si_r;

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = ir.Opcode.oc_ldobja, arg_a = arg_a, res_a = res_a, stack_before = stack_before, stack_after = stack_after });

            return stack_after;
        }

        private static Stack<StackItem> arglist(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            // arglist is currently not implemented - throw a runtime exception
            var stack_after = ldstr(n, c, stack_before, "arglist is not supported");

            var ni_ts = c.ms.m.al.GetAssembly("mscorlib").GetTypeSpec("System", "NotImplementedException");
            var ni_ctor_row = ni_ts.m.GetMethodDefRow(ni_ts, ".ctor",
                c.special_meths.inst_Rv_s, c.special_meths);
            var ni_ctor_ms = new MethodSpec
            {
                m = ni_ts.m,
                type = ni_ts,
                mdrow = ni_ctor_row,
                msig = (int)ni_ts.m.GetIntEntry(MetadataStream.tid_MethodDef, ni_ctor_row, 4),
            };

            stack_after = newobj(n, c, stack_after, ni_ctor_ms);
            stack_after = throw_(n, c, stack_after);

            // push a System.RuntimeArgumentHandle anyway as further instructions will expect it
            var srah_ts = c.ms.m.al.GetAssembly("mscorlib").GetTypeSpec("System", "RuntimeArgumentHandle");
            var stack_after2 = new Stack<StackItem>(stack_after);
            stack_after2.Push(new StackItem { ts = srah_ts });

            return stack_after2;
        }

        private static Stack<StackItem> rethrow(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            bool in_catch = false;
            foreach(var ehdr in c.ehdrs)
            {
                if(ehdr.EType == ExceptionHeader.ExceptionHeaderType.Catch)
                {
                    if((n.il_offset >= ehdr.HandlerILOffset) &&
                        (n.il_offset <= (ehdr.HandlerILOffset + ehdr.HandlerLength)))
                    {
                        in_catch = true;
                        break;
                    }
                }
            }
            if (!in_catch)
                throw new Exception("rethrow called not in catch handler");

            var stack_after = call(n, c, stack_before, false, "rethrow",
                c.special_meths, c.special_meths.rethrow);

            return stack_after;
        }

        private static Stack<StackItem> initobj(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            var t = n.GetTokenAsTypeSpec(c);

            var ts = stack_before.Peek().ts;
            if (ts.stype == TypeSpec.SpecialType.MPtr || ts.stype == TypeSpec.SpecialType.Ptr)
            {
                if (!t.IsAssignmentCompatibleWith(ts.other))
                    throw new Exception("initobj verification failed");
            }
            else if(!ts.Equals(c.ms.m.SystemIntPtr))
                throw new Exception("initobj stack value is not managed pointer");
            
            if(t.IsValueType)
            {
                var tsize = layout.Layout.GetTypeSize(t, c.t);

                var stack_after = ldc(n, c, stack_before, tsize);

                var stack_after2 = new Stack<StackItem>(stack_after);
                stack_after2.Pop();
                stack_after2.Pop();
                n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_zeromem, imm_l = layout.Layout.GetTypeSize(t, c.t), stack_before = stack_after, stack_after = stack_after2 });
                return stack_after2;
            }
            else
            {
                var stack_after = ldc(n, c, stack_before, 0, 0x1c);
                stack_after = stind(n, c, stack_after, c.t.GetPointerSize());
                return stack_after;
            }
        }

        private static Stack<StackItem> ldftn(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            var ms = c.ms.m.GetMethodSpec(n.inline_uint, c.ms.gtparams, c.ms.gmparams);
            var m = ms.m;
            var mangled_meth = m.MangleMethod(ms);

            c.t.r.MethodRequestor.Request(ms);

            var stack_after = ldlab(n, c, stack_before, mangled_meth);
            return stack_after;
        }

        private static Stack<StackItem> ehdr_trycatch_start(CilNode n, Code c, Stack<StackItem> stack_before, int ehdrIdx,
            bool is_catch = false, bool is_filter = false)
        {

            if (is_catch)
            {
                n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_enter_handler, imm_l = ehdrIdx, imm_ul = is_filter ? 1UL : 0UL, stack_after = stack_before, stack_before = stack_before });
            }

            if (is_filter)
                return new Stack<StackItem>(stack_before);

            var stack_after = ldlab(n, c, stack_before, c.ms.m.MangleMethod(c.ms) + "EH",
                ehdrIdx * layout.Layout.GetEhdrSize(c.t));
            stack_after = ldfp(n, c, stack_after);
            c.t.r.EHRequestor.Request(c);

            if(is_catch)
            {
                stack_after = call(n, c, stack_after, false, "enter_catch",
                    c.special_meths, c.special_meths.catch_enter);
            }
            else
            {
                stack_after = call(n, c, stack_after, false, "enter_try",
                    c.special_meths, c.special_meths.try_enter);
            }

            return stack_after;
        }

        private static Stack<StackItem> ldfp(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            var stack_after = new Stack<StackItem>(stack_before);

            stack_after.Push(new StackItem { ts = c.ms.m.SystemIntPtr });
            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_ldfp, stack_before = stack_before, stack_after = stack_after });

            return stack_after;
        }

        private static Stack<StackItem> endfinally(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            var stack_after = new Stack<StackItem>(stack_before);

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_ret, stack_before = stack_after, stack_after = stack_after });

            return stack_after;
        }

        private static Stack<StackItem> endfilter(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            var stack_after = new Stack<StackItem>(stack_before);

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_ret, stack_before = stack_after, stack_after = stack_after, ct = Opcode.ct_int32 });

            return stack_after;
        }

        private static Stack<StackItem> br(CilNode n, Code c, Stack<StackItem> stack_before, int il_target = int.MaxValue)
        {
            var stack_after = stack_before;

            if (il_target == int.MaxValue)
                il_target = n.il_offsets_after[0];

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_br, imm_l = il_target, stack_after = stack_after, stack_before = stack_before });
            return stack_after;
        }

        private static Stack<StackItem> leave(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            // ensure we are in a try, filter or catch block
            if(c.ehdrs != null)
            {
                c.t.r.EHRequestor.Request(c);
                foreach(var ehdr in c.ehdrs)
                {
                    if (n.il_offset >= ehdr.TryILOffset &&
                        n.il_offset < (ehdr.TryILOffset + ehdr.TryLength) &&
                        (n.il_offsets_after[0] < ehdr.TryILOffset ||
                        n.il_offsets_after[0] >= (ehdr.TryILOffset + ehdr.TryLength)))
                    {
                        // invoke the exception handler to leave the most deeply nested block but
                        //  only up until the shallowest nested block
                        var stack_after2 = new Stack<StackItem>(c.lv_types.Length);
                        stack_after2 = ldlab(n, c, stack_after2, c.ms.m.MangleMethod(c.ms) + "EH",
                            ehdr.EhdrIdx * layout.Layout.GetEhdrSize(c.t));
                        call(n, c, stack_after2, false, "leave_try",
                            c.special_meths, c.special_meths.leave);
                    }
                    else if (n.il_offset >= ehdr.HandlerILOffset &&
                        n.il_offset < (ehdr.HandlerILOffset + ehdr.HandlerLength) &&
                        (ehdr.EType == ExceptionHeader.ExceptionHeaderType.Catch ||
                        ehdr.EType == ExceptionHeader.ExceptionHeaderType.Fault ||
                        ehdr.EType == ExceptionHeader.ExceptionHeaderType.Filter))
                    {
                        // invoke the exception handler to leave the most deeply nested block
                        var stack_after2 = new Stack<StackItem>(c.lv_types.Length);
                        stack_after2 = ldlab(n, c, stack_after2, c.ms.m.MangleMethod(c.ms) + "EH",
                            ehdr.EhdrIdx * layout.Layout.GetEhdrSize(c.t));
                        call(n, c, stack_after2, false, "leave_handler",
                            c.special_meths, c.special_meths.leave);
                    }
                }
            }

            // empty execution stack
            var stack_after = new Stack<StackItem>(c.lv_types.Length);

            // jump to the targetted instruction
            return br(n, c, stack_after);
        }

        private static Stack<StackItem> ldtoken(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            var ts = n.GetTokenAsTypeSpec(c);
            var ms = n.GetTokenAsMethodSpec(c);

            TypeSpec push_ts = null;
            TypeSpec.FullySpecSignature sig_val = null;
            Stack<StackItem> stack_after;

            if (ts != null)
            {
                push_ts = ts.m.SystemRuntimeTypeHandle;
                sig_val = ts.Signature;
                c.t.r.VTableRequestor.Request(ts.Box);

                stack_after = ldlab(n, c, stack_before, ts.MangleType());
                stack_after = call(n, c, stack_after, false, "__type_from_vtbl", c.special_meths, c.special_meths.type_from_vtbl);
            }
            else if (ms != null)
            {
                // decide if method or field ref
                if (ms.is_field)
                    push_ts = ms.m.SystemRuntimeFieldHandle;
                else
                    push_ts = ms.m.SystemRuntimeMethodHandle;
                sig_val = ms.Signature;


                int sig_offset = c.t.sigt.GetSignatureAddress(sig_val, c.t);

                // build the object
                stack_after = ldlab(n, c, stack_before, c.t.sigt.GetStringTableName(), sig_offset);
            }
            else throw new Exception("Bad token");

            stack_after[stack_after.Count - 1] = new StackItem
            {
                ts = push_ts,
                fss = sig_val
            };

            return stack_after;
        }

        private static Stack<StackItem> ldobj(CilNode n, Code c, Stack<StackItem> stack_before, TypeSpec ts = null)
        {
            if (ts == null)
                ts = n.GetTokenAsTypeSpec(c);

            if(ts.IsValueType)
            {
                var esize = c.t.GetSize(ts);
                return ldind(n, c, stack_before, ts);
            }

            var ret = ldind(n, c, stack_before, c.ms.m.GetSimpleTypeSpec(0x1c));
            ret.Peek().ts = ts;
            return ret;
        }

        private static Stack<StackItem> ldobja(CilNode n, Code c, Stack<StackItem> stack_before, TypeSpec ts = null)
        {
            if (ts == null)
                ts = n.GetTokenAsTypeSpec(c);

            if (!ts.IsValueType)
            {
                throw new Exception("ldobja on reference type");
            }

            stack_before.Peek().has_address_taken = true;

            var ret = new Stack<StackItem>(stack_before);
            ret.Add(new StackItem { ts = ts.ManagedPointer });
            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_ldobja, ct = Opcode.ct_vt, ctret = Opcode.ct_ref, stack_before = stack_before, stack_after = ret });
            return ret;
        }

        private static Stack<StackItem> ldlen(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            // dereference sizes pointer
            var stack_after = ldc(n, c, stack_before, layout.Layout.GetArrayFieldOffset(layout.Layout.ArrayField.SizesPointer, c.t), 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);
            stack_after = ldind(n, c, stack_after, c.ms.m.GetSimpleTypeSpec(0x18));
            stack_after = ldind(n, c, stack_after, c.ms.m.GetSimpleTypeSpec(0x08));
            return stack_after;
        }

        private static Stack<StackItem> stelem(CilNode n, Code c, Stack<StackItem> stack_before, TypeSpec ts)
        {
            // todo: type and array bounds checks

            var tsize = c.t.GetSize(ts);

            // build offset
            var stack_after = ldc(n, c, stack_before, tsize, 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.mul, Opcode.ct_intptr, 2, -1, 1);

            // check object is a szarray to ts
            stack_after = copy_to_front(n, c, stack_after, 2);
            stack_after = castclass(n, c, stack_after, false, null,
                new TypeSpec { m = c.ms.m.al.GetAssembly("mscorlib"), stype = TypeSpec.SpecialType.SzArray, other = ts });

            // dereference data pointer
            stack_after = ldc(n, c, stack_after, layout.Layout.GetArrayFieldOffset(layout.Layout.ArrayField.DataArrayPointer, c.t), 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);
            stack_after = ldind(n, c, stack_after, c.ms.m.GetSimpleTypeSpec(0x18));

            // get pointer to actual data
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr, -1, 2, -1);

            // load data
            stack_after = stind(n, c, stack_after, tsize, 1, 0);

            stack_after = new Stack<StackItem>(stack_after);
            stack_after.Pop();
            stack_after.Pop();

            return stack_after;
        }

        private static Stack<StackItem> box(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            var ts = n.GetTokenAsTypeSpec(c);

            if (ts.IsValueType)
            {
                if(!stack_before.Peek().ts.IsVerifierAssignableTo(ts))
                    throw new Exception("Box called with invalid parameter: " + ts.ToString() +
                        " vs " + stack_before.Peek().ts);

                var boxed_ts = ts.Box;
                c.t.r.VTableRequestor.Request(boxed_ts);
                var ptr_size = c.t.GetPointerSize();
                var sysobj_size = layout.Layout.GetTypeSize(c.ms.m.SystemObject, c.t);
                var data_size = c.t.GetSize(ts);
                var boxed_size = util.util.align(sysobj_size + data_size, ptr_size);

                // create boxed object
                var stack_after = ldc(n, c, stack_before, boxed_size, 0x18);
                stack_after = call(n, c, stack_after, false, "gcmalloc", c.special_meths, c.special_meths.gcmalloc);

                // set vtbl
                stack_after = copy_to_front(n, c, stack_after);
                stack_after = ldlab(n, c, stack_after, c.ms.m.MangleType(ts));
                stack_after = stind(n, c, stack_after, ptr_size);

                /* Store mutex lock */
                stack_after = copy_to_front(n, c, stack_after);
                stack_after = ldc(n, c, stack_after, layout.Layout.GetArrayFieldOffset(layout.Layout.ArrayField.MutexLock, c.t), 0x18);
                stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add);
                stack_after = ldc(n, c, stack_after, 0, 0x18);
                stack_after = stind(n, c, stack_after, ptr_size);

                // set object
                stack_after = copy_to_front(n, c, stack_after);
                stack_after = ldc(n, c, stack_after, sysobj_size, 0x18);
                stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);
                stack_after = stind(n, c, stack_after, data_size, 2, 0);

                // get return value
                var stack_after2 = new Stack<StackItem>(stack_after);
                stack_after2.Pop();
                stack_after2.Pop();
                stack_after2.Push(new StackItem { ts = boxed_ts });
                n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = ir.Opcode.oc_stackcopy, stack_before = stack_after, stack_after = stack_after2 });

                return stack_after2;
            }
            else
            {
                c.t.r.VTableRequestor.Request(ts);
                return stack_before;
            }
        }

        private static Stack<StackItem> unbox(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            var ts = n.GetTokenAsTypeSpec(c);

            // TODO: ensure stack item is a boxed instance of ts

            // simply add the offset to the boxed instance to the original pointer
            var stack_after = ldc(n, c, stack_before, layout.Layout.GetTypeSize(c.ms.m.SystemObject, c.t), 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);

            stack_after[stack_after.Count - 1] = new StackItem { ts = ts.ManagedPointer };
            return stack_after;
        }

        private static Stack<StackItem> unbox_any(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            var ts = n.GetTokenAsTypeSpec(c);

            if (ts.IsValueType)
            {
                // TODO ensure stack item is a boxed instance of ts
                var sysobj_size = layout.Layout.GetTypeSize(c.ms.m.SystemObject, c.t);
                var stack_after = ldc(n, c, stack_before, sysobj_size, 0x18);
                stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);
                stack_after = ldind(n, c, stack_after, ts);
                return stack_after;
            }
            else
            {
                return castclass(n, c, stack_before);
            }
        }

        private static Stack<StackItem> ldelem(CilNode n, Code c, Stack<StackItem> stack_before, TypeSpec ts)
        {
            // todo: type and array bounds checks

            var tsize = c.t.GetSize(ts);

            // build offset
            var stack_after = ldc(n, c, stack_before, tsize, 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.mul, Opcode.ct_intptr);

            // check object is a szarray to ts
            stack_after = copy_to_front(n, c, stack_after, 1);
            stack_after = castclass(n, c, stack_after, false, null,
                new TypeSpec { m = c.ms.m.al.GetAssembly("mscorlib"), stype = TypeSpec.SpecialType.SzArray, other = ts });

            // dereference data pointer
            stack_after = ldc(n, c, stack_after, layout.Layout.GetArrayFieldOffset(layout.Layout.ArrayField.DataArrayPointer, c.t), 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);
            stack_after = ldind(n, c, stack_after, c.ms.m.GetSimpleTypeSpec(0x18));

            // get pointer to actual data
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);

            // load data
            stack_after = ldind(n, c, stack_after, ts);

            var stack_after2 = new Stack<StackItem>(stack_after);
            stack_after2.Pop();
            stack_after2.Pop();
            stack_after2.Push(new StackItem { ts = ts });
            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_stackcopy, arg_a = 0, res_a = 0, stack_before = stack_after, stack_after = stack_after2 });

            return stack_after2;
        }

        private static Stack<StackItem> ldelema(CilNode n, Code c, Stack<StackItem> stack_before, TypeSpec ts)
        {
            // todo: type and array bounds checks

            var tsize = c.t.GetSize(ts);

            // build offset
            var stack_after = ldc(n, c, stack_before, tsize, 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.mul, Opcode.ct_intptr);

            // check object is a szarray to ts
            stack_after = copy_to_front(n, c, stack_after, 1);
            stack_after = castclass(n, c, stack_after, false, null,
                new TypeSpec { m = c.ms.m.al.GetAssembly("mscorlib"), stype = TypeSpec.SpecialType.SzArray, other = ts });

            // dereference data pointer
            stack_after = ldc(n, c, stack_after, layout.Layout.GetArrayFieldOffset(layout.Layout.ArrayField.DataArrayPointer, c.t), 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);
            stack_after = ldind(n, c, stack_after, c.ms.m.GetSimpleTypeSpec(0x18));

            // get pointer to actual data
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);


            var stack_after2 = new Stack<StackItem>(stack_after);
            stack_after2.Pop();
            stack_after2.Pop();
            stack_after2.Push(new StackItem { ts = ts.ManagedPointer });
            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_stackcopy, arg_a = 0, res_a = 0, stack_before = stack_after, stack_after = stack_after2 });

            return stack_after2;
        }

        private static Stack<StackItem> switch_(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            // TODO: optimize for architectures that support jump tables
            int[] targets = new int[n.il_offsets_after.Count - 1];
            for (int i = 0; i < n.il_offsets_after.Count - 1; i++)
                targets[i] = n.il_offsets_after[i];

            var stack_after = new Stack<StackItem>(stack_before);
            stack_after.Pop();

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = ir.Opcode.oc_switch, arg_list = new System.Collections.Generic.List<int>(targets), stack_before = stack_before, stack_after = stack_after });

            return stack_after;
        }

        internal static Stack<StackItem> throw_(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            return call(n, c, stack_before, false, "throw", c.special_meths, c.special_meths.throw_);
        }

        private static int get_brtf_type(cil.Opcode opcode, int ct)
        {
            switch(opcode.opcode1)
            {
                case cil.Opcode.SingleOpcodes.brtrue:
                case cil.Opcode.SingleOpcodes.brtrue_s:
                    switch(ct)
                    {
                        case ir.Opcode.ct_intptr:
                        case ir.Opcode.ct_object:
                            // following added for mono csc compatibility
                        case ir.Opcode.ct_int32:
                        case ir.Opcode.ct_int64:
                        case ir.Opcode.ct_ref:
                            return ct;
                    }
                    break;
                case cil.Opcode.SingleOpcodes.brfalse:
                case cil.Opcode.SingleOpcodes.brfalse_s:
                    switch (ct)
                    {
                        case ir.Opcode.ct_int32:
                        case ir.Opcode.ct_int64:
                        case ir.Opcode.ct_object:
                        case ir.Opcode.ct_ref:
                        case ir.Opcode.ct_intptr:
                            return ct;
                    }
                    break;
            }
            throw new Exception("Invalid argument to " + opcode.ToString() + ": " + ir.Opcode.ct_names[ct]);
        }

        internal static Stack<StackItem> newobj(CilNode n, Code c, Stack<StackItem> stack_before,
            MethodSpec ctor = null, TypeSpec objtype = null)
        {
            if(ctor == null && objtype == null)
                ctor = n.GetTokenAsMethodSpec(c);
            if(objtype == null) 
                objtype = ctor.type;
            c.t.r.VTableRequestor.Request(objtype.Box);
            if(ctor != null)
                c.t.r.MethodRequestor.Request(ctor);
            var stack_after = new Stack<StackItem>(stack_before);

            int vt_adjust = 0;

            /* is this a value type? */
            if (objtype.IsValueType)
            {
                vt_adjust = 1;

                /* Create storage space on the stack for the object */
                stack_after.Push(new StackItem { ts = objtype });

                /* Load up the address of the object for passing to the constructor */
                stack_after = ldobja(n, c, stack_after, objtype);

                /* Clear the memory */
                stack_after = copy_to_front(n, c, stack_after);
                stack_after = ldc(n, c, stack_after, layout.Layout.GetTypeSize(objtype, c.t));
                stack_after = memset(n, c, stack_after);
                stack_after = pop(n, c, stack_after);
                stack_after = pop(n, c, stack_after);
            }
            else
            {
                /* Its a reference type */

                /* check if this is system.string */
                var objsize = layout.Layout.GetTypeSize(objtype, c.t);
                var vtname = objtype.MangleType();
                var intptrsize = c.t.GetPointerSize();

                /* create object */
                bool is_string = false;
                if (objtype.Equals(c.ms.m.GetSimpleTypeSpec(0x0e)))
                {
                    stack_after = newstr(n, c, stack_after, ctor);

                    // create a copy of the character length, multiply 2 and add string object length
                    //  and 4 extra bytes to ensure coreclr String.EqualsHelper works correctly
                    stack_after = copy_to_front(n, c, stack_after);
                    stack_after = ldc(n, c, stack_after, 2);
                    stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.mul, Opcode.ct_int32);
                    stack_after = ldc(n, c, stack_after, layout.Layout.GetStringFieldOffset(layout.Layout.StringField.Start_Char, c) + 4);
                    stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_int32);

                    is_string = true;
                }
                else
                    stack_after = ldc(n, c, stack_after, objsize, 0x18);

                stack_after = call(n, c, stack_after, false, "gcmalloc", c.special_meths, c.special_meths.gcmalloc);

                if(is_string)
                {
                    /* now stack is ..., ctor_args, length, obj
                     * 
                     * we need to store length to the appropriate part then
                     * adject the stack so length is no longer present
                     */
                    stack_after = copy_to_front(n, c, stack_after);
                    stack_after = ldc(n, c, stack_after, layout.Layout.GetStringFieldOffset(layout.Layout.StringField.Length, c), 0x18);
                    stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);
                    stack_after = stind(n, c, stack_after, 4, 2, 0);

                    var stack_after2 = new Stack<StackItem>(stack_after);
                    stack_after2.Pop();
                    stack_after2.Pop();
                    stack_after2.Push(new StackItem { ts = c.ms.m.SystemIntPtr });
                    n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_stackcopy, stack_before = stack_after, stack_after = stack_after2, arg_a = 0, res_a = 0 });
                    stack_after = new Stack<StackItem>(stack_after2);
                }

                /* store vtbl pointer */
                stack_after = copy_to_front(n, c, stack_after);
                stack_after = ldlab(n, c, stack_after, vtname);
                stack_after = stind(n, c, stack_after, intptrsize);

                /* Store mutex lock */
                stack_after = copy_to_front(n, c, stack_after);
                stack_after = ldc(n, c, stack_after, layout.Layout.GetArrayFieldOffset(layout.Layout.ArrayField.MutexLock, c.t), 0x18);
                stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add);
                stack_after = ldc(n, c, stack_after, 0, 0x18);
                stack_after = stind(n, c, stack_after, intptrsize);
            }

            if (ctor != null)
            {
                /* call constructor.  Arguments are 0 for object, then
                 * for the other pcount - 1 arguments, they are at
                 * pcount - 1, pcount - 2, pcount - 3 etc */
                System.Collections.Generic.List<int> p = new System.Collections.Generic.List<int>();
                var pcount = ctor.m.GetMethodDefSigParamCountIncludeThis(ctor.msig);
                p.Add(0);
                for (int i = 0; i < (pcount - 1); i++)
                    p.Add(pcount - 1 - i + vt_adjust);

                n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = ir.Opcode.oc_call, imm_ms = ctor, arg_list = p, stack_before = stack_after, stack_after = stack_after });

                /* pop all arguments and leave object on the stack */
                var stack_after2 = new Stack<StackItem>(stack_after);
                for (int i = 0; i < (pcount + vt_adjust); i++)
                    stack_after2.Pop();

                stack_after2.Push(new StackItem { ts = objtype });
                n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = ir.Opcode.oc_stackcopy, arg_a = vt_adjust, res_a = 0, stack_before = stack_after, stack_after = stack_after2 });
                stack_after = stack_after2;
            }
            else
            {
                stack_after.Peek(0).ts = objtype;
            }

            return stack_after;
        }

        private static Stack<StackItem> newstr(CilNode n, Code c, Stack<StackItem> stack_before,
            MethodSpec ctor)
        {
            // Generates an instruction stream to determine the number of bytes
            //  required for the entire string object (object size is added
            //  at end).

            // Decide on the particular constructor
            Stack<StackItem> stack_after = null;

            if (MetadataStream.CompareSignature(ctor,
                c.special_meths,
                c.special_meths.string_ci, null, null))
            {
                // string(char c, int32 count)
                stack_after = copy_to_front(n, c, stack_before);
            }
            else if (MetadataStream.CompareSignature(ctor,
                c.special_meths,
                c.special_meths.string_Zc, null, null))
            {
                // string(char[] value)
                stack_after = copy_to_front(n, c, stack_before);
                stack_after = ldc(n, c, stack_after, layout.Layout.GetArrayFieldOffset(layout.Layout.ArrayField.SizesPointer, c.t), 0x18);
                stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);
                stack_after = ldind(n, c, stack_after, c.ms.m.GetSimpleTypeSpec(0x18));
                stack_after = ldind(n, c, stack_after, c.ms.m.GetSimpleTypeSpec(0x08));
            }
            else if (MetadataStream.CompareSignature(ctor,
                c.special_meths,
                c.special_meths.string_Pcii, null, null))
            {
                // string(char* value, int32 startIndex, int32 length)
                stack_after = copy_to_front(n, c, stack_before);
            }
            else if (MetadataStream.CompareSignature(ctor,
                c.special_meths,
                c.special_meths.string_Pa, null, null))
            {
                // string(int8* value)
                stack_after = copy_to_front(n, c, stack_before);
                stack_after = call(n, c, stack_after, false,
                    "strlen", c.special_meths, c.special_meths.strlen);
            }
            else if (MetadataStream.CompareSignature(ctor,
                c.special_meths,
                c.special_meths.string_Zcii, null, null))
            {
                // string(char[] value, int32 startIndex, int32 length)
                stack_after = copy_to_front(n, c, stack_before);
            }
            else if (MetadataStream.CompareSignature(ctor,
                c.special_meths,
                c.special_meths.string_Pc, null, null))
            {
                // string(char* value)
                stack_after = copy_to_front(n, c, stack_before);
                stack_after = call(n, c, stack_after, false,
                    "wcslen", c.special_meths, c.special_meths.wcslen);
            }
            else if (MetadataStream.CompareSignature(ctor,
                c.special_meths,
                c.special_meths.string_PaiiEncoding, null, null))
            {
                // string(int8* value, int32 startIndex, int32 length, Encoding)
                stack_after = copy_to_front(n, c, stack_before, 1);
            }
            else if (MetadataStream.CompareSignature(ctor,
                c.special_meths,
                c.special_meths.string_Paii, null, null))
            {
                // string(int8* value, int32 startIndex, int32 length)
                stack_after = copy_to_front(n, c, stack_before, 1);
            }
            else
                throw new NotSupportedException();

            return stack_after;
        }

        private static Stack<StackItem> shiftop(CilNode n, Code c, Stack<StackItem> stack_before, cil.Opcode.SingleOpcodes oc,
            int ct_ret = Opcode.ct_unknown,
            int src_a = -1, int src_b = -1, int res_a = -1)
        {
            Stack<StackItem> stack_after = new Stack<StackItem>(stack_before);

            if (src_a == -1)
            {
                stack_after.Pop();
                src_a = 1;
            }
            if (src_b == -1)
            {
                stack_after.Pop();
                src_b = 0;
            }

            var si_b = stack_before.Peek(src_b);
            var si_a = stack_before.Peek(src_a);

            var ct_a = si_a.ct;
            var ct_b = si_b.ct;

            if (ct_ret == Opcode.ct_unknown)
            {
                ct_ret = int_op_valid(ct_a, ct_a, oc);
                if (ct_ret == Opcode.ct_unknown)
                    throw new Exception("Invalid shift operation between " + Opcode.ct_names[ct_a] + " and " + Opcode.ct_names[ct_b]);
            }

            StackItem si = new StackItem();
            si._ct = ct_ret;

            switch (ct_ret)
            {
                case Opcode.ct_int32:
                    si.ts = c.ms.m.GetSimpleTypeSpec(0x8);
                    break;
                case Opcode.ct_int64:
                    si.ts = c.ms.m.GetSimpleTypeSpec(0xa);
                    break;
                case Opcode.ct_intptr:
                    si.ts = c.ms.m.GetSimpleTypeSpec(0x18);
                    break;
                case Opcode.ct_float:
                    si.ts = c.ms.m.GetSimpleTypeSpec(0xd);
                    break;
                case Opcode.ct_object:
                    si.ts = c.ms.m.GetSimpleTypeSpec(0x1c);
                    break;
            }

            if (res_a == -1)
            {
                stack_after.Push(si);
                res_a = 0;
            }
            else
            {
                stack_after[stack_after.Count - 1 - res_a] = si;
            }

            int noc = 0;
            switch (oc)
            {
                case cil.Opcode.SingleOpcodes.shl:
                    noc = Opcode.oc_shl;
                    break;
                case cil.Opcode.SingleOpcodes.shr:
                    noc = Opcode.oc_shr;
                    break;
                case cil.Opcode.SingleOpcodes.shr_un:
                    noc = Opcode.oc_shr_un;
                    break;
                case cil.Opcode.SingleOpcodes.mul:
                case cil.Opcode.SingleOpcodes.mul_ovf:
                case cil.Opcode.SingleOpcodes.mul_ovf_un:
                    noc = Opcode.oc_mul;
                    break;
            }

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = noc, ct = ct_a, ct2 = ct_b, stack_before = stack_before, stack_after = stack_after, arg_a = src_a, arg_b = src_b, res_a = res_a });

            return stack_after;
        }

        private static Stack<StackItem> newarr(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            var arr_elem_type = n.GetTokenAsTypeSpec(c);
            var arr_type = new metadata.TypeSpec { m = arr_elem_type.m, stype = TypeSpec.SpecialType.SzArray, other = arr_elem_type };
            var et_size = c.t.GetSize(arr_elem_type);

            c.t.r.VTableRequestor.Request(arr_elem_type.Box);
            c.t.r.VTableRequestor.Request(arr_type);

            /* Determine size of array object.
             * 
             * Layout = 
             * 
             * Array object layout
             * Lobounds array (4 * rank)
             * Sizes array (4 * rank)
             * Data array (numElems * et_size)
             */

            var int32_size = c.t.GetCTSize(Opcode.ct_int32);
            var intptr_size = c.t.GetPointerSize();

            var arr_obj_size = layout.Layout.GetTypeSize(arr_type, c.t);
            var total_static_size = arr_obj_size +
                2 * int32_size;

            var stack_after = copy_to_front(n, c, stack_before, 0);
            stack_after = ldc(n, c, stack_after, et_size);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.mul);
            stack_after = ldc(n, c, stack_after, total_static_size);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add);
            stack_after = conv(n, c, stack_after, 0x18);

            /* Allocate object */
            stack_after = call(n, c, stack_after, false, "gcmalloc", c.special_meths, c.special_meths.gcmalloc);

            /* Store lobounds */
            stack_after = copy_to_front(n, c, stack_after);
            stack_after = ldc(n, c, stack_after, arr_obj_size, 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add);
            stack_after = ldc(n, c, stack_after, 0);
            stack_after = stind(n, c, stack_after, int32_size);

            /* Store size */
            stack_after = copy_to_front(n, c, stack_after);
            stack_after = ldc(n, c, stack_after, arr_obj_size + int32_size, 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add);
            stack_after = stind(n, c, stack_after, int32_size, 2, 0);

            /* Store vtbl pointer */
            stack_after = copy_to_front(n, c, stack_after);
            stack_after = ldlab(n, c, stack_after, c.ms.m.MangleType(arr_type));
            stack_after = stind(n, c, stack_after, intptr_size);

            /* Store mutex lock */
            stack_after = copy_to_front(n, c, stack_after);
            stack_after = ldc(n, c, stack_after, layout.Layout.GetArrayFieldOffset(layout.Layout.ArrayField.MutexLock, c.t), 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add);
            stack_after = ldc(n, c, stack_after, 0, 0x18);
            stack_after = stind(n, c, stack_after, intptr_size);

            /* Store etype vtbl pointer */
            stack_after = copy_to_front(n, c, stack_after);
            stack_after = ldc(n, c, stack_after, layout.Layout.GetArrayFieldOffset(layout.Layout.ArrayField.ElemTypeVtblPointer, c.t), 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add);
            stack_after = ldlab(n, c, stack_after, c.ms.m.MangleType(arr_elem_type));
            stack_after = stind(n, c, stack_after, intptr_size);

            /* Store lobounds pointer */
            stack_after = copy_to_front(n, c, stack_after);
            stack_after = ldc(n, c, stack_after, layout.Layout.GetArrayFieldOffset(layout.Layout.ArrayField.LoboundsPointer, c.t), 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add);
            stack_after = copy_to_front(n, c, stack_after, 1);
            stack_after = ldc(n, c, stack_after, arr_obj_size, 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add);
            stack_after = stind(n, c, stack_after, intptr_size);

            /* Store sizes pointer */
            stack_after = copy_to_front(n, c, stack_after);
            stack_after = ldc(n, c, stack_after, layout.Layout.GetArrayFieldOffset(layout.Layout.ArrayField.SizesPointer, c.t), 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add);
            stack_after = copy_to_front(n, c, stack_after, 1);
            stack_after = ldc(n, c, stack_after, arr_obj_size + int32_size, 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add);
            stack_after = stind(n, c, stack_after, intptr_size);

            /* Store data pointer */
            stack_after = copy_to_front(n, c, stack_after);
            stack_after = ldc(n, c, stack_after, layout.Layout.GetArrayFieldOffset(layout.Layout.ArrayField.DataArrayPointer, c.t), 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add);
            stack_after = copy_to_front(n, c, stack_after, 1);
            stack_after = ldc(n, c, stack_after, arr_obj_size + 2 * int32_size, 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add);
            stack_after = stind(n, c, stack_after, intptr_size);

            /* Store elem type size */
            stack_after = copy_to_front(n, c, stack_after);
            stack_after = ldc(n, c, stack_after, layout.Layout.GetArrayFieldOffset(layout.Layout.ArrayField.ElemTypeSize, c.t), 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add);
            stack_after = ldc(n, c, stack_after, et_size);
            stack_after = stind(n, c, stack_after, int32_size);

            /* Store rank */
            stack_after = copy_to_front(n, c, stack_after);
            stack_after = ldc(n, c, stack_after, layout.Layout.GetArrayFieldOffset(layout.Layout.ArrayField.Rank, c.t), 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add);
            stack_after = ldc(n, c, stack_after, 1);
            stack_after = stind(n, c, stack_after, int32_size);

            /* Convert to an object on the stack of the appropriate size */
            var stack_after2 = new Stack<StackItem>(stack_after);
            stack_after2.Pop();
            stack_after2.Pop();
            stack_after2.Push(new StackItem { ts = arr_type });
            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_stackcopy, ct = ir.Opcode.ct_object, vt_size = intptr_size, stack_before = stack_after, stack_after = stack_after2 });

            return stack_after2;
        }

        private static Stack<StackItem> castclass(CilNode n, Code c, Stack<StackItem> stack_before, bool isinst = false,
            metadata.TypeSpec from_type = null,
            metadata.TypeSpec to_type = null)
        {
            if(from_type == null)
                from_type = stack_before.Peek().ts;
            if(to_type == null)
                to_type = n.GetTokenAsTypeSpec(c);

            if (to_type.IsValueType)
                to_type = to_type.Box;

            if(from_type != null &&
                from_type.Equals(to_type) ||
                from_type.IsAssignmentCompatibleWith(to_type) ||
                from_type.IsSubclassOf(to_type))
            {
                /* We can statically prove the cast will succeed */
                var stack_after = new Stack<StackItem>(stack_before);
                stack_after.Pop();
                stack_after.Push(new StackItem { ts = to_type });
                return stack_after;
            }
            else
            {
                /* There is a chance the cast will succeed but we
                     need to resort to a runtime check */

                var to_str = to_type.MangleType();
                var stack_after = ldlab(n, c, stack_before, to_str);
                stack_after = ldc(n, c, stack_after, isinst ? 0 : 1);
                stack_after = call(n, c, stack_after, false, "castclassex", c.special_meths, c.special_meths.castclassex);

                stack_after.Peek().ts = to_type;

                c.t.r.VTableRequestor.Request(to_type);

                return stack_after;
            }

            throw new NotImplementedException();
        }

        private static Stack<StackItem> ldc(CilNode n,
            Code c, Stack<StackItem> stack_before,
            long v, int stype = 0x08)
        {
            var val = BitConverter.GetBytes(v);
            return ldc(n, c, stack_before, v, val, stype);
        }

        private static Stack<StackItem> ldc(CilNode n,
            Code c, Stack<StackItem> stack_before,
            long v, byte[] val, int stype = 0x08)
        {
            var stack_after = new Stack<StackItem>(stack_before);
            var si = new StackItem();
            si.ts = c.ms.m.GetSimpleTypeSpec(stype);
            si.min_l = v;
            si.max_l = v;

            stack_after.Add(si);

            n.irnodes.Add(new CilNode.IRNode { parent = n, imm_l = v, imm_val = val, opcode = Opcode.oc_ldc, ctret = Opcode.GetCTFromType(si.ts), vt_size = c.t.GetSize(si.ts), stack_after = stack_after, stack_before = stack_before });
            return stack_after;
        }

        private static Stack<StackItem> ldlab(CilNode n, Code c, Stack<StackItem> stack_before, string v, int disp = 0)
        {
            var stack_after = new Stack<StackItem>(stack_before);
            var si = new StackItem();
            si.ts = c.ms.m.GetSimpleTypeSpec(0x18);
            si.str_val = v;

            stack_after.Add(si);

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_ldlabaddr, ct = Opcode.ct_object, imm_l = disp, imm_lab = v, stack_after = stack_after, stack_before = stack_before });

            return stack_after;
        }

        internal static Stack<StackItem> ldind(CilNode n, Code c, Stack<StackItem> stack_before, TypeSpec ts, int src = -1, bool check_type = true, int res = -1)
        {
            Stack<StackItem> stack_after = new Stack<StackItem>(stack_before);

            StackItem st_src;
            if (src == -1)
            {
                st_src = stack_after.Pop();
                src = 0;
            }
            else
                st_src = stack_after.Peek(src);

            if (res == -1)
            {
                stack_after.Push(new StackItem { ts = ts });
                res = 0;
            }
            else
                stack_after[stack_after.Count - 1 - res] = new StackItem { ts = ts };

            var ct_src = st_src.ct;

            ulong is_tls = 0;
            if (check_type)
            {
                switch (ct_src)
                {
                    case Opcode.ct_intptr:
                    case Opcode.ct_ref:
                        break;
                    case Opcode.ct_tls_intptr:
                        is_tls = 1;
                        break;
                    default:
                        throw new Exception("Cannot perform " + n.opcode.ToString() + " from address type " + Opcode.ct_names[ct_src]);
                }
            }

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_ldind, ct = Opcode.GetCTFromType(ts), vt_size = c.t.GetSize(ts), imm_ul = is_tls, imm_l = ts.IsSigned ? 1 : 0, stack_before = stack_before, stack_after = stack_after, arg_a = src, res_a = res });

            return stack_after;
        }

        internal static Stack<StackItem> ldflda(CilNode n, Code c, Stack<StackItem> stack_before, bool is_static, out TypeSpec fld_ts, int src_a = 0,
            MethodSpec fs = null)
        {
            TypeSpec ts;
            if(fs == null)
                fs = n.GetTokenAsMethodSpec(c);
            ts = fs.type;
            //if (!c.ms.m.GetFieldDefRow(table_id, row, out ts, out fs))
            //throw new Exception("Field not found");

            if (is_static)
                c.static_types_referenced.Add(ts);

            fld_ts = c.ms.m.GetFieldType(fs, ts.gtparams, c.ms.gmparams);

            var fld_addr = layout.Layout.GetFieldOffset(ts, fs, c.t, out var is_tls, is_static);

            Stack<StackItem> stack_after = new Stack<StackItem>(stack_before);
            StackItem si = new StackItem
            {
                _ct = Opcode.ct_intptr,
                ts = fld_ts.ManagedPointer,
                max_l = fld_addr,
                max_ul = (ulong)fld_addr,
                min_l = fld_addr,
                min_ul = (ulong)fld_addr
            };

            if(is_static)
            {
                var static_name = c.ms.m.MangleType(ts) + "S";
                if (is_tls)
                {
                    static_name = static_name + "T";
                    si._ct = Opcode.ct_tls_intptr;
                }
                si.str_val = static_name;

                n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_ldlabaddr, ct = si._ct, imm_lab = static_name, imm_l = fld_addr, imm_ul = is_tls ? 1UL : 0UL, stack_before = stack_before, stack_after = stack_after });

                c.t.r.StaticFieldRequestor.Request(ts.Unbox);
            }
            else
            {
                n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_ldc, ctret = Opcode.ct_intptr, imm_l = fld_addr, stack_before = stack_before, stack_after = stack_after, arg_a = src_a, arg_b = 0 });
            }

            stack_after.Push(si);
            return stack_after;
        }

        private static Stack<StackItem> ret(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            var rt_idx = c.ms.m.GetMethodDefSigRetTypeIndex(c.ms.msig);
            var rt_ts = c.ms.m.GetTypeSpec(ref rt_idx, c.ms.gtparams, c.ms.gmparams);

            int ret_ct = ir.Opcode.ct_unknown;
            int ret_vt_size = 0;

            if (rt_ts == null && stack_before.Count != 0)
                throw new Exception("Inconsistent stack on ret");
            else if (rt_ts != null)
            {
                if (stack_before.Count != 1)
                    throw new Exception("Inconsistent stack on ret");
                ret_ct = ir.Opcode.GetCTFromType(rt_ts);
                ret_vt_size = c.t.GetSize(rt_ts);
            }

            var stack_after = new Stack<StackItem>(c.lv_types.Length);

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_ret, ct = ret_ct, vt_size = ret_vt_size, stack_before = stack_before, stack_after = stack_after });

            return stack_after;
        }

        private static Stack<StackItem> get_virt_ftn_ptr(CilNode n, Code c, Stack<StackItem> stack_before, int arg_a = -2)
        {
            var ms = c.ms.m.GetMethodSpec(n.inline_uint, c.ms.gtparams, c.ms.gmparams);
            var ts = ms.type;

            var l = layout.Layout.GetVTableOffset(ms, c.t) * c.t.psize;

            Stack<StackItem> stack_after = stack_before;

            if (arg_a != -2) // used for ldvirtftn where -2 actually means 0 ie top of stack
                stack_after = copy_to_front(n, c, stack_before, arg_a);

            // load vtable
            stack_after = conv(n, c, stack_after, 0x18);
            stack_after = ldind(n, c, stack_after, c.ms.m.SystemIntPtr);

            if (ms.type.IsInterface)
            {
                c.t.r.VTableRequestor.Request(ms.type);

                // load interface map
                stack_after = ldc(n, c, stack_after, c.t.psize, 0x18);
                stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, ir.Opcode.ct_intptr);
                stack_after = ldind(n, c, stack_after, ms.m.SystemIntPtr);

                // iterate through interface map looking for requested interface
                var t1 = c.next_mclabel--;
                var t2 = c.next_mclabel--;
                var t3 = c.next_mclabel--;

                stack_after = mclabel(n, c, stack_after, t1);
                stack_after = copy_to_front(n, c, stack_after);
                // dereference ifacemap pointer
                stack_after = ldind(n, c, stack_after, ms.m.SystemIntPtr);

                // first check if it is null
                stack_after = copy_to_front(n, c, stack_after);
                stack_after = ldc(n, c, stack_after, 0, 0x18);
                stack_after = brif(n, c, stack_after, Opcode.cc_ne, t2);
                var t2_stack_in = new Stack<StackItem>(stack_after);
                // if it is, throw missing method exception
                // break point first
                n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_break, stack_before = stack_after, stack_after = stack_after });
                stack_after = pop(n, c, stack_after);
                stack_after = pop(n, c, stack_after);

                var corlib = ms.m.al.GetAssembly("mscorlib");

                stack_after = ldstr(n, c, stack_before, ms.MangleMethod());
                stack_after = newobj(n, c, stack_after,
                    corlib.GetMethodSpec(corlib.GetTypeSpec("System", "MissingMethodException"), ".ctor",
                    c.special_meths.inst_Rv_s, c.special_meths));
                stack_after = throw_(n, c, stack_after);

                // it is not null at this point, check against the search string
                stack_after = mclabel(n, c, t2_stack_in, t2);
                stack_after = ldlab(n, c, stack_after, ms.type.MangleType());
                stack_after = brif(n, c, stack_after, Opcode.cc_eq, t3);
                var t3_stack_in = new Stack<StackItem>(stack_after);

                // it is not the correct interface at this point, so increment ifacemapptr by 2
                stack_after = ldc(n, c, stack_after, c.t.psize * 2, 0x18);
                stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);
                stack_after = br(n, c, stack_after, t1);

                // it is the correct interface, get the implementation of it (at offset +1)
                stack_after = mclabel(n, c, t3_stack_in, t3);
                stack_after = ldc(n, c, stack_after, c.t.psize, 0x18);
                stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);
                stack_after = ldind(n, c, stack_after, ms.m.SystemIntPtr);
            }

            // get the correct method within the current vtable/interface implementation
            stack_after = ldc(n, c, stack_after, l, 0x18);
            stack_after = binnumop(n, c, stack_after, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);
            stack_after = ldind(n, c, stack_after, c.ms.m.SystemIntPtr);

            stack_after.Peek().ms = ms;

            return stack_after;
        }

        private static Stack<StackItem> mclabel(CilNode n, Code c, Stack<StackItem> stack_before, int v)
        {
            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = ir.Opcode.oc_mclabel, imm_l = v, stack_after = stack_before, stack_before = stack_before });
            return stack_before;
        }

        private static Stack<StackItem> copy_this_to_front(CilNode n, Code c, Stack<StackItem> stack_before)
        {
            var ms = c.ms.m.GetMethodSpec(n.inline_uint, c.ms.gtparams, c.ms.gmparams);

            uint sig_idx = ms.m.GetIntEntry(MetadataStream.tid_MethodDef, ms.mdrow,
                4);

            var pc = ms.m.GetMethodDefSigParamCountIncludeThis((int)sig_idx);

            var stack_after = copy_to_front(n, c, stack_before, pc - 1);
            return stack_after;
        }

        static Stack<StackItem> stackcopy(CilNode n, Code c, Stack<StackItem> stack_before, int src, int dest)
        {
            Stack<StackItem> stack_after = new Stack<StackItem>(stack_before);

            var si = stack_before.Peek(src);

            stack_after[stack_after.Count - 1 - dest] = si.Clone();

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_stackcopy, ct = Opcode.GetCTFromType(si.ts), vt_size = c.t.GetSize(si.ts), arg_a = src, res_a = dest, stack_before = stack_before, stack_after = stack_after });

            return stack_after;
        }

        private static Stack<StackItem> copy_to_front(CilNode n, Code c, Stack<StackItem> stack_before, int v = 0)
        {
            Stack<StackItem> stack_after = new Stack<StackItem>(stack_before);

            var si = stack_before.Peek(v);

            var sidest = si.Clone();

            stack_after.Push(sidest);

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_stackcopy, ct = Opcode.GetCTFromType(si.ts), vt_size = c.t.GetSize(si.ts), arg_a = v, stack_before = stack_before, stack_after = stack_after });

            return stack_after;
        }

        internal static Stack<StackItem> ldarg(CilNode n, Code c, Stack<StackItem> stack_before, int v)
        {
            Stack<StackItem> stack_after = new Stack<StackItem>(stack_before);

            var si = new StackItem();
            var ts = c.la_types[v];
            si.ts = ts;
            stack_after.Push(si);

            var vt_size = c.t.GetSize(ts);
            var ct = Opcode.GetCTFromType(ts);

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_ldarg, ct = ct, vt_size = vt_size, imm_l = v, stack_before = stack_before, stack_after = stack_after });

            return stack_after;
        }

        private static Stack<StackItem> starg(CilNode n, Code c, Stack<StackItem> stack_before, int v)
        {
            Stack<StackItem> stack_after = new Stack<StackItem>(stack_before);
            stack_after.Pop();

            var ts = c.la_types[v];

            var vt_size = c.t.GetSize(ts);

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_starg, ct = ir.Opcode.GetCTFromType(ts), vt_size = vt_size, imm_l = v, stack_before = stack_before, stack_after = stack_after });

            return stack_after;
        }

        private static Stack<StackItem> ldarga(CilNode n, Code c, Stack<StackItem> stack_before, int v)
        {
            Stack<StackItem> stack_after = new Stack<StackItem>(stack_before);

            var si = new StackItem();
            var ts = c.la_types[v];
            ts = new TypeSpec { m = ts.m, stype = TypeSpec.SpecialType.MPtr, other = ts };
            si.ts = ts;
            stack_after.Push(si);

            var vt_size = c.t.GetSize(ts);
            var ct = Opcode.GetCTFromType(ts);

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_ldarga, ct = ct, vt_size = vt_size, imm_l = v, stack_before = stack_before, stack_after = stack_after });

            return stack_after;
        }

        private static Stack<StackItem> ldloca(CilNode n, Code c, Stack<StackItem> stack_before, int v)
        {
            Stack<StackItem> stack_after = new Stack<StackItem>(stack_before);

            var si = new StackItem();
            var ts = c.lv_types[v];
            ts = new TypeSpec { m = ts.m, stype = TypeSpec.SpecialType.MPtr, other = ts };
            si.ts = ts;
            stack_after.Push(si);

            var vt_size = c.t.GetSize(ts);
            var ct = Opcode.GetCTFromType(ts);

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_ldloca, ct = ct, vt_size = vt_size, imm_l = v, stack_before = stack_before, stack_after = stack_after });

            return stack_after;
        }


        internal static Stack<StackItem> stind(CilNode n, Code c, Stack<StackItem> stack_before, int vt_size, int val = 0, int addr = 1)
        {
            Stack<StackItem> stack_after = new Stack<StackItem>(stack_before);

            var st_src = stack_after.Peek(val);
            var st_dest = stack_after.Peek(addr);

            if ((addr == 0 && val == 1) || (addr == 1 && val == 0))
            {
                stack_after.Pop();
                stack_after.Pop();
            }
            else if (addr == 0 || val == 0)
                stack_after.Pop();

            var ct_dest = st_dest.ct;

            ulong is_tls = 0;
            switch(ct_dest)
            {
                case Opcode.ct_intptr:
                case Opcode.ct_ref:
                    break;
                case Opcode.ct_tls_intptr:
                    is_tls = 1;
                    break;
                default:
                    throw new Exception("Cannot perform " + n.opcode.ToString() + " to address type " + Opcode.ct_names[ct_dest]);
            }

            var ct_src = st_src.ct;

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_stind, ct = ir.Opcode.ct_intptr, ct2 = ct_src, ctret = ir.Opcode.ct_unknown, imm_ul = is_tls, vt_size = vt_size, stack_before = stack_before, stack_after = stack_after, arg_a = addr, arg_b = val });

            return stack_after;
        }

        private static Stack<StackItem> conv(CilNode n, Code c, Stack<StackItem> stack_before, int to_stype, bool is_ovf = false, bool is_un = false)
        {
            Stack<StackItem> stack_after = new Stack<StackItem>(stack_before);

            var st = stack_after.Pop();
            var ct = st.ct;

            var to_ct = Opcode.GetCTFromType(to_stype);

            if (!conv_op_valid(ct, to_stype))
                throw new Exception("Cannot perform " + n.opcode.ToString() + " with " + Opcode.ct_names[ct]);

            StackItem si = new StackItem();
            si.ts = c.ms.m.GetSimpleTypeSpec(to_stype);
            stack_after.Push(si);

            long imm = 0;
            if (is_un)
                imm |= 1;
            if (is_ovf)
                imm |= 2;

            if(Opcode.IsTLSCT(ct))
            {
                ct = Opcode.UnTLSCT(ct);
                si._ct = Opcode.TLSCT(ct);
            }

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_conv, imm_l = imm, ct = ct, ctret = to_ct, stack_before = stack_before, stack_after = stack_after });

            return stack_after;
        }

        private static bool conv_op_valid(int ct, int to_stype)
        {
            switch(ct)
            {
                case Opcode.ct_int32:
                case Opcode.ct_int64:
                case Opcode.ct_intptr:
                case Opcode.ct_float:
                case Opcode.ct_tls_intptr:
                case Opcode.ct_tls_int32:
                case Opcode.ct_tls_int64:
                    return true;

                case Opcode.ct_ref:
                case Opcode.ct_object:
                    switch(to_stype)
                    {
                        case 0x0a:
                        case 0x0b:
                        case 0x18:
                        case 0x19:
                            return true;
                        default:
                            return false;
                    }

                default:
                    return false;
            }
        }

        public static Stack<StackItem> call(CilNode n, Code c, Stack<StackItem> stack_before, bool is_calli = false, string override_name = null, MetadataStream override_m = null, int override_msig = 0,
            int calli_ftn = 0, MethodSpec override_ms = null)
        {
            if (calli_ftn != 0)
                throw new NotImplementedException();

            Stack<StackItem> stack_after = new Stack<StackItem>(stack_before);

            MetadataStream m;
            string mangled_meth;
            int sig_idx;
            MethodSpec ms;
            bool is_reinterpret_as = false;
            if(override_ms != null)
            {
                m = override_ms.m;
                mangled_meth = override_ms.MangleMethod();
                sig_idx = override_ms.msig;
                ms = override_ms;
            }
            else if (override_name != null)
            {
                m = override_m;
                mangled_meth = override_name;
                sig_idx = override_msig;

                ms = new MethodSpec { m = m, msig = sig_idx, mangle_override = override_name };
            }
            else
            {
                ms = c.ms.m.GetMethodSpec(n.inline_uint, c.ms.gtparams, c.ms.gmparams);
                m = ms.m;
                mangled_meth = c.ms.m.MangleMethod(ms);

                if (ms.mdrow != 0)
                {
                    sig_idx = (int)ms.m.GetIntEntry(MetadataStream.tid_MethodDef, ms.mdrow,
                        4);
                }
                else
                    sig_idx = ms.msig;

                if (ms.HasCustomAttribute("_ZN14libsupcs#2Edll8libsupcs28ReinterpretAsMethodAttribute_7#2Ector_Rv_P1u1t"))
                {
                    // This is a 'Reinterpret As' method - handle accordingly
                    is_reinterpret_as = true;

                    if (is_calli)
                        throw new Exception("calli/callvirt to ReinterpretAs method");
                }
                else
                {
                    intcall_delegate intcall;

                    /* mangle a non-generic version of the function to support generic method intcalls */
                    string ic_mangled_meth = mangled_meth;
                    if(ms.IsInstantiatedGenericMethod)
                    {
                        var non_g_ms = new metadata.MethodSpec { type = ms.type, gmparams = null, m = ms.m, mdrow = ms.mdrow, msig = ms.msig };
                        ic_mangled_meth = non_g_ms.m.MangleMethod(non_g_ms);
                    }

                    if (is_calli == false && intcalls.TryGetValue(ic_mangled_meth, out intcall))
                    {
                        var r = intcall(n, c, stack_before);
                        if (r != null)
                            return r;
                    }

                    var ca_mra = ms.GetCustomAttribute("_ZN14libsupcs#2Edll8libsupcs29MethodReferenceAliasAttribute_7#2Ector_Rv_P2u1tu1S");
                    if(ca_mra != -1)
                    {
                        // This is a call to a method that has a different name
                        int val_idx = (int)m.GetIntEntry(MetadataStream.tid_CustomAttribute,
                            ca_mra, 2);

                        m.SigReadUSCompressed(ref val_idx);
                        var prolog = m.sh_blob.di.ReadUShort(val_idx);
                        if (prolog == 0x0001)
                        {
                            val_idx += 2;

                            var str_len = m.SigReadUSCompressed(ref val_idx);
                            StringBuilder sb = new StringBuilder();
                            for (uint i = 0; i < str_len; i++)
                            {
                                sb.Append((char)m.sh_blob.di.ReadByte(val_idx++));
                            }
                            mangled_meth = sb.ToString();
                            ms = new MethodSpec { m = ms.m, msig = ms.msig, gmparams = ms.gmparams, mdrow = ms.mdrow, mangle_override = mangled_meth, type = ms.type };
                        }

                    }

                    c.t.r.MethodRequestor.Request(ms);
                }
            }

            var pc = m.GetMethodDefSigParamCountIncludeThis((int)sig_idx);
            var rt_idx = m.GetMethodDefSigRetTypeIndex((int)sig_idx);
            var rt = m.GetTypeSpec(ref rt_idx, ms.gtparams, ms.gmparams);

            if(is_reinterpret_as)
            {
                // Handle specially
                var popped_st = stack_after.Pop();
                var new_st = popped_st.Clone();
                new_st.ts = rt;
                stack_after.Push(new_st);

                n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_stackcopy, arg_a = 0, res_a = 0, stack_before = stack_before, stack_after = stack_after });
                return stack_after;
            }

            while (pc-- > 0)
                stack_after.Pop();
            if (is_calli)
                stack_after.Pop();

            int ct = Opcode.ct_unknown;

            if(rt != null)
            {
                StackItem r = new StackItem();
                r.ts = rt;
                stack_after.Push(r);
                ct = Opcode.GetCTFromType(rt);
            }

            int oc = Opcode.oc_call;
            if (is_calli)
                oc = Opcode.oc_calli;
            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = oc, imm_ms = ms, ct = ct, stack_before = stack_before, stack_after = stack_after });

            return stack_after;
        }

        public static Stack<StackItem> ldstr(CilNode n, Code c, Stack<StackItem> stack_before,
            string str = null)
        {
            Stack<StackItem> stack_after = new Stack<StackItem>(stack_before);

            if (str == null)
            {
                var tok = n.inline_uint;
                if ((tok & 0x70000000) != 0x70000000)
                    throw new Exception("Invalid string token");

                str = c.ms.m.GetUserString((int)(tok & 0x00ffffffUL));
            }

            var str_addr = c.t.st.GetStringAddress(str, c.t);
            var st_name = c.t.st.GetStringTableName();

            StackItem si = new StackItem();
            si.ts = c.ms.m.GetSimpleTypeSpec(0x0e);
            si.str_val = str;

            stack_after.Push(si);

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_ldlabaddr, ct = Opcode.ct_object, imm_l = str_addr, imm_lab = st_name, stack_after = stack_after, stack_before = stack_before });

            return stack_after;
        }

        private static Stack<StackItem> brif(CilNode n, Code c, Stack<StackItem> stack_before, int cc, int target = int.MaxValue)
        {
            Stack<StackItem> stack_after = new Stack<StackItem>(stack_before);

            if (target == int.MaxValue)
                target = n.il_offsets_after[1];

            var si_b = stack_after.Pop();
            var si_a = stack_after.Pop();

            var ct_a = si_a.ct;
            var ct_b = si_b.ct;

            if (!bin_comp_valid(ct_a, ct_b, cc, false))
                throw new Exception("Invalid comparison between " + Opcode.ct_names[ct_a] + " and " + Opcode.ct_names[ct_b]);

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_brif, imm_l = target, imm_ul = (uint)cc, ct = ct_a, ct2 = ct_b, stack_after = stack_after, stack_before = stack_before, arg_a = 1, arg_b = 0 });

            return stack_after;
        }

        private static Stack<StackItem> cmp(CilNode n, Code c, Stack<StackItem> stack_before, int cc)
        {
            Stack<StackItem> stack_after = new Stack<StackItem>(stack_before);

            var si_b = stack_after.Pop();
            var si_a = stack_after.Pop();

            var ct_a = si_a.ct;
            var ct_b = si_b.ct;

            if (!bin_comp_valid(ct_a, ct_b, cc, true))
                throw new Exception("Invalid comparison between " + Opcode.ct_names[ct_a] + " and " + Opcode.ct_names[ct_b]);

            var si = new StackItem();
            TypeSpec ts = c.ms.m.GetSimpleTypeSpec(0x8);
            si.ts = ts;
            stack_after.Push(si);

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_cmp, ct = ct_a, ct2 = ct_b, ctret = Opcode.ct_unknown, imm_ul = (uint)cc, stack_after = stack_after, stack_before = stack_before, arg_a = 1, arg_b = 0 });

            return stack_after;
        }

        private static Stack<StackItem> unnumop(CilNode n, Code c,
            Stack<StackItem> stack_before,
            cil.Opcode.SingleOpcodes oc)
        {
            Stack<StackItem> stack_after = new Stack<StackItem>(stack_before);

            stack_after.Pop();

            var si_a = stack_before.Peek();
            var ct_a = si_a.ct;

            var ct_ret = un_op_valid(ct_a, oc);
            if (ct_ret == Opcode.ct_unknown)
                throw new Exception("Invalid unary operation on " + Opcode.ct_names[ct_a]);

            StackItem si = new StackItem();
            si._ct = ct_ret;

            switch (ct_ret)
            {
                case Opcode.ct_int32:
                    si.ts = c.ms.m.GetSimpleTypeSpec(0x8);
                    break;
                case Opcode.ct_int64:
                    si.ts = c.ms.m.GetSimpleTypeSpec(0xa);
                    break;
                case Opcode.ct_intptr:
                    si.ts = c.ms.m.GetSimpleTypeSpec(0x18);
                    break;
                case Opcode.ct_float:
                    si.ts = c.ms.m.GetSimpleTypeSpec(0xd);
                    break;
                case Opcode.ct_object:
                    si.ts = c.ms.m.GetSimpleTypeSpec(0x1c);
                    break;
            }

            stack_after.Push(si);
            int noc = 0;
            switch(oc)
            {
                case cil.Opcode.SingleOpcodes.neg:
                    noc = Opcode.oc_neg;
                    break;
                case cil.Opcode.SingleOpcodes.not:
                    noc = Opcode.oc_not;
                    break;
                default:
                    throw new NotImplementedException();
            }

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = noc, ct = ct_a, ctret = ct_ret, stack_before = stack_before, stack_after = stack_after });

            return stack_after;
        }

        private static int un_op_valid(int ct, cil.Opcode.SingleOpcodes oc)
        {
            switch(ct)
            {
                case Opcode.ct_int32:
                case Opcode.ct_int64:
                case Opcode.ct_intptr:
                    return ct;
                case Opcode.ct_float:
                    if (oc == cil.Opcode.SingleOpcodes.neg)
                        return ct;
                    else
                        return Opcode.ct_unknown;                        
                default:
                    return Opcode.ct_unknown;
            }
        }

        internal static Stack<StackItem> binnumop(CilNode n, Code c, Stack<StackItem> stack_before, cil.Opcode.SingleOpcodes oc,
            int ct_ret = Opcode.ct_unknown,
            int src_a = -1, int src_b = -1, int res_a = -1)
        {
            Stack<StackItem> stack_after = new Stack<StackItem>(stack_before);

            if(src_a == -1)
            {
                stack_after.Pop();
                if (src_b == -1)
                    src_a = 1;
                else
                    src_a = 0;
            }
            if(src_b == -1)
            {
                stack_after.Pop();
                src_b = 0;
            }

            var si_b = stack_before.Peek(src_b);
            var si_a = stack_before.Peek(src_a);

            var ct_a = si_a.ct;
            var ct_b = si_b.ct;

            if (ct_ret == Opcode.ct_unknown)
            {
                ct_ret = bin_op_valid(ct_a, ct_b, oc);
                if (ct_ret == Opcode.ct_unknown)
                    throw new Exception("Invalid binary operation between " + Opcode.ct_names[ct_a] + " and " + Opcode.ct_names[ct_b]);
            }

            bool is_un = false;
            bool is_ovf = false;
            
            switch(oc)
            {
                case cil.Opcode.SingleOpcodes.add_ovf:
                    is_ovf = true;
                    break;
                case cil.Opcode.SingleOpcodes.add_ovf_un:
                    is_ovf = true;
                    is_un = true;
                    break;
                case cil.Opcode.SingleOpcodes.sub_ovf:
                    is_ovf = true;
                    break;
                case cil.Opcode.SingleOpcodes.sub_ovf_un:
                    is_ovf = true;
                    is_un = true;
                    break;
                case cil.Opcode.SingleOpcodes.mul_ovf:
                    is_ovf = true;
                    break;
                case cil.Opcode.SingleOpcodes.mul_ovf_un:
                    is_ovf = true;
                    is_un = true;
                    break;
                case cil.Opcode.SingleOpcodes.div_un:
                    is_un = true;
                    break;
                case cil.Opcode.SingleOpcodes.rem_un:
                    is_un = true;
                    break;
            }

            long imm = 0;
            if (is_un)
                imm |= 1;
            if (is_ovf)
                imm |= 2;

            StackItem si = new StackItem();
            si._ct = ct_ret;

            switch(ct_ret)
            {
                case Opcode.ct_int32:
                    si.ts = c.ms.m.GetSimpleTypeSpec(0x8);
                    break;
                case Opcode.ct_int64:
                    si.ts = c.ms.m.GetSimpleTypeSpec(0xa);
                    break;
                case Opcode.ct_intptr:
                    si.ts = c.ms.m.GetSimpleTypeSpec(0x18);
                    break;
                case Opcode.ct_float:
                    si.ts = c.ms.m.GetSimpleTypeSpec(0xd);
                    break;
                case Opcode.ct_object:
                    si.ts = c.ms.m.GetSimpleTypeSpec(0x1c);
                    break;
            }

            if (res_a == -1)
            {
                stack_after.Push(si);
                res_a = 0;
            }
            else
            {
                stack_after[stack_after.Count - 1 - res_a] = si;
            }

            int noc = 0;
            switch(oc)
            {
                case cil.Opcode.SingleOpcodes.add:
                case cil.Opcode.SingleOpcodes.add_ovf:
                case cil.Opcode.SingleOpcodes.add_ovf_un:
                    noc = Opcode.oc_add;
                    break;
                case cil.Opcode.SingleOpcodes.sub:
                case cil.Opcode.SingleOpcodes.sub_ovf:
                case cil.Opcode.SingleOpcodes.sub_ovf_un:
                    noc = Opcode.oc_sub;
                    break;
                case cil.Opcode.SingleOpcodes.mul:
                case cil.Opcode.SingleOpcodes.mul_ovf:
                case cil.Opcode.SingleOpcodes.mul_ovf_un:
                    noc = Opcode.oc_mul;
                    break;
                case cil.Opcode.SingleOpcodes.div:
                case cil.Opcode.SingleOpcodes.div_un:
                    noc = Opcode.oc_div;
                    break;
                case cil.Opcode.SingleOpcodes.rem:
                case cil.Opcode.SingleOpcodes.rem_un:
                    noc = Opcode.oc_rem;
                    break;
                case cil.Opcode.SingleOpcodes.and:
                    noc = Opcode.oc_and;
                    break;
                case cil.Opcode.SingleOpcodes.or:
                    noc = Opcode.oc_or;
                    break;
                case cil.Opcode.SingleOpcodes.xor:
                    noc = Opcode.oc_xor;
                    break;
            }

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = noc, ct = ct_a, ct2 = ct_b, stack_before = stack_before, stack_after = stack_after, arg_a = src_a, arg_b = src_b, res_a = res_a });

            return stack_after;
        }

        private static int bin_op_valid(int ct_a, int ct_b, cil.Opcode.SingleOpcodes oc)
        {
            if (oc == cil.Opcode.SingleOpcodes.and ||
                oc == cil.Opcode.SingleOpcodes.or ||
                oc == cil.Opcode.SingleOpcodes.xor)
                return int_op_valid(ct_a, ct_b, oc);
            switch(ct_a)
            {
                case Opcode.ct_int32:
                    switch(ct_b)
                    {
                        case Opcode.ct_int32:
                            return Opcode.ct_int32;
                        case Opcode.ct_intptr:
                            return Opcode.ct_intptr;
                        case Opcode.ct_ref:
                            if (oc == cil.Opcode.SingleOpcodes.add)
                                return Opcode.ct_ref;
                            break;
                    }
                    return Opcode.ct_unknown;

                case Opcode.ct_int64:
                    if (ct_b == Opcode.ct_int64)
                        return Opcode.ct_int64;
                    break;

                case Opcode.ct_intptr:
                    switch (ct_b)
                    {
                        case Opcode.ct_int32:
                            return Opcode.ct_intptr;
                        case Opcode.ct_intptr:
                            return Opcode.ct_intptr;
                        case Opcode.ct_ref:
                            if (oc == cil.Opcode.SingleOpcodes.add)
                                return Opcode.ct_ref;
                            break;
                    }
                    return Opcode.ct_unknown;

                case Opcode.ct_float:
                    if (ct_b == Opcode.ct_float)
                        return Opcode.ct_float;
                    return Opcode.ct_unknown;

                case Opcode.ct_ref:
                    switch(ct_b)
                    {
                        case Opcode.ct_int32:
                        case Opcode.ct_intptr:
                            switch(oc)
                            {
                                case cil.Opcode.SingleOpcodes.add:
                                case cil.Opcode.SingleOpcodes.sub:
                                    return Opcode.ct_ref;
                            }
                            break;

                        case Opcode.ct_ref:
                            if (oc == cil.Opcode.SingleOpcodes.sub)
                                return Opcode.ct_intptr;
                            break;
                    }
                    break;
            }
            return Opcode.ct_unknown;
        }

        private static int int_op_valid(int ct_a, int ct_b, cil.Opcode.SingleOpcodes oc)
        {
            switch (ct_a)
            {
                case Opcode.ct_int32:
                case Opcode.ct_intptr:
                    switch (ct_b)
                    {
                        case Opcode.ct_int32:
                            return Opcode.ct_int32;
                        case Opcode.ct_intptr:
                            return Opcode.ct_intptr;
                    }
                    break;
                case Opcode.ct_int64:
                    if (ct_b == Opcode.ct_int64)
                        return Opcode.ct_int64;
                    break;
            }
            return Opcode.ct_unknown;
        }

        private static bool bin_comp_valid(int ct_a, int ct_b, int cc, bool is_cmp)
        {
            switch(ct_a)
            {
                case Opcode.ct_int32:
                    switch(ct_b)
                    {
                        case Opcode.ct_int32:
                            return true;
                        case Opcode.ct_intptr:
                            return true;
                    }
                    return false;

                case Opcode.ct_int64:
                    switch(ct_b)
                    {
                        case Opcode.ct_int64:
                            return true;
                    }
                    return false;

                case Opcode.ct_intptr:
                    switch (ct_b)
                    {
                        case Opcode.ct_int32:
                            return true;
                        case Opcode.ct_intptr:
                            return true;
                        case Opcode.ct_ref:
                            if (!is_cmp &&
                                (cc == Opcode.cc_eq || cc == Opcode.cc_ne))
                                return true;
                            if (is_cmp &&
                                (cc == Opcode.cc_eq))
                                return true;
                            return false;
                    }
                    return false;

                case Opcode.ct_float:
                    return ct_b == Opcode.ct_float;

                case Opcode.ct_ref:
                    switch (ct_b)
                    {
                        case Opcode.ct_intptr:
                            if (!is_cmp &&
                                (cc == Opcode.cc_eq || cc == Opcode.cc_ne))
                                return true;
                            if (is_cmp &&
                                (cc == Opcode.cc_eq))
                                return true;
                            return false;

                        case Opcode.ct_ref:
                            return true;
                    }
                    return false;

                case Opcode.ct_object:
                    if (ct_b != Opcode.ct_object)
                        return false;

                    if (is_cmp &&
                        (cc == Opcode.cc_eq || cc == Opcode.cc_a))
                        return true;
                    if (!is_cmp &&
                        (cc == Opcode.cc_eq || cc == Opcode.cc_ne))
                        return true;

                    return false;
            }
            return false;
        }

        private static Stack<StackItem> ldloc(CilNode n, Code c, Stack<StackItem> stack_before, int v)
        {
            Stack<StackItem> stack_after = new Stack<StackItem>(stack_before);

            var si = new StackItem();
            var ts = c.lv_types[v];
            si.ts = ts;
            stack_after.Push(si);

            var vt_size = c.t.GetSize(ts);
            var ct = Opcode.GetCTFromType(ts);

            if (stack_before.lv_tls_addrs[v])
            {
                switch(si.ct)
                {
                    case Opcode.ct_ref:
                    case Opcode.ct_intptr:
                        si._ct = Opcode.ct_tls_intptr;
                        break;
                    case Opcode.ct_int32:
                        si._ct = Opcode.ct_tls_int32;
                        break;
                    case Opcode.ct_int64:
                        si._ct = Opcode.ct_tls_int64;
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_ldloc, ctret = ct, vt_size = vt_size, imm_l = v, imm_ul = ts.IsSigned ? 1UL : 0UL, stack_before = stack_before, stack_after = stack_after });

            return stack_after;
        }

        private static Stack<StackItem> stloc(CilNode n, Code c, Stack<StackItem> stack_before, int v)
        {
            Stack<StackItem> stack_after = new Stack<StackItem>(stack_before);
            var si = stack_after.Pop();
            var ts = si.ts;

            var to_ts = c.lv_types[v];
            // TODO: ensure top of stack can be assigned to lv

            if(si._ct == Opcode.ct_tls_int32 || si._ct == Opcode.ct_tls_int64 ||
                si._ct == Opcode.ct_tls_intptr)
            {
                // we need to mark the current lv entry as being a pointer to a TLS object
                stack_after.lv_tls_addrs[v] = true;
            }

            var vt_size = c.t.GetSize(ts);
            var ct = Opcode.GetCTFromType(ts);

            n.irnodes.Add(new CilNode.IRNode { parent = n, opcode = Opcode.oc_stloc, ct = ct, ctret = Opcode.ct_unknown, vt_size = vt_size, imm_l = v, stack_before = stack_before, stack_after = stack_after });

            return stack_after;
        }
    }
}
