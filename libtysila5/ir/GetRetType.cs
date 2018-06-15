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
    partial class Opcode
    {
        internal static int GetCTFromType(metadata.TypeSpec ts)
        {
            if (ts == null)
                return ct_unknown;

            switch(ts.stype)
            {
                case metadata.TypeSpec.SpecialType.None:
                    if (ts.m.is_corlib && ts.m.simple_type_idx[ts.tdrow] != -1)
                        return GetCTFromType(ts.m.simple_type_idx[ts.tdrow]);

                    if (ts.IsEnum)
                        return GetCTFromType(ts.UnderlyingType);

                    if (ts.IsValueType)
                        return ct_vt;

                    return ct_object;

                case metadata.TypeSpec.SpecialType.SzArray:
                case metadata.TypeSpec.SpecialType.Array:
                    return ct_object;

                case metadata.TypeSpec.SpecialType.MPtr:
                    return ct_ref;

                case metadata.TypeSpec.SpecialType.Ptr:
                    return ct_intptr;

                case metadata.TypeSpec.SpecialType.Boxed:
                    return ct_object;

                default:
                    throw new NotSupportedException();
            }
        }

        internal static metadata.TypeSpec GetTypeFromCT(int ct, metadata.MetadataStream m)
        {
            switch(ct)
            {
                case ct_int32:
                    return m.GetSimpleTypeSpec(0x08);
                case ct_int64:
                    return m.GetSimpleTypeSpec(0x0a);
                case ct_intptr:
                    return m.GetSimpleTypeSpec(0x18);
                case ct_object:
                    return m.GetSimpleTypeSpec(0x1c);
                case ct_float:
                    return m.GetSimpleTypeSpec(0x0d);
            }
            return null;
        }

        internal static bool IsTLSCT(int ct)
        {
            switch(ct)
            {
                case ct_tls_int32:
                case ct_tls_int64:
                case ct_tls_intptr:
                    return true;
                default:
                    return false;
            }
        }

        internal static int TLSCT(int ct)
        {
            switch(ct)
            {
                case ct_int32:
                    return ct_tls_int32;
                case ct_int64:
                    return ct_tls_int64;
                case ct_intptr:
                case ct_ref:
                    return ct_tls_intptr;
                default:
                    return ct;
            }
        }

        internal static int UnTLSCT(int ct)
        {
            switch(ct)
            {
                case ct_tls_int32:
                    return ct_int32;
                case ct_tls_int64:
                    return ct_int64;
                case ct_tls_intptr:
                    return ct_intptr;
                default:
                    return ct;
            }
        }

        internal static int GetCTFromType(int type)
        {
            switch (type)
            {
                case 0x02:
                case 0x03:
                case 0x04:
                case 0x05:
                case 0x06:
                case 0x07:
                case 0x08:
                case 0x09:
                    return ct_int32;

                case 0x0a:
                case 0x0b:
                    return ct_int64;

                case 0x0c:
                case 0x0d:
                    return ct_float;

                case 0x0e:
                    return ct_object;

                case 0x0f:
                    return ct_ref;

                case 0x11:
                    return ct_object;       // System.ValueType itself is a reference type

                case 0x12:
                case 0x14:
                case 0x16:
                    return ct_object;

                case 0x18:
                case 0x19:
                    return ct_intptr;

                case 0x1c:
                case 0x1d:
                    return ct_object;

                default:
                    throw new NotImplementedException();
            }
        }

        static int get_call_rettype(Opcode n, target.Target t)
        {
            // Determine the return type from the method signature
            var cs = n.uses[0];

            var ms = cs.ms;
            if (ms == null)
                throw new NotSupportedException();

            var msig = ms.msig;
            var rt_idx = ms.m.GetMethodDefSigRetTypeIndex(msig);

            throw new NotImplementedException();

            /*var ret_ts = ms.m.GetTypeSpec(ref rt_idx,
                n.n.g.ms.gtparams, n.n.g.ms.gmparams);

            var ct = GetCTFromType(ret_ts);

            return ct;*/
        }

        static int get_store_pushtype(Opcode n, target.Target t)
        {
            // Determine from the type of operand 1
            var o1 = n.uses[0];

            switch (o1.t)
            {
                case vl_stack32:
                case vl_arg32:
                case vl_lv32:
                case vl_stack64:
                case vl_arg64:
                case vl_lv64:
                case vl_c32:
                case vl_c64:
                case vl_stack:
                case vl_arg:
                case vl_lv:
                case vl_c:
                    return o1.ct;

                default:
                    throw new NotImplementedException();
            }
        }

        static int get_binnumop_pushtype(Opcode n, target.Target t)
        {
            var a = n.uses[0].ct;
            var b = n.uses[1].ct;

            switch(a)
            {
                case ct_int32:
                    switch(b)
                    {
                        case ct_int32:
                            return ct_int32;
                        case ct_intptr:
                            return ct_intptr;
                        case ct_ref:
                            if (n.oc == oc_add)
                                return ct_ref;
                            break;
                    }
                    break;
                case ct_int64:
                    switch(b)
                    {
                        case ct_int64:
                            return ct_int64;
                    }
                    break;
                case ct_intptr:
                    switch(b)
                    {
                        case ct_int32:
                            return ct_intptr;
                        case ct_intptr:
                            return ct_intptr;
                        case ct_ref:
                            if (n.oc == oc_add)
                                return ct_ref;
                            break;
                    }
                    break;
                case ct_float:
                    switch(b)
                    {
                        case ct_float:
                            return ct_float;
                    }
                    break;
                case ct_ref:
                    switch(b)
                    {
                        case ct_int32:
                            if (n.oc == oc_add || n.oc == oc_sub)
                                return ct_ref;
                            break;
                        case ct_intptr:
                            if (n.oc == oc_add || n.oc == oc_sub)
                                return ct_ref;
                            break;
                        case ct_ref:
                            if (n.oc == oc_sub)
                                return ct_intptr;
                            break;
                    }
                    break;
            }
            throw new NotSupportedException("Invalid opcode:" + n.ToString());
        }

        static int get_conv_pushtype(Opcode n, target.Target t)
        {
            var dt = n.uses[1].v;
            switch(dt)
            {
                case 1:
                case 2:
                case 4:
                case -1:
                case -2:
                case -4:
                    return ct_int32;
                case 8:
                case -8:
                    return ct_int64;
                case 14:
                case 18:
                    return ct_float;
            }
            throw new NotSupportedException("Invalid opcode: " + n.ToString());
        }

        static int get_object_pushtype(Opcode n, target.Target t)
        {
            return ct_intptr;
        }
    }
}
