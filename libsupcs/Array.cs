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

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

namespace libsupcs
{
    public class Array
    {
        [MethodAlias("_ZW6System5Array_4Copy_Rv_P6V5ArrayiV5Arrayiib")]
        [WeakLinkage]
        [AlwaysCompile]
        static unsafe void Copy(void* srcArr, int srcIndex, void* dstArr, int dstIndex, int length,
            bool reliable)
        {
            /* Ensure arrays are valid */
            if (srcArr == null || dstArr == null)
                throw new ArgumentNullException();

            /* Ensure length is valid */
            if (length < 0)
                throw new ArgumentOutOfRangeException();

            /* Ensure both arrays are of the same rank */
            var srcRank = *(int*)((byte*)srcArr + ArrayOperations.GetRankOffset());
            var dstRank = *(int*)((byte*)dstArr + ArrayOperations.GetRankOffset());
            if (srcRank != dstRank)
                throw new RankException();

            /* Get source and dest element types */
            var srcET = *(void**)((byte*)srcArr + ArrayOperations.GetElemTypeOffset());
            var dstET = *(void**)((byte*)dstArr + ArrayOperations.GetElemTypeOffset());

            /* See if we can do a quick copy */
            bool can_quick_copy = false;
            if (srcET == dstET)
                can_quick_copy = true;
            else if (TysosType.CanCast(srcET, dstET))
                can_quick_copy = true;
            else if (reliable)
                throw new ArrayTypeMismatchException();   /* ConstrainedCopy requires types to be the same or derived */
            else
                can_quick_copy = false;

            /* For now we don't handle arrays with lobounds != 0 */
            var srcLobounds = *(int**)((byte*)srcArr + ArrayOperations.GetLoboundsOffset());
            var dstLobounds = *(int**)((byte*)dstArr + ArrayOperations.GetLoboundsOffset());

            for (int i = 0; i < srcRank; i++)
            {
                if (srcLobounds[i] != 0)
                    throw new NotImplementedException();
                if (dstLobounds[i] != 0)
                    throw new NotImplementedException();
            }
            if (srcIndex < 0 || dstIndex < 0)
                throw new ArgumentOutOfRangeException();

            /* Ensure we don't overflow */
            var src_len = GetLength(srcArr);
            var dst_len = GetLength(dstArr);

            if (srcIndex + length > src_len)
            {
                System.Diagnostics.Debugger.Log(0, "libsupcs", "Array.Copy srcIndex/length out of range");
                System.Diagnostics.Debugger.Log(length, "libsupcs", "length");
                System.Diagnostics.Debugger.Log(srcIndex, "libsupcs", "srcIndex");
                System.Diagnostics.Debugger.Log(src_len, "libsupcs", "src_len");
                throw new ArgumentOutOfRangeException();
            }
            if (dstIndex + length > dst_len)
            {
                System.Diagnostics.Debugger.Log(0, "libsupcs", "Array.Copy dstIndex/length out of range");
                System.Diagnostics.Debugger.Log(length, "libsupcs", "length");
                System.Diagnostics.Debugger.Log(dstIndex, "libsupcs", "dstIndex");
                System.Diagnostics.Debugger.Log(dst_len, "libsupcs", "dst_len");
                throw new ArgumentOutOfRangeException();
            }

            if (can_quick_copy)
            {
                /* Elem size of both arrays is guaranteed to be the same and we can do a shallow copy */
                var e_size = *(int*)((byte*)srcArr + ArrayOperations.GetElemSizeOffset());

                var src_ia = *(byte**)((byte*)srcArr + ArrayOperations.GetInnerArrayOffset());
                var dst_ia = *(byte**)((byte*)dstArr + ArrayOperations.GetInnerArrayOffset());

                MemoryOperations.MemMove(dst_ia + dstIndex * e_size, src_ia + srcIndex * e_size, length * e_size);
            }
            else
            {
                /* TODO: implement as per System.Array.Copy() semantics */
                throw new NotImplementedException();
            }
        }

