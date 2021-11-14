using System;
using System.Collections.Generic;
using System.Text;

namespace libtysila5.dwarf
{
    /** <summary>Base class for a Dwarf DIE</summary> */
    public abstract class DwarfDIE
    {
        public int Offset { get; set; }
        public target.Target t { get; set; }

        public DwarfCU dcu { get; set; }

        public abstract void WriteToOutput(DwarfSections ds, IList<byte> d, DwarfDIE parent);

        /** <summary>Write string offset to output</summary> */
        protected void w(IList<byte> d, string s, StringMap smap)
        {
            uint offset = smap.GetStringAddress(s);
            var bytes = BitConverter.GetBytes(offset);
            if (t.psize == 8)
            {
                for (int i = 0; i < 4; i++)
                    d.Add(0);
            }
            for (int i = 0; i < 4; i++)
                d.Add(bytes[i]);
        }

        /** <summary>Writes 0 at the pointer length</summary> */
        protected void wp(IList<byte> d)
        {
            for (int i = 0; i < t.psize; i++)
                d.Add(0);
        }

        /** <summary>Writes v at the pointer length</summary> */
        protected void wp(IList<byte> d, long v)
        {
            var b = BitConverter.GetBytes(v);
            for (int i = 0; i < t.psize; i++)
                d.Add(b[i]);
        }
        
        /** <summary>LEB128 encode data to output file</summary> */
        protected void w(binary_library.ISection s, uint[] data)
        {
            foreach (var d in data)
                w(s.Data, d);
        }

        /** <summary>LEB128 encode data to output file</summary> */
        protected void w(IList<byte> s, uint[] data)
        {
            foreach (var d in data)
                w(s, d);
        }

        /** <summary>LEB128 encode data to output file</summary> */
        protected void w(binary_library.ISection s, uint data)
        {
            w(s.Data, data);
        }

        /** <summary>LEB128 encode data to output file</summary> */
        public static void w(IList<byte> s, uint data)
        {
            do
            {
                uint _byte = data & 0x7fU;
                data >>= 7;
                if (data != 0)
                    _byte |= 0x80U;
                s.Add((byte)_byte);
            } while (data != 0);
        }

        /** <summary>LEB128 encode data to output file</summary> */
        public static void w(IList<byte> s, int data)
        {
            bool more = true;
            bool negative = data < 0;
            var size = 32;

            while(more)
            {
                var b = data & 0x7f;
                data >>= 7;
                if(negative)
                {
                    data |= -(1 << (size - 7));
                }
                if((data == 0 && (b & 0x40) == 0) ||
                    (data == -1 && (b & 0x40) == 0x40))
                {
                    more = false;
                }
                else
                {
                    b |= 0x80;
                }
                s.Add((byte)b);
            }
        }
    }

    public class StringMap
    {
        Dictionary<string, uint> smap = new Dictionary<string, uint>();
        List<byte> d = new List<byte>();

        public uint GetStringAddress(string v)
        {
            uint addr;
            if (smap.TryGetValue(v, out addr))
            {
                return addr;
            }
            smap[v] = (uint)d.Count;
            foreach (char c in v)
            {
                d.Add((byte)c);
            }
            d.Add(0);
            return smap[v];
        }

        public void Write(binary_library.ISection str)
        {
            foreach (var c in d)
                str.Data.Add(c);
        }
    }

    /** <summary>A DIE with children</summary> */
    public class DwarfParentDIE : DwarfDIE
    {
        public List<DwarfDIE> Children { get; } = new List<DwarfDIE>();

        public override void WriteToOutput(DwarfSections ds, IList<byte> dinfo, DwarfDIE parent)
        {
            foreach (var c in Children)
            {
                c.Offset = dinfo.Count;
                c.WriteToOutput(ds, dinfo, this);
            }

            dinfo.Add(0);    // null-terminate
        }
    }

    /** <summary>Encapsulates a compilation unit</summary> */
    public class DwarfCU : DwarfParentDIE
    {
        /** <summary>The original metadata stream we load from</summary> */
        public metadata.MetadataStream m { get; set; }

