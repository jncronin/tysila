/* Copyright (C) 2008 - 2016 by John Cronin
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
    public class Opcode
    {
        public enum SingleOpcodes
        {
            nop = 0x00,
            break_ = 0x01,
            ldarg_0 = 0x02,
            ldarg_1 = 0x03,
            ldarg_2 = 0x04,
            ldarg_3 = 0x05,
            ldloc_0 = 0x06,
            ldloc_1 = 0x07,
            ldloc_2 = 0x08,
            ldloc_3 = 0x09,
            stloc_0 = 0x0A,
            stloc_1 = 0x0B,
            stloc_2 = 0x0C,
            stloc_3 = 0x0D,
            ldarg_s = 0x0E,
            ldarga_s = 0x0F,
            starg_s = 0x10,
            ldloc_s = 0x11,
            ldloca_s = 0x12,
            stloc_s = 0x13,
            ldnull = 0x14,
            ldc_i4_m1 = 0x15,
            ldc_i4_0 = 0x16,
            ldc_i4_1 = 0x17,
            ldc_i4_2 = 0x18,
            ldc_i4_3 = 0x19,
            ldc_i4_4 = 0x1A,
            ldc_i4_5 = 0x1B,
            ldc_i4_6 = 0x1C,
            ldc_i4_7 = 0x1D,
            ldc_i4_8 = 0x1E,
            ldc_i4_s = 0x1F,
            ldc_i4 = 0x20,
            ldc_i8 = 0x21,
            ldc_r4 = 0x22,
            ldc_r8 = 0x23,
            dup = 0x25,
            pop = 0x26,
            jmp = 0x27,
            call = 0x28,
            calli = 0x29,
            ret = 0x2A,
            br_s = 0x2B,
            brfalse_s = 0x2C,
            brtrue_s = 0x2D,
            beq_s = 0x2E,
            bge_s = 0x2F,
            bgt_s = 0x30,
            ble_s = 0x31,
            blt_s = 0x32,
            bne_un_s = 0x33,
            bge_un_s = 0x34,
            bgt_un_s = 0x35,
            ble_un_s = 0x36,
            blt_un_s = 0x37,
            br = 0x38,
            brfalse = 0x39,
            brtrue = 0x3A,
            beq = 0x3B,
            bge = 0x3C,
            bgt = 0x3D,
            ble = 0x3E,
            blt = 0x3F,
            bne_un = 0x40,
            bge_un = 0x41,
            bgt_un = 0x42,
            ble_un = 0x43,
            blt_un = 0x44,
            switch_ = 0x45,
            ldind_i1 = 0x46,
            ldind_u1 = 0x47,
            ldind_i2 = 0x48,
            ldind_u2 = 0x49,
            ldind_i4 = 0x4A,
            ldind_u4 = 0x4B,
            ldind_i8 = 0x4C,
            ldind_i = 0x4D,
            ldind_r4 = 0x4E,
            ldind_r8 = 0x4F,
            ldind_ref = 0x50,
            stind_ref = 0x51,
            stind_i1 = 0x52,
            stind_i2 = 0x53,
            stind_i4 = 0x54,
            stind_i8 = 0x55,
            stind_r4 = 0x56,
            stind_r8 = 0x57,
            add = 0x58,
            sub = 0x59,
            mul = 0x5A,
            div = 0x5B,
            div_un = 0x5C,
            rem = 0x5D,
            rem_un = 0x5E,
            and = 0x5F,
            or = 0x60,
            xor = 0x61,
            shl = 0x62,
            shr = 0x63,
            shr_un = 0x64,
            neg = 0x65,
            not = 0x66,
            conv_i1 = 0x67,
            conv_i2 = 0x68,
            conv_i4 = 0x69,
            conv_i8 = 0x6A,
            conv_r4 = 0x6B,
            conv_r8 = 0x6C,
            conv_u4 = 0x6D,
            conv_u8 = 0x6E,
            callvirt = 0x6F,
            cpobj = 0x70,
            ldobj = 0x71,
            ldstr = 0x72,
            newobj = 0x73,
            castclass = 0x74,
            isinst = 0x75,
            conv_r_un = 0x76,
            unbox = 0x79,
            throw_ = 0x7A,
            ldfld = 0x7B,
            ldflda = 0x7C,
            stfld = 0x7D,
            ldsfld = 0x7E,
            ldsflda = 0x7F,
            stsfld = 0x80,
            stobj = 0x81,
            conv_ovf_i1_un = 0x82,
            conv_ovf_i2_un = 0x83,
            conv_ovf_i4_un = 0x84,
            conv_ovf_i8_un = 0x85,
            conv_ovf_u1_un = 0x86,
            conv_ovf_u2_un = 0x87,
            conv_ovf_u4_un = 0x88,
            conv_ovf_u8_un = 0x89,
            conv_ovf_i_un = 0x8A,
            conv_ovf_u_un = 0x8B,
            box = 0x8C,
            newarr = 0x8D,
            ldlen = 0x8E,
            ldelema = 0x8F,
            ldelem_i1 = 0x90,
            ldelem_u1 = 0x91,
            ldelem_i2 = 0x92,
            ldelem_u2 = 0x93,
            ldelem_i4 = 0x94,
            ldelem_u4 = 0x95,
            ldelem_i8 = 0x96,
            ldelem_i = 0x97,
            ldelem_r4 = 0x98,
            ldelem_r8 = 0x99,
            ldelem_ref = 0x9A,
            stelem_i = 0x9B,
            stelem_i1 = 0x9C,
            stelem_i2 = 0x9D,
            stelem_i4 = 0x9E,
            stelem_i8 = 0x9F,
            stelem_r4 = 0xA0,
            stelem_r8 = 0xA1,
            stelem_ref = 0xA2,
            ldelem = 0xA3,
            stelem = 0xA4,
            unbox_any = 0xA5,
            conv_ovf_i1 = 0xB3,
            conv_ovf_u1 = 0xB4,
            conv_ovf_i2 = 0xB5,
            conv_ovf_u2 = 0xB6,
            conv_ovf_i4 = 0xB7,
            conv_ovf_u4 = 0xB8,
            conv_ovf_i8 = 0xB9,
            conv_ovf_u8 = 0xBA,
            refanyval = 0xC2,
            ckfinite = 0xC3,
            mkrefany = 0xC6,
            ldtoken = 0xD0,
            conv_u2 = 0xD1,
            conv_u1 = 0xD2,
            conv_i = 0xD3,
            conv_ovf_i = 0xD4,
            conv_ovf_u = 0xD5,
            add_ovf = 0xD6,
            add_ovf_un = 0xD7,
            mul_ovf = 0xD8,
            mul_ovf_un = 0xD9,
            sub_ovf = 0xDA,
            sub_ovf_un = 0xDB,
            endfinally = 0xDC,
            leave = 0xDD,
            leave_s = 0xDE,
            stind_i = 0xDF,
            conv_u = 0xE0,
            double_ = 0xFE,
            tysila = 0xFD
        };

        public enum DoubleOpcodes
        {
            arglist = 0x00,
            ceq = 0x01,
            cgt = 0x02,
            cgt_un = 0x03,
            clt = 0x04,
            clt_un = 0x05,
            ldftn = 0x06,
            ldvirtftn = 0x07,
            ldarg = 0x09,
            ldarga = 0x0A,
            starg = 0x0B,
            ldloc = 0x0C,
            ldloca = 0x0D,
            stloc = 0x0E,
            localloc = 0x0F,
            endfilter = 0x11,
            unaligned_ = 0x12,
            volatile_ = 0x13,
            tail_ = 0x14,
            initobj = 0x15,
            cpblk = 0x17,
            initblk = 0x18,
            rethrow = 0x1A,
            _sizeof = 0x1C,
            refanytype = 0x1D,

            flip = 0x20,
            flip3 = 0x21,
            init_rth = 0x22,
            castclassex = 0x23,
            throwfalse = 0x24,
            ldelem_vt = 0x25,
            init_rmh = 0x26,
            init_rfh = 0x27,
            stelem_vt = 0x28,
            profile = 0x29,
            gcmalloc = 0x2a,
            ldobj_addr = 0x2b,
            mbstrlen = 0x2c,
            loadcatchobj = 0x2d,
            instruction_label = 0x2e,
            pushback = 0x2f,
            throwtrue = 0x30,
            bringforward = 0x31,
            sthrow = 0x32
        }

        public enum SimpleOpcode
        {
            none,
            binnumop,
            unnumop,
            ldc,
            ldloc,
            ldloca,
            stloc,
            ldind,
            stind,
            ldarg,
            starg,
            ldelem,
            stelem,
            ldfld,
            stfld,
            br,
            brif1,
            brif2,
            conv,
            cmp,
            call,
            ret,
            nop,
            ldstr,
            newobj,
            initobj,
            dup,
            pop,
            castclass,
            ldtoken,
        }

        public enum PopBehaviour { Pop0 = 1, Pop1 = 2, PopI = 8, PopI8 = 32, PopR4 = 64, PopR8 = 128, PopRef = 256, VarPop = 512 };
        public enum PushBehaviour { Push0 = 1, Push1 = 2, PushI = 8, PushI8 = 16, PushR4 = 32, PushR8 = 64, PushRef = 128, VarPush = 256 };
        public enum InlineVar
        {
            InlineBrTarget, InlineField, InlineI, InlineI8, InlineMethod, InlineNone, InlineR,
            InlineSig, InlineString, InlineSwitch, InlineTok, InlineType, InlineVar, ShortInlineBrTarget,
            ShortInlineI, ShortInlineR, ShortInlineVar
        };
        public enum ControlFlow { BRANCH, CALL, COND_BRANCH, META, NEXT, RETURN, THROW, BREAK };

        public SingleOpcodes opcode1;
        public DoubleOpcodes opcode2;
        public SimpleOpcode sop;
        public bool directly_modifies_stack = false;
        public int opcode
        {
            get
            {
                if (opcode1 == SingleOpcodes.double_ || opcode1 == SingleOpcodes.tysila)
                    return (int)opcode2 + (((int)opcode1) << 8);
                else
                    return (int)opcode1;
            }
        }
        public string name;
        public int pop;
        public int push;
        public InlineVar inline;
        public ControlFlow ctrl;
        public int bb;

        public override string ToString()
        {
            if (name != null)
                return name;

            if (opcode1 == SingleOpcodes.double_ || opcode1 == SingleOpcodes.tysila)
                return opcode2.ToString();
            return opcode1.ToString();
        }
    }
}
