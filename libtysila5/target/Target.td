/* Output machine definition objects */

target_defs = [];
export target_defs;

include "Maps.td";
include "Trie.td";
include "Register.td";
include "Instr.td";
include "cc.td";

include "TargetInit.td";

// Some generic instructions
intmap("libtysila5.target", "Generic", "g", "insts", "init_instrs", [
	"phi",
	"precall",
	"postcall",
	"setupstack",
	"savecalleepreserves",
	"restorecalleepreserves",
	"loadaddress",
	"mclabel",
	"label",
]);

// Include machine specific definitions here
include "x86/x86.td";



/* Dump a target definition */
function dump_target(object target_def)
{
	n = target_def.name;
	ns = target_def.nspace;

	foreach(cc in target_def.cc)
	{
		if(cc[0] != "default")
		{
			print("\t\t\t" + ns + "." + n + "_Assembler.init_" + cc[0] + "();\n");
		}
	}
	foreach(cc in target_def.retcc)
	{
		if(cc[0] != "ret_default")
		{
			print("\t\t\t" + ns + "." + n + "_Assembler.init_" + cc[0] + "();\n");
		}
	}
	print("\t\t\tvar " + n + " = new " + ns + "." +
		n + "_Assembler();\n");
	print("\t\t\t" + n + ".name = \"" + n + "\";\n");
	print("\t\t\t" + n + ".ptype = ir.Opcode.ct_" + target_def.ptr_type + ";\n");

	/* registers */
	// first, get max register id
	max_id = -1;
	foreach(reg in target_def.regs)
	{
		if(reg.id > max_id)
		{
			max_id = reg.id;
			export max_id;	
		}
	}
	max_id = max_id + 1;
	print("\t\t\t" + target_def.nspace + "." + target_def.assembler + ".registers = new Target.Reg[" + max_id + "];\n");
	print("\t\t\t" + n + ".regs = " + target_def.nspace + "." + target_def.assembler + ".registers;\n");
	foreach(reg in target_def.regs)
	{
		print("\t\t\t" + n + ".regs[" + reg.id + "] = new Target.Reg { ");
		print("name = \"" + reg.name + "\", id = " + reg.id + ", ");
		print("type = " + reg.type + ", size = " + reg.size + ", ");
		print("mask = " + reg.mask + " };\n");

		print("\t\t\t" + target_def.nspace + "." + target_def.assembler + ".r_" + reg.name + " = " + n + ".regs[" + reg.id + "];\n");
	}

	print("\t\t\ttargets[\"" + n + "\"] = " + n + ";\n");
}

print("namespace libtysila5.target\n");
print("{\n");
print("\tpublic partial class Generic\n");
print("\t{\n");
print("\t\tpublic static System.Collections.Generic.Dictionary<int, string> insts = new System.Collections.Generic.Dictionary<int, string>(new libtysila5.GenericEqualityComparer<int>());\n");
print("\t}\n");
print("\t\n");
print("\tpublic partial class Target\n");
print("\t{\n");
print("\t\tpublic static void init_targets()\n");
print("\t\t{\n");
print("\t\t\tlibtysila5.target.Generic.init_instrs();\n");
print("\t\t\t\n");
foreach(target_def in target_defs)
{
	dump_target(target_def);
}
print("\t\t}\n");
foreach(target_def in target_defs)
{

}
print("\t}\n");
print("}\n");
print("\n");

/* define register names so they can be used as target.r_etc */
foreach(target_def in target_defs)
{
	n = target_def.name;

	print("namespace " + target_def.nspace + "\n");
	print("{\n");
	print("\tpublic partial class " + target_def.assembler + "\n");
	print("\t{\n");

	print("\t\tpublic static Target.Reg[] registers;\n");
	foreach(reg in target_def.regs)
	{
		print("\t\tpublic static Target.Reg r_" + reg.name + ";\n");
	}


	/* Add an init_cc function */
	print("\t\t\n");
	print("\t\tvoid init_ccs()\n");
	print("\t\t{\n");
	print("\t\t\tinit_cc_callee_preserves_map();\n");
	print("\t\t\tinit_cc_caller_preserves_map();\n");
	print("\t\t\tinit_cc_map();\n");
	print("\t\t\tinit_retcc_map();\n");
	print("\t\t\tinit_cc_classmap();\n");
	print("\t\t\tinit_retcc_classmap();\n");
	print("\t\t}\n");

	/* Add constructor */
	print("\t\t\n");
	print("\t\tinternal " + target_def.assembler + "()\n");
	print("\t\t{\n");
	print("\t\t\tinit_ccs();\n");
	print("\t\t\tinit_options();\n");

	/* Registers used for allocating to various ct types */
	foreach(cttype in target_def.ct_map)
	{
		x = 0;

		foreach(ctdest in cttype[1])
		{
			x = x | ctdest.mask;
		}

		print("\t\t\tct_regs[" + cttype[0] + "] = " + x + ";\n");
	}
	if(target_def.ptr_type == "int32")
	{
		ct_ptr_type = ct_int32;
	}
	else
	{
		ct_ptr_type = ct_int64;
	}
	print("\t\t\tct_regs[" + ct_intptr + "] = ct_regs[" + ct_ptr_type + "];\n");
	print("\t\t\tct_regs[" + ct_object + "] = ct_regs[" + ct_ptr_type + "];\n");
	print("\t\t\tct_regs[" + ct_ref + "] = ct_regs[" + ct_ptr_type + "];\n");

	print("\t\t\tinstrs.trie = " + n + "_instrs;\n");
	print("\t\t\tinstrs.start = " + n + "_instrs_start;\n");
	print("\t\t\tinstrs.vals = " + n + "_instrs_vals;\n");

	print("\t\t\tpsize = " + target_def.ptr_size + ";\n");

	print("\t\t}\n");
	print("\t\t\n");

	trie_export(target_def.instrs, target_def.name + "_instrs",
		"InstructionHandler", "");

	print("\t}\n");
	print("}\n");
	print("\n");
}