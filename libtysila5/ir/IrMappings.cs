/* D:\tysila\libtysila5\ir\IrMappings.cs
 * This is an auto-generated file
 * DO NOT EDIT
 * It was generated at 14:03:29 on 14 March 2021
 * from libtysila5/ir/IrMappings.td
 * by TableMap (part of tysos: http://www.tysos.org)
 * Please edit the source file, rather than this file, to make any changes
 */

namespace libtysila5.ir
{
	partial class IrGraph
	{
		static void init_simple_map()
		{
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldc, cil.Opcode.SimpleOpcode.stloc }] = ld_st;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldc, cil.Opcode.SimpleOpcode.starg }] = ld_st;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldloc, cil.Opcode.SimpleOpcode.stloc }] = ld_st;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldloc, cil.Opcode.SimpleOpcode.starg }] = ld_st;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldarg, cil.Opcode.SimpleOpcode.stloc }] = ld_st;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldarg, cil.Opcode.SimpleOpcode.starg }] = ld_st;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldc, cil.Opcode.SimpleOpcode.ret }] = ld_ret;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldloc, cil.Opcode.SimpleOpcode.ret }] = ld_ret;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldarg, cil.Opcode.SimpleOpcode.ret }] = ld_ret;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldc }] = st_lv_st;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldloc }] = st_lv_st;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldarg }] = st_lv_st;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.stloc }] = st_st_lv;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.starg }] = st_st_lv;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldloca }] = ldloca;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.nop }] = nop;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.call }] = call;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.call, cil.Opcode.SimpleOpcode.stloc }] = call_stloc;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.call, cil.Opcode.SimpleOpcode.starg }] = call_stloc;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ret }] = ret;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldc, cil.Opcode.SimpleOpcode.ret }] = ld_ret;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldloc, cil.Opcode.SimpleOpcode.ret }] = ld_ret;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldarg, cil.Opcode.SimpleOpcode.ret }] = ld_ret;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldc, cil.Opcode.SimpleOpcode.ldc, cil.Opcode.SimpleOpcode.cmp }] = ld_ld_cmp;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldc, cil.Opcode.SimpleOpcode.ldloc, cil.Opcode.SimpleOpcode.cmp }] = ld_ld_cmp;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldc, cil.Opcode.SimpleOpcode.ldarg, cil.Opcode.SimpleOpcode.cmp }] = ld_ld_cmp;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldloc, cil.Opcode.SimpleOpcode.ldc, cil.Opcode.SimpleOpcode.cmp }] = ld_ld_cmp;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldloc, cil.Opcode.SimpleOpcode.ldloc, cil.Opcode.SimpleOpcode.cmp }] = ld_ld_cmp;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldloc, cil.Opcode.SimpleOpcode.ldarg, cil.Opcode.SimpleOpcode.cmp }] = ld_ld_cmp;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldarg, cil.Opcode.SimpleOpcode.ldc, cil.Opcode.SimpleOpcode.cmp }] = ld_ld_cmp;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldarg, cil.Opcode.SimpleOpcode.ldloc, cil.Opcode.SimpleOpcode.cmp }] = ld_ld_cmp;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldarg, cil.Opcode.SimpleOpcode.ldarg, cil.Opcode.SimpleOpcode.cmp }] = ld_ld_cmp;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldc, cil.Opcode.SimpleOpcode.cmp }] = ld_cmp;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldloc, cil.Opcode.SimpleOpcode.cmp }] = ld_cmp;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldarg, cil.Opcode.SimpleOpcode.cmp }] = ld_cmp;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.cmp }] = cmp;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.brif1 }] = brif1;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.brif2 }] = brif2;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.br }] = br;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.cmp, cil.Opcode.SimpleOpcode.ldc, cil.Opcode.SimpleOpcode.cmp }] = cmp_ldc_cmp;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.binnumop }] = binnumops;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.conv }] = conv;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.stind }] = stind;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldstr }] = ldstr;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldfld }] = ldfld;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.stfld }] = stfld;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.newobj }] = newobj;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.dup }] = dup;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.pop }] = pop;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.initobj }] = initobj;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.castclass }] = castclass;
			simple_map[new cil.Opcode.SimpleOpcode[] { cil.Opcode.SimpleOpcode.ldtoken }] = ldtoken;
		}
	}
}

namespace libtysila5.ir
{
	partial class IrGraph
	{
		static void init_single_map()
		{
			single_map[new cil.Opcode.SingleOpcodes[] { cil.Opcode.SingleOpcodes.brtrue }] = brtrue;
			single_map[new cil.Opcode.SingleOpcodes[] { cil.Opcode.SingleOpcodes.brtrue_s }] = brtrue;
			single_map[new cil.Opcode.SingleOpcodes[] { cil.Opcode.SingleOpcodes.brfalse }] = brfalse;
			single_map[new cil.Opcode.SingleOpcodes[] { cil.Opcode.SingleOpcodes.brfalse_s }] = brfalse;
		}
	}
}

