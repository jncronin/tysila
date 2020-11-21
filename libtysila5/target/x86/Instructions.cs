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
using libtysila5.cil;

namespace libtysila5.target.x86
{
    partial class x86_Assembler
    {
        private static void handle_briff(Reg srca, Reg srcb, int cc, int target, List<MCInst> r, CilNode.IRNode n)
        {
            if (srca is ContentsReg && !(srcb is ContentsReg))
                cc = ir.Opcode.cc_invert_map[cc];

            int oc;
            
            switch(cc)
            {
                case ir.Opcode.cc_a:
                    cc = ir.Opcode.cc_a;
                    oc = x86_ucomisd_xmm_xmmm64;
                    break;
                case ir.Opcode.cc_ae:
                    cc = ir.Opcode.cc_ae;
                    oc = x86_ucomisd_xmm_xmmm64;
                    break;
                case ir.Opcode.cc_b:
                    cc = ir.Opcode.cc_b;
                    oc = x86_ucomisd_xmm_xmmm64;
                    break;
                case ir.Opcode.cc_be:
                    cc = ir.Opcode.cc_be;
                    oc = x86_ucomisd_xmm_xmmm64;
                    break;
                case ir.Opcode.cc_eq:
                    cc = ir.Opcode.cc_eq;
                    oc = x86_comisd_xmm_xmmm64;
                    break;
                case ir.Opcode.cc_ge:
                    cc = ir.Opcode.cc_ae;
                    oc = x86_comisd_xmm_xmmm64;
                    break;
                case ir.Opcode.cc_gt:
                    cc = ir.Opcode.cc_a;
                    oc = x86_comisd_xmm_xmmm64;
                    break;
                case ir.Opcode.cc_le:
                    cc = ir.Opcode.cc_be;
                    oc = x86_comisd_xmm_xmmm64;
                    break;
                case ir.Opcode.cc_lt:
                    cc = ir.Opcode.cc_b;
                    oc = x86_comisd_xmm_xmmm64;
                    break;
                case ir.Opcode.cc_ne:
                    cc = ir.Opcode.cc_ne;
                    oc = x86_comisd_xmm_xmmm64;
                    break;
                default:
                    throw new NotImplementedException();
            }
            if (!(srca is ContentsReg))
                r.Add(inst(oc, srca, srcb, n));
            else if (!(srcb is ContentsReg))
            {
                r.Add(inst(oc, srcb, srca, n));
                cc = ir.Opcode.cc_invert_map[cc];
            }
            else
            {
                r.Add(inst(x86_movsd_xmm_xmmm64, r_xmm7, srca, n));
                r.Add(inst(oc, srca, srcb, n));
            }
            r.Add(inst_jmp(x86_jcc_rel32, target, cc, n));
        }

        private void extern_append(StringBuilder sb, int ct)
        {
            switch (ct)
            {
                case ir.Opcode.ct_int32:
                case ir.Opcode.ct_object:
                case ir.Opcode.ct_ref:
                case ir.Opcode.ct_intptr:
                    sb.Append("i");
                    break;
                case ir.Opcode.ct_int64:
                    sb.Append("l");
                    break;
                case ir.Opcode.ct_float:
                    sb.Append("f");
                    break;
            }
        }

