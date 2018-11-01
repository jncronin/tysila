/* Copyright (C) 2016 by John Cronin
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
using binary_library;
using metadata;

namespace libtysila5.layout
{
    public partial class Layout
    {
        public static int GetTypeAlignment(metadata.TypeSpec ts,
            target.Target t, bool is_static)
        {
            if(ts.m.classlayouts[ts.tdrow] != 0 && ts.IsGeneric == false && ts.IsGenericTemplate == false)
            {
                // see if there is a packing specified
                var pack = ts.m.GetIntEntry(metadata.MetadataStream.tid_ClassLayout,
                            ts.m.classlayouts[ts.tdrow],
                            0);
                if (pack != 0)
                    return (int)pack;
            }
            if (ts.stype != TypeSpec.SpecialType.None)
                return t.psize;

            // types always align on their most strictly aligned type
            int cur_align = 1;

            if (ts.SimpleType != 0)
            {
                // simple types larger than a pointer (e.g. object/string)
                //  still aling to pointer size;
                var ret = GetTypeSize(ts, t, is_static);
                if (ret < t.psize)
                    return ret;
                return t.psize;
            }

            // reference types will always have a pointer and int64 in them
            if (is_static == false && !ts.IsValueType)
            {
                cur_align = t.psize;
                cur_align = util.util.align(cur_align, t.GetCTSize(ir.Opcode.ct_int64));
            }
            // and static will always have an 'is_initialized'
            else if (is_static)
                cur_align = t.psize;

            /* Iterate through methods looking for requested
                one */
            var first_fdef = ts.m.GetIntEntry(MetadataStream.tid_TypeDef,
                ts.tdrow, 4);
            var last_fdef = ts.m.GetLastFieldDef(ts.tdrow);
            for (uint fdef_row = first_fdef; fdef_row < last_fdef; fdef_row++)
            {
                // Ensure field is static if requested
                var flags = ts.m.GetIntEntry(MetadataStream.tid_Field,
                    (int)fdef_row, 0);
                if (((flags & 0x10) == 0x10 && is_static == true) ||
                    ((flags & 0x10) == 0 && is_static == false))
                {
                    // Get alignment of underlying type
                    var fsig = (int)ts.m.GetIntEntry(MetadataStream.tid_Field,
                        (int)fdef_row, 2);

                    var ft = ts.m.GetFieldType(ref fsig, ts.gtparams, null);

                    int ft_align;
                    if (ft.IsValueType)
                        ft_align = GetTypeAlignment(ft, t, false);
                    else
                        ft_align = t.psize;

                    if (ft_align > cur_align)
                        cur_align = ft_align;

                }
            }

            return cur_align;
        }

        public static int GetFieldOffset(metadata.TypeSpec ts,
            metadata.MethodSpec fs, target.Target t, out bool is_tls, bool is_static = false)
        {
            int align = 1;
            is_tls = false;
            if(ts.SimpleType == 0 || ts.SimpleType == 0xe)      // class or string
                align = GetTypeAlignment(ts, t, is_static);

            /* Iterate through methods looking for requested
                one */
            var first_fdef = ts.m.GetIntEntry(MetadataStream.tid_TypeDef,
                ts.tdrow, 4);
            var last_fdef = ts.m.GetLastFieldDef(ts.tdrow);

            uint search_field_name = 0;
            if (fs != null)
            {
                search_field_name = fs.m.GetIntEntry(MetadataStream.tid_Field,
                    fs.mdrow, 1);
            }

            int cur_offset = 0;
            int cur_tl_offset = 0;

            if (is_static == false && !ts.IsValueType)
            {
                if (ts.GetExtends() == null)
                {
                    // Add a vtable entry
                    cur_offset += t.GetCTSize(ir.Opcode.ct_object);
                    cur_offset = util.util.align(cur_offset, align);

                    // Add a mutex lock entry
                    cur_offset += t.GetCTSize(ir.Opcode.ct_int64);
                    cur_offset = util.util.align(cur_offset, align);
                }
                else
                {
                    cur_offset = GetFieldOffset(ts.GetExtends(), (string)null, t, out is_tls);
                    cur_offset = util.util.align(cur_offset, align);
                }
            }
            else if(is_static)
            {
                // Add an is_initalized field
                cur_offset += t.GetCTSize(ir.Opcode.ct_intptr);
                cur_offset = util.util.align(cur_offset, align);
            }

            for (uint fdef_row = first_fdef; fdef_row < last_fdef; fdef_row++)
            {
                // Ensure field is static if requested
                var flags = ts.m.GetIntEntry(MetadataStream.tid_Field,
                    (int)fdef_row, 0);
                if (((flags & 0x10) == 0x10 && is_static == true) ||
                    ((flags & 0x10) == 0 && is_static == false))
                {
                    // Check on name if we are looking for a particular field
                    bool f_is_tls = ts.m.thread_local_fields[fdef_row];
                    if (search_field_name != 0)
                    {
                        var fname = ts.m.GetIntEntry(MetadataStream.tid_Field,
                            (int)fdef_row, 1);
                        if (MetadataStream.CompareString(ts.m, fname,
                            fs.m, search_field_name))
                        {
                            if (f_is_tls)
                            {
                                is_tls = true;
                                return cur_tl_offset;
                            }
                            else
                            {
                                is_tls = false;
                                return cur_offset;
                            }
                        }
                    }

                    // Increment by type size
                    var fsig = (int)ts.m.GetIntEntry(MetadataStream.tid_Field,
                        (int)fdef_row, 2);

                    var ft = ts.m.GetFieldType(ref fsig, ts.gtparams, null);
                    var ft_size = t.GetSize(ft);

                    if (f_is_tls)
                    {
                        cur_tl_offset += ft_size;
                        cur_tl_offset = util.util.align(cur_tl_offset, align);
                    }
                    else
                    {
                        cur_offset += ft_size;
                        cur_offset = util.util.align(cur_offset, align);
                    }
                }
            }

            // Shouldn't get here if looking for a specific field
            if(search_field_name != 0)
                throw new MissingFieldException();

            // Else return size of complete type
            return cur_offset;
        }

        public static int GetFieldOffset(metadata.TypeSpec ts,
            string fname, target.Target t, out bool is_tls, bool is_static = false,
            List<TypeSpec> field_types = null, List<string> field_names = null,
            List<int> field_offsets = null)
        {
            int align = 1;
            is_tls = false;
            if (ts.SimpleType == 0) // class
                align = GetTypeAlignment(ts, t, is_static);
            if (ts.SimpleType == 0xe && !is_static) // string
                align = t.GetPointerSize();

            if(ts.Equals(ts.m.SystemString) && !is_static)
            {
                /* System.String has a special layout in dotnet clr because the fields
                 * length and firstchar are reversed */

                if(field_names != null)
                {
                    field_names.Add("__vtbl");
                    field_names.Add("__mutex_lock");
                    field_names.Add("m_stringLength");
                    field_names.Add("m_firstChar");
                }
                if(field_types != null)
                {
                    field_types.Add(ts.m.SystemIntPtr);
                    field_types.Add(ts.m.SystemInt64);
                    field_types.Add(ts.m.SystemInt32);
                    field_types.Add(ts.m.SystemChar);
                }
                if(field_offsets != null)
                {
                    field_offsets.Add(0);
                    field_offsets.Add(GetArrayFieldOffset(ArrayField.MutexLock, t));
                    field_offsets.Add(GetTypeSize(ts.m.SystemObject, t));
                    field_offsets.Add(GetTypeSize(ts.m.SystemObject, t) + t.GetPointerSize());
                }

                if(fname == null)
                {
                    // size = sizeof(Object) + sizeof(int length), aligned to pointer size
                    return GetTypeSize(ts.m.SystemObject, t) + t.GetPointerSize();
                }
                else
                {
                    if (fname == "length" || fname == "m_stringLength")
                    {
                        return GetTypeSize(ts.m.SystemObject, t);
                    }
                    else if (fname == "start_char" || fname == "m_firstChar")
                    {
                        return GetTypeSize(ts.m.SystemObject, t) + t.GetPointerSize();
                    }
                    else
                        throw new NotSupportedException();
                }
            }

            /* Iterate through methods looking for requested
                  one */
            var first_fdef = ts.m.GetIntEntry(MetadataStream.tid_TypeDef,
                ts.tdrow, 4);
            var last_fdef = ts.m.GetLastFieldDef(ts.tdrow);

            int cur_offset = 0;
            int cur_tl_offset = 0;

            if (is_static == false && !ts.IsValueType)
            {
                if (ts.GetExtends() == null)
                {
                    // Add a vtable entry
                    if (field_offsets != null)
                        field_offsets.Add(cur_offset);
                    cur_offset += t.GetCTSize(ir.Opcode.ct_object);
                    cur_offset = util.util.align(cur_offset, align);

                    if (field_types != null)
                        field_types.Add(ts.m.SystemIntPtr);
                    if (field_names != null)
                        field_names.Add("__vtbl");

                    // Add a mutex lock entry
                    if (field_offsets != null)
                        field_offsets.Add(cur_offset);
                    cur_offset += t.GetCTSize(ir.Opcode.ct_int64);
                    cur_offset = util.util.align(cur_offset, align);

                    if (field_types != null)
                        field_types.Add(ts.m.SystemInt64);
                    if (field_names != null)
                        field_names.Add("__mutex_lock");
                }
                else
                {
                    cur_offset = GetFieldOffset(ts.GetExtends(), (string)null, t,
                        out is_tls,
                        is_static, field_types, field_names, field_offsets);
                    cur_offset = util.util.align(cur_offset, align);
                }
            }
            else if (is_static)
            {
                // Add an is_initalized field
                cur_offset += t.GetCTSize(ir.Opcode.ct_intptr);
                cur_offset = util.util.align(cur_offset, align);
            }

            for (uint fdef_row = first_fdef; fdef_row < last_fdef; fdef_row++)
            {
                // Ensure field is static if requested
                var flags = ts.m.GetIntEntry(MetadataStream.tid_Field,
                    (int)fdef_row, 0);
                if (((flags & 0x10) == 0x10 && is_static == true) ||
                    ((flags & 0x10) == 0 && is_static == false))
                {
                    // Check on name if we are looking for a particular field
                    bool f_is_tls = ts.m.thread_local_fields[fdef_row];
                    if (fname != null)
                    {
                        var ffname = ts.m.GetIntEntry(MetadataStream.tid_Field,
                            (int)fdef_row, 1);
                        if (MetadataStream.CompareString(ts.m, ffname, fname))
                        {
                            if (f_is_tls)
                            {
                                is_tls = true;
                                return cur_tl_offset;
                            }
                            else
                            {
                                is_tls = false;
                                return cur_offset;
                            }
                        }
                    }

                    // Increment by type size
                    var fsig = (int)ts.m.GetIntEntry(MetadataStream.tid_Field,
                        (int)fdef_row, 2);

                    var ft = ts.m.GetFieldType(ref fsig, ts.gtparams, null);
                    var ft_size = t.GetSize(ft);

                    if (field_types != null)
                        field_types.Add(ft);
                    if(field_names != null)
                    {
                        var cur_fname = ts.m.GetStringEntry(MetadataStream.tid_Field,
                            (int)fdef_row, 1);
                        field_names.Add(cur_fname);
                    }
                    if (field_offsets != null)
                    {
                        if (f_is_tls)
                            field_offsets.Add(cur_tl_offset);
                        else
                            field_offsets.Add(cur_offset);
                    }

                    if (f_is_tls)
                    {
                        cur_tl_offset += ft_size;
                        cur_tl_offset = util.util.align(cur_tl_offset, align);
                    }
                    else
                    {
                        cur_offset += ft_size;
                        cur_offset = util.util.align(cur_offset, align);
                    }
                }
            }

            // Shouldn't get here if looking for a specific field
            if (fname != null)
                throw new MissingFieldException();

            // Else return size of complete type
            return cur_offset;
        }


        public static int GetTypeSize(metadata.TypeSpec ts,
            target.Target t, bool is_static = false)
        {
            switch(ts.stype)
            {
                case TypeSpec.SpecialType.None:
                    if(ts.Equals(ts.m.SystemRuntimeTypeHandle) ||
                        ts.Equals(ts.m.SystemRuntimeMethodHandle) ||
                        ts.Equals(ts.m.SystemRuntimeFieldHandle))
                    {
                        return is_static ? 0 : (t.GetPointerSize());
                    }
                    if (ts.m.classlayouts[ts.tdrow] != 0 && ts.IsGeneric == false && ts.IsGenericTemplate == false)
                    {
                        var size = ts.m.GetIntEntry(metadata.MetadataStream.tid_ClassLayout,
                            ts.m.classlayouts[ts.tdrow],
                            1);
                        if(size != 0)
                            return (int)size;
                    }
                    return GetFieldOffset(ts, (string)null, t, out var is_tls, is_static);
                case TypeSpec.SpecialType.SzArray:
                case TypeSpec.SpecialType.Array:
                    if (is_static)
                        return 0;
                    return GetArrayObjectSize(t);
                case TypeSpec.SpecialType.Boxed:
                    if (is_static)
                        return 0;
                    return GetTypeSize(ts.m.SystemObject, t) +
                        GetTypeSize(ts.Unbox, t);
                default:
                    throw new NotImplementedException();
            }
        }

        public static void OutputStaticFields(metadata.TypeSpec ts,
            target.Target t, binary_library.IBinaryFile of,
            MetadataStream base_m = null,
            binary_library.ISection os = null,
            binary_library.ISection tlsos = null)
        {
            // Don't compile if not for this architecture
            if (!t.IsTypeValid(ts))
                return;

            int align = 1;
            if (ts.SimpleType == 0)
                align = GetTypeAlignment(ts, t, true);

            if(os == null)
                os = of.GetDataSection();
            os.Align(t.GetPointerSize());
            os.Align(align);

            ulong offset = (ulong)os.Data.Count;
            ulong tl_offset = 0;
            if(tlsos != null)
            {
                tl_offset = (ulong)tlsos.Data.Count;
            }

            int cur_offset = 0;
            int cur_tloffset = 0;

            /* is_initialized */
            for (int i = 0; i < t.psize; i++)
                os.Data.Add(0);
            cur_offset += t.psize;

            /* Iterate through methods looking for requested
                one */
            var first_fdef = ts.m.GetIntEntry(MetadataStream.tid_TypeDef,
                ts.tdrow, 4);
            var last_fdef = ts.m.GetLastFieldDef(ts.tdrow);

            for (uint fdef_row = first_fdef; fdef_row < last_fdef; fdef_row++)
            {
                // Ensure field is static or not as required
                var flags = ts.m.GetIntEntry(MetadataStream.tid_Field,
                    (int)fdef_row, 0);
                if ((flags & 0x10) == 0x10)
                {
                    // Increment by type size
                    var fsig = (int)ts.m.GetIntEntry(MetadataStream.tid_Field,
                        (int)fdef_row, 2);

                    var ft = ts.m.GetFieldType(ref fsig, ts.gtparams, null);
                    var ft_size = t.GetSize(ft);

                    ft_size = util.util.align(ft_size, align);

                    bool is_tls = ts.m.thread_local_fields[(int)fdef_row];
                    ISection cur_os = os;
                    if(is_tls)
                    {
                        if (tlsos == null)
                            throw new NotSupportedException("No thread-local section provided");
                        cur_os = tlsos;
                    }

                    /* See if there is any data defined as an rva */
                    var rva = ts.m.fieldrvas[(int)fdef_row];
                    if (rva != 0)
                    {
                        var rrva = (int)ts.m.ResolveRVA(rva);
                        for (int i = 0; i < ft_size; i++)
                            cur_os.Data.Add(ts.m.file.ReadByte(rrva++));
                    }
                    else
                    {
                        for (int i = 0; i < ft_size; i++)
                            cur_os.Data.Add(0);
                    }

                    /* Output any additional defined symbols */
                    foreach(var alias in ts.m.GetFieldAliases((int)fdef_row))
                    {
                        var asym = of.CreateSymbol();
                        asym.Name = alias;
                        asym.Offset = offset + (ulong)cur_offset;
                        asym.Type = binary_library.SymbolType.Global;
                        asym.ObjectType = binary_library.SymbolObjectType.Object;
                        cur_os.AddSymbol(asym);

                        asym.Size = ft_size;

                        if (base_m != null && ts.m != base_m)
                            asym.Type = binary_library.SymbolType.Weak;
                    }

                    if (is_tls)
                        cur_tloffset += ft_size;
                    else
                        cur_offset += ft_size;
                }
            }

            if (cur_offset > 0)
            {
                /* Add symbol */

                var sym = of.CreateSymbol();
                sym.Name = ts.m.MangleType(ts) + "S";
                sym.Offset = offset;
                sym.Type = binary_library.SymbolType.Global;
                sym.ObjectType = binary_library.SymbolObjectType.Object;
                os.AddSymbol(sym);

                sym.Size = os.Data.Count - (int)offset;

                if (base_m != null && ts.m != base_m)
                    sym.Type = binary_library.SymbolType.Weak;
            }
            if(cur_tloffset > 0)
            {
                /* Add thread local symbol */
                var sym = of.CreateSymbol();
                sym.Name = ts.m.MangleType(ts) + "ST";
                sym.Offset = tl_offset;
                sym.Type = binary_library.SymbolType.Global;
                sym.ObjectType = binary_library.SymbolObjectType.Object;
                tlsos.AddSymbol(sym);

                sym.Size = tlsos.Data.Count - (int)tl_offset;

                if (base_m != null && ts.m != base_m)
                    sym.Type = binary_library.SymbolType.Weak;
            }
        }
    }
}
