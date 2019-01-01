/* Copyright (C) 2008 - 2011 by John Cronin
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

namespace tysos
{
    unsafe class SymbolTable
    {
        internal abstract class SymbolProvider
        {
            internal protected abstract ulong GetAddress(string s);
            internal protected abstract string GetSymbol(ulong address);
            internal protected abstract string GetSymbolAndOffset(ulong address, out ulong offset);
        }

        internal Dictionary<string, ulong> sym_to_offset;
        internal metadata.Collections.SortedList<ulong, string> offset_to_sym;
        internal Dictionary<string, ulong> sym_to_length;

        internal List<ulong> static_fields_addresses = new List<ulong>();
        internal List<ulong> static_fields_lengths = new List<ulong>();
        internal List<SymbolProvider> symbol_providers = new List<SymbolProvider>();

        public SymbolTable()
        {
            sym_to_offset = new Dictionary<string, ulong>(0x20000, new Program.MyGenericEqualityComparer<string>());
            offset_to_sym = new metadata.Collections.SortedList<ulong, string>(0x20000, new Program.MyComparer<ulong>());
            sym_to_length = new Dictionary<string, ulong>(0x20000, new Program.MyGenericEqualityComparer<string>());

            unsafe
            {
                ulong ots = libsupcs.CastOperations.ReinterpretAsUlong(offset_to_sym);
                Formatter.Write("offset_to_sym: ", Program.arch.DebugOutput);
                Formatter.Write(ots, "X", Program.arch.DebugOutput);
                Formatter.Write(", vtable: ", Program.arch.DebugOutput);
                Formatter.Write(*(ulong*)ots, "X", Program.arch.DebugOutput);
                Formatter.Write(", ti: ", Program.arch.DebugOutput);
                Formatter.Write(**(ulong**)ots, "X", Program.arch.DebugOutput);
                Formatter.WriteLine(Program.arch.DebugOutput);

            }

        }

        public void Add(string sym, ulong address)
        {
            unsafe
            {
                ulong ots = libsupcs.CastOperations.ReinterpretAsUlong(offset_to_sym);
                Formatter.Write("offset_to_sym: ", Program.arch.DebugOutput);
                Formatter.Write(ots, "X", Program.arch.DebugOutput);
                Formatter.Write(", vtable: ", Program.arch.DebugOutput);
                Formatter.Write(*(ulong*)ots, "X", Program.arch.DebugOutput);
                Formatter.Write(", ti: ", Program.arch.DebugOutput);
                Formatter.Write(**(ulong**)ots, "X", Program.arch.DebugOutput);
                Formatter.WriteLine(Program.arch.DebugOutput);

            }
            if (sym_to_offset.ContainsKey(sym))
            {
                Formatter.Write("Warning: duplicate symbol: ", Program.arch.DebugOutput);
                Formatter.WriteLine(sym, Program.arch.DebugOutput);
            }
            else
                sym_to_offset.Add(sym, address);
            if(!offset_to_sym.ContainsKey(address))
                offset_to_sym.Add(address, sym);
        }

        public void Add(string sym, ulong address, ulong length)
        {
            if (sym_to_offset.ContainsKey(sym))
            {
                Formatter.Write("Warning: duplicate symbol: ", Program.arch.DebugOutput);
                Formatter.WriteLine(sym, Program.arch.DebugOutput);
            }
            else
            {
                sym_to_offset.Add(sym, address);
                sym_to_length.Add(sym, length);
            }
            
            if (!offset_to_sym.ContainsKey(address))
                offset_to_sym.Add(address, sym);
        }

        public void AddStaticField(ulong address, ulong length)
        {
            static_fields_addresses.Add(address);
            static_fields_lengths.Add(length);
        }

        public ulong GetAddress(string sym)
        {
            foreach (SymbolProvider sp in symbol_providers)
            {
                ulong ret = sp.GetAddress(sym);
                if (ret != 0)
                    return ret;
            }

            if (sym_to_offset.ContainsKey(sym))
                return sym_to_offset[sym];
            else
                return 0;
        }

        public ulong GetLength(string sym)
        {
            /*foreach (SymbolProvider sp in symbol_providers)
            {
                ulong ret = sp.GetAddress(sym);
                if (ret != 0)
                    return ret;
            }*/

            if (sym_to_length.ContainsKey(sym))
                return sym_to_length[sym];
            else
                return 0;
        }

        public string GetSymbol(ulong address)
        {
            foreach (SymbolProvider sp in symbol_providers)
            {
                string ret = sp.GetSymbol(address);
                if (ret != null)
                    return ret;
            }

            return (string)offset_to_sym[address];
        }

        public string GetSymbolAndOffset(ulong address, out ulong offset)
        {
            foreach (SymbolProvider sp in symbol_providers)
            {
                string ret = sp.GetSymbolAndOffset(address, out offset);
                if (ret != null)
                    return ret;
            }

            if (offset_to_sym.ContainsKey(address))
            {
                offset = 0;
                return offset_to_sym[address];
            }

            offset_to_sym.Add(address, "probe");
            int idx = offset_to_sym.IndexOfKey(address);
            offset_to_sym.RemoveAt(idx);

            if (idx == 0)
            {
                offset = address;
                return "offset_0";
            }

            ulong sym_addr = offset_to_sym.Keys[idx - 1];
            offset = address - sym_addr;
            return offset_to_sym[sym_addr];
        }
    }
}
