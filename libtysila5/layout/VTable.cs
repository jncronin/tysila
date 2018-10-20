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
using libtysila5.target;
using metadata;

namespace libtysila5.layout
{
    public partial class Layout
    {
        /* Vtable:

            TIPtr (just to a glorified TypeSpec)
            IFacePtr (to list of implemented interfaces)
            Extends (to base classes for quick castclassex)
            TypeSize (to allow MemberwiseClone to work without full Reflection)
            Any target-specific data
            Method 0
            ...

            IFaceList TODO


        */

        public static void OutputVTable(TypeSpec ts,
            target.Target t, binary_library.IBinaryFile of,
            MetadataStream base_m = null,
            ISection os = null,
            ISection data_sect = null)
        {
            // Don't compile if not for this architecture
            if (!t.IsTypeValid(ts))
                return;

            /* New signature table */
            t.sigt = new SignatureTable(ts.MangleType());

            // If its a delegate type we also need to output its methods
            if (ts.IsDelegate && !ts.IsGenericTemplate)
                t.r.DelegateRequestor.Request(ts);

            if(os == null)
                os = of.GetRDataSection();
            var d = os.Data;
            var ptr_size = t.GetCTSize(ir.Opcode.ct_object);
            os.Align(ptr_size);

            ulong offset = (ulong)os.Data.Count;

            /* Symbol */
            var sym = of.CreateSymbol();
            sym.Name = ts.MangleType();
            sym.ObjectType = binary_library.SymbolObjectType.Object;
            sym.Offset = offset;
            sym.Type = binary_library.SymbolType.Global;
            os.AddSymbol(sym);

            if (base_m != null && ts.m != base_m)
                sym.Type = SymbolType.Weak;

            /* TIPtr */
            var tiptr_offset = t.sigt.GetSignatureAddress(ts.Signature, t);

            var ti_reloc = of.CreateRelocation();
            ti_reloc.Addend = tiptr_offset;
            ti_reloc.DefinedIn = os;
            ti_reloc.Offset = offset;
            ti_reloc.Type = t.GetDataToDataReloc();
            ti_reloc.References = of.CreateSymbol();
            ti_reloc.References.Name = t.sigt.GetStringTableName();
            of.AddRelocation(ti_reloc);

            for (int i = 0; i < ptr_size; i++, offset++)
                d.Add(0);

            /* IFacePtr */
            IRelocation if_reloc = null;
            if (!ts.IsGenericTemplate && !ts.IsInterface && ts.stype != TypeSpec.SpecialType.Ptr && ts.stype != TypeSpec.SpecialType.MPtr)
            {
                if_reloc = of.CreateRelocation();
                if_reloc.DefinedIn = os;
                if_reloc.Offset = offset;
                if_reloc.Type = t.GetDataToDataReloc();
                if_reloc.References = sym;
                of.AddRelocation(if_reloc);
            }

            for (int i = 0; i < ptr_size; i++, offset++)
                d.Add(0);

            /* Extends */
            var ts_extends = ts.GetExtends();
            if(ts_extends != null)
            {
                var ext_reloc = of.CreateRelocation();
                ext_reloc.Addend = 0;
                ext_reloc.DefinedIn = os;
                ext_reloc.Offset = offset;
                ext_reloc.Type = t.GetDataToDataReloc();

                var ext_sym = of.CreateSymbol();
                ext_sym.Name = ts_extends.MangleType();

                ext_reloc.References = ext_sym;
                of.AddRelocation(ext_reloc);

                t.r.VTableRequestor.Request(ts_extends);
            }
            for (int i = 0; i < ptr_size; i++, offset++)
                d.Add(0);

            if (ts.IsInterface || ts.IsGenericTemplate || ts.stype == TypeSpec.SpecialType.MPtr || ts.stype == TypeSpec.SpecialType.Ptr)
            {
                /* Type size is zero for somethinh we cannot
                 * instantiate */
                for (int i = 0; i < ptr_size; i++, offset++)
                    d.Add(0);

                // Target-specific information
                t.AddExtraVTableFields(ts, d, ref offset);
            }
            else
            { 
                /* Type size */
                var tsize = t.IntPtrArray(BitConverter.GetBytes(GetTypeSize(ts, t)));
                foreach (var b in tsize)
                {
                    d.Add(b);
                    offset++;
                }

                // Target specific information
                t.AddExtraVTableFields(ts, d, ref offset);

                /* Virtual methods */
                OutputVirtualMethods(ts, of, os,
                    d, ref offset, t);

                /* Interface implementations */

                // build list of implemented interfaces
                var ii = ts.ImplementedInterfaces;

                // first, add all interface implementations
                List<ulong> ii_offsets = new List<ulong>();

                for (int i = 0; i < ii.Count; i++)
                {
                    ii_offsets.Add(offset - sym.Offset);
                    OutputInterface(ts, ii[i],
                        of, os, d, ref offset, t);
                    t.r.VTableRequestor.Request(ii[i]);
                }

                // point iface ptr here
                if_reloc.Addend = (long)offset - (long)sym.Offset;
                for (int i = 0; i < ii.Count; i++)
                {
                    // list is pointer to interface declaration, then implementation
                    var id_ptr_sym = of.CreateSymbol();
                    id_ptr_sym.Name = ii[i].MangleType();

                    var id_ptr_reloc = of.CreateRelocation();
                    id_ptr_reloc.Addend = 0;
                    id_ptr_reloc.DefinedIn = os;
                    id_ptr_reloc.Offset = offset;
                    id_ptr_reloc.References = id_ptr_sym;
                    id_ptr_reloc.Type = t.GetDataToDataReloc();
                    of.AddRelocation(id_ptr_reloc);

                    for (int j = 0; j < ptr_size; j++, offset++)
                        d.Add(0);

                    // implementation
                    var ii_ptr_reloc = of.CreateRelocation();
                    ii_ptr_reloc.Addend = (long)ii_offsets[i];
                    ii_ptr_reloc.DefinedIn = os;
                    ii_ptr_reloc.Offset = offset;
                    ii_ptr_reloc.References = sym;
                    ii_ptr_reloc.Type = t.GetDataToDataReloc();
                    of.AddRelocation(ii_ptr_reloc);

                    for (int j = 0; j < ptr_size; j++, offset++)
                        d.Add(0);
                }

                // null terminate the list
                for (int j = 0; j < ptr_size; j++, offset++)
                    d.Add(0);
            }

            sym.Size = (long)(offset - sym.Offset);

            /* Output signature table if any */
            if (data_sect == null)
                data_sect = of.GetDataSection();
            t.sigt.WriteToOutput(of, base_m, t, data_sect);
        }

