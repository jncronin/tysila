/* Copyright (C) 2008 - 2017 by John Cronin
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
    public class SignatureTable
    {
        public string GetStringTableName()
        {
            return Label;
        }

        Dictionary<object, binary_library.ISymbol> st_cache =
            new Dictionary<object, binary_library.ISymbol>(
                new GenericEqualityComparerRef<object>());

        public int GetSignatureAddress(IEnumerable<byte> sig, target.Target t)
        {
            var ptr_size = t.GetCTSize(ir.Opcode.ct_object);
            while (str_tab.Count % ptr_size != 0)
                str_tab.Add(0);

            int ret = str_tab.Count;

            foreach (byte b in sig)
                str_tab.Add(b);

            return ret;
        }

        public int GetSignatureAddress(metadata.TypeSpec.FullySpecSignature sig, target.Target t)
        {
            var ptr_size = t.GetCTSize(ir.Opcode.ct_object);
            while (str_tab.Count % ptr_size != 0)
                str_tab.Add(0);

            int ret = str_tab.Count;

            // first is type of signature
            str_tab.AddRange(t.IntPtrArray(BitConverter.GetBytes((int)sig.Type)));

            // then any extra data if necessary
            switch(sig.Type)
            {
                case metadata.Spec.FullySpecSignature.FSSType.Field:
                    // For fields with static data we insert it here
                    AddFieldSpecFields(sig.OriginalSpec as MethodSpec, str_tab, t);
                    break;

                case Spec.FullySpecSignature.FSSType.Type:
                    AddTypeSpecFields(sig.OriginalSpec as TypeSpec, str_tab, t);
                    break;
            }

            // then is length of module references
            str_tab.AddRange(t.IntPtrArray(BitConverter.GetBytes(sig.Modules.Count)));

            // then module references
            foreach(var mod in sig.Modules)
            {
                sig_metadata_addrs[str_tab.Count] = mod.AssemblyName;
                for (int i = 0; i < ptr_size; i++)
                    str_tab.Add(0);
            }

            // then signature
            str_tab.AddRange(sig.Signature);

            return ret;
        }

        private void AddTypeSpecFields(TypeSpec ts, List<byte> str_tab, Target t)
        {
            /* For types we add four special fields:
             * 
             * First: If this is an enum, its a pointer to the vtable for the underlying type
             * If it is a zero-based array, its a pointer to the vtable for the element type
             * If its a boxed value type, its the size of the value type
             * Else zero if non-generic type, -1 if generic definition, -2 if instantiated
             * 
             * Second special field is initialized to zero, and is used at runtime
             * to hold the pointer to the System.Type instance
             * 
             * Third is a pointer to the static class constructor (.cctor) if any - this is
             * used to implement System.Runtime.CompilerServices.RunClassConstructor(vtbl)
             * 
             * Fourth is a flag field (TypeAttributes from metadata)
             */

            if (ts.Unbox.IsEnum)
            {
                var ut = ts.Unbox.UnderlyingType;

                sig_metadata_addrs[str_tab.Count] = ut.MangleType();
                for (int i = 0; i < t.psize; i++)
                    str_tab.Add(0);
            }
            else if (ts.IsBoxed && !ts.Unbox.IsGenericTemplate)
            {
                var vt_size = layout.Layout.GetTypeSize(ts.Unbox, t);
                str_tab.AddRange(t.IntPtrArray(BitConverter.GetBytes(vt_size)));
            }
            else if (ts.stype == TypeSpec.SpecialType.SzArray)
            {
                sig_metadata_addrs[str_tab.Count] = ts.other.MangleType();
                for (int i = 0; i < t.psize; i++)
                    str_tab.Add(0);
            }
            else if (ts.IsGenericTemplate)
            {
                str_tab.AddRange(t.IntPtrArray(BitConverter.GetBytes(-1L)));
            }
            else if(ts.IsGeneric)
            {
                str_tab.AddRange(t.IntPtrArray(BitConverter.GetBytes(-2L)));
            }
            else
            {
                for (int i = 0; i < t.psize; i++)
                    str_tab.Add(0);
            }

            // 2nd field
            for (int i = 0; i < t.psize; i++)
                str_tab.Add(0);

            // 3rd field
            if(ts.stype == TypeSpec.SpecialType.None && !ts.IsGenericTemplate)
            {
                var cctor = ts.m.GetMethodSpec(ts, ".cctor", 0, null, false);
                if (cctor != null)
                {
                    sig_metadata_addrs[str_tab.Count] = cctor.MangleMethod();
                    t.r.MethodRequestor.Request(cctor);
                }
            }
            for (int i = 0; i < t.psize; i++)
                str_tab.Add(0);

            // 4th field - flags
            var flags = ts.m.GetIntEntry(MetadataStream.tid_TypeDef, ts.tdrow, 0);
            str_tab.AddRange(t.IntPtrArray(BitConverter.GetBytes(flags)));
        }

        private void AddFieldSpecFields(MethodSpec fs, List<byte> str_tab, target.Target t)
        {
            /* Field specs have two special fields:
             * 
             * IntPtr field_size
             * IntPtr field_data
             * 
             * where field_data may be null if no .data member is specified for the field
             */

            // read field signature to get the type of the field
            var sig_idx = fs.msig;
            sig_idx = fs.m.GetFieldSigTypeIndex(sig_idx);
            var ts = fs.m.GetTypeSpec(ref sig_idx, fs.gtparams, null);
            var fsize = t.GetSize(ts);

            str_tab.AddRange(t.IntPtrArray(BitConverter.GetBytes(fsize)));

            // now determine if the field has an rva associated with it
            int rva_id = 0;
            for(int i = 1; i <= fs.m.table_rows[MetadataStream.tid_FieldRVA]; i++)
            {
                var field_idx = fs.m.GetIntEntry(MetadataStream.tid_FieldRVA, i, 1);
                if(field_idx == fs.mdrow)
                {
                    rva_id = i;
                    break;
                }
            }

            // rva id
            if(rva_id != 0)
            {
                // TODO checks in CIL II 22.18
                var rva = fs.m.GetIntEntry(MetadataStream.tid_FieldRVA, rva_id, 0);
                var offset = fs.m.ResolveRVA(rva);
                sig_metadata_addrs[str_tab.Count] = fs.m.AssemblyName;
                sig_metadata_addends[str_tab.Count] = offset;
            }
            for (int i = 0; i < t.psize; i++)
                str_tab.Add(0);
        }

        Dictionary<int, string> sig_metadata_addrs =
            new Dictionary<int, string>(
                new GenericEqualityComparer<int>());
        Dictionary<int, long> sig_metadata_addends =
            new Dictionary<int, long>(
                new GenericEqualityComparer<int>());

        List<byte> str_tab = new List<byte>();

        private string Label;

        public SignatureTable(string mod_name)
        {
            Label = mod_name + "_SignatureTable";
        }

        public void WriteToOutput(binary_library.IBinaryFile of,
            metadata.MetadataStream ms, target.Target t,
            binary_library.ISection rd = null)
        {
            if (str_tab.Count == 0)
                return;

            if(rd == null)
                rd = of.GetRDataSection();
            rd.Align(t.GetCTSize(ir.Opcode.ct_object));

            var stab_lab = of.CreateSymbol();
            stab_lab.Name = Label;
            stab_lab.ObjectType = binary_library.SymbolObjectType.Object;
            stab_lab.Offset = (ulong)rd.Data.Count;
            stab_lab.Type = binary_library.SymbolType.Weak;
            rd.AddSymbol(stab_lab);

            int stab_base = rd.Data.Count;

            foreach (byte b in str_tab)
                rd.Data.Add(b);

            foreach(var kvp in sig_metadata_addrs)
            {
                var reloc = of.CreateRelocation();
                reloc.DefinedIn = rd;
                reloc.Type = t.GetDataToDataReloc();
                reloc.Addend = 0;

                if (sig_metadata_addends.ContainsKey(kvp.Key))
                    reloc.Addend = sig_metadata_addends[kvp.Key];

                var md_lab = of.CreateSymbol();
                md_lab.Name = kvp.Value;
                md_lab.ObjectType = binary_library.SymbolObjectType.Object;

                reloc.References = md_lab;
                reloc.Offset = (ulong)(kvp.Key + stab_base);
                of.AddRelocation(reloc);
            }

            stab_lab.Size = rd.Data.Count - (int)stab_lab.Offset;
        }
    }
}
