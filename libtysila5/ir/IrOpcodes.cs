/* D:\tysos\libtysila5\ir\IrOpcodes.cs
 * This is an auto-generated file
 * DO NOT EDIT
 * It was generated at 17:41:31 on 27 March 2018
 * from D:\tysos\libtysila5\ir\IrOpcodes.td
 * by TableMap (part of tysos: http://www.tysos.org)
 * Please edit the source file, rather than this file, to make any changes
 */

namespace libtysila5.ir
{
	partial class Opcode
	{
		public const int vl_stack32 = 0;
		public const int vl_lv32 = 1;
		public const int vl_arg32 = 2;
		public const int vl_stack64 = 3;
		public const int vl_lv64 = 4;
		public const int vl_arg64 = 5;
		public const int vl_c32 = 6;
		public const int vl_c64 = 7;
		public const int vl_stack = 8;
		public const int vl_lv = 9;
		public const int vl_arg = 10;
		public const int vl_c = 11;
		public const int vl_call_target = 12;
		public const int vl_br_target = 13;
		public const int vl_cc = 14;
		public const int vl_str = 15;
		public const int vl_void = 16;
		public const int vl_mreg = 17;
		public const int vl_ts_token = 18;
		
		internal static void init_vl()
		{
			vl_names[0] = "stack32";
			vl_names[1] = "lv32";
			vl_names[2] = "arg32";
			vl_names[3] = "stack64";
			vl_names[4] = "lv64";
			vl_names[5] = "arg64";
			vl_names[6] = "c32";
			vl_names[7] = "c64";
			vl_names[8] = "stack";
			vl_names[9] = "lv";
			vl_names[10] = "arg";
			vl_names[11] = "c";
			vl_names[12] = "call_target";
			vl_names[13] = "br_target";
			vl_names[14] = "cc";
			vl_names[15] = "str";
			vl_names[16] = "void";
			vl_names[17] = "mreg";
			vl_names[18] = "ts_token";
		}
	}
}

namespace libtysila5.ir
{
	partial class Opcode
	{
		public const int oc_null = 19;
		public const int oc_store = 20;
		public const int oc_add = 21;
		public const int oc_sub = 22;
		public const int oc_mul = 23;
		public const int oc_div = 24;
		public const int oc_and = 25;
		public const int oc_or = 26;
		public const int oc_xor = 27;
		public const int oc_not = 28;
		public const int oc_neg = 29;
		public const int oc_call = 30;
		public const int oc_callvirt = 31;
		public const int oc_calli = 32;
		public const int oc_nop = 33;
		public const int oc_ret = 34;
		public const int oc_cmp = 35;
		public const int oc_br = 36;
		public const int oc_brif = 37;
		public const int oc_enter = 38;
		public const int oc_enter_handler = 39;
		public const int oc_conv = 40;
		public const int oc_stind = 41;
		public const int oc_ldind = 42;
		public const int oc_ldindzb = 43;
		public const int oc_ldindzw = 44;
		public const int oc_ldstr = 45;
		public const int oc_ldlabcontents = 46;
		public const int oc_ldlabaddr = 47;
		public const int oc_stlabcontents = 48;
		public const int oc_ldfp = 49;
		public const int oc_ldloca = 50;
		public const int oc_zeromem = 51;
		public const int oc_swap = 52;
		public const int oc_pop = 53;
		public const int oc_phi = 54;
		public const int oc_castclass = 55;
		public const int oc_isinst = 56;
		public const int oc_endfinally = 57;
		public const int oc_ldc = 58;
		public const int oc_ldloc = 59;
		public const int oc_stloc = 60;
		public const int oc_rem = 61;
		public const int oc_ldarg = 62;
		public const int oc_starg = 63;
		public const int oc_ldarga = 64;
		public const int oc_stackcopy = 65;
		public const int oc_localloc = 66;
		public const int oc_shr = 67;
		public const int oc_shl = 68;
		public const int oc_shr_un = 69;
		public const int oc_switch = 70;
		public const int oc_ldobja = 71;
		public const int oc_cctor_runonce = 72;
		public const int oc_break = 73;
		public const int oc_mclabel = 74;
		public const int oc_memcpy = 75;
		public const int oc_memset = 76;
		public const int oc_syncvalcompareandswap = 77;
		public const int oc_syncvalswap = 78;
		public const int oc_spinlockhint = 79;
		public const int oc_target_specific = 80;
		public const int oc_x86_portin = 81;
		public const int oc_x86_portout = 82;
		
