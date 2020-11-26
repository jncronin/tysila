/* D:\tysila\libtysila5\target\Target.cs
 * This is an auto-generated file
 * DO NOT EDIT
 * It was generated at 17:21:03 on 26 November 2020
 * from libtysila5/target/Target.td
 * by TableMap (part of tysos: http://www.tysos.org)
 * Please edit the source file, rather than this file, to make any changes
 */

namespace libtysila5.target
{
	partial class Target
	{
		public const int rt_gpr = 0;
		public const int rt_float = 1;
		public const int rt_stack = 2;
		public const int rt_contents = 3;
		public const int rt_multi = 4;
		
		internal static void init_rtmap()
		{
			rt_map[0] = "gpr";
			rt_map[1] = "float";
			rt_map[2] = "stack";
			rt_map[3] = "contents";
			rt_map[4] = "multi";
		}
	}
}

namespace libtysila5.target
{
	partial class Target
	{
		public const int pt_def = 5;
		public const int pt_use = 6;
		public const int pt_cc = 7;
		public const int pt_icc = 8;
		public const int pt_br = 9;
		public const int pt_mc = 10;
		public const int pt_tu = 11;
		public const int pt_td = 12;
		
		internal static void init_pt()
		{
			pt_names[5] = "def";
			pt_names[6] = "use";
			pt_names[7] = "cc";
			pt_names[8] = "icc";
			pt_names[9] = "br";
			pt_names[10] = "mc";
			pt_names[11] = "tu";
			pt_names[12] = "td";
		}
	}
}

namespace libtysila5.target
{
	partial class Generic
	{
		public const int g_phi = 13;
		public const int g_precall = 14;
		public const int g_postcall = 15;
		public const int g_setupstack = 16;
		public const int g_savecalleepreserves = 17;
		public const int g_restorecalleepreserves = 18;
		public const int g_loadaddress = 19;
		public const int g_mclabel = 20;
		public const int g_label = 21;
		
		internal static void init_instrs()
		{
			insts[13] = "phi";
			insts[14] = "precall";
			insts[15] = "postcall";
			insts[16] = "setupstack";
			insts[17] = "savecalleepreserves";
			insts[18] = "restorecalleepreserves";
			insts[19] = "loadaddress";
			insts[20] = "mclabel";
			insts[21] = "label";
		}
	}
}

namespace libtysila5.target.x86
{
	partial class x86_Assembler
	{
		public const int x86_mov_rm32_r32 = 22;
		public const int x86_mov_r32_rm32 = 23;
		public const int x86_mov_r8_rm8 = 24;
		public const int x86_mov_r16_rm16 = 25;
		public const int x86_mov_rm32_imm32 = 26;
		public const int x86_mov_r32_lab = 27;
		public const int x86_mov_lab_r32 = 28;
		public const int x86_mov_rm32_lab = 29;
		public const int x86_mov_r32_rm32sib = 30;
		public const int x86_mov_r32_rm32disp = 31;
		public const int x86_mov_r32_rm16disp = 32;
		public const int x86_mov_r32_rm8disp = 33;
		public const int x86_mov_r32_rm32sibscaledisp = 34;
		public const int x86_mov_rm32disp_imm32 = 35;
		public const int x86_mov_rm16disp_imm32 = 36;
		public const int x86_mov_rm8disp_imm32 = 37;
		public const int x86_mov_rm32disp_r32 = 38;
		public const int x86_mov_rm16disp_r16 = 39;
		public const int x86_mov_rm8disp_r8 = 40;
		public const int x86_movzxbd_r32_rm8sibscaledisp = 41;
		public const int x86_movzxwd_r32_rm16sibscaledisp = 42;
		public const int x86_movsxbd_r32_rm8sibscaledisp = 43;
		public const int x86_movsxwd_r32_rm16sibscaledisp = 44;
		public const int x86_neg_rm32 = 45;
		public const int x86_not_rm32 = 46;
		public const int x86_cmp_rm32_r32 = 47;
		public const int x86_cmp_r32_rm32 = 48;
		public const int x86_cmp_rm32_imm32 = 49;
		public const int x86_cmp_rm32_imm8 = 50;
		public const int x86_cmp_rm8_imm8 = 51;
		public const int x86_lock_cmpxchg_rm8_r8 = 52;
		public const int x86_lock_cmpxchg_rm32_r32 = 53;
		public const int x86_pause = 54;
		public const int x86_set_rm32 = 55;
		public const int x86_movsxbd = 56;
		public const int x86_movsxwd = 57;
		public const int x86_movzxbd = 58;
		public const int x86_movzxwd = 59;
		public const int x86_movsxbd_r32_rm8disp = 60;
		public const int x86_movzxbd_r32_rm8disp = 61;
		public const int x86_movsxwd_r32_rm16disp = 62;
		public const int x86_movzxwd_r32_rm16disp = 63;
		public const int x86_jmp_rel32 = 64;
		public const int x86_jcc_rel32 = 65;
		public const int x86_call_rel32 = 66;
		public const int x86_call_rm32 = 67;
		public const int x86_ret = 68;
		public const int x86_pop_r32 = 69;
		public const int x86_pop_rm32 = 70;
		public const int x86_push_r32 = 71;
		public const int x86_push_rm32 = 72;
		public const int x86_push_imm32 = 73;
		public const int x86_add_rm32_imm32 = 74;
		public const int x86_add_rm32_imm8 = 75;
		public const int x86_sub_rm32_imm32 = 76;
		public const int x86_sub_rm32_imm8 = 77;
		public const int x86_add_r32_rm32 = 78;
		public const int x86_add_rm32_r32 = 79;
		public const int x86_sub_r32_rm32 = 80;
		public const int x86_sub_rm32_r32 = 81;
		public const int x86_adc_r32_rm32 = 82;
		public const int x86_adc_rm32_r32 = 83;
		public const int x86_sbb_r32_rm32 = 84;
		public const int x86_sbb_rm32_r32 = 85;
		public const int x86_idiv_rm32 = 86;
		public const int x86_imul_r32_rm32_imm32 = 87;
		public const int x86_imul_r32_rm32 = 88;
		public const int x86_lea_r32 = 89;
		public const int x86_xor_r32_rm32 = 90;
		public const int x86_xor_rm32_r32 = 91;
		public const int x86_and_r32_rm32 = 92;
		public const int x86_and_rm32_r32 = 93;
		public const int x86_or_r32_rm32 = 94;
		public const int x86_or_rm32_r32 = 95;
		public const int x86_sar_rm32_imm8 = 96;
		public const int x86_sal_rm32_cl = 97;
		public const int x86_sar_rm32_cl = 98;
		public const int x86_shr_rm32_cl = 99;
		public const int x86_and_rm32_imm8 = 100;
		public const int x86_and_rm32_imm32 = 101;
		public const int x86_xchg_r32_rm32 = 102;
		public const int x86_xchg_rm32_r32 = 103;
		public const int x86_lock_xchg_rm8ptr_r8 = 104;
		public const int x86_lock_xchg_rm32ptr_r32 = 105;
		public const int x86_out_dx_al = 106;
		public const int x86_out_dx_ax = 107;
		public const int x86_out_dx_eax = 108;
		public const int x86_in_al_dx = 109;
		public const int x86_in_ax_dx = 110;
		public const int x86_in_eax_dx = 111;
		public const int x86_iret = 112;
		public const int x86_int3 = 113;
		public const int x86_pushf = 114;
		public const int x86_popf = 115;
		public const int x86_cli = 116;
		public const int x86_nop = 117;
		public const int x86_fstp_m64 = 118;
		public const int x86_fld_m64 = 119;
		public const int x86_movsd_xmm_xmmm64 = 120;
		public const int x86_movsd_xmm_xmmm64disp = 121;
		public const int x86_movsd_xmmm64_xmm = 122;
		public const int x86_movsd_xmmm64disp_xmm = 123;
		public const int x86_movss_xmm_xmmm32 = 124;
		public const int x86_movss_xmmm32_xmm = 125;
		public const int x86_movss_xmmm32disp_xmm = 126;
		public const int x86_cvtsd2si_r32_xmmm64 = 127;
		public const int x86_cvtsi2sd_xmm_rm32 = 128;
		public const int x86_cvtsd2ss_xmm_xmmm64 = 129;
		public const int x86_cvtss2sd_xmm_xmmm32 = 130;
		public const int x86_cvtss2sd_xmm_xmmm32disp = 131;
		public const int x86_addsd_xmm_xmmm64 = 132;
		public const int x86_subsd_xmm_xmmm64 = 133;
		public const int x86_mulsd_xmm_xmmm64 = 134;
		public const int x86_divsd_xmm_xmmm64 = 135;
		public const int x86_comisd_xmm_xmmm64 = 136;
		public const int x86_ucomisd_xmm_xmmm64 = 137;
		public const int x86_cmpsd_xmm_xmmm64_imm8 = 138;
		public const int x86_roundsd_xmm_xmmm64_imm8 = 139;
		public const int x86_xorpd_xmm_xmmm128 = 140;
		public const int x86_enter_cli = 141;
		public const int x86_exit_cli = 142;
		public const int x86_mov_r64_imm64 = 143;
		public const int x86_mov_rm64_imm32 = 144;
		public const int x86_mov_r64_rm64 = 145;
		public const int x86_mov_rm64_r64 = 146;
		public const int x86_mov_rm64disp_imm32 = 147;
		public const int x86_mov_r64_rm64disp = 148;
		public const int x86_mov_r64_rm32disp = 149;
		public const int x86_mov_r64_rm16disp = 150;
		public const int x86_mov_r64_rm8disp = 151;
		public const int x86_mov_rm64disp_r64 = 152;
		public const int x86_movzxbq = 153;
		public const int x86_movsxbq_r64_rm8disp = 154;
		public const int x86_movzxbq_r64_rm8disp = 155;
		public const int x86_movsxwq_r64_rm16disp = 156;
		public const int x86_movzxwq_r64_rm16disp = 157;
		public const int x86_cmp_rm64_r64 = 158;
		public const int x86_cmp_r64_rm64 = 159;
		public const int x86_cmp_rm64_imm32 = 160;
		public const int x86_cmp_rm64_imm8 = 161;
		public const int x86_movsxdq_r64_rm64 = 162;
		public const int x86_xor_r64_rm64 = 163;
		public const int x86_xor_rm64_r64 = 164;
		public const int x86_and_r64_rm64 = 165;
		public const int x86_and_rm64_r64 = 166;
		public const int x86_or_r64_rm64 = 167;
		public const int x86_or_rm64_r64 = 168;
		public const int x86_neg_rm64 = 169;
		public const int x86_not_rm64 = 170;
		public const int x86_imul_r64_rm64 = 171;
		public const int x86_idiv_rm64 = 172;
		public const int x86_sal_rm64_cl = 173;
		public const int x86_sar_rm64_cl = 174;
		public const int x86_shr_rm64_cl = 175;
		public const int x86_xchg_r64_rm64 = 176;
		public const int x86_lock_xchg_rm64ptr_r64 = 177;
		public const int x86_sub_rm64_imm8 = 178;
		public const int x86_sub_rm64_imm32 = 179;
		public const int x86_add_rm64_imm8 = 180;
		public const int x86_add_rm64_imm32 = 181;
		public const int x86_lea_r64 = 182;
		public const int x86_add_r64_rm64 = 183;
		public const int x86_add_rm64_r64 = 184;
		public const int x86_sub_r64_rm64 = 185;
		public const int x86_sub_rm64_r64 = 186;
		public const int x86_adc_r64_rm64 = 187;
		public const int x86_adc_rm64_r64 = 188;
		public const int x86_sbb_r64_rm64 = 189;
		public const int x86_sbb_rm64_r64 = 190;
		public const int x86_cvtsi2sd_xmm_rm64 = 191;
		public const int x86_cvtsd2si_r64_xmmm64 = 192;
		public const int x86_and_rm64_imm8 = 193;
		public const int x86_movzxbq_r64_rm8sibscaledisp = 194;
		public const int x86_movzxwq_r64_rm16sibscaledisp = 195;
		public const int x86_movsxbq_r64_rm8sibscaledisp = 196;
		public const int x86_movsxwq_r64_rm16sibscaledisp = 197;
		public const int x86_movsxdq_r64_rm32sibscaledisp = 198;
		public const int x86_mov_r64_rm64sibscaledisp = 199;
		public const int x86_iretq = 200;
		public const int x86_lock_cmpxchg_rm64_r64 = 201;
		public const int x86_popfq = 202;
		