        public class InterfaceMethodImplementation
        {
            public MethodSpec InterfaceMethod;
            public MethodSpec ImplementationMethod;

            public string TargetName
            {
                get
                {
                    if (ImplementationMethod == null)
                        return "__cxa_pure_virtual";
                    else
                        return ImplementationMethod.MangleMethod();
                }
            }
        }

        public static List<InterfaceMethodImplementation> ImplementInterface(
            TypeSpec impl_ts, TypeSpec iface_ts, Target t)
        {
            var ret = new List<InterfaceMethodImplementation>();
            bool is_boxed = false;
            if (impl_ts.IsBoxed)
            {
                impl_ts = impl_ts.Unbox;
                is_boxed = true;

                /* 'boxed' versions of each method should unbox the
                 * first parameter to a managed pointer then call
                 * the acutal method
                 */
            }
            /* Iterate through methods */
            var first_mdef = iface_ts.m.GetIntEntry(MetadataStream.tid_TypeDef,
                iface_ts.tdrow, 5);
            var last_mdef = iface_ts.m.GetLastMethodDef(iface_ts.tdrow);

            for (uint mdef_row = first_mdef; mdef_row < last_mdef; mdef_row++)
            {
                MethodSpec iface_ms;
                MethodSpec impl_ms = null;
                iface_ts.m.GetMethodDefRow(MetadataStream.tid_MethodDef,
                        (int)mdef_row, out iface_ms, iface_ts.gtparams, null);
                iface_ms.type = iface_ts;

                // First determine if there is a relevant MethodImpl entry
                for (int i = 1; i <= impl_ts.m.table_rows[MetadataStream.tid_MethodImpl]; i++)
                {
                    var Class = impl_ts.m.GetIntEntry(MetadataStream.tid_MethodImpl, i, 0);

                    if (Class == impl_ts.tdrow)
                    {
                        int mdecl_id, mdecl_row, mbody_id, mbody_row;
                        impl_ts.m.GetCodedIndexEntry(MetadataStream.tid_MethodImpl, i, 2,
                            impl_ts.m.MethodDefOrRef, out mdecl_id, out mdecl_row);
                        MethodSpec mdecl_ms;
                        impl_ts.m.GetMethodDefRow(mdecl_id, mdecl_row, out mdecl_ms, impl_ts.gtparams);

                        if (MetadataStream.CompareString(mdecl_ms.m,
                            mdecl_ms.m.GetIntEntry(MetadataStream.tid_MethodDef, mdecl_ms.mdrow, 3),
                            iface_ms.m,
                            iface_ms.m.GetIntEntry(MetadataStream.tid_MethodDef, iface_ms.mdrow, 3)) &&
                            MetadataStream.CompareSignature(mdecl_ms, iface_ms))
                        {
                            impl_ts.m.GetCodedIndexEntry(MetadataStream.tid_MethodImpl, i, 1,
                                impl_ts.m.MethodDefOrRef, out mbody_id, out mbody_row);
                            impl_ts.m.GetMethodDefRow(mbody_id, mbody_row, out impl_ms, impl_ts.gtparams);
                            impl_ms.type = impl_ts;
                            break;
                        }
                    }
                }

                // Then iterate through all base classes looking for an implementation
                if (impl_ms == null)
                    impl_ms = GetVirtualMethod(impl_ts, iface_ms, t, true);

                // Vectors implement methods which we need to provide
                if (impl_ms == null && impl_ts.stype == TypeSpec.SpecialType.SzArray)
                {
                    // Build a new method that is based in the vector class
                    impl_ms = iface_ms;
                    impl_ms.type = new TypeSpec
                    {
                        m = impl_ts.m,
                        tdrow = impl_ts.tdrow,
                        stype = impl_ts.stype,
                        other = impl_ts.other,
                        gtparams = iface_ms.type.gtparams
                    };
                }

                if (impl_ms != null)
                {
                    if (impl_ms.ReturnType != null && impl_ms.ReturnType.IsValueType && t.NeedsBoxRetType(impl_ms) && (iface_ms.ReturnType == null || (iface_ms.ReturnType != null && !iface_ms.ReturnType.IsValueType)))
                    {
                        impl_ms.ret_type_needs_boxing = true;
                        t.r.BoxedMethodRequestor.Request(impl_ms);
                    }
                    if (is_boxed)
                    {
                        impl_ms.is_boxed = true;
                        t.r.BoxedMethodRequestor.Request(impl_ms);
                    }
                    else
                        t.r.MethodRequestor.Request(impl_ms);
                }

                ret.Add(new InterfaceMethodImplementation { InterfaceMethod = iface_ms, ImplementationMethod = impl_ms });
            }
            return ret;
        }