        public DwarfCU(target.Target target, metadata.MetadataStream mdata)
        {
            dcu = this;
            t = target;
            m = mdata;

            AddBaseTypes();
        }

        private void AddBaseTypes()
        {
            int[] btypes = new int[]
            {
                0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
                0x0a, 0x0b, 0x0c, 0x0d, 0x1c, 0x0e
            };
            /* Force there to be a base type in the empty namespace,
             * with C# style names.
             * 
             * Link to these using typedefs from System namespace (GDB
             * doesn't seem to like base types in namespaces) */
            foreach (var btype in btypes)
            {
                DwarfDIE btdie;
                switch(btype)
                {
                    case 0x0e:
                        // string
                        btdie = GetTypeDie(m.SystemString, false);
                        ((DwarfTypeDIE)btdie).NameOverride = "string";
                        type_dies.Remove(m.SystemString);
                        break;
                    case 0x1c:
                        // object
                        btdie = GetTypeDie(m.SystemObject, false);
                        ((DwarfTypeDIE)btdie).NameOverride = "object";
                        type_dies.Remove(m.SystemObject);
                        break;
                    default:
                        btdie = new DwarfBaseTypeDIE() { dcu = this, t = t, stype = btype };
                        break;
                }
                basetype_dies[btype] = btdie;
                Children.Add(btdie);
            }
        }

        /** <summary>Use when ref4s are not yet calculable.
         *      key is Offset where relocation is written
         *      value is target offset to place at Offset</summary> */
        public Dictionary<int, DwarfDIE> fmap = new Dictionary<int, DwarfDIE>();

        /** <summary>Line number program and its relocations, excluding header</summary> */
        public List<byte> lnp = new List<byte>();
        public Dictionary<int, binary_library.ISymbol> lnp_relocs = new Dictionary<int, binary_library.ISymbol>();
        public Dictionary<string, uint> lnp_files = new Dictionary<string, uint>();
        public List<string> lnp_fnames = new List<string>();

        /** <summary>First and last symbol in CU</summary> */
        public binary_library.ISymbol first_sym, last_sym;

        public void WriteToOutput(DwarfSections odbgsect)
        {
            WriteAbbrev(odbgsect.abbrev);

            WriteInfo(odbgsect);
            odbgsect.smap.Write(odbgsect.str);

            WriteLines(odbgsect);

            WritePubTypes(odbgsect);
            WritePubNames(odbgsect);
        }

        private void WritePubTypes(DwarfSections odbgsect)
        {
            /* Write lines header */
            List<byte> d = new List<byte>();

            // Reserve space for unit_length
            if (t.psize == 4)
            {
                for (int i = 0; i < 4; i++)
                    d.Add(0);
            }
            else
            {
                for (int i = 0; i < 12; i++)
                    d.Add(0);
            }

            // version
            d.Add(2);
            d.Add(0);

            // debug_info_offset
            for (int i = 0; i < t.psize; i++)
                d.Add(0);

            // debug_info_length
            var length = odbgsect.info.Data.Count;
            var bytes = (t.psize == 4) ? BitConverter.GetBytes((int)length) :
                BitConverter.GetBytes((long)length);
            foreach (var b in bytes)
                d.Add(b);

            foreach(var kvp in type_dies)
            {
                // offset
                bytes = (t.psize == 4) ? BitConverter.GetBytes((int)kvp.Value.Offset) :
                    BitConverter.GetBytes((long)kvp.Value.Offset);
                foreach (var b in bytes)
                    d.Add(b);

                // name
                foreach (var c in kvp.Key.Name)
                    d.Add((byte)c);
                d.Add(0);
            }

            // Finally, patch the length back in
            if (t.psize == 4)
            {
                uint len = (uint)d.Count - 4;
                bytes = BitConverter.GetBytes(len);
                for (int i = 0; i < 4; i++)
                    d[i] = bytes[i];
            }
            else
            {
                ulong len = (ulong)d.Count - 12;
                bytes = BitConverter.GetBytes(len);
                for (int i = 0; i < 4; i++)
                    d[i] = 0xff;
                for (int i = 0; i < 8; i++)
                    d[i + 4] = bytes[i];
            }

            // Store to section
            for (int i = 0; i < d.Count; i++)
                odbgsect.pubtypes.Data.Add(d[i]);
        }

