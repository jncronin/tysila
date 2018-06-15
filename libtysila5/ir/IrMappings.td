// This is a table map file

include "Maps.td";

ldops = [ "ldc", "ldloc", "ldarg" ];
stops = [ "stloc", "starg" ];

function simple_opcode_handler(array entries, string val)
{
	print("\t\t\tsimple_map[new cil.Opcode.SimpleOpcode[] { ");
	for(i = 0; i < entries.length; i = i + 1)
	{
		if(i != 0)
		{
			print(", ");
		}
		print("cil.Opcode.SimpleOpcode.");
		print(entries[i]);
	}
	print(" }] = ");
	print(val);
	print(";\n");
}

function single_opcode_handler(array entries, string val)
{
	print("\t\t\tsingle_map[new cil.Opcode.SingleOpcodes[] { ");
	for(i = 0; i < entries.length; i = i + 1)
	{
		if(i != 0)
		{
			print(", ");
		}
		print("cil.Opcode.SingleOpcodes.");
		print(entries[i]);
	}
	print(" }] = ");
	print(val);
	print(";\n");
}

multimap("libtysila5.ir", "IrGraph", "simple_map", "init_simple_map", [
	[ ldops, stops, "ld_st" ],
	[ ldops, [ "ret" ], "ld_ret" ],
	[ ldops, "st_lv_st" ],
	[ stops, "st_st_lv" ],
	[ [ "ldloca" ], "ldloca" ],
	[ [ "nop" ], "nop" ],
	[ [ "call" ], "call" ],
	[ [ "call" ], stops, "call_stloc" ],
	[ [ "ret" ], "ret" ],
	[ ldops, [ "ret" ], "ld_ret" ],
	[ ldops, ldops, [ "cmp" ], "ld_ld_cmp" ],
	[ ldops, [ "cmp" ], "ld_cmp" ],
	[ [ "cmp" ], "cmp" ],
	[ [ "brif1" ], "brif1" ],
	[ [ "brif2" ], "brif2" ],
	[ [ "br" ], "br" ],
	[ [ "cmp" ], [ "ldc" ], [ "cmp" ], "cmp_ldc_cmp" ],
	[ [ "binnumop" ], "binnumops" ],
	[ [ "conv" ] , "conv" ],
	[ [ "stind" ], "stind" ],
	[ [ "ldstr" ], "ldstr" ],
	[ [ "ldfld" ], "ldfld" ],
	[ [ "stfld" ], "stfld" ],
	[ [ "newobj" ], "newobj" ],
	[ [ "dup" ], "dup" ],
	[ [ "pop" ], "pop" ],
	[ [ "initobj" ], "initobj" ],
	[ [ "castclass" ], "castclass" ],
	[ [ "ldtoken" ], "ldtoken" ],
], funcref simple_opcode_handler(array, string));

multimap("libtysila5.ir", "IrGraph", "single_map", "init_single_map", [
	[ [ "brtrue", "brtrue_s" ], "brtrue" ],
	[ [ "brfalse", "brfalse_s" ], "brfalse" ]
], funcref single_opcode_handler(array, string));