        // TODO: Use ImplementInterface
        private static void OutputInterface(TypeSpec impl_ts,
            TypeSpec iface_ts, IBinaryFile of, ISection os,
            IList<byte> d, ref ulong offset, Target t)
        {
            bool is_boxed = false;
            if(impl_ts.IsBoxed)
            {
                impl_ts = impl_ts.Unbox;
                is_boxed = true;

                /* 'boxed' versions of each method should unbox the
                 * first parameter to a managed pointer then call
                 * the acutal method
                 */
            }
            /* Iterate through methods */
            var first_mdef = iface_ts.m.GetIntEntry(MetadataStream.tid_TypeDef,
                iface_ts.tdrow, 5);
            var last_mdef = iface_ts.m.GetLastMethodDef(iface_ts.tdrow);

            for (uint mdef_row = first_mdef; mdef_row < last_mdef; mdef_row++)
            {
                MethodSpec iface_ms;
                MethodSpec impl_ms = null;
                iface_ts.m.GetMethodDefRow(MetadataStream.tid_MethodDef,
                        (int)mdef_row, out iface_ms, iface_ts.gtparams, null);
                iface_ms.type = iface_ts;

                // First determine if there is a relevant MethodImpl entry
                for (int i = 1; i <= impl_ts.m.table_rows[MetadataStream.tid_MethodImpl]; i++)
                {
                    var Class = impl_ts.m.GetIntEntry(MetadataStream.tid_MethodImpl, i, 0);

                    if (Class == impl_ts.tdrow)
                    {
                        int mdecl_id, mdecl_row, mbody_id, mbody_row;
                        impl_ts.m.GetCodedIndexEntry(MetadataStream.tid_MethodImpl, i, 2,
                            impl_ts.m.MethodDefOrRef, out mdecl_id, out mdecl_row);
                        MethodSpec mdecl_ms;
                        impl_ts.m.GetMethodDefRow(mdecl_id, mdecl_row, out mdecl_ms, impl_ts.gtparams);

                        if (MetadataStream.CompareString(mdecl_ms.m,
                            mdecl_ms.m.GetIntEntry(MetadataStream.tid_MethodDef, mdecl_ms.mdrow, 3),
                            iface_ms.m,
                            iface_ms.m.GetIntEntry(MetadataStream.tid_MethodDef, iface_ms.mdrow, 3)) &&
                            MetadataStream.CompareSignature(mdecl_ms, iface_ms))
                        {
                            impl_ts.m.GetCodedIndexEntry(MetadataStream.tid_MethodImpl, i, 1,
                                impl_ts.m.MethodDefOrRef, out mbody_id, out mbody_row);
                            impl_ts.m.GetMethodDefRow(mbody_id, mbody_row, out impl_ms, impl_ts.gtparams);
                            impl_ms.type = impl_ts;
                            break;
                        }
                    }
                }

                // Then iterate through all base classes looking for an implementation
                if (impl_ms == null)
                    impl_ms = GetVirtualMethod(impl_ts, iface_ms, t, true);

                // Vectors implement methods which we need to provide
                if (impl_ms == null && impl_ts.stype == TypeSpec.SpecialType.SzArray)
                {
                    // Build a new method that is based in the vector class
                    impl_ms = iface_ms;
                    impl_ms.type = new TypeSpec
                    {
                        m = impl_ts.m,
                        tdrow = impl_ts.tdrow,
                        stype = impl_ts.stype,
                        other = impl_ts.other,
                        gtparams = iface_ms.type.gtparams
                    };
                }

                if(impl_ms != null)
                {
                    if (impl_ms.ReturnType != null && impl_ms.ReturnType.IsValueType && t.NeedsBoxRetType(impl_ms) && (iface_ms.ReturnType == null || (iface_ms.ReturnType != null && !iface_ms.ReturnType.IsValueType)))
                    {
                        impl_ms.ret_type_needs_boxing = true;
                        t.r.BoxedMethodRequestor.Request(impl_ms);
                    }
                    if (is_boxed)
                    {
                        impl_ms.is_boxed = true;
                        t.r.BoxedMethodRequestor.Request(impl_ms);
                    }
                    else
                        t.r.MethodRequestor.Request(impl_ms);
                }

                // Output reference
                string impl_target = (impl_ms == null) ? "__cxa_pure_virtual" : impl_ms.MangleMethod();
                //if(impl_ms == null)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    var test = GetVirtualMethod(impl_ts, iface_ms, t, true);
                //}

                var impl_sym = of.CreateSymbol();
                impl_sym.Name = impl_target;
                impl_sym.ObjectType = SymbolObjectType.Function;

                var impl_reloc = of.CreateRelocation();
                impl_reloc.Addend = 0;
                impl_reloc.DefinedIn = os;
                impl_reloc.Offset = offset;
                impl_reloc.References = impl_sym;
                impl_reloc.Type = t.GetDataToCodeReloc();
                of.AddRelocation(impl_reloc);

                for (int i = 0; i < t.GetPointerSize(); i++, offset++)
                    d.Add(0);
            }
        }