		internal static void init_oc()
		{
			oc_names[19] = "null";
			oc_names[20] = "store";
			oc_names[21] = "add";
			oc_names[22] = "sub";
			oc_names[23] = "mul";
			oc_names[24] = "div";
			oc_names[25] = "and";
			oc_names[26] = "or";
			oc_names[27] = "xor";
			oc_names[28] = "not";
			oc_names[29] = "neg";
			oc_names[30] = "call";
			oc_names[31] = "callvirt";
			oc_names[32] = "calli";
			oc_names[33] = "nop";
			oc_names[34] = "ret";
			oc_names[35] = "cmp";
			oc_names[36] = "br";
			oc_names[37] = "brif";
			oc_names[38] = "enter";
			oc_names[39] = "enter_handler";
			oc_names[40] = "conv";
			oc_names[41] = "stind";
			oc_names[42] = "ldind";
			oc_names[43] = "ldindzb";
			oc_names[44] = "ldindzw";
			oc_names[45] = "ldstr";
			oc_names[46] = "ldlabcontents";
			oc_names[47] = "ldlabaddr";
			oc_names[48] = "stlabcontents";
			oc_names[49] = "ldfp";
			oc_names[50] = "ldloca";
			oc_names[51] = "zeromem";
			oc_names[52] = "swap";
			oc_names[53] = "pop";
			oc_names[54] = "phi";
			oc_names[55] = "castclass";
			oc_names[56] = "isinst";
			oc_names[57] = "endfinally";
			oc_names[58] = "ldc";
			oc_names[59] = "ldloc";
			oc_names[60] = "stloc";
			oc_names[61] = "rem";
			oc_names[62] = "ldarg";
			oc_names[63] = "starg";
			oc_names[64] = "ldarga";
			oc_names[65] = "stackcopy";
			oc_names[66] = "localloc";
			oc_names[67] = "shr";
			oc_names[68] = "shl";
			oc_names[69] = "shr_un";
			oc_names[70] = "switch";
			oc_names[71] = "ldobja";
			oc_names[72] = "cctor_runonce";
			oc_names[73] = "break";
			oc_names[74] = "mclabel";
			oc_names[75] = "memcpy";
			oc_names[76] = "memset";
			oc_names[77] = "syncvalcompareandswap";
			oc_names[78] = "syncvalswap";
			oc_names[79] = "spinlockhint";
			oc_names[80] = "target_specific";
			oc_names[81] = "x86_portin";
			oc_names[82] = "x86_portout";
		}
	}
}

namespace libtysila5.ir
{
	partial class Opcode
	{
		public const int cc_always = 83;
		public const int cc_never = 84;
		public const int cc_eq = 85;
		public const int cc_ne = 86;
		public const int cc_gt = 87;
		public const int cc_ge = 88;
		public const int cc_lt = 89;
		public const int cc_le = 90;
		public const int cc_a = 91;
		public const int cc_ae = 92;
		public const int cc_b = 93;
		public const int cc_be = 94;
		
		internal static void init_cc()
		{
			cc_names[83] = "always";
			cc_names[84] = "never";
			cc_names[85] = "eq";
			cc_names[86] = "ne";
			cc_names[87] = "gt";
			cc_names[88] = "ge";
			cc_names[89] = "lt";
			cc_names[90] = "le";
			cc_names[91] = "a";
			cc_names[92] = "ae";
			cc_names[93] = "b";
			cc_names[94] = "be";
		}
	}
}

namespace libtysila5.ir
{
	partial class Opcode
	{
		static void init_cc_invert()
		{
			cc_invert_map[cc_always] = cc_never;
			cc_invert_map[cc_never] = cc_always;
			cc_invert_map[cc_eq] = cc_ne;
			cc_invert_map[cc_ne] = cc_eq;
			cc_invert_map[cc_gt] = cc_le;
			cc_invert_map[cc_ge] = cc_lt;
			cc_invert_map[cc_lt] = cc_ge;
			cc_invert_map[cc_le] = cc_gt;
			cc_invert_map[cc_a] = cc_be;
			cc_invert_map[cc_ae] = cc_b;
			cc_invert_map[cc_b] = cc_ae;
			cc_invert_map[cc_be] = cc_a;
		}
	}
}

