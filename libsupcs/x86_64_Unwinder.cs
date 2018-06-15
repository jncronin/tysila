/* Copyright (C) 2011 by John Cronin
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
using System.Runtime.CompilerServices;

namespace libsupcs.x86_64
{
    [ArchDependent("x86_64")]
    public class Unwinder : libsupcs.Unwinder
    {
        ulong cur_rbp;
        ulong cur_rip;

        [MethodAlias("__get_unwinder")]
        [AlwaysCompile]
        static libsupcs.Unwinder GetUnwinder()
        {
            return new Unwinder().UnwindOne();
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        private static extern libsupcs.TysosMethod ReinterpretAsMethodInfo(ulong addr);

        public Unwinder()
        {
            cur_rbp = libsupcs.x86_64.Cpu.RBP;
            UnwindOne();
        }

        public override libsupcs.Unwinder UnwindOne(TysosMethod cur_method)
        {
            if ((cur_method.TysosFlags & libsupcs.TysosMethod.TF_X86_ISREC) == libsupcs.TysosMethod.TF_X86_ISREC)
                return UnwindOneWithErrorCode();
            else
                return UnwindOne();
        }

        public override libsupcs.Unwinder Init()
        {
            cur_rbp = libsupcs.x86_64.Cpu.RBP;
            UnwindOne();
            return this;
        }

        public override UIntPtr GetInstructionPointer()
        {
            unsafe
            {
                return (UIntPtr)cur_rip;
            }
        }

        public override UIntPtr GetFramePointer()
        {
            return (UIntPtr)cur_rbp;
        }

        public override libsupcs.Unwinder UnwindOne()
        {
            unsafe
            {
                cur_rip = *(ulong*)(cur_rbp + 8);
                cur_rbp = *(ulong*)cur_rbp;
            }
            return this;
        }

        public libsupcs.Unwinder UnwindOneWithErrorCode()
        {
            unsafe
            {
                cur_rip = *(ulong*)(cur_rbp + 16);
                cur_rbp = *(ulong*)cur_rbp;
            }
            return this;
        }

        public override libsupcs.TysosMethod GetMethodInfo()
        {
            unsafe
            {
                // assume a dodgy pointer if below image
                ulong meth_ptr = *(ulong*)(cur_rbp - 8);

                if ((meth_ptr < 0x40000000) || !IsValidPtr(meth_ptr))
                    return null;
                object o = libsupcs.CastOperations.ReinterpretAsObject(meth_ptr);
                return o as libsupcs.TysosMethod;
            }
        }

        [WeakLinkage]
        private static bool IsValidPtr(ulong ptr)
        {
            return true;
        }

        public ulong GetRBP() { return cur_rbp; }

        public override bool CanContinue()
        {
            return IsValidPtr(cur_rbp);
        }
    }
}
