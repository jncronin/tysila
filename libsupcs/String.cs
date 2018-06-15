/* Copyright (C) 2014 by John Cronin
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
using System.Runtime.CompilerServices;

/* System.String internal calls */

namespace libsupcs
{
    class String
    {
        [MethodReferenceAlias("strlen")]
        [MethodImpl(MethodImplOptions.InternalCall)]
        static unsafe extern int strlen(byte* s);

        [MethodReferenceAlias("mbstowcs")]
        [MethodImpl(MethodImplOptions.InternalCall)]
        static unsafe extern int mbstowcs(char* dest, byte* src, int n);

        [AlwaysCompile]
        [MethodAlias("_Zu1S_15ReplaceInternal_Ru1S_P3u1tu1Su1S")]
        static unsafe string InternalReplace(string str, string old_value, string new_value)
        {
            /* maximum size of new string is str if new_value <= old_value or
             * if new_value is larger then assume str is made up of all old_value
             * in which case it is this count * new_value + remainder (or count + 1 * new_value)
             */

            int max_new_str;
            if(new_value.Length > old_value.Length)
            {
                max_new_str = ((str.Length / old_value.Length) + 1) * new_value.Length;
            }
            else
            {
                max_new_str = str.Length;
            }

            //System.Diagnostics.Debugger.Log(0, "libsupcs", "InternalReplace: str: " + str + ", old_value: " + old_value +
            //    ", new_value: " + new_value + ", max_new_str: " + max_new_str.ToString());

            /* limit stack alloc to 1024 chars to reduce risk of stack overflows.  Implementations
             *  should have a stack guard page anyway to catch overflows of this size whilst larger
             *  stack allocs could be used to introduce code beyond the guard page.
             *  
             *  We do it this way because C# does not support conditional stack alloc assigns
             */
            int max_stack_new_str = max_new_str > 1024 ? 1024 : max_new_str;
            char* nstack = stackalloc char[max_stack_new_str];

            char* ns;

            if (max_new_str <= 1024)
                ns = nstack;
            else
                ns = (char*)MemoryOperations.GcMalloc(max_new_str * sizeof(char));
            char* ptr = ns;

            for (int i = 0; i < str.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < old_value.Length; j++)
                {
                    if (((i + j) >= str.Length) || (str[i + j] != old_value[j]))
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    for (int j = 0; j < new_value.Length; j++)
                        *ptr++ = new_value[j];
                    i += old_value.Length;
                    i--;        // the for loop adds a 1 for us
                }
                else
                {
                    *ptr++ = str[i];
                }
            }

            var str_len = ptr - ns;

            //var ret = new string(ns, 0, (int)str_len);
            //System.Diagnostics.Debugger.Log(0, "libsupcs", "InternalReplace: ret: " + ret);
            //return ret;
            return new string(ns, 0, (int)str_len);
        }

        [AlwaysCompile]
        [MethodAlias("_Zu1S_20CompareOrdinalHelper_Ri_P6u1Siiu1Sii")]
        static unsafe int CompareOrdinalHelper(byte *strA, int indexA, int countA, byte *strB, int indexB, int countB)
        {
            char* a = (char*)(strA + StringOperations.GetDataOffset()) + indexA;
            char* b = (char*)(strB + StringOperations.GetDataOffset()) + indexB;

            for(int i = 0; i < countA; i++, a++, b++)
            {
                if (*a < *b)
                    return -1;
                else if (*a > *b)
                    return 1;
            }

            return 0;
        }

        [AlwaysCompile]
        [MethodAlias("_Zu1S_7IsAscii_Rb_P1u1t")]
        static bool IsAscii(string str)
        {
            foreach(var c in str)
            {
                if (c >= 0x80)
                    return false;
            }
            return true;
        }
            

