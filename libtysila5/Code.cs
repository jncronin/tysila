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
using metadata;

namespace libtysila5
{
    public class Code
    {
        public metadata.MethodSpec ms;
        public List<cil.CilNode> cil;
        public List<cil.CilNode.IRNode> ir;
        public List<target.MCInst> mc;
        public int lvar_sig_tok;

        public List<cil.CilNode> starts;

        public target.Target.Reg[] lv_locs;
        public target.Target.Reg[] la_locs;
        public int[] lv_sizes;
        public int[] la_sizes;
        public int lv_total_size;
        public int stack_total_size = 0;
        public metadata.TypeSpec[] lv_types;
        public metadata.TypeSpec[] la_types;
        public target.Target.Reg[] incoming_args;
        public bool[] la_needs_assign;

        internal int cctor_ret_tag = -1;
        public bool is_cctor = false;
        public util.Set<TypeSpec> static_types_referenced = new util.Set<TypeSpec>();

        public metadata.TypeSpec ret_ts = null;

        public ulong regs_used = 0;
        public List<target.Target.Reg> regs_saved = new List<target.Target.Reg>();

        public int next_mclabel = -1;

        public target.Target t;

        public List<int> offset_order = new List<int>();
        public Dictionary<int, cil.CilNode> offset_map =
                new Dictionary<int, cil.CilNode>(new libtysila5.GenericEqualityComparer<int>());

        static ir.SpecialMethods _special = null;
        internal List<ExceptionHeader> ehdrs;

        public ir.SpecialMethods special_meths
        {
            get
            {
                if (_special == null)
                    _special = new libtysila5.ir.SpecialMethods(ms.m);
                return _special;
            }
        }

        public List<Label> extra_labels = new List<Label>();

        public class Label
        {
            public int Offset;
            public string Name;
        }
    }
}
