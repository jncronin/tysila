next_reg_id = 0;
export next_reg_id;

export function ResetRegId()
{
	next_reg_id = 0;
	export next_reg_id;
}

intmap("libtysila5.target", "Target", "rt", "rt_map", "init_rtmap", [
	"gpr",
	"float",
	"stack",
	"contents",
	"multi"
]);

export function Ri32(string name)
{
	ret = [ name = name, id = next_reg_id, type = rt_gpr, size = 4, mask = 1 << next_reg_id ];
	next_reg_id = next_reg_id + 1;
	export next_reg_id;
	vargen(name, ret);
	vargen(name + "reg", ret.mask);
	
	return ret;
}

export function Rf32(string name)
{
	ret = [ name = name, id = next_reg_id, type = rt_float, size = 4, mask = 1 << next_reg_id ];
	next_reg_id = next_reg_id + 1;
	export next_reg_id;
	vargen(name, ret);
	vargen(name + "reg", ret.mask);
	
	return ret;
}

export function MultiReg(string name, array regs)
{
	mask = 0;
	foreach(reg in regs)
	{
		mask = mask | reg.mask;
		export mask;
	}

	ret = [ name = name, id = next_reg_id, type = rt_multi, size = regs[0].size, mask = mask ];
	next_reg_id = next_reg_id + 1;
	export next_reg_id;
	vargen(name, ret);
	vargen(name + "reg", ret.mask);
	
	return ret;

}

export function Ri64(string name)
{
	ret = [ name = name, id = next_reg_id, type = rt_gpr, size = 8, mask = 1 << next_reg_id ];
	next_reg_id = next_reg_id + 1;
	export next_reg_id;
	vargen(name, ret);
	vargen(name + "reg", ret.mask);
	
	return ret;
}

export function Rf64(string name)
{
	ret = [ name = name, id = next_reg_id, type = rt_float, size = 8, mask = 1 << next_reg_id ];
	next_reg_id = next_reg_id + 1;
	export next_reg_id;
	vargen(name, ret);
	vargen(name + "reg", ret.mask);
	
	return ret;
}

export function Rstack()
{
	ret = [ name = "stack", id = next_reg_id, type = rt_stack, size = 0, mask = 1 << next_reg_id ];
	next_reg_id = next_reg_id + 1;
	export next_reg_id;
	vargen(ret.name, ret);
	vargen(ret.name + "reg", ret.mask);

	return ret;
}

export function Rcontents()
{
	ret = [ name = "contents", id = next_reg_id, type = rt_contents, size = 0, mask = 1 << next_reg_id ];
	next_reg_id = next_reg_id + 1;
	export next_reg_id;
	vargen(ret.name, ret);
	vargen(ret.name + "reg", ret.mask);

	return ret;
}

export function Rcontents(string orig, int offset, int size, string new)
{
	orig_val = varget("orig");
	ret = [ name = new, id = next_reg_id, type = rt_contents, size = size, offset = offset, mask = 0 ];
	next_reg_id = next_reg_id + 1;
	export next_reg_id;
	vargen(ret.name, ret);
	vargen(ret.name + "reg", reg.mask);

	return ret;
}

export function Ralias(string orig, string new)
{
	orig_val = varget(orig);
	ret = [
		name = new,
		id = orig_val.id,
		type = orig_val.type,
		size = orig_val.size,
		mask = orig_val.mask
	];
	vargen(ret.name, ret);
	vargen(ret.name + "reg", ret.mask);

	return ret;
}

export function Ralias(string orig, string new, int new_size)
{
	orig_val = varget(orig);
	ret = [
		name = new,
		id = orig_val.id,
		type = orig_val.type,
		size = new_size,
		mask = orig_val.mask
	];
	vargen(ret.name, ret);
	vargen(ret.name + "reg", ret.mask);

	return ret;
}