		internal static void init_instrs()
		{
			insts[22] = "mov_rm32_r32";
			insts[23] = "mov_r32_rm32";
			insts[24] = "mov_r8_rm8";
			insts[25] = "mov_r16_rm16";
			insts[26] = "mov_rm32_imm32";
			insts[27] = "mov_r32_lab";
			insts[28] = "mov_lab_r32";
			insts[29] = "mov_rm32_lab";
			insts[30] = "mov_r32_rm32sib";
			insts[31] = "mov_r32_rm32disp";
			insts[32] = "mov_r32_rm16disp";
			insts[33] = "mov_r32_rm8disp";
			insts[34] = "mov_r32_rm32sibscaledisp";
			insts[35] = "mov_rm32disp_imm32";
			insts[36] = "mov_rm16disp_imm32";
			insts[37] = "mov_rm8disp_imm32";
			insts[38] = "mov_rm32disp_r32";
			insts[39] = "mov_rm16disp_r16";
			insts[40] = "mov_rm8disp_r8";
			insts[41] = "movzxbd_r32_rm8sibscaledisp";
			insts[42] = "movzxwd_r32_rm16sibscaledisp";
			insts[43] = "movsxbd_r32_rm8sibscaledisp";
			insts[44] = "movsxwd_r32_rm16sibscaledisp";
			insts[45] = "neg_rm32";
			insts[46] = "not_rm32";
			insts[47] = "cmp_rm32_r32";
			insts[48] = "cmp_r32_rm32";
			insts[49] = "cmp_rm32_imm32";
			insts[50] = "cmp_rm32_imm8";
			insts[51] = "cmp_rm8_imm8";
			insts[52] = "lock_cmpxchg_rm8_r8";
			insts[53] = "lock_cmpxchg_rm32_r32";
			insts[54] = "pause";
			insts[55] = "set_rm32";
			insts[56] = "movsxbd";
			insts[57] = "movsxwd";
			insts[58] = "movzxbd";
			insts[59] = "movzxwd";
			insts[60] = "movsxbd_r32_rm8disp";
			insts[61] = "movzxbd_r32_rm8disp";
			insts[62] = "movsxwd_r32_rm16disp";
			insts[63] = "movzxwd_r32_rm16disp";
			insts[64] = "jmp_rel32";
			insts[65] = "jcc_rel32";
			insts[66] = "call_rel32";
			insts[67] = "call_rm32";
			insts[68] = "ret";
			insts[69] = "pop_r32";
			insts[70] = "pop_rm32";
			insts[71] = "push_r32";
			insts[72] = "push_rm32";
			insts[73] = "push_imm32";
			insts[74] = "add_rm32_imm32";
			insts[75] = "add_rm32_imm8";
			insts[76] = "sub_rm32_imm32";
			insts[77] = "sub_rm32_imm8";
			insts[78] = "add_r32_rm32";
			insts[79] = "add_rm32_r32";
			insts[80] = "sub_r32_rm32";
			insts[81] = "sub_rm32_r32";
			insts[82] = "adc_r32_rm32";
			insts[83] = "adc_rm32_r32";
			insts[84] = "sbb_r32_rm32";
			insts[85] = "sbb_rm32_r32";
			insts[86] = "idiv_rm32";
			insts[87] = "imul_r32_rm32_imm32";
			insts[88] = "imul_r32_rm32";
			insts[89] = "lea_r32";
			insts[90] = "xor_r32_rm32";
			insts[91] = "xor_rm32_r32";
			insts[92] = "and_r32_rm32";
			insts[93] = "and_rm32_r32";
			insts[94] = "or_r32_rm32";
			insts[95] = "or_rm32_r32";
			insts[96] = "sar_rm32_imm8";
			insts[97] = "sal_rm32_cl";
			insts[98] = "sar_rm32_cl";
			insts[99] = "shr_rm32_cl";
			insts[100] = "and_rm32_imm8";
			insts[101] = "and_rm32_imm32";
			insts[102] = "xchg_r32_rm32";
			insts[103] = "xchg_rm32_r32";
			insts[104] = "lock_xchg_rm8ptr_r8";
			insts[105] = "lock_xchg_rm32ptr_r32";
			insts[106] = "out_dx_al";
			insts[107] = "out_dx_ax";
			insts[108] = "out_dx_eax";
			insts[109] = "in_al_dx";
			insts[110] = "in_ax_dx";
			insts[111] = "in_eax_dx";
			insts[112] = "iret";
			insts[113] = "int3";
			insts[114] = "pushf";
			insts[115] = "popf";
			insts[116] = "cli";
			insts[117] = "nop";
			insts[118] = "fstp_m64";
			insts[119] = "fld_m64";
			insts[120] = "movsd_xmm_xmmm64";
			insts[121] = "movsd_xmm_xmmm64disp";
			insts[122] = "movsd_xmmm64_xmm";
			insts[123] = "movsd_xmmm64disp_xmm";
			insts[124] = "movss_xmm_xmmm32";
			insts[125] = "movss_xmmm32_xmm";
			insts[126] = "movss_xmmm32disp_xmm";
			insts[127] = "cvtsd2si_r32_xmmm64";
			insts[128] = "cvtsi2sd_xmm_rm32";
			insts[129] = "cvtsd2ss_xmm_xmmm64";
			insts[130] = "cvtss2sd_xmm_xmmm32";
			insts[131] = "cvtss2sd_xmm_xmmm32disp";
			insts[132] = "addsd_xmm_xmmm64";
			insts[133] = "subsd_xmm_xmmm64";
			insts[134] = "mulsd_xmm_xmmm64";
			insts[135] = "divsd_xmm_xmmm64";
			insts[136] = "comisd_xmm_xmmm64";
			insts[137] = "ucomisd_xmm_xmmm64";
			insts[138] = "cmpsd_xmm_xmmm64_imm8";
			insts[139] = "roundsd_xmm_xmmm64_imm8";
			insts[140] = "xorpd_xmm_xmmm128";
			insts[141] = "enter_cli";
			insts[142] = "exit_cli";
			insts[143] = "mov_r64_imm64";
			insts[144] = "mov_rm64_imm32";
			insts[145] = "mov_r64_rm64";
			insts[146] = "mov_rm64_r64";
			insts[147] = "mov_rm64disp_imm32";
			insts[148] = "mov_r64_rm64disp";
			insts[149] = "mov_r64_rm32disp";
			insts[150] = "mov_r64_rm16disp";
			insts[151] = "mov_r64_rm8disp";
			insts[152] = "mov_rm64disp_r64";
			insts[153] = "movzxbq";
			insts[154] = "movsxbq_r64_rm8disp";
			insts[155] = "movzxbq_r64_rm8disp";
			insts[156] = "movsxwq_r64_rm16disp";
			insts[157] = "movzxwq_r64_rm16disp";
			insts[158] = "cmp_rm64_r64";
			insts[159] = "cmp_r64_rm64";
			insts[160] = "cmp_rm64_imm32";
			insts[161] = "cmp_rm64_imm8";
			insts[162] = "movsxdq_r64_rm64";
			insts[163] = "xor_r64_rm64";
			insts[164] = "xor_rm64_r64";
			insts[165] = "and_r64_rm64";
			insts[166] = "and_rm64_r64";
			insts[167] = "or_r64_rm64";
			insts[168] = "or_rm64_r64";
			insts[169] = "neg_rm64";
			insts[170] = "not_rm64";
			insts[171] = "imul_r64_rm64";
			insts[172] = "idiv_rm64";
			insts[173] = "sal_rm64_cl";
			insts[174] = "sar_rm64_cl";
			insts[175] = "shr_rm64_cl";
			insts[176] = "xchg_r64_rm64";
			insts[177] = "lock_xchg_rm64ptr_r64";
			insts[178] = "sub_rm64_imm8";
			insts[179] = "sub_rm64_imm32";
			insts[180] = "add_rm64_imm8";
			insts[181] = "add_rm64_imm32";
			insts[182] = "lea_r64";
			insts[183] = "add_r64_rm64";
			insts[184] = "add_rm64_r64";
			insts[185] = "sub_r64_rm64";
			insts[186] = "sub_rm64_r64";
			insts[187] = "adc_r64_rm64";
			insts[188] = "adc_rm64_r64";
			insts[189] = "sbb_r64_rm64";
			insts[190] = "sbb_rm64_r64";
			insts[191] = "cvtsi2sd_xmm_rm64";
			insts[192] = "cvtsd2si_r64_xmmm64";
			insts[193] = "and_rm64_imm8";
			insts[194] = "movzxbq_r64_rm8sibscaledisp";
			insts[195] = "movzxwq_r64_rm16sibscaledisp";
			insts[196] = "movsxbq_r64_rm8sibscaledisp";
			insts[197] = "movsxwq_r64_rm16sibscaledisp";
			insts[198] = "movsxdq_r64_rm32sibscaledisp";
			insts[199] = "mov_r64_rm64sibscaledisp";
			insts[200] = "iretq";
			insts[201] = "lock_cmpxchg_rm64_r64";
			insts[202] = "popfq";
		}
	}
}

namespace libtysila5.target.x86
{
	partial class x86_Assembler
	{
		public static System.Collections.Generic.Dictionary<int, int[]> cc_map_sysv = new System.Collections.Generic.Dictionary<int, int[]>(new libtysila5.GenericEqualityComparer<int>());
		public static System.Collections.Generic.Dictionary<int, int> cc_classmap_sysv = new System.Collections.Generic.Dictionary<int, int>(new libtysila5.GenericEqualityComparer<int>());
		internal static void init_sysv()
		{
			cc_map_sysv[96] = new int[] { 4, };
			cc_map_sysv[98] = new int[] { 4, };
			cc_map_sysv[97] = new int[] { 4, };
			cc_map_sysv[103] = new int[] { 4, };
			cc_map_sysv[104] = new int[] { 4, };
			cc_map_sysv[105] = new int[] { 4, };
			cc_map_sysv[102] = new int[] { 4, };
		}
		
		internal const ulong sysv_caller_preserves = 1664;
		internal const ulong sysv_callee_preserves = 6400;
	}
}

namespace libtysila5.target.x86
{
	partial class x86_Assembler
	{
		public static System.Collections.Generic.Dictionary<int, int[]> cc_map_sysv_xmm = new System.Collections.Generic.Dictionary<int, int[]>(new libtysila5.GenericEqualityComparer<int>());
		public static System.Collections.Generic.Dictionary<int, int> cc_classmap_sysv_xmm = new System.Collections.Generic.Dictionary<int, int>(new libtysila5.GenericEqualityComparer<int>());
		internal static void init_sysv_xmm()
		{
			cc_map_sysv_xmm[96] = new int[] { 4, };
			cc_map_sysv_xmm[98] = new int[] { 4, };
			cc_map_sysv_xmm[97] = new int[] { 4, };
			cc_map_sysv_xmm[103] = new int[] { 4, };
			cc_map_sysv_xmm[104] = new int[] { 4, };
			cc_map_sysv_xmm[105] = new int[] { 4, };
			cc_map_sysv_xmm[102] = new int[] { 4, };
		}
		