        private void WritePubNames(DwarfSections odbgsect)
        {
            /* Write lines header */
            List<byte> d = new List<byte>();

            // Reserve space for unit_length
            if (t.psize == 4)
            {
                for (int i = 0; i < 4; i++)
                    d.Add(0);
            }
            else
            {
                for (int i = 0; i < 12; i++)
                    d.Add(0);
            }

            // version
            d.Add(2);
            d.Add(0);

            // debug_info_offset
            for (int i = 0; i < t.psize; i++)
                d.Add(0);

            // debug_info_length
            var length = odbgsect.info.Data.Count;
            var bytes = (t.psize == 4) ? BitConverter.GetBytes((int)length) :
                BitConverter.GetBytes((long)length);
            foreach (var b in bytes)
                d.Add(b);

            foreach (var kvp in method_dies)
            {
                // offset
                bytes = (t.psize == 4) ? BitConverter.GetBytes((int)kvp.Value.Offset) :
                    BitConverter.GetBytes((long)kvp.Value.Offset);
                foreach (var b in bytes)
                    d.Add(b);

                // name
                foreach (var c in kvp.Key.MangleMethod())
                    d.Add((byte)c);
                d.Add(0);
            }

            // Finally, patch the length back in
            if (t.psize == 4)
            {
                uint len = (uint)d.Count - 4;
                bytes = BitConverter.GetBytes(len);
                for (int i = 0; i < 4; i++)
                    d[i] = bytes[i];
            }
            else
            {
                ulong len = (ulong)d.Count - 12;
                bytes = BitConverter.GetBytes(len);
                for (int i = 0; i < 4; i++)
                    d[i] = 0xff;
                for (int i = 0; i < 8; i++)
                    d[i + 4] = bytes[i];
            }

            // Store to section
            for (int i = 0; i < d.Count; i++)
                odbgsect.pubnames.Data.Add(d[i]);
        }

        public int opcode_base = 13;
        public int line_base = -3;
        public int line_range = 6;
        public int mc_advance_max { get { return (255 - opcode_base) / line_range; } }

        private void WriteLines(DwarfSections odbgsect)
        {
            /* Write lines header */
            List<byte> d = new List<byte>();

            // Reserve space for unit_length
            if (t.psize == 4)
            {
                for (int i = 0; i < 4; i++)
                    d.Add(0);
            }
            else
            {
                for (int i = 0; i < 12; i++)
                    d.Add(0);
            }

            // version
            d.Add(4);
            d.Add(0);

            // header_length
            var header_length_offset = d.Count;
            for (int i = 0; i < t.psize; i++)
                d.Add(0);

            // minimum_instruction_length
            d.Add(1);

            // maximum_operations_per_instruction
            d.Add(1);

            // default_is_stmt
            d.Add(1);

            // line_base
            if (line_base < 0)
                d.Add((byte)(0x100 + line_base));
            else
                d.Add((byte)line_base);

            // line_range
            d.Add((byte)line_range);

            // opcode_base
            d.Add((byte)opcode_base);

            // standard_opcode_lengths
            d.AddRange(new byte[] { 0, 1, 1, 1, 1, 0, 0, 0, 1, 0, 0, 1 });

            // include_directories
            d.Add(0);

            // file_names
            foreach(var fname in lnp_fnames)
            {
                foreach (var c in fname)
                    d.Add((byte)c);
                d.Add(0);

                // dir index
                d.Add(0);

                // last_mod
                d.Add(0);

                // length
                d.Add(0);
            }
            d.Add(0);

            // point header_length here
            var doffset = d.Count;
            var header_length = doffset - header_length_offset - t.psize;
            byte[] bytes = (t.psize == 4) ? BitConverter.GetBytes((int)header_length) :
                BitConverter.GetBytes((long)header_length);
            for (int i = 0; i < t.psize; i++)
                d[header_length_offset + i] = bytes[i];

            // add the actual data
            d.AddRange(lnp);

            // add relocs
            foreach(var reloc in lnp_relocs)
            {
                var r = odbgsect.bf.CreateRelocation();
                r.Type = t.GetDataToDataReloc();
                r.Offset = (ulong)(doffset + reloc.Key);
                r.References = reloc.Value;
                r.DefinedIn = odbgsect.line;
                odbgsect.bf.AddRelocation(r);
            }

            // Finally, patch the length back in
            if (t.psize == 4)
            {
                uint len = (uint)d.Count - 4;
                bytes = BitConverter.GetBytes(len);
                for (int i = 0; i < 4; i++)
                    d[i] = bytes[i];
            }
            else
            {
                ulong len = (ulong)d.Count - 12;
                bytes = BitConverter.GetBytes(len);
                for (int i = 0; i < 4; i++)
                    d[i] = 0xff;
                for (int i = 0; i < 8; i++)
                    d[i + 4] = bytes[i];
            }

            // Store to section
            for (int i = 0; i < d.Count; i++)
                odbgsect.line.Data.Add(d[i]);
        }

