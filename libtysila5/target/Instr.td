next_instr_id = 0;
export next_instr_id;

vl_st_lv_arg_32 = [ vl_stack32, vl_lv32, vl_arg32, vl_c32 ];
export vl_st_lv_arg_32;

intmap("libtysila5.target", "Target", "pt", "pt_names", "init_pt", [
	"def",
	"use",
	"cc",
	"icc",
	"br",
	"mc",
	"tu",
	"td",
]);

export function _multiinstr(array usedef, array cur_try, array ret, int depth)
{
	foreach(entry in usedef[depth])
	{
		// clone cur_try
		ctc = [];
		foreach(ct in cur_try)
		{
			ctc.add(ct);
		}
		ctc.add(entry);

		if(depth == usedef.length - 1)
		{
			// end of depth - add
			ret.add(ctc);
		}
		else
		{
			_multiinstr(usedef, ctc, ret, depth + 1);
		}
	}
}

export function multiinstr(string target, int ir_node, array uses, array defs, array impl,
	array use_reg_mask, array def_reg_mask, int clobber_mask)
{
	// call instr for any combination of uses and defs
	usecount = uses.length;
	defcount = defs.length;
	
	usedefs = [];
	foreach(use in uses)
	{
		usedefs.add(use);
	}
	foreach(def in defs)
	{
		usedefs.add(def);
	}

	multi = [];
	_multiinstr(usedefs, [], multi, 0);

	foreach(m in multi)
	{
		// rebuild use/defs
		cur_uses = [];
		cur_defs = [];
		for(i = 0; i < usecount; i = i + 1)
		{
			cur_uses.add(m[i]);
		}
		for(i = 0; i < defcount; i = i + 1)
		{
			idx = usecount + i;
			cur_defs.add(m[idx]);
		}

		instr(target, ir_node, cur_uses, cur_defs, impl, use_reg_mask, def_reg_mask, clobber_mask);
	}
}

export function instr(string target, int ir_node, array uses, array defs, array impl,
	array use_reg_mask, array def_reg_mask, int clobber_mask)
{
	// build a key from ir_node, uses_count, uses, defs_count and defs
	key = [];
	key.add(ir_node);
	key.add(uses.length);
	foreach(use in uses)
	{
		key.add(use);
	}
	key.add(defs.length);
	foreach(def in defs)
	{
		key.add(def);
	}

	/* build a value from impl:
		impl_count
			impl1:
				p_count
					p_1: type, val_or_id
					p_2: ..
					p_n:
			impl2:
				...
			impln:
		reg_uses_count
			reg_mask_for_use1
			...
		reg_defs_count
			reg_mask_for_def1
			...
		reg_mask_for_clobber
	*/

	value = [];
	value.add(impl.length);
	foreach(imp in impl)
	{
		value.add(imp.length);
		foreach(p in imp)
		{
			if(p[0] == "$")
			{
				if(p[1] == "d")
				{
					value.add(varget("pt_def"));
					value.add(toint(p.substring(2)));
				}
				else if(p[1] == "u")
				{
					value.add(varget("pt_use"));
					value.add(toint(p.substring(2)));
				}
				else if(p.substring(1) == "cc")
				{
					value.add(varget("pt_cc"));
					value.add(0);
				}
				else if(p.substring(1) == "icc")
				{
					value.add(varget("pt_icc"));
					value.add(0);
				}
				else if(p[1] == "b" && p[2] == "r")
				{
					value.add(varget("pt_br"));
					value.add(toint(p.substring(3)));
				}
				else if(p.substring(1,2) == "tu")
				{
					value.add(varget("pt_tu"));
					value.add(toint(p.substring(3)));
				}
				else if(p.substring(1,2) == "td")
				{
					value.add(varget("pt_td"));
					value.add(toint(p.substring(3)));
				}			
			}
			else
			{
				value.add(varget("pt_mc"));
				value.add(varget(p));
			}
		}
	}
	value.add(use_reg_mask.length);
	foreach(use_reg_item in use_reg_mask)
	{
		value.add(use_reg_item);
	}
	value.add(def_reg_mask.length);
	foreach(def_reg_item in def_reg_mask)
	{
		value.add(def_reg_item);
	}
	value.add(clobber_mask);

	defblob(target, key, value);
}

export function instr(string name, string ir_node, array uses, array defs, array impl)
{
	ret = [ name = name, id = next_instr_id, 
		ir_node = ir_node, uses = uses, defs = defs, impl = impl ];
	next_instr_id = next_instr_id + 1;
	export next_instr_id;
	return ret;
}

export function pi_f(array ca_type, object instr)
{
	uses_count = ca_type[0];

	uses = [];
	for(i = 0; i < uses_count; i = i + 1)
	{
		uses.add(ca_type[i + 1]);
	}

	defs = [];
	defs_count = ca_type.length - 1;
	defs_count = defs_count - uses_count;
	for(i = 0; i < defs_count; i = i + 1)
	{
		defs.add(ca_type[i + 1 + uses_count]);
	}

	ret = [ name = instr.name,
		ir_node = instr.ir_node,
		id = next_instr_id,
		uses = uses,
		defs = defs,
		impl = instr.impl
	];
	next_instr_id = next_instr_id + 1;
	export next_instr_id;

	return ret;
}

export function parse_instrs(array instrs)
{
	ret = [];

	foreach(i in instrs)
	{
		/* build a composite array containing:
			[ use_count, use1, use2, ..., use n, def1, def2, ..., defn ]
		*/

		ca = [ [ i.uses.length ] ];
		for(u = 0; u < i.uses.length; u = u + 1)
		{
			ca.add(i.uses[u]);
		}
		for(d = 0; d < i.defs.length; d = d + 1)
		{
			ca.add(i.defs[d]);
		}

		nm = _add_mapping(ca, i, funcref pi_f(array, object), [], 0);
		ret.addrange(nm);
	}

	return ret;
}

