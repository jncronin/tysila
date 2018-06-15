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

/* This defines the TysosAssembly which is a subtype of System.Reflection.Assembly
 * 
 * All AssemblyInfo structures produced by tysila2 follow this layout
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace libsupcs
{
    /* System.Reflection.Assembly defines an internal constructor, so we cannot subclass
     * it directly outside of corlib therefore we need to use the following attribute */
    [ExtendsOverride("_ZW19System#2EReflection15RuntimeAssembly")]
    [VTableAlias("__tysos_assembly_vt")]
    public unsafe class TysosAssembly : System.Reflection.Assembly
    {
        TysosModule m;
        string assemblyName;

        internal TysosAssembly(TysosModule mod, string ass_name)
        {
            m = mod;
            assemblyName = ass_name;
        }

        [MethodAlias("_ZW19System#2EReflection8Assembly_8GetTypes_Ru1ZU6System4Type_P2u1tb")]
        System.Type[] GetTypes(bool exportedOnly)
        {
            throw new NotImplementedException();
        }

        [AlwaysCompile]
        [MethodAlias("_ZW19System#2EReflection8Assembly_27GetManifestResourceInternal_Ru1I_P4u1tu1SRiRV6Module")]
        static void* GetManifestResource(TysosAssembly ass, string name, out int size, out System.Reflection.Module mod)
        {
            // this always fails for now
            System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosAssembly: GetManifestResource(" + ass.assemblyName + ", " + name + ", out int size, out Module mod) called");
            size = 0;
            mod = null;
            return null;
        }

        public override string FullName => assemblyName;

        struct StackCrawlMarkHandle { internal void** ptr; }

        [AlwaysCompile]
        [MethodAlias("_ZW19System#2EReflection15RuntimeAssembly_20GetExecutingAssembly_Rv_P2U35System#2ERuntime#2ECompilerServices20StackCrawlMarkHandleV19ObjectHandleOnStack")]
        static void GetExecutingAssembly(StackCrawlMarkHandle scmh, TysosModule.ObjectHandleOnStack ret)
        {
            int scm = *(int*)scmh.ptr;

            System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosAssembly.GetExecutingAssembly: scm: " + scm.ToString());

            Unwinder u = OtherOperations.GetUnwinder();
            u.UnwindOne();
            u.UnwindOne();      // we are double-nested within coreclr so unwind this and calling method (GetExecutingAssembly(ref StackMarkHandle)) first

            switch(scm)
            {
                case 0:
                    break;
                case 1:
                    u.UnwindOne();
                    break;
                case 2:
                    u.UnwindOne();
                    u.UnwindOne();
                    break;
                default:
                    System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosAssembly.GetExecutingAssembly: unsupported scm: " + scm.ToString());
                    throw new NotSupportedException();
            }

            System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosAssembly.GetExecutingAssembly: requested pc " + ((ulong)u.GetInstructionPointer()).ToString("X"));

            void* offset;
            var name = JitOperations.GetNameOfAddress((void*)u.GetInstructionPointer(), out offset);

            if(name == null)
            {
                System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosAssembly.GetExecutingAssembly: symbol not found");
                *ret.ptr = null;
                return;
            }

            System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosAssembly.GetExecutingAssembly: found method " + name);

            var ts = Metadata.MSCorlib.DemangleObject(name);

            if(ts == null)
            {
                System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosAssembly.GetExecutingAssembly: demangler returned null");
                *ret.ptr = null;
                return;
            }
            var m = ts.Metadata;
            if (m == null)
            {
                System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosAssembly.GetExecutingAssembly: returned ts had no assembly");
                *ret.ptr = null;
                return;
            }
            var aptr = (m.file as Metadata.BinaryInterface).b;
            var retm = TysosModule.GetModule(aptr, m.AssemblyName);
            System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosAssembly.GetExecutingAssembly: returning " + retm.ass.assemblyName);
            *ret.ptr = CastOperations.ReinterpretAsPointer(retm.ass);
        }

        [MethodAlias("_ZW19System#2EReflection15RuntimeAssembly_11GetResource_RPh_P5V15RuntimeAssemblyu1SRyU35System#2ERuntime#2ECompilerServices20StackCrawlMarkHandleb")]
        [AlwaysCompile]
        static byte* GetResource(TysosAssembly ass, string resourceName, out ulong length, StackCrawlMarkHandle stackMark,
            bool skipSecurityCheck)
        {
            System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosAssembly.GetResource(" + ass.assemblyName + ", " + resourceName + ", out ulong length, " +
                "StackCrawlMarkHandle stackMark, bool skipSecurityCheck) called");

            var res_addr = JitOperations.GetAddressOfObject(ass.assemblyName + "_resources");
            if(res_addr == null)
            {
                System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosAssembly.GetResource: cannot find " + ass.assemblyName + "_resources");
                length = 0;
                return null;
            }

            uint* ret = (uint*)res_addr;

            // length is the first int32
            length = *ret++;

            System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosAssembly.GetResource: returning: " + ((ulong)ret).ToString("X") +
                ", length: " + length.ToString("X"));

            return (byte*)ret;
        }

        [AlwaysCompile]
        [MethodAlias("_ZW19System#2EReflection12AssemblyName_5nInit_Rv_P4u1tRV15RuntimeAssemblybb")]
        static unsafe void AssemblyName_nInit(byte *obj, out TysosAssembly assembly, bool forIntrospection, bool raiseResolveEvent)
        {
            string name = CastOperations.ReinterpretAsString(*(void**)(obj + ClassOperations.GetFieldOffset("_ZW19System#2EReflection12AssemblyName", "_Name")));
            System.Diagnostics.Debugger.Log(0, "libsupcs", "AssemblyName_nInit(" + name + ", out TysosAssembly, bool, bool) called");

            // split assembly name off from other fields
            int comma = name.IndexOf(',');
            string Name;
            if (comma != -1)
                Name = name.Substring(0, comma);
            else
                Name = name;

            if (Name.Equals("System.Private.CoreLib"))
                Name = "mscorlib";

            *(void**)(obj + ClassOperations.GetFieldOffset("_ZW19System#2EReflection12AssemblyName", "_Name")) =
                CastOperations.ReinterpretAsPointer(Name);

            System.Diagnostics.Debugger.Log(0, "libsupcs", "AssemblyName_nInit - setting _Name to " + Name);

            assembly = null;
        }

        [AlwaysCompile]
        [MethodAlias("_ZW19System#2EReflection12AssemblyName_18nGetPublicKeyToken_Ru1Zh_P1u1t")]
        static unsafe byte[] AssemblyName_nGetPublicKeyToken(byte *obj)
        {
            return null;
        }
    }

    [VTableAlias("__tysos_module_vt")]
    [ExtendsOverride("_ZW19System#2EReflection13RuntimeModule")]
    public unsafe class TysosModule
    {
        internal void* aptr;    /* pointer to assembly */
        long compile_time;
        public DateTime CompileTime { get { return new DateTime(compile_time); } }
        internal TysosAssembly ass;

        internal TysosModule(void *_aptr, string ass_name)
        {
            aptr = _aptr;
            ass = new TysosAssembly(this, ass_name);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern TysosModule ReinterpretAsTysosModule(System.Reflection.Module module);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        internal static extern Module ReinterpretAsModule(object o);

        internal metadata.MetadataStream m { get { return Metadata.BAL.GetAssembly(aptr); } }

        unsafe internal struct ObjectHandleOnStack { internal void** ptr; }

        [MethodAlias("_ZW6System12ModuleHandle_13GetModuleType_Rv_P2U19System#2EReflection13RuntimeModuleU35System#2ERuntime#2ECompilerServices19ObjectHandleOnStack")]
        [AlwaysCompile]
        static void ModuleHandle_GetModuleType(TysosModule mod, ObjectHandleOnStack oh)
        {
            // Get the <Module> type name for the current module.  This is always defined as tdrow 1.
            var ts = (TysosType)mod.m.GetTypeSpec(metadata.MetadataStream.tid_TypeDef, 1);
            *oh.ptr = CastOperations.ReinterpretAsPointer(ts);
        }

        static Dictionary<ulong, TysosModule> mod_cache = new Dictionary<ulong, TysosModule>(new metadata.GenericEqualityComparer<ulong>());

        internal TysosAssembly GetAssembly()
        {
            return ass;
        }

        static internal TysosModule GetModule(void *aptr, string ass_name)
        {
            var mfile = CastOperations.ReinterpretAsUlong(CastOperations.ReinterpretAsObject(aptr));
            lock (mod_cache)
            {
                if (!mod_cache.TryGetValue(mfile, out var ret))
                {
                    ret = new TysosModule(aptr, ass_name);
                    mod_cache[mfile] = ret;

                    System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosType: building new TysosModule for " + ass_name);
                }
                return ret;
            }
        }
    }
}
