/* Copyright (C) 2017 by John Cronin
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
using metadata;

/* Array object is:
 * 
 * vtbl pointer
 * mutex lock
 * elemtype vtbl pointer
 * lobounds array pointer
 * sizes array pointer
 * data array pointer
 * intptr rank
 * int etsize
 * 
 * followed by
 * lobounds array
 * sizes array
 * data array
 */

namespace libtysila5.layout
{
    public partial class Layout
    {
        public enum ArrayField
        {
            VtblPointer,
            MutexLock,
            ElemTypeVtblPointer,
            LoboundsPointer,
            SizesPointer,
            DataArrayPointer,
            Rank,
            ElemTypeSize
        }

        public static int GetArrayObjectSize(target.Target t)
        {
            // round up to 8x intptr size so size is aligned to word size
            return 8 * t.GetPointerSize();
        }

        public static int GetArrayFieldOffset(ArrayField af, target.Target t)
        {
            switch(af)
            {
                case ArrayField.VtblPointer:
                    return 0;
                case ArrayField.MutexLock:
                    return 1 * t.GetPointerSize();
                case ArrayField.ElemTypeVtblPointer:
                    return 2 * t.GetPointerSize();
                case ArrayField.LoboundsPointer:
                    return 3 * t.GetPointerSize();
                case ArrayField.SizesPointer:
                    return 4 * t.GetPointerSize();
                case ArrayField.DataArrayPointer:
                    return 5 * t.GetPointerSize();
                case ArrayField.Rank:
                    return 6 * t.GetPointerSize();
                case ArrayField.ElemTypeSize:
                    return 7 * t.GetPointerSize();
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
