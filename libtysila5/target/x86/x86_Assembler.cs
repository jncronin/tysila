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

namespace libtysila5.target.x86
{
    partial class x86_Assembler : Target
    {
        static x86_Assembler()
        {
            init_instrs();
        }

        bool has_cpu_feature(string name)
        {
            // get cpu family, model, stepping
            int family = 0;
#if HAVE_SYSTEM
            var lvls = Environment.GetEnvironmentVariable("PROCESSOR_LEVEL");
            if(lvls != null)
            {
                int.TryParse(lvls, out family);
            }
#endif

            int model = 0;
            int stepping = 0;

#if HAVE_SYSTEM
            var rev = Environment.GetEnvironmentVariable("PROCESSOR_REVISION");
            if(rev != null)
            {
                if(rev.Length == 4)
                {
                    string mods = rev.Substring(0, 2);
                    string steps = rev.Substring(2, 2);

                    int.TryParse(mods, System.Globalization.NumberStyles.HexNumber, null, out model);
                    int.TryParse(steps, System.Globalization.NumberStyles.HexNumber, null, out stepping);
                }
            }
#endif

            bool is_amd64 = false;
#if HAVE_SYSTEM
            var parch = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            if (parch != null && parch == "AMD64")
                is_amd64 = true;
#endif

            if(name == "sse2")
            {
                if (is_amd64)
                    return true;
                if (family == 6 || family == 15)
                    return true;
                return false;
            }

            if(name == "sse4_1")
            {
                if (family == 6 && model >= 6)
                    return true;
                return false;
            }

            throw new NotImplementedException();
        }

        void init_cpu_feature_option(string s)
        {
            Options.InternalAdd(s, has_cpu_feature(s));
        }

        protected void init_options()
        {
            // cpu features
            init_cpu_feature_option("sse4_1");

            Options.InternalAdd("mcmodel", "small");
        }

        public override void InitIntcalls()
        {
            ConvertToIR.intcalls["_ZN14libsupcs#2Edll8libsupcs12IoOperations_7PortOut_Rv_P2th"] = portout_byte;
            ConvertToIR.intcalls["_ZN14libsupcs#2Edll8libsupcs12IoOperations_7PortOut_Rv_P2tt"] = portout_word;
            ConvertToIR.intcalls["_ZN14libsupcs#2Edll8libsupcs12IoOperations_7PortOut_Rv_P2tj"] = portout_dword;
            ConvertToIR.intcalls["_ZN14libsupcs#2Edll8libsupcs12IoOperations_7PortInb_Rh_P1t"] = portin_byte;
            ConvertToIR.intcalls["_ZN14libsupcs#2Edll8libsupcs12IoOperations_7PortInw_Rt_P1t"] = portin_word;
            ConvertToIR.intcalls["_ZN14libsupcs#2Edll8libsupcs12IoOperations_7PortInd_Rj_P1t"] = portin_dword;
            ConvertToIR.intcalls["_ZW6System4Math_5Round_Rd_P1d"] = math_Round;
            ConvertToIR.intcalls["_ZN14libsupcs#2Edll8libsupcs15OtherOperations_27EnterUninterruptibleSection_Ru1I_P0"] = enter_cli;
            ConvertToIR.intcalls["_ZN14libsupcs#2Edll8libsupcs15OtherOperations_26ExitUninterruptibleSection_Rv_P1u1I"] = exit_cli;
        }

        private static util.Stack<StackItem> enter_cli(cil.CilNode n, Code c, util.Stack<StackItem> stack_before)
        {
            var stack_after = new util.Stack<StackItem>(stack_before);

            stack_after.Push(new StackItem { ts = c.ms.m.SystemIntPtr });
            n.irnodes.Add(new cil.CilNode.IRNode { parent = n, opcode = Opcode.oc_target_specific, imm_l = x86_enter_cli, stack_before = stack_before, stack_after = stack_after });

            return stack_after;
        }

        private static util.Stack<StackItem> exit_cli(cil.CilNode n, Code c, util.Stack<StackItem> stack_before)
        {
            var stack_after = new util.Stack<StackItem>(stack_before);

            stack_after.Pop();
            n.irnodes.Add(new cil.CilNode.IRNode { parent = n, opcode = Opcode.oc_target_specific, imm_l = x86_exit_cli, stack_before = stack_before, stack_after = stack_after });

            return stack_after;
        }