        [MethodAlias("_ZW6System5Array_8FastCopy_Rb_P5V5ArrayiV5Arrayii")]
        [WeakLinkage]
        [AlwaysCompile]
        static unsafe bool FastCopy(void *srcArr, int srcIndex, void *dstArr, int dstIndex, int length)
        {
            /* This is often called with length == 0 when the
             * first item is added to a List<T> as the default
             * array within the list has length 0.
             */
            
            if (length == 0)
                return true;

            // Ensure both arrays are of rank 1
            int srcRank = *(int*)((byte*)srcArr + ArrayOperations.GetRankOffset());
            int dstRank = *(int*)((byte*)dstArr + ArrayOperations.GetRankOffset());
            if (srcRank != 1)
                return false;
            if (dstRank != 1)
                return false;

            // Ensure srcIndex is valid
            int srcLobound = **(int**)((byte*)srcArr + ArrayOperations.GetLoboundsOffset());
            int srcSize = **(int**)((byte*)srcArr + ArrayOperations.GetSizesOffset());
            srcIndex -= srcLobound;
            if (srcIndex + length > srcSize)
                return false;

            // Ensure destIndex is valid
            int dstLobound = **(int**)((byte*)dstArr + ArrayOperations.GetLoboundsOffset());
            int dstSize = **(int**)((byte*)dstArr + ArrayOperations.GetSizesOffset());
            dstIndex -= dstLobound;
            if (dstIndex + length > dstSize)
                return false;

            // Ensure both have same element type
            void* srcET = *(void**)((byte*)srcArr + ArrayOperations.GetElemTypeOffset());
            void* dstET = *(void**)((byte*)dstArr + ArrayOperations.GetElemTypeOffset());
            if (srcET != dstET)
                return false;

            // Get element size
            int elemSize = *(int*)((byte*)srcArr + ArrayOperations.GetElemSizeOffset());

            srcIndex *= elemSize;
            dstIndex *= elemSize;

            byte* srcAddr = *(byte**)((byte*)srcArr + ArrayOperations.GetInnerArrayOffset()) + srcIndex;
            byte* dstAddr = *(byte**)((byte*)dstArr + ArrayOperations.GetInnerArrayOffset()) + dstIndex;
            length *= elemSize;

            MemoryOperations.MemMove(dstAddr, srcAddr, length);

            return true;
        }

        [MethodAlias("_ZW6System5Array_13GetLowerBound_Ri_P2u1ti")]
        [WeakLinkage]
        [AlwaysCompile]
        static unsafe int GetLowerBound(void *arr, int rank)
        {
            int arrRank = *(int*)((byte*)arr + ArrayOperations.GetRankOffset());
            if (rank < 0 || rank >= arrRank)
            {
                System.Diagnostics.Debugger.Break();
                throw new IndexOutOfRangeException();
            }

            int* lbPtr = *(int**)((byte*)arr + ArrayOperations.GetLoboundsOffset());
            return *(lbPtr + rank);
        }

        [MethodAlias("_ZW6System5Array_5Clear_Rv_P3V5Arrayii")]
        [WeakLinkage]
        [AlwaysCompile]
        static unsafe void Clear(void *arr, int index, int length)
        {
            /* Get a pointer to the source data */
            var elem_size = *(int*)((byte*)arr + ArrayOperations.GetElemSizeOffset());
            void* ia = *(void**)((byte*)arr + ArrayOperations.GetInnerArrayOffset());
            void* sptr = (void*)((byte*)ia + index * elem_size);

            var mem_size = length * elem_size;

            MemoryOperations.MemSet(sptr, 0, mem_size);
        }