        private void WriteInfo(DwarfSections ds)
        {
            var info = ds.info;
            var string_map = ds.smap;

            List<byte> d = new List<byte>();

            // Output header

            // Reserve space for unit_length
            if(t.psize == 4)
            {
                for (int i = 0; i < 4; i++)
                    d.Add(0);
            }
            else
            {
                for (int i = 0; i < 12; i++)
                    d.Add(0);
            }

            // version
            d.Add(4);
            d.Add(0);

            // debug_abbrev_offset
            for (int i = 0; i < t.psize; i++)
                d.Add(0);

            // address_size
            d.Add((byte)t.psize);

            // Store offset of root entry
            this.Offset = d.Count;


            // Write the root DIE
            w(d, 1);
            w(d, "tysila", string_map);
            w(d, 0x4);  // pretend to be C++

            var fname = m.file.Name;
            if(fname == null || fname.Equals(string.Empty))
            {
                w(d, m.AssemblyName, string_map);
                w(d, "", string_map);
            }
            else
            {
                var finfo = new System.IO.FileInfo(fname);
                w(d, finfo.Name, string_map);
                w(d, finfo.DirectoryName, string_map);
            }

            // store low/high pc offsets for patching later
            var low_pc_offset = d.Count;
            for (int i = 0; i < t.psize; i++)
                d.Add(0);
            var high_pc_offset = d.Count;
            for (int i = 0; i < t.psize; i++)
                d.Add(0);
            var stmt_list_offset = d.Count;
            for (int i = 0; i < t.psize; i++)
                d.Add(0);

            /* Children:
             *  Items in the empty namespace are placed as direct children
             *  Others are placed as children of the namespace
             */
            foreach(var kvp in ns_dies)
            {
                if(kvp.Key == "")
                {
                    foreach (var dc in kvp.Value.Children)
                        Children.Add(dc);
                }
                else
                {
                    Children.Add(kvp.Value);
                }
            }

            /* Add defintions for all the methods in the main namespace */
            foreach(var methkvp in method_dies)
            {
                var methdef = new DwarfMethodDefDIE();
                methdef.dcu = this;
                methdef.t = t;
                methdef.decl = methkvp.Value;
                Children.Add(methdef);
            }

            // Write children
            base.WriteToOutput(ds, d, null);

            // Patch relocs
            byte[] bytes;
            foreach(var kvp in fmap)
            {
                uint dest = (uint)kvp.Value.Offset;
                int addr = kvp.Key;
                bytes = BitConverter.GetBytes(dest);
                for (int i = 0; i < 4; i++)
                    d[addr + i] = bytes[i];
            }

            // Patch low/high_pc
            var low_r = ds.bf.CreateRelocation();
            low_r.Type = t.GetDataToDataReloc();
            low_r.Offset = (ulong)low_pc_offset;
            low_r.References = first_sym;
            low_r.DefinedIn = ds.info;
            ds.bf.AddRelocation(low_r);

            var pc_size = last_sym.Offset - first_sym.Offset + (ulong)last_sym.Size;
            bytes = (t.psize == 4) ? BitConverter.GetBytes((int)pc_size) :
                BitConverter.GetBytes((long)pc_size);
            for (int i = 0; i < t.psize; i++)
                d[high_pc_offset + i] = bytes[i];

            // Finally, patch the length back in
            if (t.psize == 4)
            {
                uint len = (uint)d.Count - 4;
                bytes = BitConverter.GetBytes(len);
                for (int i = 0; i < 4; i++)
                    d[i] = bytes[i];
            }
            else
            {
                ulong len = (ulong)d.Count - 12;
                bytes = BitConverter.GetBytes(len);
                for (int i = 0; i < 4; i++)
                    d[i] = 0xff;
                for (int i = 0; i < 8; i++)
                    d[i + 4] = bytes[i];
            }

            // Store to section
            for (int i = 0; i < d.Count; i++)
                info.Data.Add(d[i]);
        }

