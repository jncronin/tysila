/* Copyright (C) 2011 by John Cronin
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

/* This defines the TysosType which is a subtype of System.Type
 * 
 * All TypeInfo structures produced by tysila2 follow this layout
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

namespace libsupcs
{
    [ExtendsOverride("_ZW6System11RuntimeType")]
    [VTableAlias("__tysos_type_vt")]
    [SpecialType]
    public unsafe partial class TysosType : Type
    {
        metadata.TypeSpec ts = null;

        /** <summary>holds a pointer to the vtbl represented by this type</summary> */
        internal void* _impl;

        internal TysosType(void *vtbl)
        {
            _impl = vtbl;

            // m_handle = this;
            *(void**)((byte*)CastOperations.ReinterpretAsPointer(this) + ClassOperations.GetFieldOffset("_ZW6System11RuntimeType", "m_handle")) =
                CastOperations.ReinterpretAsPointer(this);
        }

        internal TysosType(void* vtbl, metadata.TypeSpec typeSpec) : this(vtbl)
        {
            ts = typeSpec;
        }


        internal metadata.TypeSpec tspec
        {
            get
            {
                if (ts == null)
                {
                    ts = Metadata.GetTypeSpec(this);
                }
                return ts;
            }
        }

        public static implicit operator TysosType(metadata.TypeSpec ts)
        {
            var vtbl = JitOperations.GetAddressOfObject(ts.MangleType());
            return new TysosType(vtbl, ts);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern TysosField ReinterpretAsFieldInfo(IntPtr addr);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern TysosField ReinterpretAsFieldInfo(object obj);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern TysosMethod ReinterpretAsMethodInfo(IntPtr addr);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern TysosType ReinterpretAsType(IntPtr addr);

        [Bits32Only]
        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern TysosType ReinterpretAsType(uint addr);

        [Bits64Only]
        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern TysosType ReinterpretAsType(ulong addr);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern TysosType ReinterpretAsType(object obj);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static unsafe extern TysosType ReinterpretAsType(void* obj);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static unsafe extern RuntimeTypeHandle ReinterpretAsTypeHandle(void* obj);


        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static unsafe extern TysosMethod ReinterpretAsMethodInfo(void* obj);

        public unsafe virtual int GetClassSize() {
            return *(int *)((byte*)_impl + ClassOperations.GetVtblTypeSizeOffset());
        }

        public unsafe override System.Reflection.Assembly Assembly
        {
            get {
                var ret = RTH_GetModule(this).GetAssembly();
                System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosType.GetAssembly() returning " + ((ulong)CastOperations.ReinterpretAsPointer(ret)).ToString("X") +
                    " for " + ret.FullName);
                return ret;
            }
        }

        public override string AssemblyQualifiedName
        {
            get
            {
                return FullName + ", " + Assembly.FullName;
            }
        }

        [AlwaysCompile]
        [MethodAlias("_ZW6System17RuntimeTypeHandle_11GetBaseType_RV11RuntimeType_P1V11RuntimeType")]
        internal static Type RTH_GetBaseType(TysosType t)
        {
            return t.BaseType;
        }

        public unsafe override Type BaseType
        {
            get {
                void* extends_vtbl = *(void**)((byte*)_impl + ClassOperations.GetVtblExtendsVtblPtrOffset());
                if (extends_vtbl == null)
                    return null;
                return internal_from_vtbl(extends_vtbl);
            }
        }

        public override string FullName
        {
            get { return Namespace + "." + Name; }
        }

        public override Guid GUID
        {
            get { throw new NotImplementedException(); }
        }

        protected override System.Reflection.TypeAttributes GetAttributeFlagsImpl()
        {
            throw new NotImplementedException();
        }

        protected override System.Reflection.ConstructorInfo GetConstructorImpl(System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Reflection.CallingConventions callConvention, Type[] types, System.Reflection.ParameterModifier[] modifiers)
        {
            TysosMethod meth = GetMethodImpl(".ctor", bindingAttr, binder, callConvention, types, modifiers) as TysosMethod;
            if (meth == null)
                return null;
            return new ConstructorInfo(meth, this);
        }

        public override System.Reflection.ConstructorInfo[] GetConstructors(System.Reflection.BindingFlags bindingAttr)
        {
            System.Reflection.MethodInfo[] mis = GetMethods(bindingAttr);
            int count = 0;
            foreach (System.Reflection.MethodInfo mi in mis)
            {
                if (mi.IsConstructor)
                {
                    if (((bindingAttr & System.Reflection.BindingFlags.Static) == System.Reflection.BindingFlags.Static) && mi.IsStatic)
                        count++;
                    if (((bindingAttr & System.Reflection.BindingFlags.Instance) == System.Reflection.BindingFlags.Instance) && !mi.IsStatic)
                        count++;
                }
            }
            System.Reflection.ConstructorInfo[] ret = new System.Reflection.ConstructorInfo[count];
            int i = 0;
            foreach (System.Reflection.MethodInfo mi in mis)
            {
                if (mi.IsConstructor)
                {
                    bool add = false;
                    if (((bindingAttr & System.Reflection.BindingFlags.Static) == System.Reflection.BindingFlags.Static) && mi.IsStatic)
                        add = true;
                    if (((bindingAttr & System.Reflection.BindingFlags.Instance) == System.Reflection.BindingFlags.Instance) && !mi.IsStatic)
                        add = true;

                    if (add)
                        ret[i++] = new ConstructorInfo(mi as TysosMethod, this);
                }
            }
            return ret;
        }

        public override Type GetElementType()
        {
            throw new NotImplementedException();
        }

        public override System.Reflection.EventInfo GetEvent(string name, System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override System.Reflection.EventInfo[] GetEvents(System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override System.Reflection.FieldInfo GetField(string name, System.Reflection.BindingFlags bindingAttr)
        {
            System.Reflection.FieldInfo[] fields = GetFields(bindingAttr);
            foreach (System.Reflection.FieldInfo f in fields)
            {
                if (f.Name == name)
                    return f;
            }
            return null;
        }

        public override System.Reflection.FieldInfo[] GetFields(System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            throw new NotImplementedException();
        }

        public override Type[] GetInterfaces()
        {
            throw new NotImplementedException();
        }

        public override System.Reflection.MemberInfo[] GetMembers(System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        [AlwaysCompile]
        [MethodAlias("_ZW6System17RuntimeTypeHandle_40CreateInstanceForAnotherGenericParameter_Ru1O_P2V11RuntimeTypeV11RuntimeType")]
        static unsafe internal object RTH_CreateInstanceForAnotherGenericParameter(TysosType template, TysosType newtype)
        {
            void* t_vtbl = template._impl;
            void* n_vtbl = newtype._impl;

            /* Special case a few generic types that are required before the JIT can start */
            void* spec_vtbl = null;
            void* spec_ctor = null;
            if(t_vtbl == OtherOperations.GetStaticObjectAddress("_ZW30System#2ECollections#2EGeneric25GenericEqualityComparer`1_G1i"))
            {
                if (n_vtbl == OtherOperations.GetStaticObjectAddress("_Zu1S"))
                {
                    spec_vtbl = OtherOperations.GetStaticObjectAddress("_ZW30System#2ECollections#2EGeneric25GenericEqualityComparer`1_G1u1S");
                    spec_ctor = OtherOperations.GetFunctionAddress("_ZW30System#2ECollections#2EGeneric25GenericEqualityComparer`1_G1u1S_7#2Ector_Rv_P1u1t");
                }
                else if (n_vtbl == OtherOperations.GetStaticObjectAddress("_Zj"))
                {
                    spec_vtbl = OtherOperations.GetStaticObjectAddress("_ZW30System#2ECollections#2EGeneric25GenericEqualityComparer`1_G1j");
                    spec_ctor = OtherOperations.GetFunctionAddress("_ZW30System#2ECollections#2EGeneric25GenericEqualityComparer`1_G1j_7#2Ector_Rv_P1u1t");
                }
                else if (n_vtbl == OtherOperations.GetStaticObjectAddress("_Zi"))
                {
                    spec_vtbl = OtherOperations.GetStaticObjectAddress("_ZW30System#2ECollections#2EGeneric25GenericEqualityComparer`1_G1i");
                    spec_ctor = OtherOperations.GetFunctionAddress("_ZW30System#2ECollections#2EGeneric25GenericEqualityComparer`1_G1i_7#2Ector_Rv_P1u1t");
                }
            }
            if(spec_vtbl != null)
            {
                var ntype = internal_from_vtbl(spec_vtbl);
                var obj = ntype.Create();

                OtherOperations.CallI(CastOperations.ReinterpretAsPointer(obj), spec_ctor);

                return obj;
            }

            // Else, generate from a call to MakeGenericType
            var new_tspec = template.tspec.Clone();
            new_tspec.gtparams[0] = newtype.tspec;
            var tname = new_tspec.MangleType();
            System.Diagnostics.Debugger.Log(0, "libsupcs", "CreateInstanceForAnotherGenericParameter: " + tname);

            var vtbl = JitOperations.GetAddressOfObject(tname);
            TysosType tt = null;
            if(vtbl != null)
            {
                tt = internal_from_vtbl(vtbl);
            }
            else
            {
                vtbl = JitOperations.JitCompile(new_tspec);
                tt = new TysosType(vtbl, new_tspec);
            }

            System.Diagnostics.Debugger.Log(0, "libsupcs", "CreateInstanceForAnotherGenericParameter: vtbl found");

            var newobj = tt.Create();
            var ctor = tt.GetConstructor(Type.EmptyTypes);
            if(ctor != null)
                ctor.Invoke(newobj, Type.EmptyTypes);

            return newobj;
        }

        [MethodReferenceAlias("_ZW6System11RuntimeType_15MakeGenericType_RV4Type_P2u1tu1ZV4Type")]
        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern Type RT_MakeGenericType(object _this, object _args);

        public unsafe override Type MakeGenericType(params Type[] typeArguments)
        {
            /* We can provide fast implementations for some well-known types */
            void* this_vtbl = *(void**)((byte*)CastOperations.ReinterpretAsPointer(this) + ClassOperations.GetSystemTypeImplOffset());
            int arg_count = typeArguments.Length;
            void* arg1 = null;
            if (arg_count >= 1)
                arg1 = *(void**)((byte*)CastOperations.ReinterpretAsPointer(typeArguments[0]) + ClassOperations.GetSystemTypeImplOffset());

            // IEquatable<>
            if (this_vtbl == OtherOperations.GetStaticObjectAddress("_ZW6System12IEquatable`1") && arg_count == 1)
            {
                if (arg1 == OtherOperations.GetStaticObjectAddress("_Zu1S"))
                    return internal_from_vtbl(OtherOperations.GetStaticObjectAddress("_ZW6System12IEquatable`1_G1u1S"));
                else if (arg1 == OtherOperations.GetStaticObjectAddress("_Za"))
                    return internal_from_vtbl(OtherOperations.GetStaticObjectAddress("_ZW6System12IEquatable`1_G1a"));
                else if (arg1 == OtherOperations.GetStaticObjectAddress("_Zb"))
                    return internal_from_vtbl(OtherOperations.GetStaticObjectAddress("_ZW6System12IEquatable`1_G1b"));
                else if (arg1 == OtherOperations.GetStaticObjectAddress("_Zc"))
                    return internal_from_vtbl(OtherOperations.GetStaticObjectAddress("_ZW6System12IEquatable`1_G1c"));
                else if (arg1 == OtherOperations.GetStaticObjectAddress("_Zd"))
                    return internal_from_vtbl(OtherOperations.GetStaticObjectAddress("_ZW6System12IEquatable`1_G1d"));
                else if (arg1 == OtherOperations.GetStaticObjectAddress("_Zf"))
                    return internal_from_vtbl(OtherOperations.GetStaticObjectAddress("_ZW6System12IEquatable`1_G1f"));
                else if (arg1 == OtherOperations.GetStaticObjectAddress("_Zh"))
                    return internal_from_vtbl(OtherOperations.GetStaticObjectAddress("_ZW6System12IEquatable`1_G1h"));
                else if (arg1 == OtherOperations.GetStaticObjectAddress("_Zi"))
                    return internal_from_vtbl(OtherOperations.GetStaticObjectAddress("_ZW6System12IEquatable`1_G1i"));
                else if (arg1 == OtherOperations.GetStaticObjectAddress("_Zj"))
                    return internal_from_vtbl(OtherOperations.GetStaticObjectAddress("_ZW6System12IEquatable`1_G1j"));
                else if (arg1 == OtherOperations.GetStaticObjectAddress("_Zx"))
                    return internal_from_vtbl(OtherOperations.GetStaticObjectAddress("_ZW6System12IEquatable`1_G1x"));
                else if (arg1 == OtherOperations.GetStaticObjectAddress("_Zy"))
                    return internal_from_vtbl(OtherOperations.GetStaticObjectAddress("_ZW6System12IEquatable`1_G1y"));
            }

            System.Diagnostics.Debugger.Log(0, "libsupcs", "MakeGenericType: no fast implementation for");
            System.Diagnostics.Debugger.Log((int)this_vtbl, "libsupcs", "this_vtbl");
            System.Diagnostics.Debugger.Log(arg_count, "libsupcs", "arg_count");
            System.Diagnostics.Debugger.Log((int)arg1, "libsupcs", "arg1");

            /* Create a typespec for the new type */
            var tmpl = this.tspec;

            if(tmpl.GenericParamCount != typeArguments.Length)
            {
                throw new TypeLoadException("Incorrect number of generic parameters provided for " +
                    tmpl.MangleType() + ": got " + typeArguments.Length +
                    ", expected " + tmpl.GenericParamCount);
            }

            var new_tspec = tmpl.Clone();
            new_tspec.gtparams = new metadata.TypeSpec[typeArguments.Length];
            for(int i = 0; i < typeArguments.Length; i++)
            {
                new_tspec.gtparams[i] = ((TysosType)typeArguments[i]).tspec;
            }

            var tname = new_tspec.MangleType();

            System.Diagnostics.Debugger.Log(0, "libsupcs", "MakeGenericType: " + tname);

            var new_addr = JitOperations.GetAddressOfObject(tname);
            if(new_addr == null)
            {
                System.Diagnostics.Debugger.Log(0, "libsupcs", "MakeGenericType: not available - need to create");
                var new_vtbl = JitOperations.JitCompile(new_tspec);
                var new_tt = new TysosType(new_vtbl, new_tspec);
                return new_tt;
            }
            else
            {
                System.Diagnostics.Debugger.Log(0, "libsupcs", "MakeGenericType: is available - using already compiled version");
                var existing_obj = internal_from_vtbl(new_addr);
                return existing_obj;
            }


            //return RT_MakeGenericType(this, typeArguments);
        }

        private bool MatchBindingFlags(System.Reflection.MethodInfo mi, System.Reflection.BindingFlags bindingAttr)
        {
            bool match = false;
            if (((bindingAttr & System.Reflection.BindingFlags.Static) == System.Reflection.BindingFlags.Static) && (mi.IsStatic))
                match = true;
            if (((bindingAttr & System.Reflection.BindingFlags.Instance) == System.Reflection.BindingFlags.Instance) && (!mi.IsStatic))
                match = true;

            return match;
        }        

        public override System.Reflection.MethodInfo[] GetMethods(System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override Type GetNestedType(string name, System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override Type[] GetNestedTypes(System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override System.Reflection.PropertyInfo[] GetProperties(System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        protected override System.Reflection.PropertyInfo GetPropertyImpl(string name, System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, Type returnType, Type[] types, System.Reflection.ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        protected override bool HasElementTypeImpl()
        {
            return (IsArray || IsZeroBasedArray || IsManagedPointer || IsUnmanagedPointer);
        }

        public override object InvokeMember(string name, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object target, object[] args, System.Reflection.ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters)
        {
            if ((invokeAttr & System.Reflection.BindingFlags.CreateInstance) != 0)
            {
                // Invoke constructor

                // build an array of the types to search for
                Type[] param_types = null;
                if (args != null)
                {
                    param_types = new Type[args.Length];
                    for (int i = 0; i < args.Length; i++)
                        param_types[i] = args[i].GetType();
                }

                // Find the constructor
                System.Reflection.ConstructorInfo ci = GetConstructor(invokeAttr, binder, param_types, modifiers);
                if (ci == null)
                    throw new MissingMethodException("Could not find a matching constructor");

                // Execute it
                return ci.Invoke(args);
            }
            else
            {
                throw new NotImplementedException("InvokeMember currently only defined for constructors");
            }
        }

        protected override bool IsArrayImpl()
        {
            return IsZeroBasedArray;
        }

        protected override bool IsByRefImpl()
        {
            return IsManagedPointer;
        }

        protected override bool IsCOMObjectImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool IsPointerImpl()
        {
            return IsUnmanagedPointer;
        }

        protected override bool IsPrimitiveImpl()
        {
            throw new NotImplementedException();
        }

        public override System.Reflection.Module Module
        {
            get { return TysosModule.ReinterpretAsModule(RTH_GetModule(this)); }
        }

        string nspace = null, name = null;
        public override string Namespace
        {
            get {
                if (nspace != null)
                    return nspace;

                var ts = tspec;
                if (ts == null)
                    nspace = "System";
                else
                {
                    nspace = ts.m.GetStringEntry(metadata.MetadataStream.tid_TypeDef,
                        ts.tdrow, 2);
                }

                return nspace;
            }
        }

        public override Type UnderlyingSystemType
        {
            get { return this; }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return false;
        }

        public override string Name
        {
            get
            {
                if (name != null)
                    return name;

                var ts = tspec;
                if (ts == null)
                    name = "Void";
                else
                {
                    name = ts.m.GetStringEntry(metadata.MetadataStream.tid_TypeDef,
                        ts.tdrow, 1);
                }

                return name;
            }
        }

        public virtual TysosType UnboxedType { get { throw new NotImplementedException(); } }

        public override bool IsGenericType
        {
            get
            {
                void* ti = **(void***)((byte*)CastOperations.ReinterpretAsPointer(this) + ClassOperations.GetSystemTypeImplOffset());
                var gtid = *(int*)((void**)ti + 1);
                // IsGenericType is true for instantiated and non-instantiated generic types
                if (gtid == -1 || gtid == -2)
                    return true;
                return false;
            }
        }
        public unsafe override bool IsGenericTypeDefinition {
            get {
                void* ti = **(void***)((byte*)CastOperations.ReinterpretAsPointer(this) + ClassOperations.GetSystemTypeImplOffset());
                return *(int*)((void**)ti + 1) == -1;
            }
        }

        [MethodAlias("_ZW6System4Type_18type_is_subtype_of_Rb_P3V4TypeV4Typeb")]
        [AlwaysCompile]
        static unsafe bool IsSubtypeOf(void* subclass, void* superclass, bool check_interfaces)
        {
            var sub_vt = *(void**)((byte*)subclass + ClassOperations.GetSystemTypeImplOffset());
            var super_vt = *(void**)((byte*)superclass + ClassOperations.GetSystemTypeImplOffset());

            var cur_sub_vt = *(void**)((byte*)sub_vt + ClassOperations.GetVtblExtendsVtblPtrOffset());

            while (cur_sub_vt != null)
            {
                if (cur_sub_vt == super_vt)
                    return true;

                cur_sub_vt = *(void**)((byte*)cur_sub_vt + ClassOperations.GetVtblExtendsVtblPtrOffset());
            }

            if (check_interfaces)
                throw new NotImplementedException();

            return false;
        }

        [MethodAlias("_ZW6System4Type_23type_is_assignable_from_Rb_P2V4TypeV4Type")]
        [AlwaysCompile]
        static unsafe bool IsAssignableFrom(TysosType cur_type, TysosType from_type)
        {
            if (cur_type == from_type)
                return true;

            var cur_vtbl = *((void**)((byte*)CastOperations.ReinterpretAsPointer(cur_type) + ClassOperations.GetSystemTypeImplOffset()));
            var from_vtbl = *((void**)((byte*)CastOperations.ReinterpretAsPointer(from_type) + ClassOperations.GetSystemTypeImplOffset()));

            // check extends chain
            var cur_from_vtbl = from_vtbl;
            while (cur_from_vtbl != null)
            {
                if (cur_from_vtbl == cur_vtbl)
                    return true;
                cur_from_vtbl = *((void**)((byte*)cur_from_vtbl + ClassOperations.GetVtblExtendsVtblPtrOffset()));
            }

            // check interfaces
            var cur_from_iface_ptr = *(void***)((byte*)cur_vtbl + ClassOperations.GetVtblInterfacesPtrOffset());
            while (*cur_from_iface_ptr != null)
            {
                if (*cur_from_iface_ptr == cur_vtbl)
                    return true;
                cur_from_iface_ptr += 2;
            }

            // check whether they are arrays of the same type
            var cur_ts = cur_type.tspec;
            var from_ts = cur_type.tspec;

            if (cur_ts.stype == metadata.TypeSpec.SpecialType.SzArray &&
                from_ts.stype == metadata.TypeSpec.SpecialType.SzArray)
            {
                if (cur_ts.other.Equals(from_ts.other))
                    return true;
            }

            // TODO: complex array

            return false;
        }

        public override Type GetGenericTypeDefinition()
        {
            var new_tspec = tspec.Clone();
            new_tspec.gtparams = null;
            var nts_name = new_tspec.MangleType();
            var nts_addr = JitOperations.GetAddressOfObject(nts_name);
            if (nts_addr != null)
                return internal_from_vtbl(nts_addr);
            else
                return new TysosType(null, new_tspec);
        }

        public unsafe override RuntimeTypeHandle TypeHandle
        {
            get
            {
                var vtbl = *GetImplOffset();
                return ReinterpretAsTypeHandle(vtbl);
            }
        }

        public bool IsUnmanagedPointer { get { return tspec.stype == metadata.TypeSpec.SpecialType.Ptr; } }
        public bool IsZeroBasedArray { get { return tspec.stype == metadata.TypeSpec.SpecialType.SzArray; } }
        public bool IsManagedPointer { get { return tspec.stype == metadata.TypeSpec.SpecialType.MPtr; } }
        public bool IsDynamic { get { throw new NotImplementedException(); } }
        public uint TypeFlags { get { throw new NotImplementedException(); } }

        public bool IsUninstantiatedGenericTypeParameter { get { throw new NotImplementedException(); } }
        public bool IsUninstantiatedGenericMethodParameter { get { throw new NotImplementedException(); } }
        public int UgtpIdx { get { throw new NotImplementedException(); } }
        public int UgmpIdx { get { throw new NotImplementedException(); } }

        internal unsafe static int GetValueTypeSize(void* boxed_vtbl)
        {
            /* Boxed value types (that aren't enums) store their size in the
             * typeinfo.  Enums store the underlying type there, so we have
             * to call ourselves again if this is an enum. */

            void* ti = *(void**)boxed_vtbl;
            void* extends = *(void**)((byte*)boxed_vtbl + ClassOperations.GetVtblExtendsVtblPtrOffset());
            if (extends == OtherOperations.GetStaticObjectAddress("_ZW6System4Enum"))
            {
                void* underlying_type = *((void**)ti + 1);
                return GetValueTypeSize(underlying_type);
            }

            /* All calling functions should guarantee this is a value type, so the following is valid */
            return *(int*)((void**)ti + 1);
        }

        protected override bool IsValueTypeImpl()
        {
            unsafe
            {
                void* extends = *(void**)((byte*)CastOperations.ReinterpretAsPointer(this) + ClassOperations.GetVtblExtendsVtblPtrOffset());

                if (extends == OtherOperations.GetStaticObjectAddress("_Zu1L") ||
                    extends == OtherOperations.GetStaticObjectAddress("_ZW6System4Enum"))
                    return true;
                return false;
            }
        }

        [MethodAlias("_ZW6System4Type_15make_array_type_RV4Type_P2u1ti")]
        [AlwaysCompile]
        static TysosType make_array_type(TysosType cur_type, int rank)
        {
            throw new NotImplementedException();
        }

        internal unsafe object Create()
        {
            byte* ret = (byte*)MemoryOperations.GcMalloc(GetClassSize());
            void* vtbl = *(void**)((byte*)CastOperations.ReinterpretAsPointer(this) + ClassOperations.GetSystemTypeImplOffset());

            *(void**)(ret + ClassOperations.GetVtblFieldOffset()) = vtbl;
            *(ulong*)(ret + ClassOperations.GetMutexLockOffset()) = 0;

            return CastOperations.ReinterpretAsObject(ret);
        }

        static internal int obj_id = 0;

        [MethodAlias("_Zu1O_7GetType_RW6System4Type_P1u1t")]
        [AlwaysCompile]
        static unsafe TysosType Object_GetType(void** obj)
        {
            void* vtbl = *obj;

            return internal_from_vtbl(vtbl);
        }

        [MethodAlias("_ZW35System#2ERuntime#2ECompilerServices14RuntimeHelpers_20_RunClassConstructor_Rv_P1U6System11RuntimeType")]
        [AlwaysCompile]
        static unsafe void RuntimeHelpers__RunClassConstructor(void* vtbl)
        {
            // Ensure ptr is valid
            if (vtbl == null)
            {
                System.Diagnostics.Debugger.Log(0, "libsupcs", "RuntimeHelpers._RunClassConstructor: called with null pointer");
                throw new Exception("Invalid type handle");
            }

            // dereference vtbl pointer to get ti ptr
            var ptr = *((void**)vtbl);
            if (ptr == null)
            {
                System.Diagnostics.Debugger.Log(0, "libsupcs", "RuntimeHelpers._RunClassConstructor: called with null pointer");
                throw new Exception("Invalid type handle");
            }

            if ((*((int*)ptr) & 0xf) != 0)
            {
                System.Diagnostics.Debugger.Log(0, "libsupcs", "RuntimeHelpers._RunClassConstructor: called with invalid runtimehandle: " +
                    (*((int*)ptr)).ToString() + " at " + ((ulong)ptr).ToString("X16"));
                System.Diagnostics.Debugger.Break();
                throw new Exception("Invalid type handle");
            }

            var ti_ptr = (void**)ptr;

            var cctor_addr = ti_ptr[3];
            if (cctor_addr != null)
            {
                System.Diagnostics.Debugger.Log(0, "libsupcs", "RuntimeHelpers._RunClassConstructor: running static constructor at " +
                    ((ulong)cctor_addr).ToString("X16"));
                OtherOperations.CallI(cctor_addr);
                System.Diagnostics.Debugger.Log(0, "libsupcs", "RuntimeHelpers._RunClassConstructor: finished running static constructor at " +
                    ((ulong)cctor_addr).ToString("X16"));
            }
        }

        internal unsafe void** GetImplOffset()
        {
            byte* tp = (byte*)CastOperations.ReinterpretAsPointer(this);
            return (void**)(tp + ClassOperations.GetSystemTypeImplOffset());
        }

        [WeakLinkage]
        [AlwaysCompile]
        [MethodAlias("_ZW6System4Type_13op_Inequality_Rb_P2V4TypeV4Type")]
        static internal unsafe bool NotEqualsInternal(TysosType a, TysosType b)
        {
            return !EqualsInternal(a, b);
        }

        [MethodAlias("_ZW6System4Type_11op_Equality_Rb_P2V4TypeV4Type")]
        [MethodAlias("_ZW6System4Type_14EqualsInternal_Rb_P2u1tV4Type")]
        [AlwaysCompile]
        static internal unsafe bool EqualsInternal(TysosType a, TysosType b)
        {
            if (CastOperations.ReinterpretAsPointer(a) == CastOperations.ReinterpretAsPointer(b))
                return true;
            if (CastOperations.ReinterpretAsPointer(a) == null)
                return false;
            if (CastOperations.ReinterpretAsPointer(b) == null)
                return false;

            void* a_vtbl = *(void**)((byte*)CastOperations.ReinterpretAsPointer(a) + ClassOperations.GetSystemTypeImplOffset());
            void* b_vtbl = *(void**)((byte*)CastOperations.ReinterpretAsPointer(b) + ClassOperations.GetSystemTypeImplOffset());

            if ((a_vtbl != null) && (a_vtbl == b_vtbl))
                return true;
            if (a_vtbl == null || b_vtbl == null)
                return false;

            void* a_ti = *(void**)a_vtbl;
            void* b_ti = *(void**)b_vtbl;
            // if either is non-generic, then the equality is false at this point
            if (*(int*)((void**)a_ti + 1) != 2 || *(int*)((void**)b_ti + 1) != 2)
                return false;

            return a.tspec.Equals(b.tspec);
        }

        [MethodAlias("_ZW6System17RuntimeTypeHandle_9CanCastTo_Rb_P2V11RuntimeTypeV11RuntimeType")]
        [AlwaysCompile]
        unsafe static bool CanCastTo(byte* from_type, byte* to_type)
        {
            return CanCast(*(void**)(from_type + ClassOperations.GetSystemTypeImplOffset()),
                *(void**)(to_type + ClassOperations.GetSystemTypeImplOffset()));
        }

        internal static unsafe bool CanCast(void *from_vtbl, void *to_vtbl)
        {
            void* from_type;
            void* to_type;

            if (from_vtbl == null)
                throw new InvalidCastException("CastClassEx: from_vtbl is null");
            from_type = *(void**)from_vtbl;
            to_type = *(void**)to_vtbl;

            if (from_type == to_type)
                return true;

            /* If both are arrays with non-null elem types, do an array-element-compatible-with
             *  (CIL I:8.7.1) comparison */
            void* from_extends = *(void**)((byte*)from_vtbl + ClassOperations.GetVtblExtendsVtblPtrOffset());
            void* to_extends = *(void**)((byte*)to_vtbl + ClassOperations.GetVtblExtendsVtblPtrOffset());
            if (from_extends == OtherOperations.GetStaticObjectAddress("_ZW6System5Array") &&
                from_extends == to_extends)
            {
                void* from_et = *(((void**)from_type) + 1);
                void* to_et = *(((void**)to_type) + 1);

                if (from_et != null && to_et != null)
                {
                    from_et = get_array_element_compatible_with_vt(from_et);
                    to_et = get_array_element_compatible_with_vt(to_et);

                    if (from_et == to_et)
                        return true;
                    else
                        return false;
                }
            }

            /* Check whether we extend the type */
            void* cur_extends_vtbl = *(void**)((byte*)from_vtbl + ClassOperations.GetVtblExtendsVtblPtrOffset());
            while (cur_extends_vtbl != null)
            {
                if (cur_extends_vtbl == to_vtbl)
                    return true;
                cur_extends_vtbl = *(void**)((byte*)cur_extends_vtbl + ClassOperations.GetVtblExtendsVtblPtrOffset());
            }

            /* Check whether we implement the type as an interface */
            void** cur_iface_ptr = *(void***)((byte*)from_vtbl + ClassOperations.GetVtblInterfacesPtrOffset());

            if (cur_iface_ptr != null)
            {
                while (*cur_iface_ptr != null)
                {
                    if (*cur_iface_ptr == to_vtbl)
                        return true;

                    cur_iface_ptr += 2;
                }
            }

            return false;
        }

        [AlwaysCompile]
        [MethodAlias("castclassex")]
        internal static unsafe void *CastClassEx(void *from_obj, void *to_vtbl)
        {
            if (from_obj == null)
            {
                return null;
            }

            if (to_vtbl == null)
                throw new InvalidCastException("CastClassEx: to_vtbl is null");

            var from_vtbl = *(void**)from_obj;
            if (CanCast(from_vtbl, to_vtbl))
                return from_obj;
            else
            {
                System.Diagnostics.Debugger.Log(0, "libsupcs", "CastClassEx failing");
                System.Diagnostics.Debugger.Log((int)from_obj, "libsupcs", "from_obj");
                System.Diagnostics.Debugger.Log((int)to_vtbl, "libsupcs", "to_vtbl");

                System.Diagnostics.Debugger.Log(0, "libsupcs", "from_vtbl: " + ((ulong)from_vtbl).ToString("X"));
                System.Diagnostics.Debugger.Log(0, "libsupcs", "to_vtbl: " + ((ulong)to_vtbl).ToString("X"));
                System.Diagnostics.Debugger.Log(0, "libsupcs", "from_type: " + JitOperations.GetNameOfAddress(from_vtbl, out var unused) ?? "unknown");
                System.Diagnostics.Debugger.Log(0, "libsupcs", "to_type: " + JitOperations.GetNameOfAddress(to_vtbl, out unused) ?? "unknown");
                System.Diagnostics.Debugger.Log(0, "libsupcs", "calling_pc: " + ((ulong)OtherOperations.GetUnwinder().UnwindOne().GetInstructionPointer()).ToString("X"));

                return null;
            }
        }

        private static unsafe void* get_array_element_compatible_with_vt(void* et)
        {
            /* If this is an enum, get underlying type */
            et = get_enum_underlying_type(et);

            /* If this is a signed integer type, return the unsigned counterpart */
            if (et == OtherOperations.GetStaticObjectAddress("_Za"))
                return OtherOperations.GetStaticObjectAddress("_Zh");
            else if (et == OtherOperations.GetStaticObjectAddress("_Zs"))
                return OtherOperations.GetStaticObjectAddress("_Zt");
            else if (et == OtherOperations.GetStaticObjectAddress("_Zc"))
                return OtherOperations.GetStaticObjectAddress("_Zt");
            else if (et == OtherOperations.GetStaticObjectAddress("_Zi"))
                return OtherOperations.GetStaticObjectAddress("_Zj");
            else if (et == OtherOperations.GetStaticObjectAddress("_Zx"))
                return OtherOperations.GetStaticObjectAddress("_Zy");
            else if (et == OtherOperations.GetStaticObjectAddress("_Zu1I"))
                return OtherOperations.GetStaticObjectAddress("_Zu1U");

            /* Else return the vtable unchanged */
            return et;
        }

        /* If this is an enum vtable, return its underlying type, else
         * return the vtable unchanged */
        private static unsafe void* get_enum_underlying_type(void* vt)
        {
            void* extends = *(void**)((byte*)vt + ClassOperations.GetVtblExtendsVtblPtrOffset());
            if (extends == OtherOperations.GetStaticObjectAddress("_ZW6System4Enum"))
            {
                void** ti = *(void***)vt;
                return *(ti + 1);
            }
            return vt;
        }

        /** <summary>Get the size of the type when it is a field in a type.  This will return the pointer size for
         * reference types and boxed value types and the size of the type for unboxed value types</summary> */
        internal virtual int GetSizeAsField()
        {
            if (IsValueType)
                return GetClassSize();
            else
                return OtherOperations.GetPointerSize();
        }

        [AlwaysCompile]
        [WeakLinkage]
        [MethodAlias("_ZW34System#2ERuntime#2EInteropServices8GCHandle_11InternalGet_Ru1O_P1u1I")]
        private static object InteropServices_GCHandle_InternalGet(IntPtr v)
        {
            return CastOperations.ReinterpretAsObject(CastOperations.ReinterpretAsPointer(v));
        }

        [AlwaysCompile]
        [WeakLinkage]
        [MethodAlias("_ZW6System17RuntimeTypeHandle_11GetGCHandle_Ru1I_P2V17RuntimeTypeHandleU34System#2ERuntime#2EInteropServices12GCHandleType")]
        private static TysosType RTH_GetGCHandle(RuntimeTypeHandle rth, int gch_type)
        {
            return ReinterpretAsType(rth.Value);
        }

        [AlwaysCompile]
        [WeakLinkage]
        [MethodAlias("_Zu1L_14CanCompareBits_Rb_P1u1O")]
        private static unsafe bool ValueType_CanCompareBits(void *o)
        {
            /* See InternalEquals for caveats */
            return true;
        }

        [AlwaysCompile]
        [WeakLinkage]
        [MethodAlias("_Zu1L_15FastEqualsCheck_Rb_P2u1Ou1O")]
        private static unsafe bool ValueType_FastEqualsCheck(void **o1, void **o2)
        {
            return ValueType_InternalEquals(o1, o2, out var fields);
        }

        [AlwaysCompile]
        [WeakLinkage]
        [MethodAlias("_Zu1L_14InternalEquals_Rb_P3u1Ou1ORu1Zu1O")]
        private static unsafe bool ValueType_InternalEquals(void** o1, void** o2, out void* fields)
        {
            /* This doesn't yet perform the required behaviour.  Currently, we just
             * perform a byte-by-byte comparison, however if any of the fields are
             * reference types we should instead run .Equals() on them.
             * 
             * If we were to pass the references in the fields array i.e.
             * [ obj1_field1, obj2_field1, obj1_field2, obj2_field2, ... ]
             * then mono would do this for us.
             * 
             * We need to check both type equality and byte-by-bye equality */

            fields = null;

            void* o1vt = *o1;
            void* o2vt = *o2;

            /* Rationalise vtables to enums to their underlying type counterparts */
            void* o1ext = *(void**)((byte*)o1vt + ClassOperations.GetVtblExtendsVtblPtrOffset());
            void* o2ext = *(void**)((byte*)o2vt + ClassOperations.GetVtblExtendsVtblPtrOffset());
            if(o1ext == OtherOperations.GetStaticObjectAddress("_ZW6System4Enum"))
            {
                void** enum_ti = *(void***)o1vt;
                o1vt = *(enum_ti + 1);
            }
            if (o2ext == OtherOperations.GetStaticObjectAddress("_ZW6System4Enum"))
            {
                void** enum_ti = *(void***)o2vt;
                o2vt = *(enum_ti + 1);
            }

            // This needs fixing for dynamic types
            if (o1vt != o2vt)
            {
                while (true) ;
                return false;
            }

            // Get type sizes
            int o1tsize = *(int*)((byte*)o1vt + ClassOperations.GetVtblTypeSizeOffset());
            int o2tsize = *(int*)((byte*)o2vt + ClassOperations.GetVtblTypeSizeOffset());

            if (o1tsize != o2tsize)
                return false;

            int header_size = ClassOperations.GetBoxedTypeDataOffset();

            byte* o1_ptr = (byte*)o1 + header_size;
            byte* o2_ptr = (byte*)o2 + header_size;

            if (MemoryOperations.MemCmp(o1_ptr, o2_ptr, o1tsize - header_size) == 0)
                return true;
            else
                return false;
        }

        [AlwaysCompile]
        [WeakLinkage]
        [MethodAlias("_Zu1L_11GetHashCode_Ri_P1u1t")]
        private static unsafe int ValueType_GetHashCode(void **o)
        {
            void* f;
            return ValueType_InternalGetHashCode(o, out f);
        }


        [AlwaysCompile]
        [WeakLinkage]
        [MethodAlias("_Zu1L_19InternalGetHashCode_Ri_P2u1ORu1Zu1O")]
        private static unsafe int ValueType_InternalGetHashCode(void** o, out void* fields)
        {
            /* This doesn't yet perform the required behaviour.  Currently, we just
             * perform a byte-by-byte hash, however if any of the fields are
             * reference types we should instead run .GetHashCode() on them.
             * 
             * If we were to pass the references in the fields array i.e.
             * [ field1, field2, ... ]
             * then mono would do this for us. */

            fields = null;

            void* ovt = *o;

            // Get type size
            int otsize = *(int*)((byte*)ovt + ClassOperations.GetVtblTypeSizeOffset());

            // Get pointer to data
            int header_size = ClassOperations.GetBoxedTypeDataOffset();

            byte* o_ptr = (byte*)o + header_size;

            // ELF hash
            uint h = 0, g;
            for(int i = 0; i < (otsize-header_size); i++)
            {
                h = (h << 4) + *o_ptr++;
                g = h & 0xf0000000;
                if(g != 0)
                {
                    h ^= g >> 24;
                }
                h &= ~g;
            }

            unchecked
            {
                return (int)h;
            }
        }

        [AlwaysCompile]
        [WeakLinkage]
        [MethodAlias("_Zu1O_15MemberwiseClone_Ru1O_P1u1t")]
        static unsafe void* MemberwiseClone(void *obj)
        {
            void* vtbl = *((void**)obj);
            int class_size = *(int*)((byte*)vtbl + ClassOperations.GetVtblTypeSizeOffset());

            void* ret = MemoryOperations.GcMalloc(class_size);
            MemoryOperations.MemCpy(ret, obj, class_size);

            // Set the mutex lock on the new object to 0
            *(void**)((byte*)ret + ClassOperations.GetMutexLockOffset()) = null;

            return ret;
        }

        // all instatiated types will return false.  Later we will subclass TysosType to return true
        //  for generic parameters types which we construct on the fly for generic type definitions
        public override bool IsGenericParameter { get { return false; } }

        [AlwaysCompile]
        [MethodAlias("_ZW6System17RuntimeTypeHandle_17IsGenericVariable_Rb_P1V11RuntimeType")]
        static internal unsafe bool RTH_IsGenericVariable(TysosType t)
        {
            return t.IsGenericParameter;
        }

        [AlwaysCompile]
        [MethodAlias("_ZW6System17RuntimeTypeHandle_11IsInterface_Rb_P1V11RuntimeType")]
        static internal unsafe bool RTH_IsInterface(void *vtbl)
        {
            void** ti = *(void***)vtbl;
            var flags = *(int*)(ti + 4);    // 4th special field is flags
            return ((flags & 0x20) == 0x20);
        }

        [AlwaysCompile]
        [MethodAlias("_ZW6System17RuntimeTypeHandle_9GetModule_RU19System#2EReflection13RuntimeModule_P1U6System11RuntimeType")]
        static internal unsafe TysosModule RTH_GetModule(TysosType t)
        {
            var aptr = (t.tspec.m.file as Metadata.BinaryInterface).b;
            return TysosModule.GetModule(aptr, t.tspec.m.AssemblyName);
        }

        [MethodAlias("_ZW6System4Type_17GetTypeFromHandle_RV4Type_P1V17RuntimeTypeHandle")]
        [AlwaysCompile]
        static internal unsafe TysosType Type_GetTypeFromHandle(RuntimeTypeHandle rth)
        {
            return ReinterpretAsType(rth.Value);
        }

        [AlwaysCompile]
        [MethodAlias("__type_from_vtbl")]
        static internal unsafe TysosType internal_from_vtbl(void *vtbl)
        {
            /* The default value for a type info is:
             * 
             * intptr type (= 0)
             * intptr enum_underlying_type_vtbl_ptr
             * intptr tysostype pointer
             * intptr metadata reference count
             * metadata references
             * signature
             * 
             */

            void* ti = *(void**)vtbl;

            void** ttptr = (void**)ti + 2;
            
            if(*ttptr == null)
            {
                var tt = CastOperations.ReinterpretAsPointer(new TysosType(vtbl));
                OtherOperations.CompareExchange(ttptr, tt);
            }

            return ReinterpretAsType(*ttptr);
        }

        // From CoreCLR
        internal enum TypeNameFormatFlags
        {
            FormatBasic = 0x00000000, // Not a bitmask, simply the tersest flag settings possible
            FormatNamespace = 0x00000001, // Include namespace and/or enclosing class names in type names
            FormatFullInst = 0x00000002, // Include namespace and assembly in generic types (regardless of other flag settings)
            FormatAssembly = 0x00000004, // Include assembly display name in type names
            FormatSignature = 0x00000008, // Include signature in method names
            FormatNoVersion = 0x00000010, // Suppress version and culture information in all assembly names
#if _DEBUG
        FormatDebug = 0x00000020, // For debug printing of types only
#endif
            FormatAngleBrackets = 0x00000040, // Whether generic types are C<T> or C[T]
            FormatStubInfo = 0x00000080, // Include stub info like {unbox-stub}
            FormatGenericParam = 0x00000100, // Use !name and !!name for generic type and method parameters

            // If we want to be able to distinguish between overloads whose parameter types have the same name but come from different assemblies,
            // we can add FormatAssembly | FormatNoVersion to FormatSerialization. But we are omitting it because it is not a useful scenario
            // and including the assembly name will normally increase the size of the serialized data and also decrease the performance.
            FormatSerialization = FormatNamespace |
                                  FormatGenericParam |
                                  FormatFullInst
        }

        [MethodAlias("_ZW6System17RuntimeTypeHandle_13ConstructName_Rv_P3V17RuntimeTypeHandleV19TypeNameFormatFlagsU35System#2ERuntime#2ECompilerServices19StringHandleOnStack")]
        [AlwaysCompile]
        static void ConstructName(RuntimeTypeHandle rth, TypeNameFormatFlags formatFlags, ref string ret)
        {
            var tt = ReinterpretAsType(rth.Value);
            ret = tt.ConstructName(formatFlags);
        }

        string ConstructName(TypeNameFormatFlags formatFlags)
        {
            // Very basic implementation - does not handle most flags
            StringBuilder sb = new StringBuilder();

            if((formatFlags & TypeNameFormatFlags.FormatNamespace) == TypeNameFormatFlags.FormatNamespace)
            {
                sb.Append(Namespace);
                sb.Append(".");
            }

            sb.Append(Name);

            return sb.ToString();
        }

        [MethodAlias("_ZW34System#2ERuntime#2EInteropServices7Marshal_37GetDelegateForFunctionPointerInternal_RU6System8Delegate_P2u1IV4Type")]
        [AlwaysCompile]
        static void* Marshal_GetDelegateForFunctionPointer(void *ptr, TysosType t)
        {
            // build new object of the appropriate size
            var ret = MemoryOperations.GcMalloc(t.GetClassSize());

            // extract vtbl and write it to the appropriate field
            *(void**)ret = t._impl;

            // mutex lock
            *(int*)((byte*)ret + ClassOperations.GetMutexLockOffset()) = 0;

            // insert the function pointer
            *(void**)((byte*)ret + ClassOperations.GetDelegateFPtrOffset()) = ptr;

            return ret;
        }
    }
}
