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
using System.Linq;
using System.Text;

namespace TableMap
{
    class Program
    {
        static internal StreamWriter sw;
        static void Main(string[] args)
        {

            var files = new string[]
            {
                @"D:\tysos\libtysila5\ir\IrMappings.td",
                @"D:\tysos\libtysila5\ir\IrOpcodes.td",
                @"D:\tysos\libtysila5\target\Target.td",
            };

            var this_file = System.Reflection.Assembly.GetEntryAssembly().Location;
            var this_fi = new FileInfo(this_file);

            foreach (var file in args)
            {
                MakeState s = new MakeState();

                s.search_paths.Add(this_fi.DirectoryName);

                new PrintFunction { args = new List<FunctionStatement.FunctionArg> { new FunctionStatement.FunctionArg { name = "val", argtype = Expression.EvalResult.ResultType.Int } } }.Execute(s);
                new PrintFunction { args = new List<FunctionStatement.FunctionArg> { new FunctionStatement.FunctionArg { name = "val", argtype = Expression.EvalResult.ResultType.String } } }.Execute(s);
                new PrintFunction { args = new List<FunctionStatement.FunctionArg> { new FunctionStatement.FunctionArg { name = "val", argtype = Expression.EvalResult.ResultType.Object } } }.Execute(s);
                new PrintFunction { args = new List<FunctionStatement.FunctionArg> { new FunctionStatement.FunctionArg { name = "val", argtype = Expression.EvalResult.ResultType.Array } } }.Execute(s);
                new VarGenFunction(Expression.EvalResult.ResultType.Int).Execute(s);
                new VarGenFunction(Expression.EvalResult.ResultType.Array).Execute(s);
                new VarGenFunction(Expression.EvalResult.ResultType.String).Execute(s);
                new VarGenFunction(Expression.EvalResult.ResultType.Object).Execute(s);
                new VarGetFunction().Execute(s);
                new DefineBlobFunction().Execute(s);
                new ToIntFunction().Execute(s);
                new DumpBlobFunction().Execute(s);
                new ToByteArrayFunction(4).Execute(s);
                new ToByteArrayFunction(2).Execute(s);
                new ToByteArrayFunction(1).Execute(s);
                new ToByteArrayFunction(8).Execute(s);
                new ThrowFunction().Execute(s);

                FileInfo fi = new FileInfo(file);

                string output = fi.FullName;
                output = output.Substring(0, output.Length - fi.Extension.Length) + ".cs";

                FileStream fs = new FileStream(output, FileMode.Create,
                    FileAccess.Write);
                sw = new StreamWriter(fs);

                // Boilerplate
                BoilerPlate(sw, output, file);

                ExecuteFile(file, s);
                sw.Close();
            }
        }

        private static void BoilerPlate(StreamWriter sw, string output, string file)
        {
            sw.Write("/* " + output + "\n");
            sw.Write(" * This is an auto-generated file\n");
            sw.Write(" * DO NOT EDIT\n");
            sw.Write(" * It was generated at " + DateTime.Now.ToLongTimeString() +
                " on " + DateTime.Now.ToLongDateString() + "\n");
            sw.Write(" * from " + file + "\n");
            sw.Write(" * by TableMap (part of tysos: http://www.tysos.org)\n");
            sw.Write(" * Please edit the source file, rather than this file, to make any changes\n");
            sw.Write(" */\n");
            sw.Write("\n");
        }

        internal static Expression.EvalResult ExecuteFile(string name, MakeState s)
        {
            // find the file by using search paths
            FileInfo fi = null;
            foreach(var sp in s.search_paths)
            {
                var test = sp + "/" + name;
                var test2 = sp + name;

                try
                {
                    fi = new FileInfo(test);
                    if (fi.Exists)
                        break;
                }
                catch (Exception) { }
                try
                {
                    fi = new FileInfo(test2);
                    if (fi.Exists)
                        break;
                }
                catch (Exception) { }
            }
            if (fi == null || fi.Exists == false)
                throw new Exception("included file: " + name + " not found");

            // add included files location to search paths
            s.search_paths.Insert(0, fi.DirectoryName);

            FileStream f = fi.OpenRead();
            Parser p = new Parser(new Scanner(f, fi.FullName));
            bool res = p.Parse();
            if (res == false)
                throw new Exception("Parse error");
            var ret = p.output.Execute(s);
            f.Close();
            return ret;
        }
    }

    class VarGenFunction : FunctionStatement
    {
        internal static Dictionary<string, Expression.EvalResult> all_defs =
            new Dictionary<string, Expression.EvalResult>();

        public VarGenFunction(Expression.EvalResult.ResultType arg_type)
        {
            name = "vargen";
            args = new List<FunctionArg>() {
                new FunctionArg { argtype = Expression.EvalResult.ResultType.String },
                new FunctionArg { argtype = arg_type }
            };
        }