        /** Write standard abbreviations
         *    1  - compile_unit
         *    2  - base_type
         *    3  - pointer type (generic)
         *    4  - pointer type (with base type)
         *    5  - subprogram, returns void, static, non-virt
         *    6  - subprogram, returns object, static, non-virt
         *    7  - subprogram, returns void, instance, non-virt
         *    8  - subprogram, returns object, instance, non-virt
         *    9  - subprogram, returns void, instance, virtual
         *    10 - subprogram, returns object, instance, virtual
         *    11 - formal_parameter
         *    12 - namespace
         *    13 - class
         *    14 - structure
         *    15 - base_type
         *    16 - pointer
         *    17 - reference
         *    18 - member
         *    19 - formal_parameter with no name and artificial flag set
         *    20 - typedef
         *    21 - variable
         *    22 - method definition
         *    23 - class with decl_file/line/column
         *    24 - structure with decl_file/line/column
         */
        private void WriteAbbrev(binary_library.ISection abbrev)
        {
            uint dtype = t.psize == 4 ? 0x06U : 0x07U;  // data4/data 8 depending on target

            w(abbrev, new uint[]
            {
                1, 0x11, 0x01,      // compile_unit, has children
                0x25, 0x0e,         // producer, strp
                0x13, 0x0b,         // language, data1
                0x03, 0x0e,         // name, strp
                0x1b, 0x0e,         // comp_dir, strp
                0x11, 0x01,         // low_pc, addr  
                0x12, dtype,        // high_pc, data (i.e. length)
                0x10, 0x17,         // stmt_list, sec_offset
                0x00, 0x00          // terminate
            });

            w(abbrev, new uint[]
            {
                2, 0x24, 0x00,      // base_type, no children
                0x0b, 0x0b,         // byte_size, data1
                0x3e, 0x0b,         // encoding, data1
                0x03, 0x0e,         // name, strp
                0x00, 0x00,         // terminate
            });

            w(abbrev, new uint[]
            {
                3, 0x0f, 0x00,      // pointer_type, no children
                0x0b, 0x0b,         // byte_size, data1
                0x00, 0x00,         // terminate
            });

            w(abbrev, new uint[]
            {
                4, 0x0f, 0x00,      // pointer_type, no children
                0x0b, 0x0b,         // byte_size, data1
                0x49, 0x15,         // type, LEB128 from start of CU in bytes
                0x00, 0x00,
            });

            w(abbrev, new uint[]
            {
                5, 0x2e, 0x01,      // subprogram, has children
                0x03, 0x0e,         // name, strp
                0x11, 0x01,         // low_pc, addr  
                0x12, dtype,        // high_pc, data (i.e. length)
                0x32, 0x0b,         // accessibility, data1
                0x6e, 0x0e,         // linkage_name, strp
                0x3c, 0x19,         // declaration, present
                0x27, 0x19,         // prototyped, present
                0x00, 0x00,         // terminate
            });

            w(abbrev, new uint[]
            {
                6, 0x2e, 0x01,      // subprogram, has children
                0x03, 0x0e,         // name, strp
                0x11, 0x01,         // low_pc, addr  
                0x12, dtype,        // high_pc, data (i.e. length)
                0x32, 0x0b,         // accessibility, data1
                0x6e, 0x0e,         // linkage_name, strp
                0x49, 0x13,         // type, ref4
                0x3c, 0x19,         // declaration, present
                0x27, 0x19,         // prototyped, present
                0x00, 0x00,         // terminate
            });

            w(abbrev, new uint[]
            {
                7, 0x2e, 0x01,      // subprogram, has children
                0x03, 0x0e,         // name, strp
                0x11, 0x01,         // low_pc, addr  
                0x12, dtype,        // high_pc, data (i.e. length)
                0x32, 0x0b,         // accessibility, data1
                0x6e, 0x0e,         // linkage_name, strp
                0x64, 0x13,         // object_pointer, ref4
                0x3c, 0x19,         // declaration, present
                0x27, 0x19,         // prototyped, present
                0x00, 0x00,         // terminate
            });

            w(abbrev, new uint[]
            {
                8, 0x2e, 0x01,      // subprogram, has children
                0x03, 0x0e,         // name, strp
                0x11, 0x01,         // low_pc, addr  
                0x12, dtype,        // high_pc, data (i.e. length)
                0x32, 0x0b,         // accessibility, data1
                0x6e, 0x0e,         // linkage_name, strp
                0x49, 0x13,         // type, ref4
                0x64, 0x13,         // object_pointer, ref4
                0x3c, 0x19,         // declaration, present
                0x27, 0x19,         // prototyped, present
                0x00, 0x00,         // terminate
            });

            w(abbrev, new uint[]
            {
                9, 0x2e, 0x01,      // subprogram, has children
                0x03, 0x0e,         // name, strp
                0x11, 0x01,         // low_pc, addr  
                0x12, dtype,        // high_pc, data (i.e. length)
                0x32, 0x0b,         // accessibility, data1
                0x6e, 0x0e,         // linkage_name, strp
                0x64, 0x13,         // object_pointer, ref4
                0x4c, 0x0b,         // virtuality, data1
                0x3c, 0x19,         // declaration, present
                0x27, 0x19,         // prototyped, present
                0x00, 0x00,         // terminate
            });

            w(abbrev, new uint[]
            {
                10, 0x2e, 0x01,      // subprogram, has children
                0x03, 0x0e,         // name, strp
                0x11, 0x01,         // low_pc, addr  
                0x12, dtype,        // high_pc, data (i.e. length)
                0x32, 0x0b,         // accessibility, data1
                0x6e, 0x0e,         // linkage_name, strp
                0x49, 0x13,         // type, ref4
                0x64, 0x13,         // object_pointer, ref4
                0x4c, 0x0b,         // virtuality, data1
                0x3c, 0x19,         // declaration, present
                0x27, 0x19,         // prototyped, present
                0x00, 0x00,         // terminate
            });

            w(abbrev, new uint[]
            {
                11, 0x05, 0x00,     // formal_parameter, no children
                0x03, 0x0e,         // name, strp
                0x49, 0x13,         // type, ref4
                0x02, 0x18,         // location, exprloc
                0x00, 0x00,         // terminate
            });

            w(abbrev, new uint[]
            {
                12, 0x39, 0x01,     // namespace, has children
                0x03, 0x0e,         // name, strp
                0x00, 0x00,         // terminate
            });

            w(abbrev, new uint[]
            {
                13, 0x02, 0x01,     // class, has children
                0x03, 0x0e,         // name, strp
                0x0b, 0x0f,         // byte_size, udata (LEB128)
                0x00, 0x00,         // terminate
            });

            w(abbrev, new uint[]
            {
                14, 0x13, 0x01,     // structure, has children
                0x03, 0x0e,         // name, strp
                0x0b, 0x0f,         // byte_size, udata (LEB128)
                0x00, 0x00,         // terminate
            });

            w(abbrev, new uint[]
            {
                15, 0x24, 0x00,     // base_type, no children
                0x03, 0x0e,         // name, strp
                0x0b, 0x0b,         // byte_size, data1
                0x3e, 0x0b,         // encoding, data1
                0x00, 0x00,         // terminate
            });

            w(abbrev, new uint[]
            {
                16, 0x0f, 0x00,     // pointer_type, no children
                0x0b, 0x0b,         // byte_size, data1
                0x49, 0x13,         // type, ref4
                0x00, 0x00,         // terminate
            });

            w(abbrev, new uint[]
            {
                17, 0x0f, 0x00,     // reference_type, no children
                0x0b, 0x0b,         // byte_size, data1
                0x49, 0x13,         // type, ref4
                0x00, 0x00,         // terminate
            });

            w(abbrev, new uint[]
            {
                18, 0x0d, 0x00,     // member, no children
                0x03, 0x0e,         // name, strp
                0x49, 0x13,         // type, ref4
                0x38, 0x0f,         // data_member_location, udata (LEB128)
                0x00, 0x00,
            });

            w(abbrev, new uint[]
            {
                19, 0x05, 0x00,     // formal_parameter, no children
                0x49, 0x13,         // type, ref4
                0x34, 0x19,         // artificial, flag_present
                0x02, 0x18,         // location, exprloc
                0x00, 0x00,         // terminate
            });

            w(abbrev, new uint[]
            {
                20, 0x16, 0x00,     // typedef, no children
                0x03, 0x0e,         // name, strp
                0x49, 0x13,         // type, ref4
                0x00, 0x00,         // terminate
            });

            w(abbrev, new uint[]
            {
                21, 0x34, 0x00,     // variable, no children
                0x03, 0x0e,         // name, strp
                0x49, 0x13,         // type, ref4
                0x02, 0x18,         // location, exprloc
                0x00, 0x00,         // terminate
            });

            w(abbrev, new uint[]
{
                22, 0x2e, 0x01,      // subprogram, has children
                0x47, 0x13,         // specification, ref4
                0x11, 0x01,         // low_pc, addr  
                0x12, dtype,        // high_pc, data (i.e. length)
                //0x64, 0x13,         // object_pointer, ref4
                0x00, 0x00,         // terminate
            });

            w(abbrev, new uint[]
            {
                23, 0x02, 0x01,     // class, has children
                0x03, 0x0e,         // name, strp
                0x0b, 0x0f,         // byte_size, udata (LEB128)
                0x3a, 0x0b,         // decl_file, data1
                0x3b, 0x0b,         // decl_line, data1
                0x39, 0x0b,         // decl_column, data1
                0x00, 0x00,         // terminate
            });

            w(abbrev, new uint[]
            {
                24, 0x13, 0x01,     // structure, has children
                0x03, 0x0e,         // name, strp
                0x0b, 0x0f,         // byte_size, udata (LEB128)
                0x3a, 0x0b,         // decl_file, data1
                0x3b, 0x0b,         // decl_line, data1
                0x39, 0x0b,         // decl_column, data1
                0x00, 0x00,         // terminate
            });


            // last unit should have type 0
            w(abbrev, new uint[]
            {
                0x00, 0x00
            });
        }