        private static util.Stack<StackItem> math_Round(cil.CilNode n, Code c, util.Stack<StackItem> stack_before)
        {
            var stack_after = new util.Stack<StackItem>(stack_before);

            var old = stack_after.Pop();

            // propagate immediate value if there is one
            double min_imm = Math.Round(BitConverter.ToDouble(BitConverter.GetBytes(old.min_ul), 0));
            double max_imm = Math.Round(BitConverter.ToDouble(BitConverter.GetBytes(old.max_ul), 0));

            stack_after.Push(new StackItem { ts = c.ms.m.GetSimpleTypeSpec(0xd), min_ul = BitConverter.ToUInt64(BitConverter.GetBytes(min_imm), 0), max_ul = BitConverter.ToUInt64(BitConverter.GetBytes(max_imm), 0) });

            n.irnodes.Add(new cil.CilNode.IRNode { parent = n, opcode = ir.Opcode.oc_target_specific, imm_l = x86_roundsd_xmm_xmmm64_imm8, stack_before = stack_before, stack_after = stack_after, arg_a = 0, res_a = 0 });

            return stack_after;
        }

        private static util.Stack<StackItem> portin_byte(cil.CilNode n, Code c, util.Stack<StackItem> stack_before)
        {
            var stack_after = new util.Stack<StackItem>(stack_before);

            stack_after.Pop();
            stack_after.Push(new StackItem { ts = c.ms.m.SystemByte, min_ul = 0, max_ul = 255 });

            n.irnodes.Add(new cil.CilNode.IRNode { parent = n, opcode = ir.Opcode.oc_x86_portin, vt_size = 1, stack_before = stack_before, stack_after = stack_after, arg_a = 0, res_a = 0 });

            return stack_after;
        }

        private static util.Stack<StackItem> portin_word(cil.CilNode n, Code c, util.Stack<StackItem> stack_before)
        {
            var stack_after = new util.Stack<StackItem>(stack_before);

            stack_after.Pop();
            stack_after.Push(new StackItem { ts = c.ms.m.SystemUInt16, min_ul = 0, max_ul = ushort.MaxValue });

            n.irnodes.Add(new cil.CilNode.IRNode { parent = n, opcode = ir.Opcode.oc_x86_portin, vt_size = 2, stack_before = stack_before, stack_after = stack_after, arg_a = 0, res_a = 0 });

            return stack_after;
        }

        private static util.Stack<StackItem> portin_dword(cil.CilNode n, Code c, util.Stack<StackItem> stack_before)
        {
            var stack_after = new util.Stack<StackItem>(stack_before);

            stack_after.Pop();
            stack_after.Push(new StackItem { ts = c.ms.m.SystemUInt32, min_ul = 0, max_ul = uint.MaxValue });

            n.irnodes.Add(new cil.CilNode.IRNode { parent = n, opcode = ir.Opcode.oc_x86_portin, vt_size = 4, stack_before = stack_before, stack_after = stack_after, arg_a = 0, res_a = 0 });

            return stack_after;
        }

        private static util.Stack<StackItem> portout_byte(cil.CilNode n, Code c, util.Stack<StackItem> stack_before)
        {
            var stack_after = new util.Stack<StackItem>(stack_before);

            stack_after.Pop();
            stack_after.Pop();

            n.irnodes.Add(new cil.CilNode.IRNode { parent = n, opcode = ir.Opcode.oc_x86_portout, vt_size = 1, stack_before = stack_before, stack_after = stack_after, arg_a = 1, arg_b = 0 });

            return stack_after;
        }

        private static util.Stack<StackItem> portout_word(cil.CilNode n, Code c, util.Stack<StackItem> stack_before)
        {
            var stack_after = new util.Stack<StackItem>(stack_before);

            stack_after.Pop();
            stack_after.Pop();

            n.irnodes.Add(new cil.CilNode.IRNode { parent = n, opcode = ir.Opcode.oc_x86_portout, vt_size = 2, stack_before = stack_before, stack_after = stack_after, arg_a = 1, arg_b = 0 });

            return stack_after;
        }

