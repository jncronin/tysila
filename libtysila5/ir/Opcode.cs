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
using System.Text;

namespace libtysila5.ir
{
    public partial class Opcode
    {
        public int oc = oc_null;
        public int cc = cc_always;
        /* parameters */
        public Param[] uses;
        public Param[] defs;

        public int data_size = 0;

        public int il_offset;

        public bool empties_stack = false;  // leave empties the entire stack

        public int oc_idx;  // used for SSA pass

        public metadata.TypeSpec call_retval; // value returned by a call instruction
        public int call_retval_stype;

        public List<Opcode> phis = new List<Opcode>();
        public List<Opcode> post_insts = new List<Opcode>();
        public List<Opcode> pre_insts = new List<Opcode>();

        public IEnumerable<Opcode> all_insts
        {
            get
            {
                foreach (var phi in phis)
                    yield return phi;
                foreach (var pre in pre_insts)
                    yield return pre;
                yield return this;
                foreach (var post in post_insts)
                    yield return post;
            }
        }

        public IEnumerable<Param> usesdefs
        {
            get
            {
                if (uses != null)
                {
                    foreach (Param p in uses)
                        yield return p;
                }
                if (defs != null)
                {
                    foreach (Param p in defs)
                        yield return p;
                }
            }
        }

        public bool is_mc = false;

        public List<target.MCInst> mcinsts;

        public static Dictionary<int, string> oc_names;
        public static Dictionary<int, string> cc_names;
        public static Dictionary<int, int> cc_invert_map;
        public static Dictionary<int, string> ct_names;
        public static Dictionary<cil.Opcode.SingleOpcodes, int> cc_single_map;
        public static Dictionary<cil.Opcode.DoubleOpcodes,int> cc_double_map;
        public static Dictionary<int, GetDefTypeHandler> oc_pushes_map
            = new Dictionary<int, GetDefTypeHandler>(
                new GenericEqualityComparer<int>());
        public static Dictionary<int, string> vl_names
            = new Dictionary<int, string>(
                new GenericEqualityComparer<int>());

        public delegate int GetDefTypeHandler(Opcode start, target.Target t);


        static Opcode()
        {
            // Pull in mappings defined in IrOpcodes.td
            oc_names = new Dictionary<int, string>();
            cc_names = new Dictionary<int, string>();
            ct_names = new Dictionary<int, string>();
            cc_single_map = new Dictionary<cil.Opcode.SingleOpcodes, int>();
            cc_double_map = new Dictionary<cil.Opcode.DoubleOpcodes, int>();
            cc_invert_map = new Dictionary<int, int>();
            
            init_oc();
            init_cc();
            init_ct();
            init_cc_single_map();
            init_cc_double_map();
            init_cc_invert();
            init_oc_pushes_map();
            init_vl();
        }

        public bool HasSideEffects
        {
            get
            {
                switch (oc)
                {
                    case oc_call:
                        return true;

                    default:
                        return false;
                }
            }
        }
    }

    public class Param
    {
        public int t;
        public long v;
        public long v2;
        public string str;
        public target.Target.Reg mreg;
        public metadata.MetadataStream m;
        public metadata.MethodSpec ms;
        public metadata.TypeSpec ts;
        public int ct = Opcode.ct_unknown;
        public int ssa_idx = -1;

        public bool stack_abs = false;
        public bool want_address = false;

        public enum UseDefType { Unknown, Use, Def };
        public UseDefType ud = UseDefType.Unknown;

        /* These are used for constant folding */
        internal int cf_stype = 0;
        internal metadata.TypeSpec cf_type = null;
        internal long cf_intval = 0;
        internal ulong cf_uintval = 0;
        internal bool cf_hasval = false;

        public bool IsStack { get { return t == Opcode.vl_stack || t == Opcode.vl_stack32 || t == Opcode.vl_stack64; } }
        public bool IsLV { get { return t == Opcode.vl_lv || t == Opcode.vl_lv32 || t == Opcode.vl_lv64; } }
        public bool IsLA { get { return t == Opcode.vl_arg || t == Opcode.vl_arg32 || t == Opcode.vl_arg64; } }
        public bool IsMreg { get { return t == Opcode.vl_mreg; } }
        public bool IsUse { get { return ud == UseDefType.Use; } }
        public bool IsDef { get { return ud == UseDefType.Def; } }
        public bool IsConstant { get { return t == Opcode.vl_c || t == Opcode.vl_c32 || t == Opcode.vl_c64; } }

