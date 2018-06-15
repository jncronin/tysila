/* Copyright (C) 2017 by John Cronin
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

using metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using libtysila5.target;
using binary_library.binary;

namespace tysila4
{
    class COutput
    {
        internal static void WriteHeader(MetadataStream m, Target ass, string output_header,
            string output_cinit)
        {
            FileStream fs = new FileStream(output_header, FileMode.Create, FileAccess.Write);
            var sw = fs;

            var hmsw = new MemoryStream();
            var cmsw = new MemoryStream();

            FileStream oci = null;
            System.IO.FileInfo header_fi = new FileInfo(output_header);
            if (output_cinit != null)
            {
                oci = new FileStream(output_cinit, FileMode.Create, FileAccess.Write);

                HexFile.writeStr(oci, "#include \"" + header_fi.Name + "\"", true);
                HexFile.writeStr(oci, "#include <string.h>", true);
                HexFile.writeStr(oci, "#include <stdlib.h>", true);
                HexFile.writeStr(oci, "#include <stdint.h>", true);
                HexFile.writeStr(oci, "#include <stddef.h>", true);
                HexFile.writeStr(oci, "", true);
                HexFile.writeStr(oci, "INTPTR Get_Symbol_Addr(const char *name);", true);
                HexFile.writeStr(oci, "", true);
            }

            List<string> advance_defines = new List<string>();
            List<string> external_defines = new List<string>();
            List<string> func_headers = new List<string>();

            EmitType(m.GetSimpleTypeSpec(0x1c), hmsw, cmsw, advance_defines,
                external_defines, func_headers, ass);
            EmitType(m.GetSimpleTypeSpec(0xe), hmsw, cmsw, advance_defines,
                external_defines, func_headers, ass);
            EmitArrayInit(hmsw, cmsw, func_headers, ass, m);

            for(int i = 1; i <= m.table_rows[MetadataStream.tid_CustomAttribute]; i++)
            {
                int type_tid, type_row;
                m.GetCodedIndexEntry(MetadataStream.tid_CustomAttribute,
                    i, 1, m.CustomAttributeType, out type_tid,
                    out type_row);

                MethodSpec ca_ms;
                m.GetMethodDefRow(type_tid, type_row, out ca_ms);
                var ca_ms_name = ca_ms.MangleMethod();

                if (ca_ms_name == "_ZN14libsupcs#2Edll8libsupcs22OutputCHeaderAttribute_7#2Ector_Rv_P1u1t")
                {
                    int parent_tid, parent_row;
                    m.GetCodedIndexEntry(MetadataStream.tid_CustomAttribute,
                        i, 0, m.HasCustomAttribute, out parent_tid,
                        out parent_row);

                    if(parent_tid == MetadataStream.tid_TypeDef)
                    {
                        var ts = m.GetTypeSpec(parent_tid, parent_row);

                        EmitType(ts, hmsw, cmsw, advance_defines,
                            external_defines, func_headers, ass);
                    }
                }
            }

            HexFile.writeStr(sw, "");
            HexFile.writeStr(sw, "#include <stdint.h>");
            HexFile.writeStr(sw, "");
            HexFile.writeStr(sw, "#ifdef INTPTR");
            HexFile.writeStr(sw, "#undef INTPTR");
            HexFile.writeStr(sw, "#endif");
            HexFile.writeStr(sw, "#ifdef UINTPTR");
            HexFile.writeStr(sw, "#undef UINTPTR");
            HexFile.writeStr(sw, "#endif");
            HexFile.writeStr(sw, "");
            HexFile.writeStr(sw, "#define INTPTR " + ((ass.GetPointerSize() == 4) ? ass.GetCType(m.SystemInt32) : ass.GetCType(m.SystemInt64)));
            HexFile.writeStr(sw, "#define UINTPTR " + ((ass.GetPointerSize() == 4) ? ass.GetCType(m.GetSimpleTypeSpec(0x09)) : ass.GetCType(m.GetSimpleTypeSpec(0x0b))));
            HexFile.writeStr(sw, "");
            EmitArrayType(sw, ass, m);
            foreach (string s in advance_defines)
                HexFile.writeStr(sw, s);
            HexFile.writeStr(sw, "");
            if (oci != null)
            {
                foreach (string s2 in func_headers)
                    HexFile.writeStr(sw, s2);
                HexFile.writeStr(sw, "");
            }
            hmsw.Flush();
            StreamReader hmsr = new StreamReader(hmsw);
            hmsr.BaseStream.Seek(0, SeekOrigin.Begin);
            string hs = hmsr.ReadLine();
            while (hs != null)
            {
                HexFile.writeStr(sw, hs);
                hs = hmsr.ReadLine();
            }

            sw.Close();

            if (oci != null)
            {
                foreach (string s in external_defines)
                    HexFile.writeStr(oci, s);
                HexFile.writeStr(oci, "");

                cmsw.Flush();
                StreamReader cmsr = new StreamReader(cmsw);
                cmsr.BaseStream.Seek(0, SeekOrigin.Begin);
                string cs = cmsr.ReadLine();
                while (cs != null)
                {
                    HexFile.writeStr(oci, cs);
                    cs = cmsr.ReadLine();
                }
                oci.Close();
            }
        }

        private static void EmitType(TypeSpec tdr, Stream hmsw, Stream cmsw,
            List<string> advance_defines, List<string> external_defines, List<string> header_funcs, Target ass)
        {
            //Layout l = Layout.GetTypeInfoLayout(new Assembler.TypeToCompile { _ass = ass, tsig = new Signature.Param(tdr, ass), type = tdr }, ass, false);
            //Layout l = tdr.GetLayout(new Signature.Param(new Token(tdr), ass).Type, ass, null);

            var tname = tdr.m.GetStringEntry(MetadataStream.tid_TypeDef,
                tdr.tdrow, 1);
            var tns = tdr.m.GetStringEntry(MetadataStream.tid_TypeDef,
                tdr.tdrow, 2);

            int next_rsvd = 0;

            int align = libtysila5.layout.Layout.GetTypeAlignment(tdr, ass, false);

            if (!tdr.IsEnum)
            {
                HexFile.writeStr(hmsw, "struct " + tns + "_" + tname + " {");
                advance_defines.Add("struct " + tns + "_" + tname + ";");

                List<TypeSpec> fields = new List<TypeSpec>();
                List<string> fnames = new List<string>();
                libtysila5.layout.Layout.GetFieldOffset(tdr, null, ass, out var is_tls, false,
                    fields, fnames);

                for (int i = 0; i < fields.Count; i++)
                {
                    int bytesize;
                    HexFile.writeStr(hmsw, "    " + ass.GetCType(fields[i], out bytesize) + " " + fnames[i] + ";");

                    // Pad out to align size
                    bytesize = align - bytesize;
                    while ((bytesize % 2) != 0)
                    {
                        HexFile.writeStr(hmsw, "    uint8_t __reserved" + (next_rsvd++).ToString() + ";");
                        bytesize--;
                    }
                    while ((bytesize % 4) != 0)
                    {
                        HexFile.writeStr(hmsw, "    uint16_t __reserved" + (next_rsvd++).ToString() + ";");
                        bytesize -= 2;
                    }
                    while(bytesize != 0)
                    {
                        HexFile.writeStr(hmsw, "    uint32_t __reserved" + (next_rsvd++).ToString() + ";");
                        bytesize -= 4;
                    }
                }


                //if (packed_structs)
                //    HexFile.writeStr(hmsw, "} __attribute__((__packed__));");
                //else
                HexFile.writeStr(hmsw, "};");
            }
            else
            {
                // Identify underlying type
                var utype = tdr.UnderlyingType;
                bool needs_comma = false;
                HexFile.writeStr(hmsw, "enum " + tns + "_" + tname + " {");

                var first_fdef = tdr.m.GetIntEntry(MetadataStream.tid_TypeDef,
                    tdr.tdrow, 4);
                var last_fdef = tdr.m.GetLastFieldDef(tdr.tdrow);

                for (uint fdef_row = first_fdef; fdef_row < last_fdef; fdef_row++)
                {
                    // Ensure field is static
                    var flags = tdr.m.GetIntEntry(MetadataStream.tid_Field,
                        (int)fdef_row, 0);
                    if ((flags & 0x10) == 0x10)
                    {
                        for(int cridx = 1; cridx <= tdr.m.table_rows[MetadataStream.tid_Constant]; cridx++)
                        {
                            int crpar_tid, crpar_row;
                            tdr.m.GetCodedIndexEntry(MetadataStream.tid_Constant,
                                cridx, 1, tdr.m.HasConstant, out crpar_tid, out crpar_row);

                            if(crpar_tid == MetadataStream.tid_Field &&
                                crpar_row == fdef_row)
                            {
                                var value = (int)tdr.m.GetIntEntry(MetadataStream.tid_Constant,
                                    cridx, 2);

                                if (tdr.m.SigReadUSCompressed(ref value) != 4)
                                    throw new NotSupportedException("Constant value not int32");

                                var v = tdr.m.sh_blob.di.ReadInt(value);
                                var fname = tdr.m.GetStringEntry(MetadataStream.tid_Field,
                                    (int)fdef_row, 1);

                                if (needs_comma)
                                    HexFile.writeStr(hmsw, ",");
                                HexFile.writeStr(hmsw, "    " + fname + " = " + v.ToString(), true);
                                needs_comma = true;
                            }
                        }
                    }
                }
                HexFile.writeStr(hmsw);
                HexFile.writeStr(hmsw, "};");
            }

            HexFile.writeStr(hmsw);

            //if (output_cinit != null)
            //{
                if (!tdr.IsValueType)
                {
                    string init_func = "void Init_" + tns + "_" + tname + "(struct " +
                        tns + "_" + tname + " *obj)";
                    HexFile.writeStr(cmsw, init_func);
                    header_funcs.Add(init_func + ";");
                    HexFile.writeStr(cmsw, "{");

                    HexFile.writeStr(cmsw, "    obj->__vtbl = Get_Symbol_Addr(\"" + tdr.MangleType() + "\");");
                    HexFile.writeStr(cmsw, "    obj->__mutex_lock = 0;");

                    HexFile.writeStr(cmsw, "}");
                    HexFile.writeStr(cmsw);

                    if(tdr.Equals(tdr.m.SystemString))
                        EmitStringInit(tdr, hmsw, cmsw, advance_defines, external_defines, header_funcs, ass);
                }
            //}
        }

        private static void EmitArrayInit(Stream hmsw, Stream cmsw, List<string> header_funcs,
            Target ass, MetadataStream m)
        {
            EmitArrayInit(m.SystemObject, "Ref", hmsw, cmsw, header_funcs, ass);
            EmitArrayInit(m.SystemChar, "Char", hmsw, cmsw, header_funcs, ass);
            EmitArrayInit(m.SystemIntPtr, "I", hmsw, cmsw, header_funcs, ass);
            EmitArrayInit(m.SystemInt8, "I1", hmsw, cmsw, header_funcs, ass);
            EmitArrayInit(m.SystemInt16, "I2", hmsw, cmsw, header_funcs, ass);
            EmitArrayInit(m.SystemInt32, "I4", hmsw, cmsw, header_funcs, ass);
            EmitArrayInit(m.SystemInt64, "I8", hmsw, cmsw, header_funcs, ass);
            EmitArrayInit(m.GetSimpleTypeSpec(0x19), "U", hmsw, cmsw, header_funcs, ass);
            EmitArrayInit(m.GetSimpleTypeSpec(0x05), "U1", hmsw, cmsw, header_funcs, ass);
            EmitArrayInit(m.GetSimpleTypeSpec(0x07), "U2", hmsw, cmsw, header_funcs, ass);
            EmitArrayInit(m.GetSimpleTypeSpec(0x09), "U4", hmsw, cmsw, header_funcs, ass);
            EmitArrayInit(m.GetSimpleTypeSpec(0x0b), "U8", hmsw, cmsw, header_funcs, ass);
        }

        private static void EmitArrayType(Stream hmsw, Target ass, MetadataStream m)
        {
            HexFile.writeStr(hmsw, "struct __array");
            HexFile.writeStr(hmsw, "{");
            HexFile.writeStr(hmsw, "    INTPTR           __vtbl;");
            HexFile.writeStr(hmsw, "    int64_t          __mutex_lock;");
            HexFile.writeStr(hmsw, "    INTPTR           elemtype;");
            HexFile.writeStr(hmsw, "    INTPTR           lobounds;");
            HexFile.writeStr(hmsw, "    INTPTR           sizes;");
            HexFile.writeStr(hmsw, "    INTPTR           inner_array;");
            HexFile.writeStr(hmsw, "    INTPTR           rank;");
            HexFile.writeStr(hmsw, "    int32_t          elem_size;");
            //if (packed_structs)
            //    HexFile.writeStr(hmsw, "} __attribute__((__packed__));");
            //else
            HexFile.writeStr(hmsw, "};");
            HexFile.writeStr(hmsw);
        }

        private static void EmitArrayInit(TypeSpec ts, string tname, Stream hmsw, Stream cmsw,
            List<string> header_funcs, Target ass)
        {
            string typestr = ass.GetCType(ts);
            string init_func_name = "void* Create_" + tname + "_Array(struct __array **arr_obj, int32_t length)";

            /* Arrays have four pieces of memory allocated:
             * - The array superblock
             * - The lobounds array
             * - The sizes array
             * - The actual array data
             * 
             * We do not allocate the last 3, as they may need to be placed at a different virtual address
             * when relocated - let the implementation decide this
             * 
             * Code is:
             * 
             * struct __array
             * {
             *     intptr           __vtbl;
             *     int32_t          __object_id;
             *     int64_t          __mutex_lock;
             *     int32_t          rank;
             *     int32_t          elem_size;
             *     intptr           lobounds;
             *     intptr           sizes;
             *     intptr           inner_array;
             * } __attribute__((__packed__));
             * 
             * void Create_X_Array(__array **arr_obj, int32_t num_elems)
             * {
             *     *arr_obj = (__array *)malloc(sizeof(arr_obj));
             *     (*arr_obj)->rank = 1;
             * }
             */

            //int elem_size = ass.GetPackedSizeOf(new Signature.Param(baseType_Type));

            header_funcs.Add(init_func_name + ";");
            HexFile.writeStr(cmsw, init_func_name);
            HexFile.writeStr(cmsw, "{");
            HexFile.writeStr(cmsw, "    *arr_obj = (struct __array *)malloc(sizeof(struct __array));");
            HexFile.writeStr(cmsw, "    (*arr_obj)->__vtbl = Get_Symbol_Addr(\"" + ts.SzArray.MangleType() + "\");");
            HexFile.writeStr(cmsw, "    (*arr_obj)->__mutex_lock = 0;");
            HexFile.writeStr(cmsw, "    (*arr_obj)->rank = 1;");
            HexFile.writeStr(cmsw, "    (*arr_obj)->elem_size = sizeof(" + typestr + ");");
            HexFile.writeStr(cmsw, "    (*arr_obj)->elemtype = Get_Symbol_Addr(\"" + ts.MangleType() + "\");");
            HexFile.writeStr(cmsw, "    (*arr_obj)->lobounds = (INTPTR)(intptr_t)malloc(sizeof(int32_t));");
            HexFile.writeStr(cmsw, "    (*arr_obj)->sizes = (INTPTR)(intptr_t)malloc(sizeof(int32_t));");
            HexFile.writeStr(cmsw, "    (*arr_obj)->inner_array = (INTPTR)(intptr_t)malloc(length * sizeof(" + typestr + "));");
            HexFile.writeStr(cmsw, "    *(int32_t *)(intptr_t)((*arr_obj)->lobounds) = 0;");
            HexFile.writeStr(cmsw, "    *(int32_t *)(intptr_t)((*arr_obj)->sizes) = length;");
            HexFile.writeStr(cmsw, "    return((void *)(intptr_t)((*arr_obj)->inner_array));");
            HexFile.writeStr(cmsw, "}");
            HexFile.writeStr(cmsw);
        }

        private static void EmitStringInit(TypeSpec tdr, Stream hmsw, Stream cmsw,
            List<string> advance_defines, List<string> external_defines, List<string> header_funcs, Target ass)
        {
            // Emit a string creation instruction of the form:
            // void CreateString(System_String **obj, const char *s)

            string init_func = "void CreateString(struct System_String **obj, const char *s)";
            header_funcs.Add(init_func + ";");

            HexFile.writeStr(cmsw, init_func);
            HexFile.writeStr(cmsw, "{");
            HexFile.writeStr(cmsw, "    int i;");
            HexFile.writeStr(cmsw, "    int l = s == NULL ? 0 : strlen(s);");
            HexFile.writeStr(cmsw, "    " + ass.GetCType(tdr.m.SystemChar) + " *p;");
            HexFile.writeStr(cmsw, "    *obj = (struct System_String *)malloc(sizeof(struct System_String) + l * sizeof(" +
                ass.GetCType(tdr.m.SystemChar) + "));");
            HexFile.writeStr(cmsw, "    Init_System_String(*obj);");
            HexFile.writeStr(cmsw, "    (*obj)->m_stringLength = l;");
            HexFile.writeStr(cmsw, "    p = &((*obj)->m_firstChar);");
            //HexFile.writeStr(cmsw, "    p = (" + ass.GetCType(BaseType_Type.Char) +
            //    " *)(*obj + sizeof(struct System_String));");
            HexFile.writeStr(cmsw, "    for(i = 0; i < l; i++)");
            HexFile.writeStr(cmsw, "        p[i] = (" + ass.GetCType(tdr.m.SystemChar) + ")s[i];");
            HexFile.writeStr(cmsw, "}");
            HexFile.writeStr(cmsw);
        }
    }
}