        private static util.Stack<StackItem> portout_dword(cil.CilNode n, Code c, util.Stack<StackItem> stack_before)
        {
            var stack_after = new util.Stack<StackItem>(stack_before);

            stack_after.Pop();
            stack_after.Pop();

            n.irnodes.Add(new cil.CilNode.IRNode { parent = n, opcode = ir.Opcode.oc_x86_portout, vt_size = 4, stack_before = stack_before, stack_after = stack_after, arg_a = 1, arg_b = 0 });

            return stack_after;
        }

        protected internal override Reg GetMoveDest(MCInst i)
        {
            return i.p[1].mreg;
        }

        protected internal override Reg GetMoveSrc(MCInst i)
        {
            return i.p[2].mreg;
        }

        protected internal override int GetCondCode(MCInst i)
        {
            if (i.p == null || i.p.Length < 2 ||
                i.p[1].t != Opcode.vl_cc)
                return Opcode.cc_always;
            return (int)i.p[1].v;
        }

        protected internal override bool IsBranch(MCInst i)
        {
            if (i.p != null && i.p.Length > 0 &&
                i.p[0].t == Opcode.vl_str &&
                (i.p[0].v == x86_jmp_rel32 ||
                i.p[0].v == x86_jcc_rel32))
                return true;
            return false;
        }

        protected internal override bool IsCall(MCInst i)
        {
            if (i.p != null && i.p.Length > 0 &&
                i.p[0].t == Opcode.vl_str &&
                (i.p[0].v == x86_call_rel32))
                return true;
            return false;
        }


        protected internal override void SetBranchDest(MCInst i, int d)
        {
            if (!IsBranch(i))
                throw new NotSupportedException();
            if (i.p[0].v == x86_jcc_rel32)
                i.p[2] = new Param { t = Opcode.vl_br_target, v = d };
            else
                i.p[1] = new Param { t = Opcode.vl_br_target, v = d };
        }

        protected internal override int GetBranchDest(MCInst i)
        {
            if (!IsBranch(i))
                throw new NotSupportedException();
            if (i.p[0].v == x86_jcc_rel32)
                return (int)i.p[2].v;
            else
                return (int)i.p[1].v;
        }

        protected internal override MCInst SaveRegister(Reg r)
        {
            MCInst ret = new MCInst
            {
                p = new Param[]
                {
                    new Param { t = Opcode.vl_str, str = "push", v = x86_push_r32 },
                    new Param { t = Opcode.vl_mreg, mreg = r }
                }
            };
            return ret;
        }

        protected internal override MCInst RestoreRegister(Reg r)
        {
            MCInst ret = new MCInst
            {
                p = new Param[]
                {
                    new Param { t = Opcode.vl_str, str = "pop", v = x86_pop_r32 },
                    new Param { t = Opcode.vl_mreg, mreg = r }
                }
            };
            return ret;
        }

        /* Access to variables on the incoming stack is encoded as -address - 1 */
        protected internal override Reg GetLVLocation(int lv_loc, int lv_size, Code c)
        {
            if (Opcode.GetCTFromType(c.ret_ts) == Opcode.ct_vt)
                lv_loc += psize;

            int disp = 0;
            disp = -lv_size - lv_loc;
            return new ContentsReg
            {
                basereg = r_ebp,
                disp = disp,
                size = lv_size
            };
        }

        /* Access to variables on the incoming stack is encoded as -address - 1 */
        protected internal override Reg GetLALocation(int la_loc, int la_size, Code c)
        {
            if (Opcode.GetCTFromType(c.ret_ts) == Opcode.ct_vt)
                la_loc += psize;

            return new ContentsReg
            {
                basereg = r_ebp,
                disp = la_loc + 2 * psize,
                size = la_size
            };
        }

        protected internal override MCInst[] CreateMove(Reg src, Reg dest)
        {
            throw new NotImplementedException();
        }