        private static void OutputVirtualMethods(TypeSpec decl_ts,
            IBinaryFile of, ISection os,
            IList<byte> d, ref ulong offset, target.Target t)
        {
            var vmeths = GetVirtualMethodDeclarations(decl_ts);
            ImplementVirtualMethods(decl_ts, vmeths);

            foreach(var vmeth in vmeths)
            {
                var impl_ms = vmeth.impl_meth;
                string impl_target = (impl_ms == null) ? "__cxa_pure_virtual" : impl_ms.MangleMethod();

                var impl_sym = of.CreateSymbol();
                impl_sym.Name = impl_target;
                impl_sym.ObjectType = SymbolObjectType.Function;

                var impl_reloc = of.CreateRelocation();
                impl_reloc.Addend = 0;
                impl_reloc.DefinedIn = os;
                impl_reloc.Offset = offset;
                impl_reloc.References = impl_sym;
                impl_reloc.Type = t.GetDataToCodeReloc();
                of.AddRelocation(impl_reloc);

                for (int i = 0; i < t.GetPointerSize(); i++, offset++)
                    d.Add(0);

                if (impl_ms != null)
                    t.r.MethodRequestor.Request(impl_ms);
            }
        }


        class VTableItem
        {
            internal MethodSpec unimpl_meth;     // the declaration site of the method
            internal TypeSpec max_implementor;   // the most derived class the method can possibly be implemented in
            internal MethodSpec impl_meth;       // the implementation of the method