        [MethodAlias("_ZW6System5Array_10get_Length_Ri_P1u1t")]
        [WeakLinkage]
        [AlwaysCompile]
        static unsafe int GetLength(void *arr)
        {
            int arrRank = *(int*)((byte*)arr + ArrayOperations.GetRankOffset());
            int* szPtr = *(int**)((byte*)arr + ArrayOperations.GetSizesOffset());

            int ret = 1;

            for (int i = 0; i < arrRank; i++)
                ret = ret * *(szPtr + i);
            return ret;
        }
        
        [MethodAlias("_ZW6System5Array_9GetLength_Ri_P2u1ti")]
        [WeakLinkage]
        [AlwaysCompile]
        static unsafe int GetLength(void *arr, int rank)
        {
            if(arr == null)
            {
                System.Diagnostics.Debugger.Break();
                throw new ArgumentNullException();
            }
            int arrRank = *(int*)((byte*)arr + ArrayOperations.GetRankOffset());
            if (rank < 0 || rank >= arrRank)
            {
                System.Diagnostics.Debugger.Break();
                throw new IndexOutOfRangeException();
            }

            int* szPtr = *(int**)((byte*)arr + ArrayOperations.GetSizesOffset());
            return *(szPtr + rank);
        }

        [MethodAlias("_ZW6System5Array_8get_Rank_Ri_P1u1t")]
        [MethodAlias("_ZW6System5Array_7GetRank_Ri_P1u1t")]
        [WeakLinkage]
        [AlwaysCompile]
        static unsafe int GetRank(void *arr)
        {
            return *(int*)((byte*)arr + ArrayOperations.GetRankOffset());
        }

        [MethodAlias("_ZW35System#2ERuntime#2ECompilerServices14RuntimeHelpers_15InitializeArray_Rv_P2U6System5ArrayV18RuntimeFieldHandle")]
        [WeakLinkage]
        [AlwaysCompile]
        static unsafe void InitializeArray(void *arr, void *fld_handle)
        {
            System.Diagnostics.Debugger.Log(0, "libsupcs", "InitializeArray: arr: " + ((ulong)arr).ToString("X16") + ", fld_handle: " + ((ulong)fld_handle).ToString("X16"));
            void* dst = *(void**)((byte*)arr + ArrayOperations.GetInnerArrayOffset());

            /* Get total number of elements, and hence data size */
            int* sizes = *(int**)((byte*)arr + ArrayOperations.GetSizesOffset());
            int rank = *(int*)((byte*)arr + ArrayOperations.GetRankOffset());
            if (rank == 0)
                return;

            int size = sizes[0];
            for (int i = 1; i < rank; i++)
                size *= sizes[i];

            int len = size * *(int*)((byte*)arr + ArrayOperations.GetElemSizeOffset());

            /* Field Typeinfos hava a pointer to the stored data as their third element */
            void* src = ((void**)fld_handle)[2];

            MemoryOperations.MemCpy(dst, src, len);
        }

        [WeakLinkage]
        [MethodAlias("_ZW34System#2ERuntime#2EInteropServices7Marshal_13CopyToManaged_Rv_P4u1Iu1Oii")]
        [AlwaysCompile]
        static unsafe void CopyToManaged(void *src, void *dstArr, int startIndex, int length)
        {
            var esize = *(int*)((byte*)dstArr + ArrayOperations.GetElemSizeOffset());
            int* lovals = *(int**)((byte*)dstArr + ArrayOperations.GetLoboundsOffset());

            var dst = *(byte**)((byte*)dstArr + ArrayOperations.GetInnerArrayOffset()) + (startIndex - lovals[0]) * esize;

            MemoryOperations.MemCpy(dst, src, length * esize);
        }

