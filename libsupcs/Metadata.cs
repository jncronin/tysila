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

// Interface with the metadata module to interpret metadata embedded in modules

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using metadata;

namespace libsupcs
{
    public unsafe class Metadata
    {
        static metadata.MetadataStream mscorlib = null;
        static BinaryAssemblyLoader bal = null;

        public static BinaryAssemblyLoader BAL
        {
            get
            {
                if (bal == null)
                    bal = new BinaryAssemblyLoader();
                return bal;
            }
        }

        public static metadata.MetadataStream MSCorlib
        {
            get
            {
                if (mscorlib == null)
                    load_mscorlib();
                return mscorlib;
            }
        }

        public static metadata.AssemblyLoader AssemblyLoader
        {
            get
            {
                return BAL;
            }
        }

        private static void load_mscorlib()
        {
            var str = AssemblyLoader.LoadAssembly("mscorlib");
            metadata.PEFile pef = new metadata.PEFile();
            var m = pef.Parse(str, AssemblyLoader);

            AssemblyLoader.AddToCache(m, "mscorlib");
            mscorlib = m;
            BinaryAssemblyLoader.ptr_cache[(ulong)OtherOperations.GetStaticObjectAddress("mscorlib")] = m;
        }

        internal static unsafe metadata.TypeSpec GetTypeSpec(TysosType t)
        {
            void** impl_ptr = t.GetImplOffset();
            return GetTypeSpec(*impl_ptr);
        }

        internal static unsafe metadata.TypeSpec GetTypeSpec(RuntimeTypeHandle rth)
        {
            void* ptr = CastOperations.ReinterpretAsPointer(rth.Value);
            return GetTypeSpec(ptr);
        }

        internal static unsafe metadata.TypeSpec GetTypeSpec(void* ptr)
        {
            System.Diagnostics.Debugger.Log(0, "libsupcs", "Metadata.GetTypeSpec: called with vtbl " + ((ulong)ptr).ToString("X16"));
            // Ensure ptr is valid
            if (ptr == null)
            {
                System.Diagnostics.Debugger.Log(0, "libsupcs", "Metadata.GetTypeSpec: called with null pointer");
                throw new Exception("Invalid type handle");
            }

            // dereference vtbl pointer to get ti ptr
            ptr = *((void**)ptr);
            if (ptr == null)
            {
                System.Diagnostics.Debugger.Log(0, "libsupcs", "Metadata.GetTypeSpec: called with null pointer");
                throw new Exception("Invalid type handle");
            }

            if ((*((int*)ptr) & 0xf) != 0)
            {
                System.Diagnostics.Debugger.Log(0, "libsupcs", "Metadata.GetTypeSpec: called with invalid runtimehandle: " +
                    (*((int*)ptr)).ToString() + " at " + ((ulong)ptr).ToString("X16"));
                System.Diagnostics.Debugger.Break();
                throw new Exception("Invalid type handle");
            }

            // Get number of metadata references and pointers to each
            var ti_ptr = (void**)ptr;

            // skip over type, enum underlying type field, tysos type pointer, cctor and flags
            ti_ptr += 5;

            var mdref_count = *(int*)(ti_ptr);
            var mdref_arr = ti_ptr + 1;
            var sig_ptr = (byte*)(mdref_arr + mdref_count);

            System.Diagnostics.Debugger.Log(0, "libsupcs", "Metadata.GetTypeSpec: found " + mdref_count.ToString() + " metadata references");
            System.Diagnostics.Debugger.Log(0, "libsupcs", "Metadata.GetTypeSpec: parsing signature at " + ((ulong)sig_ptr).ToString("X16"));

            // Parse the actual signature
            return ParseTypeSpecSignature(ref sig_ptr, mdref_count, mdref_arr, null, null);
        }

