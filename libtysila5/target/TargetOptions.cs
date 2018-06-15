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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using libtysila5.ir;

namespace libtysila5.target
{
    public partial class Target
    {
        public class TargetOption
        {
            object val;
            Type t;

            public void Set(object v)
            {
                if (t == null)
                {
                    val = v;
                    t = v.GetType();
                }
                else if (v.GetType().Equals(t))
                {
                    val = v;
                }
                else
                    throw new InvalidCastException();
            }

            public bool TrySet(object v)
            {
                if (t == null)
                {
                    val = v;
                    t = v.GetType();
                    return true;
                }
                else if (v.GetType().Equals(t))
                {
                    val = v;
                    return true;
                }
                else
                    return false;
            }

            public object Get()
            {
                return val;
            }

            internal TargetOption(object v)
            {
                Set(v);
            }
        }
        public class TargetOptions : IDictionary<string, object>
        {
            Dictionary<string, TargetOption> d;

            internal TargetOptions()
            {
                d = new Dictionary<string, TargetOption>(new metadata.GenericEqualityComparer<string>());
            }

            internal TargetOptions(IDictionary<string, object> v)
            {
                d = new Dictionary<string, TargetOption>(new metadata.GenericEqualityComparer<string>());
                foreach (var kvp in v)
                    d[kvp.Key] = new TargetOption(kvp.Value);
            }

            internal void InternalAdd(string key, object v)
            {
                d[key] = new TargetOption(v);
            }

            public bool TrySet(string key, object v)
            {
                if (!d.ContainsKey(key))
                    return false;
                return d[key].TrySet(v);
            }

            public object this[string key]
            {
                get
                {
                    return d[key].Get();
                }

                set
                {
                    d[key].Set(value);
                }
            }

            object IDictionary<string, object>.this[string key]
            {
                get
                {
                    return d[key].Get();
                }

                set
                {
                    d[key].Set(value);
                }
            }

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

            public ICollection<string> Keys
            {
                get
                {
                    return d.Keys;
                }
            }

            public ICollection<object> Values
            {
                get
                {
                    List<object> ret = new List<object>();
                    foreach (var v in d.Values)
                        ret.Add(v.Get());
                    return ret;
                }
            }

            int ICollection<KeyValuePair<string, object>>.Count
            {
                get
                {
                    return d.Count;
                }
            }

            bool ICollection<KeyValuePair<string, object>>.IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            ICollection<string> IDictionary<string, object>.Keys
            {
                get
                {
                    return d.Keys;
                }
            }

            ICollection<object> IDictionary<string, object>.Values
            {
                get
                {
                    List<object> ret = new List<object>();
                    foreach (var v in d.Values)
                        ret.Add(v.Get());
                    return ret;
                }
            }

            public void Add(KeyValuePair<string, object> item)
            {
                throw new NotSupportedException();
            }

            public void Add(string key, object value)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(KeyValuePair<string, object> item)
            {
                throw new NotSupportedException();
            }

            public bool ContainsKey(string key)
            {
                return d.ContainsKey(key);
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                foreach(var kvp in d)
                {
                    yield return new KeyValuePair<string, object>(kvp.Key, kvp.Value.Get());
                }
            }

            public bool Remove(KeyValuePair<string, object> item)
            {
                throw new NotSupportedException();
            }

            public bool Remove(string key)
            {
                throw new NotSupportedException();
            }

            public bool TryGetValue(string key, out object value)
            {
                if(d.ContainsKey(key))
                {
                    value = d[key].Get();
                    return true;
                }
                value = null;
                return false;
            }

            void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
            {
                throw new NotSupportedException();
            }

            void IDictionary<string, object>.Add(string key, object value)
            {
                throw new NotSupportedException();
            }

            void ICollection<KeyValuePair<string, object>>.Clear()
            {
                throw new NotSupportedException();
            }

            bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
            {
                throw new NotSupportedException();
            }

            bool IDictionary<string, object>.ContainsKey(string key)
            {
                return d.ContainsKey(key);
            }

            void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                throw new NotSupportedException();
            }

            IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
            {
                foreach (var kvp in d)
                {
                    yield return new KeyValuePair<string, object>(kvp.Key, kvp.Value.Get());
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                foreach (var kvp in d)
                {
                    yield return new KeyValuePair<string, object>(kvp.Key, kvp.Value.Get());
                }
            }

            bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
            {
                throw new NotSupportedException();
            }

            bool IDictionary<string, object>.Remove(string key)
            {
                throw new NotSupportedException();
            }

            bool IDictionary<string, object>.TryGetValue(string key, out object value)
            {
                if (d.ContainsKey(key))
                {
                    value = d[key].Get();
                    return true;
                }
                value = null;
                return false;
            }
        }
    }
}
