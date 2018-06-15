/* Copyright (C) 2008 - 2016 by John Cronin
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

// Utility functions

using System;
using System.Collections.Generic;
using System.Text;

namespace libtysila5.util
{
    public class util
    {
        /** <summary>Simple implementation of Except from C# 3.0</summary>
         * <param name="first">Elements of this set are returned as a new set, unless they occur in second</param>
         * <param name="second">If an element is in this set, it prevents that value being returned should it occur in first</param>
         */

        public static IEnumerable<T> Except<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            List<T> ret = new List<T>();
            IList<T> s = second as IList<T>;

            if (s == null)
                s = new List<T>(second);

            foreach (T t in first)
            {
                if (!(s.Contains(t)))
                    ret.Add(t);
            }
            return ret;
        }

        /** <summary>Returns a dictionary containing elements within first but not second</summary> */
        public static IEnumerable<KeyValuePair<K, V>> Except<K, V>(IDictionary<K, V> first, IDictionary<K, V> second)
        {
            foreach (KeyValuePair<K, V> kvp in first)
            {
                if (!second.ContainsKey(kvp.Key))
                    yield return kvp;
            }
            yield break;
        }

        /** <summary>Simple implementation of Intersect from C# 3.0</summary>
         * <param name="first">First set</param>
         * <param name="second">Second set</param>
         */

        public static IEnumerable<T> Intersect<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null)
                yield break;
            if (second == null)
                yield break;

            if (!(first is ICollection<T>))
                first = new List<T>(first);

            foreach (T t in second)
            {
                if (((ICollection<T>)first).Contains(t))
                    yield return t;
            }
            yield break;
        }

        /** <summary>Returns a Dictionary of items from first whose keys are in second</summary>
         * <param name="first">Source dictionary</param>
         * <param name="second">Keys to look for</param>
         */
        public static IDictionary<K, V> Intersect<K, V>(IDictionary<K, V> first, ICollection<K> second)
        {
            Dictionary<K, V> ret = new Dictionary<K, V>();
            foreach (K k in second)
            {
                if (first.ContainsKey(k))
                    ret.Add(k, first[k]);
            }
            return ret;
        }

        /** <summary>Simple implementation of Union from C# 3.0</summary>
         * <param name="first">First set</param>
         * <param name="second">Second set</param>
         */

        public static IEnumerable<T> Union<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null)
                yield break;
            if (second == null)
                yield break;

            if (!(first is ICollection<T>))
                first = new List<T>(first);

            foreach (T t in first)
                yield return t;

            foreach (T t in second)
            {
                if (!(((ICollection<T>)first).Contains(t)))
                    yield return t;
            }
            yield break;
        }

        /** <summary>Returns a dictionary containing those members of the initial dictionary whose keys are contained in match</summary>
         */
        public static IEnumerable<KeyValuePair<K, V>> Union<K, V>(IDictionary<K, V> initial, IEnumerable<K> match)
        {
            if (!(match is ICollection<K>))
                match = new List<K>(match);

            foreach (KeyValuePair<K, V> kvp in initial)
            {
                if (((ICollection<K>)match).Contains(kvp.Key))
                    yield return kvp;
            }
            yield break;
        }

        /** <summary>Aligns a value to a multiple of a particular value</summary>
         */
        public static int align(int input, int factor)
        {
            int i_rem_f = input % factor;
            if (i_rem_f == 0)
                return input;
            return input - i_rem_f + factor;
        }

        /** <summary>Return the largest of two numbers</summary>
         */
        public static int max(int a, int b)
        {
            if (a >= b)
                return a;
            else
                return b;
        }

         /** <summary>A generic high-performance set implementation</summary>
         */
        public class Set<T> : IEnumerable<T>, IEquatable<Set<T>>, ICollection<T>
        {
            Dictionary<T, bool> d = new Dictionary<T, bool>();

            public Set() { }
            public Set(ICollection<T> ts) { AddRange(ts); }

            public void Add(T t)
            {
                if (!d.ContainsKey(t))
                    d.Add(t, true);
            }

            public void AddRange(IEnumerable<T> ts)
            {
                foreach (T t in ts)
                    Add(t);
            }

            public bool Remove(T t)
            {
                if (d.ContainsKey(t))
                    return d.Remove(t);
                return false;
            }

            public bool Contains(T t)
            {
                return d.ContainsKey(t);
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("{");

                int i = 0;
                foreach (KeyValuePair<T, bool> kvp in d)
                {
                    if (i != 0)
                        sb.Append(", ");
                    sb.Append(kvp.Key.ToString());
                    i++;
                }

                sb.Append("}");
                return sb.ToString();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return (IEnumerator<T>)d.Keys.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return d.Keys.GetEnumerator();
            }

            public bool Equals(Set<T> other)
            {
                if (d.Count != other.d.Count)
                    return false;
                foreach (KeyValuePair<T, bool> kvp in d)
                {
                    if (!other.Contains(kvp.Key))
                        return false;
                }
                return true;
            }

            public Set<T> Intersect(IEnumerable<T> other)
            {
                Set<T> ret = new Set<T>();
                foreach (T t in other)
                {
                    if (Contains(t))
                        ret.Add(t);
                }
                return ret;
            }

            public Set<T> Except(IEnumerable<T> other)
            {
                Set<T> ret = new Set<T>(this);
                foreach (T t in other)
                    ret.Remove(t);
                return ret;
            }

            public Set<T> Union(IEnumerable<T> other)
            {
                Set<T> ret = new Set<T>(this);
                foreach (T t in other)
                    ret.Add(t);
                return ret;
            }

            public int Count { get { return d.Count; } }


            public void Clear()
            {
                d.Clear();
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                d.Keys.CopyTo(array, arrayIndex);
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public T ItemAtIndex(int idx)
            {
                IEnumerator<T> e = d.Keys.GetEnumerator();
                e.MoveNext();
                while(idx-- > 0)
                    e.MoveNext();
                return e.Current;
            }
        }
   
        /** <summary>A class mapping A to B and B to A</summary> */
        public class ABMap<T, U>
        {
            Dictionary<T, U> ab_map = new Dictionary<T, U>();
            Dictionary<U, T> ba_map = new Dictionary<U, T>();

            public void Add(T a, U b)
            {
                ab_map.Add(a, b);
                ba_map.Add(b, a);
            }

            public void RemoveA(T a)
            {
                U b = ab_map[a];
                ab_map.Remove(a);
                ba_map.Remove(b);
            }

            public void RemoveB(U b)
            {
                T a = ba_map[b];
                ba_map.Remove(b);
                ab_map.Remove(a);
            }

            public void ReplaceAtA(T a, U b)
            {
                ba_map.Remove(ab_map[a]);
                ab_map[a] = b;
                ba_map[b] = a;
            }

            public void ReplaceAtB(U b, T a)
            {
                ab_map.Remove(ba_map[b]);
                ba_map[b] = a;
                ab_map[a] = b;
            }

            public U GetAtA(T a)
            {
                return ab_map[a];
            }

            public T GetAtB(U b)
            {
                return ba_map[b];
            }

            public ICollection<T> GetAs()
            {
                return (ICollection<T>)ab_map.Keys;
            }

            public ICollection<U> GetBs()
            {
                return (ICollection<U>)ba_map.Keys;
            }

            public void Clear()
            {
                ab_map.Clear();
                ba_map.Clear();
            }

            public int Count
            {
                get { return ab_map.Count; }
            }

            public bool ContainsA(T a)
            {
                return ab_map.ContainsKey(a);
            }

            public bool ContainsB(U b)
            {
                return ba_map.ContainsKey(b);
            }
        }

        /** <summary>A stack of items</summary> */
        public class Stack<T> : IList<T>
        {
            List<T> l;

            public Stack()
            {
                l = new List<T>();
            }

            public Stack(IEnumerable<T> collection)
            {
                l = new List<T>(collection);
            }

            public int IndexOf(T item)
            {
                return l.IndexOf(item);
            }

            public void Insert(int index, T item)
            {
                l.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                l.RemoveAt(index);
            }

            public T this[int index]
            {
                get
                {
                    return l[index];
                }
                set
                {
                    l[index] = value;
                }
            }

            public void Add(T item)
            {
                l.Add(item);
            }

            public void Clear()
            {
                l.Clear();
            }

            public bool Contains(T item)
            {
                return l.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                l.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return l.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(T item)
            {
                return l.Remove(item);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return l.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return l.GetEnumerator();
            }

            public void Push(T item)
            {
                l.Add(item);
            }

            public T Pop()
            {
                if (l.Count == 0)
                    return default(T);

                T ret = l[l.Count - 1];
                l.RemoveAt(l.Count - 1);
                return ret;
            }

            public T Peek()
            {
                return l[l.Count - 1];
            }

            public T Peek(int n)
            {
                return l[l.Count - 1 - n];
            }
        }
    }
}