            public override string ToString()
            {
                if (unimpl_meth == null)
                    return "{null}";
                else if (impl_meth == null)
                    return unimpl_meth.ToString();
                else
                    return unimpl_meth.ToString() + " (" + impl_meth.ToString() + ")";
            }

            internal VTableItem Clone()
            {
                return new VTableItem
                {
                    unimpl_meth = unimpl_meth,
                    max_implementor = max_implementor,
                    impl_meth = impl_meth
                };
            }
        }

        static Dictionary<TypeSpec, List<VTableItem>> vmeth_list_cache =
            new Dictionary<TypeSpec, List<VTableItem>>(
                new GenericEqualityComparer<TypeSpec>());

        static List<VTableItem> GetVirtualMethodDeclarations(TypeSpec ts)
        {
            List<VTableItem> ret;
            if (vmeth_list_cache.TryGetValue(ts, out ret))
                return ret;
            var extends = ts.GetExtends();
            ret = new List<VTableItem>();
            if (extends != null)
            {
                var base_list = GetVirtualMethodDeclarations(extends);
                foreach(var base_item in base_list)
                {
                    ret.Add(base_item.Clone());
                }
            }

            /* Iterate through methods looking for virtual ones */
            var first_mdef = ts.m.GetIntEntry(MetadataStream.tid_TypeDef,
                ts.tdrow, 5);
            var last_mdef = ts.m.GetLastMethodDef(ts.tdrow);

            for (uint mdef_row = first_mdef; mdef_row < last_mdef; mdef_row++)
            {
                var flags = ts.m.GetIntEntry(MetadataStream.tid_MethodDef,
                    (int)mdef_row, 2);

                if((flags & 0x40) == 0x40)
                {
                    // This is a virtual method
                    MethodSpec decl_ms;
                    ts.m.GetMethodDefRow(MetadataStream.tid_MethodDef,
                        (int)mdef_row, out decl_ms, ts.gtparams, null);
                    decl_ms.type = ts;

                    int cur_tab_idx = GetVTableIndex(decl_ms, ret);

                    if(cur_tab_idx == -1)
                    {
                        // This is not overriding anything, so give it a new slot (newslot is ignored)
                        ret.Add(new VTableItem { unimpl_meth = decl_ms, max_implementor = null });
                    }
                    else
                    {
                        // This is overriding something.  We only assign a new slot if newslot is set
                        if((flags & 0x100) == 0x100)
                        {
                            // If newslot, then the old item is left untouched, and particularly it is
                            //  only implemented by a method up to that point in the inheritance chain
                            ret.Add(new VTableItem { unimpl_meth = decl_ms, max_implementor = null });
                            ret[cur_tab_idx].max_implementor = extends;
                        }
                        else
                        {
                            // if not, then override the original slot
                            ret[cur_tab_idx].unimpl_meth = decl_ms;
                        }
                    }
                }
            }

            vmeth_list_cache[ts] = ret;
            return ret;
        }