		internal const ulong sysv_xmm_caller_preserves = 1664;
		internal const ulong sysv_xmm_callee_preserves = 6400;
	}
}

namespace libtysila5.target.x86
{
	partial class x86_Assembler
	{
		public static System.Collections.Generic.Dictionary<int, int[]> cc_map_ret_sysv = new System.Collections.Generic.Dictionary<int, int[]>(new libtysila5.GenericEqualityComparer<int>());
		public static System.Collections.Generic.Dictionary<int, int> cc_classmap_ret_sysv = new System.Collections.Generic.Dictionary<int, int>(new libtysila5.GenericEqualityComparer<int>());
		internal static void init_ret_sysv()
		{
			cc_map_ret_sysv[96] = new int[] { 7, };
			cc_map_ret_sysv[98] = new int[] { 7, };
			cc_map_ret_sysv[103] = new int[] { 7, };
			cc_map_ret_sysv[104] = new int[] { 7, };
			cc_map_ret_sysv[97] = new int[] { 24, };
			cc_map_ret_sysv[102] = new int[] { 15, };
		}
		
		internal const ulong ret_sysv_caller_preserves = 0;
		internal const ulong ret_sysv_callee_preserves = 0;
	}
}

namespace libtysila5.target.x86
{
	partial class x86_Assembler
	{
		public static System.Collections.Generic.Dictionary<int, int[]> cc_map_ret_sysv_xmm = new System.Collections.Generic.Dictionary<int, int[]>(new libtysila5.GenericEqualityComparer<int>());
		public static System.Collections.Generic.Dictionary<int, int> cc_classmap_ret_sysv_xmm = new System.Collections.Generic.Dictionary<int, int>(new libtysila5.GenericEqualityComparer<int>());
		internal static void init_ret_sysv_xmm()
		{
			cc_map_ret_sysv_xmm[96] = new int[] { 7, };
			cc_map_ret_sysv_xmm[98] = new int[] { 7, };
			cc_map_ret_sysv_xmm[103] = new int[] { 7, };
			cc_map_ret_sysv_xmm[104] = new int[] { 7, };
			cc_map_ret_sysv_xmm[97] = new int[] { 24, };
			cc_map_ret_sysv_xmm[102] = new int[] { 16, };
		}
		
		internal const ulong ret_sysv_xmm_caller_preserves = 0;
		internal const ulong ret_sysv_xmm_callee_preserves = 0;
	}
}

namespace libtysila5.target.x86
{
	partial class x86_Assembler
	{
		internal void init_cc_caller_preserves_map()
		{
			cc_caller_preserves_map["sysv"] = sysv_caller_preserves;
			cc_caller_preserves_map["sysv_xmm"] = sysv_xmm_caller_preserves;
			cc_caller_preserves_map["default"] = sysv_caller_preserves;
		}
	}
}

namespace libtysila5.target.x86
{
	partial class x86_Assembler
	{
		internal void init_cc_callee_preserves_map()
		{
			cc_callee_preserves_map["sysv"] = sysv_callee_preserves;
			cc_callee_preserves_map["sysv_xmm"] = sysv_xmm_callee_preserves;
			cc_callee_preserves_map["default"] = sysv_callee_preserves;
		}
	}
}

namespace libtysila5.target.x86
{
	partial class x86_Assembler
	{
		internal void init_cc_classmap()
		{
			cc_classmap["sysv"] = cc_classmap_sysv;
			cc_classmap["sysv_xmm"] = cc_classmap_sysv_xmm;
			cc_classmap["default"] = cc_classmap_sysv;
		}
	}
}

namespace libtysila5.target.x86
{
	partial class x86_Assembler
	{
		internal void init_cc_map()
		{
			cc_map["sysv"] = cc_map_sysv;
			cc_map["sysv_xmm"] = cc_map_sysv_xmm;
			cc_map["default"] = cc_map_sysv;
		}
	}
}

namespace libtysila5.target.x86
{
	partial class x86_Assembler
	{
		internal void init_retcc_classmap()
		{
			retcc_classmap["ret_sysv"] = cc_classmap_ret_sysv;
			retcc_classmap["ret_sysv_xmm"] = cc_classmap_ret_sysv_xmm;
			retcc_classmap["ret_default"] = cc_classmap_ret_sysv;
		}
	}
}

namespace libtysila5.target.x86
{
	partial class x86_Assembler
	{
		internal void init_retcc_map()
		{
			retcc_map["ret_sysv"] = cc_map_ret_sysv;
			retcc_map["ret_sysv_xmm"] = cc_map_ret_sysv_xmm;
			retcc_map["ret_default"] = cc_map_ret_sysv;
		}
	}
}

namespace libtysila5.target.x86_64
{
	partial class x86_64_Assembler
	{
		public const int sysvc_MEMORY = 203;
		public const int sysvc_INTEGER = 204;
		public const int sysvc_SSE = 205;
		public const int sysvc_SSEUP = 206;
		public const int sysvc_X87 = 207;
		public const int sysvc_X87UP = 208;
		public const int sysvc_COMPLEX_X87 = 209;
	}
}

namespace libtysila5.target.x86_64
{
	partial class x86_64_Assembler
	{
		public static System.Collections.Generic.Dictionary<int, int[]> cc_map_sysv = new System.Collections.Generic.Dictionary<int, int[]>(new libtysila5.GenericEqualityComparer<int>());
		public static System.Collections.Generic.Dictionary<int, int> cc_classmap_sysv = new System.Collections.Generic.Dictionary<int, int>(new libtysila5.GenericEqualityComparer<int>());
		internal static void init_sysv()
		{
			cc_classmap_sysv[96] = 204;
			cc_classmap_sysv[98] = 204;
			cc_classmap_sysv[97] = 204;
			cc_classmap_sysv[103] = 204;
			cc_classmap_sysv[104] = 204;
			cc_classmap_sysv[102] = 205;
			cc_map_sysv[204] = new int[] { 11, 12, 10, 9, 25, 26, 4, };
			cc_map_sysv[205] = new int[] { 16, 17, 18, 19, 20, 21, 22, 23, 4, };
			cc_map_sysv[203] = new int[] { 4, };
		}
		
		internal const ulong sysv_caller_preserves = 2190953356928;
		internal const ulong sysv_callee_preserves = 8053063936;
	}
}

namespace libtysila5.target.x86_64
{
	partial class x86_64_Assembler
	{
		public static System.Collections.Generic.Dictionary<int, int[]> cc_map_ret_sysv = new System.Collections.Generic.Dictionary<int, int[]>(new libtysila5.GenericEqualityComparer<int>());
		public static System.Collections.Generic.Dictionary<int, int> cc_classmap_ret_sysv = new System.Collections.Generic.Dictionary<int, int>(new libtysila5.GenericEqualityComparer<int>());
		internal static void init_ret_sysv()
		{
			cc_map_ret_sysv[96] = new int[] { 7, };
			cc_map_ret_sysv[98] = new int[] { 7, };
			cc_map_ret_sysv[103] = new int[] { 7, };
			cc_map_ret_sysv[104] = new int[] { 7, };
			cc_map_ret_sysv[97] = new int[] { 7, };
			cc_map_ret_sysv[102] = new int[] { 16, };
		}
		
		internal const ulong ret_sysv_caller_preserves = 0;
		internal const ulong ret_sysv_callee_preserves = 0;
	}
}

namespace libtysila5.target.x86_64
{
	partial class x86_64_Assembler
	{
		public static System.Collections.Generic.Dictionary<int, int[]> cc_map_isr = new System.Collections.Generic.Dictionary<int, int[]>(new libtysila5.GenericEqualityComparer<int>());
		public static System.Collections.Generic.Dictionary<int, int> cc_classmap_isr = new System.Collections.Generic.Dictionary<int, int>(new libtysila5.GenericEqualityComparer<int>());
		internal static void init_isr()
		{
			cc_classmap_isr[96] = 204;
			cc_classmap_isr[98] = 204;
			cc_classmap_isr[97] = 204;
			cc_classmap_isr[103] = 204;
			cc_classmap_isr[104] = 204;
			cc_classmap_isr[102] = 205;
		}
		
		internal const ulong isr_caller_preserves = 0;
		internal const ulong isr_callee_preserves = 2199006420864;
	}
}

namespace libtysila5.target.x86_64
{
	partial class x86_64_Assembler
	{
		public static System.Collections.Generic.Dictionary<int, int[]> cc_map_ret_isr = new System.Collections.Generic.Dictionary<int, int[]>(new libtysila5.GenericEqualityComparer<int>());
		public static System.Collections.Generic.Dictionary<int, int> cc_classmap_ret_isr = new System.Collections.Generic.Dictionary<int, int>(new libtysila5.GenericEqualityComparer<int>());
		internal static void init_ret_isr()
		{
		}
		
		internal const ulong ret_isr_caller_preserves = 0;
		internal const ulong ret_isr_callee_preserves = 0;
	}
}

namespace libtysila5.target.x86_64
{
	partial class x86_64_Assembler
	{
		public static System.Collections.Generic.Dictionary<int, int[]> cc_map_isrec = new System.Collections.Generic.Dictionary<int, int[]>(new libtysila5.GenericEqualityComparer<int>());
		public static System.Collections.Generic.Dictionary<int, int> cc_classmap_isrec = new System.Collections.Generic.Dictionary<int, int>(new libtysila5.GenericEqualityComparer<int>());
		internal static void init_isrec()
		{
			cc_classmap_isrec[96] = 204;
			cc_classmap_isrec[98] = 204;
			cc_classmap_isrec[97] = 204;
			cc_classmap_isrec[103] = 204;
			cc_classmap_isrec[104] = 204;
			cc_classmap_isrec[102] = 205;
		}
		
		internal const ulong isrec_caller_preserves = 0;
		internal const ulong isrec_callee_preserves = 2199006420864;
	}
}

namespace libtysila5.target.x86_64
{
	partial class x86_64_Assembler
	{
		public static System.Collections.Generic.Dictionary<int, int[]> cc_map_ret_isrec = new System.Collections.Generic.Dictionary<int, int[]>(new libtysila5.GenericEqualityComparer<int>());
		public static System.Collections.Generic.Dictionary<int, int> cc_classmap_ret_isrec = new System.Collections.Generic.Dictionary<int, int>(new libtysila5.GenericEqualityComparer<int>());
		internal static void init_ret_isrec()
		{
		}
		
		internal const ulong ret_isrec_caller_preserves = 0;
		internal const ulong ret_isrec_callee_preserves = 0;
	}
}

namespace libtysila5.target.x86_64
{
	partial class x86_64_Assembler
	{
		internal void init_cc_caller_preserves_map()
		{
			cc_caller_preserves_map["sysv"] = sysv_caller_preserves;
			cc_caller_preserves_map["isr"] = isr_caller_preserves;
			cc_caller_preserves_map["isrec"] = isrec_caller_preserves;
			cc_caller_preserves_map["default"] = sysv_caller_preserves;
		}
	}
}

namespace libtysila5.target.x86_64
{
	partial class x86_64_Assembler
	{
		internal void init_cc_callee_preserves_map()
		{
			cc_callee_preserves_map["sysv"] = sysv_callee_preserves;
			cc_callee_preserves_map["isr"] = isr_callee_preserves;
			cc_callee_preserves_map["isrec"] = isrec_callee_preserves;
			cc_callee_preserves_map["default"] = sysv_callee_preserves;
		}
	}
}