        protected internal override MCInst[] SetupStack(int lv_size)
        {
            if (lv_size == 0)
                return new MCInst[0];
            else
                return new MCInst[]
                {
                    new MCInst { p = new Param[]
                    {
                        new Param { t = Opcode.vl_str, str = "sub", v = x86_sub_rm32_imm32 },
                        new Param { t = Opcode.vl_mreg, mreg = r_esp },
                        new Param { t = Opcode.vl_mreg, mreg = r_esp },
                        new Param { t = Opcode.vl_c, v = lv_size }
                    } }
                };
        }

        protected internal override IRelocationType GetDataToDataReloc()
        {
            return new binary_library.elf.ElfFile.Rel_386_32();
        }

        protected internal override IRelocationType GetDataToCodeReloc()
        {
            return new binary_library.elf.ElfFile.Rel_386_32();
        }

        public override Reg AllocateStackLocation(Code c, int size, ref int cur_stack)
        {
            size = util.util.align(size, psize);
            cur_stack -= size;

            if (-cur_stack > c.stack_total_size)
                c.stack_total_size = -cur_stack;

            return new ContentsReg { basereg = r_ebp, disp = cur_stack - c.lv_total_size, size = size };
        }

        protected internal override int GetCTFromTypeForCC(TypeSpec t)
        {
            if (t.Equals(t.m.SystemRuntimeTypeHandle) || t.Equals(t.m.SystemRuntimeMethodHandle) ||
                t.Equals(t.m.SystemRuntimeFieldHandle))
                return ir.Opcode.ct_int32;
            return base.GetCTFromTypeForCC(t);
        }

        protected internal override bool NeedsBoxRetType(MethodSpec ms)
        {
            if (Opcode.GetCTFromType(ms.ReturnType) == Opcode.ct_vt)
                return true;
            else
                return false;
        }

        protected internal override Code AssembleBoxRetTypeMethod(MethodSpec ms)
        {
            // Move all parameters up one - this will require knowledge of the param locs
            // if the return type is an object (to get the 'from' locations) and
            // knowledge of the param locs in the target method (the 'to' locations)
            // Allocate space on heap (boxed return object)

            var cc = cc_map[ms.CallingConvention];
            var cc_class_map = cc_classmap[ms.CallingConvention];
            int stack_loc = 0;
            var from_locs = GetRegLocs(new ir.Param
            {
                m = ms.m,
                ms = ms,
            }, ref stack_loc, cc, cc_class_map,
            ms.CallingConvention,
            out var from_sizes, out var from_types,
            null);
            stack_loc = 0;
            var to_locs = GetRegLocs(new ir.Param
            {
                m = ms.m,
                ms = ms,
            }, ref stack_loc, cc, cc_class_map,
            ms.CallingConvention,
            out var to_sizes, out var to_types,
            ms.m.SystemIntPtr);

            // Generate code
            var c = new Code();
            c.mc = new List<MCInst>();
            var n = new cil.CilNode(ms, 0);
            var ir = new cil.CilNode.IRNode { parent = n, mc = c.mc };
            ir.opcode = Opcode.oc_nop;
            n.irnodes.Add(ir);
            c.starts = new List<cil.CilNode>();
            c.starts.Add(n);
            c.ms = ms;
            c.t = this;

            // copy from[0:n] to to[1:n+1] in reverse order so
            //  we don't overwrite the previous registers
            ulong defined = 0;
            for(int i = from_locs.Length - 1; i >= 0 ; i--)
            {
                handle_move(to_locs[i + 1], from_locs[i], c.mc, ir, c, x86_64.x86_64_Assembler.r_rax, from_sizes[i]);
                defined |= to_locs[i + 1].mask;
            }

            // call gcmalloc with the size of the boxed version of the return
            //  type
            var ts = ms.ReturnType;
            var tsize = GetSize(ts);
            var sysobj_size = layout.Layout.GetTypeSize(c.ms.m.SystemObject, c.t);
            var boxed_size = util.util.align(sysobj_size + tsize, psize);

            // decide on the registers we need to save around gcmalloc
            var caller_preserves = cc_caller_preserves_map[ms.CallingConvention];
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
            int push_length = 0;
            foreach(var r in push_list)
            {
                handle_push(r, ref push_length, c.mc, ir, c);
            }
            c.mc.Add(inst(x86_call_rel32, new Param { t = Opcode.vl_call_target, str = "gcmalloc" },
                new Param { t = Opcode.vl_c32, v = boxed_size }, ir));
            for (int i = push_list.Count - 1; i >= 0; i--)
                handle_pop(push_list[i], ref push_length, c.mc, ir, c);

            // put vtable pointer into gcmalloc result
            c.mc.Add(inst(x86_mov_rm32_lab, new ContentsReg { basereg = r_eax, size = 4 },
                new Param { t = Opcode.vl_str, str = ts.MangleType() }, ir));

            // put rax into to[0]
            handle_move(to_locs[0], r_eax, c.mc, ir, c);

            // unbox to[0] (i.e. increment pointer so it points to the inner data)
            c.mc.Add(inst(psize == 4 ? x86_add_rm32_imm8 : x86_add_rm64_imm8, to_locs[0], sysobj_size, ir));

            // call the actual function (see AssembleBoxedMethod below)
            var unboxed = ms.Unbox;
            var act_meth = unboxed.MangleMethod();
            r.MethodRequestor.Request(unboxed);

            // Save rax around the call and return it
            // We do this because the actual method returns the address of a value type in rax
            //  and we want to return the address of the boxed object instead
            c.mc.Add(inst(x86_push_r32, r_eax, ir));
            c.mc.Add(inst(x86_call_rel32, new Param { t = Opcode.vl_str, str = act_meth }, ir));
            c.mc.Add(inst(x86_pop_r32, r_eax, ir));
            c.mc.Add(inst(x86_ret, ir));

            return c;
        }

