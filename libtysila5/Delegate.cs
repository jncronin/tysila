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
using libtysila5.target;
using libtysila5.util;
using metadata;
using libtysila5.ir;

namespace libtysila5.ir
{
    public partial class ConvertToIR
    {
        static MethodSpec delegate_m_target;
        static MethodSpec delegate_method_ptr;
        
        public static bool CreateDelegate(metadata.TypeSpec ts,
            target.Target t)
        {
            if(delegate_m_target == null)
            {
                delegate_m_target = ts.m.GetFieldDefRow("m_target", ts.m.SystemDelegate);
                if (delegate_m_target == null)
                    delegate_m_target = ts.m.GetFieldDefRow("_target", ts.m.SystemDelegate);
                delegate_method_ptr = ts.m.GetFieldDefRow("method_ptr", ts.m.SystemDelegate);
                if (delegate_method_ptr == null)
                    delegate_method_ptr = ts.m.GetFieldDefRow("_methodPtr", ts.m.SystemDelegate);
            }

            // Generate required delegate methods in IR
            CreateDelegateCtor(ts, t);
            CreateDelegateInvoke(ts, t);
            CreateDelegateBeginInvoke(ts, t);
            CreateDelegateEndInvoke(ts, t);


            return true;
        }

        private static void CreateDelegateEndInvoke(TypeSpec ts, Target t)
        {
            var ms = ts.m.GetMethodSpec(ts, "EndInvoke", 0, null);

            Code c = new Code { t = t, ms = ms };
            t.AllocateLocalVarsArgs(c);
            cil.CilNode n = new cil.CilNode(ms, 0);

            List<cil.CilNode.IRNode> ret = new List<cil.CilNode.IRNode>();
            util.Stack<StackItem> stack_before = new util.Stack<StackItem>();

            // Enter
            n.irnodes.Add(new cil.CilNode.IRNode { parent = n, opcode = Opcode.oc_enter, stack_before = stack_before, stack_after = stack_before });

            // Error message
            stack_before = ConvertToIR.ldstr(n, c, stack_before, "BeginInvoke is currently implemented");
            
            // Create NotImplementedException
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

            stack_before = ConvertToIR.newobj(n, c, stack_before, ni_ctor_ms);
            stack_before = ConvertToIR.throw_(n, c, stack_before);

            // Ret - not reached
            //n.irnodes.Add(new cil.CilNode.IRNode { parent = n, opcode = Opcode.oc_ret, ct = ((ret_ts == null) ? ir.Opcode.ct_unknown : Opcode.GetCTFromType(ret_ts)), stack_before = stack_before, stack_after = stack_before });

            c.cil = new List<cil.CilNode> { n };
            c.ir = n.irnodes;

            c.starts = new List<cil.CilNode> { n };

            var msc = new layout.Layout.MethodSpecWithEhdr
            {
                ms = ms,
                c = c
            };
            t.r.MethodRequestor.Remove(msc);
            t.r.MethodRequestor.Request(msc);
        }

        private static void CreateDelegateBeginInvoke(TypeSpec ts, Target t)
        {
            var ms = ts.m.GetMethodSpec(ts, "BeginInvoke", 0, null);

            Code c = new Code { t = t, ms = ms };
            t.AllocateLocalVarsArgs(c);
            cil.CilNode n = new cil.CilNode(ms, 0);

            List<cil.CilNode.IRNode> ret = new List<cil.CilNode.IRNode>();
            util.Stack<StackItem> stack_before = new util.Stack<StackItem>();

            // Enter
            n.irnodes.Add(new cil.CilNode.IRNode { parent = n, opcode = Opcode.oc_enter, stack_before = stack_before, stack_after = stack_before });

            // Error message
            stack_before = ConvertToIR.ldstr(n, c, stack_before, "BeginInvoke is currently implemented");

            // Create NotImplementedException
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

            stack_before = ConvertToIR.newobj(n, c, stack_before, ni_ctor_ms);
            stack_before = ConvertToIR.throw_(n, c, stack_before);

            // Ret - not reached
            //n.irnodes.Add(new cil.CilNode.IRNode { parent = n, opcode = Opcode.oc_ret, ct = ((ret_ts == null) ? ir.Opcode.ct_unknown : Opcode.GetCTFromType(ret_ts)), stack_before = stack_before, stack_after = stack_before });

            c.cil = new List<cil.CilNode> { n };
            c.ir = n.irnodes;

            c.starts = new List<cil.CilNode> { n };

            var msc = new layout.Layout.MethodSpecWithEhdr
            {
                ms = ms,
                c = c
            };
            t.r.MethodRequestor.Remove(msc);
            t.r.MethodRequestor.Request(msc);
        }