namespace libtysila5.target.x86_64
{
	partial class x86_64_Assembler
	{
		internal void init_cc_classmap()
		{
			cc_classmap["sysv"] = cc_classmap_sysv;
			cc_classmap["isr"] = cc_classmap_isr;
			cc_classmap["isrec"] = cc_classmap_isrec;
			cc_classmap["default"] = cc_classmap_sysv;
		}
	}
}

namespace libtysila5.target.x86_64
{
	partial class x86_64_Assembler
	{
		internal void init_cc_map()
		{
			cc_map["sysv"] = cc_map_sysv;
			cc_map["isr"] = cc_map_isr;
			cc_map["isrec"] = cc_map_isrec;
			cc_map["default"] = cc_map_sysv;
		}
	}
}

namespace libtysila5.target.x86_64
{
	partial class x86_64_Assembler
	{
		internal void init_retcc_classmap()
		{
			retcc_classmap["ret_sysv"] = cc_classmap_ret_sysv;
			retcc_classmap["ret_isr"] = cc_classmap_ret_isr;
			retcc_classmap["ret_isrec"] = cc_classmap_ret_isrec;
			retcc_classmap["ret_default"] = cc_classmap_ret_sysv;
		}
	}
}

namespace libtysila5.target.x86_64
{
	partial class x86_64_Assembler
	{
		internal void init_retcc_map()
		{
			retcc_map["ret_sysv"] = cc_map_ret_sysv;
			retcc_map["ret_isr"] = cc_map_ret_isr;
			retcc_map["ret_isrec"] = cc_map_ret_isrec;
			retcc_map["ret_default"] = cc_map_ret_sysv;
		}
	}
}

namespace libtysila5.target.arm
{
	partial class arm_Assembler
	{
		public const int arm_add_imm = 210;
		public const int arm_add_reg = 211;
		public const int arm_add_sp_imm = 212;
		public const int arm_add_sp_reg = 213;
		public const int arm_b = 214;
		public const int arm_bl = 215;
		public const int arm_blx = 216;
		public const int arm_bx = 217;
		public const int arm_ldm = 218;
		public const int arm_ldr_imm = 219;
		public const int arm_ldr_lit = 220;
		public const int arm_ldr_reg = 221;
		public const int arm_mov_imm = 222;
		public const int arm_mov_reg = 223;
		public const int arm_movt_imm = 224;
		public const int arm_orr_imm = 225;
		public const int arm_orr_reg = 226;
		public const int arm_pop = 227;
		public const int arm_push = 228;
		public const int arm_stmdb = 229;
		public const int arm_str_imm = 230;
		public const int arm_str_reg = 231;
		public const int arm_sub_imm = 232;
		public const int arm_sub_reg = 233;
		public const int arm_sub_sp_imm = 234;
		public const int arm_sub_sp_reg = 235;
		
		internal static void init_instrs()
		{
			insts[210] = "add_imm";
			insts[211] = "add_reg";
			insts[212] = "add_sp_imm";
			insts[213] = "add_sp_reg";
			insts[214] = "b";
			insts[215] = "bl";
			insts[216] = "blx";
			insts[217] = "bx";
			insts[218] = "ldm";
			insts[219] = "ldr_imm";
			insts[220] = "ldr_lit";
			insts[221] = "ldr_reg";
			insts[222] = "mov_imm";
			insts[223] = "mov_reg";
			insts[224] = "movt_imm";
			insts[225] = "orr_imm";
			insts[226] = "orr_reg";
			insts[227] = "pop";
			insts[228] = "push";
			insts[229] = "stmdb";
			insts[230] = "str_imm";
			insts[231] = "str_reg";
			insts[232] = "sub_imm";
			insts[233] = "sub_reg";
			insts[234] = "sub_sp_imm";
			insts[235] = "sub_sp_reg";
		}
	}
}

namespace libtysila5.target.arm
{
	partial class arm_Assembler
	{
		public const int eabic_MEMORY = 236;
		public const int eabic_INTEGER = 237;
		public const int eabic_FLOAT = 238;
	}
}

namespace libtysila5.target.arm
{
	partial class arm_Assembler
	{
		public static System.Collections.Generic.Dictionary<int, int[]> cc_map_eabi = new System.Collections.Generic.Dictionary<int, int[]>(new libtysila5.GenericEqualityComparer<int>());
		public static System.Collections.Generic.Dictionary<int, int> cc_classmap_eabi = new System.Collections.Generic.Dictionary<int, int>(new libtysila5.GenericEqualityComparer<int>());
		internal static void init_eabi()
		{
			cc_classmap_eabi[96] = 237;
			cc_classmap_eabi[98] = 237;
			cc_classmap_eabi[97] = 237;
			cc_classmap_eabi[103] = 237;
			cc_classmap_eabi[104] = 237;
			cc_classmap_eabi[102] = 238;
			cc_map_eabi[237] = new int[] { 0, 1, 2, 3, 32, };
			cc_map_eabi[238] = new int[] { 16, 17, 18, 19, 20, 21, 22, 23, 32, };
			cc_map_eabi[236] = new int[] { 32, };
		}
		
		internal const ulong eabi_caller_preserves = 16715791;
		internal const ulong eabi_callee_preserves = 4278194160;
	}
}

namespace libtysila5.target.arm
{
	partial class arm_Assembler
	{
		public static System.Collections.Generic.Dictionary<int, int[]> cc_map_ret_eabi = new System.Collections.Generic.Dictionary<int, int[]>(new libtysila5.GenericEqualityComparer<int>());
		public static System.Collections.Generic.Dictionary<int, int> cc_classmap_ret_eabi = new System.Collections.Generic.Dictionary<int, int>(new libtysila5.GenericEqualityComparer<int>());
		internal static void init_ret_eabi()
		{
			cc_map_ret_eabi[96] = new int[] { 0, };
			cc_map_ret_eabi[98] = new int[] { 0, };
			cc_map_ret_eabi[103] = new int[] { 0, };
			cc_map_ret_eabi[104] = new int[] { 0, };
			cc_map_ret_eabi[97] = new int[] { 34, };
			cc_map_ret_eabi[102] = new int[] { 16, };
		}
		
		internal const ulong ret_eabi_caller_preserves = 0;
		internal const ulong ret_eabi_callee_preserves = 0;
	}
}

namespace libtysila5.target.arm
{
	partial class arm_Assembler
	{
		internal void init_cc_caller_preserves_map()
		{
			cc_caller_preserves_map["eabi"] = eabi_caller_preserves;
			cc_caller_preserves_map["default"] = eabi_caller_preserves;
		}
	}
}

namespace libtysila5.target.arm
{
	partial class arm_Assembler
	{
		internal void init_cc_callee_preserves_map()
		{
			cc_callee_preserves_map["eabi"] = eabi_callee_preserves;
			cc_callee_preserves_map["default"] = eabi_callee_preserves;
		}
	}
}

namespace libtysila5.target.arm
{
	partial class arm_Assembler
	{
		internal void init_cc_classmap()
		{
			cc_classmap["eabi"] = cc_classmap_eabi;
			cc_classmap["default"] = cc_classmap_eabi;
		}
	}
}

namespace libtysila5.target.arm
{
	partial class arm_Assembler
	{
		internal void init_cc_map()
		{
			cc_map["eabi"] = cc_map_eabi;
			cc_map["default"] = cc_map_eabi;
		}
	}
}

namespace libtysila5.target.arm
{
	partial class arm_Assembler
	{
		internal void init_retcc_classmap()
		{
			retcc_classmap["ret_eabi"] = cc_classmap_ret_eabi;
			retcc_classmap["ret_default"] = cc_classmap_ret_eabi;
		}
	}
}

namespace libtysila5.target.arm
{
	partial class arm_Assembler
	{
		internal void init_retcc_map()
		{
			retcc_map["ret_eabi"] = cc_map_ret_eabi;
			retcc_map["ret_default"] = cc_map_ret_eabi;
		}
	}
}

namespace libtysila5.target
{
	public partial class Generic
	{
		public static System.Collections.Generic.Dictionary<int, string> insts = new System.Collections.Generic.Dictionary<int, string>(new libtysila5.GenericEqualityComparer<int>());
	}
	