        protected internal override Code AssembleBoxedMethod(MethodSpec ms)
        {
            /* To unbox, we simply add the size of system.object to 
             * first argument, then jmp to the actual method
             */

            var c = new Code();
            c.mc = new List<MCInst>();

            var this_reg = psize == 4 ? new ContentsReg { basereg = r_ebp, disp = 8, size = 4 } : x86_64.x86_64_Assembler.r_rdi;
            var sysobjsize = layout.Layout.GetTypeSize(ms.m.SystemObject, this);
            c.mc.Add(inst(psize == 4 ? x86_add_rm32_imm8 : x86_add_rm64_imm8, this_reg, sysobjsize, null));

            var unboxed = ms.Unbox;
            var act_meth = unboxed.MangleMethod();
            r.MethodRequestor.Request(unboxed);


            c.mc.Add(inst(x86_jmp_rel32, new Param { t = Opcode.vl_str, str = act_meth }, null));

            return c;
        }
    }
}

namespace libtysila5.target.x86_64
{
    partial class x86_64_Assembler : x86.x86_Assembler
    {
        protected internal override IRelocationType GetDataToCodeReloc()
        {
            return new binary_library.elf.ElfFile.Rel_x86_64_64();
        }

        protected internal override IRelocationType GetDataToDataReloc()
        {
            return new binary_library.elf.ElfFile.Rel_x86_64_64();
        }