        private static void CreateDelegateInvoke(TypeSpec ts, Target t)
        {
            var ms = ts.m.GetMethodSpec(ts, "Invoke", 0, null);

            Code c = new Code { t = t, ms = ms };
            t.AllocateLocalVarsArgs(c);
            cil.CilNode n = new cil.CilNode(ms, 0);

            List<cil.CilNode.IRNode> ret = new List<cil.CilNode.IRNode>();
            util.Stack<StackItem> stack_before = new util.Stack<StackItem>();
            TypeSpec fld_ts;

            // Enter
            n.irnodes.Add(new cil.CilNode.IRNode { parent = n, opcode = Opcode.oc_enter, stack_before = stack_before, stack_after = stack_before });

            // Load m_target
            stack_before = ConvertToIR.ldarg(n, c, stack_before, 0);
            stack_before = ConvertToIR.ldflda(n, c, stack_before, false, out fld_ts, 0, delegate_m_target);
            stack_before = ConvertToIR.binnumop(n, c, stack_before, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);
            stack_before = ConvertToIR.ldind(n, c, stack_before, fld_ts);

            // Duplicate and branch to the static implementation if null
            stack_before = ConvertToIR.copy_to_front(n, c, stack_before);
            stack_before = ConvertToIR.ldc(n, c, stack_before, 0, (int)CorElementType.Object);
            var tstatic = c.next_mclabel--;
            stack_before = ConvertToIR.brif(n, c, stack_before, Opcode.cc_eq, tstatic);
            var tstatic_stack_in = new util.Stack<StackItem>(stack_before);

            // Get number of params and push left to right
            var sig_idx = ms.msig;
            var pcount = ms.m.GetMethodDefSigParamCount(sig_idx);
            for(int i = 0; i < pcount; i++)
                stack_before = ConvertToIR.ldarg(n, c, stack_before, i + 1);

            // Load method_ptr
            stack_before = ConvertToIR.ldarg(n, c, stack_before, 0);
            stack_before = ConvertToIR.ldflda(n, c, stack_before, false, out fld_ts, 0, delegate_method_ptr);
            stack_before = ConvertToIR.binnumop(n, c, stack_before, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);
            stack_before = ConvertToIR.ldind(n, c, stack_before, fld_ts);

            // Build new method signature containing System.Object followed by the parameters of Invoke
            sig_idx = ms.m.GetMethodDefSigRetTypeIndex(sig_idx);
            var ret_ts = ms.m.GetTypeSpec(ref sig_idx, ms.gtparams, ms.gmparams);
            List<TypeSpec> p = new List<TypeSpec>();
            p.Add(ms.m.SystemObject);
            for (int i = 0; i < pcount; i++)
                p.Add(ms.m.GetTypeSpec(ref sig_idx, ms.gtparams, ms.gmparams));
            var new_msig = c.special_meths.CreateMethodSignature(ret_ts, p.ToArray());
            c.ret_ts = ret_ts;

            // Calli
            stack_before = ConvertToIR.call(n, c, stack_before, true, "noname", c.special_meths, new_msig);

            // Ret
            n.irnodes.Add(new cil.CilNode.IRNode { parent = n, opcode = Opcode.oc_ret, ct = ((ret_ts == null) ? ir.Opcode.ct_unknown : Opcode.GetCTFromType(ret_ts)), stack_before = stack_before, stack_after = stack_before });

            // Static version of above
            stack_before = mclabel(n, c, tstatic_stack_in, tstatic);
            stack_before = ConvertToIR.pop(n, c, stack_before); // remove null m_target pointer

            // Get number of params and push left to right
            sig_idx = ms.msig;
            pcount = ms.m.GetMethodDefSigParamCount(sig_idx);
            for (int i = 0; i < pcount; i++)
                stack_before = ConvertToIR.ldarg(n, c, stack_before, i + 1);

            // Load method_ptr
            stack_before = ConvertToIR.ldarg(n, c, stack_before, 0);
            stack_before = ConvertToIR.ldflda(n, c, stack_before, false, out fld_ts, 0, delegate_method_ptr);
            stack_before = ConvertToIR.binnumop(n, c, stack_before, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr);
            stack_before = ConvertToIR.ldind(n, c, stack_before, fld_ts);

            // Build new method signature containing the parameters of Invoke
            sig_idx = ms.m.GetMethodDefSigRetTypeIndex(sig_idx);
            ret_ts = ms.m.GetTypeSpec(ref sig_idx, ms.gtparams, ms.gmparams);
            p = new List<TypeSpec>();
            for (int i = 0; i < pcount; i++)
                p.Add(ms.m.GetTypeSpec(ref sig_idx, ms.gtparams, ms.gmparams));
            new_msig = c.special_meths.CreateMethodSignature(ret_ts, p.ToArray());
            c.ret_ts = ret_ts;

            // Calli
            stack_before = ConvertToIR.call(n, c, stack_before, true, "noname", c.special_meths, new_msig);

            // Ret
            n.irnodes.Add(new cil.CilNode.IRNode { parent = n, opcode = Opcode.oc_ret, ct = ((ret_ts == null) ? ir.Opcode.ct_unknown : Opcode.GetCTFromType(ret_ts)), stack_before = stack_before, stack_after = stack_before });

            c.cil = new List<cil.CilNode> { n };
            c.ir = n.irnodes;

            c.starts = new List<cil.CilNode> { n };

            var msc = new layout.Layout.MethodSpecWithEhdr
            {
                ms = ms,
                c = c
            };
            t.r.MethodRequestor.Remove(msc);
            t.r.MethodRequestor.Request(msc);
        }