        [WeakLinkage]
        [MethodAlias("_ZW34System#2ERuntime#2EInteropServices7Marshal_12CopyToNative_Rv_P4u1Oiu1Ii")]
        [AlwaysCompile]
        static unsafe void CopyToNative(void *srcArr, int startIndex, void *dst, int length)
        {
            var esize = *(int*)((byte*)srcArr + ArrayOperations.GetElemSizeOffset());
            int* lovals = *(int**)((byte*)srcArr + ArrayOperations.GetLoboundsOffset());

            var src = *(byte**)((byte*)srcArr + ArrayOperations.GetInnerArrayOffset()) + (startIndex - lovals[0]) * esize;

            MemoryOperations.MemCpy(dst, src, length * esize);
        }

        [WeakLinkage]
        [MethodAlias("_ZW6System5Array_20InternalGetReference_Rv_P4u1tPviPi")]
        [AlwaysCompile]
        static unsafe void InternalGetReference(void *arr, System.TypedReference *typedref, int ranks, int *rank_indices)
        {
            // idx = rightmost-index + 2nd-right * rightmost-size + 3rd-right * 2nd-right-size * right-size + ...

            // rank checking is done by System.Array members in coreclr
            int* lovals = *(int**)((byte*)arr + ArrayOperations.GetLoboundsOffset());
            int* sizes = *(int**)((byte*)arr + ArrayOperations.GetSizesOffset());

            // first get index of first rank
            if (rank_indices[0] > sizes[0])
                throw new IndexOutOfRangeException();
            int index = rank_indices[0] - lovals[0];

            // now repeat mul rank size; add rank index; rank-1 times
            for(int rank = 1; rank < ranks; rank++)
            {
                if (rank_indices[rank] > sizes[rank])
                    throw new IndexOutOfRangeException();

                index *= sizes[rank];
                index += rank_indices[rank];
            }

            // get pointer to actual data
            int et_size = *(int*)((byte*)arr + ArrayOperations.GetElemSizeOffset());
            void* ptr = *(byte**)((byte*)arr + ArrayOperations.GetInnerArrayOffset()) + index * et_size;

            // store to the typed reference
            *(void**)((byte*)typedref + ClassOperations.GetTypedReferenceValueOffset()) = ptr;
            *(void**)((byte*)typedref + ClassOperations.GetTypedReferenceTypeOffset()) =
                *(void**)((byte*)arr + ArrayOperations.GetElemTypeOffset());
        }

        [MethodAlias("_Zu1T_16InternalToObject_Ru1O_P1Pv")]
        [WeakLinkage]
        [AlwaysCompile]
        static unsafe void* InternalToObject(void *typedref)
        {
            // get the type from the typed reference to see if we need to box the object
            //  or simply return the address as a reference type

            void* et = *(void**)((byte*)typedref + ClassOperations.GetTypedReferenceTypeOffset());
            void* src = *(void**)((byte*)typedref + ClassOperations.GetTypedReferenceValueOffset());

            void* etextends = *(void**)((byte*)et + ClassOperations.GetVtblExtendsVtblPtrOffset());
            if (etextends == OtherOperations.GetStaticObjectAddress("_Zu1L") ||
                etextends == OtherOperations.GetStaticObjectAddress("_ZW6System4Enum"))
            {
                // this is a boxed value type.  Get its size
                var vt_size = TysosType.GetValueTypeSize(et);

                // build a new boxed type
                var ret = MemoryOperations.GcMalloc(*(int*)((byte*)et + ClassOperations.GetVtblTypeSizeOffset()));

                // dst ptr
                var dst = (byte*)ret + ClassOperations.GetBoxedTypeDataOffset();

                CopyMem(dst, (byte*)src, vt_size);

                return ret;
            }
            else
            {
                // simply copy the reference
                return *(void**)src;
            }
        }

