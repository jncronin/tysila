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
using libtysila5.util;

namespace libtysila5.target.x86
{
    partial class x86_Assembler
    {
        protected internal override void AssemblePass(Code c)
        { 
            var Code = text_section.Data as List<byte>;
            var code_start = Code.Count;

            Dictionary<int, int> il_starts = new Dictionary<int, int>(
                new GenericEqualityComparer<int>());

            /* Get maximum il offset of method */
            int max_il = 0;
            if (c.cil != null)
            {
                foreach (var cil in c.cil)
                {
                    if (cil.il_offset > max_il)
                        max_il = cil.il_offset;
                }
            }
            max_il++;

            for (int i = 0; i < max_il; i++)
                il_starts[i] = -1;

            List<int> rel_srcs = new List<int>();
            List<int> rel_dests = new List<int>();

            foreach(var I in c.mc)
            {
                var mc_offset = Code.Count - code_start;
                I.offset = mc_offset;
                I.addr = Code.Count;
                I.end_addr = Code.Count;

                if (I.parent != null)
                {
                    var cil = I.parent.parent;
                    if (il_starts[cil.il_offset] == -1)
                    {
                        var ir = I.parent;
                        /* We don't want the il offset for the first node to point
                         * to the enter/enter_handler opcode as this potentially breaks
                         * small methods like:
                         * 
                         * IL_0000: br.s IL_0000
                         * 
                         * where we want the jmp to be to the br irnode, rather than the enter irnode */
                        if (ir.opcode != libtysila5.ir.Opcode.oc_enter &&
                            ir.opcode != libtysila5.ir.Opcode.oc_enter_handler &&
                            ir.ignore_for_mcoffset == false)
                        {
                            il_starts[cil.il_offset] = mc_offset;
                            cil.mc_offset = mc_offset;
                        }
                    }
                }

                if (I.p.Length == 0)
                    continue;

                int tls_flag = (int)I.p[0].v2;
                switch (I.p[0].v)
                {
                    case Generic.g_mclabel:
                        il_starts[(int)I.p[1].v] = mc_offset;
                        break;
                    case Generic.g_label:
                        c.extra_labels.Add(new Code.Label
                        {
                            Offset = mc_offset + code_start,
                            Name = I.p[1].str
                        });
                        break;

                    case x86_push_rm32:
                        AddRex(Code, Rex(0, null, I.p[1].mreg));
                        Code.Add(0xff);
                        Code.AddRange(ModRMSIB(6, I.p[1].mreg));
                        break;
                    case x86_push_r32:
                        AddRex(Code, Rex(0, null, I.p[1].mreg));
                        Code.Add(PlusRD(0x50, I.p[1].mreg));
                        break;
                    case x86_push_imm32:
                        Code.Add(0x68);
                        AddImm32(Code, I.p[1].v);
                        break;
                    case x86_pop_rm32:
                        AddRex(Code, Rex(0, null, I.p[1].mreg));
                        Code.Add(0x8f);
                        Code.AddRange(ModRMSIB(0, I.p[1].mreg));
                        break;
                    case x86_pop_r32:
                        AddRex(Code, Rex(0, null, I.p[1].mreg));
                        Code.Add(PlusRD(0x58, I.p[1].mreg));
                        break;
                    case x86_mov_r32_rm32:
                    case x86_mov_r8_rm8:
                    case x86_mov_r16_rm16:
                    case x86_mov_r64_rm64:
                        if (I.p[0].v == x86_mov_r8_rm8)
                        {
                            Code.AddRange(TLSOverride(ref tls_flag));
                            Code.Add(0x8a);
                        }
                        else if (I.p[0].v == x86_mov_r16_rm16)
                        {
                            Code.AddRange(TLSOverride(ref tls_flag));
                            Code.Add(0x67);
                            Code.Add(0x8b);
                        }
                        else
                        {
                            Code.AddRange(TLSOverride(ref tls_flag));
                            AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                            Code.Add(0x8b);
                        }

                        switch(I.p[2].t)
                        {
                            case ir.Opcode.vl_mreg:
                                Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                                break;
                            case ir.Opcode.vl_str:
                                Code.AddRange(ModRMSIB(GetR(I.p[1].mreg), 5, 0, -1, 0, 0, false));
                                var reloc = bf.CreateRelocation();
                                reloc.DefinedIn = text_section;
                                reloc.Type = new binary_library.elf.ElfFile.Rel_386_32();
                                reloc.Addend = I.p[2].v;
                                reloc.References = bf.CreateSymbol();
                                reloc.References.Name = I.p[2].str;
                                reloc.Offset = (ulong)Code.Count;
                                bf.AddRelocation(reloc);
                                AddImm32(Code, 0);
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                        break;
                    case x86_mov_rm32_r32:
                    case x86_mov_rm64_r64:
                        AddRex(Code, Rex(I.p[0].v, I.p[2].mreg, I.p[1].mreg));
                        Code.Add(0x89);
                        Code.AddRange(ModRMSIB(I.p[2].mreg, I.p[1].mreg));
                        break;
                    case x86_mov_rm64_imm32:
                    case x86_mov_rm32_imm32:
                        AddRex(Code, Rex(I.p[0].v, null, I.p[1].mreg));
                        Code.Add(0xc7);
                        Code.AddRange(ModRMSIB(0, I.p[1].mreg));

                        switch (I.p[2].t)
                        {
                            case ir.Opcode.vl_c:
                            case ir.Opcode.vl_c32:
                                AddImm32(Code, I.p[2].v);
                                break;
                            case ir.Opcode.vl_str:
                                var reloc = bf.CreateRelocation();
                                reloc.DefinedIn = text_section;
                                if (I.p[2].v2 == 1)
                                {
                                    if (psize == 4)
                                        reloc.Type = new binary_library.elf.ElfFile.Rel_386_TLS_DTPOFF32();
                                    else
                                        reloc.Type = new binary_library.elf.ElfFile.Rel_x86_64_TLS_TPOFF32();
                                }
                                else if (psize == 4)
                                    reloc.Type = new binary_library.elf.ElfFile.Rel_386_32();
                                else
                                    reloc.Type = new binary_library.elf.ElfFile.Rel_x86_64_32();
                                reloc.Addend = I.p[2].v;
                                reloc.References = bf.CreateSymbol();
                                reloc.References.Name = I.p[2].str;
                                reloc.Offset = (ulong)Code.Count;
                                bf.AddRelocation(reloc);
                                AddImm32(Code, 0);
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                        break;
                    case x86_add_rm32_imm32:
                    case x86_add_rm64_imm32:
                        AddRex(Code, Rex(I.p[0].v, null, I.p[1].mreg));
                        Code.Add(0x81);
                        Code.AddRange(ModRMSIB(0, I.p[1].mreg));
                        AddImm32(Code, I.p[2].v);
                        if (I.p.Length == 4)
                            throw new Exception();
                        break;
                    case x86_add_rm32_imm8:
                    case x86_add_rm64_imm8:
                        AddRex(Code, Rex(I.p[0].v, null, I.p[1].mreg));
                        Code.Add(0x83);
                        Code.AddRange(ModRMSIB(0, I.p[1].mreg));
                        AddImm8(Code, I.p[2].v);
                        if (I.p.Length == 4)
                            throw new Exception();
                        break;
                    case x86_adc_r32_rm32:
                        Code.Add(0x13);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        if (I.p.Length == 4)
                            throw new Exception();
                        break;
                    case x86_sub_rm32_imm32:
                    case x86_sub_rm64_imm32:
                        AddRex(Code, Rex(I.p[0].v, null, I.p[1].mreg));
                        Code.Add(0x81);
                        Code.AddRange(ModRMSIB(5, I.p[1].mreg));
                        AddImm32(Code, I.p[2].v);
                        if (I.p.Length == 4)
                            throw new Exception();
                        break;
                    case x86_sub_rm32_imm8:
                    case x86_sub_rm64_imm8:
                        AddRex(Code, Rex(I.p[0].v, null, I.p[1].mreg));
                        Code.Add(0x83);
                        Code.AddRange(ModRMSIB(5, I.p[1].mreg));
                        AddImm8(Code, I.p[2].v);
                        if (I.p.Length == 4)
                            throw new Exception();
                        break;
                    case x86_sub_r32_rm32:
                    case x86_sub_r64_rm64:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x2b);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        if (I.p.Length == 4)
                            throw new Exception();
                        break;
                    case x86_and_r32_rm32:
                    case x86_and_r64_rm64:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x23);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        if (I.p.Length == 4)
                            throw new Exception();
                        break;
                    case x86_and_rm32_r32:
                    case x86_and_rm64_r64:
                        AddRex(Code, Rex(I.p[0].v, I.p[2].mreg, I.p[1].mreg));
                        Code.Add(0x21);
                        Code.AddRange(ModRMSIB(I.p[2].mreg, I.p[1].mreg));
                        if (I.p.Length == 4)
                            throw new Exception();
                        break;
                    case x86_or_r32_rm32:
                    case x86_or_r64_rm64:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x0b);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        if (I.p.Length == 4)
                            throw new Exception();
                        break;
                    case x86_or_rm32_r32:
                    case x86_or_rm64_r64:
                        AddRex(Code, Rex(I.p[0].v, I.p[2].mreg, I.p[1].mreg));
                        Code.Add(0x09);
                        Code.AddRange(ModRMSIB(I.p[2].mreg, I.p[1].mreg));
                        if (I.p.Length == 4)
                            throw new Exception();
                        break;
                    case x86_xor_r32_rm32:
                    case x86_xor_r64_rm64:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x33);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        if (I.p.Length == 4)
                            throw new Exception();
                        break;
                    case x86_xor_rm32_r32:
                    case x86_xor_rm64_r64:
                        AddRex(Code, Rex(I.p[0].v, I.p[2].mreg, I.p[1].mreg));
                        Code.Add(0x31);
                        Code.AddRange(ModRMSIB(I.p[2].mreg, I.p[1].mreg));
                        if (I.p.Length == 4)
                            throw new Exception();
                        break;
                    case x86_sbb_r32_rm32:
                    case x86_sbb_r64_rm64:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x1b);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        if (I.p.Length == 4)
                            throw new Exception();
                        break;
                    case x86_add_r64_rm64:
                    case x86_add_r32_rm32:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x03);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        if (I.p.Length == 4)
                            throw new Exception();
                        break;
                    case x86_add_rm32_r32:
                    case x86_add_rm64_r64:
                        AddRex(Code, Rex(I.p[0].v, I.p[2].mreg, I.p[1].mreg));
                        Code.Add(0x01);
                        Code.AddRange(ModRMSIB(I.p[2].mreg, I.p[1].mreg));
                        if (I.p.Length == 4)
                            throw new Exception();
                        break;
                    case x86_call_rel32:
                        {
                            Code.Add(0xe8);
                            var reloc = bf.CreateRelocation();
                            reloc.DefinedIn = text_section;
                            reloc.Type = new binary_library.elf.ElfFile.Rel_386_PC32();
                            reloc.Addend = -4;
                            reloc.References = bf.CreateSymbol();
                            reloc.References.Name = I.p[1].str;
                            reloc.References.ObjectType = binary_library.SymbolObjectType.Function;
                            reloc.Offset = (ulong)Code.Count;
                            bf.AddRelocation(reloc);
                            AddImm32(Code, 0);
                        }
                        break;
                    case x86_call_rm32:
                        {
                            AddRex(Code, Rex(I.p[0].v, null, I.p[1].mreg));
                            Code.Add(0xff);
                            var obj = I.p[1];
                            Code.AddRange(ModRMSIB(2, obj.mreg));
                            break;
                        }
                    case x86_cmp_rm32_r32:
                    case x86_cmp_rm64_r64:
                        AddRex(Code, Rex(I.p[0].v, I.p[2].mreg, I.p[1].mreg));
                        Code.Add(0x39);
                        Code.AddRange(ModRMSIB(I.p[2].mreg, I.p[1].mreg));
                        break;
                    case x86_cmp_r32_rm32:
                    case x86_cmp_r64_rm64:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x3b);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;
                    case x86_cmp_rm32_imm32:
                        if (I.p[1].mreg == r_eax)
                            Code.Add(0x3d);
                        else
                        {
                            Code.Add(0x81);
                            Code.AddRange(ModRMSIB(7, I.p[1].mreg));
                        }
                        AddImm32(Code, I.p[2].v);
                        break;
                    case x86_cmp_rm8_imm8:
                        if (I.p[1].mreg == r_eax)
                            Code.Add(0x3c);
                        else
                        {
                            AddRex(Code, Rex(0, null, I.p[1].mreg));
                            Code.Add(0x80);
                            Code.AddRange(ModRMSIB(7, I.p[1].mreg));
                        }
                        AddImm8(Code, I.p[2].v);
                        break;
                    case x86_cmp_rm32_imm8:
                        Code.Add(0x83);
                        Code.AddRange(ModRMSIB(7, I.p[1].mreg));
                        AddImm8(Code, I.p[2].v);
                        break;
                    case x86_lock_cmpxchg_rm8_r8:
                        Code.Add(0xf0); // lock before rex
                        AddRex(Code, Rex(I.p[0].v, I.p[2].mreg, I.p[1].mreg, null, true, true));
                        Code.Add(0x0f);
                        Code.Add(0xb0);
                        Code.AddRange(ModRMSIB(I.p[2].mreg, I.p[1].mreg));
                        break;
                    case x86_lock_cmpxchg_rm32_r32:
                    case x86_lock_cmpxchg_rm64_r64:
                        Code.Add(0xf0); // lock before rex
                        Code.AddRange(TLSOverride(ref tls_flag));
                        AddRex(Code, Rex(I.p[0].v, I.p[2].mreg, I.p[1].mreg));
                        Code.Add(0x0f);
                        Code.Add(0xb1);
                        Code.AddRange(ModRMSIB(I.p[2].mreg, I.p[1].mreg));
                        break;
                    case x86_lock_cmpxchg8b_m64:
                        Code.Add(0xf0);
                        Code.AddRange(TLSOverride(ref tls_flag));
                        Code.Add(0x0f);
                        Code.Add(0xc7);
                        Code.AddRange(ModRMSIB(1, I.p[1].mreg));
                        break;
                    case x86_lock_xchg_rm32ptr_r32:
                    case x86_lock_xchg_rm64ptr_r64:
                        Code.Add(0xf0);
                        Code.AddRange(TLSOverride(ref tls_flag));
                        AddRex(Code, Rex(I.p[0].v, I.p[2].mreg, I.p[1].mreg));
                        Code.Add(0x87);
                        Code.AddRange(ModRMSIB(I.p[2].mreg, new ContentsReg { basereg = I.p[1].mreg }));
                        break;
                    case x86_pause:
                        Code.Add(0xf3);
                        Code.Add(0x90);
                        break;
                    case x86_set_rm32:
                        if (I.p[1].v != ir.Opcode.cc_never)
                        {
                            Code.Add(0x0f);
                            switch (I.p[1].v)
                            {
                                case ir.Opcode.cc_a:
                                    Code.Add(0x97);
                                    break;
                                case ir.Opcode.cc_ae:
                                    Code.Add(0x93);
                                    break;
                                case ir.Opcode.cc_b:
                                    Code.Add(0x92);
                                    break;
                                case ir.Opcode.cc_be:
                                    Code.Add(0x96);
                                    break;
                                case ir.Opcode.cc_eq:
                                    Code.Add(0x94);
                                    break;
                                case ir.Opcode.cc_ge:
                                    Code.Add(0x9d);
                                    break;
                                case ir.Opcode.cc_gt:
                                    Code.Add(0x9f);
                                    break;
                                case ir.Opcode.cc_le:
                                    Code.Add(0x9e);
                                    break;
                                case ir.Opcode.cc_lt:
                                    Code.Add(0x9c);
                                    break;
                                case ir.Opcode.cc_ne:
                                    Code.Add(0x95);
                                    break;
                                case ir.Opcode.cc_always:
                                    throw new NotImplementedException();
                            }
                            Code.AddRange(ModRMSIB(2, I.p[2].mreg));
                        }
                        break;
                    case x86_movsxbd:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg, null, true));
                        Code.Add(0x0f);
                        Code.Add(0xbe);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;
                    case x86_movsxwd:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x0f);
                        Code.Add(0xbf);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;
                    case x86_movsxdq_r64_rm64:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x63);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;
                    case x86_movzxbd:
                    case x86_movzxbq:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg, null, true));
                        Code.Add(0x0f);
                        Code.Add(0xb6);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;
                    case x86_movzxwd:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x0f);
                        Code.Add(0xb7);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;
                    case x86_movsxbd_r32_rm8disp:
                    case x86_movsxbq_r64_rm8disp:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg, null, true));
                        Code.Add(0x0f);
                        Code.Add(0xbe);
                        Code.AddRange(ModRMSIB(GetR(I.p[1].mreg), GetRM(I.p[2].mreg), 2, -1, -1, (int)I.p[3].v));
                        break;
                    case x86_movsxwd_r32_rm16disp:
                    case x86_movsxwq_r64_rm16disp:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x0f);
                        Code.Add(0xbf);
                        Code.AddRange(ModRMSIB(GetR(I.p[1].mreg), GetRM(I.p[2].mreg), 2, -1, -1, (int)I.p[3].v));
                        break;
                    case x86_movzxbd_r32_rm8disp:
                    case x86_movzxbq_r64_rm8disp:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg, null, true));
                        Code.Add(0x0f);
                        Code.Add(0xb6);
                        Code.AddRange(ModRMSIB(GetR(I.p[1].mreg), GetRM(I.p[2].mreg), 2, -1, -1, (int)I.p[3].v));
                        break;
                    case x86_movzxwd_r32_rm16disp:
                    case x86_movzxwq_r64_rm16disp:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x0f);
                        Code.Add(0xb7);
                        Code.AddRange(ModRMSIB(GetR(I.p[1].mreg), GetRM(I.p[2].mreg), 2, -1, -1, (int)I.p[3].v));
                        break;

                    case x86_jcc_rel32:
                        if (I.p[1].v != ir.Opcode.cc_never)
                        {
                            Code.Add(0x0f);
                            switch (I.p[1].v)
                            {
                                case ir.Opcode.cc_a:
                                    Code.Add(0x87);
                                    break;
                                case ir.Opcode.cc_ae:
                                    Code.Add(0x83);
                                    break;
                                case ir.Opcode.cc_b:
                                    Code.Add(0x82);
                                    break;
                                case ir.Opcode.cc_be:
                                    Code.Add(0x86);
                                    break;
                                case ir.Opcode.cc_eq:
                                    Code.Add(0x84);
                                    break;
                                case ir.Opcode.cc_ge:
                                    Code.Add(0x8d);
                                    break;
                                case ir.Opcode.cc_gt:
                                    Code.Add(0x8f);
                                    break;
                                case ir.Opcode.cc_le:
                                    Code.Add(0x8e);
                                    break;
                                case ir.Opcode.cc_lt:
                                    Code.Add(0x8c);
                                    break;
                                case ir.Opcode.cc_ne:
                                    Code.Add(0x85);
                                    break;
                                case ir.Opcode.cc_always:
                                    throw new NotImplementedException();
                            }

                            rel_srcs.Add(Code.Count);
                            rel_dests.Add((int)I.p[2].v);
                            AddImm32(Code, 0);
                        }
                        break;
                    case x86_jmp_rel32:
                        Code.Add(0xe9);

                        if (I.p[1].t == ir.Opcode.vl_br_target)
                        {
                            rel_srcs.Add(Code.Count);
                            rel_dests.Add((int)I.p[1].v);
                            AddImm32(Code, 0);
                        }
                        else if (I.p[1].t == ir.Opcode.vl_str)
                        {
                            var reloc = bf.CreateRelocation();
                            reloc.DefinedIn = text_section;
                            if (psize == 4)
                                reloc.Type = new binary_library.elf.ElfFile.Rel_386_PC32();
                            else
                                reloc.Type = new binary_library.elf.ElfFile.Rel_x86_64_pc32();
                            reloc.Addend = -4;
                            reloc.References = bf.CreateSymbol();
                            reloc.References.Name = I.p[1].str;
                            reloc.References.ObjectType = binary_library.SymbolObjectType.Function;
                            reloc.Offset = (ulong)Code.Count;
                            bf.AddRelocation(reloc);
                            AddImm32(Code, 0);
                        }
                        else
                            throw new NotSupportedException();
                        break;
                    case x86_ret:
                        Code.Add(0xc3);
                        break;
                    case Generic.g_loadaddress:
                        {
                            Code.Add(0xc7);
                            Code.AddRange(ModRMSIB(0, I.p[1].mreg));
                            var reloc = bf.CreateRelocation();
                            reloc.DefinedIn = text_section;
                            reloc.Type = new binary_library.elf.ElfFile.Rel_386_32();
                            reloc.Addend = I.p[2].v;
                            reloc.References = bf.CreateSymbol();
                            reloc.References.Name = I.p[2].str;
                            reloc.References.ObjectType = binary_library.SymbolObjectType.Function;
                            reloc.Offset = (ulong)Code.Count;
                            bf.AddRelocation(reloc);
                            AddImm32(Code, 0);
                        }
                        break;

                    case x86_mov_rm32_lab:
                        {
                            Code.Add(0xc7);
                            Code.AddRange(ModRMSIB(0, I.p[1].mreg));
                            var reloc = bf.CreateRelocation();
                            reloc.DefinedIn = text_section;
                            reloc.Type = new binary_library.elf.ElfFile.Rel_386_32();
                            reloc.Addend = I.p[2].v;
                            reloc.References = bf.CreateSymbol();
                            reloc.References.Name = I.p[2].str;
                            reloc.References.ObjectType = binary_library.SymbolObjectType.Object;
                            reloc.Offset = (ulong)Code.Count;
                            bf.AddRelocation(reloc);
                            AddImm32(Code, 0);
                        }
                        break;

                    case x86_mov_r32_lab:
                        {
                            Code.Add(0x8b);
                            Code.AddRange(ModRMSIB(GetR(I.p[1].mreg), 5, 0, -1, 0, 0, false));
                            var reloc = bf.CreateRelocation();
                            reloc.DefinedIn = text_section;
                            reloc.Type = new binary_library.elf.ElfFile.Rel_386_32();
                            reloc.Addend = I.p[2].v;
                            reloc.References = bf.CreateSymbol();
                            reloc.References.Name = I.p[2].str;
                            reloc.References.ObjectType = binary_library.SymbolObjectType.Object;
                            reloc.Offset = (ulong)Code.Count;
                            bf.AddRelocation(reloc);
                            AddImm32(Code, 0);
                        }
                        break;

                    case x86_mov_lab_r32:
                        {
                            Code.Add(0x89);
                            Code.AddRange(ModRMSIB(GetR(I.p[2].mreg), 5, 0, -1, 0, 0, false));
                            var reloc = bf.CreateRelocation();
                            reloc.DefinedIn = text_section;
                            reloc.Type = new binary_library.elf.ElfFile.Rel_386_32();
                            reloc.Addend = I.p[1].v;
                            reloc.References = bf.CreateSymbol();
                            reloc.References.Name = I.p[1].str;
                            reloc.References.ObjectType = binary_library.SymbolObjectType.Object;
                            reloc.Offset = (ulong)Code.Count;
                            bf.AddRelocation(reloc);
                            AddImm32(Code, 0);
                        }
                        break;

                    case x86_idiv_rm32:
                    case x86_idiv_rm64:
                        AddRex(Code, Rex(I.p[0].v, null, I.p[1].mreg));
                        Code.Add(0xf7);
                        Code.AddRange(ModRMSIB(7, I.p[1].mreg));
                        break;

                    case x86_imul_r32_rm32_imm32:
                        Code.Add(0x69);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        AddImm32(Code, I.p[3].v);
                        break;

                    case x86_imul_r32_rm32:
                    case x86_imul_r64_rm64:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x0f);
                        Code.Add(0xaf);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;

                    case x86_mov_r32_rm32sib:
                        Code.Add(0x8b);
                        Code.AddRange(ModRMSIB(GetR(I.p[1].mreg), GetRM(I.p[3].mreg), 0, GetR(I.p[2].mreg)));
                        break;

                    case x86_mov_r32_rm32disp:
                    case x86_mov_r64_rm64disp:
                        Code.AddRange(TLSOverride(ref tls_flag));
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x8b);
                        Code.AddRange(ModRMSIB(GetR(I.p[1].mreg), GetRM(I.p[2].mreg), 2, -1, -1, (int)I.p[3].v, I.p[2].mreg.Equals(x86_64.x86_64_Assembler.r_r13)));
                        break;
                    case x86_mov_r32_rm16disp:
                        Code.Add(0x66);
                        Code.Add(0x8b);
                        Code.AddRange(ModRMSIB(GetR(I.p[1].mreg), GetRM(I.p[2].mreg), 2, -1, -1, (int)I.p[3].v));
                        break;
                    case x86_mov_r32_rm8disp:
                        Code.Add(0x8a);
                        Code.AddRange(ModRMSIB(GetR(I.p[1].mreg), GetRM(I.p[2].mreg), 2, -1, -1, (int)I.p[3].v));
                        break;


                    case x86_mov_rm32disp_imm32:
                    case x86_mov_rm64disp_imm32:
                        AddRex(Code, Rex(I.p[0].v, null, I.p[1].mreg));
                        Code.Add(0xc7);
                        Code.AddRange(ModRMSIB(0, GetRM(I.p[1].mreg), 2, -1, -1, (int)I.p[2].v));
                        AddImm32(Code, I.p[3].v);
                        break;
                    case x86_mov_rm16disp_imm32:
                        Code.Add(0x66);
                        Code.Add(0xc7);
                        Code.AddRange(ModRMSIB(0, GetRM(I.p[1].mreg), 2, -1, -1, (int)I.p[2].v));
                        AddImm16(Code, I.p[3].v);
                        break;
                    case x86_mov_rm8disp_imm32:
                        AddRex(Code, Rex(I.p[0].v, null, I.p[1].mreg, null, true));
                        Code.Add(0xc6);
                        Code.AddRange(ModRMSIB(0, GetRM(I.p[1].mreg), 2, -1, -1, (int)I.p[2].v));
                        AddImm8(Code, I.p[3].v);
                        break;

                    case x86_mov_rm8disp_r8:
                        AddRex(Code, Rex(I.p[0].v, I.p[3].mreg, I.p[1].mreg, null, true, true));
                        Code.Add(0x88);
                        Code.AddRange(ModRMSIB(GetR(I.p[3].mreg), GetRM(I.p[1].mreg), 2, -1, -1, (int)I.p[2].v));
                        break;
                    case x86_mov_rm16disp_r16:
                        Code.Add(0x66);
                        AddRex(Code, Rex(I.p[0].v, I.p[3].mreg, I.p[1].mreg));
                        Code.Add(0x89);
                        Code.AddRange(ModRMSIB(GetR(I.p[3].mreg), GetRM(I.p[1].mreg), 2, -1, -1, (int)I.p[2].v));
                        break;
                    case x86_mov_rm32disp_r32:
                    case x86_mov_rm64disp_r64:
                        Code.AddRange(TLSOverride(ref tls_flag));
                        AddRex(Code, Rex(I.p[0].v, I.p[3].mreg, I.p[1].mreg));
                        Code.Add(0x89);
                        Code.AddRange(ModRMSIB(GetR(I.p[3].mreg), GetRM(I.p[1].mreg), 2, -1, -1, (int)I.p[2].v));
                        break;

                    case x86_mov_r32_rm32sibscaledisp:
                    case x86_mov_r64_rm64sibscaledisp:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg, I.p[3].mreg));
                        Code.Add(0x8b);
                        Code.AddRange(ModRMSIB(GetR(I.p[1].mreg), GetRM(I.p[2].mreg), 2, GetRM(I.p[3].mreg), -1, (int)I.p[5].v, false, (int)I.p[4].v));
                        break;

                    case x86_movzxbd_r32_rm8sibscaledisp:
                    case x86_movzxbq_r64_rm8sibscaledisp:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg, I.p[3].mreg, true));
                        Code.Add(0x0f);
                        Code.Add(0xb6);
                        Code.AddRange(ModRMSIB(GetR(I.p[1].mreg), GetRM(I.p[2].mreg), 2, GetRM(I.p[3].mreg), -1, (int)I.p[5].v, false, (int)I.p[4].v));
                        break;

                    case x86_movzxwd_r32_rm16sibscaledisp:
                    case x86_movzxwq_r64_rm16sibscaledisp:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg, I.p[3].mreg));
                        Code.Add(0x0f);
                        Code.Add(0xb7);
                        Code.AddRange(ModRMSIB(GetR(I.p[1].mreg), GetRM(I.p[2].mreg), 2, GetRM(I.p[3].mreg), -1, (int)I.p[5].v, false, (int)I.p[4].v));
                        break;

                    case x86_movsxbd_r32_rm8sibscaledisp:
                    case x86_movsxbq_r64_rm8sibscaledisp:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg, I.p[3].mreg, true));
                        Code.Add(0x0f);
                        Code.Add(0xbe);
                        Code.AddRange(ModRMSIB(GetR(I.p[1].mreg), GetRM(I.p[2].mreg), 2, GetRM(I.p[3].mreg), -1, (int)I.p[5].v, false, (int)I.p[4].v));
                        break;

                    case x86_movsxwd_r32_rm16sibscaledisp:
                    case x86_movsxwq_r64_rm16sibscaledisp:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg, I.p[3].mreg));
                        Code.Add(0x0f);
                        Code.Add(0xbf);
                        Code.AddRange(ModRMSIB(GetR(I.p[1].mreg), GetRM(I.p[2].mreg), 2, GetRM(I.p[3].mreg), -1, (int)I.p[5].v, false, (int)I.p[4].v));
                        break;

                    case x86_movsxdq_r64_rm32sibscaledisp:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg, I.p[3].mreg));
                        Code.Add(0x63);
                        Code.AddRange(ModRMSIB(GetR(I.p[1].mreg), GetRM(I.p[2].mreg), 2, GetRM(I.p[3].mreg), -1, (int)I.p[5].v, false, (int)I.p[4].v));
                        break;

                    case x86_neg_rm32:
                    case x86_neg_rm64:
                        AddRex(Code, Rex(I.p[0].v, null, I.p[1].mreg));
                        Code.Add(0xf7);
                        Code.AddRange(ModRMSIB(3, I.p[1].mreg));
                        break;

                    case x86_not_rm32:
                    case x86_not_rm64:
                        AddRex(Code, Rex(I.p[0].v, null, I.p[1].mreg));
                        Code.Add(0xf7);
                        Code.AddRange(ModRMSIB(2, I.p[1].mreg));
                        break;

                    case x86_lea_r32:
                    case x86_lea_r64:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x8d);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;

                    case x86_sar_rm32_imm8:
                        Code.Add(0xc1);
                        Code.AddRange(ModRMSIB(7, I.p[1].mreg));
                        AddImm8(Code, I.p[2].v);
                        break;

                    case x86_and_rm32_imm8:
                    case x86_and_rm64_imm8:
                        AddRex(Code, Rex(I.p[0].v, null, I.p[1].mreg));
                        Code.Add(0x83);
                        Code.AddRange(ModRMSIB(4, I.p[1].mreg));
                        AddImm8(Code, I.p[2].v);
                        if (I.p.Length == 4)
                            throw new Exception();
                        break;

                    case x86_and_rm32_imm32:
                        Code.Add(0x81);
                        Code.AddRange(ModRMSIB(4, I.p[1].mreg));
                        AddImm8(Code, I.p[2].v);
                        if (I.p.Length == 4)
                            throw new Exception();
                        break;

                    case x86_sal_rm32_cl:
                    case x86_sal_rm64_cl:
                        AddRex(Code, Rex(I.p[0].v, null, I.p[1].mreg));
                        Code.Add(0xd3);
                        Code.AddRange(ModRMSIB(4, I.p[1].mreg));
                        break;

                    case x86_sar_rm32_cl:
                    case x86_sar_rm64_cl:
                        AddRex(Code, Rex(I.p[0].v, null, I.p[1].mreg));
                        Code.Add(0xd3);
                        Code.AddRange(ModRMSIB(7, I.p[1].mreg));
                        break;

                    case x86_shr_rm32_cl:
                    case x86_shr_rm64_cl:
                        AddRex(Code, Rex(I.p[0].v, null, I.p[1].mreg));
                        Code.Add(0xd3);
                        Code.AddRange(ModRMSIB(5, I.p[1].mreg));
                        break;

                    case x86_xchg_r32_rm32:
                    case x86_xchg_r64_rm64:
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x87);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;
                    case x86_xchg_rm32_r32:
                        Code.Add(0x87);
                        Code.AddRange(ModRMSIB(I.p[2].mreg, I.p[1].mreg));
                        break;

                    case x86_mov_r64_imm64:
                        AddRex(Code, Rex(I.p[0].v, null, I.p[1].mreg));
                        Code.Add(PlusRD(0xb8, I.p[1].mreg));
                        switch (I.p[2].t)
                        {
                            case ir.Opcode.vl_c:
                            case ir.Opcode.vl_c32:
                            case ir.Opcode.vl_c64:
                                AddImm64(Code, I.p[2].v);
                                break;
                            case ir.Opcode.vl_str:
                                var reloc = bf.CreateRelocation();
                                reloc.DefinedIn = text_section;
                                if (I.p[2].v2 == 1)
                                {
                                    throw new NotImplementedException("TLS label with mcmodel large");
                                }
                                reloc.Type = new binary_library.elf.ElfFile.Rel_x86_64_64();
                                reloc.Addend = I.p[2].v;
                                reloc.References = bf.CreateSymbol();
                                reloc.References.Name = I.p[2].str;
                                reloc.Offset = (ulong)Code.Count;
                                bf.AddRelocation(reloc);
                                AddImm64(Code, 0);
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                        break;

                    case x86_nop:
                        Code.Add(0x90);
                        break;

                    case x86_out_dx_al:
                        Code.Add(0xee);
                        break;
                    case x86_out_dx_ax:
                        Code.Add(0x66);
                        Code.Add(0xef);
                        break;
                    case x86_out_dx_eax:
                        Code.Add(0xef);
                        break;

                    case x86_in_al_dx:
                        Code.Add(0xec);
                        break;
                    case x86_in_ax_dx:
                        Code.Add(0x66);
                        Code.Add(0xed);
                        break;
                    case x86_in_eax_dx:
                        Code.Add(0xed);
                        break;

                    case x86_int3:
                        Code.Add(0xcc);
                        break;

                    case x86_xorpd_xmm_xmmm128:
                        Code.Add(0x66);
                        Code.Add(0x0f);
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x57);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;

                    case x86_movsd_xmmm64_xmm:
                        Code.Add(0xf2);
                        AddRex(Code, Rex(I.p[0].v, I.p[2].mreg, I.p[1].mreg));
                        Code.Add(0x0f);
                        Code.Add(0x11);
                        Code.AddRange(ModRMSIB(I.p[2].mreg, I.p[1].mreg));
                        break;

                    case x86_movsd_xmmm64disp_xmm:
                        Code.Add(0xf2);
                        AddRex(Code, Rex(I.p[0].v, I.p[3].mreg, I.p[1].mreg));
                        Code.Add(0x0f);
                        Code.Add(0x11);
                        Code.AddRange(ModRMSIB(GetR(I.p[3].mreg), GetRM(I.p[1].mreg), 2, -1, -1, (int)I.p[2].v));
                        break;

                    case x86_movsd_xmm_xmmm64:
                        Code.Add(0xf2);
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x0f);
                        Code.Add(0x10);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;

                    case x86_movsd_xmm_xmmm64disp:
                        Code.Add(0xf2);
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x0f);
                        Code.Add(0x10);
                        Code.AddRange(ModRMSIB(GetR(I.p[1].mreg), GetRM(I.p[2].mreg), 2, -1, -1, (int)I.p[3].v));
                        break;

                    case x86_movss_xmmm32_xmm:
                        Code.Add(0xf3);
                        AddRex(Code, Rex(I.p[0].v, I.p[2].mreg, I.p[1].mreg));
                        Code.Add(0x0f);
                        Code.Add(0x11);
                        Code.AddRange(ModRMSIB(I.p[2].mreg, I.p[1].mreg));
                        break;

                    case x86_movss_xmmm32disp_xmm:
                        Code.Add(0xf3);
                        AddRex(Code, Rex(I.p[0].v, I.p[3].mreg, I.p[1].mreg));
                        Code.Add(0x0f);
                        Code.Add(0x11);
                        Code.AddRange(ModRMSIB(GetR(I.p[3].mreg), GetRM(I.p[1].mreg), 2, -1, -1, (int)I.p[2].v));
                        break;

                    case x86_movss_xmm_xmmm32:
                        Code.Add(0xf3);
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x0f);
                        Code.Add(0x10);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;

                    case x86_cvtsd2si_r32_xmmm64:
                    case x86_cvtsd2si_r64_xmmm64:
                        Code.Add(0xf2);
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x0f);
                        Code.Add(0x2d);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;

                    case x86_cvtsi2sd_xmm_rm32:
                    case x86_cvtsi2sd_xmm_rm64:
                        Code.Add(0xf2);
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x0f);
                        Code.Add(0x2a);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;

                    case x86_cvtsd2ss_xmm_xmmm64:
                        Code.Add(0xf2);
                        Code.Add(0x0f);
                        Code.Add(0x5a);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;

                    case x86_cvtss2sd_xmm_xmmm32:
                        Code.Add(0xf3);
                        Code.Add(0x0f);
                        Code.Add(0x5a);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;

                    case x86_cvtss2sd_xmm_xmmm32disp:
                        Code.Add(0xf3);
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x0f);
                        Code.Add(0x5a);
                        Code.AddRange(ModRMSIB(GetR(I.p[1].mreg), GetRM(I.p[2].mreg), 2, -1, -1, (int)I.p[3].v));
                        break;

                    case x86_addsd_xmm_xmmm64:
                        Code.Add(0xf2);
                        Code.Add(0x0f);
                        Code.Add(0x58);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;

                    case x86_subsd_xmm_xmmm64:
                        Code.Add(0xf2);
                        Code.Add(0x0f);
                        Code.Add(0x5c);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;

                    case x86_mulsd_xmm_xmmm64:
                        Code.Add(0xf2);
                        Code.Add(0x0f);
                        Code.Add(0x59);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;

                    case x86_divsd_xmm_xmmm64:
                        Code.Add(0xf2);
                        Code.Add(0x0f);
                        Code.Add(0x5e);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;

                    case x86_comisd_xmm_xmmm64:
                        Code.Add(0x66);
                        Code.Add(0x0f);
                        Code.Add(0x2f);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;

                    case x86_ucomisd_xmm_xmmm64:
                        Code.Add(0x66);
                        Code.Add(0x0f);
                        Code.Add(0x2e);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        break;

                    case x86_cmpsd_xmm_xmmm64_imm8:
                        Code.Add(0xf2);
                        Code.Add(0x0f);
                        Code.Add(0xc2);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        AddImm8(Code, I.p[3].v);
                        break;

                    case x86_roundsd_xmm_xmmm64_imm8:
                        Code.Add(0x66);
                        Code.Add(0x0f);
                        AddRex(Code, Rex(I.p[0].v, I.p[1].mreg, I.p[2].mreg));
                        Code.Add(0x3a);
                        Code.Add(0x0b);
                        Code.AddRange(ModRMSIB(I.p[1].mreg, I.p[2].mreg));
                        AddImm8(Code, I.p[3].v);
                        break;

                    case x86_iret:
                    case x86_iretq:
                        AddRex(Code, Rex(I.p[0].v, null, null));
                        Code.Add(0xcf);
                        break;

                    case x86_pushf:
                        Code.Add(0x9c);
                        break;

                    case x86_popf:
                    case x86_popfq:
                        AddRex(Code, Rex(I.p[0].v, null, null));
                        Code.Add(0x9d);
                        break;

                    case x86_cli:
                        Code.Add(0xfa);
                        break;

                    case x86_fstp_m64:
                        Code.Add(0xdd);
                        Code.AddRange(ModRMSIB(3, I.p[1].mreg));
                        break;

                    case x86_fld_m64:
                        Code.Add(0xdd);
                        Code.AddRange(ModRMSIB(0, I.p[1].mreg));
                        break;

                    default:
                        throw new NotImplementedException(insts[(int)I.p[0].v]);
                }

                if (tls_flag == 1)
                    throw new NotImplementedException("TLS not supported for " + insts[(int)I.p[0].v]);
                I.end_addr = Code.Count;
            }

            
            // Handle cil instructions which encode to nothing (e.g. nop) but may still be branch targets - point them to the next instruction
            int cur_il_start = -1;
            for (int i = 0; i < max_il; i++)
            {
                if (il_starts[i] == -1)
                    il_starts[i] = cur_il_start;
                else
                    cur_il_start = il_starts[i];
            }

            // Patch up references
            for (int i = 0; i < rel_srcs.Count; i++)
            {
                var src = rel_srcs[i];
                var dest = rel_dests[i];
                var dest_offset = il_starts[dest] + code_start;
                var offset = dest_offset - src - 4;
                InsertImm32(Code, offset, src);
            }
        }

        private IEnumerable<byte> TLSOverride(ref int tls_flag)
        {
            if (tls_flag == 0)
                return new byte[0];
            if(psize == 8)
            {
                // use %fs for TLS
                tls_flag = 0;
                return new byte[] { 0x64 };
            }
            throw new NotImplementedException();
        }

        private void AddRex(List<byte> code, byte v)
        {
            if (v != 0)
                code.Add(v);
        }

        private byte Rex(long oc, Reg r, Reg rm_ocreg, Reg sib_index = null, bool is_rm8 = false, bool is_r8 = false)
        {
            int rex = 0;
            if (oc >= x86_mov_r64_imm64)
                rex |= 0x8;
            if (r != null && r.id >= x86_64.x86_64_Assembler.r_r8.id)
                rex |= 0x4;
            if (rm_ocreg != null && rm_ocreg.id >= x86_64.x86_64_Assembler.r_r8.id)
                rex |= 0x1;
            if ((rm_ocreg is ContentsReg) && (((ContentsReg)rm_ocreg).basereg.id >= x86_64.x86_64_Assembler.r_r8.id))
                rex |= 0x1;
            if(sib_index != null && sib_index.id >= x86_64.x86_64_Assembler.r_r8.id)
                rex |= 0x2;
            if (is_rm8 && rm_ocreg != null && !(rm_ocreg is ContentsReg))
            {
                if (rm_ocreg.Equals(r_esp) ||
                    rm_ocreg.Equals(r_ebp) ||
                    rm_ocreg.Equals(r_edi) ||
                    rm_ocreg.Equals(r_esi))
                {
                    rex |= 0x40;
                }
            }
            if (is_r8 && r != null && !(r is ContentsReg))
            {
                if (r.Equals(r_esp) ||
                    r.Equals(r_ebp) ||
                    r.Equals(r_edi) ||
                    r.Equals(r_esi))
                {
                    rex |= 0x40;
                }
            }
            if (rex != 0)
            {
                if (psize == 4)
                    throw new Exception("Cannot encode rex prefix in ia32 mode");
                return (byte)(0x40 | rex);
            }
            return 0;
        }

        private void InsertImm32(List<byte> c, int v, int offset)
        {
            c[offset] = (byte)(v & 0xff);
            c[offset + 1] = (byte)((v >> 8) & 0xff);
            c[offset + 2] = (byte)((v >> 16) & 0xff);
            c[offset + 3] = (byte)((v >> 24) & 0xff);
        }

        private void AddImm64(List<byte> c, long v)
        {
            c.Add((byte)(v & 0xff));
            c.Add((byte)((v >> 8) & 0xff));
            c.Add((byte)((v >> 16) & 0xff));
            c.Add((byte)((v >> 24) & 0xff));
            c.Add((byte)((v >> 32) & 0xff));
            c.Add((byte)((v >> 40) & 0xff));
            c.Add((byte)((v >> 48) & 0xff));
            c.Add((byte)((v >> 56) & 0xff));
        }

        private void AddImm32(List<byte> c, long v)
        {
            c.Add((byte)(v & 0xff));
            c.Add((byte)((v >> 8) & 0xff));
            c.Add((byte)((v >> 16) & 0xff));
            c.Add((byte)((v >> 24) & 0xff));
        }

        private void AddImm16(List<byte> c, long v)
        {
            c.Add((byte)(v & 0xff));
            c.Add((byte)((v >> 8) & 0xff));
        }

        private void AddImm8(List<byte> c, long v)
        {
            c.Add((byte)(v & 0xff));
        }

        private IEnumerable<byte> ModRMSIB(Reg r, Reg rm, bool is_rm8 = false)
        {
            int r_val = GetR(r);
            int rm_val, mod_val, disp_len, disp_val;
            bool rm_is_ebp;
            GetModRM(rm, out rm_val, out mod_val, out disp_len, out disp_val, out rm_is_ebp);
            return ModRMSIB(r_val, rm_val, mod_val, -1, disp_len, disp_val, rm_is_ebp);
        }

        private IEnumerable<byte> ModRMSIB(int r, Target.Reg rm, bool is_rm8 = false)
        {
            int rm_val, mod_val, disp_len, disp_val;
            bool rm_is_ebp;
            GetModRM(rm, out rm_val, out mod_val, out disp_len, out disp_val, out rm_is_ebp);
            return ModRMSIB(r, rm_val, mod_val, -1, disp_len, disp_val, rm_is_ebp);
        }
        /* private IEnumerable<byte> ModRM(Reg r, Reg rm)
        {
            int r_val = GetR(r);
            int rm_val, mod_val, disp_len, disp_val;
            GetModRM(rm, out rm_val, out mod_val, out disp_len, out disp_val);
            return ModRM(r_val, rm_val, mod_val, disp_len, disp_val);
        } */

        private int GetR(Reg r)
        {
            if (r.Equals(r_eax))
                return 0;
            else if (r.Equals(r_ecx))
                return 1;
            else if (r.Equals(r_edx))
                return 2;
            else if (r.Equals(r_ebx))
                return 3;
            else if (r.Equals(r_esp))
                return 4;
            else if (r.Equals(r_ebp))
                return 5;
            else if (r.Equals(r_esi))
                return 6;
            else if (r.Equals(r_edi))
                return 7;
            else if (r.Equals(r_xmm0))
                return 0;
            else if (r.Equals(r_xmm1))
                return 1;
            else if (r.Equals(r_xmm2))
                return 2;
            else if (r.Equals(r_xmm3))
                return 3;
            else if (r.Equals(r_xmm4))
                return 4;
            else if (r.Equals(r_xmm5))
                return 5;
            else if (r.Equals(r_xmm6))
                return 6;
            else if (r.Equals(r_xmm7))
                return 7;
            else if (r.Equals(x86_64.x86_64_Assembler.r_r8))
                return 0;
            else if (r.Equals(x86_64.x86_64_Assembler.r_r9))
                return 1;
            else if (r.Equals(x86_64.x86_64_Assembler.r_r10))
                return 2;
            else if (r.Equals(x86_64.x86_64_Assembler.r_r11))
                return 3;
            else if (r.Equals(x86_64.x86_64_Assembler.r_r12))
                return 4;
            else if (r.Equals(x86_64.x86_64_Assembler.r_r13))
                return 5;
            else if (r.Equals(x86_64.x86_64_Assembler.r_r14))
                return 6;
            else if (r.Equals(x86_64.x86_64_Assembler.r_r15))
                return 7;
            else if (r.Equals(x86_64.x86_64_Assembler.r_xmm8))
                return 0;
            else if (r.Equals(x86_64.x86_64_Assembler.r_xmm9))
                return 1;
            else if (r.Equals(x86_64.x86_64_Assembler.r_xmm10))
                return 2;
            else if (r.Equals(x86_64.x86_64_Assembler.r_xmm11))
                return 3;
            else if (r.Equals(x86_64.x86_64_Assembler.r_xmm12))
                return 4;
            else if (r.Equals(x86_64.x86_64_Assembler.r_xmm13))
                return 5;
            else if (r.Equals(x86_64.x86_64_Assembler.r_xmm14))
                return 6;
            else if (r.Equals(x86_64.x86_64_Assembler.r_xmm15))
                return 7;

            throw new NotSupportedException();
        }

        private byte PlusRD(int v, Reg mreg)
        {
            if (mreg.Equals(r_eax))
                return (byte)(v + 0);
            else if (mreg.Equals(r_ecx))
                return (byte)(v + 1);
            else if (mreg.Equals(r_edx))
                return (byte)(v + 2);
            else if (mreg.Equals(r_ebx))
                return (byte)(v + 3);
            else if (mreg.Equals(r_esp))
                return (byte)(v + 4);
            else if (mreg.Equals(r_ebp))
                return (byte)(v + 5);
            else if (mreg.Equals(r_esi))
                return (byte)(v + 6);
            else if (mreg.Equals(r_edi))
                return (byte)(v + 7);
            else if (mreg.Equals(x86_64.x86_64_Assembler.r_r8))
                return (byte)(v + 0);
            else if (mreg.Equals(x86_64.x86_64_Assembler.r_r9))
                return (byte)(v + 1);
            else if (mreg.Equals(x86_64.x86_64_Assembler.r_r10))
                return (byte)(v + 2);
            else if (mreg.Equals(x86_64.x86_64_Assembler.r_r11))
                return (byte)(v + 3);
            else if (mreg.Equals(x86_64.x86_64_Assembler.r_r12))
                return (byte)(v + 4);
            else if (mreg.Equals(x86_64.x86_64_Assembler.r_r13))
                return (byte)(v + 5);
            else if (mreg.Equals(x86_64.x86_64_Assembler.r_r14))
                return (byte)(v + 6);
            else if (mreg.Equals(x86_64.x86_64_Assembler.r_r15))
                return (byte)(v + 7);

            throw new NotSupportedException();
        }

        /* private IEnumerable<byte> ModRM(int r, Target.Reg rm)
        {
            int rm_val, mod_val, disp_len, disp_val;
            GetModRM(rm, out rm_val, out mod_val, out disp_len, out disp_val);
            return ModRM(r, rm_val, mod_val, disp_len, disp_val);
        } */

        private void GetModRM(Reg rm, out int rm_val, out int mod_val, out int disp_len, out int disp_val, out bool rm_is_ebp)
        {
            if (rm is Target.ContentsReg)
            {
                var cr = rm as Target.ContentsReg;
                if (cr.disp == 0 && !cr.basereg.Equals(r_ebp))
                {
                    mod_val = 0;
                    disp_len = 0;
                }
                else if (cr.disp >= -128 && cr.disp < 127)
                {
                    mod_val = 1;
                    disp_len = 1;
                }
                else
                {
                    mod_val = 2;
                    disp_len = 4;
                }
                disp_val = (int)cr.disp;
                rm_val = GetRM(cr.basereg);
                rm_is_ebp = cr.basereg.Equals(r_ebp) || cr.basereg.Equals(x86_64.x86_64_Assembler.r_r13);
            }
            else
            {
                mod_val = 3;
                disp_len = 0;
                disp_val = 0;
                rm_val = GetRM(rm);
                rm_is_ebp = false;
            }
        }

        private int GetMod(Target.Reg rm)
        {
            if(rm is Target.ContentsReg)
            {
                var cr = rm as Target.ContentsReg;
                if (cr.disp == 0)
                    return 0;
                else if (cr.disp >= -128 && cr.disp < 127)
                    return 1;
                else
                    return 2;
            }
            return 3;
        }

        private int GetRM(Target.Reg rm)
        {
            if(rm is Target.ContentsReg)
            {
                var cr = rm as Target.ContentsReg;
                rm = cr.basereg;
            }

            if (rm.Equals(r_eax))
                return 0;
            else if (rm.Equals(r_ecx))
                return 1;
            else if (rm.Equals(r_edx))
                return 2;
            else if (rm.Equals(r_ebx))
                return 3;
            else if (rm.Equals(r_esp))
                return 4;
            else if (rm.Equals(r_ebp))
                return 5;
            else if (rm.Equals(r_esi))
                return 6;
            else if (rm.Equals(r_edi))
                return 7;
            else if (rm.Equals(r_xmm0))
                return 0;
            else if (rm.Equals(r_xmm1))
                return 1;
            else if (rm.Equals(r_xmm2))
                return 2;
            else if (rm.Equals(r_xmm3))
                return 3;
            else if (rm.Equals(r_xmm4))
                return 4;
            else if (rm.Equals(r_xmm5))
                return 5;
            else if (rm.Equals(r_xmm6))
                return 6;
            else if (rm.Equals(r_xmm7))
                return 7;
            else if (rm.Equals(x86_64.x86_64_Assembler.r_r8))
                return 0;
            else if (rm.Equals(x86_64.x86_64_Assembler.r_r9))
                return 1;
            else if (rm.Equals(x86_64.x86_64_Assembler.r_r10))
                return 2;
            else if (rm.Equals(x86_64.x86_64_Assembler.r_r11))
                return 3;
            else if (rm.Equals(x86_64.x86_64_Assembler.r_r12))
                return 4;
            else if (rm.Equals(x86_64.x86_64_Assembler.r_r13))
                return 5;
            else if (rm.Equals(x86_64.x86_64_Assembler.r_r14))
                return 6;
            else if (rm.Equals(x86_64.x86_64_Assembler.r_r15))
                return 7;
            else if (rm.Equals(x86_64.x86_64_Assembler.r_xmm8))
                return 0;
            else if (rm.Equals(x86_64.x86_64_Assembler.r_xmm9))
                return 1;
            else if (rm.Equals(x86_64.x86_64_Assembler.r_xmm10))
                return 2;
            else if (rm.Equals(x86_64.x86_64_Assembler.r_xmm11))
                return 3;
            else if (rm.Equals(x86_64.x86_64_Assembler.r_xmm12))
                return 4;
            else if (rm.Equals(x86_64.x86_64_Assembler.r_xmm13))
                return 5;
            else if (rm.Equals(x86_64.x86_64_Assembler.r_xmm14))
                return 6;
            else if (rm.Equals(x86_64.x86_64_Assembler.r_xmm15))
                return 7;

            if(rm.type == rt_stack)
            {
                var rm_reg = new ContentsReg
                {
                    basereg = r_esp,
                    disp = rm.stack_loc,
                    size = rm.size
                };
                return GetRM(rm_reg);
            }

            throw new NotSupportedException();
        }

        /* private IEnumerable<byte> ModRM(int r, int rm, int mod, int disp_len = 0, int disp = 0)
        {
            yield return (byte)(mod << 6 | r << 3 | rm);
            for (int i = 0; i < disp_len; i++)
                yield return (byte)(disp >> (8 * i));
        } */

        private IEnumerable<byte> ModRMSIB(int r, int rm, int mod,
            int index = -1, int disp_len = 0,
            int disp = 0, bool rm_is_ebp = true, int scale = -1)
        {
            /* catch the case where we're trying to do something to esp
                or ebp without an sib byte */

            int _base = -1;
            int ss = -1;
            bool has_sib = false;

            if (index >= 0)
            {
                _base = rm;
                has_sib = true;
                rm = 4;
                if (mod == 3)
                    throw new NotSupportedException("SIB addressing with mod == 3");
                if (scale == -1)
                    scale = 1;
            }
            else if (rm == 4 && mod != 3)
            {
                _base = 4;
                index = 4;
                has_sib = true;
            }
            else if(rm == 5 && mod == 0 && rm_is_ebp)
            {
                _base = 5;
                index = 4;
                ss = 0;
                if (disp_len == 0)
                {
                    disp = 0;
                    disp_len = 1;
                }
                has_sib = true;
            }

            if(disp_len == -1 && mod == 2)
            {
                if (disp == 0 && !rm_is_ebp)
                {
                    mod = 0;
                    disp_len = 0;
                }
                else if (disp >= sbyte.MinValue && disp <= sbyte.MaxValue)
                    disp_len = 1;
                else
                    disp_len = 4;
            }

            if (disp_len == 1)
                mod = 1;
            else if (disp_len == 4)
                mod = 2;

            if(rm == 5 && mod == 0)
            {
                // 
            }

            yield return (byte)(mod << 6 | r << 3 | rm);

            if(has_sib)
            {
                if(ss == -1)
                {
                    switch(scale)
                    {
                        case -1:
                        case 1:
                            ss = 0;
                            break;
                        case 2:
                            ss = 1;
                            break;
                        case 4:
                            ss = 2;
                            break;
                        case 8:
                            ss = 3;
                            break;
                        default:
                            throw new NotSupportedException("Invalid SIB scale: " + scale.ToString());
                    }
                }

                yield return (byte)(ss << 6 | index << 3 | _base);
            }

            for (int i = 0; i < disp_len; i++)
                yield return (byte)(disp >> (8 * i));
        }
    }
}