        public override int GetCCClassFromCT(int ct, int size, TypeSpec ts, string cc)
        {
            if (cc == "sysv" | cc == "default")
            {
                switch (ct)
                {
                    case Opcode.ct_vt:
                        // breaks spec - need to only use MEMORY for those more than 32 bytes
                        //  but we don't wupport splitting arguments up yet
                        return sysvc_MEMORY;
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            return base.GetCCClassFromCT(ct, size, ts, cc);
        }

        /* runtime handles are void* pointers - we use the following to ensure they are passed in
         * registers rather than on the stack */
        protected internal override int GetCTFromTypeForCC(TypeSpec t)
        {
            if (t.Equals(t.m.SystemRuntimeTypeHandle) || t.Equals(t.m.SystemRuntimeMethodHandle) ||
                t.Equals(t.m.SystemRuntimeFieldHandle))
                return ir.Opcode.ct_int64;
            return base.GetCTFromTypeForCC(t);
        }

        protected override Reg GetRegLoc(Param csite, ref int stack_loc, int cc_next, int ct, TypeSpec ts, string cc)
        {
            if(cc == "isr")
            {
                /* ISR with error codes have stack as:
                 * 
                 * cur_rax          - pushed at start of isr so we have a scratch reg [rbp-8]
                 *      this is exchanged with the regs pointer as part of the init phase
                 * old_rbp          - pushed at start as part of push ebp; mov ebp, esp sequence [rbp]
                 * ret_rip          - [rbp + 8]
                 * ret_cs           - [rbp + 16]
                 * rflags           - [rbp + 24]
                 * ret_rsp          - [rbp + 32]
                 * ret_ss           - [rbp + 40]
                 */

                switch (cc_next)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        return new ContentsReg { basereg = r_ebp, disp = 8 + 8 * cc_next, size = 8 };
                    case 5:
                        return new ContentsReg { basereg = r_ebp, disp = -8, size = 8 };
                }
            }
            else if(cc == "isrec")
            {
                /* ISR with error codes have stack as:
                 * 
                 * cur_rax          - pushed at start of isr so we have a scratch reg [rbp-8]
                 *      this is exchanged with the regs pointer as part of the init phase
                 * old_rbp          - pushed at start as part of push ebp; mov ebp, esp sequence [rbp]
                 * error_code       - [rbp + 8]
                 * ret_rip          - [rbp + 16]
                 * ret_cs           - [rbp + 24]
                 * rflags           - [rbp + 32]
                 * ret_rsp          - [rbp + 40]
                 * ret_ss           - [rbp + 48]
                 */
                
                switch(cc_next)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        return new ContentsReg { basereg = r_ebp, disp = 8 + 8 * cc_next, size = 8 };
                    case 6:
                        return new ContentsReg { basereg = r_ebp, disp = -8, size = 8 };
                }
            }

            return base.GetRegLoc(csite, ref stack_loc, cc_next, ct, ts, cc);
        }

        internal override int GetCCStackReserve(string cc)
        {
            // isrs use stack space for the address of the regs array
            if (cc == "isr" || cc == "isrec")
                return psize;
            else
                return base.GetCCStackReserve(cc);
        }

        protected internal override void AddExtraVTableFields(TypeSpec ts, IList<byte> d, ref ulong offset)
        {
            // Unboxed versions here (all VTables refer to boxed types by definition as only these
            //  have a vtbl, however for Invoke et al we want to unbox members of the object[] array)
            if (ts.IsBoxed)
                ts = ts.Unbox;

            // We add the SysV ABI calling convention class code here
            int size;
            if (ts.IsInterface || ts.IsGenericTemplate || ts.stype == TypeSpec.SpecialType.MPtr || ts.stype == TypeSpec.SpecialType.Ptr)
            {
                size = 0;
            }
            else
            {
                size = GetSize(ts);
            }

            var cmap = cc_classmap["sysv"];
            var ct = GetCTFromTypeForCC(ts);
            int class_code;
            if (cmap.ContainsKey(ct))
                class_code = cmap[ct];
            else
                class_code = GetCCClassFromCT(ct, size, ts, "sysv");

            switch(class_code)
            {
                case sysvc_INTEGER:
                    d.Add(0);
                    break;
                case sysvc_SSE:
                    d.Add(2);
                    break;
                case sysvc_MEMORY:
                    d.Add(3);
                    break;
                default:
                    throw new NotSupportedException();
            }

            // Now add the ct type encoded as class code (see libsupcs/x86_64_invoke.cs)
            switch(ct)
            {
                case Opcode.ct_float:
                    d.Add(2);
                    break;
                case Opcode.ct_int32:
                    d.Add(4);
                    break;
                case Opcode.ct_int64:
                case Opcode.ct_intptr:
                    d.Add(1);
                    break;
                case Opcode.ct_object:
                case Opcode.ct_ref:
                    d.Add(0);
                    break;
                case Opcode.ct_vt:
                    d.Add(0);
                    break;
                default:
                    throw new NotSupportedException();
            }

            // Now the simple type
            d.Add((byte)ts.SimpleType);

            // Pad with 5 zeros
            for (int i = 0; i < 5; i++)
                d.Add(0);

            offset += 8;
        }

        protected internal override int ExtraVTableFieldsPointerLength => 1;
    }
}
