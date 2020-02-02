/* Copyright (C) 2019 by John Cronin
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

namespace libtysila5.target.arm
{
    partial class arm_Assembler
    {
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

        /* Will the value 'v' fit into an immediate of length 'bits' ? */
        bool FitsBits(long v, int bits, bool signed = false)
        {
            long mask = (~0L) << bits;

            if (signed)
            {
                if ((v & mask) == 0)
                    return true;
                if ((v & mask) == mask)
                    return true;
                return false;
            }
            else
            {
                if ((v & mask) == 0)
                    return true;
                return false;
            }
        }

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

            foreach (var I in c.mc)
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

                    case arm_bl:
                        {
                            var target = I.p[3];
                            if (target.t == ir.Opcode.vl_call_target)
                            {
                                var reloc = bf.CreateRelocation();
                                reloc.DefinedIn = text_section;
                                reloc.Type = new binary_library.elf.ElfFile.Rel_Arm_Thm_Call();
                                reloc.Addend = 0;
                                reloc.References = bf.CreateSymbol();
                                reloc.References.Name = target.str;
                                reloc.References.ObjectType = binary_library.SymbolObjectType.Function;
                                reloc.Offset = (ulong)Code.Count;
                                bf.AddRelocation(reloc);

                                AddImm32(Code, 0xf000);
                                AddImm32(Code, 0xd000);
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                        }
                        break;

                    case arm_bx:
                        AddImm16(Code, (Rm(I) << 3) | 0x4700);
                        break;

                    case arm_ldm:
                        if(FitsBits(Rn(I), 3) &&
                            (FitsBits(Rlist(I), 8)) &&
                            (((W(I) == 1) && ((Rlist(I) & (1 << Rn(I))) == 0)) ||
                            ((W(I) == 0) && ((Rlist(I) & (1 << Rn(I))) != 0))))
                        {
                            // encoding T1
                            throw new NotImplementedException();
                        }
                        else
                        {
                            // encoding T2
                            AddImm16(Code, Rn(I) | (W(I) << 5) | 0xe890);
                            AddImm16(Code, Rlist(I));
                        }
                        break;

                    case arm_ldr_imm:
                        if ((Rn(I) == 13) && FitsBits(Rt(I), 3) && FitsBits(Imm(I), 10) && ((Imm(I) & 0x3) == 0) && W(I) == 0)
                        {
                            // encoding T2
                            throw new NotImplementedException();
                        }
                        else if (FitsBits(Rn(I), 3) && FitsBits(Rt(I), 3) && FitsBits(Imm(I), 7) && ((Imm(I) & 0x3) == 0) && W(I) == 0)
                        {
                            // encoding T1
                            throw new NotImplementedException();
                        }
                        else if (FitsBits(Imm(I), 12) && W(I) == 0)
                        {
                            // encoding T3
                            throw new NotImplementedException();
                        }
                        else if ((Imm(I) < 256) && (Imm(I) > -256))
                        {
                            // encoding T4
                            if (W(I) == 1)
                                throw new NotImplementedException();

                            int index = 1;
                            int wback = 0;
                            int val = Imm(I);
                            int add = 1;
                            if (val < 0)
                            {
                                val = -val;
                                add = 0;
                            }

                            AddImm16(Code, Rn(I) | 0xf850);
                            AddImm16(Code, val | (wback << 8) | (add << 9) | (index << 10) | (1 << 11) | (Rt(I) << 12));
                        }
                        else
                            throw new NotImplementedException();
                        break;


                    case arm_mov_reg:
                        AddImm16(Code, (Rd(I) & 0x7) | ((Rm(I) & 0xf) << 3) | (((Rd(I) >> 3) & 0x1) << 7) | 0x4600);
                        break;

                    case arm_push:
                        if(BitCount(Rlist(I)) == 0)
                        {
                            // NOP
                        }
                        if((Rlist(I) & 0xbf00) == 0)
                        {
                            // encoding T1
                            int m = (Rlist(I) >> 14) & 0x1;
                            AddImm16(Code, (Rlist(I) & 0xff) | (m << 8) | 0xb400);
                        }
                        else if(BitCount(Rlist(I)) > 1)
                        {
                            // encoding T2
                            throw new NotImplementedException();
                        }
                        else
                        {
                            // encoding T3
                            throw new NotImplementedException();
                        }
                        break;

                    case arm_pop:
                        if (BitCount(Rlist(I)) == 0)
                        {
                            // NOP
                        }
                        if ((Rlist(I) & 0x7f00) == 0)
                        {
                            // encoding T1
                            int p = (Rlist(I) >> 15) & 0x1;
                            AddImm16(Code, (Rlist(I) & 0xff) | (p << 8) | 0xbc00);
                        }
                        else if (BitCount(Rlist(I)) > 1)
                        {
                            // encoding T2
                            throw new NotImplementedException();
                        }
                        else
                        {
                            // encoding T3
                            throw new NotImplementedException();
                        }
                        break;


                    case arm_stmdb:
                        AddImm16(Code, Rn(I) | (W(I) << 5) | 0xe900);
                        AddImm16(Code, Rlist(I));
                        break;

                    case arm_str_imm:
                        if ((Rn(I) == 13) && FitsBits(Rt(I), 3) && FitsBits(Imm(I), 10) && ((Imm(I) & 0x3) == 0) && W(I) == 0)
                        {
                            // encoding T2
                            throw new NotImplementedException();
                        }
                        else if (FitsBits(Rn(I), 3) && FitsBits(Rt(I), 3) && FitsBits(Imm(I), 7) && ((Imm(I) & 0x3) == 0) && W(I) == 0)
                        {
                            // encoding T1
                            throw new NotImplementedException();
                        }
                        else if (FitsBits(Imm(I), 12) && W(I) == 0)
                        {
                            // encoding T3
                            throw new NotImplementedException();
                        }
                        else if((Imm(I) < 256) && (Imm(I) > -256))
                        {
                            // encoding T4
                            if (W(I) == 1)
                                throw new NotImplementedException();

                            int index = 1;
                            int wback = 0;
                            int val = Imm(I);
                            int add = 1;
                            if(val < 0)
                            {
                                val = -val;
                                add = 0;
                            }

                            AddImm16(Code, Rn(I) | 0xf840);
                            AddImm16(Code, val | (wback << 8) | (add << 9) | (index << 10) | (1 << 11) | (Rt(I) << 12));
                        }
                        else
                            throw new NotImplementedException();
                        break;

                    case arm_sub_imm:
                        if (FitsBits(Rn(I), 3) && FitsBits(Rd(I), 3) && FitsBits(Imm(I), 3))
                        {
                            // encoding T1
                            throw new NotImplementedException();
                        }
                        else if ((Rn(I) == Rd(I)) && FitsBits(Rn(I), 3) && FitsBits(Imm(I), 8))
                        {
                            // encoding T2
                            throw new NotImplementedException();
                        }
                        else if (FitsBits(Imm(I), 12))
                        {
                            // encoding T4
                            AddImm16(Code, Rn(I) | (((Imm(I) >> 11) & 0x1) << 10) | 0xf2a0);
                            AddImm16(Code, (Imm(I) & 0xf) | (Rd(I) << 8) | (((Imm(I) >> 8) & 0x7) << 12));
                        }
                        else
                            throw new NotImplementedException();
                        break;

                    default:
                        throw new NotImplementedException(insts[(int)I.p[0].v]);

                }
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
                throw new NotImplementedException();
                //InsertImm32(Code, offset, src);
            }
        }

        private int BitCount(int v)
        {
            int cnt = 0;
            for(int i = 0; i < 32; i++)
            {
                cnt += (v & 0x1);
                v >>= 1;
            }
            return cnt;
        }

        private int Rn(MCInst i)
        {
            return i.p[1].mreg.id;
        }

        private int Rd(MCInst i)
        {
            return i.p[2].mreg.id;
        }

        private int Rm(MCInst i)
        {
            return i.p[3].mreg.id;
        }

        private int Rt(MCInst i)
        {
            return i.p[4].mreg.id;
        }

        private int W(MCInst i)
        {
            return (int)i.p[6].v;
        }

        private int Imm(MCInst i)
        {
            return (int)i.p[5].v;
        }

        private int Rlist(MCInst i)
        {
            return (int)i.p[8].v;
        }
    }
}

