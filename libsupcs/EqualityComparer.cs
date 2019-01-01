/* Copyright (C) 2014-2016 by John Cronin
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

namespace libsupcs
{
    /* The following class is taken from Mono.  mono/corlib/System.Collections.Generic/EqualityComparer.cs
     * Authors: Ben Maurer (bmaurer@ximian.com), Copyright (C) 2004 Novell, Inc under the same license as this file
     * 
     * We need to use our own version of this as EqualityComparer<T> has a static constructor which instantiates
     * a generic type, and if the jit is not functioning this cannot yet be done */
    public class GenericEqualityComparer<T> : EqualityComparer<T> where T : System.IEquatable<T>
    {
        public override int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }

        public override bool Equals(T x, T y)
        {
            if (x == null)
                return y == null;

            return x.Equals(y);
        }
    }

    public class GenericEqualityComparerRef<T> : EqualityComparer<T> where T : class
    {
        public override int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }

        public override bool Equals(T x, T y)
        {
            if (x == null)
                return y == null;

            return x.Equals(y);
        }
    }

    public class GenericComparer<T> : IComparer<T> where T : System.IComparable<T>
    {
        public int Compare(T x, T y)
        {
            return x.CompareTo(y);
        }
    }

    public class IntPtrComparer : IComparer<IntPtr>
    {
        [MethodReferenceAlias("intptr_compare")]
        [MethodImpl(MethodImplOptions.InternalCall)]
        extern public int Compare(IntPtr x, IntPtr y);

        [MethodAlias("intptr_compare")]
        [AlwaysCompile]
        [Bits32Only]
        int Compare32(int x, int y)
        {
            if (x > y)
                return 1;
            if (x == y)
                return 0;
            return -1;
        }

        [MethodAlias("intptr_compare")]
        [AlwaysCompile]
        [Bits64Only]
        int Compare64(long x, long y)
        {
            if (x > y)
                return 1;
            if (x == y)
                return 0;
            return -1;
        }
    }

    public class UIntPtrComparer : IComparer<UIntPtr>
    {
        [MethodReferenceAlias("uintptr_compare")]
        [MethodImpl(MethodImplOptions.InternalCall)]
        extern public int Compare(UIntPtr x, UIntPtr y);

        [MethodAlias("uintptr_compare")]
        [AlwaysCompile]
        [Bits32Only]
        int Compare32(uint x, uint y)
        {
            if (x > y)
                return 1;
            if (x == y)
                return 0;
            return -1;
        }

        [MethodAlias("uintptr_compare")]
        [AlwaysCompile]
        [Bits64Only]
        int Compare64(ulong x, ulong y)
        {
            if (x > y)
                return 1;
            if (x == y)
                return 0;
            return -1;
        }
    }

}