        private static void handle_stind(Reg val, Reg addr, int disp, int vt_size, List<MCInst> r, CilNode.IRNode n, Code c, bool is_tls = false)
        {
            if (is_tls)
                throw new NotImplementedException();
            if (addr is ContentsReg || ((addr.Equals(r_esi) || addr.Equals(r_edi)) && vt_size != 4))
            {
                r.Add(inst(x86_mov_r32_rm32, r_eax, addr, n));
                addr = r_eax;
            }

            var act_val = val;

            if (val is ContentsReg || ((val.Equals(r_esi) || val.Equals(r_edi)) && n.vt_size == 1))
            {
                val = r_edx;
                r.Add(inst(x86_mov_r32_rm32, val, act_val, n));
            }

            if((addr.Equals(r_esi) || addr.Equals(r_edi)) && n.vt_size == 1 && c.t.psize == 4)
            {
                throw new NotImplementedException();
            }

            switch (vt_size)
            {
                case 1:
                    r.Add(inst(x86_mov_rm8disp_r8, addr, disp, val, n));
                    break;
                case 2:
                    r.Add(inst(x86_mov_rm16disp_r16, addr, disp, val, n));
                    break;
                case 4:
                    r.Add(inst(x86_mov_rm32disp_r32, addr, disp, val, n));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private static void handle_brifi32(Reg srca, Reg srcb, int cc, int target, List<MCInst> r, CilNode.IRNode n)
        {
            if (!(srca is ContentsReg))
                r.Add(inst(x86_cmp_r32_rm32, srca, srcb, n));
            else if (!(srcb is ContentsReg))
                r.Add(inst(x86_cmp_rm32_r32, srca, srcb, n));
            else
            {
                r.Add(inst(x86_mov_r32_rm32, r_eax, srca, n));
                r.Add(inst(x86_cmp_r32_rm32, r_eax, srcb, n));
            }
            r.Add(inst_jmp(x86_jcc_rel32, target, cc, n));
        }

        private static void handle_brifi64(Reg srca, Reg srcb, int cc, int target, List<MCInst> r, CilNode.IRNode n)
        {
            if (!(srca is ContentsReg))
                r.Add(inst(x86_cmp_r64_rm64, srca, srcb, n));
            else if (!(srcb is ContentsReg))
                r.Add(inst(x86_cmp_rm64_r64, srca, srcb, n));
            else
            {
                r.Add(inst(x86_mov_r64_rm64, r_eax, srca, n));
                r.Add(inst(x86_cmp_r64_rm64, r_eax, srcb, n));
            }
            r.Add(inst_jmp(x86_jcc_rel32, target, cc, n));
        }

        private static void handle_brifi32(Reg srca, long v, int cc, int target, List<MCInst> r, CilNode.IRNode n)
        {
            if (v >= -128 && v < 127)
                r.Add(inst(x86_cmp_rm32_imm8, srca, v, n));
            else if (v >= int.MinValue && v <= int.MaxValue)
                r.Add(inst(x86_cmp_rm32_imm32, srca, v, n));
            else
                throw new NotImplementedException();
            r.Add(inst_jmp(x86_jcc_rel32, target, cc, n));
        }

        protected static void handle_move(Reg dest, Reg src, List<MCInst> r, CilNode.IRNode n, Code c, Reg temp_reg = null, int size = -1,
            bool src_is_tls = false, bool dest_is_tls = false)
        {
            if (src_is_tls || dest_is_tls)
                throw new NotImplementedException();
            if (dest.Equals(src))
                return;
            if (src is ContentsReg && dest is ContentsReg)
            {
                var crs = src as ContentsReg;
                var crd = dest as ContentsReg;

                var vt_size = crs.size;
                vt_size = util.util.align(vt_size, c.t.psize);
                if (vt_size != util.util.align(crd.size, c.t.psize))
                    throw new Exception("Differing size in move");

                if (temp_reg == null)
                {
                    if (crs.basereg.Equals(r_eax) || crd.basereg.Equals(r_eax))
                        temp_reg = r_edx;
                    else
                        temp_reg = r_eax;
                }

                if (vt_size > 4 * c.t.psize)
                {
                    // emit call to memcpy(dest, src, n)
                    r.AddRange(handle_call(n, c,
                        c.special_meths.GetMethodSpec(c.special_meths.memcpy),
                        new ir.Param[]
                        {
                            new ir.Param { t = ir.Opcode.vl_mreg, mreg = dest, want_address = true },
                            new ir.Param { t = ir.Opcode.vl_mreg, mreg = src, want_address = true },
                            vt_size
                        },
                        null, "memcpy", temp_reg));
                }
                else if(c.t.psize == 4)
                {
                    for (int i = 0; i < vt_size; i += 4)
                    {
                        var new_crs = new ContentsReg { basereg = crs.basereg, disp = crs.disp + i, size = 4 };
                        var new_crd = new ContentsReg { basereg = crd.basereg, disp = crd.disp + i, size = 4 };

                        // first store to rax
                        r.Add(inst(x86_mov_r32_rm32, temp_reg, new_crs, n));
                        r.Add(inst(x86_mov_rm32_r32, new_crd, temp_reg, n));
                    }
                }
                else
                {
                    for (int i = 0; i < vt_size; i += 8)
                    {
                        var new_crs = new ContentsReg { basereg = crs.basereg, disp = crs.disp + i, size = 8 };
                        var new_crd = new ContentsReg { basereg = crd.basereg, disp = crd.disp + i, size = 8 };

                        // first store to rax
                        r.Add(inst(x86_mov_r64_rm64, temp_reg, new_crs, n));
                        r.Add(inst(x86_mov_rm64_r64, new_crd, temp_reg, n));
                    }
                }
            }
            else if (src is ContentsReg)
            {
                if (dest.type == rt_gpr)
                {
                    if (src.size == 4 || size == 4)
                        r.Add(inst(x86_mov_r32_rm32, dest, src, n));
                    else if (c.t.psize == 8)
                        r.Add(inst(x86_mov_r64_rm64, dest, src, n));
                    else
                        throw new NotImplementedException();
                }
                else if (dest.type == rt_float)
                    r.Add(inst(x86_movsd_xmm_xmmm64, dest, src, n));
                else
                    throw new NotImplementedException();
            }
            else
            {
                if (src.type == rt_gpr)
                {
                    if (c.t.psize == 4)
                        r.Add(inst(x86_mov_rm32_r32, dest, src, n));
                    else
                        r.Add(inst(x86_mov_rm64_r64, dest, src, n));
                }
                else if (src.type == rt_float)
                {
                    if (src.Equals(r_st0))
                    {
                        // Need to copy via the stack
                        r.Add(inst(x86_fstp_m64, new ContentsReg { basereg = r_esp, disp = -8, size = 8 }, n));
                        r.Add(inst(x86_movsd_xmm_xmmm64, dest, new ContentsReg { basereg = r_esp, disp = -8, size = 8 }, n));
                    }
                    else
                    {
                        r.Add(inst(x86_movsd_xmmm64_xmm, dest, src, n));
                    }
                }
                else if (src.type == rt_multi)
                {
                    for (int i = 0; i < dest.size; i += c.t.psize)
                    {
                        var sa = src.SubReg(i, c.t.psize, c.t);
                        var da = dest.SubReg(i, c.t.psize, c.t);
                        handle_move(da, sa, r, n, c, temp_reg);
                    }
                }
                else
                    throw new NotImplementedException();
            }
        }

        private static void handle_sub(Target t, Reg srca, Reg srcb, Reg dest, List<MCInst> r, CilNode.IRNode n, bool with_borrow = false)
        {
            if (!(srca is ContentsReg) && srca.Equals(dest))
            {
                if (with_borrow)
                    r.Add(inst(t.psize == 4 ? x86_sbb_r32_rm32 : x86_sbb_r64_rm64, srca, srcb, n));
                else
                    r.Add(inst(t.psize == 4 ? x86_sub_r32_rm32 : x86_sub_r64_rm64, srca, srcb, n));
            }
            else if (!(srcb is ContentsReg) && srca.Equals(dest))
            {
                if (with_borrow)
                    r.Add(inst(t.psize == 4 ? x86_sbb_rm32_r32 : x86_sbb_rm64_r64, srca, srcb, n));
                else
                    r.Add(inst(t.psize == 4 ? x86_sub_rm32_r32 : x86_sub_rm64_r64, srca, srcb, n));
            }
            else
            {
                // complex way, do calc in rax, then store
                r.Add(inst(t.psize == 4 ? x86_mov_r32_rm32 : x86_mov_r64_rm64, r_eax, srca, n));
                if (with_borrow)
                    r.Add(inst(t.psize == 4 ? x86_sbb_r32_rm32 : x86_sbb_r64_rm64, r_eax, srcb, n));
                else
                    r.Add(inst(t.psize == 4 ? x86_sub_r32_rm32 : x86_sub_r64_rm64, r_eax, srcb, n));
                r.Add(inst(t.psize == 4 ? x86_mov_rm32_r32 : x86_mov_rm64_r64, dest, r_eax, n));
            }
        }

        private static void handle_add(Target t, Reg srca, Reg srcb, Reg dest, List<MCInst> r, CilNode.IRNode n, bool with_carry = false)
        {
            if (!(srca is ContentsReg) && srca.Equals(dest))
            {
                if (with_carry)
                    r.Add(inst(t.psize == 4 ? x86_adc_r32_rm32 : x86_adc_r64_rm64, srca, srcb, n));
                else
                    r.Add(inst(t.psize == 4 ? x86_add_r32_rm32 : x86_add_r64_rm64, srca, srcb, n));
            }
            else if (!(srcb is ContentsReg) && srca.Equals(dest))
            {
                if (with_carry)
                    r.Add(inst(t.psize == 4 ? x86_adc_rm32_r32 : x86_adc_rm64_r64, srca, srcb, n));
                else
                    r.Add(inst(t.psize == 4 ? x86_add_rm32_r32 : x86_add_rm64_r64, srca, srcb, n));
            }
            else
            {
                // complex way, do calc in rax, then store
                r.Add(inst(t.psize == 4 ? x86_mov_r32_rm32 : x86_mov_r64_rm64, r_eax, srca, n));
                if (with_carry)
                    r.Add(inst(t.psize == 4 ? x86_adc_r32_rm32 : x86_adc_r64_rm64, r_eax, srcb, n));
                else
                    r.Add(inst(t.psize == 4 ? x86_add_r32_rm32 : x86_add_r64_rm64, r_eax, srcb, n));
                r.Add(inst(t.psize == 4 ? x86_mov_rm32_r32 : x86_mov_rm64_r64, dest, r_eax, n));
            }
        }

        private static void handle_and(Reg srca, Reg srcb, Reg dest, List<MCInst> r, CilNode.IRNode n)
        {
            if (srca.size == 4)
            {
                if (!(srca is ContentsReg) && srca.Equals(dest))
                {
                    r.Add(inst(x86_and_r32_rm32, srca, srcb, n));
                }
                else if (!(srcb is ContentsReg) && srca.Equals(dest))
                {
                    r.Add(inst(x86_and_rm32_r32, srca, srcb, n));
                }
                else
                {
                    // complex way, do calc in rax, then store
                    r.Add(inst(x86_mov_r32_rm32, r_eax, srca, n));
                    r.Add(inst(x86_and_r32_rm32, r_eax, srcb, n));
                    r.Add(inst(x86_mov_rm32_r32, dest, r_eax, n));
                }
            }
            else
            {
                if (!(srca is ContentsReg) && srca.Equals(dest))
                {
                    r.Add(inst(x86_and_r64_rm64, srca, srcb, n));
                }
                else if (!(srcb is ContentsReg) && srca.Equals(dest))
                {
                    r.Add(inst(x86_and_rm64_r64, srca, srcb, n));
                }
                else
                {
                    // complex way, do calc in rax, then store
                    r.Add(inst(x86_mov_r64_rm64, r_eax, srca, n));
                    r.Add(inst(x86_and_r64_rm64, r_eax, srcb, n));
                    r.Add(inst(x86_mov_rm64_r64, dest, r_eax, n));
                }
            }
        }

        private static void handle_or(Reg srca, Reg srcb, Reg dest, List<MCInst> r, CilNode.IRNode n)
        {
            if (srca.size == 4)
            {
                if (!(srca is ContentsReg) && srca.Equals(dest))
                {
                    r.Add(inst(x86_or_r32_rm32, srca, srcb, n));
                }
                else if (!(srcb is ContentsReg) && srca.Equals(dest))
                {
                    r.Add(inst(x86_or_rm32_r32, srca, srcb, n));
                }
                else
                {
                    // complex way, do calc in rax, then store
                    r.Add(inst(x86_mov_r32_rm32, r_eax, srca, n));
                    r.Add(inst(x86_or_r32_rm32, r_eax, srcb, n));
                    r.Add(inst(x86_mov_rm32_r32, dest, r_eax, n));
                }
            }
            else
            {
                if (!(srca is ContentsReg) && srca.Equals(dest))
                {
                    r.Add(inst(x86_or_r64_rm64, srca, srcb, n));
                }
                else if (!(srcb is ContentsReg) && srca.Equals(dest))
                {
                    r.Add(inst(x86_or_rm64_r64, srca, srcb, n));
                }
                else
                {
                    // complex way, do calc in rax, then store
                    r.Add(inst(x86_mov_r64_rm64, r_eax, srca, n));
                    r.Add(inst(x86_or_r64_rm64, r_eax, srcb, n));
                    r.Add(inst(x86_mov_rm64_r64, dest, r_eax, n));
                }
            }
        }

        private static void handle_xor(Reg srca, Reg srcb, Reg dest, List<MCInst> r, CilNode.IRNode n)
        {
            if (srca.size == 4)
            {
                if (!(srca is ContentsReg) && srca.Equals(dest))
                {
                    r.Add(inst(x86_xor_r32_rm32, srca, srcb, n));
                }
                else if (!(srcb is ContentsReg) && srca.Equals(dest))
                {
                    r.Add(inst(x86_xor_rm32_r32, srca, srcb, n));
                }
                else
                {
                    // complex way, do calc in rax, then store
                    r.Add(inst(x86_mov_r32_rm32, r_eax, srca, n));
                    r.Add(inst(x86_xor_r32_rm32, r_eax, srcb, n));
                    r.Add(inst(x86_mov_rm32_r32, dest, r_eax, n));
                }
            }
            else
            {
                if (!(srca is ContentsReg) && srca.Equals(dest))
                {
                    r.Add(inst(x86_xor_r64_rm64, srca, srcb, n));
                }
                else if (!(srcb is ContentsReg) && srca.Equals(dest))
                {
                    r.Add(inst(x86_xor_rm64_r64, srca, srcb, n));
                }
                else
                {
                    // complex way, do calc in rax, then store
                    r.Add(inst(x86_mov_r64_rm64, r_eax, srca, n));
                    r.Add(inst(x86_xor_r64_rm64, r_eax, srcb, n));
                    r.Add(inst(x86_mov_rm64_r64, dest, r_eax, n));
                }
            }
        }

        private static void handle_ldind(Reg val, Reg addr, int disp, int vt_size,
            List<MCInst> r, CilNode.IRNode n, Target t, bool is_tls = false)
        {
            if (addr is ContentsReg)
            {
                r.Add(inst(t.psize == 4 ? x86_mov_r32_rm32 : x86_mov_r64_rm64, r_eax, addr, n));
                addr = r_eax;
            }

            var act_val = val;

            if (val is ContentsReg)
            {
                val = r_edx;
            }

            switch (vt_size)
            {
                case 1:
                    if (n.imm_l == 1)
                        r.Add(inst(t.psize == 4 ? x86_movsxbd_r32_rm8disp : x86_movsxbq_r64_rm8disp, val, addr, disp, n, is_tls));
                    else
                        r.Add(inst(t.psize == 4 ? x86_movzxbd_r32_rm8disp : x86_movzxbq_r64_rm8disp, val, addr, disp, n, is_tls));
                    break;
                case 2:
                    if (n.imm_l == 1)
                        r.Add(inst(t.psize == 4 ? x86_movsxwd_r32_rm16disp : x86_movsxwq_r64_rm16disp, val, addr, disp, n, is_tls));
                    else
                        r.Add(inst(t.psize == 4 ? x86_movzxwd_r32_rm16disp : x86_movzxwq_r64_rm16disp, val, addr, disp, n, is_tls));
                    break;
                case 4:
                    r.Add(inst(x86_mov_r32_rm32disp, val, addr, disp, n, is_tls));
                    break;
                case 8:
                    if (t.psize != 8)
                        throw new NotImplementedException();
                    r.Add(inst(x86_mov_r64_rm64disp, val, addr, disp, n, is_tls));
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (!val.Equals(act_val))
            {
                r.Add(inst(t.psize == 4 ? x86_mov_rm32_r32 : x86_mov_rm64_r64, act_val, val, n));
            }
        }

        private static List<MCInst> handle_ret(CilNode.IRNode n, Code c, Target t)
        {
            List<MCInst> r = new List<MCInst>();

            /* Put a local label here so we can jmp here at will */
            int ret_lab = c.cctor_ret_tag;
            if(ret_lab == -1)
            {
                ret_lab = c.next_mclabel--;
                c.cctor_ret_tag = ret_lab;
            }
            r.Add(inst(Generic.g_mclabel, new ir.Param { t = ir.Opcode.vl_br_target, v = ret_lab }, n));

            if (n.stack_before.Count == 1)
            {
                var reg = n.stack_before.Peek().reg;

                switch (n.ct)
                {
                    case ir.Opcode.ct_int32:
                    case ir.Opcode.ct_intptr:
                    case ir.Opcode.ct_object:
                    case ir.Opcode.ct_ref:
                        r.Add(inst(t.psize == 4 ? x86_mov_r32_rm32 : x86_mov_r64_rm64, r_eax, n.stack_before[0].reg, n));
                        break;

                    case ir.Opcode.ct_int64:
                        if (t.psize == 4)
                        {
                            var dra = reg.SubReg(0, 4, c.t);
                            var drb = reg.SubReg(4, 4, c.t);
                            r.Add(inst(x86_mov_r32_rm32, r_eax, dra, n));
                            r.Add(inst(x86_mov_r32_rm32, r_edx, drb, n));
                        }
                        else
                            r.Add(inst(x86_mov_r64_rm64, r_eax, reg, n));
                        break;

                    case ir.Opcode.ct_vt:
                        // move address to save to to eax
                        handle_move(r_eax, new ContentsReg { basereg = r_ebp, disp = -t.psize, size = t.psize },
                            r, n, c);

                        // move struct to [eax]
                        var vt_size = c.t.GetSize(c.ret_ts);
                        handle_move(new ContentsReg { basereg = r_eax, size = vt_size },
                            reg, r, n, c);

                        break;

                    case ir.Opcode.ct_float:
                        if(t.psize == 4)
                        {
                            // move to st0
                            throw new NotImplementedException();
                        }
                        else
                            handle_move(r_xmm0, reg, r, n, c);
                        break;

                    default:
                        throw new NotImplementedException(ir.Opcode.ct_names[n.ct]);
                }
            }

            // Restore used regs

            if (!n.parent.is_in_excpt_handler)
            {
                // we use values relative to rbp here in case the function
                // did a localloc which has changed the stack pointer
                for (int i = 0; i < c.regs_saved.Count; i++)
                {
                    ContentsReg cr = new ContentsReg { basereg = r_ebp, disp = -c.lv_total_size - c.stack_total_size - t.psize * (i + 1), size = t.psize };
                    handle_move(c.regs_saved[i], cr, r, n, c);
                }
            }
            else
            {
                // in exception handler we have to pop the values off the stack
                // because rsp is not by default restored so we may return to the
                // wrong place

                // this works because we do not allow localloc in exception handlers
                int x = 0;
                for (int i = c.regs_saved.Count - 1; i >= 0; i--)
                    handle_pop(c.regs_saved[i], ref x, r, n, c);
            }

            if(!n.parent.is_in_excpt_handler)
                r.Add(inst(t.psize == 4 ? x86_mov_r32_rm32 : x86_mov_r64_rm64, r_esp, r_ebp, n));
            r.Add(inst(x86_pop_r32, r_ebp, n));

            // Insert a code sequence here if this is a static constructor
            if(c.is_cctor)
            {
                /* Sequence is:
                 * 
                 * Load static field pointer
                 *      mov rcx, lab_addr
                 * 
                 * Set done flag
                 *      mov_rm8_imm8 [rcx], 2
                 *      
                 * Get return eip
                 *      mov_r32_rm32/64 rcx, [rsp]
                 *      
                 * Is this cctor was called by a direct call (i.e. [rcx-6] == 0xe8) then:
                 * 
                 * Overwrite the preceeding 5 bytes with nops
                 *      mov_rm32_imm32 [rcx - 5], 0x00401f0f ; 4 byte nop
                 *      mov_rm8_imm8 [rcx - 1], 0x90 ; 1 byte nop
                 *      
                 * Else skip to here (this is the case where we were called indirectly from
                 *  System.Runtime.CompilerServices._RunClassConstructor())
                 */
                
                r.Add(inst(x86_mov_rm32_imm32, r_ecx, new ir.Param { t = ir.Opcode.vl_str, str = c.ms.type.MangleType() + "S" }, n));
                r.Add(inst(x86_mov_rm8disp_imm32, r_ecx, 0, 2, n));
                r.Add(inst(t.psize == 4 ? x86_mov_r32_rm32 : x86_mov_r64_rm64, r_ecx, new ContentsReg { basereg = r_esp }, n));
                r.Add(inst(x86_cmp_rm8_imm8, new ContentsReg { basereg = r_ecx, disp = -6, size = 1 }, 0xe8, n));
                int end_lab = c.next_mclabel--;
                r.Add(inst_jmp(x86_jcc_rel32, end_lab, ir.Opcode.cc_ne, n));
                //r.Add(inst(x86_mov_rm32disp_imm32, r_ecx, -5, 0x00401f0f, n));
                r.Add(inst(x86_mov_rm8disp_imm32, r_ecx, -1, 0x90, n));
                r.Add(inst(x86_mov_rm8disp_imm32, r_ecx, -2, 0x90, n));
                r.Add(inst(x86_mov_rm8disp_imm32, r_ecx, -3, 0x90, n));
                r.Add(inst(x86_mov_rm8disp_imm32, r_ecx, -4, 0x90, n));
                r.Add(inst(x86_mov_rm8disp_imm32, r_ecx, -5, 0x90, n));
                r.Add(inst(Generic.g_mclabel, new ir.Param { t = ir.Opcode.vl_br_target, v = end_lab }, n));
            }

            if (c.ms.CallingConvention == "isrec")
            {
                // pop error code from stack
                r.Add(inst(t.psize == 4 ? x86_add_rm32_imm8 : x86_add_rm64_imm8,
                    r_esp, 8, n));
            }
            if (c.ms.CallingConvention == "isr" || c.ms.CallingConvention == "isrec")
            {
                r.Add(inst(t.psize == 4 ? x86_iret : x86_iretq, n));
            }
            else
            {
                r.Add(inst(x86_ret, n));
            }

            return r;
        }

        static List<Reg> get_push_list(CilNode.IRNode n, Code c,
            metadata.MethodSpec call_ms, out metadata.TypeSpec rt,
            ref Reg dest, out int rct, bool want_return = true)
        {
            /* Determine which registers we need to save */
            var caller_preserves = c.t.cc_caller_preserves_map[call_ms.CallingConvention];
            ulong defined = 0;
            foreach (var si in n.stack_after)
                defined |= si.reg.mask;
            foreach (var si in n.stack_before)
                defined |= si.reg.mask;

            var rt_idx = call_ms.m.GetMethodDefSigRetTypeIndex(call_ms.msig);
            rt = call_ms.m.GetTypeSpec(ref rt_idx, call_ms.gtparams, call_ms.gmparams);
            rct = ir.Opcode.ct_unknown;
            if (rt != null && want_return)
            {
                defined &= ~n.stack_after.Peek().reg.mask;
                if(dest == null)
                    dest = n.stack_after.Peek().reg;
                rct = ir.Opcode.GetCTFromType(rt);
            }

            var to_push = new util.Set();
            to_push.Union(defined);
            to_push.Intersect(caller_preserves);
            List<Reg> push_list = new List<Reg>();
            while (!to_push.Empty)
            {
                var first_set = to_push.get_first_set();
                push_list.Add(c.t.regs[first_set]);
                to_push.unset(first_set);
            }

            return push_list;
        }

        private static List<MCInst> handle_call(CilNode.IRNode n,
            Code c, metadata.MethodSpec call_ms, ir.Param[] p,
            Reg dest, string target = null, Reg temp_reg = null)
            
        {
            /* used for handling calls to utility functions
             *  (e.g. memcpy/memset etc) whilst ensuring that
             *  all required registers are saved around the
             *  call */

            return handle_call(n, c, false, c.t, target, call_ms, p, dest, dest != null);
        }

        private static List<MCInst> handle_call(CilNode.IRNode n, Code c, bool is_calli, Target t,
            string target = null, metadata.MethodSpec call_ms = null,
            ir.Param[] p = null, Reg dest = null, bool want_return = true)
        {
            List<MCInst> r = new List<MCInst>();
            if(call_ms == null)
                call_ms = n.imm_ms;
            if(target == null && is_calli == false)
                target = call_ms.m.MangleMethod(call_ms);

            Reg act_dest = null;
            metadata.TypeSpec rt;
            int rct;
            var push_list = get_push_list(n, c, call_ms,
                out rt, ref dest, out rct, want_return);

            // Store the current index, we will insert instructions
            //  to save clobbered registers here
            int push_list_index = r.Count;

            /* Push arguments */
            int push_length = 0;
            int vt_push_length = 0;
            bool vt_dest_adjust = false;

            if (rct == ir.Opcode.ct_vt)
            {
                if (dest is ContentsReg)
                {
                    act_dest = dest;
                }
                else
                {
                    throw new NotImplementedException();
                    var rsize = c.t.GetSize(rt);
                    rsize = util.util.align(rsize, 4);
                    vt_dest_adjust = true;

                    act_dest = new ContentsReg { basereg = r_esp, disp = 0, size = rsize };
                    r.Add(inst(rsize <= 127 ? x86_sub_rm32_imm8 : x86_sub_rm32_imm32, r_esp, rsize, n));
                    vt_push_length += rsize;
                }
            }

            var sig_idx = call_ms.msig;
            var pcount = call_ms.m.GetMethodDefSigParamCountIncludeThis(sig_idx);
            sig_idx = call_ms.m.GetMethodDefSigRetTypeIndex(sig_idx);
            var rt2 = call_ms.m.GetTypeSpec(ref sig_idx, call_ms.gtparams == null ? c.ms.gtparams : call_ms.gtparams, c.ms.gmparams);

            int calli_adjust = is_calli ? 1 : 0;

            metadata.TypeSpec[] push_tss = new metadata.TypeSpec[pcount];
            for (int i = 0; i < pcount; i++)
            {
                if (i == 0 && call_ms.m.GetMethodDefSigHasNonExplicitThis(call_ms.msig))
                    push_tss[i] = call_ms.type;
                else
                    push_tss[i] = call_ms.m.GetTypeSpec(ref sig_idx, call_ms.gtparams, call_ms.gmparams);
            }

            /* Push value type address if required */
            metadata.TypeSpec hidden_loc_type = null;
            if (rct == ir.Opcode.ct_vt)
            {
                var act_dest_cr = act_dest as ContentsReg;
                if(act_dest_cr == null)
                    throw new NotImplementedException();

                if (vt_dest_adjust)
                {
                    throw new NotImplementedException();
                    act_dest_cr.disp += push_length;
                }


                //r.Add(inst(x86_lea_r32, r_eax, act_dest, n));
                //r.Add(inst(x86_push_r32, r_eax, n));
                hidden_loc_type = call_ms.m.SystemIntPtr;
            }

            // Build list of source and destination registers for parameters
            int cstack_loc = 0;
            var cc = t.cc_map[call_ms.CallingConvention];
            var cc_class = t.cc_classmap[call_ms.CallingConvention];
            int[] la_sizes;
            metadata.TypeSpec[] la_types;
            var to_locs = t.GetRegLocs(new ir.Param
            {
                m = call_ms.m,
                v2 = call_ms.msig,
                ms = call_ms
            },
                ref cstack_loc, cc, cc_class, call_ms.CallingConvention,
                out la_sizes, out la_types,
                hidden_loc_type
            );

            Reg calli_reg = null;
            if(is_calli)
            {
                if (t.psize == 4)
                    calli_reg = r_edx;  // not used as a parameter register on ia32
                else
                    calli_reg = x86_64.x86_64_Assembler.r_r15;

                // Add the target register to those we want to pass
                Reg[] new_to_locs = new Reg[to_locs.Length + calli_adjust];
                for (int i = 0; i < to_locs.Length; i++)
                    new_to_locs[i] = to_locs[i];
                new_to_locs[to_locs.Length] = calli_reg;

                to_locs = new_to_locs;
            }

            // Append the register arguments to the push list
            foreach (var arg in to_locs)
            {
                if (arg.type == rt_gpr || arg.type == rt_float)
                {
                    if (!push_list.Contains(arg))
                        push_list.Add(arg);
                }
            }
            List<MCInst> r2 = new List<MCInst>();

            // Insert the push instructions at the start of the stream
            int x = 0;
            foreach (var push_reg in push_list)
                handle_push(push_reg, ref x, r2, n, c);
            foreach (var r2inst in r2)
                r.Insert(push_list_index++, r2inst);

            // Get from locs

            ir.Param[] from_locs;
            int hidden_adjust = hidden_loc_type == null ? 0 : 1;
            if (p == null)
            {
                from_locs = new ir.Param[pcount + hidden_adjust + calli_adjust];
                for (int i = 0; i < pcount; i++)
                {
                    var stack_loc = pcount - i - 1;
                    if (n.arg_list != null)
                        stack_loc = n.arg_list[i];
                    from_locs[i + hidden_adjust] = n.stack_before.Peek(stack_loc + calli_adjust).reg;
                }
                if (is_calli)
                    from_locs[pcount + hidden_adjust] = n.stack_before.Peek(0).reg;
            }
            else
            {
                from_locs = p;

                // adjust any rsp relative registers dependent on how many registers we have saved
                foreach (var l in from_locs)
                {
                    if (l != null &&
                        l.t == ir.Opcode.vl_mreg &&
                        l.mreg is ContentsReg)
                    {
                        var l2 = l.mreg as ContentsReg;
                        if (l2.basereg.Equals(r_esp))
                        {
                            l2.disp += x;
                        }
                    }

                }
            }

            // Reserve any required stack space
            if (cstack_loc != 0)
            {
                push_length += cstack_loc;
                r.Add(inst(cstack_loc <= 127 ? x86_sub_rm32_imm8 : x86_sub_rm32_imm32,
                    r_esp, cstack_loc, n));
            }

            // Move from the from list to the to list such that
            //  we never overwrite a from loc that hasn't been
            //  transfered yet
            pcount += hidden_adjust;
            pcount += calli_adjust;
            var to_do = pcount;
            bool[] done = new bool[pcount];

            if(hidden_adjust != 0)
            {
                // load up the address of the return value

                var ret_to = to_locs[0];
                if (ret_to is ContentsReg || (ret_to.type == rt_stack))
                {
                    r.Add(inst(t.psize == 4 ? x86_lea_r32 : x86_lea_r64, r_eax, act_dest, n));
                    handle_move(ret_to, r_eax, r, n, c);
                }
                else
                {
                    r.Add(inst(t.psize == 4 ? x86_lea_r32 : x86_lea_r64, ret_to, act_dest, n));
                }

                to_do--;
                done[0] = true;
            }

            while (to_do > 0)
            {
                int done_this_iter = 0;

                for(int to_i = 0; to_i < pcount; to_i++)
                {
                    if(!done[to_i])
                    {
                        var to_reg = to_locs[to_i];
                        if(to_reg.type == rt_stack)
                        {
                            to_reg = new ContentsReg
                            {
                                basereg = r_esp,
                                disp = to_reg.stack_loc,
                                size = to_reg.size
                            };
                        }

                        bool possible = true;

                        // determine if this to register is the source of a from
                        for(int from_i = 0; from_i < pcount; from_i++)
                        {
                            if (to_i == from_i)
                                continue;
                            if (!done[from_i] && from_locs[from_i].mreg != null &&
                                from_locs[from_i].mreg.Equals(to_reg))
                            {
                                possible = false;
                                break;
                            }
                        }

                        if(possible)
                        {
                            var from_reg = from_locs[to_i];
                            switch(from_reg.t)
                            {
                                case ir.Opcode.vl_mreg:
                                    if (from_reg.want_address)
                                    {
                                        Reg lea_to = to_reg;
                                        if (to_reg is ContentsReg)
                                            lea_to = r_eax;
                                        r.Add(inst(t.psize == 4 ? x86_lea_r32 : x86_lea_r64,
                                            lea_to, from_reg.mreg, n));
                                        handle_move(to_reg, lea_to, r, n, c);
                                    }
                                    else
                                        handle_move(to_reg, from_reg.mreg, r, n, c);
                                    break;
                                case ir.Opcode.vl_c:
                                case ir.Opcode.vl_c32:
                                case ir.Opcode.vl_c64:
                                    if (from_reg.v > int.MaxValue ||
                                        from_reg.v < int.MinValue)
                                    {
                                        throw new NotImplementedException();
                                    }
                                    else
                                    {
                                        r.Add(inst(to_reg.size == 8 ? x86_mov_rm64_imm32 : x86_mov_rm32_imm32,
                                            to_reg, from_reg, n));
                                    }
                                    break;
                                default:
                                    throw new NotSupportedException();
                            }
                            to_do--;
                            done_this_iter++;
                            done[to_i] = true;
                        }
                    }
                }

                if (done_this_iter == 0)
                {
                    // find two gprs/xmms we can swap to put them both
                    //  in the correct locations

                    for (int i = 0; i < pcount; i++)
                    {
                        if (done[i])
                            continue;

                        var from_i = from_locs[i].mreg;
                        var to_i = to_locs[i];

                        if (from_i == null)
                            continue;

                        if (from_i.type != rt_gpr &&
                            from_i.type != rt_float)
                            continue;
                        if (to_i.type != rt_gpr &&
                            to_i.type != rt_float)
                            continue;

                        for (int j = 0; j < pcount; j++)
                        {
                            if (j == i)
                                continue;
                            if (done[j])
                                continue;

                            var from_j = from_locs[j].mreg;

                            if (from_j == null)
                                continue;

                            var to_j = to_locs[j];

                            if (from_i.Equals(to_j) &&
                                from_j.Equals(to_i))
                            {
                                // we can swap these
                                if (from_i.type == rt_gpr)
                                {
                                    r.Add(inst(t.psize == 4 ? x86_xchg_r32_rm32 :
                                        x86_xchg_r64_rm64, to_i, to_j, n));
                                }
                                else
                                {
                                    handle_move(r_xmm7, to_i, r, n, c);
                                    handle_move(to_i, to_j, r, n, c);
                                    handle_move(to_j, r_xmm7, r, n, c);
                                }
                                done_this_iter += 2;
                                to_do -= 2;

                                done[i] = true;
                                done[j] = true;
                                break;
                            }

                        }
                    }
                }
                if (done_this_iter == 0)
                {
                    // find two unassigned gprs/xmms we can swap, which
                    //  may not necessarily put them in the correct place
                    bool shift_found = false;

                    // try with gprs first
                    int a = -1;
                    int b = -1;
                    for (int i = 0; i < pcount; i++)
                    {
                        if (done[i] == false && from_locs[i].mreg != null &&
                            from_locs[i].mreg.type == rt_gpr)
                        {
                            if (a == -1)
                                a = i;
                            else
                            {
                                b = i;
                                shift_found = true;
                            }
                        }
                    }

                    if(shift_found)
                    {
                        r.Add(inst(t.psize == 4 ? x86_xchg_r32_rm32 :
                            x86_xchg_r64_rm64, from_locs[a].mreg, from_locs[b].mreg, n));

                        var tmp = from_locs[a];
                        from_locs[a] = from_locs[b];
                        from_locs[b] = from_locs[a];
                    }
                    else
                    {
                        a = -1;
                        b = -1;
                        for (int i = 0; i < pcount; i++)
                        {
                            if (done[i] == false && from_locs[i].mreg != null &&
                                from_locs[i].mreg.type == rt_float)
                            {
                                if (a == -1)
                                    a = i;
                                else
                                {
                                    b = i;
                                    shift_found = true;
                                }
                            }
                        }

                        if(shift_found)
                        {
                            handle_move(r_xmm7, from_locs[a].mreg, r, n, c);
                            handle_move(from_locs[a].mreg, from_locs[b].mreg, r, n, c);
                            handle_move(from_locs[b].mreg, r_xmm7, r, n, c);

                            var tmp = from_locs[a];
                            from_locs[a] = from_locs[b];
                            from_locs[b] = from_locs[a];
                        }
                    }
                    if(!shift_found)
                        throw new NotImplementedException();
                }
            }

            // Do the call
            if (is_calli)
            {
                r.Add(inst(x86_call_rm32, calli_reg, n));
            }
            else
                r.Add(inst(x86_call_rel32, new ir.Param { t = ir.Opcode.vl_call_target, str = target }, n));

            // Restore stack
            if (push_length != 0)
            {
                var add_oc = t.psize == 4 ? x86_add_rm32_imm32 : x86_add_rm64_imm32;
                if (push_length < 128)
                    add_oc = t.psize == 4 ? x86_add_rm32_imm8 : x86_add_rm64_imm8;
                r.Add(inst(add_oc, r_esp, push_length, n));
            }

            // Get vt return value
            if (rct == ir.Opcode.ct_vt && !act_dest.Equals(dest))
            {
                handle_move(dest, act_dest, r, n, c);
            }

            // Restore saved registers
            for (int i = push_list.Count - 1; i >= 0; i--)
                handle_pop(push_list[i], ref x, r, n, c);

            // Get other return value
            if (rt != null && rct != ir.Opcode.ct_vt && rct != ir.Opcode.ct_unknown)
            {
                var rt_size = c.t.GetSize(rt);
                var retccmap = c.t.retcc_map["ret_" + call_ms.CallingConvention];

                if (rct == ir.Opcode.ct_float)
                {
                    if (t.psize == 4)
                    {
                        var from = t.regs[retccmap[rct][0]];
                        handle_move(dest, from, r, n, c);
                    }
                    else
                    {
                        if (!dest.Equals(r_xmm0))
                        {
                            r.Add(inst(x86_movsd_xmmm64_xmm, dest, r_xmm0, n));
                        }
                    }
                }
                else if (rt_size <= 4)
                    r.Add(inst(x86_mov_rm32_r32, dest, r_eax, n));
                else if (rt_size == 8)
                {
                    if (t.psize == 4)
                    {
                        var drda = dest.SubReg(0, 4, c.t);
                        var drdb = dest.SubReg(4, 4, c.t);
                        r.Add(inst(x86_mov_rm32_r32, drda, r_eax, n));
                        r.Add(inst(x86_mov_rm32_r32, drdb, r_edx, n));
                    }
                    else
                    {
                        r.Add(inst(x86_mov_rm64_r64, dest, r_eax, n));
                    }
                }
                else
                    throw new NotImplementedException();
            }

            return r;
        }

        private static void handle_push(Reg reg, ref int push_length, List<MCInst> r, CilNode.IRNode n, Code c)
        {
            if (reg is ContentsReg)
            {
                ContentsReg cr = reg as ContentsReg;
                if (cr.size < 4)
                    throw new NotImplementedException();
                else if (cr.size == 4)
                    r.Add(inst(x86_push_rm32, reg, n));
                else if (cr.size == 8 && c.t.psize == 8)
                    r.Add(inst(x86_push_rm32, reg, n));
                else
                {
                    var psize = util.util.align(cr.size, 4);
                    if (psize <= 127)
                        r.Add(inst(c.t.psize == 4 ? x86_sub_rm32_imm8 : x86_sub_rm64_imm8, r_esp, psize, n));
                    else
                        r.Add(inst(c.t.psize == 4 ? x86_sub_rm32_imm32 : x86_sub_rm64_imm32, r_esp, psize, n));
                    handle_move(new ContentsReg { basereg = r_esp, size = cr.size },
                        cr, r, n, c);
                }
                push_length += util.util.align(cr.size, c.t.psize);
            }
            else
            {
                if (reg.type == rt_gpr)
                {
                    r.Add(inst(x86_push_r32, reg, n));
                    push_length += c.t.psize;
                }
                else if (reg.type == rt_float)
                {
                    r.Add(inst(c.t.psize == 4 ? x86_sub_rm32_imm8 : x86_sub_rm64_imm8, r_esp, 8, n));
                    r.Add(inst(x86_movsd_xmmm64_xmm, new ContentsReg { basereg = r_esp, size = 8 }, reg, n));
                    push_length += 8;
                }
            }
        }

        private static void handle_pop(Reg reg, ref int pop_length, List<MCInst> r, CilNode.IRNode n, Code c)
        {
            if (reg is ContentsReg)
            {
                ContentsReg cr = reg as ContentsReg;
                if (cr.size < 4)
                    throw new NotImplementedException();
                else if (cr.size == 4)
                    r.Add(inst(x86_pop_rm32, reg, n));
                else
                {
                    handle_move(cr, new ContentsReg { basereg = r_esp, size = cr.size },
                        r, n, c);
                    var psize = util.util.align(cr.size, 4);
                    if (psize <= 127)
                        r.Add(inst(c.t.psize == 4 ? x86_add_rm32_imm8 : x86_add_rm64_imm8, r_esp, psize, n));
                    else
                        r.Add(inst(c.t.psize == 4 ? x86_add_rm32_imm32 : x86_add_rm64_imm32, r_esp, psize, n));
                }
                pop_length += 4;
            }
            else
            {
                if (reg.type == rt_gpr)
                {
                    r.Add(inst(x86_pop_r32, reg, n));
                    pop_length += 4;
                }
                else if (reg.type == rt_float)
                {
                    r.Add(inst(x86_movsd_xmm_xmmm64, reg, new ContentsReg { basereg = r_esp, size = 8 }, n));
                    r.Add(inst(c.t.psize == 4 ? x86_add_rm32_imm8 : x86_add_rm64_imm8, r_esp, 8, n));
                    pop_length += 8;
                }
            }
        }

        static protected MCInst inst_jmp(int idx, int jmp_target, CilNode.IRNode p)
        {
            return new MCInst
            {
                p = new ir.Param[]
                {
                    new ir.Param { t = ir.Opcode.vl_str, v = idx, str = insts[idx] },
                    new ir.Param { t = ir.Opcode.vl_br_target, v = jmp_target },
                },
                parent = p
            };
        }

        static protected MCInst inst_jmp(int idx, int jmp_target, int cc, CilNode.IRNode p)
        {
            return new MCInst
            {
                p = new ir.Param[]
                {
                    new ir.Param { t = ir.Opcode.vl_str, v = idx, str = insts[idx] },
                    new ir.Param { t = ir.Opcode.vl_cc, v = cc },
                    new ir.Param { t = ir.Opcode.vl_br_target, v = jmp_target }
                },
                parent = p
            };
        }

        protected static MCInst inst(int idx, ir.Param v1, ir.Param v2, ir.Param v3, CilNode.IRNode p, bool is_tls = false)
        {
            return new MCInst
            {
                p = new ir.Param[]
                {
                    new ir.Param { t = ir.Opcode.vl_str, v = idx, str = insts[idx], v2 = is_tls ? 1 : 0 },
                    v1,
                    v2,
                    v3
                },
                parent = p
            };
        }

        protected static MCInst inst(int idx, ir.Param v1, ir.Param v2, ir.Param v3, ir.Param v4, ir.Param v5, CilNode.IRNode p, bool is_tls = false)
        {
            if (is_tls)
                throw new NotImplementedException();
            return new MCInst
            {
                p = new ir.Param[]
                {
                    new ir.Param { t = ir.Opcode.vl_str, v = idx, str = insts[idx] },
                    v1,
                    v2,
                    v3,
                    v4,
                    v5
                },
                parent = p
            };
        }


        protected static MCInst inst(int idx, ir.Param v1, ir.Param v2, CilNode.IRNode p, bool is_tls = false)
        {
            return new MCInst
            {
                p = new ir.Param[]
                {
                    new ir.Param { t = ir.Opcode.vl_str, v = idx, str = insts[idx], v2 = is_tls ? 1 : 0 },
                    v1,
                    v2
                },
                parent = p
            };
        }

        protected static MCInst inst(int idx, ir.Param v1, CilNode.IRNode p, bool is_tls = false)
        {
            if (is_tls)
                throw new NotImplementedException();
            string str = null;
            insts.TryGetValue(idx, out str);
            return new MCInst
            {
                p = new ir.Param[]
                {
                    new ir.Param { t = ir.Opcode.vl_str, v = idx, str = str },
                    v1
                },
                parent = p
            };
        }

        protected static MCInst inst(int idx, CilNode.IRNode p, bool is_tls = false)
        {
            if (is_tls)
                throw new NotImplementedException();
            string str = null;
            insts.TryGetValue(idx, out str);
            return new MCInst
            {
                p = new ir.Param[]
                {
                    new ir.Param { t = ir.Opcode.vl_str, v = idx, str = str },
                },
                parent = p
            };
        }

        internal static List<MCInst> handle_add(
            Target t,
            List<CilNode.IRNode> nodes,
            int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct = n.ct;
            var srca = n.stack_before.Peek(n.arg_a).reg;
            var srcb = n.stack_before.Peek(n.arg_b).reg;
            var dest = n.stack_after.Peek(n.res_a).reg;

            List<MCInst> r = new List<MCInst>();

            switch (n_ct)
            {
                case ir.Opcode.ct_int32:
                    handle_add(t, srca, srcb, dest, r, n);
                    return r;

                case ir.Opcode.ct_int64:
                    {
                        if (t.psize == 4)
                        {
                            var srcaa = srca.SubReg(0, 4);
                            var srcab = srca.SubReg(4, 4);
                            var srcba = srcb.SubReg(0, 4);
                            var srcbb = srcb.SubReg(4, 4);
                            var desta = dest.SubReg(0, 4);
                            var destb = dest.SubReg(4, 4);
                            handle_add(t, srcaa, srcba, desta, r, n);
                            handle_add(t, srcab, srcbb, destb, r, n, true);
                            return r;
                        }
                        else
                        {
                            handle_add(t, srca, srcb, dest, r, n);
                            return r;
                        }
                    }

                case ir.Opcode.ct_float:
                    {
                        var actdest = dest;

                        if(dest is ContentsReg)
                            actdest = r_xmm7;

                        handle_move(actdest, srca, r, n, c);
                        r.Add(inst(x86_addsd_xmm_xmmm64,
                            actdest, srcb, n));
                        handle_move(dest, actdest, r, n, c);
                        return r;
                    }
            }
            return null;
        }

        internal static List<MCInst> handle_sub(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct = n.ct;
            var srca = n.stack_before.Peek(n.arg_a).reg;
            var srcb = n.stack_before.Peek(n.arg_b).reg;
            var dest = n.stack_after.Peek(n.res_a).reg;

            List<MCInst> r = new List<MCInst>();

            switch (n_ct)
            {
                case ir.Opcode.ct_int32:
                    handle_sub(t, srca, srcb, dest, r, n);
                    return r;

                case ir.Opcode.ct_int64:
                    if(t.psize == 8)
                    {
                        handle_sub(t, srca, srcb, dest, r, n);
                        return r;
                    }
                    else
                    {
                        var draa = srca.SubReg(0, 4, c.t);
                        var drab = srca.SubReg(4, 4, c.t);
                        var drba = srcb.SubReg(0, 4, c.t);
                        var drbb = srcb.SubReg(4, 4, c.t);
                        var drda = dest.SubReg(0, 4, c.t);
                        var drdb = dest.SubReg(4, 4, c.t);
                        handle_sub(t, draa, drba, drda, r, n);
                        handle_sub(t, drab, drbb, drdb, r, n, true);
                        return r;
                    }

                case ir.Opcode.ct_float:
                    {
                        var actdest = dest;

                        if (dest is ContentsReg)
                            actdest = r_xmm7;

                        handle_move(actdest, srca, r, n, c);
                        r.Add(inst(x86_subsd_xmm_xmmm64,
                            actdest, srcb, n));
                        handle_move(dest, actdest, r, n, c);
                        return r;
                    }
            }

            return null;
        }

        internal static List<MCInst> handle_mul(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct = n.ct;
            if (n_ct == ir.Opcode.ct_int32)
            {
                var srca = n.stack_before.Peek(n.arg_a).reg;
                var srcb = n.stack_before.Peek(n.arg_b).reg;
                var dest = n.stack_after.Peek(n.res_a).reg;

                if (srca.Equals(dest) && !(srca is ContentsReg))
                {
                    return new List<MCInst>
                    {
                        inst(x86_imul_r32_rm32, dest, srcb, n)
                    };
                }
                else
                {
                    var r = new List<MCInst>();
                    handle_move(r_eax, srca, r, n, c);
                    r.Add(inst(x86_imul_r32_rm32, r_eax, srcb, n));
                    handle_move(dest, r_eax, r, n, c);
                    return r;
                }
            }
            else if (n_ct == ir.Opcode.ct_float)
            {
                var srca = n.stack_before.Peek(n.arg_a).reg;
                var srcb = n.stack_before.Peek(n.arg_b).reg;
                var dest = n.stack_after.Peek(n.res_a).reg;

                if (srca.Equals(dest) && !(srca is ContentsReg))
                {
                    return new List<MCInst>
                    {
                        inst(x86_mulsd_xmm_xmmm64, dest, srcb, n)
                    };
                }
                else
                {
                    var r = new List<MCInst>();
                    handle_move(r_xmm7, srca, r, n, c);
                    r.Add(inst(x86_mulsd_xmm_xmmm64, r_xmm7, srcb, n));
                    handle_move(dest, r_xmm7, r, n, c);
                    return r;
                }
            }
            else if (n_ct == ir.Opcode.ct_int64)
            {
                if (t.psize == 8)
                {
                    var srca = n.stack_before.Peek(n.arg_a).reg;
                    var srcb = n.stack_before.Peek(n.arg_b).reg;
                    var dest = n.stack_after.Peek(n.res_a).reg;

                    if (srca.Equals(dest) && !(srca is ContentsReg))
                    {
                        return new List<MCInst>
                    {
                        inst(x86_imul_r64_rm64, dest, srcb, n)
                    };
                    }
                    else
                    {
                        var r = new List<MCInst>();
                        handle_move(x86_64.x86_64_Assembler.r_rax, srca, r, n, c);
                        r.Add(inst(x86_imul_r64_rm64, x86_64.x86_64_Assembler.r_rax, srcb, n));
                        handle_move(dest, x86_64.x86_64_Assembler.r_rax, r, n, c);
                        return r;
                    }
                }
                else
                {
                    return t.handle_external(t, nodes, start, count, c,
                        "__muldi3");
                }
            }
            return null;
        }

        internal static List<MCInst> handle_div(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var srca = n.stack_before.Peek(n.arg_a).reg;
            var srcb = n.stack_before.Peek(n.arg_b).reg;
            var dest = n.stack_after.Peek(n.res_a).reg;
            var n_ct = n.ct;

            switch(n_ct)
            {
                case ir.Opcode.ct_int32:
                    {
                        List<MCInst> r = new List<MCInst>();
                        handle_move(r_eax, srca, r, n, c);
                        r.Add(inst(x86_xor_r32_rm32, r_edx, r_edx, n));
                        r.Add(inst(x86_idiv_rm32, srcb, n));
                        handle_move(dest, r_eax, r, n, c);
                        return r;
                    }

                case ir.Opcode.ct_float:
                    {
                        List<MCInst> r = new List<MCInst>();

                        if(!srca.Equals(dest) &&
                            !(dest is ContentsReg))
                        {
                            handle_move(dest, srca, r, n, c);
                            dest = srca;
                        }
                        var act_dest = dest;
                        if(dest is ContentsReg)
                        {
                            handle_move(r_xmm7, srca, r, n, c);
                            act_dest = r_xmm7;
                        }

                        r.Add(inst(x86_divsd_xmm_xmmm64, act_dest, srcb, n));
                        handle_move(dest, act_dest, r, n, c);

                        return r;
                    }

                case ir.Opcode.ct_int64:
                    if (t.psize == 8)
                    {
                        List<MCInst> r = new List<MCInst>();
                        handle_move(x86_64.x86_64_Assembler.r_rax, srca, r, n, c);
                        r.Add(inst(x86_xor_r64_rm64, x86_64.x86_64_Assembler.r_rdx, x86_64.x86_64_Assembler.r_rdx, n));
                        r.Add(inst(x86_idiv_rm64, srcb, n));
                        handle_move(dest, x86_64.x86_64_Assembler.r_rax, r, n, c);
                        return r;
                    }
                    else
                    {
                        return t.handle_external(t, nodes, start, count, c,
                            "__divdi3");
                    }
            }

            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_and(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct = n.ct;
            var srca = n.stack_before.Peek(n.arg_a).reg;
            var srcb = n.stack_before.Peek(n.arg_b).reg;
            var dest = n.stack_after.Peek(n.res_a).reg;

            List<MCInst> r = new List<MCInst>();

            switch (n_ct)
            {
                case ir.Opcode.ct_int32:
                    handle_and(srca, srcb, dest, r, n);
                    return r;

                case ir.Opcode.ct_int64:
                    if(t.psize == 8)
                    {
                        handle_and(srca, srcb, dest, r, n);
                        return r;
                    }
                    else
                    {
                        var draa = srca.SubReg(0, 4, c.t);
                        var drab = srca.SubReg(4, 4, c.t);
                        var drba = srcb.SubReg(0, 4, c.t);
                        var drbb = srcb.SubReg(4, 4, c.t);
                        var drda = dest.SubReg(0, 4, c.t);
                        var drdb = dest.SubReg(4, 4, c.t);
                        handle_and(draa, drba, drda, r, n);
                        handle_and(drab, drbb, drdb, r, n);
                        return r;
                    }
            }
            return null;
        }

        internal static List<MCInst> handle_or(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct = n.ct;
            var srca = n.stack_before.Peek(n.arg_a).reg;
            var srcb = n.stack_before.Peek(n.arg_b).reg;
            var dest = n.stack_after.Peek(n.res_a).reg;

            List<MCInst> r = new List<MCInst>();

            switch (n_ct)
            {
                case ir.Opcode.ct_int32:
                    handle_or(srca, srcb, dest, r, n);
                    return r;

                case ir.Opcode.ct_int64:
                    if(t.psize == 8)
                    {
                        handle_or(srca, srcb, dest, r, n);
                        return r;
                    }
                    else
                    {
                        var draa = srca.SubReg(0, 4, c.t);
                        var drab = srca.SubReg(4, 4, c.t);
                        var drba = srcb.SubReg(0, 4, c.t);
                        var drbb = srcb.SubReg(4, 4, c.t);
                        var drda = dest.SubReg(0, 4, c.t);
                        var drdb = dest.SubReg(4, 4, c.t);
                        handle_or(draa, drba, drda, r, n);
                        handle_or(drab, drbb, drdb, r, n);
                        return r;
                    }
            }
            return null;
        }

        internal static List<MCInst> handle_xor(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct = n.ct;
            var srca = n.stack_before.Peek(n.arg_a).reg;
            var srcb = n.stack_before.Peek(n.arg_b).reg;
            var dest = n.stack_after.Peek(n.res_a).reg;

            List<MCInst> r = new List<MCInst>();

            switch (n_ct)
            {
                case ir.Opcode.ct_int32:
                    handle_xor(srca, srcb, dest, r, n);
                    return r;

                case ir.Opcode.ct_int64:
                    if (t.psize == 8)
                    {
                        handle_xor(srca, srcb, dest, r, n);
                        return r;
                    }
                    else
                    {
                        var draa = srca.SubReg(0, 4, c.t);
                        var drab = srca.SubReg(4, 4, c.t);
                        var drba = srcb.SubReg(0, 4, c.t);
                        var drbb = srcb.SubReg(4, 4, c.t);
                        var drda = dest.SubReg(0, 4, c.t);
                        var drdb = dest.SubReg(4, 4, c.t);
                        handle_xor(draa, drba, drda, r, n);
                        handle_xor(drab, drbb, drdb, r, n);
                        return r;
                    }
            }
            return null;
        }

        internal static List<MCInst> handle_not(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct = n.ct;
            var src = n.stack_before.Peek(n.arg_a).reg;
            var dest = n.stack_after.Peek(n.res_a).reg;

            if (n_ct == ir.Opcode.ct_int32)
            {
                List<MCInst> r = new List<MCInst>();
                if (src != dest)
                    handle_move(dest, src, r, n, c);
                r.Add(inst(x86_not_rm32, dest, n));
                return r;
            }
            else if(n_ct == ir.Opcode.ct_int64)
            {
                if (t.psize == 8)
                {
                    List<MCInst> r = new List<MCInst>();
                    if (src != dest)
                        handle_move(dest, src, r, n, c);
                    r.Add(inst(x86_not_rm64, dest, n));
                    return r;
                }
                else
                {
                    var sa = src.SubReg(0, 4);
                    var sb = src.SubReg(4, 4);
                    var da = dest.SubReg(0, 4);
                    var db = dest.SubReg(4, 4);

                    List<MCInst> r = new List<MCInst>();
                    if (!sa.Equals(da))
                        handle_move(da, sa, r, n, c);
                    r.Add(inst(x86_not_rm32, da, n));
                    if (!sb.Equals(db))
                        handle_move(db, sb, r, n, c);
                    r.Add(inst(x86_not_rm32, db, n));
                    return r;
                }
            }
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_neg(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct = n.ct;
            var src = n.stack_before.Peek(n.arg_a).reg;
            var dest = n.stack_after.Peek(n.res_a).reg;

            if (n_ct == ir.Opcode.ct_int32)
            {
                List<MCInst> r = new List<MCInst>();
                if (src != dest)
                    handle_move(dest, src, r, n, c);
                r.Add(inst(x86_neg_rm32, dest, n));
                return r;
            }
            else if (n_ct == ir.Opcode.ct_int64)
            {
                if (t.psize == 8)
                {
                    List<MCInst> r = new List<MCInst>();
                    if (src != dest)
                        handle_move(dest, src, r, n, c);
                    r.Add(inst(x86_neg_rm64, dest, n));
                    return r;
                }
                else
                {
                    return t.handle_external(t, nodes, start, count,
                        c, "__negdi2");
                }
            }
            else if (n_ct == ir.Opcode.ct_float)
            {
                // first get 0.0 in xmm7 by performing cmpneqsd
                List<MCInst> r = new List<MCInst>();
                r.Add(inst(x86_cmpsd_xmm_xmmm64_imm8,
                    r_xmm7, r_xmm7, 4, n));

                // now subtract input value from it (0.0)
                r.Add(inst(x86_subsd_xmm_xmmm64, r_xmm7, src, n));

                handle_move(dest, r_xmm7, r, n, c);

                return r;
            }
            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_cctor_runonce(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];

            /* We emit a complex instruction sequence:

            First, load address of static object
                mov rdx, [lab_addr]

            Now, execute a loop testing for [rdx] being either:
                0 - cctor has not been run and is not running,
                    therefore try and acquire the lock and run
                    it
                1 - cctor is running in this or another thread
                2 - cctor has finished

            CIL II 10.5.3.3 allows the cctor to return if the
            cctor is already running in the current thread or
            any thread that is blocking on the current.
            TODO: we don't support the second part yet, but
            return if [rdx] == 1 too.

                .t1:
                cmp_rm8_imm8 [rdx], 2
                je .ret

                cmp_rm8_imm8 [rdx], 1   // TODO: should check thread id here
                je .ret

            Attempt to acquire the lock here.  AL is the value
            to test, CL is the value to set if we acquire it,
            ZF reports success.
                xor_rm32_r32 rax, rax
                mov_r32_imm32 rcx, 1
                lock_cmpxchg_rm8_r8 [rdx], cl

                jnz .t1

            */

            // Assign local labels
            var t1 = c.next_mclabel--;

            int ret = c.cctor_ret_tag;
            if (ret == -1)
            {
                ret = c.next_mclabel--;
                c.cctor_ret_tag = ret;
            }

            List<MCInst> r = new List<MCInst>();

            r.Add(inst(x86_mov_rm32_imm32, r_edx, new ir.Param { t = ir.Opcode.vl_str, str = n.imm_ts.MangleType() + "S" }, n));

            // t1
            r.Add(inst(Generic.g_mclabel, new ir.Param { t = ir.Opcode.vl_br_target, v = t1 }, n));

            r.Add(inst(x86_cmp_rm8_imm8, new ContentsReg { basereg = r_edx }, 2, n));
            r.Add(inst_jmp(x86_jcc_rel32, ret, ir.Opcode.cc_eq, n));

            r.Add(inst(x86_cmp_rm8_imm8, new ContentsReg { basereg = r_edx }, 1, n));
            r.Add(inst_jmp(x86_jcc_rel32, ret, ir.Opcode.cc_eq, n));

            r.Add(inst(x86_xor_r32_rm32, r_eax, r_eax, n));
            r.Add(inst(x86_mov_rm32_imm32, r_ecx, 1, n));
            r.Add(inst(x86_lock_cmpxchg_rm8_r8, new ContentsReg { basereg = r_edx }, r_ecx, n));
            r.Add(inst_jmp(x86_jcc_rel32, t1, ir.Opcode.cc_ne, n));

            return r;
        }

        internal static List<MCInst> handle_memset(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];

            var dr = n.stack_before.Peek(n.arg_a).reg;
            var cv = n.stack_before.Peek(n.arg_b);
            var cr = cv.reg;

            List<MCInst> r = new List<MCInst>();

            if (cv.min_l == cv.max_l && cv.min_l <= t.psize * 4 &&
                cv.min_l % t.psize == 0 &&
                dr.type == rt_gpr)
            {
                // can optimise call away
                for(int i = 0; i < cv.min_l; i+= t.psize)
                {
                    var cdr = new ContentsReg { basereg = dr, disp = i, size = t.psize };

                    r.Add(inst(t.psize == 4 ? x86_mov_rm32_imm32 : x86_mov_rm64_imm32,
                        cdr, new ir.Param { t = ir.Opcode.vl_c32, v = 0 }, n));

                }
                return r;
            }

            r.AddRange(handle_call(n, c,
                c.special_meths.GetMethodSpec(c.special_meths.memset),
                new ir.Param[]
                {
                            new ir.Param { t = ir.Opcode.vl_mreg, mreg = dr },
                            0,
                            new ir.Param { t = ir.Opcode.vl_mreg, mreg = cr },
                },
                null, "memset"));

            return r;
        }

        internal static List<MCInst> handle_memcpy(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];

            var dr = n.stack_before.Peek(n.arg_a).reg;
            var sr = n.stack_before.Peek(n.arg_b).reg;
            var cv = n.stack_before.Peek(n.arg_c);
            var cr = cv.reg;

            List<MCInst> r = new List<MCInst>();

            if(cv.min_l == cv.max_l && cv.min_l <= t.psize * 4 &&
                dr.type == rt_gpr && sr.type == rt_gpr)
            {
                // can optimise call away
                sr = new ContentsReg { basereg = sr, size = (int)cv.min_l };
                dr = new ContentsReg { basereg = dr, size = (int)cv.min_l };
                handle_move(dr, sr, r, n, c);
                return r;
            }

            // emit call to memcpy(dest, src, n)
            r.AddRange(handle_call(n, c,
                c.special_meths.GetMethodSpec(c.special_meths.memcpy),
                new ir.Param[]
                {
                            new ir.Param { t = ir.Opcode.vl_mreg, mreg = dr },
                            new ir.Param { t = ir.Opcode.vl_mreg, mreg = sr },
                            new ir.Param { t = ir.Opcode.vl_mreg, mreg = cr },
                },
                null, "memcpy"));

            return r;
        }

        internal static List<MCInst> handle_call(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            return handle_call(n, c, false, t);
        }

        internal static List<MCInst> handle_calli(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            return handle_call(n, c, true, t);
        }

        internal static List<MCInst> handle_ret(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            return handle_ret(n, c, t);
        }

        internal static List<MCInst> handle_cmp(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct = n.ct;
            if (n_ct == ir.Opcode.ct_int32)
            {
                var srca = n.stack_before.Peek(n.arg_a).reg;
                var srcb = n.stack_before.Peek(n.arg_b).reg;
                var dest = n.stack_after.Peek(n.res_a).reg;

                List<MCInst> r = new List<MCInst>();
                if (!(srca is ContentsReg))
                    r.Add(inst(x86_cmp_r32_rm32, srca, srcb, n));
                else if (!(srcb is ContentsReg))
                    r.Add(inst(x86_cmp_rm32_r32, srca, srcb, n));
                else
                {
                    r.Add(inst(x86_mov_r32_rm32, r_eax, srca, n));
                    r.Add(inst(x86_cmp_r32_rm32, r_eax, srcb, n));
                }

                r.Add(inst(x86_set_rm32, new ir.Param { t = ir.Opcode.vl_cc, v = (int)n.imm_ul }, r_eax, n));
                if (dest is ContentsReg)
                {
                    r.Add(inst(x86_movzxbd, r_eax, r_eax, n));
                    r.Add(inst(x86_mov_rm32_r32, dest, r_eax, n));
                }
                else
                    r.Add(inst(x86_movzxbd, dest, r_eax, n));

                return r;
            }
            else if (n_ct == ir.Opcode.ct_int64)
            {
                var srca = n.stack_before.Peek(n.arg_a).reg;
                var srcb = n.stack_before.Peek(n.arg_b).reg;
                var dest = n.stack_after.Peek(n.res_a).reg;

                List<MCInst> r = new List<MCInst>();

                if (t.psize == 8)
                {
                    if (!(srca is ContentsReg))
                        r.Add(inst(x86_cmp_r64_rm64, srca, srcb, n));
                    else if (!(srcb is ContentsReg))
                        r.Add(inst(x86_cmp_rm64_r64, srca, srcb, n));
                    else
                    {
                        r.Add(inst(x86_mov_r32_rm32, r_eax, srca, n));
                        r.Add(inst(x86_cmp_r64_rm64, r_eax, srcb, n));
                    }

                    r.Add(inst(x86_set_rm32, new ir.Param { t = ir.Opcode.vl_cc, v = (int)n.imm_ul }, r_eax, n));
                    if (dest is ContentsReg)
                    {
                        r.Add(inst(x86_movzxbd, r_eax, r_eax, n));
                        r.Add(inst(x86_mov_rm32_r32, dest, r_eax, n));
                    }
                    else
                        r.Add(inst(x86_movzxbd, dest, r_eax, n));
                }
                else
                {
                    var sal = srca.SubReg(0, 4);
                    var sah = srca.SubReg(4, 4);
                    var sbl = srcb.SubReg(0, 4);
                    var sbh = srcb.SubReg(4, 4);


                    var cc = (int)n.imm_ul;

                    /* For equal/not equal do a simple compare of
                     * low and high halves.
                     * 
                     * For other comparisons its a bit more complex:
                     * 
                     * cmp sah, sbh
                     * j1 L2            <- opposite, excluding equal of comparison
                     * j2 L1            <- original comparison, excluding equal
                     * cmp sal, sbl     <- this is only performed if sah == sbh
                     * j3 L2            <- opposite, including complement of equal, unsigned
                     * L1:              <- success path
                     * mov dest, 1
                     * jmp L3
                     * L2:              <- fail path
                     * mov dest, 0
                     * L3:              <- end
                     */

                    var fail_path = c.next_mclabel--;
                    var end_path = c.next_mclabel--;

                    if (cc == ir.Opcode.cc_eq ||
                        cc == ir.Opcode.cc_ne)
                    {
                        var other_cc = ir.Opcode.cc_invert_map[cc];

                        // invert the comparison
                        if (sbh is ContentsReg)
                        {
                            handle_move(r_edx, sbh, r, n, c);
                            sbh = r_edx;
                        }
                        r.Add(inst(x86_cmp_rm32_r32, sah, sbh, n));
                        r.Add(inst_jmp(x86_jcc_rel32, fail_path, other_cc, n));

                        if (sbl is ContentsReg)
                        {
                            handle_move(r_edx, sbl, r, n, c);
                            sbl = r_edx;
                        }
                        r.Add(inst(x86_cmp_rm32_r32, sal, sbl, n));
                        r.Add(inst_jmp(x86_jcc_rel32, fail_path, other_cc, n));

                        // success
                        r.Add(inst(x86_mov_rm32_imm32, dest, 1, n));
                        r.Add(inst_jmp(x86_jmp_rel32, end_path, n));

                        // fail
                        r.Add(inst(Generic.g_mclabel, new ir.Param { t = ir.Opcode.vl_br_target, v = fail_path }, n));
                        r.Add(inst(x86_mov_rm32_imm32, dest, 0, n));

                        // end
                        r.Add(inst(Generic.g_mclabel, new ir.Param { t = ir.Opcode.vl_br_target, v = end_path }, n));
                    }
                    else
                    {
                        var success_path = c.next_mclabel--;
                        int j1_cc = 0;
                        int j2_cc = 0;
                        int j3_cc = 0;

                        switch (cc)
                        {
                            case ir.Opcode.cc_a:
                                j1_cc = ir.Opcode.cc_b;
                                j2_cc = ir.Opcode.cc_a;
                                j3_cc = ir.Opcode.cc_be;
                                break;
                            case ir.Opcode.cc_ae:
                                j1_cc = ir.Opcode.cc_b;
                                j2_cc = ir.Opcode.cc_a;
                                j3_cc = ir.Opcode.cc_b;
                                break;
                            case ir.Opcode.cc_b:
                                j1_cc = ir.Opcode.cc_a;
                                j2_cc = ir.Opcode.cc_b;
                                j3_cc = ir.Opcode.cc_ae;
                                break;
                            case ir.Opcode.cc_be:
                                j1_cc = ir.Opcode.cc_a;
                                j2_cc = ir.Opcode.cc_b;
                                j3_cc = ir.Opcode.cc_a;
                                break;
                            case ir.Opcode.cc_ge:
                                j1_cc = ir.Opcode.cc_lt;
                                j2_cc = ir.Opcode.cc_gt;
                                j3_cc = ir.Opcode.cc_b;
                                break;
                            case ir.Opcode.cc_gt:
                                j1_cc = ir.Opcode.cc_lt;
                                j2_cc = ir.Opcode.cc_gt;
                                j3_cc = ir.Opcode.cc_be;
                                break;
                            case ir.Opcode.cc_le:
                                j1_cc = ir.Opcode.cc_gt;
                                j2_cc = ir.Opcode.cc_lt;
                                j3_cc = ir.Opcode.cc_a;
                                break;
                            case ir.Opcode.cc_lt:
                                j1_cc = ir.Opcode.cc_gt;
                                j2_cc = ir.Opcode.cc_lt;
                                j3_cc = ir.Opcode.cc_ae;
                                break;
                            default:
                                throw new NotSupportedException();
                        }

                        if (sbh is ContentsReg)
                        {
                            handle_move(r_edx, sbh, r, n, c);
                            sbh = r_edx;
                        }
                        r.Add(inst(x86_cmp_rm32_r32, sah, sbh, n));
                        r.Add(inst_jmp(x86_jcc_rel32, fail_path, j1_cc, n));
                        r.Add(inst_jmp(x86_jcc_rel32, success_path, j2_cc, n));

                        if (sbl is ContentsReg)
                        {
                            handle_move(r_edx, sbl, r, n, c);
                            sbl = r_edx;
                        }
                        r.Add(inst(x86_cmp_rm32_r32, sal, sbl, n));
                        r.Add(inst_jmp(x86_jcc_rel32, fail_path, j3_cc, n));

                        // success
                        r.Add(inst(Generic.g_mclabel, new ir.Param { t = ir.Opcode.vl_br_target, v = success_path }, n));
                        r.Add(inst(x86_mov_rm32_imm32, dest, 1, n));
                        r.Add(inst_jmp(x86_jmp_rel32, end_path, n));

                        // fail
                        r.Add(inst(Generic.g_mclabel, new ir.Param { t = ir.Opcode.vl_br_target, v = fail_path }, n));
                        r.Add(inst(x86_mov_rm32_imm32, dest, 0, n));

                        // end
                        r.Add(inst(Generic.g_mclabel, new ir.Param { t = ir.Opcode.vl_br_target, v = end_path }, n));
                    }
                }
                return r;
            }
            else if(n_ct == ir.Opcode.ct_float)
            {
                var srca = n.stack_before.Peek(n.arg_a).reg;
                var srcb = n.stack_before.Peek(n.arg_b).reg;
                var dest = n.stack_after.Peek(n.res_a).reg;

                List<MCInst> r = new List<MCInst>();

                if(srca is ContentsReg)
                {
                    handle_move(r_xmm7, srca, r, n, c);
                    srca = r_xmm7;
                }

                var cc = (int)n.imm_ul;

                int oc = 0;
                int act_cc = 0;

                switch(cc)
                {
                    case ir.Opcode.cc_a:
                        act_cc = ir.Opcode.cc_a;
                        oc = x86_ucomisd_xmm_xmmm64;
                        break;
                    case ir.Opcode.cc_ae:
                        act_cc = ir.Opcode.cc_ae;
                        oc = x86_ucomisd_xmm_xmmm64;
                        break;
                    case ir.Opcode.cc_b:
                        act_cc = ir.Opcode.cc_b;
                        oc = x86_ucomisd_xmm_xmmm64;
                        break;
                    case ir.Opcode.cc_be:
                        act_cc = ir.Opcode.cc_be;
                        oc = x86_ucomisd_xmm_xmmm64;
                        break;
                    case ir.Opcode.cc_eq:
                        act_cc = ir.Opcode.cc_eq;
                        oc = x86_comisd_xmm_xmmm64;
                        break;
                    case ir.Opcode.cc_ge:
                        act_cc = ir.Opcode.cc_ae;
                        oc = x86_comisd_xmm_xmmm64;
                        break;
                    case ir.Opcode.cc_gt:
                        act_cc = ir.Opcode.cc_a;
                        oc = x86_comisd_xmm_xmmm64;
                        break;
                    case ir.Opcode.cc_le:
                        act_cc = ir.Opcode.cc_be;
                        oc = x86_comisd_xmm_xmmm64;
                        break;
                    case ir.Opcode.cc_lt:
                        act_cc = ir.Opcode.cc_b;
                        oc = x86_comisd_xmm_xmmm64;
                        break;
                    case ir.Opcode.cc_ne:
                        act_cc = ir.Opcode.cc_ne;
                        oc = x86_comisd_xmm_xmmm64;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                r.Add(inst(oc, srca, srcb, n));

                r.Add(inst(x86_set_rm32, new ir.Param { t = ir.Opcode.vl_cc, v = act_cc }, r_eax, n));
                if (dest is ContentsReg)
                {
                    r.Add(inst(x86_movzxbd, r_eax, r_eax, n));
                    r.Add(inst(x86_mov_rm32_r32, dest, r_eax, n));
                }
                else
                    r.Add(inst(x86_movzxbd, dest, r_eax, n));

                return r;
            }
            return null;
        }

        internal static List<MCInst> handle_br(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            return new List<MCInst> { inst_jmp(x86_jmp_rel32, (int)n.imm_l, n) };
        }

        internal static List<MCInst> handle_brif(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct = n.ct;
            if (n_ct == ir.Opcode.ct_int32)
            {
                var srca = n.stack_before.Peek(n.arg_a).reg;
                var srcb = n.stack_before.Peek(n.arg_b).reg;

                List<MCInst> r = new List<MCInst>();

                var cc = (int)n.imm_ul;
                var target = (int)n.imm_l;

                handle_brifi32(srca, srcb, cc, target, r, n);

                return r;
            }
            else if (n_ct == ir.Opcode.ct_int64)
            {
                if (t.psize == 4)
                {
                    var srca = n.stack_before.Peek(n.arg_a).reg;
                    var srcb = n.stack_before.Peek(n.arg_b).reg;

                    var srcaa = srca.SubReg(0, 4, c.t);
                    var srcab = srca.SubReg(4, 4, c.t);
                    var srcba = srcb.SubReg(0, 4, c.t);
                    var srcbb = srcb.SubReg(4, 4, c.t);

                    List<MCInst> r = new List<MCInst>();

                    var cc = (int)n.imm_ul;
                    var target = (int)n.imm_l;

                    // the following is untested
                    var other_cc = ir.Opcode.cc_invert_map[cc];
                    var neq_path = c.next_mclabel--;
                    handle_brifi32(srcab, srcbb, other_cc, neq_path, r, n);
                    handle_brifi32(srcaa, srcba, cc, target, r, n);
                    r.Add(inst(Generic.g_mclabel, new ir.Param { t = ir.Opcode.vl_br_target, v = neq_path }, n));

                    return r;
                }
                else
                {
                    var srca = n.stack_before.Peek(n.arg_a).reg;
                    var srcb = n.stack_before.Peek(n.arg_b).reg;

                    List<MCInst> r = new List<MCInst>();

                    var cc = (int)n.imm_ul;
                    var target = (int)n.imm_l;

                    handle_brifi64(srca, srcb, cc, target, r, n);

                    return r;
                }
            }
            else if (n_ct == ir.Opcode.ct_float)
            {
                var srca = n.stack_before.Peek(n.arg_a).reg;
                var srcb = n.stack_before.Peek(n.arg_b).reg;

                List<MCInst> r = new List<MCInst>();

                var cc = (int)n.imm_ul;
                var target = (int)n.imm_l;

                handle_briff(srca, srcb, cc, target, r, n);

                return r;
            }
            return null;
        }

        internal static List<MCInst> handle_enter(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            List<MCInst> r = new List<MCInst>();
            var n = nodes[start];

            var lv_size = c.lv_total_size + c.stack_total_size;

            if (ir.Opcode.GetCTFromType(c.ret_ts) == ir.Opcode.ct_vt)
            {
                if (t.psize == 4)
                {
                    r.Add(inst(x86_pop_r32, r_eax, n));
                    r.Add(inst(x86_xchg_r32_rm32, r_eax, new ContentsReg { basereg = r_esp, disp = 0, size = 4 }, n));
                }
            }

            r.Add(inst(x86_push_r32, r_ebp, n));
            handle_move(r_ebp, r_esp, r, n, c);
            if (lv_size != 0)
            {
                if (lv_size <= 127)
                    r.Add(inst(t.psize == 4 ? x86_sub_rm32_imm8 : x86_sub_rm64_imm8, r_esp, lv_size, n));
                else
                    r.Add(inst(t.psize == 4 ? x86_sub_rm32_imm32 : x86_sub_rm64_imm32, r_esp, lv_size, n));
            }

            /* Move incoming arguments to the appropriate locations */
            for(int i = 0; i < c.la_needs_assign.Length; i++)
            {
                if(c.la_needs_assign[i])
                {
                    var from = c.incoming_args[i];
                    var to = c.la_locs[i];

                    handle_move(to, from, r, n, c);
                }
            }

            var regs_to_save = c.regs_used & c.t.cc_callee_preserves_map[c.ms.CallingConvention];
            
            // all registers are saved in isrs
            if (c.ms.CallingConvention == "isr" || c.ms.CallingConvention == "isrec")
                regs_to_save = c.t.cc_callee_preserves_map[c.ms.CallingConvention];

            var regs_set = new util.Set();
            regs_set.Union(regs_to_save);
            int y = 0;
            while (regs_set.Empty == false)
            {
                var reg = regs_set.get_first_set();
                regs_set.unset(reg);
                var cur_reg = t.regs[reg];
                handle_push(cur_reg, ref y, r, n, c);
                c.regs_saved.Add(cur_reg);
            }

            if (ir.Opcode.GetCTFromType(c.ret_ts) == ir.Opcode.ct_vt)
            {
                handle_move(new ContentsReg { basereg = r_ebp, disp = -t.psize, size = t.psize },
                    t.psize == 4 ? r_eax : r_edi, r, n, c);
            }

            if(c.ms.CallingConvention == "isr" || c.ms.CallingConvention == "isrec")
            {
                // store the current stack pointer as the 'regs' parameter
                handle_move(c.la_locs[c.la_locs.Length - 1], r_esp, r, n, c);
            }

            return r;
        }

        internal static List<MCInst> handle_break(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];

            List<MCInst> r = new List<MCInst>();

            var br_target = c.next_mclabel--;

            r.Add(inst(x86_mov_rm32_imm32, r_eax, 0, n));
            r.Add(inst(Generic.g_mclabel, new ir.Param { t = ir.Opcode.vl_br_target, v = br_target }, n));
            r.Add(inst(x86_cmp_rm32_imm32, r_eax, 1, n));
            r.Add(inst_jmp(x86_jcc_rel32, br_target, ir.Opcode.cc_ne, n));

            //r.Add(inst(x86_int3, n));

            return r;
        }

        internal static List<MCInst> handle_enter_handler(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var r = new List<MCInst>();

            string eh_str = "EH";
            if (n.imm_ul == 1UL)
                eh_str = "EHF";     // filter block

            r.Add(inst(Generic.g_label, c.ms.MangleMethod() + eh_str + n.imm_l.ToString(), n));

            // store current frame pointer (we need to restore it at the
            //  end in order to return properly to the exception handling
            //  mechanism)
            r.Add(inst(x86_push_r32, r_ebp, n));

            // extract our frame pointer from the passed argument
            if (t.psize == 8)
            {
                r.Add(inst(x86_mov_r64_rm64, r_ebp, r_edi, n));
            }
            else
            {
                r.Add(inst(x86_mov_r32_rm32, r_ebp, new ContentsReg { basereg = r_esp, disp = 4, size = 4 }, n));
            }

            // store registers used by function
            int y = 0;
            foreach (var reg in c.regs_saved)
                handle_push(reg, ref y, r, n, c);

            return r;
        }

        internal static List<MCInst> handle_conv(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct = n.ct;
            bool is_un = (n.imm_l & 0x1) != 0;
            bool is_ovf = (n.imm_l & 0x2) != 0;
            if (n_ct == ir.Opcode.ct_int32)
            {
                var r = new List<MCInst>();

                var si = n.stack_before.Peek(n.arg_a);
                var di = n.stack_after.Peek(n.res_a);
                var to_type = di.ts.SimpleType;

                var sreg = si.reg;
                var dreg = di.reg;

                var actdreg = dreg;
                if (dreg is ContentsReg)
                {
                    if (actdreg.size == 8 && t.psize == 4)
                        dreg = r_eaxedx;
                    else
                        dreg = r_edx;
                }

                if(to_type == 0x18)
                {
                    if (t.psize == 4)
                        to_type = 0x08;
                    else
                        to_type = 0x0a;
                }
                else if(to_type == 0x19)
                {
                    if (t.psize == 4)
                        to_type = 0x09;
                    else
                        to_type = 0x0b;
                }

                switch (to_type)
                {
                    case 0x04:
                        if (sreg.Equals(r_esi) || sreg.Equals(r_edi))
                        {
                            r.Add(inst(x86_mov_r32_rm32, r_eax, sreg, n));
                            sreg = r_eax;
                        }
                        r.Add(inst(x86_movsxbd, dreg, sreg, n));
                        break;
                    case 0x05:
                        if (sreg.Equals(r_esi) || sreg.Equals(r_edi))
                        {
                            r.Add(inst(x86_mov_r32_rm32, r_eax, sreg, n));
                            sreg = r_eax;
                        }
                        r.Add(inst(x86_movzxbd, dreg, sreg, n));
                        break;
                    case 0x06:
                        // int16
                        r.Add(inst(x86_movsxwd, dreg, sreg, n));
                        break;
                    case 0x07:
                        // uint16
                        r.Add(inst(x86_movzxwd, dreg, sreg, n));
                        break;
                    case 0x08:
                    case 0x09:
                        // nop
                        if (!sreg.Equals(dreg))
                            handle_move(dreg, sreg, r, n, c);
                        else
                            r.Add(inst(x86_nop, n));    // in case this is a br target
                        return r;
                    case 0x0a:
                        // conv to i8
                        {
                            if (t.psize == 4)
                            {
                                if (dreg.type != rt_multi)
                                    throw new NotSupportedException();

                                List<Reg> set_regs = new List<Reg>();
                                for(int i = 0; i < 63; i++)
                                {
                                    if ((dreg.mask & (1UL << i)) != 0UL)
                                        set_regs.Add(t.regs[i]);
                                }
                                if (!(set_regs[0].Equals(sreg)))
                                {
                                    handle_move(set_regs[0], sreg, r, n, c);
                                }
                                handle_move(set_regs[1], sreg, r, n, c);
                                r.Add(inst(x86_sar_rm32_imm8, set_regs[1], 31, n));
                            }
                            else
                            {
                                r.Add(inst(x86_movsxdq_r64_rm64, dreg, sreg, n));
                            }
                        }
                        break;
                    case 0x0b:
                        // conv to u8
                        if(t.psize == 4)
                        {
                            var dra = dreg.SubReg(0, 4, c.t);
                            var drb = dreg.SubReg(4, 4, c.t);
                            if (!(dra.Equals(sreg)))
                            {
                                handle_move(dra, sreg, r, n, c);
                            }
                            r.Add(inst(x86_mov_rm32_imm32, drb, new ir.Param { t = ir.Opcode.vl_c, v = 0 }, n));
                        }
                        else
                        {
                            if (dreg is ContentsReg)
                                throw new NotImplementedException();
                            r.Add(inst(x86_mov_r32_rm32, dreg, sreg, n));
                        }
                        break;
                    case 0x0c:
                    case 0x0d:
                        // conv to r4/8 (internal representation is the same)
                        {
                            if (dreg == r_edx)
                                dreg = r_xmm7;
                            r.Add(inst(x86_cvtsi2sd_xmm_rm32, dreg, sreg, n));
                            break;
                        }
                    default:
                        throw new NotImplementedException("Convert to " + to_type.ToString());
                }

                if (!dreg.Equals(actdreg))
                    handle_move(actdreg, dreg, r, n, c);

                return r;
            }
            else if (n_ct == ir.Opcode.ct_int64)
            {
                var r = new List<MCInst>();

                var si = n.stack_before.Peek(n.arg_a);
                var di = n.stack_after.Peek(n.res_a);
                var to_type = di.ts.SimpleType;

                var sreg = si.reg;
                var dreg = di.reg;

                Reg srca, srcb;

                if (t.psize == 4)
                {
                    srca = sreg.SubReg(0, 4);
                    srcb = sreg.SubReg(4, 4);
                }
                else
                    srca = sreg;

                var actdreg = dreg;
                if (dreg is ContentsReg)
                    dreg = r_edx;

                switch (to_type)
                {
                    case 0x04:
                        // i1
                        if (dreg.Equals(r_edi) || dreg.Equals(r_esi))
                            dreg = r_edx;
                        handle_move(dreg, srca, r, n, c);
                        r.Add(inst(x86_movsxbd, dreg, dreg, n));
                        break;
                    case 0x05:
                        // u1
                        if (!srca.Equals(dreg))
                            handle_move(dreg, srca, r, n, c);
                        r.Add(inst(x86_movzxbd, dreg, dreg, n));
                        break;
                    case 0x06:
                        // i2
                        handle_move(dreg, srca, r, n, c);
                        r.Add(inst(x86_movsxwd, dreg, dreg, n));
                        break;
                    case 0x07:
                        // u2
                        handle_move(dreg, srca, r, n, c);
                        r.Add(inst(x86_movzxwd, dreg, dreg, n));
                        break;
                    case 0x08:
                    case 0x09:
                    case 0x18:
                    case 0x19:
                        if (!srca.Equals(dreg))
                        {
                            handle_move(dreg, sreg.SubReg(0, 4, c.t), r, n, c);
                        }
                        // nop - ignore high 32 bits
                        else
                            r.Add(inst(x86_nop, n));    // this could be a br target
                        return r;
                    case 0x0a:
                        // conv to i8
                        handle_move(dreg, sreg, r, n, c);
                        return r;
                    case 0x0b:
                        // conv to u8
                        handle_move(dreg, sreg, r, n, c);
                        return r;
                    case 0x0c:
                    case 0x0d:
                        // conv to float
                        if (dreg.Equals(r_edx))
                            dreg = r_xmm7;
                        if (is_un)
                        {
                            return t.handle_external(t, nodes,
                                start, count, c, "__floatundidf");
                        }
                        else
                        {
                            if (t.psize == 8)
                            {
                                r.Add(inst(x86_cvtsi2sd_xmm_rm64, dreg, sreg, n));
                                break;
                            }
                            else
                            {
                                return t.handle_external(t, nodes,
                                    start, count, c, "__floatdidf");
                            }
                        }
                    default:
                        throw new NotImplementedException("Convert to " + to_type.ToString());
                }

                if (!dreg.Equals(actdreg))
                {
                    if (dreg.Equals(r_edx))
                        r.Add(inst(x86_mov_rm32_r32, actdreg, dreg, n));
                    else if (dreg.Equals(r_xmm7))
                        r.Add(inst(x86_movsd_xmmm64_xmm, actdreg, dreg, n));
                }

                return r;
            }
            else if (n_ct == ir.Opcode.ct_float)
            {
                var r = new List<MCInst>();

                var si = n.stack_before.Peek(n.arg_a);
                var di = n.stack_after.Peek(n.res_a);
                var to_type = di.ts.SimpleType;

                var sreg = si.reg;
                var dreg = di.reg;
                var act_dreg = dreg;

                switch (to_type)
                {
                    case 0x04:
                        // int8
                        if (dreg is ContentsReg)
                            act_dreg = r_eax;
                        r.Add(inst(x86_cvtsd2si_r32_xmmm64, act_dreg, sreg, n));
                        r.Add(inst(x86_movsxbd, act_dreg, act_dreg, n));
                        handle_move(dreg, act_dreg, r, n, c);
                        return r;
                    case 0x05:
                        // uint8
                        if (dreg is ContentsReg)
                            act_dreg = r_eax;
                        r.Add(inst(x86_cvtsd2si_r32_xmmm64, act_dreg, sreg, n));
                        r.Add(inst(x86_movzxbd, act_dreg, act_dreg, n));
                        handle_move(dreg, act_dreg, r, n, c);
                        return r;
                    case 0x06:
                        // int16
                        if (dreg is ContentsReg)
                            act_dreg = r_eax;
                        r.Add(inst(x86_cvtsd2si_r32_xmmm64, act_dreg, sreg, n));
                        r.Add(inst(x86_movsxwd, act_dreg, act_dreg, n));
                        handle_move(dreg, act_dreg, r, n, c);
                        return r;
                    case 0x07:
                        // uint16
                        if (dreg is ContentsReg)
                            act_dreg = r_eax;
                        r.Add(inst(x86_cvtsd2si_r32_xmmm64, act_dreg, sreg, n));
                        r.Add(inst(x86_movzxwd, act_dreg, act_dreg, n));
                        handle_move(dreg, act_dreg, r, n, c);
                        return r;
                    case 0x08:
                        // int32
                        if (dreg is ContentsReg)
                            act_dreg = r_eax;
                        r.Add(inst(x86_cvtsd2si_r32_xmmm64, act_dreg, sreg, n));
                        handle_move(dreg, act_dreg, r, n, c);
                        return r;
                    case 0x09:
                        // uint32
                        //fc_override = "__fixunsdfsi";
                        return t.handle_external(t, nodes, start, count, c,
                            "__fixunsdfsi");
                    case 0x0a:
                        // int64
                        //fc_override = "__fixdfti";
                        if (t.psize == 8)
                        {
                            if (dreg is ContentsReg)
                                act_dreg = x86_64.x86_64_Assembler.r_rax;
                            r.Add(inst(x86_cvtsd2si_r64_xmmm64, act_dreg, sreg, n));
                            handle_move(dreg, act_dreg, r, n, c);
                            return r;
                        }
                        else
                        {
                            return t.handle_external(t, nodes, start, count, c,
                                "__fixdfdi");
                        }
                    case 0x0b:
                        // uint64
                        //fc_override = "__fixunsdfti";
                        return t.handle_external(t, nodes, start, count, c,
                            "__fixunsdfdi");
                    case 0x0c:
                    case 0x0d:
                        // float to float (all floats are 64-bit internally)
                        if (!dreg.Equals(sreg))
                            handle_move(dreg, sreg, r, n, c);
                        else
                            r.Add(inst(x86_nop, n));        // this could be a br target
                        return r;
                    default:
                        throw new NotImplementedException();
                }
            }
            return null;
        }

        internal static List<MCInst> handle_stind(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct2 = n.ct2;
            var addr = n.stack_before.Peek(n.arg_a).reg;
            var val = n.stack_before.Peek(n.arg_b).reg;
            bool is_tls = n.imm_ul == 1UL;
            if (n_ct2 == ir.Opcode.ct_int32 && t.psize == 4)
            {
                List<MCInst> r = new List<MCInst>();
                if (addr is ContentsReg)
                {
                    r.Add(inst(x86_mov_r32_rm32, r_eax, addr, n));
                    addr = r_eax;
                }
                if (val is ContentsReg || ((val.Equals(r_esi) || val.Equals(r_edi) && n.vt_size != 4)))
                {
                    r.Add(inst(x86_mov_r32_rm32, r_edx, val, n));
                    val = r_edx;
                }

                if(n.vt_size == 1 && c.t.psize == 4)        // r8 and r/m8 can only be al/bl/cl/dl on x86_32
                {
                    if (addr.Equals(r_edi) || addr.Equals(r_esi))
                    {
                        handle_move(r_eax, addr, r, n, c);
                        addr = r_eax;
                    }
                    if (val.Equals(r_edi) || val.Equals(r_esi))
                    {
                        handle_move(r_edx, val, r, n, c);
                        val = r_edx;
                    }
                }

                switch (n.vt_size)
                {
                    case 1:
                        r.Add(inst(x86_mov_rm8disp_r8, addr, 0, val, n, is_tls));
                        break;
                    case 2:
                        r.Add(inst(x86_mov_rm16disp_r16, addr, 0, val, n, is_tls));
                        break;
                    case 4:
                        r.Add(inst(x86_mov_rm32disp_r32, addr, 0, val, n, is_tls));
                        break;
                    default:
                        throw new NotImplementedException();
                }

                return r;
            }
            else if((n_ct2 == ir.Opcode.ct_int64 ||
                n_ct2 == ir.Opcode.ct_int32)
                && t.psize == 8)
            {
                List<MCInst> r = new List<MCInst>();
                if (addr is ContentsReg)
                {
                    r.Add(inst(x86_mov_r64_rm64, r_eax, addr, n));
                    addr = r_eax;
                }
                if (val is ContentsReg || ((val.Equals(r_esi) || val.Equals(r_edi) && n.vt_size < 4)))
                {
                    r.Add(inst(x86_mov_r64_rm64, r_edx, val, n));
                    val = r_edx;
                }

                if (n.vt_size == 1 && c.t.psize == 4)
                {
                    if (addr.Equals(r_edi) || addr.Equals(r_esi))
                        throw new NotImplementedException();
                    if (val.Equals(r_edi) || addr.Equals(r_esi))
                        throw new NotImplementedException();
                }

                switch (n.vt_size)
                {
                    case 1:
                        r.Add(inst(x86_mov_rm8disp_r8, addr, 0, val, n, is_tls));
                        break;
                    case 2:
                        r.Add(inst(x86_mov_rm16disp_r16, addr, 0, val, n, is_tls));
                        break;
                    case 4:
                        r.Add(inst(x86_mov_rm32disp_r32, addr, 0, val, n, is_tls));
                        break;
                    case 8:
                        r.Add(inst(x86_mov_rm64disp_r64, addr, 0, val, n, is_tls));
                        break;
                    default:
                        throw new NotImplementedException();
                }

                return r;
            }
            else if (n_ct2 == ir.Opcode.ct_int64 &&
                t.psize == 4)
            {
                var dra = val.SubReg(0, 4, c.t);
                var drb = val.SubReg(4, 4, c.t);

                List<MCInst> r = new List<MCInst>();
                if (addr is ContentsReg)
                {
                    r.Add(inst(x86_mov_r32_rm32, r_eax, addr, n));
                    addr = r_eax;
                }

                handle_stind(dra, addr, 0, 4, r, n, c, is_tls);
                handle_stind(drb, addr, 4, 4, r, n, c, is_tls);
                return r;
            }
            else if (n_ct2 == ir.Opcode.ct_vt)
            {
                if (n.vt_size <= 16 && val is ContentsReg)
                {
                    // handle as a bunch of moves
                    List<MCInst> r = new List<MCInst>();
                    if (addr is ContentsReg)
                    {
                        handle_move(r_edx, addr, r, n, c);
                        addr = r_edx;
                    }
                    addr = new ContentsReg { basereg = addr, size = n.vt_size };
                    handle_move(addr, val, r, n, c, r_eax, -1, false, is_tls);
                    return r;
                }
                else
                {
                    if (is_tls)
                        throw new NotImplementedException();
                    // emit call to memcpy(dest, src, n)
                    List<MCInst> r = new List<MCInst>();
                    r.AddRange(handle_call(n, c,
                        c.special_meths.GetMethodSpec(c.special_meths.memcpy),
                        new ir.Param[] {
                            addr,
                            new ir.Param { t = ir.Opcode.vl_mreg, mreg = val, want_address = true },
                            n.vt_size },
                        null, "memcpy"));
                    return r;
                }
            }
            else if(n_ct2 == ir.Opcode.ct_float)
            {
                List<MCInst> r = new List<MCInst>();
                if (addr is ContentsReg)
                {
                    handle_move(r_edx, addr, r, n, c);
                    addr = r_edx;
                }
                if(val is ContentsReg)
                {
                    handle_move(r_xmm7, val, r, n, c);
                    val = r_xmm7;
                }

                addr = new ContentsReg { basereg = addr, size = n.vt_size };

                if (n.vt_size == 4)
                {
                    // convert to r4, then store
                    r.Add(inst(x86_cvtsd2ss_xmm_xmmm64, val, val, n, is_tls));
                    r.Add(inst(x86_movss_xmmm32_xmm, addr, val, n, is_tls));
                }
                else if (n.vt_size == 8)
                {
                    // direct store
                    r.Add(inst(x86_movsd_xmmm64_xmm, addr, val, n, is_tls));
                }
                else
                    throw new NotSupportedException();
                return r;
            }

            return null;
        }

        internal static List<MCInst> handle_ldind(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct = n.ctret;
            var addr = n.stack_before.Peek(n.arg_a).reg;
            var val = n.stack_after.Peek(n.res_a).reg;
            bool is_tls = n.imm_ul == 1UL;
            if (n_ct == ir.Opcode.ct_int32 ||
                (n_ct == ir.Opcode.ct_int64 && t.psize == 8))
            {
                List<MCInst> r = new List<MCInst>();
                handle_ldind(val, addr, 0, n.vt_size, r, n, t, is_tls);

                return r;
            }
            else if (n_ct == ir.Opcode.ct_int64 && t.psize == 4)
            {
                List<MCInst> r = new List<MCInst>();

                var dra = val.SubReg(0, 4, c.t);
                var drb = val.SubReg(4, 4, c.t);

                /* Do this here so its only done once.  We don't need the [esi]/[edi]
                 * check as the vt_size is guaranteed to be 4.  However, the address
                 * used in the second iteration may overwrite the return value in
                 * the first, so we need to check for that.  We do it backwards
                 * so that this is less likely (e.g. stack goes from
                 * ..., esi to ..., esi, edi thus assigning first to edi won't
                 * cause a problem */
                if (addr is ContentsReg || addr.Equals(drb))
                {
                    r.Add(inst(x86_mov_r32_rm32, r_eax, addr, n));
                    addr = r_eax;
                }

                handle_ldind(drb, addr, 4, 4, r, n, t, is_tls);
                handle_ldind(dra, addr, 0, 4, r, n, t, is_tls);

                return r;
            }
            else if(n_ct == ir.Opcode.ct_vt)
            {
                // handle as a bunch of moves
                List<MCInst> r = new List<MCInst>();
                if(addr is ContentsReg)
                {
                    handle_move(r_edx, addr, r, n, c);
                    addr = r_edx;
                }
                addr = new ContentsReg { basereg = addr, size = n.vt_size };
                handle_move(val, addr, r, n, c, r_eax, -1, is_tls);
                return r;                    
            }
            else if(n_ct == ir.Opcode.ct_float)
            {
                List<MCInst> r = new List<MCInst>();
                if(addr is ContentsReg)
                {
                    handle_move(r_edx, addr, r, n, c);
                    addr = r_edx;
                }
                var act_dest = val;
                if(act_dest is ContentsReg)
                {
                    act_dest = r_xmm7;
                }
                addr = new ContentsReg { basereg = addr, size = n.vt_size };
                switch(n.vt_size)
                {
                    case 4:
                        r.Add(inst(x86_cvtss2sd_xmm_xmmm32, act_dest, addr, n, is_tls));
                        break;
                    case 8:
                        r.Add(inst(x86_movsd_xmm_xmmm64, act_dest, addr, n, is_tls));
                        break;
                    default:
                        throw new NotSupportedException();
                }
                handle_move(val, act_dest, r, n, c);
                return r;
            }
            
            return null;
        }

        internal static List<MCInst> handle_ldfp(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var dest = n.stack_after.Peek(n.res_a).reg;

            List<MCInst> r = new List<MCInst>();
            r.Add(inst(t.psize == 4 ? x86_mov_rm32_r32 : x86_mov_rm64_r64, dest, r_ebp, n));

            return r;
        }

        internal static List<MCInst> handle_ldlabaddr(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var dest = n.stack_after.Peek(n.res_a).reg;

            int oc = x86_mov_rm32_imm32;
            if (n.imm_ul == 1 && t.psize == 8)
                oc = x86_mov_rm64_imm32;        // ensure TLS address loads are sign-extended    

            if ((string)t.Options["mcmodel"] == "large")
                oc = x86_mov_r64_imm64;

            List<MCInst> r = new List<MCInst>();
            if (dest is ContentsReg)
            {
                r.Add(inst(oc, r_eax, new ir.Param { t = ir.Opcode.vl_str, str = n.imm_lab, v = n.imm_l, v2 = (long)n.imm_ul }, n));
                handle_move(dest, r_eax, r, n, c);
            }
            else
                r.Add(inst(oc, dest, new ir.Param { t = ir.Opcode.vl_str, str = n.imm_lab, v = n.imm_l, v2 = (long)n.imm_ul }, n));
            return r;
        }

        internal static List<MCInst> handle_zeromem(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var sizev = n.stack_before.Peek(n.arg_a);
            var dest = n.stack_before.Peek(n.arg_b).reg;

            int size = (int)n.imm_l;
            if(size == 0)
            {
                if (sizev.min_l == sizev.max_l)
                    size = (int)sizev.min_l;
            }
            if (size == 0)
            {
                // emit call to memset here
                throw new NotImplementedException();
            }

            List<MCInst> r = new List<MCInst>();
            handle_zeromem(t, dest, size, r, n, c);
            return r;
        }

        internal static List<MCInst> handle_ldelem(
            Target t,
            List<CilNode.IRNode> nodes,
            int start, int count, Code c)
        {
            var n = nodes[start];
            var n1 = nodes[start + 1];
            var n2 = nodes[start + 2];
            var n3 = nodes[start + 3];
            var n4 = nodes[start + 4];
            var n5 = nodes[start + 5];
            var n6 = nodes[start + 6];
            var n7 = nodes[start + 7];
            var n8 = nodes[start + 8];

            // sanity checks
            if (n.res_a != 0)
                return null;
            if (n1.arg_a != 1 || n1.arg_b != 0 || n1.res_a != 0)
                return null;
            if (n2.arg_a != 1 || n2.res_a != 0)
                return null;
            if (n3.res_a != 0)
                return null;
            if (n4.arg_a != 1 || n4.arg_b != 0 || n4.res_a != 0)
                return null;
            if (n5.arg_a != 0 || n5.res_a != 0)
                return null;
            if (n6.arg_a != 1 || n6.arg_b != 0 || n6.res_a != 0)
                return null;
            if (n7.arg_a != 0 || n7.res_a != 0)
                return null;
            if (n8.arg_a != 0 || n8.res_a != 0)
                return null;

            // extract values
            var arr = n.stack_before.Peek(1).reg;
            var index = n.stack_before.Peek(0).reg;
            var tsize = n.imm_l;
            var dataoffset = n3.imm_l;
            var dest = n8.stack_after.Peek(0).reg;

            // ensure this is possible
            if (arr is ContentsReg)
                return null;
            if (index is ContentsReg)
                return null;
            if (tsize > t.psize)
                return null;
            if (tsize != 1 && tsize != 2 && tsize != 4 && tsize != 8)
                return null;
            if (dest is ContentsReg)
                return null;
            if (dataoffset > int.MaxValue)
                return null;
            if (dest.Equals(index))
                return null;

            /* handle as:
             * 
             * mov dest, [arr + dataoffset]
             * mov(szbwx) dest, [dest + index * tsize]
             */

            List<MCInst> r = new List<MCInst>();
            r.Add(inst(t.psize == 4 ? x86_mov_r32_rm32disp : x86_mov_r64_rm64disp,
                dest, arr, dataoffset, n));

            // get correct ld opcode
            int oc = 0;
            switch (tsize)
            {
                case 1:
                    if (n7.imm_l == 1)
                        oc = t.psize == 4 ? x86_movsxbd_r32_rm8sibscaledisp : x86_movsxbq_r64_rm8sibscaledisp;
                    else
                        oc = t.psize == 4 ? x86_movzxbd_r32_rm8sibscaledisp : x86_movzxbq_r64_rm8sibscaledisp;
                    break;
                case 2:
                    if (n7.imm_l == 1)
                        oc = t.psize == 4 ? x86_movsxwd_r32_rm16sibscaledisp : x86_movsxwq_r64_rm16sibscaledisp;
                    else
                        oc = t.psize == 4 ? x86_movzxwd_r32_rm16sibscaledisp : x86_movzxwq_r64_rm16sibscaledisp;
                    break;
                case 4:
                    if (n7.imm_l == 1)
                        oc = t.psize == 4 ? x86_mov_r32_rm32sibscaledisp : x86_movsxdq_r64_rm32sibscaledisp;
                    else
                        oc = x86_mov_r32_rm32sibscaledisp;
                    break;
                case 8:
                    if (t.psize != 8)
                        return null;
                    oc = x86_mov_r64_rm64sibscaledisp;
                    break;
                default:
                    return null;
            }

            r.Add(inst(oc, dest, dest, index, tsize, 0, n));

            return r;
        }

        internal static List<MCInst> handle_ldc_add(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n2 = nodes[start + 1];

            var obj = n2.stack_before.Peek(n2.arg_a).reg;
            if (n2.arg_b != 0)
                return null;
            var res = n2.stack_after.Peek(n2.res_a).reg;

            var val = n.imm_l;
            if (val < int.MinValue || val > int.MaxValue)
                return null;

            List<MCInst> r = new List<MCInst>();
            handle_move(res, obj, r, n, c);
            if(val != 0)
            {
                if (val <= 127 && val >= -128)
                    r.Add(inst(t.psize == 4 ? x86_add_rm32_imm8 : x86_add_rm64_imm8, res, val, n));
                else
                    r.Add(inst(t.psize == 4 ? x86_add_rm32_imm32 : x86_add_rm64_imm32, res, val, n));
            }
            return r;
        }

        internal static List<MCInst> handle_ldc_add_ldind(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n2 = nodes[start + 1];
            var n3 = nodes[start + 2];

            var obj = n2.stack_before.Peek(n2.arg_a).reg;
            if (n2.arg_b != 0)
                return null;
            var val = n3.stack_after.Peek(n3.res_a).reg;
            if (n3.arg_a != 0)
                return null;

            if (n3.imm_ul == 1)     // TLS
                throw new NotImplementedException();

            var disp = n.imm_l;

            if (disp < int.MinValue || disp > int.MaxValue)
                return null;

            if (obj is ContentsReg)
                return null;
            if (val is ContentsReg)
                return null;

            int size = n3.vt_size;
            if (size > t.psize)
                return null;

            int oc = 0;
            switch (n3.ctret)
            {
                case ir.Opcode.ct_int32:
                case ir.Opcode.ct_int64:
                case ir.Opcode.ct_intptr:
                    switch (size)
                    {
                        case 1:
                            if (n3.imm_l == 1)
                                oc = t.psize == 4 ? x86_movsxbd_r32_rm8disp : x86_movsxbq_r64_rm8disp;
                            else
                                oc = t.psize == 4 ? x86_movzxbd_r32_rm8disp : x86_movzxbq_r64_rm8disp;
                            break;
                        case 2:
                            if (n3.imm_l == 1)
                                oc = t.psize == 4 ? x86_movsxwd_r32_rm16disp : x86_movsxwq_r64_rm16disp;
                            else
                                oc = t.psize == 4 ? x86_movzxwd_r32_rm16disp : x86_movzxwq_r64_rm16disp;
                            break;
                        case 4:
                            oc = x86_mov_r32_rm32disp;
                            break;
                        case 8:
                            if (t.psize != 8)
                                return null;
                            oc = x86_mov_r64_rm64disp;
                            break;
                        default:
                            return null;
                    }
                    break;
                case ir.Opcode.ct_float:
                    switch (size)
                    {
                        case 4:
                            oc = x86_cvtss2sd_xmm_xmmm32disp;
                            break;
                        case 8:
                            oc = x86_movsd_xmm_xmmm64disp;
                            break;
                        default:
                            return null;
                    }
                    break;
                default:
                    return null;
            }

            List<MCInst> r = new List<MCInst>();
            r.Add(inst(oc, val, obj, disp, n));

            return r;
        }

        internal static List<MCInst> handle_ldc_ldc_add_stind(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n2 = nodes[start + 1];
            var n3 = nodes[start + 2];
            var n4 = nodes[start + 3];

            var obj = n3.stack_before.Peek(n3.arg_a).reg;
            if (n2.arg_b != 0)
                return null;
            if (n4.arg_a != 0)
                return null;
            if (n4.arg_b != 1)
                return null;

            if (n4.imm_ul == 1)     // TLS
                throw new NotImplementedException();

            var disp = n2.imm_l;
            if (disp < int.MinValue || disp > int.MaxValue)
                return null;

            var val = n.imm_l;
            if (val < int.MinValue || val > int.MaxValue)
                return null;


            if (obj is ContentsReg)
                return null;

            int size = n4.vt_size;
            if (size > t.psize)
                return null;

            int oc = 0;
            switch (size)
            {
                case 1:
                    oc = x86_mov_rm8disp_imm32;
                    break;
                case 2:
                    oc = x86_mov_rm16disp_imm32;
                    break;
                case 4:
                    oc = x86_mov_rm32disp_imm32;
                    break;
                case 8:
                    oc = x86_mov_rm64disp_imm32;
                    break;
                default:
                    return null;
            }

            List<MCInst> r = new List<MCInst>();
            r.Add(inst(oc, obj, disp, val, n));

            return r;
        }

        internal static List<MCInst> handle_ldc_add_stind(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n2 = nodes[start + 1];
            var n3 = nodes[start + 2];
            var n_ct = n3.ct2;

            var obj = n2.stack_before.Peek(n2.arg_a).reg;
            if (n2.arg_b != 0)
                return null;
            var val = n3.stack_before.Peek(n3.arg_b).reg;
            if (n3.arg_a != 0)
                return null;
            var disp = n.imm_l;

            if (n3.imm_ul == 1)     // TLS
                throw new NotImplementedException();

            if (disp < int.MinValue || disp > int.MaxValue)
                return null;

            if (obj is ContentsReg)
                return null;
            if (val is ContentsReg)
                return null;

            int size = n3.vt_size;
            if (size > t.psize)
                return null;

            List<MCInst> r = new List<MCInst>();
            int oc = 0;
            switch (n_ct)
            {
                case ir.Opcode.ct_int32:
                case ir.Opcode.ct_int64:
                case ir.Opcode.ct_intptr:
                    switch (size)
                    {
                        case 1:
                            oc = x86_mov_rm8disp_r8;
                            break;
                        case 2:
                            oc = x86_mov_rm16disp_r16;
                            break;
                        case 4:
                            oc = x86_mov_rm32disp_r32;
                            break;
                        case 8:
                            oc = x86_mov_rm64disp_r64;
                            break;
                        default:
                            return null;
                    }
                    break;
                case ir.Opcode.ct_float:
                    if (size == 4)
                    {
                        // convert to r4 first
                        r.Add(inst(x86_cvtsd2ss_xmm_xmmm64, r_xmm7, val, n));
                        val = r_xmm7;
                        oc = x86_movss_xmmm32disp_xmm;
                    }
                    else
                        oc = x86_movsd_xmmm64disp_xmm;
                    break;
                default:
                    return null;
            }

            r.Add(inst(oc, obj, disp, val, n));

            return r;
        }

        internal static List<MCInst> handle_ldc_zeromem(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n2 = nodes[start + 1];

            if (n2.arg_a != 0)
                return null;

            var dest = n2.stack_before.Peek(n.arg_b).reg;
            var size = n.imm_l;

            List<MCInst> r = new List<MCInst>();
            handle_zeromem(t, dest, (int)size, r, n2, c);
            return r;
        }

        private static void handle_zeromem(Target t, Reg dest, int size,
            List<MCInst> r, CilNode.IRNode n, Code c)
        {
            if (size > t.psize * 4)
            {
                // emit call to memset(void *s, int c, size_t n) here
                r.AddRange(handle_call(n, c,
                    c.special_meths.GetMethodSpec(c.special_meths.memset),
                    new ir.Param[]
                    {
                            new ir.Param { t = ir.Opcode.vl_mreg, mreg = dest },
                            0,
                            size
                    },
                    null, "memset"));
                return;
            }

            if(dest is ContentsReg)
            {
                handle_move(r_edx, dest, r, n, c);
                dest = r_edx;
            }

            size = util.util.align(size, t.psize);
            var cr = new ContentsReg { basereg = dest, size = size };

            for(int i = 0; i < size; i += t.psize)
            {
                var d = cr.SubReg(i, t.psize);
                r.Add(inst(t.psize == 4 ? x86_mov_rm32_imm32 : x86_mov_rm64_imm32, d, 0, n));
            }
        }

        internal static List<MCInst> handle_ldc(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct = n.ctret;

            switch(n_ct)
            {
                case ir.Opcode.ct_int32:
                    return new List<MCInst> { inst(x86_mov_rm32_imm32, n.stack_after.Peek(n.res_a).reg, (int)n.imm_l, n) };

                case ir.Opcode.ct_float:
                    if (n.vt_size == 4)
                    {
                        var dest = n.stack_after.Peek(n.res_a).reg;
                        var act_dest = dest;
                        var r = new List<MCInst>();

                        if (dest is ContentsReg)
                            act_dest = r_xmm7;

                        r.Add(inst(x86_push_imm32, n.imm_l, n));
                        r.Add(inst(x86_cvtss2sd_xmm_xmmm32, act_dest,
                            new ContentsReg { basereg = r_esp, size = 4 }, n));
                        r.Add(inst(t.psize == 4 ? x86_add_rm32_imm8 : x86_add_rm64_imm8, r_esp, t.psize, n));
                        handle_move(dest, act_dest, r, n, c);
                        return r;
                    }
                    else if (n.vt_size == 8)
                    {
                        if (t.psize == 4)
                        {
                            var high_val = BitConverter.ToInt32(n.imm_val, 4);
                            var low_val = BitConverter.ToInt32(n.imm_val, 0);

                            var dest = n.stack_after.Peek(n.res_a).reg;
                            var act_dest = dest;
                            var r = new List<MCInst>();

                            if (dest is ContentsReg)
                                act_dest = r_xmm7;

                            r.Add(inst(x86_push_imm32, high_val, n));
                            r.Add(inst(x86_push_imm32, low_val, n));
                            r.Add(inst(x86_movsd_xmm_xmmm64, act_dest,
                                new ContentsReg { basereg = r_esp, size = 8 }, n));
                            r.Add(inst(x86_add_rm32_imm8, r_esp, 8, n));
                            handle_move(dest, act_dest, r, n, c);
                            return r;
                        }
                        else
                        {
                            var dest = n.stack_after.Peek(n.res_a).reg;
                            var act_dest = dest;

                            if (dest is ContentsReg)
                                act_dest = r_xmm7;

                            var r = new List<MCInst>();
                            r.Add(inst(x86_mov_r64_imm64, r_eax, n.imm_l, n));
                            r.Add(inst(x86_push_r32, r_eax, n));
                            r.Add(inst(x86_movsd_xmm_xmmm64, act_dest,
                                new ContentsReg { basereg = r_esp, size = 8 }, n));
                            r.Add(inst(x86_add_rm64_imm8, r_esp, 8, n));
                            handle_move(dest, act_dest, r, n, c);
                            return r;
                        }
                    }
                    else
                        throw new NotSupportedException();

                case ir.Opcode.ct_int64:
                    {
                        var dest = n.stack_after.Peek(n.res_a).reg;

                        var da = dest.SubReg(0, 4);
                        var db = dest.SubReg(4, 4);

                        var sa = BitConverter.ToInt32(n.imm_val, 0);
                        var sb = BitConverter.ToInt32(n.imm_val, 4);

                        var r = new List<MCInst>();
                        r.Add(inst(x86_mov_rm32_imm32, da, sa, n));
                        r.Add(inst(x86_mov_rm32_imm32, db, sb, n));
                        return r;
                    }

                default:
                    throw new NotImplementedException();
            }

            return null;
        }

        internal static List<MCInst> handle_ldloc(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct = n.ctret;
            var src = c.lv_locs[(int)n.imm_l];
            var dest = n.stack_after.Peek(n.res_a).reg;
            var vt_size = n.vt_size;

            var r = new List<MCInst>();
            if (n_ct == ir.Opcode.ct_int32 ||
                n_ct == ir.Opcode.ct_float ||
                n_ct == ir.Opcode.ct_vt ||
                (n_ct == ir.Opcode.ct_int64 && t.psize == 8))
            {
                if (vt_size < 4)
                {
                    // perform a sign/zero extension load
                    int oc = 0;
                    if (vt_size == 1)
                    {
                        if (n.imm_ul == 0)
                            oc = x86_movzxbd;
                        else
                            oc = x86_movsxbd;
                    }
                    else if (vt_size == 2)
                    {
                        if (n.imm_ul == 0)
                            oc = x86_movzxwd;
                        else
                            oc = x86_movsxwd;
                    }
                    else
                        throw new NotSupportedException("Invalid vt_size");

                    if (dest is ContentsReg)
                    {
                        r.Add(inst(oc, r_eax, src, n));
                        handle_move(dest, r_eax, r, n, c);
                    }
                    else
                    {
                        r.Add(inst(oc, dest, src, n));
                    }
                }
                else
                {
                    handle_move(dest, src, r, n, c, null,
                        n_ct == ir.Opcode.ct_int32 ? 4 : -1);
                }
                return r;
            }
            else if (n_ct == ir.Opcode.ct_int64)
            {
                var desta = dest.SubReg(0, 4);
                var destb = dest.SubReg(4, 4);
                var srca = src.SubReg(0, 4);
                var srcb = src.SubReg(4, 4);

                handle_move(desta, srca, r, n, c);
                handle_move(destb, srcb, r, n, c);

                return r;
            }
            return null;
        }

        internal static List<MCInst> handle_stloc(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct = n.ct;
            var src = n.stack_before.Peek(n.arg_a).reg;
            var dest = c.lv_locs[(int)n.imm_l];

            var r = new List<MCInst>();

            if (n_ct == ir.Opcode.ct_int32 ||
                n_ct == ir.Opcode.ct_float ||
                n_ct == ir.Opcode.ct_vt ||
                (n_ct == ir.Opcode.ct_int64 && t.psize == 8))
            {
                handle_move(dest, src, r, n, c);
                return r;
            }
            else if (n_ct == ir.Opcode.ct_int64)
            {
                var srca = src.SubReg(0, 4);
                var srcb = src.SubReg(4, 4);
                var desta = dest.SubReg(0, 4);
                var destb = dest.SubReg(4, 4);

                handle_move(desta, srca, r, n, c);
                handle_move(destb, srcb, r, n, c);

                return r;
            }
            return null;
        }

        internal static List<MCInst> handle_rem(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var srca = n.stack_before.Peek(n.arg_a).reg;
            var srcb = n.stack_before.Peek(n.arg_b).reg;
            var dest = n.stack_after.Peek(n.res_a).reg;
            var n_ct = n.ct;

            switch(n_ct)
            {
                case ir.Opcode.ct_int32:
                    {
                        List<MCInst> r = new List<MCInst>();
                        handle_move(r_eax, srca, r, n, c);
                        r.Add(inst(x86_xor_r32_rm32, r_edx, r_edx, n));
                        r.Add(inst(x86_idiv_rm32, srcb, n));
                        handle_move(dest, r_edx, r, n, c);
                        return r;
                    }
                case ir.Opcode.ct_int64:
                    if(t.psize == 8)
                    {
                        List<MCInst> r = new List<MCInst>();
                        handle_move(x86_64.x86_64_Assembler.r_rax, srca, r, n, c);
                        r.Add(inst(x86_xor_r64_rm64, x86_64.x86_64_Assembler.r_rdx, x86_64.x86_64_Assembler.r_rdx, n));
                        r.Add(inst(x86_idiv_rm64, srcb, n));
                        handle_move(dest, x86_64.x86_64_Assembler.r_rdx, r, n, c);
                        return r;
                    }
                    else
                        return t.handle_external(t, nodes, start, count, c, "__moddi3");
                case ir.Opcode.ct_float:
                    {
                        // (a - b * floor(a / b))

                        // 1) do work in xmm7
                        List<MCInst> r = new List<MCInst>();
                        handle_move(r_xmm7, srca, r, n, c);

                        // 2) xmm7 <- a/b
                        r.Add(inst(x86_divsd_xmm_xmmm64, r_xmm7, srcb, n));

                        // 3) xmm 7 <- floor(a/b)
                        if ((bool)t.Options["sse4_1"] == true)
                        {
                            r.Add(inst(x86_roundsd_xmm_xmmm64_imm8, r_xmm7, r_xmm7, 3, n));
                        }
                        else
                        {
                            int pl = 0;
                            handle_push(r_xmm7, ref pl, r, n, c);
                            r.AddRange(handle_call(n, c,
                                c.special_meths.GetMethodSpec(c.special_meths.rint),
                                new ir.Param[] { r_xmm7 }, dest, "floor"));
                            handle_pop(r_xmm7, ref pl, r, n, c);
                        }

                        // 4) xmm7 <- b * floor(a/b)
                        r.Add(inst(x86_mulsd_xmm_xmmm64, r_xmm7, srcb, n));

                        if (!(dest is ContentsReg))
                        {
                            // 5) dest <- a
                            handle_move(dest, srca, r, n, c);

                            // 6) dest <- a - xmm7
                            r.Add(inst(x86_subsd_xmm_xmmm64, dest, r_xmm7, n));
                        }
                        else
                        {
                            // 5) dest <- xmm7 - a
                            r.Add(inst(x86_subsd_xmm_xmmm64, r_xmm7, srca, n));
                            handle_move(dest, r_xmm7, r, n, c);

                            // 6) xmm7 <- 0
                            r.Add(inst(x86_xorpd_xmm_xmmm128, r_xmm7, r_xmm7, n));

                            // 7) xmm7 <- -(dest)
                            r.Add(inst(x86_subsd_xmm_xmmm64, r_xmm7, dest, n));

                            // 8) dest <- -dest
                            handle_move(dest, r_xmm7, r, n, c);
                        }
                        return r;
                    }
                    return t.handle_external(t, nodes, start, count, c, "fmod");
            }

            throw new NotImplementedException();
        }

        internal static List<MCInst> handle_ldarga(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var src = c.la_locs[(int)n.imm_l];
            var dest = n.stack_after.Peek(n.res_a).reg;

            if (!(src is ContentsReg))
                throw new Exception("ldarga from " + src.ToString());

            var act_dest = dest;
            if (dest is ContentsReg)
                dest = r_eax;

            List<MCInst> r = new List<MCInst>();
            if (!(src is ContentsReg))
                throw new NotImplementedException();
            r.Add(inst(t.psize == 4 ? x86_lea_r32 : x86_lea_r64, dest, src, n));

            handle_move(act_dest, dest, r, n, c);

            return r;
        }

        internal static List<MCInst> handle_ldloca(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var src = c.lv_locs[(int)n.imm_l];
            var dest = n.stack_after.Peek(n.res_a).reg;

            if (!(src is ContentsReg))
                throw new Exception("ldloca from " + src.ToString());

            var act_dest = dest;
            if (dest is ContentsReg)
                dest = r_eax;

            List<MCInst> r = new List<MCInst>();
            if (!(src is ContentsReg))
                throw new NotImplementedException();

            r.Add(inst(t.psize == 4 ? x86_lea_r32 : x86_lea_r64, dest, src, n));

            handle_move(act_dest, dest, r, n, c);

            return r;
        }

        internal static List<MCInst> handle_starg(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct = n.ct;
            var dest = c.la_locs[(int)n.imm_l];
            var src = n.stack_before.Peek(n.arg_a).reg;

            var r = new List<MCInst>();
            if (n_ct == ir.Opcode.ct_int32 ||
                n_ct == ir.Opcode.ct_float ||
                n_ct == ir.Opcode.ct_vt ||
                n_ct == ir.Opcode.ct_int64)
            {
                handle_move(dest, src, r, n, c);
                return r;
            }
            return null;
        }

        internal static List<MCInst> handle_syncvalswap(
            Target t,
            List<CilNode.IRNode> nodes,
            int start, int count, Code c)
        {
            var n = nodes[start];

            var r = new List<MCInst>();
            var size = n.imm_l;

            var ptr = n.stack_before.Peek(n.arg_a).reg;
            var newval = n.stack_before.Peek(n.arg_b).reg;
            var dest = n.stack_after.Peek(n.res_a).reg;

            bool is_tls = ir.Opcode.IsTLSCT(n.stack_before.Peek(n.arg_a).ct);

            if ((ptr is ContentsReg) && (newval is ContentsReg))
            {
                // we need another spare register for this
                throw new NotImplementedException();
            }
            if (ptr is ContentsReg)
            {
                handle_move(r_edx, ptr, r, n, c);
                ptr = r_edx;
            }
            if (newval is ContentsReg)
            {
                handle_move(r_edx, newval, r, n, c);
                newval = r_edx;
            }

            int oc = 0;
            int oc2 = 0;
            switch (size)
            {
                case 1:
                    oc = x86_lock_xchg_rm8ptr_r8;
                    //if (issigned)
                    //    throw new NotImplementedException();
                    //else
                        oc2 = t.psize == 4 ? x86_movzxbd : x86_movzxbq;
                    break;
                case 4:
                    oc = x86_lock_xchg_rm32ptr_r32;
                    oc2 = 0;
                    break;
                case 8:
                    oc = x86_lock_xchg_rm64ptr_r64;
                    oc2 = 0;
                    break;
                default:
                    throw new NotImplementedException();
            }

            r.Add(inst(oc, new ContentsReg { basereg = ptr }, newval, n, is_tls));

            if (dest is ContentsReg)
            {
                if (oc2 != 0)
                    r.Add(inst(oc2, r_eax, newval, n));
                handle_move(dest, r_eax, r, n, c);
            }
            else if (oc2 != 0)
                r.Add(inst(oc2, dest, newval, n));
            else
                handle_move(dest, newval, r, n, c);

            return r;
        }

        internal static List<MCInst> handle_syncvalcompareandswap(
            Target t,
            List<CilNode.IRNode> nodes,
            int start, int count, Code c)
        {
            var n = nodes[start];

            var r = new List<MCInst>();
            var size = n.imm_l;
            var issigned = n.imm_ul == 0 ? false : true;


            int oc = 0;
            int oc2 = 0;
            switch(size)
            {
                case 1:
                    oc = x86_lock_cmpxchg_rm8_r8;
                    if (issigned)
                        throw new NotImplementedException();
                    else
                        oc2 = t.psize == 4 ? x86_movzxbd : x86_movzxbq;
                    break;
                case 4:
                    oc = x86_lock_cmpxchg_rm32_r32;
                    oc2 = 0;
                    break;
                case 8:
                    oc = x86_lock_cmpxchg_rm64_r64;
                    oc2 = 0;
                    break;
                default:
                    throw new NotImplementedException();
            }

            var ptr = n.stack_before.Peek(n.arg_c).reg;
            var oldval = n.stack_before.Peek(n.arg_b).reg;
            var newval = n.stack_before.Peek(n.arg_a).reg;
            var dest = n.stack_after.Peek(n.res_a).reg;

            bool is_tls = ir.Opcode.IsTLSCT(n.stack_before.Peek(n.arg_c).ct);

            // lock cmpxchg takes the old value in rax,
            //  ptr in first argument and new val in second
            // the old value is returned in rax regardless of
            // success or failure

            if((ptr is ContentsReg) && (newval is ContentsReg))
            {
                // we need another spare register for this
                throw new NotImplementedException();
            }
            if(ptr is ContentsReg)
            {
                handle_move(r_edx, ptr, r, n, c);
                ptr = r_edx;
            }
            if(newval is ContentsReg)
            {
                handle_move(r_edx, newval, r, n, c);
                newval = r_edx;
            }
            handle_move(r_eax, oldval, r, n, c);

            r.Add(inst(oc, new ContentsReg { basereg = ptr }, newval, n, is_tls));

            if (dest is ContentsReg)
            {
                if (oc2 != 0)
                    r.Add(inst(oc2, r_eax, r_eax, n));
                handle_move(dest, r_eax, r, n, c);
            }
            else if (oc2 != 0)
                r.Add(inst(oc2, dest, r_eax, n));
            else
                handle_move(dest, r_eax, r, n, c);            

            return r;
        }

        internal static List<MCInst> handle_target_specific(
                        Target t,
            List<CilNode.IRNode> nodes,
            int start, int count, Code c)
        {
            var n = nodes[start];

            switch(n.imm_l)
            {
                case x86_roundsd_xmm_xmmm64_imm8:
                    return handle_roundsd(t, nodes, start, count, c);
                case x86_enter_cli:
                    return handle_enter_cli(t, nodes, start, count, c);
                case x86_exit_cli:
                    return handle_exit_cli(t, nodes, start, count, c);
                default:
                    throw new NotImplementedException("Invalid target specific operation");
            }
        }

        internal static List<MCInst> handle_enter_cli(
                Target t,
                List<CilNode.IRNode> nodes,
                int start, int count, Code c)
        {
            var n = nodes[start];

            var dest = n.stack_after.Peek(n.res_a).reg;

            var r = new List<MCInst>();
            r.Add(inst(x86_pushf, n));
            if (dest is ContentsReg)
                r.Add(inst(x86_pop_rm32, dest, n));
            else
                r.Add(inst(x86_pop_r32, dest, n));
            r.Add(inst(x86_cli, n));

            return r;
        }

        internal static List<MCInst> handle_exit_cli(
                Target t,
                List<CilNode.IRNode> nodes,
                int start, int count, Code c)
        {
            var n = nodes[start];

            var src = n.stack_before.Peek(n.arg_a).reg;

            var r = new List<MCInst>();
            if (src is ContentsReg)
                r.Add(inst(x86_push_rm32, src, n));
            else
                r.Add(inst(x86_push_r32, src, n));
            if (t.psize == 4)
                r.Add(inst(x86_popf, n));
            else
                r.Add(inst(x86_popfq, n));

            return r;
        }

        internal static List<MCInst> handle_roundsd(
                        Target t,
            List<CilNode.IRNode> nodes,
            int start, int count, Code c)
        {
            var n = nodes[start];

            var src = n.stack_before[n.arg_a].reg;
            var dest = n.stack_after[n.res_a].reg;
            var act_dest = dest;

            var r = new List<MCInst>();

            if ((bool)t.Options["sse4_1"] == true)
            {
                if (dest is ContentsReg)
                    act_dest = r_xmm7;

                r.Add(inst(x86_roundsd_xmm_xmmm64_imm8, act_dest, src, 0, n));

                handle_move(dest, act_dest, r, n, c);
            }
            else
            {
                r.AddRange(handle_call(n, c,
                    c.special_meths.GetMethodSpec(c.special_meths.rint),
                    new ir.Param[] { src }, dest, "rint"));
            }

            return r;
        }

        internal static List<MCInst> handle_spinlockhint(
            Target t,
            List<CilNode.IRNode> nodes,
            int start, int count, Code c)
        {
            var n = nodes[start];
            var r = new List<MCInst>();
            r.Add(inst(x86_pause, n));
            return r;
        }

        internal static List<MCInst> handle_ldarg(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct = n.ctret;
            var src = c.la_locs[(int)n.imm_l];
            var dest = n.stack_after.Peek(n.res_a).reg;

            var r = new List<MCInst>();
            if (n_ct == ir.Opcode.ct_int32 ||
                n_ct == ir.Opcode.ct_float ||
                n_ct == ir.Opcode.ct_vt ||
                n_ct == ir.Opcode.ct_int64)
            {
                handle_move(dest, src, r, n, c, null,
                    n_ct == ir.Opcode.ct_int32 ? 4 : -1);
                return r;
            }
            return null;
        }

        internal static List<MCInst> handle_stackcopy(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var src = n.stack_before.Peek(n.arg_a).reg;
            var dest = n.stack_after.Peek(n.res_a).reg;

            List<MCInst> r = new List<MCInst>();
            handle_move(dest, src, r, n, c);
            return r;
        }

        internal static List<MCInst> handle_localloc(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var size = n.stack_before.Peek(n.arg_a).reg;
            var addr = n.stack_after.Peek(n.res_a).reg;

            var act_dest = addr;
            if (act_dest is ContentsReg)
                act_dest = r_edx;

            if (n.parent.is_in_excpt_handler)
                throw new Exception("localloc not allowed in exception handlers");

            List<MCInst> r = new List<MCInst>();
            handle_sub(t, r_esp, size, r_esp, r, n);
            r.Add(inst(t.psize == 4 ? x86_and_rm32_imm8 :
                x86_and_rm64_imm8, r_esp, t.psize == 4 ? 0xfc : 0xf8, n));
            r.Add(inst(t.psize == 4 ? x86_lea_r32 : x86_lea_r64, act_dest,
                new ContentsReg { basereg = r_esp }, n));
            handle_move(addr, act_dest, r, n, c);
            return r;
        }

        internal static List<MCInst> handle_ldobja(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var src = n.stack_before.Peek(n.arg_a).reg;
            var addr = n.stack_after.Peek(n.res_a).reg;

            var act_addr = addr;
            if (addr is ContentsReg)
                addr = r_edx;

            List<MCInst> r = new List<MCInst>();
            if (!(src is ContentsReg))
                throw new NotImplementedException();

            r.Add(inst(t.psize == 4 ? x86_lea_r32 : x86_lea_r64, addr, src, n));
            handle_move(act_addr, addr, r, n, c);

            return r;
        }

        internal static List<MCInst> handle_shift(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct = n.ct;
            var srca = n.stack_before.Peek(n.arg_a).reg;
            var srcb = n.stack_before.Peek(n.arg_b).reg;
            var dest = n.stack_after.Peek(n.res_a).reg;

            List<MCInst> r = new List<MCInst>();

            if (n_ct == ir.Opcode.ct_int32 ||
                (n_ct == ir.Opcode.ct_int64 && t.psize == 8))
            {
                handle_shift(srca, srcb, dest, r, n, c,
                    n_ct == ir.Opcode.ct_int32 ? 4 : 8);

                return r;
            }
            else if (n_ct == ir.Opcode.ct_int64)
            {
                var sb = n.stack_before.Peek(n.arg_b);

                if (sb.min_l == sb.max_l &&
                    sb.min_l == 1)
                {
                    // handle as a shift through cf using two
                    //  calls to handle_shift_i32 - NB need
                    //  to change handle_shift_i32 to handle
                    //  use_cf = true
                    throw new NotImplementedException();
                }
                else
                {
                    // handle as a function call
                    string fname = null;
                    switch(n.opcode)
                    {
                        case ir.Opcode.oc_shl:
                            fname = "__ashldi3";
                            break;
                        case ir.Opcode.oc_shr:
                            fname = "__ashrdi3";
                            break;
                        case ir.Opcode.oc_shr_un:
                            fname = "__lshrdi3";
                            break;
                    }
                    return t.handle_external(t, nodes, start,
                        count, c, fname);
                }
            }
            return null;
        }

        private static void handle_shift(Reg srca, Reg srcb,
            Reg dest, List<MCInst> r, CilNode.IRNode n, Code c,
            int size,
            bool use_cf = false)
        {
            bool cl_in_use_before = false;
            bool cl_in_use_after = false;

            var orig_srca = srca;

            if (!srca.Equals(dest) || srca.Equals(r_ecx))
            {
                // if either of the above is true, we need to move
                //  the source to eax
                if (size == 4)
                {
                    handle_move(r_eax, srca, r, n, c);
                    srca = r_eax;
                }
                else
                {
                    handle_move(x86_64.x86_64_Assembler.r_rax, srca, r, n, c);
                    srca = x86_64.x86_64_Assembler.r_rax;
                }
            }

            if (!srcb.Equals(r_ecx))
            {
                // need to assign the number to move to cl
                // first determine if it is in use

                ulong defined = 0;
                foreach (var si in n.stack_before)
                    defined |= si.reg.mask;
                if ((defined & r_ecx.mask) != 0)
                    cl_in_use_before = true;
                if (orig_srca.Equals(r_ecx))
                    cl_in_use_before = false; // we have moved srca out of cl
                defined = 0;
                foreach (var si in n.stack_after)
                    defined |= si.reg.mask;
                if ((defined & r_ecx.mask) != 0)
                    cl_in_use_after = true;
                if (dest.Equals(r_ecx))
                    cl_in_use_after = false; // we don't need to save rcx here as we are assigning to it
            }

            bool cl_pushed = false;

            if (cl_in_use_before && cl_in_use_after)
            {
                r.Add(inst(x86_push_r32, r_ecx, n));
                cl_pushed = true;
            }

            if(!srcb.Equals(r_ecx))
            {
                handle_move(r_ecx, srcb, r, n, c);
                srcb = r_ecx;
            }

            int oc = 0;
            if (size == 4)
            {
                switch (n.opcode)
                {
                    case ir.Opcode.oc_shl:
                        oc = x86_sal_rm32_cl;
                        break;
                    case ir.Opcode.oc_shr:
                        oc = x86_sar_rm32_cl;
                        break;
                    case ir.Opcode.oc_shr_un:
                        oc = x86_shr_rm32_cl;
                        break;
                }
            }
            else
            {
                switch (n.opcode)
                {
                    case ir.Opcode.oc_shl:
                        oc = x86_sal_rm64_cl;
                        break;
                    case ir.Opcode.oc_shr:
                        oc = x86_sar_rm64_cl;
                        break;
                    case ir.Opcode.oc_shr_un:
                        oc = x86_shr_rm64_cl;
                        break;
                }
            }

            r.Add(inst(oc, srca, n));

            if (!srca.Equals(dest))
                handle_move(dest, srca, r, n, c);

            if (cl_pushed)
                r.Add(inst(x86_pop_r32, r_ecx, n));
        }

        internal static List<MCInst> handle_switch(
            Target t,
            List<CilNode.IRNode> nodes,
            int start, int count, Code c)
        {
            var n = nodes[start];

            var src = n.stack_before.Peek(n.arg_a).reg;
            List<MCInst> r = new List<MCInst>();

            /* Implement as a series of brifs for now */
            for(int i = 0; i < n.arg_list.Count; i++)
                handle_brifi32(src, i, ir.Opcode.cc_eq, n.arg_list[i], r, n);

            return r;
        }

        internal static List<MCInst> handle_getCharSeq(
            Target t,
            List<CilNode.IRNode> nodes,
            int start, int count, Code c)
        {
            var n = nodes[start];
            var ldc_scale = nodes[start];
            var conv = nodes[start + 2];
            var ldc_offset = nodes[start + 4];
            var ldind = nodes[start + 6];

            var offset = ldc_offset.imm_l;
            var scale = ldc_scale.imm_l;

            // only handle valid scales
            switch(scale)
            {
                case 1:
                case 2:
                case 4:
                case 8:
                    break;
                default:
                    return null;
            }

            // ensure the types are valid
            if (conv.ctret != ir.Opcode.ct_int32)
                return null;
            
            // only handle valid data sizes
            switch(ldind.vt_size)
            {
                case 1:
                case 2:
                case 4:
                    break;
                default:
                    return null;
            }

            var obj = n.stack_before.Peek(1).reg;
            var idx = n.stack_before.Peek(0).reg;

            var dest = ldind.stack_after.Peek().reg;

            // emit as mov[sz]x[bw]d dest, [obj + offset + idx * scale]
            List<MCInst> r = new List<MCInst>();
            if(obj is ContentsReg)
            {
                handle_move(r_edx, obj, r, n, c);
                obj = r_edx;
            }
            if(idx is ContentsReg)
            {
                handle_move(r_eax, idx, r, n, c);
                idx = r_eax;
            }

            var act_dest = dest;
            if (dest is ContentsReg)
                dest = r_edx;

            switch(ldind.vt_size)
            {
                case 1:
                    if (ldind.imm_l == 1)
                        r.Add(inst(x86_movsxbd_r32_rm8sibscaledisp, dest, obj, idx, scale, offset, n));
                    else
                        r.Add(inst(x86_movzxbd_r32_rm8sibscaledisp, dest, obj, idx, scale, offset, n));
                    break;
                case 2:
                    if (ldind.imm_l == 1)
                        r.Add(inst(x86_movsxwd_r32_rm16sibscaledisp, dest, obj, idx, scale, offset, n));
                    else
                        r.Add(inst(x86_movzxwd_r32_rm16sibscaledisp, dest, obj, idx, scale, offset, n));
                    break;
                case 4:
                    r.Add(inst(x86_mov_r32_rm32sibscaledisp, dest, obj, idx, scale, offset, n));
                    break;
            }

            handle_move(act_dest, dest, r, n, c);

            return r;
        }

        internal static List<MCInst> handle_portin(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];

            var port = n.stack_before.Peek(n.arg_a).reg;
            var v = n.stack_after.Peek(n.res_a).reg;

            List<MCInst> r = new List<MCInst>();
            handle_move(r_edx, port, r, n, c);

            int oc = 0;
            int oc_movzx = 0;
            switch (n.vt_size)
            {
                case 1:
                    oc = x86_in_al_dx;
                    oc_movzx = x86_movzxbd;
                    break;
                case 2:
                    oc = x86_in_ax_dx;
                    oc_movzx = x86_movzxwd;
                    break;
                case 4:
                    oc = x86_in_eax_dx;
                    break;
                default:
                    throw new NotImplementedException();
            }

            r.Add(inst(oc, n));

            if (oc_movzx != 0)
            {
                if (v is ContentsReg)
                {
                    r.Add(inst(oc_movzx, r_eax, r_eax, n));
                    handle_move(v, r_eax, r, n, c);
                }
                else
                    r.Add(inst(oc_movzx, v, r_eax, n));
            }
            else
                handle_move(v, r_eax, r, n, c);

            return r;
        }

        internal static List<MCInst> handle_portout(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];

            var port = n.stack_before.Peek(n.arg_a).reg;
            var v = n.stack_before.Peek(n.arg_b).reg;

            List<MCInst> r = new List<MCInst>();
            handle_move(r_eax, v, r, n, c);
            handle_move(r_edx, port, r, n, c, r_edx);

            int oc = 0;
            switch(n.vt_size)
            {
                case 1:
                    oc = x86_out_dx_al;
                    break;
                case 2:
                    oc = x86_out_dx_ax;
                    break;
                case 4:
                    oc = x86_out_dx_eax;
                    break;
                default:
                    throw new NotImplementedException();
            }

            r.Add(inst(oc, n));

            return r;
        }
    }
}

namespace libtysila5.target.x86_64
{
    public partial class x86_64_Assembler
    {
        internal static List<MCInst> handle_ldc(
           Target t,
           List<CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];
            var n_ct = n.ctret;

            if(n_ct == ir.Opcode.ct_int64)
            {
                var dest = n.stack_after.Peek(n.res_a).reg;

                var s = n.imm_l;

                var r = new List<MCInst>();
                if(s <= Int32.MaxValue && s >= Int32.MinValue)
                {
                    r.Add(inst(x86_mov_rm64_imm32, dest, s, n));
                }
                else
                {
                    var act_dest = dest;
                    if (dest is ContentsReg)
                        dest = r_rax;
                    r.Add(inst(x86_mov_r64_imm64, dest, s, n));
                    handle_move(act_dest, dest, r, n, c);
                }
                return r;
            }

            return x86.x86_Assembler.handle_ldc(t, nodes, start, count, c);
        }
    }
}