        public override Expression.EvalResult Run(MakeState s, List<Expression.EvalResult> passed_args)
        {
            var n = passed_args[0].strval;

            s.SetDefine(n, passed_args[1], true);
            all_defs[n] = passed_args[1];

            return passed_args[1];
        }
    }

    class ToIntFunction : FunctionStatement
    {
        public ToIntFunction()
        {
            name = "toint";
            args = new List<FunctionArg>() { new FunctionArg { argtype = Expression.EvalResult.ResultType.String } };
        }

        public override Expression.EvalResult Run(MakeState s, List<Expression.EvalResult> passed_args)
        {
            return new Expression.EvalResult(int.Parse(passed_args[0].strval));
        }
    }

    class ThrowFunction : FunctionStatement
    {
        public ThrowFunction()
        {
            name = "throw";
            args = new List<FunctionArg> { new FunctionArg { argtype = Expression.EvalResult.ResultType.String }, new FunctionArg { argtype = Expression.EvalResult.ResultType.String } };
        }

        public override Expression.EvalResult Run(MakeState s, List<Expression.EvalResult> passed_args)
        {
            var obj_type = passed_args[0].strval;
            var msg = passed_args[1].strval;

            var obj_ts = Type.GetType(obj_type);
            var obj_ctor = obj_ts.GetConstructor(new Type[] { typeof(string) });
            var obj = obj_ctor.Invoke(new object[] { msg });
            throw obj as Exception;
        }
    }

    class ToByteArrayFunction : FunctionStatement
    {
        int bc = 0;

        public ToByteArrayFunction(int byte_count)
        {
            name = "tobytearray" + byte_count.ToString();
            args = new List<FunctionArg> { new FunctionArg { argtype = Expression.EvalResult.ResultType.Any } };
            bc = byte_count;
        }

        public override Expression.EvalResult Run(MakeState s, List<Expression.EvalResult> passed_args)
        {
            IList<byte> ret = GetBytes(passed_args[0]);
            if (ret == null)
                throw new Exception("Cannot call " + name + " with " + passed_args[0].ToString());

            List<Expression.EvalResult> r = new List<Expression.EvalResult>();
            for(int i = 0; i < bc; i++)
            {
                if (i < ret.Count)
                    r.Add(new Expression.EvalResult(ret[i]));
                else
                    r.Add(new Expression.EvalResult(0));
            }
            return new Expression.EvalResult(r);
        }

        private IList<byte> GetBytes(Expression.EvalResult e)
        {
            switch (e.Type)
            {
                case Expression.EvalResult.ResultType.Int:
                    return BitConverter.GetBytes(e.intval);
                case Expression.EvalResult.ResultType.String:
                    return Encoding.UTF8.GetBytes(e.strval);
                case Expression.EvalResult.ResultType.Array:
                    var ret = new List<byte>();
                    foreach (var aentry in e.arrval)
                        ret.AddRange(GetBytes(aentry));
                    return ret;
                default:
                    return null;
            }
        }
    }

    class VarGetFunction : FunctionStatement
    {
        public VarGetFunction()
        {
            name = "varget";
            args = new List<FunctionArg>()
            {
                new FunctionArg { argtype = Expression.EvalResult.ResultType.String }
            };
        }

        public override Expression.EvalResult Run(MakeState s, List<Expression.EvalResult> passed_args)
        {
            return VarGenFunction.all_defs[passed_args[0].strval];
        }
    }

    class DefineBlobFunction : FunctionStatement
    {
        /* Used to generate a hash table
            
            Arguments are:
                name
                key
                value

            Hash table contains three parts:
                bucket list
                chain list
                data list
                    - concatenations of:
                        1 byte: length of key
                        key
                        value

        */

        public static Dictionary<string, HTable> tables = new Dictionary<string, HTable>();

        public class HTable
        {
            public List<byte> data = new List<byte>();
            public List<KeyIndex> keys = new List<KeyIndex>();
            
            public class KeyIndex
            {
                public List<byte> key = new List<byte>();
                public int idx;
                public uint hc;
            }
        }
        
        public DefineBlobFunction()
        {
            name = "defblob";
            args = new List<FunctionArg>()
            {
                new FunctionArg { argtype = Expression.EvalResult.ResultType.String },
                new FunctionArg { argtype = Expression.EvalResult.ResultType.Array },
                new FunctionArg { argtype = Expression.EvalResult.ResultType.Array }
            };
        }

