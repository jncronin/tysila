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
using System.Runtime.CompilerServices;

namespace libsupcs
{
    unsafe class exceptions
    {
        [ThreadStatic]
        static void** start, end, cur;
        const int stack_length = 1024;

        [MethodImpl(MethodImplOptions.InternalCall)]
        [MethodReferenceAlias("push_ehdr")]
        static extern void PushEhdr(void* eh, void* fp);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [MethodReferenceAlias("pop_ehdr")]
        static extern void* PopEhdr();

        [MethodImpl(MethodImplOptions.InternalCall)]
        [MethodReferenceAlias("peek_ehdr")]
        static extern void* PeekEhdr();

        [MethodImpl(MethodImplOptions.InternalCall)]
        [MethodReferenceAlias("pop_fp")]
        static extern void* PopFramePointer();
        
        // Default versions of push/pop ehdr, not thread safe
        [AlwaysCompile]
        [WeakLinkage]
        [MethodAlias("push_ehdr")]
        static void push_ehdr(void *eh, void *fp)
        {
            if(start == null)
            {
                void** new_start = (void**)MemoryOperations.GcMalloc(stack_length * sizeof(void*));

                fixed(void ***start_addr = &start)
                {
                    if(OtherOperations.CompareExchange((void**)start_addr, (void*)new_start) == null)
                    {
                        end = start + stack_length;
                        cur = start;
                        System.Diagnostics.Debugger.Log(0, "libsupcs", "new ehdr stack at " + ((ulong)start).ToString("X") + "-" + ((ulong)end).ToString("X"));
                    }
                }
            }

            if ((cur + 1) >= end)
            {
                System.Diagnostics.Debugger.Break();
                throw new OutOfMemoryException("exception header stack overflowed");
            }

            //if(System.Threading.Thread.CurrentThread.ManagedThreadId != 1)
            //    System.Diagnostics.Debugger.Log(0, "libsupcs", "exceptions: push_ehdr: " + ((ulong)eh).ToString("X"));

            // the following should be atomic for the current stack only, so disabling interrupts is
            //  sufficient
            var state = OtherOperations.EnterUninterruptibleSection();
            *cur++ = eh;
            *cur++ = fp;
            OtherOperations.ExitUninterruptibleSection(state);
        }

        [AlwaysCompile]
        [WeakLinkage]
        [MethodAlias("pop_ehdr")]
        static void* pop_ehdr()
        {
            if (start == null || (void**)cur <= (void**)start)
            {
                System.Diagnostics.Debugger.Break();
                throw new OutOfMemoryException("exception header stack underflowed");
            }

            var state = OtherOperations.EnterUninterruptibleSection();
            var ret = *--cur;
            OtherOperations.ExitUninterruptibleSection(state);

            return ret;

            //if (System.Threading.Thread.CurrentThread.ManagedThreadId != 1)
            //    System.Diagnostics.Debugger.Log(0, "libsupcs", "exceptions: pop_ehdr: " + ((ulong)*cur).ToString("X"));
        }

        [AlwaysCompile]
        [WeakLinkage]
        [MethodAlias("peek_ehdr")]
        static void* peek_ehdr()
        {
            if (start == null || (void**)cur <= (void**)start)
            {
                return null;
            }

            var state = OtherOperations.EnterUninterruptibleSection();
            var ret = *(cur - 2);
            OtherOperations.ExitUninterruptibleSection(state);

            return ret;
        }

        [AlwaysCompile]
        [WeakLinkage]
        [MethodAlias("pop_fp")]
        static void* pop_fp()
        {
            if (start == null || cur <= start)
            {
                System.Diagnostics.Debugger.Break();
                throw new OutOfMemoryException("exception header stack underflowed");
            }

            var state = OtherOperations.EnterUninterruptibleSection();
            var ret = *--cur;
            OtherOperations.ExitUninterruptibleSection(state);

            return ret;
        }

        [AlwaysCompile]
        [WeakLinkage]
        [MethodAlias("enter_try")]
        internal static void enter_try(void *eh, void *fp)
        {
            if (eh == PeekEhdr())
            {
                System.Diagnostics.Debugger.Log(0, "libsupcs", "enter_try: recursive try block found from calling_pc: " +
                    ((ulong)OtherOperations.GetUnwinder().UnwindOne().GetInstructionPointer()).ToString("X"));
            }
            PushEhdr(eh, fp);
        }

        [AlwaysCompile]
        [WeakLinkage]
        [MethodAlias("enter_catch")]
        internal static void enter_catch(void* eh, void *fp)
        {
            PushEhdr(eh, fp);
        }

        [AlwaysCompile]
        [WeakLinkage]
        [MethodAlias("leave_try")]
        internal static void leave_try(void* eh)
        {
            void* fp = PopFramePointer();
            void* popped = PopEhdr();
            if (eh != popped)
            {
                System.Diagnostics.Debugger.Log(0, "libsupcs", "leave_try: popping incorrect exception header");
                System.Diagnostics.Debugger.Log(0, "libsupcs", "expected: " + ((ulong)eh).ToString("X"));
                System.Diagnostics.Debugger.Log(0, "libsupcs", "got: " + ((ulong)popped).ToString("X") + ", fp: " + ((ulong)fp).ToString("X"));
                System.Diagnostics.Debugger.Log(0, "libsupcs", "start: " + ((ulong)start).ToString("X"));
                System.Diagnostics.Debugger.Log(0, "libsupcs", "end: " + ((ulong)end).ToString("X"));
                System.Diagnostics.Debugger.Log(0, "libsupcs", "cur: " + ((ulong)cur).ToString("X"));
                System.Diagnostics.Debugger.Log(0, "libsupcs", "calling_pc: " + ((ulong)OtherOperations.GetUnwinder().UnwindOne().GetInstructionPointer()).ToString("X"));
                while (true) ;
            }

            void** ehdr = (void**)eh;

            int eh_type = *(int*)ehdr;
            if (eh_type == 2)
            {
                // handle finally clause
                void* handler = *(ehdr + 1);
                OtherOperations.CallI(fp, handler);
                PopFramePointer();
                PopEhdr();
            }
        }

        [AlwaysCompile]
        [WeakLinkage]
        [MethodAlias("leave_handler")]
        internal static void leave_handler(void* eh)
        {
            while (true) ;
        }

        [AlwaysCompile]
        [WeakLinkage]
        [MethodAlias("rethrow")]
        internal static void rethrow(void* eh)
        {
            while (true) ;
        }
    }
}