        [MethodAlias("_ZW6System5Array_16InternalSetValue_Rv_P2Pvu1O")]
        [WeakLinkage]
        [AlwaysCompile]
        static unsafe void InternalSetValue(void* typedref, void* objval)
        {
            // get the type from the typed reference to see if we need to unbox the object
            //  or store as a reference type

            void* et = *(void**)((byte*)typedref + ClassOperations.GetTypedReferenceTypeOffset());
            void* ptr = *(void**)((byte*)typedref + ClassOperations.GetTypedReferenceValueOffset());

            void* etextends = *(void**)((byte*)et + ClassOperations.GetVtblExtendsVtblPtrOffset());
            if (etextends == OtherOperations.GetStaticObjectAddress("_Zu1L") ||
                etextends == OtherOperations.GetStaticObjectAddress("_ZW6System4Enum"))
            {
                // this is a boxed value type.  Get its size
                var vt_size = TysosType.GetValueTypeSize(et);

                // src ptr
                void* src = *(void**)((byte*)objval + ClassOperations.GetBoxedTypeDataOffset());

                CopyMem((byte*)ptr, (byte*)src, vt_size);
            }
            else
            {
                // simply copy the reference
                *(void**)ptr = objval;
            }
        }

        private static unsafe void CopyMem(byte* dst, byte* src, int vt_size)
        {
            /* memcpy for non-aligned sizes and addresses - ensures we don't overwrite adjacent
             * array indices.  Does not return so breaks memcpy semantics */
            
            // TODO ensure pointers are aligned
            while(vt_size >+ 8)
            {
                *(ulong*)dst = *(ulong*)src;
                dst += 8;
                src += 8;
                vt_size -= 8;
            }
            while (vt_size >+ 4)
            {
                *(uint*)dst = *(uint*)src;
                dst += 4;
                src += 4;
                vt_size -= 4;
            }
            while (vt_size >+ 2)
            {
                *(ushort*)dst = *(ushort*)src;
                dst += 2;
                src += 2;
                vt_size -= 2;
            }
            while(vt_size >= 1)
            {
                *dst = *src;
                dst++;
                src++;
                vt_size--;
            }
        }

        [MethodAlias("_ZW6System5Array_12GetValueImpl_Ru1O_P2u1ti")]
        [WeakLinkage]
        [AlwaysCompile]
        static unsafe void *GetValueImpl(void *arr, int pos)
        {
            /* Get the element type of the array */
            void* et = *(void**)((byte*)arr + ArrayOperations.GetElemTypeOffset());

            /* Get a pointer to the source data */
            var elem_size = *(int*)((byte*)arr + ArrayOperations.GetElemSizeOffset());
            void* ia = *(void**)((byte*)arr + ArrayOperations.GetInnerArrayOffset());
            void* sptr = (void*)((byte*)ia + pos * elem_size);

            /* Is this a value type? In which case we need to return a boxed value */
            void* extends = *(void**)((byte*)et + ClassOperations.GetVtblExtendsVtblPtrOffset());

            if (extends == OtherOperations.GetStaticObjectAddress("_Zu1L") ||
                extends == OtherOperations.GetStaticObjectAddress("_ZW6System4Enum"))
            {
                /* This is a value type.  We need to read the size of the element,
                 * create a new object of the appropriate size and copy the data
                 * into it */
                byte *ret = (byte*)MemoryOperations.GcMalloc(elem_size + ClassOperations.GetBoxedTypeDataOffset());
                *(void**)(ret + ClassOperations.GetVtblFieldOffset()) = et;
                *(ulong*)(ret + ClassOperations.GetMutexLockOffset()) = 0;

                /* Avoid calls to memcpy if possible */
                switch(elem_size)
                {
                    case 1:
                        *(byte*)(ret + ClassOperations.GetBoxedTypeDataOffset()) = *(byte*)sptr;
                        return ret;
                    case 2:
                        *(ushort*)(ret + ClassOperations.GetBoxedTypeDataOffset()) = *(ushort*)sptr;
                        return ret;
                    case 4:
                        *(uint*)(ret + ClassOperations.GetBoxedTypeDataOffset()) = *(uint*)sptr;
                        return ret;
                    case 8:
                        if (OtherOperations.GetPointerSize() >= 8)
                        {
                            *(ulong*)(ret + ClassOperations.GetBoxedTypeDataOffset()) = *(ulong*)sptr;
                            return ret;
                        }
                        else
                        {
                            *(uint*)(ret + ClassOperations.GetBoxedTypeDataOffset()) = *(uint*)sptr;
                            *(uint*)(ret + ClassOperations.GetBoxedTypeDataOffset() + 4) = *(uint*)((byte*)sptr + 4);
                            return ret;
                        }
                    case 16:
                        if (OtherOperations.GetPointerSize() >= 8)
                        {
                            *(ulong*)(ret + ClassOperations.GetBoxedTypeDataOffset()) = *(ulong*)sptr;
                            *(ulong*)(ret + ClassOperations.GetBoxedTypeDataOffset() + 8) = *(ulong*)((byte*)sptr + 8);
                            return ret;
                        }
                        else
                        {
                            *(uint*)(ret + ClassOperations.GetBoxedTypeDataOffset()) = *(uint*)sptr;
                            *(uint*)(ret + ClassOperations.GetBoxedTypeDataOffset() + 4) = *(uint*)((byte*)sptr + 4);
                            *(uint*)(ret + ClassOperations.GetBoxedTypeDataOffset() + 8) = *(uint*)((byte*)sptr + 8);
                            *(uint*)(ret + ClassOperations.GetBoxedTypeDataOffset() + 12) = *(uint*)((byte*)sptr + 12);
                            return ret;
                        }
                }

                /* Do data copy via memcpy */
                MemoryOperations.MemCpy(ret + ClassOperations.GetBoxedTypeDataOffset(),
                    sptr, elem_size);
                return ret;
            }
            else
            {
                /* Its a reference type, so just return the pointer */
                return *(void**)sptr;                 
            }
        }

