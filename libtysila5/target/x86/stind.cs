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
        void LowerStind(Opcode irnode, ref int next_temp_reg)
        {
            irnode.is_mc = true;
            irnode.mcinsts = new List<MCInst>();

            /* Parameters are:
                0: addr
                1: offset
                2: value
            */

            var addr = irnode.uses[0];
            var offset = irnode.uses[1];
            var _value = irnode.uses[2];
            var destsize = irnode.data_size;

            // for now, fail if addr or value aren't on
            //  stack.
            // TODO: handle this - assign to temporaries
            if (!addr.IsStack || (!_value.IsStack && !_value.IsConstant))
                throw new NotImplementedException();

            int oc = 0;

            switch(destsize)
            {
                case 1:
                    if (_value.IsStack)
                        oc = x86_mov_rm8disp_r8;
                    else
                        oc = x86_mov_rm8disp_imm32;
                    break;
                case 2:
                    if (_value.IsStack)
                        oc = x86_mov_rm16disp_r16;
                    else
                        oc = x86_mov_rm16disp_imm32;
                    break;
                case 4:
                    if (_value.IsStack)
                        oc = x86_mov_rm32disp_r32;
                    else
                        oc = x86_mov_rm32disp_imm32;
                    break;
                case 8:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }

            irnode.mcinsts.Add(new MCInst
            {
                p = new Param[]
                {
                    new Param { t = Opcode.vl_str, v = oc, str = "stind" },
                    addr,
                    offset,
                    _value,
                }
            });
        }
    }
}