        /** <summary>Decorate the current type to include bitness</summary> */
        public int DecoratedType(target.Target tgt)
        {
            int new_ct = ct;

            if (new_ct == Opcode.ct_intptr)
                new_ct = tgt.ptype;

            switch(t)
            {
                case Opcode.vl_arg:
                    if (new_ct == Opcode.ct_int32)
                        return Opcode.vl_arg32;
                    else if (new_ct == Opcode.ct_int64)
                        return Opcode.vl_arg64;
                    return t;

                case Opcode.vl_stack:
                    if (new_ct == Opcode.ct_int32)
                        return Opcode.vl_stack32;
                    else if (new_ct == Opcode.ct_int64)
                        return Opcode.vl_stack64;
                    return t;

                case Opcode.vl_lv:
                    if (new_ct == Opcode.ct_int32)
                        return Opcode.vl_lv32;
                    else if (new_ct == Opcode.ct_int64)
                        return Opcode.vl_lv64;
                    return t;

                case Opcode.vl_c:
                    if (new_ct == Opcode.ct_int32)
                        return Opcode.vl_c32;
                    else if (new_ct == Opcode.ct_int64)
                        return Opcode.vl_c64;
                    return t;

                default:
                    return t;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            switch(t)
            {
                case Opcode.vl_c:
                case Opcode.vl_c32:
                case Opcode.vl_c64:
                    sb.Append("$" + v.ToString());
                    break;
                case Opcode.vl_arg:
                case Opcode.vl_arg32:
                case Opcode.vl_arg64:
                    sb.Append("la" + v.ToString());
                    break;
                case Opcode.vl_lv:
                case Opcode.vl_lv32:
                case Opcode.vl_lv64:
                    sb.Append("lv" + v.ToString());
                    break;
                case Opcode.vl_stack:
                case Opcode.vl_stack32:
                case Opcode.vl_stack64:
                    if (ssa_idx != -1)
                        sb.Append("vreg" + ssa_idx.ToString());
                    else
                        sb.Append("st" + v.ToString());
                    break;
                case Opcode.vl_call_target:
                    sb.Append("callsite(");
                    if (str != null)
                        sb.Append(str);
                    else if(ms != null)
                    {
                        sb.Append(ms.mdrow.ToString());
                        sb.Append(" [");
                        sb.Append(ms.msig.ToString());
                        sb.Append("]");
                    }
                    else
                    {
                        sb.Append(v.ToString());
                        sb.Append(" [");
                        sb.Append(v2.ToString());
                        sb.Append("]");
                    }
                    sb.Append(")");
                    break;
                case Opcode.vl_cc:
                    sb.Append(Opcode.cc_names[(int)v]);
                    break;
                case Opcode.vl_str:
                    sb.Append(str);
                    if (v > 0)
                    {
                        sb.Append("+");
                        sb.Append(v.ToString());
                    }
                    else if (v < 0)
                    {
                        sb.Append("-");
                        sb.Append(v.ToString());
                    }

                    break;
                case Opcode.vl_br_target:
                    sb.Append("IL" + v.ToString("X4"));
                    break;
                case Opcode.vl_mreg:
                    sb.Append("%" + mreg.ToString());
                    break;
                case Opcode.vl_ts_token:
                    sb.Append("TypeSpec: ");
                    sb.Append(ts.m.MangleType(ts));
                    break;
                default:
                    return "{null}";
            }
            
            switch(ct)
            {
                case Opcode.ct_int32:
                case Opcode.ct_int64:
                case Opcode.ct_intptr:
                case Opcode.ct_object:
                case Opcode.ct_ref:
                case Opcode.ct_float:
                    sb.Append(": ");
                    sb.Append(Opcode.ct_names[ct]);
                    break;
            }

            if (ud == UseDefType.Use)
                sb.Append(" (use) ");
            else if (ud == UseDefType.Def)
                sb.Append(" (def) ");

            sb.Append("}");
            return sb.ToString();
        }

        public static implicit operator Param(long v)
        {
            return new Param { t = Opcode.vl_c, v = v };
        }

        public static implicit operator Param(string v)
        {
            return new Param { t = Opcode.vl_str, str = v };
        }
    }
}