	public partial class Target
	{
		public static void init_targets()
		{
			libtysila5.target.Generic.init_instrs();
			
			libtysila5.target.x86.x86_Assembler.init_sysv();
			libtysila5.target.x86.x86_Assembler.init_sysv_xmm();
			libtysila5.target.x86.x86_Assembler.init_ret_sysv();
			libtysila5.target.x86.x86_Assembler.init_ret_sysv_xmm();
			var x86 = new libtysila5.target.x86.x86_Assembler();
			x86.name = "x86";
			x86.ptype = ir.Opcode.ct_int32;
			libtysila5.target.x86.x86_Assembler.registers = new Target.Reg[25];
			x86.regs = libtysila5.target.x86.x86_Assembler.registers;
			x86.regs[4] = new Target.Reg { name = "stack", id = 4, type = 2, size = 0, mask = 16 };
			libtysila5.target.x86.x86_Assembler.r_stack = x86.regs[4];
			x86.regs[5] = new Target.Reg { name = "contents", id = 5, type = 3, size = 0, mask = 32 };
			libtysila5.target.x86.x86_Assembler.r_contents = x86.regs[5];
			x86.regs[6] = new Target.Reg { name = "eip", id = 6, type = 0, size = 4, mask = 64 };
			libtysila5.target.x86.x86_Assembler.r_eip = x86.regs[6];
			x86.regs[7] = new Target.Reg { name = "eax", id = 7, type = 0, size = 4, mask = 128 };
			libtysila5.target.x86.x86_Assembler.r_eax = x86.regs[7];
			x86.regs[8] = new Target.Reg { name = "ebx", id = 8, type = 0, size = 4, mask = 256 };
			libtysila5.target.x86.x86_Assembler.r_ebx = x86.regs[8];
			x86.regs[9] = new Target.Reg { name = "ecx", id = 9, type = 0, size = 4, mask = 512 };
			libtysila5.target.x86.x86_Assembler.r_ecx = x86.regs[9];
			x86.regs[10] = new Target.Reg { name = "edx", id = 10, type = 0, size = 4, mask = 1024 };
			libtysila5.target.x86.x86_Assembler.r_edx = x86.regs[10];
			x86.regs[11] = new Target.Reg { name = "edi", id = 11, type = 0, size = 4, mask = 2048 };
			libtysila5.target.x86.x86_Assembler.r_edi = x86.regs[11];
			x86.regs[12] = new Target.Reg { name = "esi", id = 12, type = 0, size = 4, mask = 4096 };
			libtysila5.target.x86.x86_Assembler.r_esi = x86.regs[12];
			x86.regs[13] = new Target.Reg { name = "esp", id = 13, type = 0, size = 4, mask = 8192 };
			libtysila5.target.x86.x86_Assembler.r_esp = x86.regs[13];
			x86.regs[14] = new Target.Reg { name = "ebp", id = 14, type = 0, size = 4, mask = 16384 };
			libtysila5.target.x86.x86_Assembler.r_ebp = x86.regs[14];
			x86.regs[15] = new Target.Reg { name = "st0", id = 15, type = 1, size = 8, mask = 32768 };
			libtysila5.target.x86.x86_Assembler.r_st0 = x86.regs[15];
			x86.regs[16] = new Target.Reg { name = "xmm0", id = 16, type = 1, size = 8, mask = 65536 };
			libtysila5.target.x86.x86_Assembler.r_xmm0 = x86.regs[16];
			x86.regs[17] = new Target.Reg { name = "xmm1", id = 17, type = 1, size = 8, mask = 131072 };
			libtysila5.target.x86.x86_Assembler.r_xmm1 = x86.regs[17];
			x86.regs[18] = new Target.Reg { name = "xmm2", id = 18, type = 1, size = 8, mask = 262144 };
			libtysila5.target.x86.x86_Assembler.r_xmm2 = x86.regs[18];
			x86.regs[19] = new Target.Reg { name = "xmm3", id = 19, type = 1, size = 8, mask = 524288 };
			libtysila5.target.x86.x86_Assembler.r_xmm3 = x86.regs[19];
			x86.regs[20] = new Target.Reg { name = "xmm4", id = 20, type = 1, size = 8, mask = 1048576 };
			libtysila5.target.x86.x86_Assembler.r_xmm4 = x86.regs[20];
			x86.regs[21] = new Target.Reg { name = "xmm5", id = 21, type = 1, size = 8, mask = 2097152 };
			libtysila5.target.x86.x86_Assembler.r_xmm5 = x86.regs[21];
			x86.regs[22] = new Target.Reg { name = "xmm6", id = 22, type = 1, size = 8, mask = 4194304 };
			libtysila5.target.x86.x86_Assembler.r_xmm6 = x86.regs[22];
			x86.regs[23] = new Target.Reg { name = "xmm7", id = 23, type = 1, size = 8, mask = 8388608 };
			libtysila5.target.x86.x86_Assembler.r_xmm7 = x86.regs[23];
			x86.regs[24] = new Target.Reg { name = "eaxedx", id = 24, type = 4, size = 4, mask = 1152 };
			libtysila5.target.x86.x86_Assembler.r_eaxedx = x86.regs[24];
			targets["x86"] = x86;
			libtysila5.target.x86_64.x86_64_Assembler.init_sysv();
			libtysila5.target.x86_64.x86_64_Assembler.init_isr();
			libtysila5.target.x86_64.x86_64_Assembler.init_isrec();
			libtysila5.target.x86_64.x86_64_Assembler.init_ret_sysv();
			libtysila5.target.x86_64.x86_64_Assembler.init_ret_isr();
			libtysila5.target.x86_64.x86_64_Assembler.init_ret_isrec();
			var x86_64 = new libtysila5.target.x86_64.x86_64_Assembler();
			x86_64.name = "x86_64";
			x86_64.ptype = ir.Opcode.ct_int64;
			libtysila5.target.x86_64.x86_64_Assembler.registers = new Target.Reg[42];
			x86_64.regs = libtysila5.target.x86_64.x86_64_Assembler.registers;
			x86_64.regs[4] = new Target.Reg { name = "stack", id = 4, type = 2, size = 0, mask = 16 };
			libtysila5.target.x86_64.x86_64_Assembler.r_stack = x86_64.regs[4];
			x86_64.regs[5] = new Target.Reg { name = "contents", id = 5, type = 3, size = 0, mask = 32 };
			libtysila5.target.x86_64.x86_64_Assembler.r_contents = x86_64.regs[5];
			x86_64.regs[6] = new Target.Reg { name = "rip", id = 6, type = 0, size = 8, mask = 64 };
			libtysila5.target.x86_64.x86_64_Assembler.r_rip = x86_64.regs[6];
			x86_64.regs[7] = new Target.Reg { name = "rax", id = 7, type = 0, size = 8, mask = 128 };
			libtysila5.target.x86_64.x86_64_Assembler.r_rax = x86_64.regs[7];
			x86_64.regs[8] = new Target.Reg { name = "rbx", id = 8, type = 0, size = 8, mask = 256 };
			libtysila5.target.x86_64.x86_64_Assembler.r_rbx = x86_64.regs[8];
			x86_64.regs[9] = new Target.Reg { name = "rcx", id = 9, type = 0, size = 8, mask = 512 };
			libtysila5.target.x86_64.x86_64_Assembler.r_rcx = x86_64.regs[9];
			x86_64.regs[10] = new Target.Reg { name = "rdx", id = 10, type = 0, size = 8, mask = 1024 };
			libtysila5.target.x86_64.x86_64_Assembler.r_rdx = x86_64.regs[10];
			x86_64.regs[12] = new Target.Reg { name = "rsi", id = 12, type = 0, size = 8, mask = 4096 };
			libtysila5.target.x86_64.x86_64_Assembler.r_rsi = x86_64.regs[12];
			x86_64.regs[11] = new Target.Reg { name = "rdi", id = 11, type = 0, size = 8, mask = 2048 };
			libtysila5.target.x86_64.x86_64_Assembler.r_rdi = x86_64.regs[11];
			x86_64.regs[25] = new Target.Reg { name = "r8", id = 25, type = 0, size = 8, mask = 33554432 };
			libtysila5.target.x86_64.x86_64_Assembler.r_r8 = x86_64.regs[25];
			x86_64.regs[26] = new Target.Reg { name = "r9", id = 26, type = 0, size = 8, mask = 67108864 };
			libtysila5.target.x86_64.x86_64_Assembler.r_r9 = x86_64.regs[26];
			x86_64.regs[27] = new Target.Reg { name = "r10", id = 27, type = 0, size = 8, mask = 134217728 };
			libtysila5.target.x86_64.x86_64_Assembler.r_r10 = x86_64.regs[27];
			x86_64.regs[28] = new Target.Reg { name = "r11", id = 28, type = 0, size = 8, mask = 268435456 };
			libtysila5.target.x86_64.x86_64_Assembler.r_r11 = x86_64.regs[28];
			x86_64.regs[29] = new Target.Reg { name = "r12", id = 29, type = 0, size = 8, mask = 536870912 };
			libtysila5.target.x86_64.x86_64_Assembler.r_r12 = x86_64.regs[29];
			x86_64.regs[30] = new Target.Reg { name = "r13", id = 30, type = 0, size = 8, mask = 1073741824 };
			libtysila5.target.x86_64.x86_64_Assembler.r_r13 = x86_64.regs[30];
			x86_64.regs[31] = new Target.Reg { name = "r14", id = 31, type = 0, size = 8, mask = 2147483648 };
			libtysila5.target.x86_64.x86_64_Assembler.r_r14 = x86_64.regs[31];
			x86_64.regs[32] = new Target.Reg { name = "r15", id = 32, type = 0, size = 8, mask = 4294967296 };
			libtysila5.target.x86_64.x86_64_Assembler.r_r15 = x86_64.regs[32];
			x86_64.regs[16] = new Target.Reg { name = "xmm0", id = 16, type = 1, size = 8, mask = 65536 };
			libtysila5.target.x86_64.x86_64_Assembler.r_xmm0 = x86_64.regs[16];
			x86_64.regs[17] = new Target.Reg { name = "xmm1", id = 17, type = 1, size = 8, mask = 131072 };
			libtysila5.target.x86_64.x86_64_Assembler.r_xmm1 = x86_64.regs[17];
			x86_64.regs[18] = new Target.Reg { name = "xmm2", id = 18, type = 1, size = 8, mask = 262144 };
			libtysila5.target.x86_64.x86_64_Assembler.r_xmm2 = x86_64.regs[18];
			x86_64.regs[19] = new Target.Reg { name = "xmm3", id = 19, type = 1, size = 8, mask = 524288 };
			libtysila5.target.x86_64.x86_64_Assembler.r_xmm3 = x86_64.regs[19];
			x86_64.regs[20] = new Target.Reg { name = "xmm4", id = 20, type = 1, size = 8, mask = 1048576 };
			libtysila5.target.x86_64.x86_64_Assembler.r_xmm4 = x86_64.regs[20];
			x86_64.regs[21] = new Target.Reg { name = "xmm5", id = 21, type = 1, size = 8, mask = 2097152 };
			libtysila5.target.x86_64.x86_64_Assembler.r_xmm5 = x86_64.regs[21];
			x86_64.regs[22] = new Target.Reg { name = "xmm6", id = 22, type = 1, size = 8, mask = 4194304 };
			libtysila5.target.x86_64.x86_64_Assembler.r_xmm6 = x86_64.regs[22];
			x86_64.regs[23] = new Target.Reg { name = "xmm7", id = 23, type = 1, size = 8, mask = 8388608 };
			libtysila5.target.x86_64.x86_64_Assembler.r_xmm7 = x86_64.regs[23];
			x86_64.regs[33] = new Target.Reg { name = "xmm8", id = 33, type = 1, size = 8, mask = 8589934592 };
			libtysila5.target.x86_64.x86_64_Assembler.r_xmm8 = x86_64.regs[33];
			x86_64.regs[34] = new Target.Reg { name = "xmm9", id = 34, type = 1, size = 8, mask = 17179869184 };
			libtysila5.target.x86_64.x86_64_Assembler.r_xmm9 = x86_64.regs[34];
			x86_64.regs[35] = new Target.Reg { name = "xmm10", id = 35, type = 1, size = 8, mask = 34359738368 };
			libtysila5.target.x86_64.x86_64_Assembler.r_xmm10 = x86_64.regs[35];
			x86_64.regs[36] = new Target.Reg { name = "xmm11", id = 36, type = 1, size = 8, mask = 68719476736 };
			libtysila5.target.x86_64.x86_64_Assembler.r_xmm11 = x86_64.regs[36];
			x86_64.regs[37] = new Target.Reg { name = "xmm12", id = 37, type = 1, size = 8, mask = 137438953472 };
			libtysila5.target.x86_64.x86_64_Assembler.r_xmm12 = x86_64.regs[37];
			x86_64.regs[38] = new Target.Reg { name = "xmm13", id = 38, type = 1, size = 8, mask = 274877906944 };
			libtysila5.target.x86_64.x86_64_Assembler.r_xmm13 = x86_64.regs[38];
			x86_64.regs[39] = new Target.Reg { name = "xmm14", id = 39, type = 1, size = 8, mask = 549755813888 };
			libtysila5.target.x86_64.x86_64_Assembler.r_xmm14 = x86_64.regs[39];
			x86_64.regs[40] = new Target.Reg { name = "xmm15", id = 40, type = 1, size = 8, mask = 1099511627776 };
			libtysila5.target.x86_64.x86_64_Assembler.r_xmm15 = x86_64.regs[40];
			x86_64.regs[41] = new Target.Reg { name = "raxrdx", id = 41, type = 4, size = 8, mask = 1152 };
			libtysila5.target.x86_64.x86_64_Assembler.r_raxrdx = x86_64.regs[41];
			targets["x86_64"] = x86_64;
			libtysila5.target.arm.arm_Assembler.init_eabi();
			libtysila5.target.arm.arm_Assembler.init_ret_eabi();
			var arm = new libtysila5.target.arm.arm_Assembler();
			arm.name = "arm";
			arm.ptype = ir.Opcode.ct_int32;
			libtysila5.target.arm.arm_Assembler.registers = new Target.Reg[35];
			arm.regs = libtysila5.target.arm.arm_Assembler.registers;
			arm.regs[0] = new Target.Reg { name = "r0", id = 0, type = 0, size = 4, mask = 1 };
			libtysila5.target.arm.arm_Assembler.r_r0 = arm.regs[0];
			arm.regs[1] = new Target.Reg { name = "r1", id = 1, type = 0, size = 4, mask = 2 };
			libtysila5.target.arm.arm_Assembler.r_r1 = arm.regs[1];
			arm.regs[2] = new Target.Reg { name = "r2", id = 2, type = 0, size = 4, mask = 4 };
			libtysila5.target.arm.arm_Assembler.r_r2 = arm.regs[2];
			arm.regs[3] = new Target.Reg { name = "r3", id = 3, type = 0, size = 4, mask = 8 };
			libtysila5.target.arm.arm_Assembler.r_r3 = arm.regs[3];
			arm.regs[4] = new Target.Reg { name = "r4", id = 4, type = 0, size = 4, mask = 16 };
			libtysila5.target.arm.arm_Assembler.r_r4 = arm.regs[4];
			arm.regs[5] = new Target.Reg { name = "r5", id = 5, type = 0, size = 4, mask = 32 };
			libtysila5.target.arm.arm_Assembler.r_r5 = arm.regs[5];
			arm.regs[6] = new Target.Reg { name = "r6", id = 6, type = 0, size = 4, mask = 64 };
			libtysila5.target.arm.arm_Assembler.r_r6 = arm.regs[6];
			arm.regs[7] = new Target.Reg { name = "r7", id = 7, type = 0, size = 4, mask = 128 };
			libtysila5.target.arm.arm_Assembler.r_r7 = arm.regs[7];
			arm.regs[8] = new Target.Reg { name = "r8", id = 8, type = 0, size = 4, mask = 256 };
			libtysila5.target.arm.arm_Assembler.r_r8 = arm.regs[8];
			arm.regs[9] = new Target.Reg { name = "r9", id = 9, type = 0, size = 4, mask = 512 };
			libtysila5.target.arm.arm_Assembler.r_r9 = arm.regs[9];
			arm.regs[10] = new Target.Reg { name = "r10", id = 10, type = 0, size = 4, mask = 1024 };
			libtysila5.target.arm.arm_Assembler.r_r10 = arm.regs[10];
			arm.regs[11] = new Target.Reg { name = "r11", id = 11, type = 0, size = 4, mask = 2048 };
			libtysila5.target.arm.arm_Assembler.r_r11 = arm.regs[11];
			arm.regs[12] = new Target.Reg { name = "r12", id = 12, type = 0, size = 4, mask = 4096 };
			libtysila5.target.arm.arm_Assembler.r_r12 = arm.regs[12];
			arm.regs[13] = new Target.Reg { name = "r13", id = 13, type = 0, size = 4, mask = 8192 };
			libtysila5.target.arm.arm_Assembler.r_r13 = arm.regs[13];
			arm.regs[14] = new Target.Reg { name = "r14", id = 14, type = 0, size = 4, mask = 16384 };
			libtysila5.target.arm.arm_Assembler.r_r14 = arm.regs[14];
			arm.regs[15] = new Target.Reg { name = "r15", id = 15, type = 0, size = 4, mask = 32768 };
			libtysila5.target.arm.arm_Assembler.r_r15 = arm.regs[15];
			arm.regs[16] = new Target.Reg { name = "s0", id = 16, type = 1, size = 4, mask = 65536 };
			libtysila5.target.arm.arm_Assembler.r_s0 = arm.regs[16];
			arm.regs[17] = new Target.Reg { name = "s1", id = 17, type = 1, size = 4, mask = 131072 };
			libtysila5.target.arm.arm_Assembler.r_s1 = arm.regs[17];
			arm.regs[18] = new Target.Reg { name = "s2", id = 18, type = 1, size = 4, mask = 262144 };
			libtysila5.target.arm.arm_Assembler.r_s2 = arm.regs[18];
			arm.regs[19] = new Target.Reg { name = "s3", id = 19, type = 1, size = 4, mask = 524288 };
			libtysila5.target.arm.arm_Assembler.r_s3 = arm.regs[19];
			arm.regs[20] = new Target.Reg { name = "s4", id = 20, type = 1, size = 4, mask = 1048576 };
			libtysila5.target.arm.arm_Assembler.r_s4 = arm.regs[20];
			arm.regs[21] = new Target.Reg { name = "s5", id = 21, type = 1, size = 4, mask = 2097152 };
			libtysila5.target.arm.arm_Assembler.r_s5 = arm.regs[21];
			arm.regs[22] = new Target.Reg { name = "s6", id = 22, type = 1, size = 4, mask = 4194304 };
			libtysila5.target.arm.arm_Assembler.r_s6 = arm.regs[22];
			arm.regs[23] = new Target.Reg { name = "s7", id = 23, type = 1, size = 4, mask = 8388608 };
			libtysila5.target.arm.arm_Assembler.r_s7 = arm.regs[23];
			arm.regs[24] = new Target.Reg { name = "s8", id = 24, type = 1, size = 4, mask = 16777216 };
			libtysila5.target.arm.arm_Assembler.r_s8 = arm.regs[24];
			arm.regs[25] = new Target.Reg { name = "s9", id = 25, type = 1, size = 4, mask = 33554432 };
			libtysila5.target.arm.arm_Assembler.r_s9 = arm.regs[25];
			arm.regs[26] = new Target.Reg { name = "s10", id = 26, type = 1, size = 4, mask = 67108864 };
			libtysila5.target.arm.arm_Assembler.r_s10 = arm.regs[26];
			arm.regs[27] = new Target.Reg { name = "s11", id = 27, type = 1, size = 4, mask = 134217728 };
			libtysila5.target.arm.arm_Assembler.r_s11 = arm.regs[27];
			arm.regs[28] = new Target.Reg { name = "s12", id = 28, type = 1, size = 4, mask = 268435456 };
			libtysila5.target.arm.arm_Assembler.r_s12 = arm.regs[28];
			arm.regs[29] = new Target.Reg { name = "s13", id = 29, type = 1, size = 4, mask = 536870912 };
			libtysila5.target.arm.arm_Assembler.r_s13 = arm.regs[29];
			arm.regs[30] = new Target.Reg { name = "s14", id = 30, type = 1, size = 4, mask = 1073741824 };
			libtysila5.target.arm.arm_Assembler.r_s14 = arm.regs[30];
			arm.regs[31] = new Target.Reg { name = "s15", id = 31, type = 1, size = 4, mask = 2147483648 };
			libtysila5.target.arm.arm_Assembler.r_s15 = arm.regs[31];
			arm.regs[15] = new Target.Reg { name = "pc", id = 15, type = 0, size = 4, mask = 32768 };
			libtysila5.target.arm.arm_Assembler.r_pc = arm.regs[15];
			arm.regs[14] = new Target.Reg { name = "lr", id = 14, type = 0, size = 4, mask = 16384 };
			libtysila5.target.arm.arm_Assembler.r_lr = arm.regs[14];
			arm.regs[13] = new Target.Reg { name = "sp", id = 13, type = 0, size = 4, mask = 8192 };
			libtysila5.target.arm.arm_Assembler.r_sp = arm.regs[13];
			arm.regs[12] = new Target.Reg { name = "ip", id = 12, type = 0, size = 4, mask = 4096 };
			libtysila5.target.arm.arm_Assembler.r_ip = arm.regs[12];
			arm.regs[11] = new Target.Reg { name = "fp", id = 11, type = 0, size = 4, mask = 2048 };
			libtysila5.target.arm.arm_Assembler.r_fp = arm.regs[11];
			arm.regs[32] = new Target.Reg { name = "stack", id = 32, type = 2, size = 0, mask = 4294967296 };
			libtysila5.target.arm.arm_Assembler.r_stack = arm.regs[32];
			arm.regs[33] = new Target.Reg { name = "contents", id = 33, type = 3, size = 0, mask = 8589934592 };
			libtysila5.target.arm.arm_Assembler.r_contents = arm.regs[33];
			arm.regs[34] = new Target.Reg { name = "r0r1", id = 34, type = 4, size = 4, mask = 3 };
			libtysila5.target.arm.arm_Assembler.r_r0r1 = arm.regs[34];
			targets["arm"] = arm;
		}
	}
}

