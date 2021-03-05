/* Copyright (C) 2008 - 2013 by John Cronin
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

using System.Runtime.CompilerServices;
namespace libsupcs
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class SpecialTypeAttribute : System.Attribute
    { }

    /** <summary>This attribute is for use by the CMExpLib library to identify certain entries in TysosType structures etc which
     * point to a null-terminated list of items of type 'type' (e.g. the Method list)</summary> */
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public sealed class NullTerminatedListOfAttribute : System.Attribute
    {
        public NullTerminatedListOfAttribute(System.Type type) { }
    }

    /** <summary>Apply to an interface or method to have the call changed to one to __invoke(void *mptr, object[] params) </summary> */
    [System.AttributeUsage(System.AttributeTargets.Interface | System.AttributeTargets.Method, AllowMultiple = false)]
    public sealed class AlwaysInvokeAttribute : System.Attribute
    { }

    /** <summary>Have the class or method only be compiled for a particular architecture</summary> */
    [global::System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class ArchDependentAttribute : System.Attribute
    {
        public ArchDependentAttribute(string arch) { }
    }

    /** <summary>Have the class or method only be compiled for a particular OS</summary> */
    [global::System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class OSDependentAttribute : System.Attribute
    {
        public OSDependentAttribute(string os) { }
    }

    /** <summary>Have the class or method only be compiled for 64-bit targets</summary> */
    [global::System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class Bits64OnlyAttribute : System.Attribute
    {
        public Bits64OnlyAttribute() { }
    }

    /** <summary>Have the class or method only be compiled for 32-bit targets</summary> */
    [global::System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class Bits32OnlyAttribute : System.Attribute
    {
        public Bits32OnlyAttribute() { }
    }

    /** <summary>This marks a structure that is used to pass register contents to an interrupt handler</summary> */
    [global::System.AttributeUsage(System.AttributeTargets.Struct)]
    public sealed class InterruptRegisterStructureAttribute : System.Attribute
    {
        public InterruptRegisterStructureAttribute() { }
    }

    /** <summary>Any calls to this method (declared as extern without a body in C#) will instead call the following label</summary>
     */
    [global::System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class MethodReferenceAliasAttribute : System.Attribute
    {
        public MethodReferenceAliasAttribute(string alias) { }
    }

    /** <summary>Give this field a separate alias to allow easy access for unmanaged code.  Only applicable to static fields (ignored for instance fields).</summary> 
     */
    [global::System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class FieldAliasAttribute : System.Attribute
    {
        public FieldAliasAttribute(string alias) { }
    }

    /** <summary>Any references to this field (declared as a static field) will instead reference the following label</summary>
     */
    [global::System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class FieldReferenceAliasAttribute : System.Attribute
    {
        public FieldReferenceAliasAttribute(string alias) { }
    }

    /** <summary>Any references to this field (declared as a static field) will instead reference the address of following label (don't try to write to it)</summary>
     */
    [global::System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class FieldReferenceAliasAddressAttribute : System.Attribute
    {
        public FieldReferenceAliasAddressAttribute(string alias) { }
    }

    /** <summary>Mark the method as a 'ReinterpretAs' method that performs a cast without type checking</summary> */
    [global::System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class ReinterpretAsMethodAttribute : System.Attribute
    {
        public ReinterpretAsMethodAttribute() { }
    }

    /** <summary>Generate another symbol name for the given method</summary> */
    [global::System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class MethodAliasAttribute : System.Attribute
    {
        public MethodAliasAttribute(string alias) { }
    }

    /** <summary>Override the calling convention used by a particular method</summary> */
    [global::System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class CallingConventionAttribute : System.Attribute
    {
        public CallingConventionAttribute(string callconv) { }
    }

    /** <summary>Mark the method to have weak linkage</summary> */
    [global::System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class WeakLinkageAttribute : System.Attribute
    {
        public WeakLinkageAttribute() { }
    }

    [global::System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class ExtraArgumentAttribute : System.Attribute
    {
        public ExtraArgumentAttribute(int arg_no, int base_type) { }
    }

    [global::System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class IgnoreImplementationAttribute : System.Attribute
    { }

    [global::System.AttributeUsage(System.AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class AlwaysCompileAttribute : System.Attribute
    { }

    [global::System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class SyscallAttribute : System.Attribute
    { }

    [global::System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Enum | System.AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class OutputCHeaderAttribute : System.Attribute
    { }

    [global::System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class ProfileAttribute : System.Attribute
    {
        public ProfileAttribute(bool profile) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class VTableAliasAttribute : System.Attribute
    {
        public VTableAliasAttribute(string alias) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class TypeInfoAliasAttribute : System.Attribute
    {
        public TypeInfoAliasAttribute(string alias) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ExtendsOverrideAttribute : System.Attribute
    {
        public ExtendsOverrideAttribute(string extends) { }
    }

    /** <summary> Marks the class as having no base class (not even System.Object)</summary> */
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class NoBaseClassAttribute : System.Attribute
    {
        public NoBaseClassAttribute() { }
    }

    public class MemoryOperations
    {
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        public static extern byte PeekU1(System.UIntPtr addr);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        public static extern ushort PeekU2(System.UIntPtr addr);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        public static extern uint PeekU4(System.UIntPtr addr);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        public static extern ulong PeekU8(System.UIntPtr addr);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        public static extern void Poke(System.UIntPtr addr, byte b);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        public static extern void Poke(System.UIntPtr addr, ushort v);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        public static extern void Poke(System.UIntPtr addr, uint v);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        public static extern void Poke(System.UIntPtr addr, ulong v);

        [Bits64Only]
        [WeakLinkage]
        public static void QuickClearAligned16(ulong addr, ulong size)
        {
            unsafe
            {
                MemSet((void*)addr, 0, (int)size);
            }
        }

        [MethodReferenceAlias("memset")]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static unsafe extern void* MemSet(void* s, int c, int size);

        [MethodReferenceAlias("memcpy")]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static unsafe extern void* MemCpy(void* dest, void* src, int size);

        [MethodReferenceAlias("memcmp")]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static unsafe extern int MemCmp(void* s1, void* s2, int size);

        [MethodReferenceAlias("memmove")]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static unsafe extern void* MemMove(void* dest, void* src, int size);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public unsafe static extern void * GetInternalArray(System.Array array);

        [MethodReferenceAlias("gcmalloc")]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern object GcMalloc(System.IntPtr size);

        [MethodReferenceAlias("gcmalloc")]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static unsafe extern void* GcMalloc(int size);
    }

    public class IoOperations
    {
        [ArchDependent("x86_64")]
        [ArchDependent("x86")]
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        public static extern void PortOut(ushort port, byte v);

        [ArchDependent("x86_64")]
        [ArchDependent("x86")]
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        public static extern void PortOut(ushort port, ushort v);

        [ArchDependent("x86_64")]
        [ArchDependent("x86")]
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        public static extern void PortOut(ushort port, uint v);

        [ArchDependent("x86_64")]
        [ArchDependent("x86")]
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        public static extern byte PortInb(ushort port);

        [ArchDependent("x86_64")]
        [ArchDependent("x86")]
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        public static extern ushort PortInw(ushort port);

        [ArchDependent("x86_64")]
        [ArchDependent("x86")]
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        public static extern uint PortInd(ushort port);
    }

    public class CastOperations
    {
        [Bits64Only]
        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern ulong ReinterpretAsUlong(object o);

        [Bits32Only]
        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern uint ReinterpretAsUInt(object o);

        [Bits64Only]
        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern object ReinterpretAsObject(ulong addr);

        [Bits32Only]
        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern object ReinterpretAsObject(uint addr);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public unsafe static extern object ReinterpretAsObject(void* addr);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public unsafe static extern string ReinterpretAsString(void* addr);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern System.UIntPtr ReinterpretAsUIntPtr(object o);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern System.IntPtr ReinterpretAsIntPtr(object o);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public unsafe static extern void* ReinterpretAsPointer(object o);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public unsafe static extern void* ReinterpretAsPointer(System.IntPtr o);

        [Bits64Only]
        [MethodImpl(MethodImplOptions.InternalCall)]
        [IgnoreImplementation]
        public static extern ulong GetArg0U8();
    }

    public class ClassOperations
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetTypedReferenceTypeOffset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetTypedReferenceValueOffset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetMutexLockOffset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetVtblFieldOffset();

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //public static extern int GetObjectIdFieldOffset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetVtblTypeInfoPtrOffset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetVtblInterfacesPtrOffset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetVtblExtendsVtblPtrOffset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetVtblTypeSizeOffset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetVtblFlagsOffset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetVtblBaseTypeVtblOffset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetVtblTargetFieldsOffset();


        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetBoxedTypeDataOffset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetSystemTypeImplOffset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        [IgnoreImplementation]
        public extern static int GetFieldOffset(string typename, string field);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [IgnoreImplementation]
        public extern static int GetStaticFieldOffset(string typename, string field);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetDelegateFPtrOffset();
    }

    public unsafe class JitOperations
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [MethodReferenceAlias("jit_tm")]
        public static extern void* JitCompile(TysosMethod m);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [MethodReferenceAlias("jit_vtable")]
        public static extern void* JitCompile(metadata.TypeSpec ts);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [MethodReferenceAlias("jit_addrof")]
        public static extern void* GetAddressOfObject(string name);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [MethodReferenceAlias("jit_nameof")]
        public static extern string GetNameOfAddress(void* addr, out void* offset);
    }

    public class OtherOperations
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Halt();

        [MethodImpl(MethodImplOptions.InternalCall)]
        [IgnoreImplementation]
        public static extern void Exit();

        [Bits64Only]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static void CallI(ulong address);

        [Bits32Only]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static void CallI(uint address);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static void CallI(System.UIntPtr address);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public unsafe extern static void CallI(void *address);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public unsafe extern static void CallI(void* obj, void* address);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public unsafe extern static T CallI<T>(void* address);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [IgnoreImplementation]
        public extern static int GetUsedStackSize();

        [MethodImpl(MethodImplOptions.InternalCall)]
        [IgnoreImplementation]
        public unsafe extern static void* GetStaticObjectAddress(string name);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [IgnoreImplementation]
        public unsafe extern static void* GetFunctionAddress(string name);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static int GetPointerSize();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static System.UIntPtr Add(System.UIntPtr a, System.UIntPtr b);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static System.IntPtr Add(System.IntPtr a, System.IntPtr b);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static System.UIntPtr Sub(System.UIntPtr a, System.UIntPtr b);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static System.IntPtr Sub(System.IntPtr a, System.IntPtr b);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static System.UIntPtr Mul(System.UIntPtr a, System.UIntPtr b);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static System.IntPtr Mul(System.IntPtr a, System.IntPtr b);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public unsafe static extern void* GetReturnAddress();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public unsafe static extern void Spinlock(void* ptr);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public unsafe static extern void Spinunlock(void* ptr);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public unsafe static extern byte SyncValCompareAndSwap(byte* ptr, byte oldval, byte newval);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public unsafe static extern ushort SyncValCompareAndSwap(ushort* ptr, ushort oldval, ushort newval);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public unsafe static extern short SyncValCompareAndSwap(short* ptr, short oldval, short newval);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public unsafe static extern uint SyncValCompareAndSwap(uint* ptr, uint oldval, uint newval);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public unsafe static extern int SyncValCompareAndSwap(int* ptr, int oldval, int newval);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public unsafe static extern void SpinlockHint();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void AsmBreakpoint();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern System.IntPtr EnterUninterruptibleSection();
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void ExitUninterruptibleSection(System.IntPtr state);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public unsafe static extern void* CompareExchange(void** addr, void* value, void* comparand = null);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [MethodReferenceAlias("__get_unwinder")]
        public static extern Unwinder GetUnwinder();
    }

    public class ArrayOperations
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static unsafe extern int GetRankOffset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static unsafe extern int GetElemSizeOffset();

        /** <summary>Get the number of items in the inner array (multiply by elemsize to get the byte length) */
        //[MethodImpl(MethodImplOptions.InternalCall)]
        //public static unsafe extern int GetInnerArrayLengthOffset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static unsafe extern int GetElemTypeOffset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static unsafe extern int GetLoboundsOffset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static unsafe extern int GetSizesOffset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static unsafe extern int GetInnerArrayOffset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetArrayClassSize();
    }

    public class StringOperations
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static unsafe extern int GetLengthOffset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static unsafe extern int GetDataOffset();

        internal static unsafe char* GetChars(string s)
        {
            byte* s2 = (byte*)CastOperations.ReinterpretAsPointer(s);
            return (char*)(s2 + GetDataOffset());
        }
    }
}
