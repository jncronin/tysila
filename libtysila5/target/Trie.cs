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

using System.Collections.Generic;

namespace libtysila5.target
{
    public class Trie<T>
    {
        public int[] trie;
        public int start;
        public T[] vals;

        public int MaxDepth
        {
            get
            {
                return trie_get_max_depth(start);
            }
        }

        public T GetValue(int[] keys)
        {
            var max_depth = trie_get_max_depth(start);
            if (keys.Length > max_depth)
                return default(T);
            return trie_get(start, vals, keys, 0);
        }

        public T GetValue(List<cil.CilNode.IRNode> nodes,
            int start_key, int count)
        {
            var max_depth = trie_get_max_depth(start);
            if (count > max_depth)
                return default(T);
            return trie_get(start, vals, nodes, start_key, count, 0);
        }

        private int trie_get_max_depth(int idx)
        {
            return trie[idx + 1];
        }

        private int trie_get_start(int idx)
        {
            return trie[idx + 2];
        }

        private int trie_get_length(int idx)
        {
            return trie[idx + 3];
        }

        private int trie_get_next_trie(int idx, int next_key)
        {
            var start = trie_get_start(idx);
            var length = trie_get_length(idx);
            var end = start + length;

            if (next_key < start || next_key >= end)
                return 0;
            return trie[idx + 4 + next_key - start];
        }

        private T trie_get(int idx, T[] vals, int[] keys, int v)
        {
            if (v >= keys.Length)
            {
                return trie_get_val(idx, vals);
            }

            var next_key = keys[v];
            var next_trie = trie_get_next_trie(idx, next_key);
            if (next_trie == 0)
                return default(T);
            return trie_get(next_trie, vals, keys, v + 1);
        }

        private T trie_get(int idx, T[] vals,
            List<cil.CilNode.IRNode> nodes,
            int start_key, int count, int v)
        {
            if (v >= count)
            {
                return trie_get_val(idx, vals);
            }

            var next_key = nodes[start_key + v].opcode;
            var next_trie = trie_get_next_trie(idx, next_key);
            if (next_trie == 0)
                return default(T);
            return trie_get(next_trie, vals, nodes,
                start_key, count, v + 1);
        }

        private T trie_get_val(int idx, T[] vals)
        {
            return vals[trie[idx]];
        }

    }
}
