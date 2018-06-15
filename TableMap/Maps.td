next_intmap_id = 0;
export next_intmap_id;

/* Generate a list of public const ints within a class, with an associated reverse
	map in a Dictionary<int, string> */
export function intmap(string namespace, string class, string name, string map_name, string init_func, array entries)
{
	print("namespace " + namespace + "\n");
	print("{\n");
	print("\tpartial class " + class + "\n");
	print("\t{\n");

	old_intmap_id = next_intmap_id;
	
	foreach(entry in entries)
	{
		print("\t\tpublic const int " + name + "_" + entry + " = " + next_intmap_id + ";\n");
		vargen(name + "_" + entry, next_intmap_id);
		next_intmap_id = next_intmap_id + 1;
		export next_intmap_id;
	}
	print("\t\t\n");

	print("\t\tinternal static void " + init_func + "()\n");
	print("\t\t{\n");
	foreach(entry in entries)
	{
		print("\t\t\t" + map_name + "[" + old_intmap_id + "] = \"" + entry + "\";\n");
		old_intmap_id = old_intmap_id + 1;
		export old_intmap_id;
	}
	print("\t\t}\n");

	print("\t}\n");
	print("}\n");
	print("\n");

	export next_intmap_id;
	return next_intmap_id - 1;
}

/* Generate a simple list of constants, i.e. like intmap but without the dictionary */
export function intlist(string namespace, string class, string name, array entries)
{
	print("namespace " + namespace + "\n");
	print("{\n");
	print("\tpartial class " + class + "\n");
	print("\t{\n");

	old_intmap_id = next_intmap_id;
	
	foreach(entry in entries)
	{
		print("\t\tpublic const int " + name + "_" + entry + " = " + next_intmap_id + ";\n");
		vargen(name + "_" + entry, next_intmap_id);
		next_intmap_id = next_intmap_id + 1;
		export next_intmap_id;
	}

	print("\t}\n");
	print("}\n");
	print("\n");

	export next_intmap_id;
	return next_intmap_id - 1;
}


/* Generate a simple hash map from index to item (int to int) */
export function simple_map(string namespace, string class, string map_name, string init_func, array entries,
	int do_string, string to_type)
{
	print("namespace " + namespace + "\n");
	print("{\n");
	print("\tpartial class " + class + "\n");
	print("\t{\n");

	if(to_type != "")
	{
		if(do_string == 1)
		{
			print("\t\tpublic System.Collections.Generic.Dictionary<string, " + to_type + "> " + map_name + " = ");
			print("new System.Collections.Generic.Dictionary<string, " + to_type + ">");
			print("(new libtysila5.GenericEqualityComparer<string>());\n");
		}
		else
		{
			print("\t\tpublic System.Collections.Generic.Dictionary<int, " + to_type + "> " + map_name + " = ");
			print("new System.Collections.Generic.Dictionary<int, " + to_type + ">");
			print("(new libtysila5.GenericEqualityComparer<int>());\n");
		}
	}

	print("\t\tinternal void " + init_func + "()\n");
	print("\t\t{\n");

	foreach(entry in entries)
	{
		print("\t\t\t" + map_name + "[");
		if(do_string == 1)
		{
			print("\"" + entry[0] + "\"");
		}
		else
		{
			print(entry[0]);
		}
		print("] = " + entry[1] + ";\n");
	}

	print("\t\t}\n");

	print("\t}\n");
	print("}\n");
	print("\n");

	return entries;
}
export function simple_map(string namespace, string class, string map_name, string init_func, array entries)
{
	return simple_map(namespace, class, map_name, init_func, entries, 0, "int");
}
export function simple_map_string(string namespace, string class, string map_name, string init_func, array entries)
{
	return simple_map(namespace, class, map_name, init_func, entries, 1, "int");
}


export function _add_mapping(array sop_map, object val, function f, array cur_try, int depth)
{
	ret = [];
	foreach(entry in sop_map[depth])
	{
		// clone cur_try
		ctc = [];
		foreach(ct in cur_try)
		{
			ctc.add(ct);
		}
		ctc.add(entry);

		if(depth == sop_map.length - 1)
		{
			// end of depth - add
			/*foreach(ct in ctc)
			{
				print(ct + ", ");
			}
			print(" = ");
			print(val + "\n");*/

			ret.add(f(ctc, val));
		}
		else
		{
			nm = _add_mapping(sop_map, val, funcref f(array, object), ctc, depth + 1);
			ret.addrange(nm);
		}
	}

	return ret;
}

export function _add_mapping(array sop_map, string val, function f, array cur_try, int depth)
{
	ret = [];
	foreach(entry in sop_map[depth])
	{
		// clone cur_try
		ctc = [];
		foreach(ct in cur_try)
		{
			ctc.add(ct);
		}
		ctc.add(entry);

		if(depth == sop_map.length - 1)
		{
			// end of depth - add
			/*foreach(ct in ctc)
			{
				print(ct + ", ");
			}
			print(" = ");
			print(val + "\n");*/

			ret.add(f(ctc, val));
		}
		else
		{
			nm = _add_mapping(sop_map, val, funcref f(array, string), ctc, depth + 1);
			ret.addrange(nm);
		}
	}

	return ret;
}

/* Generate a map from a bunch of arrays, calling function f(array, string) with
	the result.  Example: if entries is:
		[ [ [ a, b ], [ c, d ], many ],
		  [ [ e ], [ f, g ], some ],
		  [ [ h ], [ i ], few ] ]
	then it will call:
		f([a, c], many)
		f([a, d], many)
		f([b, c], many)
		f([b, d], many)
		f([e, f], some)
		f([e, g], some)
		f([h, i], few)
	i.e. all iterations of the first length - 1 entries in the array
*/
export function multimap(string namespace, string class, string name,
	string init_func, array entries, function f)
{
	print("namespace " + namespace + "\n");
	print("{\n");
	print("\tpartial class " + class + "\n");
	print("\t{\n");
	print("\t\tstatic void " + init_func + "()\n");
	print("\t\t{\n");

	foreach(entry in entries)
	{
		val = entry[entry.length - 1];
		sop_map = [];

		for(i = 0; i < entry.length - 1; i = i + 1)
		{
			sop_map.add(entry[i]);
		}

		_add_mapping(sop_map, val, funcref f(array, string), [], 0);
	}

	print("\t\t}\n");
	print("\t}\n");
	print("}\n");
	print("\n");
}
