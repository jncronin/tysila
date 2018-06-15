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
using System.Collections.Generic;
using System.Text;
using metadata;

namespace libtysila5.layout
{
    public partial class Layout
    {
        public enum StringField
        {
            Length,
            Start_Char,
        }

        static MetadataStream corlib = null;
        static int length = 0;
        static int sc = 0;
        
        public static int GetStringFieldOffset(StringField sf, Code c)
        {
            if (corlib == null)
                LayoutString(c);

            switch(sf)
            {
                case StringField.Length:
                    return length;
                case StringField.Start_Char:
                    return sc;
                default:
                    throw new NotSupportedException();
            }
        }

        private static void LayoutString(Code c)
        {
            corlib = c.ms.m.al.GetAssembly("mscorlib");

            var ts = corlib.GetSimpleTypeSpec(0x0e);

            length = GetFieldOffset(ts, "length", c.t, out var is_tls);
            sc = GetFieldOffset(ts, "start_char", c.t, out is_tls);
        }
    }
}
