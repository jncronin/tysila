/* Initialize a target definition */

export function instr(object t, string oc)
{
	trie_add(t.instrs, [ varget("oc_" + oc) ], t.nspace + "." + t.assembler + ".handle_" + oc);
}
export function instr(object t, array oc, string name)
{
	trie_add(t.instrs, oc, t.nspace + "." + t.assembler + ".handle_" + name);
}

export function target_init(string name, int ptr_size, int is_msb)
{
	if(is_msb)
	{
		throw("System.NotImplementedException", "MSB targets not implemented yet");
	}

	if(ptr_size == 4)
	{
		ptr_type = "int32";
	}
	else
	{
		if(ptr_size == 8)
		{
			ptr_type = "int64";
		}
		else
		{
			throw("System.NotImplementedException", "Pointer size of " + ptr_size + " not implemented yet");
		}
	}

	tdef = [
		name = name,
		ptr_type = ptr_type,
		nspace = "libtysila5.target." + name,
		assembler = name + "_Assembler",
		regs = [ Rstack(), Rcontents() ],
		instrs = trie_gen(),
		ptr_size = ptr_size
	];

	/* Opcodes that must be implemented */
	t = tdef.instrs;
	instr(tdef, "add");
	instr(tdef, "sub");
	instr(tdef, "mul");
	instr(tdef, "div");
	instr(tdef, "and");
	instr(tdef, "or");
	instr(tdef, "xor");
	instr(tdef, "not");
	instr(tdef, "neg");
	instr(tdef, "call");
	instr(tdef, "ldc");
	instr(tdef, "stackcopy");
	instr(tdef, "stloc");
	instr(tdef, "ldloc");
	instr(tdef, "ldloca");
	instr(tdef, "ldobja");
	instr(tdef, "ldarg");
	instr(tdef, "ldarga");
	instr(tdef, "starg");
	instr(tdef, "shl");
	instr(tdef, "shr");
	instr(tdef, "shr_un");
	instr(tdef, "cmp");
	instr(tdef, "brif");
	instr(tdef, "conv");
	instr(tdef, "stind");
	instr(tdef, "ldind");
	instr(tdef, "br");
	instr(tdef, "ldlabaddr");
	instr(tdef, "calli");
	instr(tdef, "ret");
	instr(tdef, "enter");
	instr(tdef, "enter_handler");
	instr(tdef, "localloc");
	instr(tdef, "rem");
	instr(tdef, "switch");
	instr(tdef, "zeromem");
	instr(tdef, "cctor_runonce");
	instr(tdef, "ldfp");
	instr(tdef, "break");
	instr(tdef, "mclabel");
	instr(tdef, "memcpy");
	instr(tdef, "memset");
	instr(tdef, "syncvalcompareandswap");
	instr(tdef, "syncvalswap");
	instr(tdef, "syncvalexchangeandadd");
	instr(tdef, "spinlockhint");
	instr(tdef, "target_specific");
	instr(tdef, "nop");

	target_defs.add(tdef);

	return tdef;
}