        private static void CreateDelegateCtor(TypeSpec ts, Target t)
        {
            var ms = ts.m.GetMethodSpec(ts, ".ctor", 0, null);

            Code c = new Code { t = t, ms = ms };
            t.AllocateLocalVarsArgs(c);
            cil.CilNode n = new cil.CilNode(ms, 0);

            List<cil.CilNode.IRNode> ret = new List<cil.CilNode.IRNode>();
            util.Stack<StackItem> stack_before = new util.Stack<StackItem>(0);
            TypeSpec fld_ts;

            // Enter
            n.irnodes.Add(new cil.CilNode.IRNode { parent = n, opcode = Opcode.oc_enter, stack_before = stack_before, stack_after = stack_before });

            // Store m_target
            stack_before = ConvertToIR.ldarg(n, c, stack_before, 0);
            stack_before = ConvertToIR.ldarg(n, c, stack_before, 1);
            stack_before = ConvertToIR.ldflda(n, c, stack_before, false, out fld_ts, 1, delegate_m_target);
            stack_before = ConvertToIR.binnumop(n, c, stack_before, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr, 2);
            stack_before = ConvertToIR.stind(n, c, stack_before, c.t.GetSize(fld_ts), 1, 0);
            stack_before.Pop();

            // Store method_ptr
            stack_before = ConvertToIR.ldarg(n, c, stack_before, 0);
            stack_before = ConvertToIR.ldarg(n, c, stack_before, 2);
            stack_before = ConvertToIR.ldflda(n, c, stack_before, false, out fld_ts, 1, delegate_method_ptr);
            stack_before = ConvertToIR.binnumop(n, c, stack_before, cil.Opcode.SingleOpcodes.add, Opcode.ct_intptr, 2);
            stack_before = ConvertToIR.stind(n, c, stack_before, c.t.GetSize(fld_ts), 1, 0);
            stack_before.Pop();

            // Ret
            n.irnodes.Add(new cil.CilNode.IRNode { parent = n, opcode = Opcode.oc_ret, stack_before = stack_before, stack_after = stack_before });

            c.cil = new List<cil.CilNode> { n };
            c.ir = n.irnodes;

            c.starts = new List<cil.CilNode> { n };

            var msc = new layout.Layout.MethodSpecWithEhdr
            {
                ms = ms,
                c = c
            };
            t.r.MethodRequestor.Remove(msc);
            t.r.MethodRequestor.Request(msc);
        }
    }
}