        private static void ImplementVirtualMethods(TypeSpec ts, List<VTableItem> list)
        {
            // We implement those methods in the most derived class first and work back
            foreach (var i in list)
            {
                // skip if already done
                if (i.impl_meth != null)
                    continue;

                // if we've gone back far enough to the max_implementor class
                //  we can start looking for implementations from here
                if (i.max_implementor != null && i.max_implementor.Equals(ts))
                    i.max_implementor = null;

                // if we haven't, skip this method for this class (it will be
                //  implemented in a base class)
                if (i.max_implementor != null)
                    continue;

                // Now, search for a matching method in this particular class
                /* Iterate through methods looking for virtual ones */
                var first_mdef = ts.m.GetIntEntry(MetadataStream.tid_TypeDef,
                    ts.tdrow, 5);
                var last_mdef = ts.m.GetLastMethodDef(ts.tdrow);
                var search_meth_name = i.unimpl_meth.m.GetIntEntry(MetadataStream.tid_MethodDef,
                    i.unimpl_meth.mdrow, 3);

                for (uint mdef_row = first_mdef; mdef_row < last_mdef; mdef_row++)
                {
                    var flags = ts.m.GetIntEntry(MetadataStream.tid_MethodDef,
                        (int)mdef_row, 2);

                    if ((flags & 0x40) == 0x40)
                    {
                        // its a virtual method
                        MethodSpec impl_ms;
                        ts.m.GetMethodDefRow(MetadataStream.tid_MethodDef,
                            (int)mdef_row, out impl_ms, ts.gtparams, null);
                        impl_ms.type = ts;

                        // compare on name
                        if (MetadataStream.CompareString(i.unimpl_meth.m, search_meth_name,
                            ts.m, ts.m.GetIntEntry(MetadataStream.tid_MethodDef, (int)mdef_row, 3)))
                        {
                            // and on signature
                            if (MetadataStream.CompareSignature(i.unimpl_meth, impl_ms))
                            {
                                // If its marked abstract, we dont implement it
                                if ((flags & 0x400) == 0x400)
                                {
                                    i.impl_meth = null;
                                }
                                else
                                {
                                    // we have found a valid implementing method
                                    i.impl_meth = impl_ms;
                                }
                            }
                        }
                    }
                }
            }

            // now implement on base classes
            var extends = ts.GetExtends();
            if (extends != null)
                ImplementVirtualMethods(extends, list);
        }

        private static int GetVTableIndex(MethodSpec ms, List<VTableItem> list)
        {
            for(int idx = 0; idx < list.Count; idx++)
            {
                var i = list[idx];
                if (MetadataStream.CompareString(ms.m, ms.m.GetIntEntry(MetadataStream.tid_MethodDef, ms.mdrow, 3),
                    i.unimpl_meth.m, i.unimpl_meth.m.GetIntEntry(MetadataStream.tid_MethodDef, i.unimpl_meth.mdrow, 3)))
                {
                    if(MetadataStream.CompareSignature(ms, i.unimpl_meth))
                    {
                        return idx;
                    }
                }
            }
            return -1;
        }

