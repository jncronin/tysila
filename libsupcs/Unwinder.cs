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

namespace libsupcs
{
    public abstract class Unwinder
    {
        public abstract UIntPtr GetInstructionPointer();
        public abstract UIntPtr GetFramePointer();
        public abstract Unwinder UnwindOne();
        public abstract Unwinder UnwindOne(libsupcs.TysosMethod cur_method);
        public abstract libsupcs.TysosMethod GetMethodInfo();
        public abstract Unwinder Init();
        public abstract bool CanContinue();
        public virtual object[] DoUnwind(UIntPtr exit_address) { return DoUnwind(this, exit_address); }

        public class UnwinderEntry
        {
            public UIntPtr ProgramCounter;
            public string Symbol;
            public UIntPtr Offset;
        }

        internal static unsafe object[] DoUnwind(Unwinder u, UIntPtr exit_address, bool get_symbols = true)
        {
            System.Collections.ArrayList ret = new System.Collections.ArrayList();
            UIntPtr pc;

            while (u.CanContinue() && ((pc = u.GetInstructionPointer()) != exit_address))
            {
                void* offset;
                string sym;
                if (get_symbols)
                {
                    sym = JitOperations.GetNameOfAddress((void*)pc, out offset);
                }
                else
                {
                    offset = (void*)pc;
                    sym = "offset_0";                    
                }
                ret.Add(new UnwinderEntry { ProgramCounter = pc, Symbol = sym, Offset = (UIntPtr)offset });
                u.UnwindOne();
            }

            return ret.ToArray();
        }

        [AlwaysCompile]
        [WeakLinkage]
        [MethodAlias("_ZW20System#2EDiagnostics10StackTrace_22GetStackFramesInternal_Rv_P4V16StackFrameHelperibU6System9Exception")]
        internal static unsafe void StackTrace_GetStackFramesInternal(void *sfh, int iSkip, bool fNeedFileInfo, Exception e)
        {
            /* We set the 'reentrant' member of the stack frame helper here to prevent InitializeSourceInfo running further and set
             * iFrameCount to zero to prevent stack traces occuring via CoreCLR */
            *(int*)((byte*)OtherOperations.GetStaticObjectAddress("_ZW20System#2EDiagnostics16StackFrameHelperS")
                + ClassOperations.GetStaticFieldOffset("_ZW20System#2EDiagnostics16StackFrameHelper", "t_reentrancy")) = 1;
            *(int*)((byte*)sfh + ClassOperations.GetFieldOffset("_ZW20System#2EDiagnostics16StackFrameHelper", "iFrameCount")) = 0;
        }

        [AlwaysCompile]
        [WeakLinkage]
        [MethodAlias("_ZW20System#2EDiagnostics6Assert_23ShowDefaultAssertDialog_Ri_P4u1Su1Su1Su1S")]
        internal static int Assert_ShowDefaultAssertDialog(string conditionString, string message, string stackTrace, string windowTitle)
        {
            OtherOperations.EnterUninterruptibleSection();

            System.Diagnostics.Debugger.Log(0, "Assert", windowTitle);
            System.Diagnostics.Debugger.Log(0, "Assert", conditionString);
            System.Diagnostics.Debugger.Log(0, "Assert", message);
            System.Diagnostics.Debugger.Log(0, "Assert", stackTrace);

            while (true) ;
        }
    }
}
