/* Copyright (C) 2008 - 2011 by John Cronin
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using libtysila5.target;
using metadata;

namespace libtysila5
{
    public class StringTable
    {
        public metadata.TypeSpec StringObject;
        public int string_obj_len = 0;
        public int length_offset = 0;

        public string GetStringTableName()
        {
            return Label;
        }

        Dictionary<object, binary_library.ISymbol> st_cache =
            new Dictionary<object, binary_library.ISymbol>(
                new GenericEqualityComparerRef<object>());

        public binary_library.ISymbol GetStringTableSymbol(binary_library.IBinaryFile of)
        {
            binary_library.ISymbol sym;
            if(st_cache.TryGetValue(of, out sym))
            {
                return sym;
            }
            else
            {
                sym = of.CreateSymbol();
                st_cache[of] = sym;
                return sym;
            }
        }

        public int GetStringAddress(string s, target.Target t)
        {
            if (str_addrs.ContainsKey(s))
                return str_addrs[s];
            else
            {
                var ptr_size = t.GetCTSize(ir.Opcode.ct_object);
                while (str_tab.Count % ptr_size != 0)
                    str_tab.Add(0);

                int ret = str_tab.Count;
                str_addrs.Add(s, ret);

                //int string_obj_len = layout.Layout.GetTypeSize(StringObject,
                //    t, false);
                //int string_type_offset = 

                int i = 0;
                for (; i < length_offset; i++)
                    str_tab.Add(0);
                int len = s.Length;
                str_tab.Add((byte)(len & 0xff));
                str_tab.Add((byte)((len >> 8) & 0xff));
                str_tab.Add((byte)((len >> 16) & 0xff));
                str_tab.Add((byte)((len >> 24) & 0xff));
                i += 4;
                for (; i < string_obj_len; i++)
                    str_tab.Add(0);

                foreach (char c in s)
                {
                    str_tab.Add((byte)(c & 0xff));
                    str_tab.Add((byte)((c >> 8) & 0xff));
                }

                // null-terminate and align up to ensure coreclr String.EqualsHelper works
                str_tab.Add(0);
                str_tab.Add(0);
                while ((str_tab.Count % t.GetPointerSize()) != 0)
                    str_tab.Add(0);
                return ret;
            }
        }

        Dictionary<string, int> str_addrs =
            new Dictionary<string, int>(
                new GenericEqualityComparer<string>());

        List<byte> str_tab = new List<byte>();

        private string Label;

        public StringTable(string mod_name, metadata.AssemblyLoader al,
            target.Target t)
        {
            Label = mod_name + "_StringTable";

            var corlib = al.GetAssembly("mscorlib");
            StringObject = corlib.GetTypeSpec("System", "String");
            var fs_len = corlib.GetFieldDefRow("m_stringLength", StringObject);
            length_offset = layout.Layout.GetFieldOffset(StringObject, fs_len,
                t, out var is_tls);
            var fs_start = corlib.GetFieldDefRow("m_firstChar", StringObject);
            string_obj_len = layout.Layout.GetFieldOffset(StringObject, fs_start,
                t, out is_tls);
        }

        public void WriteToOutput(binary_library.IBinaryFile of,
            metadata.MetadataStream ms, target.Target t)
        {
            var rd = of.GetRDataSection();
            rd.Align(t.GetCTSize(ir.Opcode.ct_object));

            var stab_lab = GetStringTableSymbol(of);
            stab_lab.Name = Label;
            stab_lab.ObjectType = binary_library.SymbolObjectType.Object;
            stab_lab.Offset = (ulong)rd.Data.Count;
            stab_lab.Type = binary_library.SymbolType.Global;
            rd.AddSymbol(stab_lab);

            int stab_base = rd.Data.Count;

            foreach (byte b in str_tab)
                rd.Data.Add(b);

            var str_lab = of.CreateSymbol();
            str_lab.Name = StringObject.m.MangleType(StringObject);
            str_lab.ObjectType = binary_library.SymbolObjectType.Object;

            foreach(var str_addr in str_addrs.Values)
            {
                var reloc = of.CreateRelocation();
                reloc.DefinedIn = rd;
                reloc.Type = t.GetDataToDataReloc();
                reloc.Addend = 0;
                reloc.References = str_lab;
                reloc.Offset = (ulong)(str_addr + stab_base);
                of.AddRelocation(reloc);
            }

            stab_lab.Size = rd.Data.Count - (int)stab_lab.Offset;
        }
    }
}
