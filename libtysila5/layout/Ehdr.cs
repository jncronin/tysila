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
using System.Text;
using metadata;
using libtysila5.target;

/* Exception headers are pushed by entry to protected blocks
 * They are composed of:
 *  intptr EType
 *  intptr Handler (native code offset)
 *  intptr Catch Object (if applicable)
 */

namespace libtysila5.layout
{
    public partial class Layout
    {
        public static int GetEhdrSize(Target t)
        {
            return 3 * t.GetPointerSize();
        }

        public static void OutputEHdr(MethodSpecWithEhdr ms,
            Target t, binary_library.IBinaryFile of,
            MetadataStream base_m = null,
            binary_library.ISection os = null)
        {
            // Don't compile if not for this architecture
            if (!t.IsMethodValid(ms.ms))
                return;

            if(os == null)
                os = of.GetRDataSection();
            os.Align(t.GetPointerSize());
            var d = os.Data;

            /* Symbol */
            var sym = of.CreateSymbol();
            sym.Name = ms.ms.MangleMethod() + "EH";
            sym.ObjectType = binary_library.SymbolObjectType.Object;
            sym.Offset = (ulong)d.Count;
            sym.Type = binary_library.SymbolType.Global;
            os.AddSymbol(sym);

            if (base_m != null && ms.ms.m != base_m)
                sym.Type = binary_library.SymbolType.Weak;

            foreach(var ehdr in ms.c.ehdrs)
            {
                var v = t.IntPtrArray(BitConverter.GetBytes((int)ehdr.EType));
                foreach (var b in v)
                    d.Add(b);

                /* Handler */
                var hand_sym = of.CreateSymbol();
                hand_sym.Name = ms.ms.MangleMethod() + "EH" + ehdr.EhdrIdx.ToString();

                var hand_reloc = of.CreateRelocation();
                hand_reloc.Addend = 0;
                hand_reloc.DefinedIn = os;
                hand_reloc.Offset = (ulong)d.Count;
                hand_reloc.References = hand_sym;
                hand_reloc.Type = t.GetDataToCodeReloc();
                of.AddRelocation(hand_reloc);

                for (int i = 0; i < t.GetPointerSize(); i++)
                    d.Add(0);

                /* Catch object */
                if (ehdr.ClassToken != null)
                {
                    var catch_sym = of.CreateSymbol();
                    catch_sym.Name = ehdr.ClassToken.MangleType();

                    var catch_reloc = of.CreateRelocation();
                    catch_reloc.Addend = 0;
                    catch_reloc.DefinedIn = os;
                    catch_reloc.Offset = (ulong)d.Count;
                    catch_reloc.References = catch_sym;
                    catch_reloc.Type = t.GetDataToDataReloc();
                    of.AddRelocation(catch_reloc);

                    t.r.VTableRequestor.Request(ehdr.ClassToken);
                }
                else if(ehdr.EType == ExceptionHeader.ExceptionHeaderType.Filter)
                {
                    var filt_sym = of.CreateSymbol();
                    filt_sym.Name = ms.ms.MangleMethod() + "EHF" + ehdr.EhdrIdx.ToString();

                    var filt_reloc = of.CreateRelocation();
                    filt_reloc.Addend = 0;
                    filt_reloc.DefinedIn = os;
                    filt_reloc.Offset = (ulong)d.Count;
                    filt_reloc.References = filt_sym;
                    filt_reloc.Type = t.GetDataToCodeReloc();
                    of.AddRelocation(filt_reloc);
                }
                for (int i = 0; i < t.GetPointerSize(); i++)
                    d.Add(0);
            }

            sym.Size = (long)((ulong)d.Count - sym.Offset);
        }

        public class MethodSpecWithEhdr : Spec, IEquatable<MethodSpecWithEhdr>
        {
            public MethodSpec ms;
            public Code c;

            public override MetadataStream Metadata
            {
                get
                {
                    return ms.Metadata;
                }
            }

            public override bool IsInstantiatedGenericMethod
            {
                get
                {
                    return ms.IsInstantiatedGenericMethod;
                }
            }

            public override bool IsInstantiatedGenericType
            {
                get
                {
                    return ms.IsInstantiatedGenericType;
                }
            }

            public override bool IsArray
            {
                get
                {
                    return ms.IsArray;
                }
            }

            public override IEnumerable<int> CustomAttributes(string ctor = null)
            {
                return ms.CustomAttributes(ctor);
            }

            public bool Equals(MethodSpecWithEhdr other)
            {
                if (other == null)
                    return false;
                return ms.Equals(other.ms);
            }

            public override int GetHashCode()
            {
                return ms.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as MethodSpecWithEhdr);
            }

            public static implicit operator MethodSpecWithEhdr(Code c)
            {
                return new MethodSpecWithEhdr
                {
                    c = c,
                    ms = c.ms
                };
            }

            public static implicit operator MethodSpecWithEhdr(MethodSpec ms)
            {
                return new MethodSpecWithEhdr { ms = ms, c = null };
            }

            public override string Name
            {
                get
                {
                    return ms.Name;
                }
            }
        }
    }
}