namespace libtysila5.target.x86
{
	public partial class x86_Assembler
	{
		public static Target.Reg[] registers;
		public static Target.Reg r_stack;
		public static Target.Reg r_contents;
		public static Target.Reg r_eip;
		public static Target.Reg r_eax;
		public static Target.Reg r_ebx;
		public static Target.Reg r_ecx;
		public static Target.Reg r_edx;
		public static Target.Reg r_edi;
		public static Target.Reg r_esi;
		public static Target.Reg r_esp;
		public static Target.Reg r_ebp;
		public static Target.Reg r_st0;
		public static Target.Reg r_xmm0;
		public static Target.Reg r_xmm1;
		public static Target.Reg r_xmm2;
		public static Target.Reg r_xmm3;
		public static Target.Reg r_xmm4;
		public static Target.Reg r_xmm5;
		public static Target.Reg r_xmm6;
		public static Target.Reg r_xmm7;
		public static Target.Reg r_eaxedx;
		
		void init_ccs()
		{
			init_cc_callee_preserves_map();
			init_cc_caller_preserves_map();
			init_cc_map();
			init_retcc_map();
			init_cc_classmap();
			init_retcc_classmap();
		}
		
		internal x86_Assembler()
		{
			init_ccs();
			init_options();
			ct_regs[96] = 6912;
			ct_regs[97] = 0;
			ct_regs[102] = 8257536;
			ct_regs[98] = ct_regs[96];
			ct_regs[103] = ct_regs[96];
			ct_regs[104] = ct_regs[96];
			instrs.trie = x86_instrs;
			instrs.start = x86_instrs_start;
			instrs.vals = x86_instrs_vals;
			psize = 4;
		}
		
		int[] x86_instrs = new int[] {
			0, 1, 0, 0, 1, 0, 2, 0, 0, 1, 0, 3, 0, 0, 1, 0, 
			4, 0, 0, 1, 0, 5, 0, 0, 1, 0, 6, 0, 0, 1, 0, 7, 0, 
			0, 1, 0, 8, 0, 0, 1, 0, 9, 0, 0, 1, 0, 10, 0, 0, 1, 
			0, 11, 0, 0, 1, 0, 12, 0, 0, 1, 0, 13, 0, 0, 1, 0, 14, 
			0, 0, 1, 0, 15, 0, 0, 1, 0, 16, 0, 0, 1, 0, 17, 0, 0, 
			1, 0, 18, 0, 0, 1, 0, 19, 0, 0, 1, 0, 20, 0, 0, 1, 0, 
			21, 0, 0, 1, 0, 22, 0, 0, 1, 0, 23, 0, 0, 1, 0, 24, 0, 
			0, 1, 0, 25, 0, 0, 1, 0, 26, 0, 0, 1, 0, 27, 1, 41, 2, 
			121, 126, 28, 0, 0, 1, 0, 0, 1, 42, 1, 137, 0, 2, 21, 1, 142, 
			0, 3, 58, 1, 147, 0, 4, 21, 1, 152, 29, 0, 0, 1, 0, 0, 1, 
			65, 1, 162, 0, 2, 42, 1, 167, 0, 3, 21, 1, 172, 0, 4, 42, 1, 
			177, 0, 5, 21, 1, 182, 0, 6, 58, 1, 187, 0, 7, 40, 26, 157, 0, 
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
			0, 0, 0, 0, 0, 0, 192, 30, 0, 0, 1, 0, 31, 0, 0, 1, 0, 
			0, 1, 41, 1, 232, 0, 2, 21, 1, 237, 32, 8, 21, 38, 131, 0, 197, 
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 227, 0, 0, 0, 0, 0, 0, 
			242, 33, 0, 0, 1, 0, 34, 0, 0, 1, 0, 35, 0, 0, 1, 0, 36, 
			0, 0, 1, 0, 37, 0, 0, 1, 0, 38, 0, 0, 1, 0, 39, 0, 0, 
			1, 0, 40, 0, 0, 1, 0, 41, 0, 0, 1, 0, 42, 0, 0, 1, 0, 
			43, 0, 0, 1, 0, 44, 0, 0, 1, 0, 45, 0, 0, 1, 0, 46, 0, 
			0, 1, 0, 47, 0, 0, 1, 0, 48, 0, 0, 1, 0, 49, 0, 0, 1, 
			0, 50, 0, 0, 1, 0, 51, 0, 0, 1, 0, 52, 0, 0, 1, 0, 53, 
			0, 0, 1, 0, 54, 0, 0, 1, 0, 55, 0, 0, 1, 0, 56, 0, 0, 
			1, 0, 0, 9, 21, 62, 1, 6, 11, 16, 21, 26, 31, 36, 41, 46, 0, 
			51, 0, 56, 61, 66, 71, 76, 81, 86, 91, 96, 0, 0, 0, 0, 101, 0, 
			106, 111, 116, 0, 0, 0, 0, 0, 0, 247, 289, 294, 299, 304, 309, 314, 319, 
			324, 329, 334, 339, 344, 349, 354, 359, 364, 369, 374, 379, 384, 389, 394, 399, 404, 
		};
		
