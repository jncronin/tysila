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

using metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace libtysila5
{
    public partial class libtysila
    {
        public static int MajorVersion { get { return 0; } }
        public static int MinorVersion { get { return 5; } }
        public static int BuildVersion { get { return 0; } }
        public static string VersionString
        {
            get
            {
                return MajorVersion.ToString() + "." +
                    MinorVersion.ToString() + "." +
                    BuildVersion.ToString();
            }
        }

        public class AssemblyLoader : metadata.AssemblyLoader
        {
            FileLoader f;

            public AssemblyLoader(FileLoader fl)
            {
                f = fl;
            }

            public override DataInterface LoadAssembly(string name)
            {
                var flr = f.LoadFile(name);
                if (flr == null)
                    return null;

                // Load to an array interface (access is quicker than
                //  constantly searching the stream)
                var s = flr.Stream;
                s.Seek(0, SeekOrigin.Begin);
                var l = s.Length;

                var arr = new byte[l];
                s.Read(arr, 0, (int)l);

                var ret = new ArrayInterface(arr);
                ret.Name = flr.FullFilename;

                return ret;
            }
        }
    }
}
