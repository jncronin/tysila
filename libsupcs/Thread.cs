/* Copyright (C) 2018 by John Cronin
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

namespace libsupcs
{
    class Thread
    {
        [MethodAlias("_ZW18System#2EThreading6Thread_8SetStart_Rv_P3u1tU6System8Delegatei")]
        [WeakLinkage]
        [AlwaysCompile]
        unsafe static void SetStart(byte *thread, void *d, int max_stack)
        {
            int delegate_offset = ClassOperations.GetFieldOffset("_ZW18System#2EThreading6Thread", "m_Delegate");
            *(void**)(thread + delegate_offset) = d;
        }

        [MethodAlias("_ZW18System#2EThreading6Thread_19get_ManagedThreadId_Ri_P1u1t")]
        [WeakLinkage]
        [AlwaysCompile]
        unsafe static int get_ManagedThreadId(byte *thread)
        {
            if (thread == null)
                return 1;
            int tid_offset = ClassOperations.GetFieldOffset("_ZW18System#2EThreading6Thread", "m_ManagedThreadId");
            return *(int*)(thread + tid_offset);
        }

        /* Implement the following as a weak function so that if not overridden it returns null, and thus
         * the default get_ManagedThreadId will return 1, but also allow it to be overridden in systems
         * that support multithreading */
        [MethodAlias("_ZW18System#2EThreading6Thread_22GetCurrentThreadNative_RV6Thread_P0")]
        [WeakLinkage]
        [AlwaysCompile]
        static object GetCurrentThreadNative()
        {
            return null;
        }
    }
}
