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
using System.Collections.Generic;
using System.Text;

namespace libtysila5.target
{
    public class ChooseInstructions
    {
        public static void DoChoosing(Code c)
        {
            c.mc = new List<MCInst>();
            var md = c.t.instrs.MaxDepth;

            /* Rationalise CT types depending on platform */
            foreach(var ir in c.ir)
            {
                ir.ct = c.t.RationaliseCT(ir.ct);
                ir.ct2 = c.t.RationaliseCT(ir.ct2);
                ir.ctret = c.t.RationaliseCT(ir.ctret);
            }

            for (int i = 0; i < c.ir.Count;)
            {
                int end_node = i + 1;

                while (end_node < c.ir.Count && c.ir[end_node].parent.is_block_start == false &&
                    end_node <= (i + md))
                    end_node++;

                int count = end_node - i;

                while(count > 0)
                {
                    var encoder = c.t.instrs.GetValue(c.ir, i, count);
                    if(encoder != null)
                    {
                        var instrs = encoder(c.t, c.ir, i, count, c);
                        if(instrs != null)
                        {
                            c.ir[i].mc = instrs;
                            c.mc.AddRange(instrs);
                            break;
                        }
                    }

                    count--;
                }
                if (count == 0)
                {
                    var irnode = c.ir[i];
                    throw new Exception("Cannot encode " + irnode.ToString());
                }

                i += count;
            }
        }
    }

    public partial class Target
    {

        internal List<MCInst> handle_external(Target t,
            List<cil.CilNode.IRNode> nodes,
            int start, int count, Code c, string func_name)
        {
            var n = nodes[start];
            var lastn = nodes[start + count - 1];

            metadata.TypeSpec[] p;

            if (n.ct != ir.Opcode.ct_unknown)
            {
                var ats = ir.Opcode.GetTypeFromCT(n.ct, c.ms.m);
                if (n.ct2 != ir.Opcode.ct_unknown)
                {
                    var bts = ir.Opcode.GetTypeFromCT(n.ct, c.ms.m);
                    p = new metadata.TypeSpec[] { ats, bts };
                }
                else
                    p = new metadata.TypeSpec[] { ats };
            }
            else
                p = new metadata.TypeSpec[] { };

            var rts = ir.Opcode.GetTypeFromCT(lastn.ctret, c.ms.m);

            /* If we are compressing lots of ir into a single call,
             *  make sure the call instuction is aware of the end stack
             *  state */
            if (n != lastn)
                n.stack_after = lastn.stack_after;

            var msig = c.special_meths.CreateMethodSignature(rts, p);
            n.imm_ms = new metadata.MethodSpec { mangle_override = func_name, m = c.special_meths, msig = msig };

            n.opcode = ir.Opcode.oc_call;

            /* Now locate the call instruction */
            var call_handler = instrs.GetValue(nodes, start, 1);
            if (call_handler == null)
                throw new Exception("Unable to locate call handler");

            var r = call_handler(t, nodes, start, 1, c);
            if (r == null)
                throw new Exception("Unable to invoke call handler to handle external call");

            return r;
        }
    }

    public delegate List<MCInst> InstructionHandler(
        Target t,
        List<cil.CilNode.IRNode> node,
        int start, int count, Code c);
}
