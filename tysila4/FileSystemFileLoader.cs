/* Copyright (C) 2012-2016 by John Cronin
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
using libtysila5;
using System.IO;

namespace tysila4
{
    public class FileSystemFileLoader : FileLoader
    {
        public override FileLoader.FileLoadResults LoadFile(string filename)
        {
            string ass_name = null;
            string full_ass_name = null;

            List<string> fnames = new List<string>();
            // Try and find the requested assembly
            foreach (string sd in Program.search_dirs)
            {
                string bname = sd;
                if ((bname != "") && !bname.EndsWith(Program.DirectoryDelimiter))
                    bname += Program.DirectoryDelimiter;
                fnames.Add(bname + filename + ".dll");
                fnames.Add(bname + filename + ".exe");
                fnames.Add(bname + filename);
            }

            string modname = "";

            foreach (string trial in fnames)
            {
                try
                {
                    FileInfo fi = new FileInfo(trial);
                    if (fi.Exists)
                    {
                        ass_name = trial;
                        full_ass_name = fi.FullName;
                        modname = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
                    }
                }
                catch (Exception) { }
                if (ass_name != null)
                    break;
            }

            if (ass_name == null)
                return null;

            FileStream s = new FileStream(ass_name, FileMode.Open, FileAccess.Read);

            return new FileLoadResults { FullFilename = full_ass_name, ModuleName = modname, Stream = s };
        }
    }
}