        [WeakLinkage]
        [AlwaysCompile]
        [MethodAlias("_Zu1S_14InternalStrcpy_Rv_P5u1Siu1Sii")]
        static unsafe bool InternalStrcpy(byte *dest, int destPos, byte *src, int srcPos, int count)
        {
            /* Get size of src and dest */
            int srcLength = *(int*)(src + StringOperations.GetLengthOffset());
            int destLength = *(int*)(dest + StringOperations.GetLengthOffset());

            /* Ensure the source and destination are big enough */
            if (destPos < 0)
                return false;
            if (srcPos < 0)
                return false;
            if (count < 0)
                return false;
            if (destPos + count > destLength)
                return false;
            if (srcPos + count > srcLength)
                return false;

            /* Do the copy */
            MemoryOperations.MemCpy((void*)(dest + StringOperations.GetDataOffset() + destPos * 2),
                (void*)(src + StringOperations.GetDataOffset() + srcPos * 2), count * 2);

            return true;
        }

        [WeakLinkage]
        [AlwaysCompile]
        [MethodAlias("_Zu1S_14InternalStrcpy_Rv_P3u1Siu1S")]
        static unsafe bool InternalStrcpy(byte* dest, int destPos, byte* src)
        {
            /* Get size of src and dest */
            int srcLength = *(int*)(src + StringOperations.GetLengthOffset());
            int destLength = *(int*)(dest + StringOperations.GetLengthOffset());
            int srcPos = 0;
            int count = srcLength;

            /* Ensure the source and destination are big enough */
            if (destPos < 0)
                return false;
            if (destPos + count > destLength)
                return false;

            /* Do the copy */
            MemoryOperations.MemCpy((void*)(dest + StringOperations.GetDataOffset() + destPos * 2),
                (void*)(src + StringOperations.GetDataOffset() + srcPos * 2), count * 2);

            return true;
        }

        [MethodAlias("_Zu1S_7#2Ector_Rv_P2u1tu1Zc")]
        [AlwaysCompile]
        static unsafe void StringCtor(byte *str, char[] srcArr)
        {
            void* src = MemoryOperations.GetInternalArray(srcArr);
            int len = srcArr.Length * sizeof(char);
            void* dst = str + StringOperations.GetDataOffset();

            MemoryOperations.MemCpy(dst, src, len);
        }

        [MethodAlias("_Zu1S_7#2Ector_Rv_P3u1tci")]
        [AlwaysCompile]
        static unsafe void StringCtor(byte *str, char c, int count)
        {
            char* dst = (char *)(str + StringOperations.GetDataOffset());
            for (int i = 0; i < count; i++)
                *dst++ = c;
        }

        [MethodAlias("_Zu1S_7#2Ector_Rv_P4u1tu1Zcii")]
        [AlwaysCompile]
        static unsafe void StringCtor(byte *str, char[] srcArr, int startIndex, int length)
        {
            void* src = (byte*)MemoryOperations.GetInternalArray(srcArr) + sizeof(char) * startIndex;
            int len = length * sizeof(char);
            void* dst = str + StringOperations.GetDataOffset();

            MemoryOperations.MemCpy(dst, src, len);
        }

        [MethodAlias("_Zu1S_7#2Ector_Rv_P4u1tPcii")]
        [AlwaysCompile]
        static unsafe void StringCtor(byte* str, char* value, int startIndex, int length)
        {
            void* src = value + startIndex;
            int len = length * sizeof(char);
            void* dst = str + StringOperations.GetDataOffset();

            MemoryOperations.MemCpy(dst, src, len);
        }

        [MethodAlias("_Zu1S_7#2Ector_Rv_P2u1tPa")]
        [AlwaysCompile]
        static unsafe void StringCtor(byte *str, sbyte *value)
        {
            int len = strlen((byte*)value);
            void* dst = str + StringOperations.GetDataOffset();
            mbstowcs((char*)dst, (byte*)value, len);
        }

        [MethodAlias("_Zu1S_28InternalUseRandomizedHashing_Rb_P0")]
        [AlwaysCompile]
        [WeakLinkage]
        static bool InternalUseRandomizedHashing()
        {
            return false;
        }
    }
}
