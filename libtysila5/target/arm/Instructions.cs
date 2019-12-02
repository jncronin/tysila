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
using libtysila5.cil;

namespace libtysila5.target.arm
{
    partial class arm_Assembler : Target
    {
        internal static List<MCInst> handle_add(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_and(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_br(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_break(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_brif(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_call(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_calli(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_cctor_runonce(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_cmp(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_conv(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_div(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_enter(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_enter_handler(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_ldarg(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_ldarga(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_ldc(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_ldfp(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_ldind(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_ldlabaddr(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_ldloc(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_ldloca(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_ldobja(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_localloc(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_memcpy(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_memset(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_mul(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_neg(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_not(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_or(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_rem(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_ret(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_shl(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_shr(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_shr_un(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_spinlockhint(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_stackcopy(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_starg(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_stind(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_stloc(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_sub(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_switch(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_syncvalcompareandswap(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_syncvalswap(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_target_specific(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_xor(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_zeromem(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            throw new NotImplementedException();
        }
    }
}
