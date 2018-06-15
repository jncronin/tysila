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

/* System.Buffer internal calls */

namespace libsupcs
{
    class Buffer
    {
        [MethodAlias("_ZW6System6Buffer_17BlockCopyInternal_Rb_P5V5ArrayiV5Arrayii")]
        static unsafe bool BlockCopyInternal(byte* src, int srcOffset, byte* dest, int destOffset,
            int count)
        {
            /* Return false on overflow within src or dest */
            int srcElemSize = *(int*)(src + ArrayOperations.GetElemSizeOffset());
            //int srcIALength = *(int*)(src + ArrayOperations.GetInnerArrayLengthOffset());
            int srcIALength = 0;
            throw new NotImplementedException();
            int srcByteSize = srcElemSize * srcIALength;

            if ((srcOffset + count) > srcByteSize)
                return false;

            int destElemSize = *(int *)(dest + ArrayOperations.GetElemSizeOffset());
            //int destIALength = *(int*)(dest + ArrayOperations.GetInnerArrayLengthOffset());
            int destIALength = 0;
            int destByteSize = destElemSize * destIALength;

            if ((destOffset + count) > destByteSize)
                return false;

            /* Get source and dest addresses */
            byte* srcAddr = *(byte**)(src + ArrayOperations.GetInnerArrayOffset()) + srcOffset;
            byte* destAddr = *(byte**)(dest + ArrayOperations.GetInnerArrayOffset()) + destOffset;

            /* Execute a memmove */
            MemoryOperations.MemMove((void*)destAddr, (void*)srcAddr, count);

            return true;
        }

        [MethodAlias("_ZW6System6Buffer_18ByteLengthInternal_Ri_P1V5Array")]
        static unsafe int ByteLengthInternal(byte* arr)
        {
            int elemSize = *(int*)(arr + ArrayOperations.GetElemSizeOffset());
            //int iaLength = *(int*)(arr + ArrayOperations.GetInnerArrayLengthOffset());
            throw new NotImplementedException();
            //return elemSize * iaLength;
        }
    }
}
