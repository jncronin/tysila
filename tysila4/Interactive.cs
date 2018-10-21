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
using System.Text;
using System.Threading.Tasks;
using metadata;

namespace tysila4
{
    class Interactive
    {
        class InteractiveState
        {
            public InteractiveMetadataStream m = null;
            public InteractiveTypeSpec ts = null;
            public InteractiveMethodSpec ms = null;
        }

        class InteractiveMethodSpec
        {
            public metadata.MethodSpec ms = null;
            public string Name;

            public static implicit operator InteractiveMethodSpec(metadata.MethodSpec _ms) { return new InteractiveMethodSpec(_ms); }
            public static implicit operator MethodSpec(InteractiveMethodSpec ims) { return ims.ms; }

            public InteractiveMethodSpec(metadata.MethodSpec _ms)
            {
                ms = _ms;

                StringBuilder sb = new StringBuilder();

                var meth_name = ms.m.GetStringEntry(metadata.MetadataStream.tid_MethodDef, ms.mdrow, 3);
                var cur_sig = (int)ms.m.GetIntEntry(metadata.MetadataStream.tid_MethodDef, ms.mdrow, 4);

                sb.Append(meth_name);
                if(ms.IsInstantiatedGenericMethod)
                {
                    sb.Append("<");
                    for(int idx = 0; idx < ms.gmparams.Length; idx++)
                    {
                        if (idx > 0)
                            sb.Append(",");
                        sb.Append(((InteractiveTypeSpec)ms.gmparams[idx]).Name);
                    }
                    sb.Append(">");
                }
                sb.Append("(");

                var need_this = ms.m.GetMethodDefSigHasNonExplicitThis(cur_sig);
                var pcount = ms.m.GetMethodDefSigParamCount(cur_sig);
                cur_sig = ms.m.GetMethodDefSigRetTypeIndex(cur_sig);

                if(need_this)
                {
                    sb.Append("this");
                }

                // skip ret type
                ms.m.GetTypeSpec(ref cur_sig, ms.gtparams, ms.gmparams);
                for(int idx = 0; idx < pcount; idx++)
                {
                    if (idx > 0 || need_this)
                        sb.Append(",");
                    sb.Append(((InteractiveTypeSpec)ms.m.GetTypeSpec(ref cur_sig, ms.gtparams, ms.gmparams)).Name);
                }
                sb.Append(")");

                Name = sb.ToString();
            }
        }

        class InteractiveTypeSpec
        {
            public metadata.TypeSpec ts = null;
            public string Name;

            public InteractiveTypeSpec() { }
            public InteractiveTypeSpec(metadata.TypeSpec _ts)
            {
                ts = _ts;

                StringBuilder sb = new StringBuilder();

                GetName(ts, sb);

                Name = sb.ToString();
            }