        private static TypeSpec ParseTypeSpecSignature(ref byte* sig_ptr, int mdref_count, void** mdref_arr,
            TypeSpec[] gtparams, TypeSpec[] gmparams)
        {
            TypeSpec ts = new TypeSpec();

            var b = *sig_ptr++;

            bool is_generic = false;

            // CMOD_REQD/OPT
            while (b == 0x1f || b == 0x20)
                b = *sig_ptr++;

            if(b == 0x15)
            {
                is_generic = true;
                b = *sig_ptr++;
            }

            switch (b)
            {
                case 0x01:
                    // VOID
                    return null;
                case 0x02:
                case 0x03:
                case 0x04:
                case 0x05:
                case 0x06:
                case 0x07:
                case 0x08:
                case 0x09:
                case 0x0a:
                case 0x0b:
                case 0x0c:
                case 0x0d:
                case 0x0e:
                case 0x16:
                case 0x18:
                case 0x19:
                case 0x1c:
                    ts.m = MSCorlib;
                    ts.tdrow = ts.m.simple_type_rev_idx[b];
                    break;

                case 0x0f:
                    ts.m = AssemblyLoader.GetAssembly("mscorlib");
                    ts.stype = TypeSpec.SpecialType.Ptr;
                    ts.other = ParseTypeSpecSignature(ref sig_ptr, mdref_count, mdref_arr, gtparams, gmparams);
                    break;

                case 0x10:
                    ts.m = AssemblyLoader.GetAssembly("mscorlib");
                    ts.stype = TypeSpec.SpecialType.MPtr;
                    ts.other = ParseTypeSpecSignature(ref sig_ptr, mdref_count, mdref_arr, gtparams, gmparams);
                    break;

                case 0x31:
                case 0x32:
                    // Encoded class/vtype
                    var mdidx = SigReadUSCompressed(ref sig_ptr);
                    ts.m = BAL.GetAssembly(*(mdref_arr + mdidx));
                    var tok = SigReadUSCompressed(ref sig_ptr);
                    int tid, trow;
                    ts.m.GetCodedIndexEntry(tok, ts.m.TypeDefOrRef,
                        out tid, out trow);

                    ts = ts.m.GetTypeSpec(tid, trow, gtparams, gmparams);
                    break;

                case 0x13:
                    // VAR
                    var gtidx = SigReadUSCompressed(ref sig_ptr);
                    if (gtparams == null)
                        ts = new TypeSpec { stype = TypeSpec.SpecialType.Var, idx = (int)gtidx };
                    else
                        ts = gtparams[gtidx];
                    break;

                case 0x1e:
                    // MVAR
                    var gmidx = SigReadUSCompressed(ref sig_ptr);
                    if (gmparams == null)
                        ts = new TypeSpec { stype = TypeSpec.SpecialType.MVar, idx = (int)gmidx };
                    else
                        ts = gmparams[gmidx];
                    break;

                case 0x1d:
                    ts.m = BAL.GetAssembly("mscorlib");
                    ts.stype = TypeSpec.SpecialType.SzArray;

                    ts.other = ParseTypeSpecSignature(ref sig_ptr, mdref_count, mdref_arr, gtparams, gmparams);
                    break;

                case 0x14:
                    ts.m = BAL.GetAssembly("mscorlib");
                    ts.stype = TypeSpec.SpecialType.Array;

                    ts.other = ParseTypeSpecSignature(ref sig_ptr, mdref_count, mdref_arr, gtparams, gmparams);
                    ts.arr_rank = (int)SigReadUSCompressed(ref sig_ptr);

                    int boundsCount = (int)SigReadUSCompressed(ref sig_ptr);
                    ts.arr_sizes = new int[boundsCount];
                    for (int i = 0; i < boundsCount; i++)
                        ts.arr_sizes[i] = (int)SigReadUSCompressed(ref sig_ptr);

                    int loCount = (int)SigReadUSCompressed(ref sig_ptr);
                    ts.arr_lobounds = new int[loCount];
                    for (int i = 0; i < loCount; i++)
                        ts.arr_lobounds[i] = (int)SigReadUSCompressed(ref sig_ptr);

                    break;

                case 0x45:
                    ts = ParseTypeSpecSignature(ref sig_ptr, mdref_count, mdref_arr, gtparams, gmparams);
                    ts.Pinned = true;
                    break;

                default:
                    System.Diagnostics.Debugger.Log(0, "libsupcs", "Metadata.ParseTypeSpecSignature: invalid signature byte: " + b.ToString());
                    throw new NotImplementedException();
            }

            if (is_generic)
            {
                var gen_count = SigReadUSCompressed(ref sig_ptr);
                ts.gtparams = new TypeSpec[gen_count];
                for (uint i = 0; i < gen_count; i++)
                {
                    ts.gtparams[i] = ParseTypeSpecSignature(ref sig_ptr, mdref_count, mdref_arr, gtparams, gmparams);
                }
            }

            return ts;
        }

