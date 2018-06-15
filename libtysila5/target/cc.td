/* generate a simple function to map cli types to registers */
export function cc(string namespace, string class, string name,
	array map)
{
	return cc(namespace, class, name, map, 0, 0);
}

export function cc(string namespace, string class, string name,
	array map, int caller_preserves, int callee_preserves)
{
	return(cc(namespace, class, name, [], map, caller_preserves, callee_preserves));
}

export function cc(string namespace, string class, string name,
	array class_map,
	array map, int caller_preserves, int callee_preserves)
{
	print("namespace " + namespace + "\n");
	print("{\n");
	print("\tpartial class " + class + "\n");
	print("\t{\n");
	print("\t\tpublic static System.Collections.Generic.Dictionary<int, int[]> cc_map_" + name + " = ");
	print("new System.Collections.Generic.Dictionary<int, int[]>");
	print("(new libtysila5.GenericEqualityComparer<int>());\n");
	print("\t\tpublic static System.Collections.Generic.Dictionary<int, int> cc_classmap_" + name + " = ");
	print("new System.Collections.Generic.Dictionary<int, int>");
	print("(new libtysila5.GenericEqualityComparer<int>());\n");

	print("\t\tinternal static void init_" + name + "()\n");
	print("\t\t{\n");

	foreach(entry in class_map)
	{
		print("\t\t\tcc_classmap_" + name + "[" + entry[0] + "] = " + entry[1] + ";\n");
	}

	foreach(entry in map)
	{
		print("\t\t\tcc_map_" + name + "[" + entry[0] + "] = new int[] { ");
		foreach(entry2 in entry[1])
		{
			print(entry2.id + ", ");
		}
		print("};\n");
	}

	print("\t\t}\n");

	print("\t\t\n");
	print("\t\tinternal const ulong " + name + "_caller_preserves = " + caller_preserves + ";\n");
	print("\t\tinternal const ulong " + name + "_callee_preserves = " + callee_preserves + ";\n");

	print("\t}\n");
	print("}\n");
	print("\n");

	return [ "cc_map_" + name, "cc_classmap_" + name ];
}

export function cc_map(string namespace, string class, array entries)
{
	caller = [];
	callee = [];
	foreach(c in entries)
	{
		caller.add([ c[0], c[0] + "_caller_preserves" ]);
		callee.add([ c[0], c[0] + "_callee_preserves" ]);
	}
	caller.add([ "default", entries[0][0] + "_caller_preserves" ]);
	callee.add([ "default", entries[0][0] + "_callee_preserves" ]);
	simple_map(namespace, class, "cc_caller_preserves_map",
		"init_cc_caller_preserves_map", caller,
		1, "");
	simple_map(namespace, class, "cc_callee_preserves_map",
		"init_cc_callee_preserves_map", callee,
		1, "");

	cc_arr = [];
	foreach(e in entries)
	{
		cc_arr.add([ e[0], e[1][0] ]);
	}
	cc_arr.add([ "default", entries[0][1][0] ]);
	cc_classmap_arr = [];
	foreach(e in entries)
	{
		cc_classmap_arr.add([ e[0], e[1][1] ]);
	}
	cc_classmap_arr.add([ "default", entries[0][1][1] ]);
	simple_map(namespace, class, "cc_classmap", "init_cc_classmap", cc_classmap_arr,
		1, "");
	return simple_map(namespace, class, "cc_map", "init_cc_map", cc_arr,
		1, "");
}

export function retcc_map(string namespace, string class, array entries)
{
	cc_arr = [];
	foreach(e in entries)
	{
		cc_arr.add([ e[0], e[1][0] ]);
	}
	cc_arr.add([ "ret_default", entries[0][1][0] ]);
	cc_classmap_arr = [];
	foreach(e in entries)
	{
		cc_classmap_arr.add([ e[0], e[1][1] ]);
	}
	cc_classmap_arr.add([ "ret_default", entries[0][1][1] ]);
	simple_map(namespace, class, "retcc_classmap", "init_retcc_classmap", cc_classmap_arr,
		1, "");
	return simple_map(namespace, class, "retcc_map", "init_retcc_map", cc_arr,
		1, "");
}