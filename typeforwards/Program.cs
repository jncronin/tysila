/* Copyright (C) 2017-2018 by John Cronin
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace typeforwards
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 4)
            {
                disp_usage();
                return;
            }
            libtysila5.libtysila.AssemblyLoader al = new libtysila5.libtysila.AssemblyLoader(
                new tysila4.FileSystemFileLoader());
            tysila4.Program.search_dirs.Add(args[0]);
            tysila4.Program.search_dirs.Add(new System.IO.FileInfo(args[2]).DirectoryName);

            /* Load up all types in corefx files */
            Dictionary<string, string> map = new Dictionary<string, string>();
            var indir = new System.IO.DirectoryInfo(args[0]);
            var files = indir.GetFiles(args[1]);
            foreach (var fi in files)
            {
                var m = al.GetAssembly(fi.FullName);

                for (int i = 1; i <= m.table_rows[metadata.MetadataStream.tid_TypeDef]; i++)
                {
                    var flags = m.GetIntEntry(metadata.MetadataStream.tid_TypeDef, i, 0);
                    metadata.TypeSpec ts = new metadata.TypeSpec { m = m, tdrow = i };

                    if((flags & 0x7) == 0x1 || (flags & 0x7) == 0x2)
                    {
                        // public
                        var nspace = ts.Namespace;
                        var name = ts.Name;
                        var fullname = nspace + "." + name;
                        map[fullname] = fi.Name;
                    }
                }
            }

            /* Generate a mapping from type name/namespaces to the files they are contained in */
            var infiledi = al.LoadAssembly(args[2]);
            var infile = new metadata.PEFile().Parse(infiledi, al, false);
            var o = new System.IO.StreamWriter(args[3]);
            for (int i = 1; i <= infile.table_rows[metadata.MetadataStream.tid_TypeDef]; i++)
            {
                var flags = infile.GetIntEntry(metadata.MetadataStream.tid_TypeDef, i, 0);
                metadata.TypeSpec ts = new metadata.TypeSpec { m = infile, tdrow = i };

                if ((flags & 0x7) == 0x1 || (flags & 0x7) == 0x2)
                {
                    // public
                    var nspace = ts.Namespace;
                    var name = ts.Name;
                    var fullname = nspace + "." + name;

                    if(map.ContainsKey(fullname))
                    {
                        o.WriteLine(infile.AssemblyName + "!" + fullname + "=" + map[fullname]);
                    }
                }
            }
            for(int i = 1; i <= infile.table_rows[metadata.MetadataStream.tid_ExportedType]; i++)
            {
                var nspace = infile.GetStringEntry(metadata.MetadataStream.tid_ExportedType, i, 3);
                var name = infile.GetStringEntry(metadata.MetadataStream.tid_ExportedType, i, 2);
                var fullname = nspace + "." + name;

                if(map.ContainsKey(fullname))
                {
                    o.WriteLine(infile.AssemblyName + "!" + fullname + "=" + map[fullname]);
                }
            }
            o.Close();
        }

        private static void disp_usage()
        {
            Console.WriteLine("Usage: typeforwards <input_dir> <pattern> <reference_assembly> <outfile>");
        }
    }
}