        private static uint SigReadUSCompressed(ref byte* sig_ptr)
        {
            byte b1 = *sig_ptr++;
            if ((b1 & 0x80) == 0)
                return b1;

            byte b2 = *sig_ptr++;
            if ((b1 & 0xc0) == 0x80)
                return (b1 & 0x3fU) << 8 | b2;

            byte b3 = *sig_ptr++;
            byte b4 = *sig_ptr++;
            return (b1 & 0x1fU) << 24 | ((uint)b2 << 16) |
                ((uint)b3 << 8) | b4;
        }

        public class BinaryAssemblyLoader : metadata.AssemblyLoader
        {
            internal static Dictionary<ulong, metadata.MetadataStream> ptr_cache =
                new Dictionary<ulong, metadata.MetadataStream>(
                    new GenericEqualityComparer<ulong>());

            public override DataInterface LoadAssembly(string name)
            {
                void* ptr;
                System.Diagnostics.Debugger.Log(0, "metadata", "Metadata.BinaryAssemblyLoader.LoadAssembly: request to load " + name);
                if(name == "mscorlib" || name == "mscorlib.dll")
                {
                    ptr = OtherOperations.GetStaticObjectAddress("mscorlib");
                }
                else if(name == "libsupcs" || name == "libsupcs.dll")
                {
                    ptr = OtherOperations.GetStaticObjectAddress("libsupcs");
                }
                else if (name == "metadata" || name == "metadata.dll")
                {
                    ptr = OtherOperations.GetStaticObjectAddress("metadata");
                }
                else
                {
                    ptr = JitOperations.GetAddressOfObject(name);
                }

                return new BinaryInterface(ptr);
            }

            public unsafe virtual MetadataStream GetAssembly(void *ptr)
            {
                MetadataStream m;
                if (ptr_cache.TryGetValue((ulong)ptr, out m))
                    return m;

                System.Diagnostics.Debugger.Log(0, "libsupcs", "Metadata.BinaryAssemblyLoader: loading assembly at: " + ((ulong)ptr).ToString());

                var bi = new BinaryInterface(ptr);
                PEFile p = new PEFile();
                m = p.Parse(bi, this);

                ptr_cache[(ulong)ptr] = m;
                cache[m.AssemblyName] = m;

                return m;
            }
        }

        unsafe internal class BinaryInterface : metadata.DataInterface
        {
            internal byte* b;

            public BinaryInterface(void *ptr)
            {
                b = (byte*)ptr;
            }

            public override byte ReadByte(int offset)
            {
                var ret = *(b + offset);
                //System.Diagnostics.Debugger.Break();
                return ret;
            }

            public override DataInterface Clone(int offset)
            {
                return new BinaryInterface(b + offset);
            }
        }

        class BinaryStream : System.IO.Stream
        {
            byte* d;
            bool canwrite;
            long pos;
            long len;

            public BinaryStream(byte *data, long length)
            {
                System.Diagnostics.Debugger.Break();
                d = data;
                len = length;
                canwrite = true;
                pos = 0;
            }

            public override int ReadByte()
            {
                var ret = *(d + pos++);
                System.Diagnostics.Debugger.Break();
                return ret;
            }

            public override bool CanRead
            {
                get
                {
                    return true;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return true;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return canwrite;
                }
            }

            public override long Length
            {
                get
                {
                    return len;
                }
            }

            public override long Position
            {
                get
                {
                    return pos;
                }

                set
                {
                    pos = value;
                }
            }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                for(int i = 0; i < count; i++)
                {
                    buffer[offset + i] = *(d + pos++);
                }
                System.Diagnostics.Debugger.Break();
                return count;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                switch(origin)
                {
                    case SeekOrigin.Begin:
                        pos = offset;
                        break;
                    case SeekOrigin.Current:
                        pos += offset;
                        break;
                    case SeekOrigin.End:
                        pos = len - offset;
                        break;
                }
                return pos;
            }

            public override void SetLength(long value)
            {
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                for(int i = 0; i < count; i++)
                {
                    *(d + pos++) = buffer[offset + i];
                }
            }
        }
    }
}
