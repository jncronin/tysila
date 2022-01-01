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

/* This defines the TysosMethod which is a subtype of System.Reflection.MethodInfo
 * 
 * All MethodInfo structures produced by tysila2 follow this layout
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

namespace libsupcs
{
    [ExtendsOverride("_ZW19System#2EReflection17RuntimeMethodInfo")]
    [VTableAlias("__tysos_method_vt")]
    public unsafe class TysosMethod : System.Reflection.MethodInfo
    {
        public TysosType OwningType;
        public TysosType _ReturnType;
        public bool returns_void;
        [NullTerminatedListOf(typeof(TysosType))]
        public TysosType[] _ParamTypes;
        public TysosParameterInfo[] _Params;
        public string _Name;
        public string _MangledName;
        public IntPtr Signature;
        public IntPtr Sig_references;
        public Int32 Flags;
        public Int32 ImplFlags;
        public UInt32 TysosFlags;
        public void* MethodAddress;
        [NullTerminatedListOf(typeof(EHClause))]
        public IntPtr EHClauses;
        public IntPtr Instructions;

        public const string PureVirtualName = "__cxa_pure_virtual";

        public metadata.MethodSpec mspec;

        public TysosMethod(metadata.MethodSpec ms, TysosType owning_type)
        {
            mspec = ms;
            OwningType = owning_type;
        }

        public const UInt32 TF_X86_ISR = 0x10000001;
        public const UInt32 TF_X86_ISREC = 0x10000002;

        public const UInt32 TF_CC_STANDARD = 1;
        public const UInt32 TF_CC_VARARGS = 2;
        public const UInt32 TF_CC_HASTHIS = 32;
        public const UInt32 TF_CC_EXPLICITTHIS = 64;
        public const UInt32 TF_CC_MASK = 0x7f;

        [VTableAlias("__tysos_ehclause_vt")]
        public class EHClause
        {
            public IntPtr TryStart;
            public IntPtr TryEnd;
            public IntPtr Handler;
            public TysosType CatchObject;
            public Int32 Flags;

            public bool IsFinally { get { if ((Flags & 0x2) == 0x2) return true; return false; } }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        [MethodReferenceAlias("__invoke")]
        static extern void* InternalInvoke(void* maddr, int pcnt, void** parameters, void** types, void* ret_vtbl, uint flags);

        internal const uint invoke_flag_instance = 1U;
        internal const uint invoke_flag_vt = 2U;
        internal const uint invoke_flag_vt_ret = 4U;


        public override System.Reflection.CallingConventions CallingConvention
        {
            get
            {
                return (System.Reflection.CallingConventions)(int)(TysosFlags & TF_CC_MASK);
            }
        }

        public override System.Reflection.MethodInfo GetBaseDefinition()
        {
            throw new NotImplementedException();
        }

        public override System.Reflection.ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get { throw new NotImplementedException(); }
        }

        public override System.Reflection.MethodAttributes Attributes
        {
            get
            {
                return (System.Reflection.MethodAttributes)mspec.m.GetIntEntry(metadata.MetadataStream.tid_MethodDef, mspec.mdrow, 2);
            }
        }

        public override System.Reflection.MethodImplAttributes GetMethodImplementationFlags()
        {
            return (System.Reflection.MethodImplAttributes)mspec.m.GetIntEntry(metadata.MetadataStream.tid_MethodDef, mspec.mdrow, 1);
        }

        public unsafe override System.Reflection.ParameterInfo[] GetParameters()
        {
            if (_Params == null)
            {
                var pc = mspec.m.GetMethodDefSigParamCount(mspec.msig);
                var rt_idx = mspec.m.GetMethodDefSigRetTypeIndex(mspec.msig);
                mspec.m.GetTypeSpec(ref rt_idx, mspec.gtparams, mspec.gmparams);

                var _params = new TysosParameterInfo[pc];
                var _ptypes = new TysosType[pc];

                for (int i = 0; i < pc; i++)
                {
                    var tspec = mspec.m.GetTypeSpec(ref rt_idx, mspec.gtparams, mspec.gmparams);
                    TysosType tt = tspec;
                    _ptypes[i] = tt;

                    TysosParameterInfo tpi = new TysosParameterInfo(tt, i, this);
                    _params[i] = tpi;
                }

                if (System.Threading.Interlocked.CompareExchange(ref _Params, _params, null) == null)
                {
                    _ParamTypes = _ptypes;
                }
            }

            return _Params;
        }

        public override Type ReturnType
        {
            get
            {
                if (_ReturnType == null && !returns_void)
                {
                    var rtspec = mspec.ReturnType;
                    TysosType _rt;
                    if (rtspec == null)
                    {
                        _rt = null;
                        returns_void = true;
                    }
                    else
                        _rt = rtspec;

                    System.Threading.Interlocked.CompareExchange(ref _ReturnType, _rt, null);
                }
                return _ReturnType;
            }
        }

        public override object Invoke(object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object[] parameters, System.Globalization.CultureInfo culture)
        {
            uint flags = 0;

            if (MethodAddress == null)
            {
                var mangled_name = mspec.MangleMethod();
                System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosMethod.Invoke: requesting run-time address for " + mangled_name);
                MethodAddress = JitOperations.GetAddressOfObject(mspec.MangleMethod());
                if (MethodAddress == null)
                {
                    System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosMethod.Invoke: jit compiling method");
                    MethodAddress = JitOperations.JitCompile(this.mspec);
                }
            }
            if (MethodAddress == null)
                throw new System.Reflection.TargetException("Method does not have a defined implementation (" + OwningType.FullName + "." + Name + "())");
            if (!IsStatic && (obj == null))
                throw new System.Reflection.TargetException("Instance method and obj is null (" + OwningType.FullName + "." + Name + "())");

            // TODO: check number and type of parameters is what the method expects

            // Get total number of parameters
            int p_length = 0;
            if (parameters != null)
                p_length = parameters.Length;
            if (!IsStatic)
                p_length++;

            // See InternalStrCpy for the rationale here
            int max_stack_alloc = p_length > 512 ? 512 : p_length;
            IntPtr* pstack = stackalloc IntPtr[max_stack_alloc];
            IntPtr* tstack = stackalloc IntPtr[max_stack_alloc];

            void** ps, ts;
            if (max_stack_alloc <= 512)
            {
                ps = (void**)pstack;
                ts = (void**)tstack;
            }
            else
            {
                ps = (void**)MemoryOperations.GcMalloc(p_length * sizeof(void*));
                ts = (void**)MemoryOperations.GcMalloc(p_length * sizeof(void*));
            }

            // Build a new params array to include obj if necessary, and a tysos type array
            int curptr = 0;
            if (!IsStatic)
            {
                ps[0] = CastOperations.ReinterpretAsPointer(obj);
                ts[0] = OtherOperations.GetStaticObjectAddress("_Zu1O");
                curptr++;
            }
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++, curptr++)
                {
                    var cp = CastOperations.ReinterpretAsPointer(parameters[i]);
                    ps[curptr] = cp;
                    ts[curptr] = *(void**)cp;
                }
            }

            if (!IsStatic)
                flags |= invoke_flag_instance;
            if (OwningType.IsValueType)
                flags |= invoke_flag_vt;
            if (ReturnType != null && ReturnType.IsValueType)
                flags |= invoke_flag_vt_ret;

            return CastOperations.ReinterpretAsObject(InternalInvoke(MethodAddress, p_length, ps, ts,
                (ReturnType != null) ? TysosType.ReinterpretAsType(ReturnType)._impl : null, flags));
        }

        /** <summary>Override this in you application to pass Invokes across thread boundaries</summary> */
        [AlwaysCompile]
        [WeakLinkage]
        [MethodAlias("invoke")]
        public static void* Invoke(void* mptr, object[] args, void* rtype, uint flags)
        {
            return InternalInvoke(mptr, args, rtype, flags);
        }

        public static void* InternalInvoke(void* mptr, object[] args, void* rtype, uint flags)
        {
            int p_length = (args == null) ? 0 : args.Length;

            // See InternalStrCpy for the rationale here
            int max_stack_alloc = p_length > 512 ? 512 : p_length;
            IntPtr* pstack = stackalloc IntPtr[max_stack_alloc];
            IntPtr* tstack = stackalloc IntPtr[max_stack_alloc];

            void** ps, ts;
            if (max_stack_alloc <= 512)
            {
                ps = (void**)pstack;
                ts = (void**)tstack;
            }
            else
            {
                ps = (void**)MemoryOperations.GcMalloc(p_length * sizeof(void*));
                ts = (void**)MemoryOperations.GcMalloc(p_length * sizeof(void*));
            }

            // Build a new params array and a tysos type array
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    var cp = CastOperations.ReinterpretAsPointer(args[i]);
                    ps[i] = cp;
                    ts[i] = *(void**)cp;
                }
            }

            return InternalInvoke(mptr, args.Length, ps, ts, rtype, flags);
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get { throw new NotImplementedException(); }
        }

        public override Type DeclaringType
        {
            get { return OwningType; }
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
            throw new NotImplementedException();
        }

        public override string Name
        {
            get
            {
                if(_Name == null)
                {
                    string name = mspec.m.GetStringEntry(metadata.MetadataStream.tid_MethodDef, mspec.mdrow, 3);
                    System.Threading.Interlocked.CompareExchange(ref _Name, name, null);
                }
                return _Name;
            }
        }

        public override Type ReflectedType
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class TysosParameterInfo : System.Reflection.ParameterInfo
    {
        internal TysosParameterInfo(Type param_type, int param_no, TysosMethod decl_method)
        {
            /* This is a basic implementation: tysila does not currently provide either the
             * parameter name or its attributes in the _Params list */

            this.ClassImpl = param_type;
            this.PositionImpl = param_no;
            this.NameImpl = param_no.ToString();
            this.MemberImpl = decl_method;
            this.DefaultValueImpl = null;
            this.AttrsImpl = System.Reflection.ParameterAttributes.None;
        }
    }
}