		InstructionHandler[] x86_instrs_vals = new InstructionHandler[] {
			default(InstructionHandler),
			libtysila5.target.x86.x86_Assembler.handle_add,
			libtysila5.target.x86.x86_Assembler.handle_sub,
			libtysila5.target.x86.x86_Assembler.handle_mul,
			libtysila5.target.x86.x86_Assembler.handle_div,
			libtysila5.target.x86.x86_Assembler.handle_and,
			libtysila5.target.x86.x86_Assembler.handle_or,
			libtysila5.target.x86.x86_Assembler.handle_xor,
			libtysila5.target.x86.x86_Assembler.handle_not,
			libtysila5.target.x86.x86_Assembler.handle_neg,
			libtysila5.target.x86.x86_Assembler.handle_call,
			libtysila5.target.x86.x86_Assembler.handle_calli,
			libtysila5.target.x86.x86_Assembler.handle_ret,
			libtysila5.target.x86.x86_Assembler.handle_cmp,
			libtysila5.target.x86.x86_Assembler.handle_br,
			libtysila5.target.x86.x86_Assembler.handle_brif,
			libtysila5.target.x86.x86_Assembler.handle_enter,
			libtysila5.target.x86.x86_Assembler.handle_enter_handler,
			libtysila5.target.x86.x86_Assembler.handle_conv,
			libtysila5.target.x86.x86_Assembler.handle_stind,
			libtysila5.target.x86.x86_Assembler.handle_ldind,
			libtysila5.target.x86.x86_Assembler.handle_ldlabaddr,
			libtysila5.target.x86.x86_Assembler.handle_ldfp,
			libtysila5.target.x86.x86_Assembler.handle_ldloca,
			libtysila5.target.x86.x86_Assembler.handle_zeromem,
			libtysila5.target.x86.x86_Assembler.handle_ldc_add_stind,
			libtysila5.target.x86.x86_Assembler.handle_ldc_add_ldind,
			libtysila5.target.x86.x86_Assembler.handle_ldc_add,
			libtysila5.target.x86.x86_Assembler.handle_getCharSeq,
			libtysila5.target.x86.x86_Assembler.handle_ldelem,
			libtysila5.target.x86.x86_Assembler.handle_ldc_zeromem,
			libtysila5.target.x86.x86_Assembler.handle_ldc_ldc_add_stind,
			libtysila5.target.x86.x86_Assembler.handle_ldc,
			libtysila5.target.x86.x86_Assembler.handle_ldloc,
			libtysila5.target.x86.x86_Assembler.handle_stloc,
			libtysila5.target.x86.x86_Assembler.handle_rem,
			libtysila5.target.x86.x86_Assembler.handle_ldarg,
			libtysila5.target.x86.x86_Assembler.handle_starg,
			libtysila5.target.x86.x86_Assembler.handle_ldarga,
			libtysila5.target.x86.x86_Assembler.handle_stackcopy,
			libtysila5.target.x86.x86_Assembler.handle_localloc,
			libtysila5.target.x86.x86_Assembler.handle_shift,
			libtysila5.target.x86.x86_Assembler.handle_shift,
			libtysila5.target.x86.x86_Assembler.handle_shift,
			libtysila5.target.x86.x86_Assembler.handle_switch,
			libtysila5.target.x86.x86_Assembler.handle_ldobja,
			libtysila5.target.x86.x86_Assembler.handle_cctor_runonce,
			libtysila5.target.x86.x86_Assembler.handle_break,
			libtysila5.target.x86.x86_Assembler.handle_mclabel,
			libtysila5.target.x86.x86_Assembler.handle_memcpy,
			libtysila5.target.x86.x86_Assembler.handle_memset,
			libtysila5.target.x86.x86_Assembler.handle_syncvalcompareandswap,
			libtysila5.target.x86.x86_Assembler.handle_syncvalswap,
			libtysila5.target.x86.x86_Assembler.handle_spinlockhint,
			libtysila5.target.x86.x86_Assembler.handle_target_specific,
			libtysila5.target.x86.x86_Assembler.handle_portin,
			libtysila5.target.x86.x86_Assembler.handle_portout,
		};
		
		int x86_instrs_start = 409;
	}
}

namespace libtysila5.target.x86_64
{
	public partial class x86_64_Assembler
	{
		public static Target.Reg[] registers;
		public static Target.Reg r_stack;
		public static Target.Reg r_contents;
		public static Target.Reg r_rip;
		public static Target.Reg r_rax;
		public static Target.Reg r_rbx;
		public static Target.Reg r_rcx;
		public static Target.Reg r_rdx;
		public static Target.Reg r_rsi;
		public static Target.Reg r_rdi;
		public static Target.Reg r_r8;
		public static Target.Reg r_r9;
		public static Target.Reg r_r10;
		public static Target.Reg r_r11;
		public static Target.Reg r_r12;
		public static Target.Reg r_r13;
		public static Target.Reg r_r14;
		public static Target.Reg r_r15;
		public static Target.Reg r_xmm0;
		public static Target.Reg r_xmm1;
		public static Target.Reg r_xmm2;
		public static Target.Reg r_xmm3;
		public static Target.Reg r_xmm4;
		public static Target.Reg r_xmm5;
		public static Target.Reg r_xmm6;
		public static Target.Reg r_xmm7;
		public static Target.Reg r_xmm8;
		public static Target.Reg r_xmm9;
		public static Target.Reg r_xmm10;
		public static Target.Reg r_xmm11;
		public static Target.Reg r_xmm12;
		public static Target.Reg r_xmm13;
		public static Target.Reg r_xmm14;
		public static Target.Reg r_xmm15;
		public static Target.Reg r_raxrdx;
		
		void init_ccs()
		{
			init_cc_callee_preserves_map();
			init_cc_caller_preserves_map();
			init_cc_map();
			init_retcc_map();
			init_cc_classmap();
			init_retcc_classmap();
		}
		
		internal x86_64_Assembler()
		{
			init_ccs();
			init_options();
			ct_regs[96] = 8556387072;
			ct_regs[97] = 8556387072;
			ct_regs[102] = 2190441578496;
			ct_regs[98] = ct_regs[97];
			ct_regs[103] = ct_regs[97];
			ct_regs[104] = ct_regs[97];
			instrs.trie = x86_64_instrs;
			instrs.start = x86_64_instrs_start;
			instrs.vals = x86_64_instrs_vals;
			psize = 8;
		}
		
		int[] x86_64_instrs = new int[] {
			0, 1, 0, 0, 1, 0, 2, 0, 0, 1, 0, 3, 0, 0, 1, 0, 
			4, 0, 0, 1, 0, 5, 0, 0, 1, 0, 6, 0, 0, 1, 0, 7, 0, 
			0, 1, 0, 8, 0, 0, 1, 0, 9, 0, 0, 1, 0, 10, 0, 0, 1, 
			0, 11, 0, 0, 1, 0, 12, 0, 0, 1, 0, 13, 0, 0, 1, 0, 14, 
			0, 0, 1, 0, 15, 0, 0, 1, 0, 16, 0, 0, 1, 0, 17, 0, 0, 
			1, 0, 18, 0, 0, 1, 0, 19, 0, 0, 1, 0, 20, 0, 0, 1, 0, 
			21, 0, 0, 1, 0, 22, 0, 0, 1, 0, 23, 0, 0, 1, 0, 24, 0, 
			0, 1, 0, 25, 0, 0, 1, 0, 26, 0, 0, 1, 0, 27, 1, 41, 2, 
			121, 126, 28, 0, 0, 1, 0, 0, 1, 42, 1, 137, 0, 2, 21, 1, 142, 
			0, 3, 58, 1, 147, 0, 4, 21, 1, 152, 29, 0, 0, 1, 0, 0, 1, 
			65, 1, 162, 0, 2, 42, 1, 167, 0, 3, 21, 1, 172, 0, 4, 42, 1, 
			177, 0, 5, 21, 1, 182, 0, 6, 58, 1, 187, 0, 7, 40, 26, 157, 0, 
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
			0, 0, 0, 0, 0, 0, 192, 30, 0, 0, 1, 0, 31, 0, 0, 1, 0, 
			0, 1, 41, 1, 232, 0, 2, 21, 1, 237, 32, 8, 21, 38, 131, 0, 197, 
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 227, 0, 0, 0, 0, 0, 0, 
			242, 33, 0, 0, 1, 0, 34, 0, 0, 1, 0, 35, 0, 0, 1, 0, 36, 
			0, 0, 1, 0, 37, 0, 0, 1, 0, 38, 0, 0, 1, 0, 39, 0, 0, 
			1, 0, 40, 0, 0, 1, 0, 41, 0, 0, 1, 0, 42, 0, 0, 1, 0, 
			43, 0, 0, 1, 0, 44, 0, 0, 1, 0, 45, 0, 0, 1, 0, 46, 0, 
			0, 1, 0, 47, 0, 0, 1, 0, 48, 0, 0, 1, 0, 49, 0, 0, 1, 
			0, 50, 0, 0, 1, 0, 51, 0, 0, 1, 0, 52, 0, 0, 1, 0, 53, 
			0, 0, 1, 0, 54, 0, 0, 1, 0, 55, 0, 0, 1, 0, 56, 0, 0, 
			1, 0, 0, 9, 21, 62, 1, 6, 11, 16, 21, 26, 31, 36, 41, 46, 0, 
			51, 0, 56, 61, 66, 71, 76, 81, 86, 91, 96, 0, 0, 0, 0, 101, 0, 
			106, 111, 116, 0, 0, 0, 0, 0, 0, 247, 289, 294, 299, 304, 309, 314, 319, 
			324, 329, 334, 339, 344, 349, 354, 359, 364, 369, 374, 379, 384, 389, 394, 399, 404, 
		};
		