        /* Here, we store all the allocated and non-allocated types etc that we
         *  later need to access.
         * Calling the 'Get' function will create a blank DIE if necessary but ensure
         *  that DIE's are unique for each spec type */
        Dictionary<string, DwarfNSDIE> ns_dies = new Dictionary<string, DwarfNSDIE>();
        Dictionary<metadata.TypeSpec, DwarfTypeDIE> type_dies = new Dictionary<metadata.TypeSpec, DwarfTypeDIE>();
        Dictionary<metadata.MethodSpec, DwarfMethodDIE> method_dies = new Dictionary<metadata.MethodSpec, DwarfMethodDIE>();
        public Dictionary<int, DwarfDIE> basetype_dies = new Dictionary<int, DwarfDIE>();
            
        public DwarfNSDIE GetNSDie(string ns)
        {
            DwarfNSDIE ret;
            if (ns_dies.TryGetValue(ns, out ret))
                return ret;
            ret = new DwarfNSDIE();
            ret.ns = ns;
            ret.t = t;
            ret.dcu = this;
            ns_dies[ns] = ret;
            return ret;
        }

        public DwarfDIE GetTypeDie(metadata.TypeSpec ts, bool add_ns = true)
        {
            DwarfTypeDIE ret;
            if (type_dies.TryGetValue(ts, out ret))
                return ret;
            if(ts.SimpleType != 0)
            {
                DwarfDIE dret;
                if (basetype_dies.TryGetValue(ts.SimpleType, out dret))
                    return dret;
            }
            ret = new DwarfTypeDIE();
            ret.ts = ts;
            ret.t = t;
            ret.dcu = this;
            type_dies[ts] = ret;

            if (add_ns)
            {
                // Generate namespace too
                var ns = GetNSDie(ts.Namespace);
                ns.Children.Add(ret);
            }

            // Ensure base classes etc are referenced
            if (ts.GetExtends() != null)
                GetTypeDie(ts.GetExtends());
            switch(ts.stype)
            {
                case metadata.TypeSpec.SpecialType.Array:
                case metadata.TypeSpec.SpecialType.SzArray:
                case metadata.TypeSpec.SpecialType.MPtr:
                case metadata.TypeSpec.SpecialType.Ptr:
                    if(ts.other != null)
                        GetTypeDie(ts.other);
                    break;
            }

            // Ensure field types are referenced
            if(ts.SimpleType == 0x1c ||     // Object
                ts.SimpleType == 0x0e ||    // String
                (ts.SimpleType == 0 &&
                ts.stype == metadata.TypeSpec.SpecialType.None &&
                ts.m == m)) // Class/struct in current module
            {
                bool is_tls;
                List<metadata.TypeSpec> fld_types = new List<metadata.TypeSpec>();
                List<string> fnames = new List<string>();
                List<int> foffsets = new List<int>();

                layout.Layout.GetFieldOffset(ts, null, t, out is_tls,
                    false, fld_types, fnames, foffsets);
                for(int i = 0; i < fld_types.Count; i++)
                {
                    var ft = fld_types[i];

                    var fdie = new DwarfMemberDIE();
                    fdie.dcu = dcu;
                    fdie.Name = fnames[i];
                    fdie.FieldOffset = foffsets[i];
                    fdie.FieldType = GetTypeDie(ft);
                    fdie.t = t;
                    ret.Children.Add(fdie);
                }

                layout.Layout.GetFieldOffset(ts, null, t, out is_tls,
                    true, fld_types);
                foreach (var ft in fld_types)
                    GetTypeDie(ft);
            }

            return ret;
        }

