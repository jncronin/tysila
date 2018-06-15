/* Copyright (C) 2016 by John Cronin
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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace libtysila5.util
{
    public class Set<T> : ICollection<T> where T : class, System.IEquatable<T>
    {
        Dictionary<T, int> d = new Dictionary<T, int>(
            new GenericEqualityComparer<T>());

        public int Count
        {
            get
            {
                return d.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(T item)
        {
            d[item] = 0;
        }

        public void Clear()
        {
            d.Clear();
        }

        public bool Contains(T item)
        {
            return d.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var kvp in d)
                array[arrayIndex++] = kvp.Key;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return d.Keys.GetEnumerator();
        }

        public bool Remove(T item)
        {
            return d.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return d.Keys.GetEnumerator();
        }

        public T GetAny()
        {
            if (d.Count == 0)
                return null;

            var e = d.Keys.GetEnumerator();
            e.MoveNext();
            return e.Current;
        }

        internal Set<T> Clone()
        {
            var other = new Set<T>();
            foreach (var key in d.Keys)
                other.d[key] = 0;
            return other;
        }
    }
}
