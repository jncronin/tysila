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

namespace libtysila5.cil
{
    public class CilNode
    {
        public CilNode(metadata.MethodSpec ms, int cil_offset)
        {
            m = ms.m;
            il_offset = cil_offset;
            _ms = ms;
        }

        public class IRNode
        {
            public CilNode parent;
            public List<target.MCInst> mc;

            public bool ignore_for_mcoffset = false;

            public int opcode;
            public int ct = ir.Opcode.ct_unknown;
            public int ct2 = ir.Opcode.ct_unknown;
            int _ctret = ir.Opcode.ct_unknown;
            bool has_ctret = false;
            public int ctret { get { if (has_ctret) return _ctret; else return ct; } set { _ctret = value; has_ctret = true; } }

            public int vt_size;

            public util.Stack<ir.StackItem> stack_before;
            public util.Stack<ir.StackItem> stack_after;

            public int arg_a = 0;
            public int arg_b = 1;
            public int arg_c = 2;
            public int arg_d = 3;
            public List<int> arg_list = null;
            public int res_a = 0;

            public long imm_l;
            public ulong imm_ul;
            public byte[] imm_val;
            public string imm_lab;
            public metadata.MethodSpec imm_ms;
            public metadata.TypeSpec imm_ts;

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(ir.Opcode.oc_names[opcode]);
                if(ct != ir.Opcode.ct_unknown)
                {
                    sb.Append(".");
                    sb.Append(ir.Opcode.ct_names[ct]);
                }
                if (ct2 != ir.Opcode.ct_unknown)
                {
                    sb.Append(".");
                    sb.Append(ir.Opcode.ct_names[ct2]);
                }
                if(vt_size != 0)
                {
                    sb.Append(".");
                    sb.Append(vt_size.ToString());
                }
                return sb.ToString();
            }
        }

        public List<IRNode> irnodes = new List<IRNode>();

        metadata.MetadataStream m;
        metadata.MethodSpec _ms;
        public int il_offset;
        public int mc_offset = -1;
        public bool is_block_start = false;

        public bool is_meth_start = false;
        public bool is_eh_start = false;
        public bool is_filter_start = false;

        public bool is_in_excpt_handler = false;

        public bool visited = false;
        public util.Stack<ir.StackItem> stack_after;

        public List<metadata.ExceptionHeader> try_starts = new List<metadata.ExceptionHeader>();
        public List<metadata.ExceptionHeader> handler_starts = new List<metadata.ExceptionHeader>();

        public Opcode opcode;
        public int inline_int;
        public uint inline_uint;
        public long inline_long;
        public byte[] inline_val;
        public double inline_double;
        public float inline_float;
        public List<int> inline_array;

        public List<int> il_offsets_after = new List<int>();
        public int il_offset_after;

        public List<CilNode> prev = new List<CilNode>();

        // prefixes
        public bool constrained;
        public uint constrained_tok;
        public bool no_typecheck;
        public bool no_rangecheck;
        public bool no_nullcheck;
        public bool read_only;
        public bool tail;
        public bool unaligned;
        public int unaligned_alignment;
        public bool volatile_;

        public override string ToString()
        {
            return opcode == null ? "" : opcode.ToString();
        }

        public bool HasConstant
        { get { return opcode.sop == Opcode.SimpleOpcode.ldc; } }
        public bool HasLocalVarLoc
        {
            get
            {
                if (opcode.sop != Opcode.SimpleOpcode.ldloc &&
                    opcode.sop != Opcode.SimpleOpcode.stloc)
                    return false;
                return true;
            }
        }
        public bool HasLocalArgLoc
        {
            get
            {
                if (opcode.sop != Opcode.SimpleOpcode.ldarg &&
                    opcode.sop != Opcode.SimpleOpcode.starg)
                    return false;
                return true;
            }
        }

        public int GetConstantType()
        {
            if (opcode.sop != Opcode.SimpleOpcode.ldc)
                throw new Exception("Not a constant node");
            switch (opcode.opcode1)
            {
                case Opcode.SingleOpcodes.ldc_i4_0:
                case Opcode.SingleOpcodes.ldc_i4_1:
                case Opcode.SingleOpcodes.ldc_i4_2:
                case Opcode.SingleOpcodes.ldc_i4_3:
                case Opcode.SingleOpcodes.ldc_i4_4:
                case Opcode.SingleOpcodes.ldc_i4_5:
                case Opcode.SingleOpcodes.ldc_i4_6:
                case Opcode.SingleOpcodes.ldc_i4_7:
                case Opcode.SingleOpcodes.ldc_i4_8:
                case Opcode.SingleOpcodes.ldc_i4_m1:
                case Opcode.SingleOpcodes.ldc_i4:
                case Opcode.SingleOpcodes.ldc_i4_s:
                    return ir.Opcode.ct_int32;
                case Opcode.SingleOpcodes.ldc_i8:
                    return ir.Opcode.ct_int64;
                case Opcode.SingleOpcodes.ldc_r4:
                case Opcode.SingleOpcodes.ldc_r8:
                    return ir.Opcode.ct_float;
                case Opcode.SingleOpcodes.ldnull:
                    return ir.Opcode.ct_object;
            }
            throw new NotSupportedException();
        }