            private void GetName(TypeSpec ts, StringBuilder sb)
            {
                switch (ts.stype)
                {
                    case metadata.TypeSpec.SpecialType.None:
                        sb.Append(ts.Namespace + "." + ts.Name.Replace('+', '.'));
                        if (ts.IsInstantiatedGenericType)
                        {
                            sb.Append("<");
                            for (int idx = 0; idx < ts.gtparams.Length; idx++)
                            {
                                if (idx > 0)
                                    sb.Append(",");
                                sb.Append(((InteractiveTypeSpec)ts.gtparams[idx]).Name);
                            }
                            sb.Append(">");
                        }
                        break;
                    case TypeSpec.SpecialType.SzArray:
                        GetName(ts.other, sb);
                        sb.Append("[]");
                        break;
                    case TypeSpec.SpecialType.Ptr:
                        GetName(ts.other, sb);
                        sb.Append("*");
                        break;
                    case TypeSpec.SpecialType.Var:
                        sb.Append("!");
                        sb.Append('T' + ts.idx);
                        break;
                    case TypeSpec.SpecialType.MVar:
                        sb.Append("!!");
                        sb.Append('t' + ts.idx);
                        break;
                    case TypeSpec.SpecialType.Boxed:
                        GetName(ts.other, sb);
                        break;
                    case TypeSpec.SpecialType.MPtr:
                        GetName(ts.other, sb);
                        sb.Append("&");
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            public static implicit operator InteractiveTypeSpec(metadata.TypeSpec _ts) { return new InteractiveTypeSpec(_ts); }
            public static implicit operator metadata.TypeSpec(InteractiveTypeSpec its) { return its.ts; }

            internal Dictionary<string, InteractiveMethodSpec> all_methods = null;

            public Dictionary<string, InteractiveMethodSpec> AllMethods
            {
                get
                {
                    if (all_methods == null)
                    {
                        all_methods = new Dictionary<string, InteractiveMethodSpec>();

                        var first_mdef = ts.m.GetIntEntry(metadata.MetadataStream.tid_TypeDef, ts.tdrow, 5);
                        var last_mdef = ts.m.GetLastMethodDef(ts.tdrow);

                        for (uint mdef_row = first_mdef; mdef_row < last_mdef; mdef_row++)
                        {
                            var cur_sig = (int)ts.m.GetIntEntry(metadata.MetadataStream.tid_MethodDef, (int)mdef_row, 4);

                            var ms = new metadata.MethodSpec { m = ts.m, type = ts, mdrow = (int)mdef_row, msig = cur_sig };

                            var ims = new InteractiveMethodSpec(ms);
                            all_methods[ims.Name] = ims;
                        }
                    }
                    return all_methods;
                }
            }
        }

        class InteractiveMetadataStream
        {
            public metadata.MetadataStream m = null;

            internal Dictionary<string, InteractiveTypeSpec> all_types = null;

            public Dictionary<string, InteractiveTypeSpec> AllTypes
            {
                get
                {
                    if(all_types == null)
                    {
                        all_types = new Dictionary<string, InteractiveTypeSpec>();

                        for (int i = 1; i <= m.table_rows[metadata.MetadataStream.tid_TypeDef]; i++)
                        {
                            var ts = new metadata.TypeSpec { m = m, tdrow = i };
                            var name = ts.Namespace + "." + ts.Name.Replace('+', '.');

                            all_types[name] = new InteractiveTypeSpec { Name = name, ts = ts };
                        }
                    }

                    /* Add in basic corlib types */
                    var mscorlib = m.al.GetAssembly("mscorlib");
                    all_types["string"] = mscorlib.GetTypeSpec("System", "String");
                    all_types["object"] = mscorlib.GetTypeSpec("System", "Object");
                    all_types["int"] = mscorlib.GetTypeSpec("System", "Int32");
                    all_types["uint"] = mscorlib.GetTypeSpec("System", "UInt32");
                    all_types["long"] = mscorlib.GetTypeSpec("System", "Int64");
                    all_types["ulong"] = mscorlib.GetTypeSpec("System", "UInt64");
                    all_types["short"] = mscorlib.GetTypeSpec("System", "Int16");
                    all_types["ushort"] = mscorlib.GetTypeSpec("System", "UInt16");
                    all_types["byte"] = mscorlib.GetTypeSpec("System", "Byte");
                    all_types["sbyte"] = mscorlib.GetTypeSpec("System", "SByte");
                    all_types["char"] = mscorlib.GetTypeSpec("System", "Char");
                    all_types["float"] = mscorlib.GetTypeSpec("System", "Single");
                    all_types["double"] = mscorlib.GetTypeSpec("System", "Double");
                    
                    return all_types;
                }
            }
        }

        InteractiveState s = new InteractiveState();
        libtysila5.target.Target t;
        InteractiveMetadataStream corlib;
        AutoCompleteHandler ach;

        List<string> cmds = new List<string>
        {
            "select.type",
            "select.method",
            "quit",
            "q",
            "continue",
            "c",
            "assemble.method",
            "list.interfaces",
            "list.methods",
            "implement.interface",
            "list.vmethods",
        };

        internal Interactive(metadata.MetadataStream metadata, libtysila5.target.Target target)
        {
            s.m = new InteractiveMetadataStream { m = metadata };
            t = target;
            corlib = new InteractiveMetadataStream { m = metadata.al.GetAssembly("mscorlib") };
        }

        internal bool DoInteractive()
        {
            ach = new AutoCompleteHandler(this);
            ReadLine.AutoCompletionHandler = ach;
            ReadLine.HistoryEnabled = true;

            while(true)
            {
                dump_state();

                string[] cmd;
                while ((cmd = get_command()).Length == 0) ;

                // handle the command
                int idx = 0;
                if (cmd[idx] == "select.type")
                {
                    idx++;
                    s.m = ParseModule(cmd, ref idx);
                    s.ts = ParseType(cmd, ref idx);
                    s.ts = instantiate_type(s.ts);
                }
                else if (cmd[idx] == "select.method")
                {
                    idx++;
                    s.m = ParseModule(cmd, ref idx);
                    s.ts = ParseType(cmd, ref idx);
                    s.ms = ParseMethod(cmd, ref idx);
                }
                else if (cmd[idx] == "quit" || cmd[idx] == "q")
                {
                    return false;
                }
                else if (cmd[idx] == "continue" || cmd[idx] == "c")
                {
                    return true;
                }
                else if (cmd[idx] == "assemble.method")
                {
                    idx++;
                    s.m = ParseModule(cmd, ref idx);
                    s.ts = ParseType(cmd, ref idx);
                    s.ms = ParseMethod(cmd, ref idx);

                    t.InitIntcalls();
                    t.r = new libtysila5.CachingRequestor(s.m.m);
                    StringBuilder sb = new StringBuilder();
                    libtysila5.libtysila.AssembleMethod(s.ms.ms, new binary_library.binary.FlatBinaryFile(), t, sb);

                    Console.WriteLine(sb.ToString());
                }
                else if (cmd[idx] == "list.methods")
                {
                    idx++;
                    s.m = ParseModule(cmd, ref idx);
                    s.ts = ParseType(cmd, ref idx);

                    foreach (var meth in s.ts.AllMethods.Keys)
                        Console.WriteLine(meth);
                }
                else if (cmd[idx] == "list.vmethods")
                {
                    idx++;

                    s.m = ParseModule(cmd, ref idx);
                    s.ts = ParseType(cmd, ref idx);

                    var vmeths = libtysila5.layout.Layout.GetVirtualMethodDeclarations(s.ts);
                    libtysila5.layout.Layout.ImplementVirtualMethods(s.ts, vmeths);

                    foreach (var vmeth in vmeths)
                    {
                        var impl_ms = vmeth.impl_meth;
                        string impl_target = (impl_ms == null) ? "__cxa_pure_virtual" : impl_ms.MangleMethod();
                        var ims = new InteractiveMethodSpec(vmeth.unimpl_meth);
                        Console.WriteLine(ims.Name + " -> " + impl_target);
                    }
                }
                else if (cmd[idx] == "list.interfaces")
                {
                    idx++;
                    s.m = ParseModule(cmd, ref idx);
                    s.ts = ParseType(cmd, ref idx);

                    foreach (var ii in s.ts.ts.ImplementedInterfaces)
                        Console.WriteLine(((InteractiveTypeSpec)ii).Name);
                }
                else if (cmd[idx] == "implement.interface")
                {
                    idx++;

                    InteractiveState istate = new InteractiveState();
                    istate.m = ParseModule(cmd, ref idx);
                    istate.ts = ParseType(cmd, ref idx, istate);

                    t.InitIntcalls();
                    t.r = new libtysila5.CachingRequestor(s.m.m);

                    var iis = libtysila5.layout.Layout.ImplementInterface(s.ts, istate.ts, t);
                    foreach (var ii in iis)
                    {
                        Console.WriteLine(((InteractiveMethodSpec)ii.InterfaceMethod).Name + " -> " + ii.TargetName);
                    }
                }
                else
                {
                    Console.WriteLine("Unknown command: " + cmd[idx]);
                }
            }

            throw new NotImplementedException();

            return false;
        }

        private InteractiveTypeSpec instantiate_type(InteractiveTypeSpec ts)
        {
            if (ts.ts.IsGenericTemplate)
            {
                TypeSpec[] gtparams = new TypeSpec[ts.ts.GenericParamCount];
                for(int i = 0; i < ts.ts.GenericParamCount; i++)
                {
                    string[] cmd;
                    ach.JustType = true;
                    while ((cmd = get_command("GP" + i.ToString() + "> ")).Length == 0) ;
                    ach.JustType = false;
                    int idx = 0;
                    var type = ParseType(cmd, ref idx);
                    gtparams[i] = type;
                }
                ts.ts.gtparams = gtparams;
                return ts;
            }
            else
                return ts;
        }

        private InteractiveMethodSpec ParseMethod(string[] cmd, ref int idx)
        {
            if (idx >= (cmd.Length - 1) || cmd[idx] != ":")
                return s.ms;
            idx++;
            var mname = cmd[idx++];
            if (s.ts.AllMethods.ContainsKey(mname))
            {
                var new_ms = s.ts.AllMethods[mname];
                return new_ms;
            }
            else
            {
                Console.WriteLine("Method: " + mname + " not found in [" + s.m.m.AssemblyName + "]" + s.ts.ts.Namespace + "." + s.ts.ts.Name.Replace('+', '.'));
                return s.ms;
            }
        }

        private InteractiveMetadataStream ParseModule(string[] cmd, ref int idx)
        {
            if (idx >= (cmd.Length - 2) || cmd[idx] != "[" || cmd[idx + 2] != "]")
                return s.m;
            var new_m = s.m.m.al.GetAssembly(cmd[idx + 1]);
            idx += 3;
            return new InteractiveMetadataStream { m = new_m };
        }

        private InteractiveTypeSpec ParseType(string[] cmd, ref int idx, InteractiveState state = null)
        {
            if (state == null)
                state = s;
            if (idx >= cmd.Length || cmd[idx] == ":")
                return state.ts;

            var tname = cmd[idx++];

            InteractiveTypeSpec new_ts = null;
            if(state.m.AllTypes.ContainsKey(tname))
                new_ts = state.m.AllTypes[tname];
            else if(corlib.AllTypes.ContainsKey(tname))
                new_ts = corlib.AllTypes[tname];

            if(new_ts == null)
            {
                Console.WriteLine("Type: " + tname + " not found in " + state.m.m.AssemblyName);
                return state.ts;
            }

            if(idx < cmd.Length)
            {
                if(cmd[idx] == "<")
                {
                    // handle generics
                    idx++;
                    var gtparams = new List<TypeSpec>();
                    while (cmd[idx] != ">")
                    {
                        gtparams.Add(ParseType(cmd, ref idx, state));
                        if (cmd[idx] == ",")
                            idx++;
                    }
                    idx++;
                    new_ts.ts.gtparams = gtparams.ToArray();
                }
            }

            return new_ts;
        }



        private string[] get_command(string prompt = "> ")
        {
            var cmd = get_string(prompt);
            return AutoCompleteHandler.Tokenize(cmd, AutoCompleteHandler.seps).ToArray();
        }

        class AutoCompleteHandler : IAutoCompleteHandler
        {
            internal static char[] seps = new char[] { ' ', '>', '<', '[', ']', ':' };
            public char[] Separators { get; set; } = seps;

            Interactive i;
            internal bool JustType;

            public AutoCompleteHandler(Interactive interactive) { i = interactive; }

            public string[] GetSuggestions(string text, int index)
            {
                string match_text = text.Substring(index);
                string context_text = text.Substring(0, index);

                List<string> opts;
                if (JustType)
                {
                    var ctx = Tokenize(context_text, Separators);
                    int idx = 0;
                    opts = ParseTypeForOptions(ctx, ref idx, i.s);
                }
                else   
                    opts = GetContext(context_text);

                List<string> ret = new List<string>();
                foreach(var test in opts)
                {
                    if (test.StartsWith(match_text))
                        ret.Add(test);
                }

                if (ret.Count == 0)
                    return new string[] { };
                if (ret.Count == 1)
                    return new string[] { ret[0] + " " };   // if only one entry can put space after

                /* Find common starting characters */
                int start_common = 0;
                while(true)
                {
                    char cur_c = ' ';           // matching character at position start_common
                    bool is_first = true;       // is this the first string in the foreach loop?
                    foreach(var str in ret)
                    {
                        if (start_common >= str.Length)
                            return new string[] { ret[0].Substring(0, start_common) };

                        if (is_first)
                        {
                            cur_c = str[start_common];
                            is_first = false;
                        }
                        else
                        {
                            if (str[start_common] != cur_c)
                                return new string[] { ret[0].Substring(0, start_common) };
                        }
                    }
                    start_common++;
                }
            }

            private List<string> GetContext(string context_text)
            {
                context_text = context_text.Trim();
                if (context_text == string.Empty)
                    return i.cmds;

                var ctx = Tokenize(context_text, Separators);

                int idx = 0;
                var state = new InteractiveState { m = i.s.m, ms = i.s.ms, ts = i.s.ts };
                return ParseCommandForOptions(ctx, ref idx, state);
                throw new NotImplementedException();
            }

            private List<string> ParseCommandForOptions(List<string> ctx, ref int idx, InteractiveState state)
            {
                var cmd = ctx[idx++];
                if (cmd == "select.type")
                    return ParseTypeForOptions(ctx, ref idx, state);
                if (cmd == "implement.interface")
                    return ParseInterfaceForOptions(ctx, ref idx, state);
                else if (cmd == "select.method")
                    return ParseMethodForOptions(ctx, ref idx, state);
                else if (cmd == "assemble.method")
                {
                    if (idx < ctx.Count || state.ms == null)
                        return ParseMethodForOptions(ctx, ref idx, state);
                    else
                        return new List<string>();
                }
                else if (cmd == "list.methods" || cmd == "list.interfaces" || cmd == "list.vmethods")
                {
                    if (idx < ctx.Count || state.ts == null)
                        return ParseTypeForOptions(ctx, ref idx, state);
                    else
                        return new List<string>();
                }
                return new List<string>();
                throw new NotImplementedException();
            }

            private List<string> ParseInterfaceForOptions(List<string> ctx, ref int idx, InteractiveState state)
            {
                if (idx >= ctx.Count && state.ts != null)
                {
                    var ret = new List<string>();
                    foreach (var ii in state.ts.ts.ImplementedInterfaces)
                        ret.Add(((InteractiveTypeSpec)ii).Name);
                    return ret;
                }
                return new List<string>();
            }

            private List<string> ParseMethodForOptions(List<string> ctx, ref int idx, InteractiveState state)
            {
                // methods may be described by name or type:name
                var ret = ParseTypeForOptions(ctx, ref idx, state);
                if (ret == null)
                    ret = new List<string>();

                if (idx < ctx.Count && ctx[idx] == ":")
                    idx++;

                if (idx >= ctx.Count && state.ts != null)
                    ret.AddRange(state.ts.AllMethods.Keys);

                return ret;
            }

            private List<string> ParseTypeForOptions(List<string> ctx, ref int idx, InteractiveState state)
            {
                if(idx >= ctx.Count)
                {
                    // no types listed, return all in the current module
                    return new List<string>(state.m.AllTypes.Keys);
                }
                if(ctx[idx] == "[")
                {
                    idx++;
                    var ret = ParseModuleForOptions(ctx, ref idx, state);
                    if (ret != null)
                        return ret;
                }
                if (state.m.AllTypes.ContainsKey(ctx[idx]))
                    state.ts = state.m.AllTypes[ctx[idx++]];
                return null;
            }

            private List<string> ParseModuleForOptions(List<string> ctx, ref int idx, InteractiveState state)
            {
                if(idx >= ctx.Count)
                {
                    // end of the parse, return all loaded modules
                    return new List<string>(state.m.m.al.LoadedAssemblies);
                }

                // else, make the new module our current scope
                var mod_list = new List<string>(state.m.m.al.LoadedAssemblies);
                var new_mod = ctx[idx++];
                if(mod_list.Contains(new_mod))
                {
                    var new_m = new InteractiveMetadataStream { m = state.m.m.al.GetAssembly(new_mod) };
                    state.m = new_m;
                }

                if(idx++ >= ctx.Count)
                {
                    // must terminate a module with closing bracket
                    return new List<string> { "]" };
                }

                return ParseTypeForOptions(ctx, ref idx, state);
            }

            internal static List<string> Tokenize(string text, char[] separators)
            {
                List<string> ret = new List<string>();

                StringBuilder sb = new StringBuilder();
                foreach(var c in text)
                {
                    bool in_sep = false;
                    foreach(var sep in separators)
                    {
                        if(sep == c)
                        {
                            in_sep = true;
                            break;
                        }
                    }

                    if (in_sep)
                    {
                        var txt = sb.ToString().Trim();
                        if (txt.Length > 0)
                            ret.Add(txt);
                        sb = new StringBuilder();
                        if(c != ' ')
                            ret.Add(new string(new char[] { c }));
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }

                var txt2 = sb.ToString().Trim();
                if (txt2.Length > 0)
                    ret.Add(txt2);

                return ret;
            }
        }

        private string get_string(string prompt = "")
        {
            return ReadLine.Read(prompt);
        }

        private void dump_state()
        {
            Console.WriteLine();
            Console.WriteLine("Module: " + s.m.m.FullName);
            Console.WriteLine("Type: " + (s.ts == null ? "{null}" : s.ts.Name));
            Console.WriteLine("Method: " + (s.ms == null ? "{null}" : s.ms.Name));
        }
    }
}
