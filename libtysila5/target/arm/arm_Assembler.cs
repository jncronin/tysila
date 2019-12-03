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

namespace libtysila5.target.arm
{
    partial class arm_Assembler : Target
    {
        static arm_Assembler()
        {
            init_instrs();
        }

        protected void init_options()
        {
            // cpu features

        }

        public override Reg AllocateStackLocation(Code c, int size, ref int cur_stack)
        {
            throw new NotImplementedException();
        }

        protected internal override bool IsCall(MCInst i)
        {
            throw new NotImplementedException();
        }

        protected internal override MCInst SaveRegister(Reg r)
        {
            throw new NotImplementedException();
        }

        protected internal override MCInst[] CreateMove(Reg src, Reg dest)
        {
            throw new NotImplementedException();
        }

        protected internal override bool NeedsBoxRetType(MethodSpec ms)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetBranchDest(MCInst i, int d)
        {
            throw new NotImplementedException();
        }

        protected internal override int GetCondCode(MCInst i)
        {
            throw new NotImplementedException();
        }

        protected internal override Reg GetLALocation(int la_loc, int la_size, Code c)
        {
            if (Opcode.GetCTFromType(c.ret_ts) == Opcode.ct_vt)
                la_loc += psize;

            return new ContentsReg
            {
                basereg = r_fp,
                disp = la_loc + 2 * psize,
                size = la_size
            };
        }

        protected internal override bool IsBranch(MCInst i)
        {
            throw new NotImplementedException();
        }

        protected internal override Reg GetMoveDest(MCInst i)
        {
            throw new NotImplementedException();
        }

        protected internal override int GetBranchDest(MCInst i)
        {
            throw new NotImplementedException();
        }

        protected internal override MCInst RestoreRegister(Reg r)
        {
            throw new NotImplementedException();
        }

        protected internal override Reg GetLVLocation(int lv_loc, int lv_size, Code c)
        {
            if (Opcode.GetCTFromType(c.ret_ts) == Opcode.ct_vt)
                lv_loc += psize;

            int disp = 0;
            disp = -lv_size - lv_loc;
            return new ContentsReg
            {
                basereg = r_fp,
                disp = disp,
                size = lv_size
            };
        }

        protected internal override MCInst[] SetupStack(int lv_size)
        {
            throw new NotImplementedException();
        }

        protected internal override IRelocationType GetDataToDataReloc()
        {
            throw new NotImplementedException();
        }

        protected internal override IRelocationType GetDataToCodeReloc()
        {
            throw new NotImplementedException();
        }

        protected internal override Code AssembleBoxRetTypeMethod(MethodSpec ms)
        {
            throw new NotImplementedException();
        }

        protected internal override Reg GetMoveSrc(MCInst i)
        {
            throw new NotImplementedException();
        }

        protected internal override Code AssembleBoxedMethod(MethodSpec ms)
        {
            throw new NotImplementedException();
        }
    }
}