        public override Expression.EvalResult Run(MakeState s, List<Expression.EvalResult> passed_args)
        {
            var name = passed_args[0].strval;
            var key = passed_args[1].arrval;
            var value = passed_args[2].arrval;

            /* Coerce key and value to byte arrays */
            var key_b = ToByteArray(key);
            var value_b = ToByteArray(value);

            /* Get hash code */
            var hc = Hash(key_b);

            /* Get hash table */
            HTable ht;
            if(tables.TryGetValue(name, out ht) == false)
            {
                ht = new HTable();
                tables[name] = ht;
            }

            /* Build table entry */
            HTable.KeyIndex k = new HTable.KeyIndex();
            k.hc = hc;
            k.idx = ht.data.Count;
            k.key = key_b;
            ht.keys.Add(k);

            /* Add data entry */
            if (key_b.Count > 255)
                throw new Exception("key too large");
            ht.data.Add((byte)key_b.Count);
            ht.data.AddRange(key_b);
            ht.data.AddRange(value_b);

            return new Expression.EvalResult();
        }

        public static uint Hash(IEnumerable<byte> v)
        {
            uint h = 0;
            uint g = 0;

            foreach(var b in v)
            {
                h = (h << 4) + b;
                g = h & 0xf0000000U;
                if (g != 0)
                    h ^= g >> 24;
                h &= ~g;
            }
            return h;
        }

        private List<byte> ToByteArray(List<Expression.EvalResult> v)
        {
            List<byte> ret = new List<byte>();
            foreach(var b in v)
            {
                ToByteArray(b, ret);
            }
            return ret;
        }

        public List<byte> ToByteArray(Expression.EvalResult v)
        {
            List<byte> ret = new List<byte>();
            ToByteArray(v, ret);
            return ret;
        }

        public void CompressInt(int val, List<byte> ret)
        {
            var u = BitConverter.ToUInt32(BitConverter.GetBytes(val), 0);

            CompressUInt(u, ret);
        }

        public void CompressUInt(uint u, List<byte> ret)
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

