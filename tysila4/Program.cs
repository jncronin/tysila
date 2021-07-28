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
using binary_library;
using binary_library.elf;
using XGetoptCS;

namespace tysila4
{
    public class Program
    {
        /* Boiler plate */
        const string year = "2009 - 2018";
        const string authors = "John Cronin <jncronin@tysos.org>";
        const string website = "http://www.tysos.org";
        const string nl = "\n";
        public static string bplate = "tysila " + libtysila5.libtysila.VersionString + " (" + website + ")" + nl +
            "Copyright (C) " + year + " " + authors + nl +
            "This is free software.  Please see the source for copying conditions.  There is no warranty, " +
            "not even for merchantability or fitness for a particular purpose";

        static string comment = nl + "tysila" + nl + "ver: " + libtysila5.libtysila.VersionString + nl;

        public static List<string> search_dirs = new List<string> {
            "",
            ".",
            Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Program)).Location),
            DirectoryDelimiter
        };
        static List<string> new_search_dirs = new List<string>();

        internal static libtysila5.target.Target t;

        static bool func_sects = false;
        static bool data_sects = false;
        static bool class_sects = false;

        static bool do_dwarf = true;

        public static string DirectoryDelimiter
        {
            get
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                    return "/";
                else
                    return "\\";
            }
        }

        static void Main(string[] args)
        {
            var argc = args.Length;
            char c;
            var go = new XGetoptCS.XGetopt();
            var arg_str = "t:L:f:e:o:d:qDC:H:m:i";
            string target = "x86";
            string debug_file = null;
            string output_file = null;
            string epoint = null;
            string cfile = null;
            string hfile = null;
            bool quiet = false;
            bool require_metadata_version_match = true;
            bool interactive = false;
            string act_epoint = null;
            Dictionary<string, object> opts = new Dictionary<string, object>();

            while((c = go.Getopt(argc, args, arg_str)) != '\0')
            {
                switch(c)
                {
                    case 't':
                        target = go.Optarg;
                        break;
                    case 'L':
                        new_search_dirs.Add(go.Optarg);
                        break;
                    case 'd':
                        debug_file = go.Optarg;
                        break;
                    case 'o':
                        output_file = go.Optarg;
                        break;
                    case 'e':
                        epoint = go.Optarg;
                        break;
                    case 'q':
                        quiet = true;
                        break;
                    case 'D':
                        require_metadata_version_match = false;
                        break;
                    case 'C':
                        cfile = go.Optarg;
                        break;
                    case 'H':
                        hfile = go.Optarg;
                        break;
                    case 'f':
                        parse_f_option(go);
                        break;
                    case 'i':
                        interactive = true;
                        break;
                    case 'm':
                        {
                            var opt = go.Optarg;
                            object optval = true;
                            if(opt.Contains("="))
                            {
                                var optvals = opt.Substring(opt.IndexOf("=") + 1);
                                opt = opt.Substring(0, opt.IndexOf("="));

                                int intval;
                                if (optvals.ToLower() == "false" || optvals.ToLower() == "off" || optvals.ToLower() == "no")
                                    optval = false;
                                else if (optvals.ToLower() == "true" || optvals.ToLower() == "on" || optvals.ToLower() == "yes")
                                    optval = true;
                                else if (int.TryParse(optvals, out intval))
                                    optval = intval;
                                else
                                    optval = optvals;
                            }
                            else if(opt.StartsWith("no-"))
                            {
                                opt = opt.Substring(3);
                                optval = false;
                            }
                            opts[opt] = optval;
                        }
                        break;
                }
            }

            var fname = go.Optarg;

            if(fname == String.Empty)
            {
                Console.WriteLine("No input file specified");
                return;
            }

            // Insert library directories specified on the command line before the defaults
            search_dirs.InsertRange(0, new_search_dirs);

            if(cfile != null && hfile == null)
            {
                Console.WriteLine("-H must be used if -C is used");
                return;
            }

            libtysila5.libtysila.AssemblyLoader al = new libtysila5.libtysila.AssemblyLoader(
                new FileSystemFileLoader());

            /* Load up type forwarders */
            foreach(var libdir in search_dirs)
            {
                try
                {
                    var di = new DirectoryInfo(libdir);
                    if (di.Exists)
                    {
                        foreach (var tfw_file in di.GetFiles("*.tfw"))
                        {
                            var tfr = new StreamReader(tfw_file.OpenRead());
                            while (!tfr.EndOfStream)
                            {
                                var tfr_line = tfr.ReadLine();
                                var tfr_lsplit = tfr_line.Split('=');
                                al.TypeForwarders[tfr_lsplit[0]] = tfr_lsplit[1];
                            }
                            tfr.Close();
                        }
                    }
                }
                catch (Exception) { }
            }

            //search_dirs.Add(@"..\mono\corlib");
            al.RequireVersionMatch = require_metadata_version_match;

            // add containing directory of input to search dirs
            var ifi = new FileInfo(fname);
            search_dirs.Add(ifi.DirectoryName);

            var m = al.GetAssembly(fname);
            if(m == null)
            {
                Console.WriteLine("Input file " + fname + " not found");
                throw new Exception(fname + " not found");
            }

            t = libtysila5.target.Target.targets[target];
            // try and set target options
            foreach(var kvp in opts)
            {
                if(!t.Options.TrySet(kvp.Key, kvp.Value))
                {
                    Console.WriteLine("Unable to set target option " + kvp.Key + " to " + kvp.Value.ToString());
                    return;
                }
            }

            if (interactive)
            {
                if (new Interactive(m, t).DoInteractive() == false)
                    return;
            }

            libtysila5.dwarf.DwarfCU dwarf = null;
            if(do_dwarf)
            {
                dwarf = new libtysila5.dwarf.DwarfCU();
                dwarf.m = m;
                dwarf.t = t;
            }

            if (output_file != null)
            {
                var bf = new binary_library.elf.ElfFile(binary_library.Bitness.Bits32);
                t.bf = bf;
                bf.Init();
                bf.Architecture = target;
                var st = new libtysila5.StringTable(
                    m.GetStringEntry(metadata.MetadataStream.tid_Module,
                    1, 1), al, t);
                t.st = st;
                t.r = new libtysila5.CachingRequestor(m);
                t.InitIntcalls();

                /* for now, just assemble all public and protected
                non-generic methods in public types, plus the
                entry point */
                StringBuilder debug = new StringBuilder();
                for (int i = 1; i <= m.table_rows[metadata.MetadataStream.tid_MethodDef]; i++)
                {
                    metadata.MethodSpec ms = new metadata.MethodSpec
                    {
                        m = m,
                        mdrow = i,
                        msig = 0
                    };

                    ms.type = new metadata.TypeSpec
                    {
                        m = m,
                        tdrow = m.methoddef_owners[ms.mdrow]
                    };

                    var mflags = m.GetIntEntry(metadata.MetadataStream.tid_MethodDef,
                        i, 2);
                    var tflags = m.GetIntEntry(metadata.MetadataStream.tid_TypeDef,
                        ms.type.tdrow, 0);

                    mflags &= 0x7;
                    tflags &= 0x7;

                    ms.msig = (int)m.GetIntEntry(metadata.MetadataStream.tid_MethodDef,
                        i, 4);

                    /* See if this is the entry point */
                    int tid, row;
                    m.InterpretToken(m.entry_point_token, out tid, out row);
                    if (tid == metadata.MetadataStream.tid_MethodDef)
                    {
                        if (row == i)
                        {
                            if (epoint != null)
                                ms.aliases = new List<string> { epoint };

                            mflags = 6;
                            tflags = 1;

                            ms.AlwaysCompile = true;
                            act_epoint = ms.MangleMethod();
                        }
                    }

                    /* See if we have an always compile attribute */
                    if (ms.HasCustomAttribute("_ZN14libsupcs#2Edll8libsupcs22AlwaysCompileAttribute_7#2Ector_Rv_P1u1t") ||
                        ms.type.HasCustomAttribute("_ZN14libsupcs#2Edll8libsupcs22AlwaysCompileAttribute_7#2Ector_Rv_P1u1t"))
                    {
                        mflags = 6;
                        tflags = 1;

                        ms.AlwaysCompile = true;
                    }

                    if (ms.type.IsGenericTemplate == false &&
                        ms.IsGenericTemplate == false &&
                        (mflags == 0x4 || mflags == 0x5 || mflags == 0x6) &&
                        tflags != 0)
                    {
                        t.r.MethodRequestor.Request(ms);
                    }
                }

                /* Also assemble all public non-generic type infos */
                for (int i = 1; i <= m.table_rows[metadata.MetadataStream.tid_TypeDef]; i++)
                {
                    var flags = (int)m.GetIntEntry(metadata.MetadataStream.tid_TypeDef,
                        i, 0);
                    if (((flags & 0x7) != 0x1) &&
                        ((flags & 0x7) != 0x2))
                        continue;
                    var ts = new metadata.TypeSpec { m = m, tdrow = i };
                    if (ts.IsGeneric)
                        continue;
                    t.r.StaticFieldRequestor.Request(ts);
                    t.r.VTableRequestor.Request(ts.Box);
                }

                /* If corlib, add in the default equality comparers so we don't have to jit these
                 * commonly used classes */
                if(m.is_corlib)
                {
                    t.r.MethodRequestor.Request(m.GetMethodSpec(m.GetTypeSpec("System.Collections.Generic", "GenericEqualityComparer`1", new metadata.TypeSpec[] { m.SystemByte }), ".ctor"));
                    t.r.MethodRequestor.Request(m.GetMethodSpec(m.GetTypeSpec("System.Collections.Generic", "GenericEqualityComparer`1", new metadata.TypeSpec[] { m.SystemInt8 }), ".ctor"));
                    t.r.MethodRequestor.Request(m.GetMethodSpec(m.GetTypeSpec("System.Collections.Generic", "GenericEqualityComparer`1", new metadata.TypeSpec[] { m.SystemInt16 }), ".ctor"));
                    t.r.MethodRequestor.Request(m.GetMethodSpec(m.GetTypeSpec("System.Collections.Generic", "GenericEqualityComparer`1", new metadata.TypeSpec[] { m.SystemUInt16 }), ".ctor"));
                    t.r.MethodRequestor.Request(m.GetMethodSpec(m.GetTypeSpec("System.Collections.Generic", "GenericEqualityComparer`1", new metadata.TypeSpec[] { m.SystemInt32 }), ".ctor"));
                    t.r.MethodRequestor.Request(m.GetMethodSpec(m.GetTypeSpec("System.Collections.Generic", "GenericEqualityComparer`1", new metadata.TypeSpec[] { m.SystemUInt32 }), ".ctor"));
                    t.r.MethodRequestor.Request(m.GetMethodSpec(m.GetTypeSpec("System.Collections.Generic", "GenericEqualityComparer`1", new metadata.TypeSpec[] { m.SystemInt64 }), ".ctor"));
                    t.r.MethodRequestor.Request(m.GetMethodSpec(m.GetTypeSpec("System.Collections.Generic", "GenericEqualityComparer`1", new metadata.TypeSpec[] { m.SystemUInt64 }), ".ctor"));
                    t.r.MethodRequestor.Request(m.GetMethodSpec(m.GetTypeSpec("System.Collections.Generic", "GenericEqualityComparer`1", new metadata.TypeSpec[] { m.SystemString }), ".ctor"));
                    t.r.MethodRequestor.Request(m.GetMethodSpec(m.GetTypeSpec("System.Collections.Generic", "GenericEqualityComparer`1", new metadata.TypeSpec[] { m.SystemObject }), ".ctor"));

                    t.r.VTableRequestor.Request(m.GetTypeSpec("System.Collections.Generic", "GenericEqualityComparer`1", new metadata.TypeSpec[] { m.SystemByte }));
                    t.r.VTableRequestor.Request(m.GetTypeSpec("System.Collections.Generic", "GenericEqualityComparer`1", new metadata.TypeSpec[] { m.SystemInt8 }));
                    t.r.VTableRequestor.Request(m.GetTypeSpec("System.Collections.Generic", "GenericEqualityComparer`1", new metadata.TypeSpec[] { m.SystemInt16 }));
                    t.r.VTableRequestor.Request(m.GetTypeSpec("System.Collections.Generic", "GenericEqualityComparer`1", new metadata.TypeSpec[] { m.SystemUInt16 }));
                    t.r.VTableRequestor.Request(m.GetTypeSpec("System.Collections.Generic", "GenericEqualityComparer`1", new metadata.TypeSpec[] { m.SystemInt32 }));
                    t.r.VTableRequestor.Request(m.GetTypeSpec("System.Collections.Generic", "GenericEqualityComparer`1", new metadata.TypeSpec[] { m.SystemUInt32 }));
                    t.r.VTableRequestor.Request(m.GetTypeSpec("System.Collections.Generic", "GenericEqualityComparer`1", new metadata.TypeSpec[] { m.SystemInt64 }));
                    t.r.VTableRequestor.Request(m.GetTypeSpec("System.Collections.Generic", "GenericEqualityComparer`1", new metadata.TypeSpec[] { m.SystemUInt64 }));
                    t.r.VTableRequestor.Request(m.GetTypeSpec("System.Collections.Generic", "GenericEqualityComparer`1", new metadata.TypeSpec[] { m.SystemString }));
                    t.r.VTableRequestor.Request(m.GetTypeSpec("System.Collections.Generic", "GenericEqualityComparer`1", new metadata.TypeSpec[] { m.SystemObject }));
                }

                /* Generate a thread-local data section.  We may not use it. */
                var tlsos = bf.CreateContentsSection();
                tlsos.Name = ".tdata";
                tlsos.IsAlloc = true;
                tlsos.IsExecutable = false;
                tlsos.IsWriteable = true;
                tlsos.IsThreadLocal = true;

                while (!t.r.Empty)
                {
                    if (!t.r.MethodRequestor.Empty)
                    {
                        var ms = t.r.MethodRequestor.GetNext();

                        ISection tsect = null;
                        ISection datasect = null;
                        if (func_sects && !ms.ms.AlwaysCompile)
                        {
                            tsect = get_decorated_section(bf, bf.GetTextSection(), "." + ms.ms.MangleMethod());
                            datasect = get_decorated_section(bf, bf.GetDataSection(), "." + ms.ms.MangleMethod() + "_SignatureTable");
                        }
                        else if(class_sects && !ms.ms.AlwaysCompile)
                        {
                            tsect = get_decorated_section(bf, bf.GetTextSection(), "." + ms.ms.type.MangleType());
                            datasect = get_decorated_section(bf, bf.GetDataSection(), "." + ms.ms.type.MangleType() + "_SignatureTable");
                        }

                        libtysila5.libtysila.AssembleMethod(ms.ms,
                            bf, t, debug, m, ms.c, tsect, datasect, dwarf);
                        if (!quiet)
                            Console.WriteLine(ms.ms.m.MangleMethod(ms.ms));
                    }
                    else if (!t.r.StaticFieldRequestor.Empty)
                    {
                        var sf = t.r.StaticFieldRequestor.GetNext();

                        ISection tsect = null;
                        if (data_sects && !sf.AlwaysCompile)
                            tsect = get_decorated_section(bf, bf.GetDataSection(), "." + sf.MangleType() + "S");
                        else if (class_sects && !sf.AlwaysCompile)
                        {
                            tsect = get_decorated_section(bf, bf.GetDataSection(), "." + sf.MangleType() + "S");
                        }

                        libtysila5.layout.Layout.OutputStaticFields(sf,
                            t, bf, m, tsect, tlsos);
                        if (!quiet)
                            Console.WriteLine(sf.MangleType() + "S");
                    }
                    else if (!t.r.EHRequestor.Empty)
                    {
                        var eh = t.r.EHRequestor.GetNext();

                        ISection tsect = null;
                        if (func_sects && !eh.ms.AlwaysCompile)
                            tsect = get_decorated_section(bf, bf.GetRDataSection(), "." + eh.ms.MangleMethod() + "EH");
                        else if (class_sects && !eh.ms.AlwaysCompile)
                        {
                            tsect = get_decorated_section(bf, bf.GetRDataSection(), "." + eh.ms.type.MangleType() + "EH");
                        }

                        libtysila5.layout.Layout.OutputEHdr(eh,
                            t, bf, m, tsect);
                        if (!quiet)
                            Console.WriteLine(eh.ms.MangleMethod() + "EH");
                    }
                    else if (!t.r.VTableRequestor.Empty)
                    {
                        var vt = t.r.VTableRequestor.GetNext();

                        ISection tsect = null;
                        ISection data_sect = null;
                        if (data_sects && !vt.AlwaysCompile)
                        {
                            tsect = get_decorated_section(bf, bf.GetRDataSection(), "." + vt.MangleType());
                            data_sect = get_decorated_section(bf, bf.GetDataSection(), "." + vt.MangleType() + "_SignatureTable");
                        }
                        else if (class_sects && !vt.AlwaysCompile)
                        {
                            tsect = get_decorated_section(bf, bf.GetTextSection(), "." + vt.MangleType());
                            data_sect = get_decorated_section(bf, bf.GetDataSection(), "." + vt.MangleType() + "_SignatureTable");
                        }

                        libtysila5.layout.Layout.OutputVTable(vt,
                            t, bf, m, tsect, data_sect);
                        if (!quiet)
                            Console.WriteLine(vt.MangleType());
                    }
                    else if(!t.r.DelegateRequestor.Empty)
                    {
                        var d = t.r.DelegateRequestor.GetNext();
                        libtysila5.ir.ConvertToIR.CreateDelegate(d, t);
                        if (!quiet)
                            Console.WriteLine(d.MangleType() + "D");
                    }
                    else if(!t.r.BoxedMethodRequestor.Empty)
                    {
                        var bm = t.r.BoxedMethodRequestor.GetNext();

                        ISection tsect = null;
                        if (func_sects && !bm.ms.AlwaysCompile)
                            tsect = get_decorated_section(bf, bf.GetTextSection(), "." + bm.ms.MangleMethod());
                        else if (class_sects && !bm.ms.AlwaysCompile)
                        {
                            tsect = get_decorated_section(bf, bf.GetTextSection(), "." + bm.ms.type.MangleType());
                        }

                        libtysila5.libtysila.AssembleBoxedMethod(bm.ms,
                            bf, t, debug, tsect);
                        if (!quiet)
                            Console.WriteLine(bm.ms.MangleMethod());
                    }
                }

                if (debug_file != null)
                {
                    string d = debug.ToString();

                    StreamWriter sw = new StreamWriter(debug_file);
                    sw.Write(d);
                    sw.Close();
                }

                if (tlsos.Length > 0)
                    bf.AddSection(tlsos);

                /* String table */
                st.WriteToOutput(bf, m, t);

                /* Include original metadata */
                var rdata = bf.GetRDataSection();
                rdata.Align(t.GetPointerSize());
                var mdsym = bf.CreateSymbol();
                mdsym.Name = m.AssemblyName;
                mdsym.ObjectType = binary_library.SymbolObjectType.Object;
                mdsym.Offset = (ulong)rdata.Data.Count;
                mdsym.Type = binary_library.SymbolType.Global;
                var len = m.file.GetLength();
                mdsym.Size = len;
                rdata.AddSymbol(mdsym);

                for (int i = 0; i < len; i++)
                    rdata.Data.Add(m.file.ReadByte(i));

                var mdsymend = bf.CreateSymbol();
                mdsymend.Name = m.AssemblyName + "_end";
                mdsymend.ObjectType = binary_library.SymbolObjectType.Object;
                mdsymend.Offset = (ulong)rdata.Data.Count;
                mdsymend.Type = binary_library.SymbolType.Global;
                mdsymend.Size = 0;
                rdata.AddSymbol(mdsymend);

                /* Add resource symbol if present */
                if(m.GetPEFile().ResourcesSize != 0)
                {
                    var rsym = bf.CreateSymbol();
                    rsym.Name = m.AssemblyName + "_resources";
                    rsym.ObjectType = SymbolObjectType.Object;
                    rsym.Offset = (ulong)m.GetPEFile().ResourcesOffset + mdsym.Offset;
                    rsym.Type = SymbolType.Global;
                    rsym.Size = m.GetPEFile().ResourcesSize;
                    rdata.AddSymbol(rsym);
                }

                /* Add comment */
                var csect = bf.CreateContentsSection();
                csect.IsAlloc = false;
                csect.IsExecutable = false;
                csect.IsWriteable = false;
                csect.Name = ".comment";
                var cbytes = Encoding.ASCII.GetBytes(comment);
                foreach (var cbyte in cbytes)
                    csect.Data.Add(cbyte);
                csect.Data.Add(0);
                if(act_epoint != null)
                {
                    var epbytes = Encoding.ASCII.GetBytes("entry: " +
                        act_epoint);
                    foreach (var epbyte in epbytes)
                        csect.Data.Add(epbyte);
                    csect.Data.Add(0);
                }
                bf.AddSection(csect);

                /* Add debugger sections */
                if(dwarf != null)
                {
                    var dwarf_sects = new libtysila5.dwarf.DwarfSections(bf);
                    dwarf.WriteToOutput(dwarf_sects);
                }


                /* Write output file */
                bf.Filename = output_file;
                bf.Write();
            }

            if(hfile != null)
            {
                COutput.WriteHeader(m, t, hfile, cfile);
            }
        }

        private static ISection get_decorated_section(ElfFile bf, ISection section, string add_to_name)
        {
            var decorated_name = section.Name + add_to_name;
            var sect = bf.FindSection(decorated_name);
            if(sect == null)
            {
                sect = bf.CopySectionType(section, decorated_name);
            }
            return sect;
        }

        private static void parse_f_option(XGetopt go)
        {
            if (go.Optarg == "function-sections")
                func_sects = true;
            else if (go.Optarg == "data-sections")
                data_sects = true;
            else if (go.Optarg == "class-sections")
                class_sects = true;
            else
                throw new NotImplementedException();
        }
    }
}
