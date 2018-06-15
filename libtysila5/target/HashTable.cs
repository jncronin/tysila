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
using System.IO;
using System.Text;

namespace libtysila5.target
{
    public class HashTable
    {
        public int nbucket;
        public int nchain;
        public int[] bucket;
        public int[] chain;
        public int[] idx_map;
        public byte[] data;

        public int GetBlobIndex(IList<byte> key)
        {
            var hc = Hash(key);

            var bucket_id = hc % (uint)nbucket;
            var cur_idx = bucket[bucket_id];

            while (cur_idx != -1)
            {
                if (CompareKey(cur_idx, key))
                    return idx_map[cur_idx];
                cur_idx = chain[cur_idx];
            }

            return -1;
        }

        public uint ReadCompressedUInt(ref int idx)
        {
            byte b1 = data[idx++];
            if ((b1 & 0x80) == 0)
                return b1;

            byte b2 = data[idx++];
            if ((b1 & 0xc0) == 0x80)
                return (b1 & 0x3fU) << 8 | b2;

            byte b3 = data[idx++];
            byte b4 = data[idx++];
            return (b1 & 0x1fU) << 24 | ((uint)b2 << 16) |
                ((uint)b3 << 8) | b4;
        }

        public int GetValueIndex(int blob_index)
        {
            var key_len = data[blob_index];
            return blob_index + 1 + key_len;
        }

        private bool CompareKey(int cur_idx, IList<byte> key)
        {
            // get key length
            var blob_idx = idx_map[cur_idx];
            var key_len = data[blob_idx];

            if (key_len != key.Count)
                return false;

            blob_idx++;
            for(int i = 0; i < key_len; i++)
            {
                if (data[blob_idx + i] != key[i])
                    return false;
            }
            return true;
        }

        public static uint Hash(IEnumerable<byte> v)
        {
            uint h = 0;
            uint g = 0;

            foreach (var b in v)
            {
                h = (h << 4) + b;
                g = h & 0xf0000000U;
                if (g != 0)
                    h ^= g >> 24;
                h &= ~g;
            }
            return h;
        }

        public static void CompressInt(int val, List<byte> ret)
        {
            var u = BitConverter.ToUInt32(BitConverter.GetBytes(val), 0);

            CompressUInt(u, ret);
        }

        public static void CompressUInt(uint u, List<byte> ret)
        {
            var b1 = u & 0xff;
            var b2 = (u >> 8) & 0xff;
            var b3 = (u >> 16) & 0xff;
            var b4 = (u >> 24) & 0xff;

            if (u <= 0x7fU)
            {
                ret.Add((byte)b1);
                return;
            }
            else if (u <= 0x3fffU)
            {
                ret.Add((byte)(b2 | 0x80U));
                ret.Add((byte)b1);
            }
            else if (u <= 0x1FFFFFFFU)
            {
                ret.Add((byte)(b4 | 0xc0U));
                ret.Add((byte)b3);
                ret.Add((byte)b2);
                ret.Add((byte)b1);
            }
            else
                throw new Exception("integer too large to compress");
        }
    }
}
