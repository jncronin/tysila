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
using libtysila5.ir;

namespace libtysila5.target.x86
{
    partial class x86_Assembler
    {
        void LowerZeromem(Opcode irnode, ref int next_temp_reg)
        {
            irnode.is_mc = true;
            irnode.mcinsts = new List<MCInst>();

            /* Parameters are:
                0: addr
                1: length
            */

            var addr = irnode.uses[0];
            var len = irnode.uses[1].v;

            // for now, fail if value isn't on
            //  stack.
            // TODO: handle this - assign to temporaries
            if (!addr.IsStack)
                throw new NotImplementedException();

            // We handle up to 4x pointer size by direct stores, otherwise call
            //  to memset()

            if (len <= 16)
            {
                for (int i = 0; i < len; i += 4)
                {
                    irnode.mcinsts.Add(new MCInst
                    {
                        p = new Param[]
                        {
                            new Param { t = Opcode.vl_str, v = x86_mov_rm32disp_imm32 },
                            addr,
                            new Param { t = Opcode.vl_c32, v = i },
                            new Param { t = Opcode.vl_c32, v = 0 }
                        }
                    });
                }
            }
            else
                throw new NotImplementedException();
        }
    }
}