		InstructionHandler[] x86_64_instrs_vals = new InstructionHandler[] {
			default(InstructionHandler),
			libtysila5.target.x86_64.x86_64_Assembler.handle_add,
			libtysila5.target.x86_64.x86_64_Assembler.handle_sub,
			libtysila5.target.x86_64.x86_64_Assembler.handle_mul,
			libtysila5.target.x86_64.x86_64_Assembler.handle_div,
			libtysila5.target.x86_64.x86_64_Assembler.handle_and,
			libtysila5.target.x86_64.x86_64_Assembler.handle_or,
			libtysila5.target.x86_64.x86_64_Assembler.handle_xor,
			libtysila5.target.x86_64.x86_64_Assembler.handle_not,
			libtysila5.target.x86_64.x86_64_Assembler.handle_neg,
			libtysila5.target.x86_64.x86_64_Assembler.handle_call,
			libtysila5.target.x86_64.x86_64_Assembler.handle_calli,
			libtysila5.target.x86_64.x86_64_Assembler.handle_ret,
			libtysila5.target.x86_64.x86_64_Assembler.handle_cmp,
			libtysila5.target.x86_64.x86_64_Assembler.handle_br,
			libtysila5.target.x86_64.x86_64_Assembler.handle_brif,
			libtysila5.target.x86_64.x86_64_Assembler.handle_enter,
			libtysila5.target.x86_64.x86_64_Assembler.handle_enter_handler,
			libtysila5.target.x86_64.x86_64_Assembler.handle_conv,
			libtysila5.target.x86_64.x86_64_Assembler.handle_stind,
			libtysila5.target.x86_64.x86_64_Assembler.handle_ldind,
			libtysila5.target.x86_64.x86_64_Assembler.handle_ldlabaddr,
			libtysila5.target.x86_64.x86_64_Assembler.handle_ldfp,
			libtysila5.target.x86_64.x86_64_Assembler.handle_ldloca,
			libtysila5.target.x86_64.x86_64_Assembler.handle_zeromem,
			libtysila5.target.x86_64.x86_64_Assembler.handle_ldc_add_stind,
			libtysila5.target.x86_64.x86_64_Assembler.handle_ldc_add_ldind,
			libtysila5.target.x86_64.x86_64_Assembler.handle_ldc_add,
			libtysila5.target.x86_64.x86_64_Assembler.handle_getCharSeq,
			libtysila5.target.x86_64.x86_64_Assembler.handle_ldelem,
			libtysila5.target.x86_64.x86_64_Assembler.handle_ldc_zeromem,
			libtysila5.target.x86_64.x86_64_Assembler.handle_ldc_ldc_add_stind,
			libtysila5.target.x86_64.x86_64_Assembler.handle_ldc,
			libtysila5.target.x86_64.x86_64_Assembler.handle_ldloc,
			libtysila5.target.x86_64.x86_64_Assembler.handle_stloc,
			libtysila5.target.x86_64.x86_64_Assembler.handle_rem,
			libtysila5.target.x86_64.x86_64_Assembler.handle_ldarg,
			libtysila5.target.x86_64.x86_64_Assembler.handle_starg,
			libtysila5.target.x86_64.x86_64_Assembler.handle_ldarga,
			libtysila5.target.x86_64.x86_64_Assembler.handle_stackcopy,
			libtysila5.target.x86_64.x86_64_Assembler.handle_localloc,
			libtysila5.target.x86_64.x86_64_Assembler.handle_shift,
			libtysila5.target.x86_64.x86_64_Assembler.handle_shift,
			libtysila5.target.x86_64.x86_64_Assembler.handle_shift,
			libtysila5.target.x86_64.x86_64_Assembler.handle_switch,
			libtysila5.target.x86_64.x86_64_Assembler.handle_ldobja,
			libtysila5.target.x86_64.x86_64_Assembler.handle_cctor_runonce,
			libtysila5.target.x86_64.x86_64_Assembler.handle_break,
			libtysila5.target.x86_64.x86_64_Assembler.handle_mclabel,
			libtysila5.target.x86_64.x86_64_Assembler.handle_memcpy,
			libtysila5.target.x86_64.x86_64_Assembler.handle_memset,
			libtysila5.target.x86_64.x86_64_Assembler.handle_syncvalcompareandswap,
			libtysila5.target.x86_64.x86_64_Assembler.handle_syncvalswap,
			libtysila5.target.x86_64.x86_64_Assembler.handle_spinlockhint,
			libtysila5.target.x86_64.x86_64_Assembler.handle_target_specific,
			libtysila5.target.x86_64.x86_64_Assembler.handle_portin,
			libtysila5.target.x86_64.x86_64_Assembler.handle_portout,
		};
		
		int x86_64_instrs_start = 409;
	}
}

namespace libtysila5.target.arm
{
	public partial class arm_Assembler
	{
		public static Target.Reg[] registers;
		public static Target.Reg r_r0;
		public static Target.Reg r_r1;
		public static Target.Reg r_r2;
		public static Target.Reg r_r3;
		public static Target.Reg r_r4;
		public static Target.Reg r_r5;
		public static Target.Reg r_r6;
		public static Target.Reg r_r7;
		public static Target.Reg r_r8;
		public static Target.Reg r_r9;
		public static Target.Reg r_r10;
		public static Target.Reg r_r11;
		public static Target.Reg r_r12;
		public static Target.Reg r_r13;
		public static Target.Reg r_r14;
		public static Target.Reg r_r15;
		public static Target.Reg r_s0;
		public static Target.Reg r_s1;
		public static Target.Reg r_s2;
		public static Target.Reg r_s3;
		public static Target.Reg r_s4;
		public static Target.Reg r_s5;
		public static Target.Reg r_s6;
		public static Target.Reg r_s7;
		public static Target.Reg r_s8;
		public static Target.Reg r_s9;
		public static Target.Reg r_s10;
		public static Target.Reg r_s11;
		public static Target.Reg r_s12;
		public static Target.Reg r_s13;
		public static Target.Reg r_s14;
		public static Target.Reg r_s15;
		public static Target.Reg r_pc;
		public static Target.Reg r_lr;
		public static Target.Reg r_sp;
		public static Target.Reg r_ip;
		public static Target.Reg r_fp;
		public static Target.Reg r_stack;
		public static Target.Reg r_contents;
		public static Target.Reg r_r0r1;
		
		void init_ccs()
		{
			init_cc_callee_preserves_map();
			init_cc_caller_preserves_map();
			init_cc_map();
			init_retcc_map();
			init_cc_classmap();
			init_retcc_classmap();
		}
		
		internal arm_Assembler()
		{
			init_ccs();
			init_options();
			ct_regs[96] = 1023;
			ct_regs[97] = 0;
			ct_regs[102] = 4294901760;
			ct_regs[98] = ct_regs[96];
			ct_regs[103] = ct_regs[96];
			ct_regs[104] = ct_regs[96];
			instrs.trie = arm_instrs;
			instrs.start = arm_instrs_start;
			instrs.vals = arm_instrs_vals;
			psize = 4;
		}
		
		int[] arm_instrs = new int[] {
			0, 1, 0, 0, 1, 0, 2, 0, 0, 1, 0, 3, 0, 0, 1, 0, 
			4, 0, 0, 1, 0, 5, 0, 0, 1, 0, 6, 0, 0, 1, 0, 7, 0, 
			0, 1, 0, 8, 0, 0, 1, 0, 9, 0, 0, 1, 0, 10, 0, 0, 1, 
			0, 11, 0, 0, 1, 0, 12, 0, 0, 1, 0, 13, 0, 0, 1, 0, 14, 
			0, 0, 1, 0, 15, 0, 0, 1, 0, 16, 0, 0, 1, 0, 17, 0, 0, 
			1, 0, 18, 0, 0, 1, 0, 19, 0, 0, 1, 0, 20, 0, 0, 1, 0, 
			21, 0, 0, 1, 0, 22, 0, 0, 1, 0, 23, 0, 0, 1, 0, 24, 0, 
			0, 1, 0, 25, 0, 0, 1, 0, 26, 0, 0, 1, 0, 27, 0, 0, 1, 
			0, 28, 0, 0, 1, 0, 29, 0, 0, 1, 0, 30, 0, 0, 1, 0, 31, 
			0, 0, 1, 0, 32, 0, 0, 1, 0, 33, 0, 0, 1, 0, 34, 0, 0, 
			1, 0, 35, 0, 0, 1, 0, 36, 0, 0, 1, 0, 37, 0, 0, 1, 0, 
			38, 0, 0, 1, 0, 39, 0, 0, 1, 0, 40, 0, 0, 1, 0, 41, 0, 
			0, 1, 0, 42, 0, 0, 1, 0, 43, 0, 0, 1, 0, 44, 0, 0, 1, 
			0, 45, 0, 0, 1, 0, 46, 0, 0, 1, 0, 47, 0, 0, 1, 0, 0, 
			1, 21, 60, 1, 6, 11, 16, 21, 26, 31, 36, 41, 46, 0, 51, 0, 56, 
			61, 66, 71, 76, 81, 86, 91, 96, 0, 0, 0, 0, 101, 0, 106, 111, 116, 
			0, 0, 0, 0, 0, 0, 121, 126, 131, 136, 141, 146, 151, 156, 161, 166, 171, 
			176, 181, 186, 191, 196, 201, 206, 211, 216, 221, 226, 231, 
		};
		
		InstructionHandler[] arm_instrs_vals = new InstructionHandler[] {
			default(InstructionHandler),
			libtysila5.target.arm.arm_Assembler.handle_add,
			libtysila5.target.arm.arm_Assembler.handle_sub,
			libtysila5.target.arm.arm_Assembler.handle_mul,
			libtysila5.target.arm.arm_Assembler.handle_div,
			libtysila5.target.arm.arm_Assembler.handle_and,
			libtysila5.target.arm.arm_Assembler.handle_or,
			libtysila5.target.arm.arm_Assembler.handle_xor,
			libtysila5.target.arm.arm_Assembler.handle_not,
			libtysila5.target.arm.arm_Assembler.handle_neg,
			libtysila5.target.arm.arm_Assembler.handle_call,
			libtysila5.target.arm.arm_Assembler.handle_calli,
			libtysila5.target.arm.arm_Assembler.handle_ret,
			libtysila5.target.arm.arm_Assembler.handle_cmp,
			libtysila5.target.arm.arm_Assembler.handle_br,
			libtysila5.target.arm.arm_Assembler.handle_brif,
			libtysila5.target.arm.arm_Assembler.handle_enter,
			libtysila5.target.arm.arm_Assembler.handle_enter_handler,
			libtysila5.target.arm.arm_Assembler.handle_conv,
			libtysila5.target.arm.arm_Assembler.handle_stind,
			libtysila5.target.arm.arm_Assembler.handle_ldind,
			libtysila5.target.arm.arm_Assembler.handle_ldlabaddr,
			libtysila5.target.arm.arm_Assembler.handle_ldfp,
			libtysila5.target.arm.arm_Assembler.handle_ldloca,
			libtysila5.target.arm.arm_Assembler.handle_zeromem,
			libtysila5.target.arm.arm_Assembler.handle_ldc,
			libtysila5.target.arm.arm_Assembler.handle_ldloc,
			libtysila5.target.arm.arm_Assembler.handle_stloc,
			libtysila5.target.arm.arm_Assembler.handle_rem,
			libtysila5.target.arm.arm_Assembler.handle_ldarg,
			libtysila5.target.arm.arm_Assembler.handle_starg,
			libtysila5.target.arm.arm_Assembler.handle_ldarga,
			libtysila5.target.arm.arm_Assembler.handle_stackcopy,
			libtysila5.target.arm.arm_Assembler.handle_localloc,
			libtysila5.target.arm.arm_Assembler.handle_shr,
			libtysila5.target.arm.arm_Assembler.handle_shl,
			libtysila5.target.arm.arm_Assembler.handle_shr_un,
			libtysila5.target.arm.arm_Assembler.handle_switch,
			libtysila5.target.arm.arm_Assembler.handle_ldobja,
			libtysila5.target.arm.arm_Assembler.handle_cctor_runonce,
			libtysila5.target.arm.arm_Assembler.handle_break,
			libtysila5.target.arm.arm_Assembler.handle_mclabel,
			libtysila5.target.arm.arm_Assembler.handle_memcpy,
			libtysila5.target.arm.arm_Assembler.handle_memset,
			libtysila5.target.arm.arm_Assembler.handle_syncvalcompareandswap,
			libtysila5.target.arm.arm_Assembler.handle_syncvalswap,
			libtysila5.target.arm.arm_Assembler.handle_spinlockhint,
			libtysila5.target.arm.arm_Assembler.handle_target_specific,
		};
		
		int arm_instrs_start = 236;
	}
}