        /* Build an array of a particular type */
        public static unsafe T[] CreateSZArray<T>(int nitems, void* data_addr)
        {
            TysosType arrtt = (TysosType)typeof(T[]);
            TysosType elemtt = (TysosType)typeof(T);

            int elemsize;
            if (elemtt.IsValueType)
            {
                elemsize = elemtt.GetClassSize() - ClassOperations.GetBoxedTypeDataOffset();
            }
            else
            {
                elemsize = OtherOperations.GetPointerSize();
            }

            if (data_addr == null)
                data_addr = MemoryOperations.GcMalloc(elemsize * nitems);

            byte* ret = (byte*)MemoryOperations.GcMalloc(ArrayOperations.GetArrayClassSize() + 8);     // extra space for lobounds and length array

            void* vtbl = *(void**)((byte*)CastOperations.ReinterpretAsPointer(arrtt) + ClassOperations.GetSystemTypeImplOffset());

            *(void**)(ret + ClassOperations.GetVtblFieldOffset()) = vtbl;
            *(ulong*)(ret + ClassOperations.GetMutexLockOffset()) = 0;

            *(void**)(ret + ArrayOperations.GetElemTypeOffset()) = (byte*)CastOperations.ReinterpretAsPointer(elemtt) + ClassOperations.GetSystemTypeImplOffset();
            *(int*)(ret + ArrayOperations.GetElemSizeOffset()) = elemsize;
            *(void**)(ret + ArrayOperations.GetInnerArrayOffset()) = data_addr;
            *(void**)(ret + ArrayOperations.GetLoboundsOffset()) =  ret + ArrayOperations.GetArrayClassSize();
            *(void**)(ret + ArrayOperations.GetSizesOffset()) = ret + ArrayOperations.GetArrayClassSize() + 4;
            *(int*)(ret + ArrayOperations.GetRankOffset()) = 1;
            *(int*)(ret + ArrayOperations.GetArrayClassSize()) = 0; // lobounds[0]
            *(int*)(ret + ArrayOperations.GetArrayClassSize() + 4) = nitems;    // sizes[0]

            return (T[])CastOperations.ReinterpretAsObject(ret);
        }
    }
}