        private static MethodSpec GetVirtualMethod(TypeSpec impl_ts, MethodSpec decl_ms,
            target.Target t, bool allow_non_virtual = false)
        {
            /* Iterate through methods looking for virtual ones */
            var first_mdef = impl_ts.m.GetIntEntry(MetadataStream.tid_TypeDef,
                impl_ts.tdrow, 5);
            var last_mdef = impl_ts.m.GetLastMethodDef(impl_ts.tdrow);

            for (uint mdef_row = first_mdef; mdef_row < last_mdef; mdef_row++)
            {
                var flags = impl_ts.m.GetIntEntry(MetadataStream.tid_MethodDef,
                    (int)mdef_row, 2);

                if (allow_non_virtual || (flags & 0x40) == 0x40)
                {
                    MethodSpec impl_ms;
                    impl_ts.m.GetMethodDefRow(MetadataStream.tid_MethodDef,
                        (int)mdef_row, out impl_ms, impl_ts.gtparams, null);
                    impl_ms.type = impl_ts;

                    if ((flags & 0x400) != 0x400)
                    {
                        // Not marked abstract
                        if (MetadataStream.CompareString(impl_ms.m,
                            impl_ms.m.GetIntEntry(MetadataStream.tid_MethodDef, (int)mdef_row, 3),
                            decl_ms.m,
                            decl_ms.m.GetIntEntry(MetadataStream.tid_MethodDef, (int)decl_ms.mdrow, 3)))
                        {
                            if (MetadataStream.CompareSignature(impl_ms.m, impl_ms.msig,
                                impl_ts.gtparams, null,
                                decl_ms.m, decl_ms.msig, impl_ts.gtparams, null))
                            {
                                // this is the correct one
                                return impl_ms;
                            }
                            if (MetadataStream.CompareSignature(impl_ms.m, impl_ms.msig,
                                impl_ts.gtparams, null,
                                decl_ms.m, decl_ms.msig, decl_ms.gtparams, null))
                            {
                                // this is the correct one
                                return impl_ms;
                            }
                        }
                    }
                }
            }

            // if not found, look to base classes
            var bc = impl_ts.GetExtends();
            if (bc != null)
                return GetVirtualMethod(bc, decl_ms, t, allow_non_virtual);
            else
                return null;
        }

        public static int GetVTableOffset(metadata.MethodSpec ms, Target t)
        { return GetVTableOffset(ms.type, ms, t); }

        public static int GetVTableOffset(metadata.TypeSpec ts,
            metadata.MethodSpec ms, Target t)
        {
            var vtbl = GetVirtualMethodDeclarations(ts);
            var search_meth_name = ms.m.GetIntEntry(MetadataStream.tid_MethodDef,
                ms.mdrow, 3);

            // find the requested method, match on name, signature and declaring type
            for(int i = 0; i < vtbl.Count; i++)
            {
                var test = vtbl[i];
                var mdecl = test.unimpl_meth;
                if(mdecl.type.Equals(ts))
                {
                    // Check on name
                    var mname = mdecl.m.GetIntEntry(MetadataStream.tid_MethodDef,
                        mdecl.mdrow, 3);
                    if (MetadataStream.CompareString(mdecl.m, mname,
                        ms.m, search_meth_name))
                    {
                        // Check on signature
                        if (MetadataStream.CompareSignature(mdecl.m, mdecl.msig,
                            mdecl.gtparams, mdecl.gmparams,
                            ms.m, ms.msig, ms.gtparams, ms.gmparams))
                        {
                            if (ts.IsInterface == false)
                                i += (4 + t.ExtraVTableFieldsPointerLength);
                            return i;
                        }
                    }
                }
            }

            throw new Exception("Requested virtual method slot not found");
        }
    }
}