        public void ToByteArray(Expression.EvalResult v, List<byte> ret)
        {
            switch(v.Type)
            {
                case Expression.EvalResult.ResultType.Array:
                    foreach (var a in v.arrval)
                        ToByteArray(a, ret);
                    break;
                case Expression.EvalResult.ResultType.Int:
                    CompressInt((int)v.intval, ret);
                    /*ret.Add((byte)(v.intval & 0xff));
                    ret.Add((byte)((v.intval >> 8) & 0xff));
                    ret.Add((byte)((v.intval >> 16) & 0xff));
                    ret.Add((byte)((v.intval >> 24) & 0xff));*/
                    break;
                case Expression.EvalResult.ResultType.Object:
                    var vlist = new List<string>();
                    foreach (var kvp in v.objval)
                        vlist.Add(kvp.Key);
                    vlist.Sort();
                    foreach (var k in vlist)
                        ToByteArray(v.objval[k]);
                    break;
                case Expression.EvalResult.ResultType.String:
                    ret.AddRange(Encoding.UTF8.GetBytes(v.strval));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    class DumpBlobFunction : FunctionStatement
    {
        /* Dump the hash table defined with a DefBlob function */

        public DumpBlobFunction()
        {
            name = "dumpblob";
            args = new List<FunctionArg>()
            {
                new FunctionArg { argtype = Expression.EvalResult.ResultType.String },
                new FunctionArg { argtype = Expression.EvalResult.ResultType.String }
            };
        }

        public override Expression.EvalResult Run(MakeState s, List<Expression.EvalResult> passed_args)
        {
            var blob_name = passed_args[0].strval;
            var hc_name = passed_args[1].strval;

            var hc = DefineBlobFunction.tables[blob_name];

            // Decide on a sensible value for nbuckets - sqroot(n)
            //  seems appropriate
            var nbuckets = (int)Math.Sqrt(hc.keys.Count);

            /* We need a map between key indices and data blob offsets
                - this is stored in idx_map.

               To find entries, we perform hc % nbuckets to get a
                bucket number.  Then index buckets[bucket_no] to get
                index of first item.  If it is not what we want, go to
                next item chain[cur_index] and so on.
            
               To create the hash table, therefore, we iterate through
                each key.  First, store its index to the idx_map.  Next,
                calculate bucket_no.  chain[cur_idx] is set to whatever
                is currently in buckets[bucket_no], and buckets[bucket_no]
                is updated to the current index.  This means the last item
                added will actually be the first out, and the first item
                will have its chain[] value set to -1 (the initial value
                of buckets[])
            */

            int[] buckets = new int[nbuckets];
            for (int i = 0; i < nbuckets; i++)
                buckets[i] = -1;
            int[] chain = new int[hc.keys.Count];
            int[] idx_map = new int[hc.keys.Count];

            for (int i = 0; i < hc.keys.Count; i++)
            {
                var hte = hc.keys[i];

                idx_map[i] = hte.idx;

                var bucket_no = hte.hc % (uint)nbuckets;

                var cur_bucket = buckets[bucket_no];
                chain[i] = cur_bucket;
                buckets[bucket_no] = i;
            }

            /* Now dump the hash table:
                var hc_name = new HashTable {
                    nbucket = nbuckets,
                    nchain = hc.keys.Count,
                    bucket = new byte[] {
                        bucket_dump
                    },
                    chain = new byte[] {
                        chain_dump
                    },
                    idx_map = new byte[] {
                        idx_map_dump
                    },
                    data = new byte[] {
                        data_dump
                    }
                };
            */

            Program.sw.Write("\t\t\tvar " + hc_name + " = new HashTable {\n");
            Program.sw.Write("\t\t\t\tnbucket = " + nbuckets.ToString() + ",\n");
            Program.sw.Write("\t\t\t\tnchain = " + hc.keys.Count.ToString() + ",\n");
            Program.sw.Write("\t\t\t\tbucket = new int[] {\n");
            DumpArray<int>(buckets);
            Program.sw.Write("\t\t\t\t},\n");
            Program.sw.Write("\t\t\t\tchain = new int[] {\n");
            DumpArray<int>(chain);
            Program.sw.Write("\t\t\t\t},\n");
            Program.sw.Write("\t\t\t\tidx_map = new int[] {\n");
            DumpArray<int>(idx_map);
            Program.sw.Write("\t\t\t\t},\n");
            Program.sw.Write("\t\t\t\tdata = new byte[] {\n");
            DumpArray<byte>(hc.data, "\t\t\t\t\t", 16);
            Program.sw.Write("\t\t\t\t},\n");
            Program.sw.Write("\t\t\t};\n");
            Program.sw.Write("\n");

            return new Expression.EvalResult();
        }

        void DumpArray<T>(IList<T> arr)
        {
            DumpArray(arr, "\t\t\t\t\t", 8);
        }

        void DumpArray<T>(IList<T> arr, string line_prefix, int per_line)
        {
            int cur_line = 0;

            for(int i = 0; i < arr.Count; i++)
            {
                var b = arr[i];

                if (cur_line == 0)
                    Program.sw.Write(line_prefix);

                Program.sw.Write(b.ToString());
                Program.sw.Write(", ");

                cur_line++;
                if(cur_line == per_line)
                {
                    Program.sw.Write("\n");
                    cur_line = 0;
                }
            }

            if (cur_line != 0)
                Program.sw.Write("\n");
        }
    }

    class PrintFunction : FunctionStatement
    {
        public PrintFunction()
        {
            name = "print";
        }

        public override Expression.EvalResult Run(MakeState s, List<Expression.EvalResult> args)
        {
            Print(args[0], true);

            return new Expression.EvalResult();
        }

        void Print(Expression.EvalResult e, bool toplevel)
        {
            switch (e.Type)
            {
                case Expression.EvalResult.ResultType.Int:
                    Program.sw.Write(e.intval);
                    break;
                case Expression.EvalResult.ResultType.String:
                    if (!toplevel)
                        Program.sw.Write("\"");
                    Program.sw.Write(e.strval);
                    if (!toplevel)
                        Program.sw.Write("\"");
                    break;
                case Expression.EvalResult.ResultType.Array:
                    Program.sw.Write("[ ");
                    for (int i = 0; i < e.arrval.Count; i++)
                    {
                        if (i != 0)
                            Program.sw.Write(", ");
                        Print(e.arrval[i], false);
                    }
                    Program.sw.Write(" ]");
                    break;
                case Expression.EvalResult.ResultType.Object:
                    Program.sw.Write("[ ");
                    int j = 0;
                    foreach (KeyValuePair<string, Expression.EvalResult> kvp in e.objval)
                    {
                        if (j != 0)
                            Program.sw.Write(", ");
                        Program.sw.Write(kvp.Key);
                        Program.sw.Write(": ");
                        Print(kvp.Value, false);
                        j++;
                    }
                    Program.sw.Write(" ]");
                    break;
            }
        }
    }

    partial class Parser
    {
        internal Parser(Scanner s) : base(s) { }

        internal void AddDefine(string t, string val)
        {
            throw new NotImplementedException();
        }

        internal void AddDefine(string t, int val)
        {
            throw new NotImplementedException();
        }

        internal int ResolveAsInt(string t)
        {
            throw new NotImplementedException();
        }
    }

    partial class Scanner
    {
        string filename;
        internal Scanner(Stream file, string fname) : this(file) { filename = fname; }

        public override void yyerror(string format, params object[] args)
        {
            throw new ParseException(String.Format(format, args) + " at line " + yyline + ", col " + yycol + " in " + filename, yyline, yycol);
        }

        internal int sline { get { return yyline; } }
        internal int scol { get { return yycol; } }
    }

    public class ParseException : Exception
    {
        int l, c;
        public ParseException(string msg, int line, int col) : base(msg) { l = line; c = col; }
    }
}
