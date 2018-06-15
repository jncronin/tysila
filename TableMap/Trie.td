/* Generate trie root */
export function trie_gen()
{
	return [ val = null, keys = [] ];
}

/* Add a value to a trie */
export function trie_add(object trie, array key, any val, int level)
{
	if(level == key.length)
	{
		trie.val = val;
	}
	else
	{
		current_key = key[level];
		next_trie = trie.keys[current_key];

		if(next_trie == null)
		{
			next_trie = trie_gen();
			trie.keys[current_key] = next_trie;
		}

		trie_add(next_trie, key, val, level + 1);
	}
}

export function trie_add(object trie, array key, any val)
{
	trie_add(trie, key, val, 0);
}

/* Export trie to an array of ints, layout of each level is:
		value_idx <- where value is in the value array
		max_depth <- maximum trie depth below this
		start_key <- key where this level's array starts
		key_length <- length of key entries at this level
		key[start_key]		<- index of next level trie
		key[start_key + 1]	<- index of next level trie
		...
		key[start_key + key_length - 1]
*/
export function trie_start_key(object trie)
{
	for(i = 0; i < trie.keys.length; i = i + 1)
	{
		if(trie.keys[i] != null)
		{
			return i;
		}
	}
	return 0;
}
export function trie_length(object trie)
{
	sk = trie_start_key(trie);
	cur_length = 0;
	for(i = sk; i < trie.keys.length; i = i + 1)
	{
		if(trie.keys[i] != null)
		{
			cur_length = i - sk;
		}
	}
	return cur_length + 1;
}
export function trie_max_depth_recurse(object trie)
{
	max_depth = 0;
	foreach(x in trie.keys)
	{
		if(x != null)
		{
			cur_max_depth = trie_max_depth_recurse(x) + 1;
			if(cur_max_depth > max_depth)
			{
				max_depth = cur_max_depth;
			}
		}
	}
	trie.max_depth = max_depth;
	trie.start_key = trie_start_key(trie);
	trie.length = trie_length(trie);
	return max_depth;
}
export function trie_add_recurse(object trie, array output, array val_out)
{
	foreach(x in trie.keys)
	{
		if(x != null)
		{
			trie_add_recurse(x, output, val_out);
		}
	}

	trie.offset = output.length;
	if(trie.val != null)
	{
		output.add(val_out.length);
		val_out.add(trie.val);
	}
	else
	{
		output.add(0);
	}
	output.add(trie.max_depth);
	output.add(trie.start_key);
	output.add(trie.length);

	end_key = trie.start_key + trie.length;
	for(i = trie.start_key; i < end_key; i = i + 1)
	{
		x = trie.keys[i];
		if(x == null)
		{
			output.add(0);
		}
		else
		{
			output.add(x.offset);
		}
	}

	return trie.offset;
}
export function trie_build_arrays(object trie, array output, array val_out)
{
	trie_max_depth_recurse(trie);
	val_out.add(null);
	output.add(0);
	return trie_add_recurse(trie, output, val_out);
}
export function trie_export(object trie, string name, string val_type)
{
	return trie_export(trie, name, val_type, "internal static");
}
export function trie_export(object trie, string name, string val_type, string qualifier)
{
	if(qualifier.length > 0 && qualifier[qualifier.length - 1] != " ")
	{
		qualifier = qualifier + " ";
	}

	output = [];
	val_out = [];
	v = trie_build_arrays(trie, output, val_out);

	print("\t\t" + qualifier + "int[] " + name + " = new int[] {\n\t\t\t");
	line_count = 0;
	for(i = 0; i < output.length; i = i + 1)
	{
		if(line_count == 16)
		{
			print("\n\t\t\t");
			line_count = 0;
		}
		else
		{
			line_count = line_count + 1;
		}
		print(output[i] + ", ");
	}
	print("\n");
	print("\t\t};\n");
	print("\t\t\n");
	print("\t\t" + qualifier + val_type + "[] " + name + "_vals = new " + val_type + "[] {\n");
	foreach(val in val_out)
	{
		if(val == null)
		{
			print("\t\t\tdefault(" + val_type + "),\n");
		}
		else
		{
			if(val_type == "string")
			{
				print("\t\t\t\"" + val + "\",\n");
			}
			else
			{
				print("\t\t\t" + val + ",\n");
			}
		}
	}
	print("\t\t};\n");
	print("\t\t\n");
	print("\t\t" + qualifier + "int " + name + "_start = " + v + ";\n");
}

export function trie_export(object trie, string namespace, string class, string name, string val_type)
{
	print("namespace " + namespace + "\n");
	print("{\n");
	print("\tpartial class " + class + "\n");
	print("\t{\n");

	trie_export(trie, name, val_type);

	print("\t}\n");
	print("}\n");
	print("\n");
}
