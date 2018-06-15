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
    public class AllocRegs
    {
        public static void DoAllocation(Code c)
        {
            foreach(var n in c.ir)
            {
                DoAllocation(c, n.stack_before, c.t);
                DoAllocation(c, n.stack_after, c.t);
            }
        }

        internal static target.Target.Reg DoAllocation(Code c, int ct, target.Target t, ref long alloced, ref int cur_stack)
        {
            // rationalise thread-local addresses to use the same registers as normal ones
            if (ct == Opcode.ct_tls_int32)
                ct = Opcode.ct_int32;
            else if (ct == Opcode.ct_tls_int64)
                ct = Opcode.ct_int64;
            else if (ct == Opcode.ct_tls_intptr)
                ct = Opcode.ct_intptr;

            long avail = t.ct_regs[ct] & ~alloced;

            if (avail != 0)
            {
                // We have a valid allocation to use
                int idx = 0;
                while ((avail & 0x1) == 0)
                {
                    idx++;
                    avail >>= 1;
                }
                var reg = t.regs[idx];
                alloced |= (1L << idx);
                return reg;
            }
            else
            {
                return t.AllocateStackLocation(c, t.GetCTSize(ct), ref cur_stack);
            }
        }

        protected static target.Target.Reg DoAllocation(Code c, StackItem si, target.Target t, ref long alloced, ref int cur_stack)
        {
            si.reg = DoAllocation(c, si.ct, t, ref alloced, ref cur_stack);
            return si.reg;
        }

        private static void DoAllocation(Code c, Stack<StackItem> stack, target.Target t)
        {
            long alloced = 0;
            int stack_loc = 0;

            foreach(var si in stack)
            {
                if (si.ct == Opcode.ct_vt)
                {
                    si.reg = t.AllocateValueType(c, si.ts, ref alloced, ref stack_loc);
                }
                else if(si.has_address_taken)
                {
                    int size = 0;
                    if (si.ts.IsValueType)
                        size = layout.Layout.GetTypeSize(si.ts, t, false);
                    else
                        size = t.GetPointerSize();
                    si.reg = t.AllocateStackLocation(c, size, ref stack_loc);
                }
                else
                    DoAllocation(c, si, t, ref alloced, ref stack_loc);

                if (si.reg != null)
                    c.regs_used |= si.reg.mask;
            }
        }
    }
}
