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

using System.Runtime.CompilerServices;

namespace libsupcs
{
    public class Monitor
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void ReliableEnter(object o, ref bool success);

        public static void Enter(object o)
        {
            bool s = false;
            ReliableEnter(o, ref s);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Exit(object o);

        [libsupcs.AlwaysCompile]
        [libsupcs.WeakLinkage]
        [libsupcs.MethodAlias("_ZW18System#2EThreading7Monitor_13ReliableEnter_Rv_P2u1ORb")]
        [MethodAlias("_ZN14libsupcs#2Edll8libsupcs7Monitor_13ReliableEnter_Rv_P2u1ORb")]
        static unsafe void ReliableEnter(byte *obj, ref bool success)
        {
            var tid = System.Threading.Thread.CurrentThread.ManagedThreadId;
            var mla = obj + ClassOperations.GetMutexLockOffset();

            while (try_acquire(mla, tid) != 1) ;

            success = true;
        }


        [libsupcs.AlwaysCompile]
        [libsupcs.MethodAlias("_ZW18System#2EThreading7Monitor_4Exit_Rv_P1u1O")]
        [MethodAlias("_ZN14libsupcs#2Edll8libsupcs7Monitor_4Exit_Rv_P1u1O")]
        static unsafe void Monitor_exit(byte *obj)
        {
            var tid = System.Threading.Thread.CurrentThread.ManagedThreadId;
            var mla = obj + ClassOperations.GetMutexLockOffset();

            release(mla, tid);
        }

        [libsupcs.AlwaysCompile]
        [libsupcs.MethodAlias("__try_acquire")]
        static unsafe int try_acquire(byte* mutex_lock_address, int cur_thread_id)
        {
            // Low byte of mutex is used as a spinlock
            // 2nd and 3rd bytes (as a ushort) are nesting level
            // 4th-7th bytes (as int) are thread id

            int ret = 0;

            libsupcs.OtherOperations.Spinlock(mutex_lock_address);

            unsafe
            {
                int* thread_id = (int*)(mutex_lock_address + 4);
                ushort* nest = (ushort*)(mutex_lock_address + 2);

                if ((*thread_id == 0) || (*thread_id == cur_thread_id))
                {
                    *thread_id = cur_thread_id;

                    ushort cur_nest = *nest;
                    cur_nest++;
                    *nest = cur_nest;
                    ret = 1;
                }
            }

            libsupcs.OtherOperations.Spinunlock(mutex_lock_address);

            return ret;
        }

        [libsupcs.AlwaysCompile]
        [libsupcs.MethodAlias("__release")]
        unsafe static void release(byte* mutex_lock_address, int cur_thread_id)
        {
            libsupcs.OtherOperations.Spinlock(mutex_lock_address);

            unsafe
            {
                int* thread_id = (int*)(mutex_lock_address + 4);
                ushort* nest = (ushort*)(mutex_lock_address + 2);

                if (*thread_id == cur_thread_id)
                {
                    ushort cur_nest = *nest;
                    cur_nest--;
                    *nest = cur_nest;

                    if (cur_nest == 0)
                        *thread_id = 0;
                }
                else
                {
                    System.Diagnostics.Debugger.Break();
                    libsupcs.OtherOperations.Spinunlock(mutex_lock_address);
                    throw new System.Threading.SynchronizationLockException("Attempt to release lock not owned by thread");
                }
            }

            libsupcs.OtherOperations.Spinunlock(mutex_lock_address);
        }

    }
}
