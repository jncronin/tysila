/* Copyright (C) 2015 by John Cronin
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
using System.Runtime.CompilerServices;

namespace libsupcs.x86_64
{
    [ArchDependent("x86_64")]
    class x86_64_invoke
    {
        [MethodReferenceAlias("__x86_64_invoke")]
        [MethodImpl(MethodImplOptions.InternalCall)]
        static unsafe extern void* asm_invoke(void *maddr, int p_length,
            void* parameters, void* plocs);

        [MethodAlias("__invoke")]
        [AlwaysCompile]
        [Bits64Only]
        static unsafe void* InternalInvoke(void* maddr, int pcnt, void **parameters, void **types, TysosMethod meth)
        {
            /* Modify the types array to contain the call locations of each parameter
             * 
             * 0 - INTEGER (pass as-is)
             * 1 - INTEGER (unbox in asm)
             * 2 - SSE (unbox in asm)
             * 3 - MEMORY (upper 24 bits give length of object)
             * 4 - INTEGER (unbox low 32 bits in asm)
             * 5 - INTEGER (unbox to byref in asm)
             */

            for(int i = 0; i < pcnt; i++)
            {
                // handle this pointer
                if(i == 0 && !meth.IsStatic)
                {
                    if (meth.OwningType.IsValueType)
                    {
                        // we need to unbox the this pointer to a managed pointer
                        types[i] = (void*)5;
                    }
                    else
                    {
                        types[i] = (void*)0;
                    }
                }
                else
                {
                    // the type we need is encoded in the vtable
                    var vtbl = types[i];
                    var cur_class = *((byte*)vtbl + 0x1 + ClassOperations.GetVtblTargetFieldsOffset());
                    types[i] = (void*)cur_class;
                }
            }

            var ret = asm_invoke(maddr, pcnt, parameters, types);

            var rettype = meth.ReturnType;

            // See if we have to box the return type
            if (rettype != null && rettype.IsValueType)
            {
                // Get the size of the return type
                var tsize = meth._ReturnType.GetClassSize() - ClassOperations.GetBoxedTypeDataOffset();     // GetClassSize always returns boxed size
                
                /* TODO: handle VTypes that don't fit in a register */
                if(tsize > 8)
                    throw new NotImplementedException("InternalInvoke: return type " + rettype.FullName + " ( " +
                        CastOperations.ReinterpretAsUlong(rettype).ToString("X") + " (not supported (size " +
                        tsize.ToString() + ")");

                // Build a new boxed version of the type
                var obj = (void**)MemoryOperations.GcMalloc(tsize + ClassOperations.GetBoxedTypeDataOffset());
                *obj = meth._ReturnType._impl;
                *(int*)((byte*)obj + ClassOperations.GetMutexLockOffset()) = 0;

                System.Diagnostics.Debugger.Log(0, "libsupcs", "x86_64_invoke: returning boxed " + rettype.FullName + " of size " + tsize.ToString());

                if (tsize > 4)
                    *(long*)((byte*)obj + ClassOperations.GetBoxedTypeDataOffset()) = (long)ret;
                else
                    *(int*)((byte*)obj + ClassOperations.GetBoxedTypeDataOffset()) = (int)((long)ret & 0xffffffff);

                return obj;
            }
            else if (rettype == null)
            {
                System.Diagnostics.Debugger.Log(0, "libsupcs", "x86_64_invoke: returning void");
                return ret;
            }
            else
            {
                System.Diagnostics.Debugger.Log(0, "libsupcs", "x86_64_invoke: returning " + rettype.FullName + " (" +
                    CastOperations.ReinterpretAsUlong(rettype).ToString("X") + ")");
                return ret;
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern int ReinterpretAsInt(object addr);
        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern uint ReinterpretAsUInt(object addr);
        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern long ReinterpretAsLong(object addr);
        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern ulong ReinterpretAsULong(object addr);
        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern short ReinterpretAsShort(object addr);
        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern ushort ReinterpretAsUShort(object addr);
        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern byte ReinterpretAsByte(object addr);
        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern sbyte ReinterpretAsSByte(object addr);
        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern char ReinterpretAsChar(object addr);
        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern IntPtr ReinterpretAsIntPtr(object addr);
        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern UIntPtr ReinterpretAsUIntPtr(object addr);
        [MethodImpl(MethodImplOptions.InternalCall)]
        [ReinterpretAsMethod]
        public static extern bool ReinterpretAsBoolean(object addr);
    }
}