        public DwarfMethodDIE GetMethodDie(metadata.MethodSpec ms)
        {
            DwarfMethodDIE ret;
            if (method_dies.TryGetValue(ms, out ret))
                return ret;
            ret = new DwarfMethodDIE();
            ret.ms = ms;
            ret.t = t;
            ret.dcu = this;
            method_dies[ms] = ret;
            return ret;
        }
    }

    /** <summary>All the various .debug sections for the current compilation</summary> */
    public class DwarfSections
    {
        public binary_library.ISection abbrev, aranges, frame, info, line, loc, macinfo, pubnames, pubtypes,
            ranges, str, types;
        public binary_library.IBinaryFile bf;
        public StringMap smap = new StringMap();

        binary_library.ISection CreateDwarfSection(string name)
        {
            name = ".debug_" + name;

            var ret = bf.FindSection(name);
            if (ret != null)
                return ret;

            ret = bf.CreateContentsSection();
            ret.IsAlloc = false;
            ret.IsExecutable = false;
            ret.IsWriteable = false;
            ret.Name = name;

            bf.AddSection(ret);
            return ret;
        }
        public DwarfSections(binary_library.IBinaryFile _bf)
        {
            bf = _bf;

            abbrev = CreateDwarfSection("abbrev");
            //aranges = CreateDwarfSection("aranges");
            frame = CreateDwarfSection("frame");
            info = CreateDwarfSection("info");
            line = CreateDwarfSection("line");
            //loc = CreateDwarfSection("loc");
            //macinfo = CreateDwarfSection("macinfo");
            pubnames = CreateDwarfSection("pubnames");
            pubtypes = CreateDwarfSection("pubtypes");
            //ranges = CreateDwarfSection("ranges");
            str = CreateDwarfSection("str");
            //types = CreateDwarfSection("types");
        }
    }
}