namespace libtysila5.ir
{
	partial class Opcode
	{
		public const int ct_unknown = 95;
		public const int ct_int32 = 96;
		public const int ct_int64 = 97;
		public const int ct_intptr = 98;
		public const int ct_tls_int32 = 99;
		public const int ct_tls_int64 = 100;
		public const int ct_tls_intptr = 101;
		public const int ct_float = 102;
		public const int ct_object = 103;
		public const int ct_ref = 104;
		public const int ct_vt = 105;
		
		internal static void init_ct()
		{
			ct_names[95] = "unknown";
			ct_names[96] = "int32";
			ct_names[97] = "int64";
			ct_names[98] = "intptr";
			ct_names[99] = "tls_int32";
			ct_names[100] = "tls_int64";
			ct_names[101] = "tls_intptr";
			ct_names[102] = "float";
			ct_names[103] = "object";
			ct_names[104] = "ref";
			ct_names[105] = "vt";
		}
	}
}

namespace libtysila5.ir
{
	partial class Opcode
	{
		static void init_cc_single_map()
		{
			cc_single_map[cil.Opcode.SingleOpcodes.beq] = cc_eq;
			cc_single_map[cil.Opcode.SingleOpcodes.bge] = cc_ge;
			cc_single_map[cil.Opcode.SingleOpcodes.bge_un] = cc_ae;
			cc_single_map[cil.Opcode.SingleOpcodes.bgt] = cc_gt;
			cc_single_map[cil.Opcode.SingleOpcodes.bgt_un] = cc_a;
			cc_single_map[cil.Opcode.SingleOpcodes.ble] = cc_le;
			cc_single_map[cil.Opcode.SingleOpcodes.ble_un] = cc_be;
			cc_single_map[cil.Opcode.SingleOpcodes.blt] = cc_lt;
			cc_single_map[cil.Opcode.SingleOpcodes.blt_un] = cc_b;
			cc_single_map[cil.Opcode.SingleOpcodes.bne_un] = cc_ne;
			cc_single_map[cil.Opcode.SingleOpcodes.brfalse] = cc_eq;
			cc_single_map[cil.Opcode.SingleOpcodes.brtrue] = cc_ne;
			cc_single_map[cil.Opcode.SingleOpcodes.beq_s] = cc_eq;
			cc_single_map[cil.Opcode.SingleOpcodes.bge_s] = cc_ge;
			cc_single_map[cil.Opcode.SingleOpcodes.bge_un_s] = cc_ae;
			cc_single_map[cil.Opcode.SingleOpcodes.bgt_s] = cc_gt;
			cc_single_map[cil.Opcode.SingleOpcodes.bgt_un_s] = cc_a;
			cc_single_map[cil.Opcode.SingleOpcodes.ble_s] = cc_le;
			cc_single_map[cil.Opcode.SingleOpcodes.ble_un_s] = cc_be;
			cc_single_map[cil.Opcode.SingleOpcodes.blt_s] = cc_lt;
			cc_single_map[cil.Opcode.SingleOpcodes.blt_un_s] = cc_b;
			cc_single_map[cil.Opcode.SingleOpcodes.bne_un_s] = cc_ne;
			cc_single_map[cil.Opcode.SingleOpcodes.brfalse_s] = cc_eq;
			cc_single_map[cil.Opcode.SingleOpcodes.brtrue_s] = cc_ne;
		}
	}
}

namespace libtysila5.ir
{
	partial class Opcode
	{
		static void init_cc_double_map()
		{
			cc_double_map[cil.Opcode.DoubleOpcodes.ceq] = cc_eq;
			cc_double_map[cil.Opcode.DoubleOpcodes.cgt] = cc_gt;
			cc_double_map[cil.Opcode.DoubleOpcodes.cgt_un] = cc_a;
			cc_double_map[cil.Opcode.DoubleOpcodes.clt] = cc_lt;
			cc_double_map[cil.Opcode.DoubleOpcodes.clt_un] = cc_b;
		}
	}
}

namespace libtysila5.ir
{
	partial class Opcode
	{
		static void init_oc_pushes_map()
		{
			oc_pushes_map[oc_call] = get_call_rettype;
			oc_pushes_map[oc_callvirt] = get_call_rettype;
			oc_pushes_map[oc_store] = get_store_pushtype;
			oc_pushes_map[oc_add] = get_binnumop_pushtype;
			oc_pushes_map[oc_sub] = get_binnumop_pushtype;
			oc_pushes_map[oc_conv] = get_conv_pushtype;
			oc_pushes_map[oc_ldstr] = get_object_pushtype;
		}
	}
}