        public long GetConstant()
        {
            if (opcode.sop != Opcode.SimpleOpcode.ldc)
                throw new Exception("Not a constant node");

            switch(opcode.opcode1)
            {
                case Opcode.SingleOpcodes.ldc_i4_0:
                case Opcode.SingleOpcodes.ldnull:
                    return 0;
                case Opcode.SingleOpcodes.ldc_i4_1:
                    return 1;
                case Opcode.SingleOpcodes.ldc_i4_2:
                    return 2;
                case Opcode.SingleOpcodes.ldc_i4_3:
                    return 3;
                case Opcode.SingleOpcodes.ldc_i4_4:
                    return 4;
                case Opcode.SingleOpcodes.ldc_i4_5:
                    return 5;
                case Opcode.SingleOpcodes.ldc_i4_6:
                    return 6;
                case Opcode.SingleOpcodes.ldc_i4_7:
                    return 7;
                case Opcode.SingleOpcodes.ldc_i4_8:
                    return 8;
                case Opcode.SingleOpcodes.ldc_i4_m1:
                    return -1;
                case Opcode.SingleOpcodes.ldc_i4:
                case Opcode.SingleOpcodes.ldc_i4_s:
                case Opcode.SingleOpcodes.ldc_i8:
                    return inline_long;
                case Opcode.SingleOpcodes.ldc_r4:
                case Opcode.SingleOpcodes.ldc_r8:
                    throw new NotImplementedException();
            }
            throw new NotSupportedException();
        }

        public int GetCondCode()
        {
            int ret;
            if (opcode.opcode1 == Opcode.SingleOpcodes.double_)
            {
                if (ir.Opcode.cc_double_map.TryGetValue(opcode.opcode2, out ret))
                    return ret;
                else
                    return ir.Opcode.cc_always;
            }
            else if (ir.Opcode.cc_single_map.TryGetValue(opcode.opcode1, out ret))
                return ret;
            else
                return ir.Opcode.cc_always;
        }

        public int GetLocalArgLoc()
        {
            if (opcode.sop != Opcode.SimpleOpcode.ldarg &&
                opcode.sop != Opcode.SimpleOpcode.starg)
                throw new Exception("Not a local arg node");

            switch (opcode.opcode1)
            {
                case Opcode.SingleOpcodes.ldarg_s:
                case Opcode.SingleOpcodes.ldarga_s:
                case Opcode.SingleOpcodes.starg_s:
                    return inline_int;
                case Opcode.SingleOpcodes.ldarg_0:
                    return 0;
                case Opcode.SingleOpcodes.ldarg_1:
                    return 1;
                case Opcode.SingleOpcodes.ldarg_2:
                    return 2;
                case Opcode.SingleOpcodes.ldarg_3:
                    return 3;
                default:
                    throw new NotSupportedException();
            }
        }

        public int GetLocalVarLoc()
        {
            switch(opcode.opcode1)
            {
                case Opcode.SingleOpcodes.ldloc_0:
                case Opcode.SingleOpcodes.stloc_0:
                    return 0;
                case Opcode.SingleOpcodes.ldloc_1:
                case Opcode.SingleOpcodes.stloc_1:
                    return 1;
                case Opcode.SingleOpcodes.ldloc_2:
                case Opcode.SingleOpcodes.stloc_2:
                    return 2;
                case Opcode.SingleOpcodes.ldloc_3:
                case Opcode.SingleOpcodes.stloc_3:
                    return 3;
                case Opcode.SingleOpcodes.ldloc_s:
                case Opcode.SingleOpcodes.stloc_s:
                case Opcode.SingleOpcodes.ldloca_s:
                    return inline_int;

                case Opcode.SingleOpcodes.double_:
                    switch(opcode.opcode2)
                    {
                        case Opcode.DoubleOpcodes.ldloc:
                        case Opcode.DoubleOpcodes.ldloca:
                        case Opcode.DoubleOpcodes.stloc:
                            return inline_int;
                    }
                    break;
            }
            throw new Exception("Not a local var node");
        }

        public void GetToken(out metadata.MetadataStream metadata,
            out uint token)
        {
            metadata = m;
            token = inline_uint;            
        }

        public metadata.TypeSpec GetTokenAsTypeSpec(Code c)
        {
            int table_id, row;
            m.InterpretToken(inline_uint, out table_id, out row);
            return m.GetTypeSpec(table_id, row, c.ms.gtparams, c.ms.gmparams);
        }

        public metadata.MethodSpec GetTokenAsMethodSpec(Code c)
        {
            return m.GetMethodSpec(inline_uint, c.ms.gtparams, c.ms.gmparams);
        }
    }
}
