/* Copyright (C) 2016 by John Cronin
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
using binary_library;
using libtysila5.ir;
using libtysila5.util;
using metadata;
using libtysila5.cil;

namespace libtysila5.target.arm
{
    partial class arm_Assembler : Target
    {
        static MCInst inst(CilNode.IRNode parent,
            int inst,
            Param Rn = null,
            Param Rd = null,
            Param Rm = null,
            Param Rt = null,

            int imm = 0,

            int W = 0,
            int S = 0,

            Param[] register_list = null,
            
            string str_target = null,

            bool is_tls = false)
        {
            string str = null;
            insts.TryGetValue(inst, out str);

            MCInst ret = new MCInst
            {
                parent = parent,
                p = new Param[]
                {
                    new Param { t = ir.Opcode.vl_str, v = inst, str = str, v2 = is_tls ? 1 : 0 },
                    Rn,
                    Rd,
                    Rm,
                    Rt,
                    imm,
                    W,
                    S,
                    RegListToInt(register_list),
                    str_target
                }
            };

            return ret;
        }

        static private Param RegListToInt(Param[] register_list)
        {
            if (register_list == null)
                return 0;

            ulong val = 0;
            foreach(var r in register_list)
            {
                val |= r.mreg.mask;
            }

            return (int)val;
        }

        internal static List<MCInst> handle_add(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var a = n.stack_before.Peek(n.arg_a).reg;
            var b = n.stack_before.Peek(n.arg_b).reg;
            var res = n.stack_after.Peek(n.res_a).reg;

            List<MCInst> r = new List<MCInst>();

            if(a.type == rt_gpr && b.type == rt_gpr && res.type == rt_gpr)
            {
                r.Add(inst(n, arm_add_reg, Rd: res, Rn: a, Rm: b));

                return r;
            }

            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_and(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_br(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_break(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_brif(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_call(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            return handle_call(n, c, false, t);
        }

        internal static List<MCInst> handle_calli(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            return handle_call(n, c, true, t);
        }

        private static List<MCInst> handle_call(CilNode.IRNode n,
    Code c, metadata.MethodSpec call_ms, ir.Param[] p,
    Reg dest, string target = null, Reg temp_reg = null)

        {
            /* used for handling calls to utility functions
             *  (e.g. memcpy/memset etc) whilst ensuring that
             *  all required registers are saved around the
             *  call */

            return handle_call(n, c, false, c.t, target, call_ms, p, dest, dest != null);
        }

        static List<Reg> get_push_list(CilNode.IRNode n, Code c,
            metadata.MethodSpec call_ms, out metadata.TypeSpec rt,
            ref Reg dest, out int rct, bool want_return = true)
        {
            /* Determine which registers we need to save */
            var caller_preserves = c.t.cc_caller_preserves_map[call_ms.CallingConvention];
            ulong defined = 0;
            foreach (var si in n.stack_after)
                defined |= si.reg.mask;
            foreach (var si in n.stack_before)
                defined |= si.reg.mask;

            var rt_idx = call_ms.m.GetMethodDefSigRetTypeIndex(call_ms.msig);
            rt = call_ms.m.GetTypeSpec(ref rt_idx, call_ms.gtparams, call_ms.gmparams);
            rct = ir.Opcode.ct_unknown;
            if (rt != null && want_return)
            {
                defined &= ~n.stack_after.Peek().reg.mask;
                if (dest == null)
                    dest = n.stack_after.Peek().reg;
                rct = ir.Opcode.GetCTFromType(rt);
            }

            var to_push = new util.Set();
            to_push.Union(defined);
            to_push.Intersect(caller_preserves);
            List<Reg> push_list = new List<Reg>();
            while (!to_push.Empty)
            {
                var first_set = to_push.get_first_set();
                push_list.Add(c.t.regs[first_set]);
                to_push.unset(first_set);
            }

            return push_list;
        }

        private static List<MCInst> handle_call(CilNode.IRNode n, Code c, bool is_calli, Target t,
            string target = null, metadata.MethodSpec call_ms = null,
            ir.Param[] p = null, Reg dest = null, bool want_return = true)
        {
            List<MCInst> r = new List<MCInst>();
            if (call_ms == null)
                call_ms = n.imm_ms;
            if (target == null && is_calli == false)
                target = call_ms.m.MangleMethod(call_ms);

            Reg act_dest = null;
            metadata.TypeSpec rt;
            int rct;
            var push_list = get_push_list(n, c, call_ms,
                out rt, ref dest, out rct, want_return);

            // Store the current index, we will insert instructions
            //  to save clobbered registers here
            int push_list_index = r.Count;

            /* Push arguments */
            int push_length = 0;
            int vt_push_length = 0;
            bool vt_dest_adjust = false;

            if (rct == ir.Opcode.ct_vt)
            {
                if (dest is ContentsReg)
                {
                    act_dest = dest;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            var sig_idx = call_ms.msig;
            var pcount = call_ms.m.GetMethodDefSigParamCountIncludeThis(sig_idx);
            sig_idx = call_ms.m.GetMethodDefSigRetTypeIndex(sig_idx);
            var rt2 = call_ms.m.GetTypeSpec(ref sig_idx, call_ms.gtparams == null ? c.ms.gtparams : call_ms.gtparams, c.ms.gmparams);

            int calli_adjust = is_calli ? 1 : 0;

            metadata.TypeSpec[] push_tss = new metadata.TypeSpec[pcount];
            for (int i = 0; i < pcount; i++)
            {
                if (i == 0 && call_ms.m.GetMethodDefSigHasNonExplicitThis(call_ms.msig))
                    push_tss[i] = call_ms.type;
                else
                    push_tss[i] = call_ms.m.GetTypeSpec(ref sig_idx, call_ms.gtparams, call_ms.gmparams);
            }

            /* Push value type address if required */
            metadata.TypeSpec hidden_loc_type = null;
            if (rct == ir.Opcode.ct_vt)
            {
                var act_dest_cr = act_dest as ContentsReg;
                if (act_dest_cr == null)
                    throw new NotImplementedException();

                if (vt_dest_adjust)
                {
                    throw new NotImplementedException();
                }


                //r.Add(inst(x86_lea_r32, r_eax, act_dest, n));
                //r.Add(inst(x86_push_r32, r_eax, n));
                hidden_loc_type = call_ms.m.SystemIntPtr;
            }

            // Build list of source and destination registers for parameters
            int cstack_loc = 0;
            var cc = t.cc_map[call_ms.CallingConvention];
            var cc_class = t.cc_classmap[call_ms.CallingConvention];
            int[] la_sizes;
            metadata.TypeSpec[] la_types;
            var to_locs = t.GetRegLocs(new ir.Param
            {
                m = call_ms.m,
                v2 = call_ms.msig,
                ms = call_ms
            },
                ref cstack_loc, cc, cc_class, call_ms.CallingConvention,
                out la_sizes, out la_types,
                hidden_loc_type
            );

            Reg calli_reg = null;
            if (is_calli)
            {
                calli_reg = r_r10;

                // Add the target register to those we want to pass
                Reg[] new_to_locs = new Reg[to_locs.Length + calli_adjust];
                for (int i = 0; i < to_locs.Length; i++)
                    new_to_locs[i] = to_locs[i];
                new_to_locs[to_locs.Length] = calli_reg;

                to_locs = new_to_locs;
            }

            // Append the register arguments to the push list
            foreach (var arg in to_locs)
            {
                if (arg.type == rt_gpr || arg.type == rt_float)
                {
                    if (!push_list.Contains(arg))
                        push_list.Add(arg);
                }
            }
            List<MCInst> r2 = new List<MCInst>();

            // Insert the push instructions at the start of the stream
            int x = 0;
            foreach (var push_reg in push_list)
                handle_push(push_reg, ref x, r2, n, c);
            foreach (var r2inst in r2)
                r.Insert(push_list_index++, r2inst);

            // Get from locs

            ir.Param[] from_locs;
            int hidden_adjust = hidden_loc_type == null ? 0 : 1;
            if (p == null)
            {
                from_locs = new ir.Param[pcount + hidden_adjust + calli_adjust];
                for (int i = 0; i < pcount; i++)
                {
                    var stack_loc = pcount - i - 1;
                    if (n.arg_list != null)
                        stack_loc = n.arg_list[i];
                    from_locs[i + hidden_adjust] = n.stack_before.Peek(stack_loc + calli_adjust).reg;
                }
                if (is_calli)
                    from_locs[pcount + hidden_adjust] = n.stack_before.Peek(0).reg;
            }
            else
            {
                from_locs = p;

                // adjust any rsp relative registers dependent on how many registers we have saved
                foreach (var l in from_locs)
                {
                    if (l != null &&
                        l.t == ir.Opcode.vl_mreg &&
                        l.mreg is ContentsReg)
                    {
                        var l2 = l.mreg as ContentsReg;
                        if (l2.basereg.Equals(r_sp))
                        {
                            l2.disp += x;
                        }
                    }

                }
            }

            // Reserve any required stack space
            if (cstack_loc != 0)
            {
                push_length += cstack_loc;
                r.Add(inst(n, arm_sub_sp_imm, imm: cstack_loc));
            }

            // Move from the from list to the to list such that
            //  we never overwrite a from loc that hasn't been
            //  transfered yet
            pcount += hidden_adjust;
            pcount += calli_adjust;
            var to_do = pcount;
            bool[] done = new bool[pcount];

            if (hidden_adjust != 0)
            {
                // load up the address of the return value
                throw new NotImplementedException();

                /*
                var ret_to = to_locs[0];
                if (!(ret_to is ContentsReg))
                    r.Add(inst(t.psize == 4 ? x86_lea_r32 : x86_lea_r64, ret_to, act_dest, n));
                else
                {
                    r.Add(inst(t.psize == 4 ? x86_lea_r32 : x86_lea_r64, r_eax, act_dest, n));
                    handle_move(ret_to, r_eax, r, n, c);

                }

                to_do--;
                done[0] = true;*/
            }

            while (to_do > 0)
            {
                int done_this_iter = 0;

                for (int to_i = 0; to_i < pcount; to_i++)
                {
                    if (!done[to_i])
                    {
                        var to_reg = to_locs[to_i];
                        if (to_reg.type == rt_stack)
                        {
                            to_reg = new ContentsReg
                            {
                                basereg = r_sp,
                                disp = to_reg.stack_loc,
                                size = to_reg.size
                            };
                        }

                        bool possible = true;

                        // determine if this to register is the source of a from
                        for (int from_i = 0; from_i < pcount; from_i++)
                        {
                            if (to_i == from_i)
                                continue;
                            if (!done[from_i] && from_locs[from_i].mreg != null &&
                                from_locs[from_i].mreg.Equals(to_reg))
                            {
                                possible = false;
                                break;
                            }
                        }

                        if (possible)
                        {
                            var from_reg = from_locs[to_i];
                            switch (from_reg.t)
                            {
                                case ir.Opcode.vl_mreg:
                                    if (from_reg.want_address)
                                    {
                                        throw new NotImplementedException();
                                        /*
                                        Reg lea_to = to_reg;
                                        if (to_reg is ContentsReg)
                                            lea_to = r_eax;
                                        r.Add(inst(t.psize == 4 ? x86_lea_r32 : x86_lea_r64,
                                            lea_to, from_reg.mreg, n));
                                        handle_move(to_reg, lea_to, r, n, c);
                                        */
                                    }
                                    else
                                        handle_move(to_reg, from_reg.mreg, r, n, c);
                                    break;
                                case ir.Opcode.vl_c:
                                case ir.Opcode.vl_c32:
                                case ir.Opcode.vl_c64:
                                    if (from_reg.v > int.MaxValue ||
                                        from_reg.v < int.MinValue)
                                    {
                                        throw new NotImplementedException();
                                    }
                                    else
                                    {
                                        handle_const(to_reg, from_reg.v, r, n, c);
                                    }
                                    break;
                                default:
                                    throw new NotSupportedException();
                            }
                            to_do--;
                            done_this_iter++;
                            done[to_i] = true;
                        }
                    }
                }

                if (done_this_iter == 0)
                {
                    // find two gprs/xmms we can swap to put them both
                    //  in the correct locations

                    for (int i = 0; i < pcount; i++)
                    {
                        if (done[i])
                            continue;

                        var from_i = from_locs[i].mreg;
                        var to_i = to_locs[i];

                        if (from_i == null)
                            continue;

                        if (from_i.type != rt_gpr &&
                            from_i.type != rt_float)
                            continue;
                        if (to_i.type != rt_gpr &&
                            to_i.type != rt_float)
                            continue;

                        for (int j = 0; j < pcount; j++)
                        {
                            if (j == i)
                                continue;
                            if (done[j])
                                continue;

                            var from_j = from_locs[j].mreg;

                            if (from_j == null)
                                continue;

                            var to_j = to_locs[j];

                            if (from_i.Equals(to_j) &&
                                from_j.Equals(to_i))
                            {
                                // we can swap these
                                if (from_i.type == rt_gpr || from_i.type == rt_float)
                                {
                                    handle_swap(to_i, to_j, r, n, c);
                                }
                                else if(from_i.type == rt_float)
                                {
                                    throw new NotImplementedException();
                                }
                                done_this_iter += 2;
                                to_do -= 2;

                                done[i] = true;
                                done[j] = true;
                                break;
                            }

                        }
                    }
                }
                if (done_this_iter == 0)
                {
                    // find two unassigned gprs/xmms we can swap, which
                    //  may not necessarily put them in the correct place
                    bool shift_found = false;

                    // try with gprs first
                    int a = -1;
                    int b = -1;
                    for (int i = 0; i < pcount; i++)
                    {
                        if (done[i] == false && from_locs[i].mreg != null &&
                            from_locs[i].mreg.type == rt_gpr)
                        {
                            if (a == -1)
                                a = i;
                            else
                            {
                                b = i;
                                shift_found = true;
                            }
                        }
                    }

                    if (shift_found)
                    {
                        handle_swap(from_locs[a].mreg, from_locs[b].mreg, r, n, c);

                        var tmp = from_locs[a];
                        from_locs[a] = from_locs[b];
                        from_locs[b] = from_locs[a];
                    }
                    else
                    {
                        a = -1;
                        b = -1;
                        for (int i = 0; i < pcount; i++)
                        {
                            if (done[i] == false && from_locs[i].mreg != null &&
                                from_locs[i].mreg.type == rt_float)
                            {
                                if (a == -1)
                                    a = i;
                                else
                                {
                                    b = i;
                                    shift_found = true;
                                }
                            }
                        }

                        if (shift_found)
                        {
                            handle_swap(from_locs[a].mreg, from_locs[b].mreg, r, n, c);

                            var tmp = from_locs[a];
                            from_locs[a] = from_locs[b];
                            from_locs[b] = from_locs[a];
                        }
                    }
                    if (!shift_found)
                        throw new NotImplementedException();
                }
            }

            // Do the call
            if (is_calli)
            {
                // Thumb mode requires LSB set to 1
                r.Add(inst(n, arm_orr_imm, Rd: calli_reg, Rn: calli_reg, imm: 1));
                r.Add(inst(n, arm_blx, Rm: calli_reg));
            }
            else
            {
                r.Add(inst(n, arm_bl, Rm: new ir.Param { t = ir.Opcode.vl_call_target, str = target }));
            }

            // Restore stack
            if (push_length != 0)
            {
                r.Add(inst(n, arm_add_sp_imm, imm: push_length));
            }

            // Get vt return value
            if (rct == ir.Opcode.ct_vt && !act_dest.Equals(dest))
            {
                handle_move(dest, act_dest, r, n, c);
            }

            // Restore saved registers
            for (int i = push_list.Count - 1; i >= 0; i--)
                handle_pop(push_list[i], ref x, r, n, c);

            // Get other return value
            if (rt != null && rct != ir.Opcode.ct_vt && rct != ir.Opcode.ct_unknown)
            {
                var rt_size = c.t.GetSize(rt);
                if (rct == ir.Opcode.ct_float)
                {
                    handle_move(dest, r_s0, r, n, c);
                }
                else if (rt_size <= 4)
                {
                    handle_move(dest, r_r0, r, n, c);
                }
                else if (rt_size == 8)
                {
                    throw new NotImplementedException();

                    /*
                    if (t.psize == 4)
                    {
                        var drd = dest as DoubleReg;
                        r.Add(inst(x86_mov_rm32_r32, drd.a, r_eax, n));
                        r.Add(inst(x86_mov_rm32_r32, drd.b, r_edx, n));
                    }
                    else
                    {
                        r.Add(inst(x86_mov_rm64_r64, dest, r_eax, n));
                    } */
                }
                else
                    throw new NotImplementedException();
            }

            return r;
        }

        private static void handle_pop(Reg reg, ref int push_length, List<MCInst> r, CilNode.IRNode n, Code c)
        {
            switch (reg.type)
            {
                case rt_gpr:
                    r.Add(inst(n, arm_pop, register_list: new Param[] { reg }));
                    push_length += c.t.psize;

                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private static void handle_swap(Reg to_i, Reg to_j, List<MCInst> r, CilNode.IRNode n, Code c)
        {
            throw new NotImplementedException();
        }

        private static void handle_const(Reg to_reg, long v, List<MCInst> r, CilNode.IRNode n, Code c)
        {
            if (to_reg.type != rt_gpr)
                throw new NotImplementedException();

            r.Add(inst(n, arm_mov_imm, Rd: to_reg, imm: (int)(v & 0xffff)));
            if(v < 0 || v > UInt16.MaxValue)
            {
                r.Add(inst(n, arm_movt_imm, Rd: to_reg, imm: (int)((v >> 16) & 0xffff)));
            }
        }

        internal static List<MCInst> handle_cctor_runonce(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_cmp(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_conv(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_div(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_enter(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            List<MCInst> r = new List<MCInst>();
            var n = nodes[start];

            var lv_size = c.lv_total_size + c.stack_total_size;

            if (ir.Opcode.GetCTFromType(c.ret_ts) == ir.Opcode.ct_vt)
            {
                throw new NotImplementedException();
            }

            /* standard ARM prologue is:
             * 
             * mov ip, sp
             * stmdb sp!, {fp, ip, lr, pc}      (push fp, ip, lr and pc)
             * sub fp, ip, #4
             */
            r.Add(inst(n, arm_mov_reg, Rd: r_ip, Rm: r_sp));
            r.Add(inst(n, arm_stmdb, Rn: r_sp, W: 1, register_list: new Param[] { r_fp, r_ip, r_lr, r_pc }));
            r.Add(inst(n, arm_sub_imm, Rd: r_fp, Rn: r_ip, imm: 4));

            /* Move incoming arguments to the appropriate locations */
            for (int i = 0; i < c.la_needs_assign.Length; i++)
            {
                if (c.la_needs_assign[i])
                {
                    var from = c.incoming_args[i];
                    var to = c.la_locs[i];

                    handle_move(to, from, r, n, c);
                }
            }

            /* Save clobbered registers */
            var regs_to_save = c.regs_used & c.t.cc_callee_preserves_map[c.ms.CallingConvention];

            var regs_set = new util.Set();
            regs_set.Union(regs_to_save);
            int y = 0;
            while (regs_set.Empty == false)
            {
                var reg = regs_set.get_first_set();
                regs_set.unset(reg);
                var cur_reg = t.regs[reg];
                handle_push(cur_reg, ref y, r, n, c);
                c.regs_saved.Add(cur_reg);
            }

            if (ir.Opcode.GetCTFromType(c.ret_ts) == ir.Opcode.ct_vt)
            {
                throw new NotImplementedException();
                //handle_move(new ContentsReg { basereg = r_ebp, disp = -t.psize, size = t.psize },
                //    t.psize == 4 ? r_eax : r_edi, r, n, c);
            }

            return r;
        }

        private static void handle_push(Reg reg, ref int push_length, List<MCInst> r, CilNode.IRNode n, Code c)
        {
            switch(reg.type)
            {
                case rt_gpr:
                    r.Add(inst(n, arm_push, register_list: new Param[] { reg }));
                    push_length += c.t.psize;

                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private static void handle_move(Reg to, Reg from, List<MCInst> r, CilNode.IRNode n, Code c)
        {
            if (to.type == rt_contents && from.type == rt_contents)
            {
                throw new NotImplementedException();
            }
            else if (to.type == rt_contents && from.type == rt_gpr)
            {
                var cto = to as ContentsReg;
                if (cto.size != c.t.psize)
                {
                    throw new NotImplementedException();
                }
                r.Add(inst(n, arm_str_imm, Rt: from, Rn: cto.basereg, imm: (int)cto.disp));
            }
            else if (to.type == rt_gpr && from.type == rt_contents)
            {
                var cfrom = from as ContentsReg;
                if (cfrom.size != c.t.psize)
                {
                    throw new NotImplementedException();
                }
                r.Add(inst(n, arm_ldr_imm, Rt: to, Rn: cfrom.basereg, imm: (int)cfrom.disp));
            }
            else if(to.type == rt_gpr && from.type == rt_gpr)
            {
                r.Add(inst(n, arm_mov_reg, Rd: to, Rm: from));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        internal static List<MCInst> handle_enter_handler(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_ldarg(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct = n.ctret;
            var src = c.la_locs[(int)n.imm_l];
            var dest = n.stack_after.Peek(n.res_a).reg;

            var r = new List<MCInst>();
            if (n_ct == ir.Opcode.ct_int32 ||
                n_ct == ir.Opcode.ct_float ||
                n_ct == ir.Opcode.ct_vt ||
                n_ct == ir.Opcode.ct_int64)
            {
                handle_move(dest, src, r, n, c);
                return r;
            }
            return null;
        }

        internal static List<MCInst> handle_ldarga(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_ldc(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            List<MCInst> r = new List<MCInst>();

            var dest = n.stack_after.Peek(n.res_a);
            handle_const(dest.reg, n.imm_l, r, n, c);
            return r;
        }

        internal static List<MCInst> handle_ldfp(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_ldind(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_ldlabaddr(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            List<MCInst> r = new List<MCInst>();

            var dest = n.stack_after.Peek(n.res_a);
            r.Add(inst(n, arm_mov_imm, Rd: dest.reg, imm: 0, str_target: n.imm_lab));
            r.Add(inst(n, arm_movt_imm, Rd: dest.reg, imm: 0, str_target: n.imm_lab));

            return r;
        }

        internal static List<MCInst> handle_ldloc(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];

            var n_ct = n.ct;
            var src = c.lv_locs[(int)n.imm_l];
            var dest = n.stack_after.Peek(n.res_a).reg;

            List<MCInst> r = new List<MCInst>();

            if (dest.type == rt_gpr)
            {
                handle_move(dest, src, r, n, c);
                return r;
            }

            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_ldloca(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_ldobja(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_localloc(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_memcpy(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_memset(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_mul(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_neg(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_not(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_nop(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_or(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_rem(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_ret(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            return handle_ret(n, c, t);
        }

        private static List<MCInst> handle_ret(CilNode.IRNode n, Code c, Target t)
        {
            List<MCInst> r = new List<MCInst>();

            /* Put a local label here so we can jmp here at will */
            int ret_lab = c.cctor_ret_tag;
            if (ret_lab == -1)
            {
                ret_lab = c.next_mclabel--;
                c.cctor_ret_tag = ret_lab;
            }
            r.Add(inst(n, Generic.g_mclabel, new ir.Param { t = ir.Opcode.vl_br_target, v = ret_lab }));

            if (n.stack_before.Count == 1)
            {
                var reg = n.stack_before.Peek().reg;

                switch (n.ct)
                {
                    case ir.Opcode.ct_int32:
                    case ir.Opcode.ct_intptr:
                    case ir.Opcode.ct_object:
                    case ir.Opcode.ct_ref:
                        handle_move(r_r0, reg, r, n, c);
                        break;

                    case ir.Opcode.ct_int64:
                        throw new NotImplementedException();
                        break;

                    case ir.Opcode.ct_vt:
                        throw new NotImplementedException();
                        /*
                        // move address to save to to eax
                        handle_move(r_eax, new ContentsReg { basereg = r_ebp, disp = -t.psize, size = t.psize },
                            r, n, c);

                        // move struct to [eax]
                        var vt_size = c.t.GetSize(c.ret_ts);
                        handle_move(new ContentsReg { basereg = r_eax, size = vt_size },
                            reg, r, n, c); */

                        break;

                    case ir.Opcode.ct_float:
                        handle_move(r_s0, reg, r, n, c);
                        break;

                    default:
                        throw new NotImplementedException(ir.Opcode.ct_names[n.ct]);
                }
            }

            // Restore used regs

            if (!n.parent.is_in_excpt_handler)
            {
                // we use values relative to rbp here in case the function
                // did a localloc which has changed the stack pointer
                for (int i = 0; i < c.regs_saved.Count; i++)
                {
                    ContentsReg cr = new ContentsReg { basereg = r_fp, disp = -c.lv_total_size - c.stack_total_size - t.psize * (i + 1), size = t.psize };
                    handle_move(c.regs_saved[i], cr, r, n, c);
                }
            }
            else
            {
                // in exception handler we have to pop the values off the stack
                // because rsp is not by default restored so we may return to the
                // wrong place

                // this works because we do not allow localloc in exception handlers
                int x = 0;
                for (int i = c.regs_saved.Count - 1; i >= 0; i--)
                    handle_pop(c.regs_saved[i], ref x, r, n, c);
            }

            if (n.parent.is_in_excpt_handler)
            {
                // in exception handler - don't restore sp
                r.Add(inst(n, arm_pop, register_list: new Param[] { r_fp, r_lr }));
            }
            else
            {
                // standard function
                r.Add(inst(n, arm_ldm, Rn: r_sp, register_list: new Param[] { r_fp, r_sp, r_lr }));
            }

            // Insert a code sequence here if this is a static constructor
            if (c.is_cctor)
            {
                /* Sequence is:
                 * 
                 * Load static field pointer
                 *      mov rcx, lab_addr
                 * 
                 * Set done flag
                 *      mov_rm8_imm8 [rcx], 2
                 *      
                 * Get return eip
                 *      mov_r32_rm32/64 rcx, [rsp]
                 *      
                 * Is this cctor was called by a direct call (i.e. [rcx-6] == 0xe8) then:
                 * 
                 * Overwrite the preceeding 5 bytes with nops
                 *      mov_rm32_imm32 [rcx - 5], 0x00401f0f ; 4 byte nop
                 *      mov_rm8_imm8 [rcx - 1], 0x90 ; 1 byte nop
                 *      
                 * Else skip to here (this is the case where we were called indirectly from
                 *  System.Runtime.CompilerServices._RunClassConstructor())
                 */

                throw new NotImplementedException();

                /*
                r.Add(inst(x86_mov_rm32_imm32, r_ecx, new ir.Param { t = ir.Opcode.vl_str, str = c.ms.type.MangleType() + "S" }, n));
                r.Add(inst(x86_mov_rm8disp_imm32, r_ecx, 0, 2, n));
                r.Add(inst(t.psize == 4 ? x86_mov_r32_rm32 : x86_mov_r64_rm64, r_ecx, new ContentsReg { basereg = r_esp }, n));
                r.Add(inst(x86_cmp_rm8_imm8, new ContentsReg { basereg = r_ecx, disp = -6, size = 1 }, 0xe8, n));
                int end_lab = c.next_mclabel--;
                r.Add(inst_jmp(x86_jcc_rel32, end_lab, ir.Opcode.cc_ne, n));
                //r.Add(inst(x86_mov_rm32disp_imm32, r_ecx, -5, 0x00401f0f, n));
                r.Add(inst(x86_mov_rm8disp_imm32, r_ecx, -1, 0x90, n));
                r.Add(inst(x86_mov_rm8disp_imm32, r_ecx, -2, 0x90, n));
                r.Add(inst(x86_mov_rm8disp_imm32, r_ecx, -3, 0x90, n));
                r.Add(inst(x86_mov_rm8disp_imm32, r_ecx, -4, 0x90, n));
                r.Add(inst(x86_mov_rm8disp_imm32, r_ecx, -5, 0x90, n));
                r.Add(inst(Generic.g_mclabel, new ir.Param { t = ir.Opcode.vl_br_target, v = end_lab }, n)); */
            }

            if (c.ms.CallingConvention == "isrec")
            {
                throw new NotImplementedException();
                // pop error code from stack
                //r.Add(inst(t.psize == 4 ? x86_add_rm32_imm8 : x86_add_rm64_imm8,
                //    r_esp, 8, n));
            }
            if (c.ms.CallingConvention == "isr" || c.ms.CallingConvention == "isrec")
            {
                throw new NotImplementedException();
                //r.Add(inst(t.psize == 4 ? x86_iret : x86_iretq, n));
            }
            else
            {
                r.Add(inst(n, arm_bx, Rm: r_lr));
            }

            return r;
        }

        internal static List<MCInst> handle_shl(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_shr(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_shr_un(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_spinlockhint(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_stackcopy(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var src = n.stack_before.Peek(n.arg_a).reg;
            var dest = n.stack_after.Peek(n.res_a).reg;

            List<MCInst> r = new List<MCInst>();
            handle_move(dest, src, r, n, c);
            return r;
        }

        internal static List<MCInst> handle_starg(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_stind(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct2 = n.ct2;
            var addr = n.stack_before.Peek(n.arg_a).reg;
            var val = n.stack_before.Peek(n.arg_b).reg;
            bool is_tls = n.imm_ul == 1UL;

            List<MCInst> r = new List<MCInst>();

            if (addr.type == rt_gpr && val.type == rt_gpr)
            {
                handle_move(new ContentsReg { basereg = addr, size = c.t.psize }, val, r, n, c);
                return r;
            }

            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_stloc(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];

            var n_ct = n.ct;
            var src = n.stack_before.Peek(n.arg_a).reg;
            var dest = c.lv_locs[(int)n.imm_l];

            List<MCInst> r = new List<MCInst>();

            if(src.type == rt_gpr)
            {
                handle_move(dest, src, r, n, c);
                return r;
            }

            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_sub(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_switch(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_syncvalcompareandswap(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_syncvalswap(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_syncvalexchangeandadd(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_target_specific(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_xor(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_zeromem(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }
    }
}
