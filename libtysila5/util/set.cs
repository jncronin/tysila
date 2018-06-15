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
using System.Collections.Generic;

namespace libtysila5.util
{
    /** <summary>Simple and fast representation of a set as a
    bitmap for quick setting an getting</summary> */
    public class Set : IEnumerable<int>
    {
        int min_val;
        int set_count;

        List<ulong> b;

        public Set(int length = 64, int min = 0)
        {
            min_val = min;

            var ulongs = (length - 1) / 64 + 1;
            b = new List<ulong>(ulongs);
            set_count = 0;
        }

        private Set() { }

        public void set(int bit)
        {
            if (bit < min_val)
                throw new ArgumentOutOfRangeException();

            int ul_idx = (bit - min_val) / 64;
            int bit_idx = (bit - min_val) % 64;

            while (ul_idx >= b.Count)
                b.Add(0);

            if ((b[ul_idx] & (1UL << bit_idx)) == 0)
                set_count++;
            b[ul_idx] |= (1UL << bit_idx);
        }

        public void set(IEnumerable<int> other)
        {
            foreach (var o in other)
                set(o);
        }

        public void unset(int bit)
        {
            if (bit < min_val)
                throw new ArgumentOutOfRangeException();

            int ul_idx = (bit - min_val) / 64;
            int bit_idx = (bit - min_val) % 64;

            while (ul_idx >= b.Count)
                b.Add(0);

            if ((b[ul_idx] & (1UL << bit_idx)) != 0)
                set_count--;
            b[ul_idx] &= ~(1UL << bit_idx);
        }

        internal void Intersect(ulong v)
        {
            if (b.Count == 0)
                return;
            if (b.Count > 1)
                b.RemoveRange(1, b.Count - 1);
            b[0] = b[0] & v;
            recalc_set_count();
        }

        public void Union(ulong v)
        {
            if (b.Count == 0)
                b.Add(v);
            else
                b[0] |= v;
            recalc_set_count();
        }

        public void unset(Set other) { AndNot(other); }
        public void set(Set other) { Union(other); }

        public bool get(int bit)
        {
            if (bit < min_val)
                throw new ArgumentOutOfRangeException();

            int ul_idx = (bit - min_val) / 64;
            int bit_idx = (bit - min_val) % 64;

            if (ul_idx >= b.Count)
                return false;

            var ret = b[ul_idx] & (1UL << bit_idx);
            if (ret == 0)
                return false;
            return true;
        }

        public int get_first_unset()
        {
            for(int i = 0; i < b.Count; i++)
            {
                var ul = b[i];

                if (ul == 0xffffffffffffffffUL)
                    continue;
                
                for(int bit_idx = 0; bit_idx < 64; bit_idx++)
                {
                    if (((ul >> bit_idx) & 1UL) == 1)
                        continue;
                    return min_val + i * 64 + bit_idx;
                }
            }

            return -1;
        }

        public int get_first_set()
        {
            for (int i = 0; i < b.Count; i++)
            {
                var ul = b[i];

                if (ul == 0)
                    continue;

                for (int bit_idx = 0; bit_idx < 64; bit_idx++)
                {
                    if (((ul >> bit_idx) & 1UL) == 0)
                        continue;
                    return min_val + i * 64 + bit_idx;
                }
            }

            return -1;
        }

        public int get_last_set()
        {
            for (int i = b.Count - 1; i >= 0; i--)
            {
                var ul = b[i];

                if (ul == 0)
                    continue;

                for (int bit_idx = 63; bit_idx >= 0; bit_idx--)
                {
                    if (((ul >> bit_idx) & 1UL) == 0)
                        continue;
                    return min_val + i * 64 + bit_idx;
                }
            }

            return -1;
        }

        public Set Clone()
        {
            var ret = new Set();
            ret.min_val = min_val;
            ret.b = new List<ulong>(b);
            ret.set_count = set_count;
            return ret;
        }

        public int Count { get { return set_count; } }
        public bool Empty { get { return set_count == 0; } }

        public IEnumerator<int> GetEnumerator()
        {
            for(int ul_idx = 0; ul_idx < b.Count; ul_idx++)
            {
                var cur_b = b[ul_idx];
                if (cur_b == 0)
                    continue;
                for(int bit_idx = 0; bit_idx < 64; bit_idx++)
                {
                    if ((cur_b & (1UL << bit_idx)) != 0)
                        yield return min_val + ul_idx * 64 + bit_idx;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void recalc_set_count()
        {
            set_count = 0;
            for(int i = 0; i < b.Count; i++)
            {
                var cur_b = b[i];
                for(int bit_idx = 0; bit_idx < 64; bit_idx++)
                {
                    if ((cur_b & (1UL << bit_idx)) != 0)
                        set_count++;
                }
            }
        }

        public void Intersect(Set other)
        {
            int max_ulongs = b.Count;
            if (other.b.Count > max_ulongs)
                max_ulongs = other.b.Count;

            while(b.Count < max_ulongs)
                b.Add(0);
            while(other.b.Count < max_ulongs)
                other.b.Add(0);

            for (int i = 0; i < b.Count; i++)
                b[i] &= other.b[i];

            recalc_set_count();
        }

        public void Union(Set other)
        {
            int max_ulongs = b.Count;
            if (other.b.Count > max_ulongs)
                max_ulongs = other.b.Count;

            while (b.Count < max_ulongs)
                b.Add(0);
            while (other.b.Count < max_ulongs)
                other.b.Add(0);

            for (int i = 0; i < b.Count; i++)
                b[i] |= other.b[i];

            recalc_set_count();
        }

        public void AndNot(Set other)
        {
            int max_ulongs = b.Count;
            if (other.b.Count > max_ulongs)
                max_ulongs = other.b.Count;

            while (b.Count < max_ulongs)
                b.Add(0);
            while (other.b.Count < max_ulongs)
                other.b.Add(0);

            for (int i = 0; i < b.Count; i++)
                b[i] &= ~other.b[i];

            recalc_set_count();
        }

        public override bool Equals(object obj)
        {
            var other = obj as Set;
            if (other == null)
                return false;

            int max_ulongs = b.Count;
            if (other.b.Count > max_ulongs)
                max_ulongs = other.b.Count;

            while (b.Count < max_ulongs)
                b.Add(0);
            while (other.b.Count < max_ulongs)
                other.b.Add(0);

            for (int i = 0; i < b.Count; i++)
            {
                if (b[i] != other.b[i])
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hc = 0;
            foreach (var u in b)
                hc = (hc << 4) ^ u.GetHashCode();
            return hc;
        }

        public void Clear()
        {
            for (int i = 0; i < b.Count; i++)
                b[i] = 0;
            set_count = 0;
        }
    }
}
