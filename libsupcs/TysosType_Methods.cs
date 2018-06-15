/* Copyright (C) 2018 by John Cronin
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

namespace libsupcs
{
    partial class TysosType : Type
    {
        /* represents a cache of all methods for this type - built on demand
         * 
         * methods is a pure list of all methods
         * first_name allows fast indexing by name
         * in the instance two methods have the same name, next_name points to the
         * next in the chain, or -1 if its the end of the chain.
         */
        
        TysosMethod[] methods = null;
        Dictionary<string, int> first_name = null;
        int[] next_name = null;


        protected override System.Reflection.MethodInfo GetMethodImpl(string name, System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Reflection.CallingConventions callConvention, Type[] types, System.Reflection.ParameterModifier[] modifiers)
        {
            if (next_name == null)
                build_method_list();

            System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosType: GetMethodImpl(" + name + ", " + ((types == null) ? "null" :  types.Length.ToString()) + ")");

            if (first_name.TryGetValue(name, out var fn) == false)
            {
                System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosType: GetMethodImpl: no methods by that name found");
                return null;
            }

            while(fn != -1)
            {
                var cur_m = methods[fn];

                // check binding flags
                if (MatchBindingFlags(cur_m, bindingAttr))
                {
                    // check parameters
                    bool match = true;
                    if (types != null)
                    {
                        var p = cur_m.GetParameters();
                        if (p.Length == types.Length)
                        {
                            for (int i = 0; i < p.Length; i++)
                            {
                                if (p[i].ParameterType != types[i])
                                {
                                    System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosType: GetMethodImpl: failing because " +
                                        p[i].ParameterType.FullName + " != " + types[i].FullName);
                                    match = false;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            match = false;
                        }
                    }
                    if (match)
                    {
                        System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosType: GetMethodImpl: match found");
                        return cur_m;
                    }
                }

                fn = next_name[fn];
            }

            System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosType: GetMethodImpl: no match found on attributes/parameters");
            return null;
        }

        private void build_method_list()
        {
            System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosType: building method list for " + FullName);

            var t = tspec;
            var first_mdef = ts.m.GetIntEntry(metadata.MetadataStream.tid_TypeDef, t.tdrow, 5);
            var last_mdef = ts.m.GetLastMethodDef(t.tdrow);

            var _methods = new TysosMethod[last_mdef - first_mdef];
            var _first_name = new Dictionary<string, int>(new metadata.GenericEqualityComparer<string>());
            var _next_name = new int[last_mdef - first_mdef];

            for(int i = 0; i < last_mdef - first_mdef; i++)
                _next_name[i] = -1;

            var cur_idx = 0;
            for(var cur_mdef = first_mdef; cur_mdef < last_mdef; cur_mdef++, cur_idx++)
            {
                metadata.MethodSpec ms = new metadata.MethodSpec { type = t, mdrow = (int)cur_mdef, m = t.m, msig = (int)ts.m.GetIntEntry(metadata.MetadataStream.tid_MethodDef, (int)cur_mdef, 4) };
                var cur_m = new TysosMethod(ms, this);
                System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosType: adding method " + cur_m.Name);
                _methods[cur_idx] = cur_m;
                if (_first_name.TryGetValue(cur_m.Name, out var fn))
                {
                    while (_next_name[fn] != -1)
                        fn = _next_name[fn];
                    _next_name[fn] = cur_idx;
                }
                else
                    _first_name[cur_m.Name] = cur_idx;
            }

            if(System.Threading.Interlocked.CompareExchange(ref methods, _methods, null) == null)
            {
                first_name = _first_name;
                next_name = _next_name;
            }

            System.Diagnostics.Debugger.Log(0, "libsupcs", "TysosType: done building method list");
        }
    }
}
