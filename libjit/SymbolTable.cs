/* Copyright (C) 2008 - 2019 by John Cronin
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
            internal protected abstract void* GetAddress(string s);
            internal protected abstract string GetSymbol(void* address);
            internal protected abstract string GetSymbolAndOffset(void* address, out UIntPtr offset);
        }

        internal Dictionary<string, UIntPtr> sym_to_offset;
        internal metadata.Collections.SortedList<UIntPtr, string> offset_to_sym;
        internal Dictionary<string, UIntPtr> sym_to_length;

        internal List<ulong> static_fields_addresses = new List<ulong>();
        internal List<ulong> static_fields_lengths = new List<ulong>();
        internal List<SymbolProvider> symbol_providers = new List<SymbolProvider>();

        public SymbolTable()
        {
            sym_to_offset = new Dictionary<string, UIntPtr>(0x20000, new libsupcs.GenericEqualityComparer<string>());
            offset_to_sym = new metadata.Collections.SortedList<UIntPtr, string>(0x20000, new libsupcs.UIntPtrComparer());
            sym_to_length = new Dictionary<string, UIntPtr>(0x20000, new libsupcs.GenericEqualityComparer<string>());
        }

        public void Add(string sym, void* address)
        {
            unsafe
            {
                ulong ots = libsupcs.CastOperations.ReinterpretAsUlong(offset_to_sym);
            }
            if (sym_to_offset.ContainsKey(sym))
            {
                System.Diagnostics.Debugger.Log(0, "libjit", "SymbolTable.Add: Warning: duplicate symbol: " +
                    sym);
            }
            else
                sym_to_offset.Add(sym, (UIntPtr)address);
            if(!offset_to_sym.ContainsKey((UIntPtr)address))
                offset_to_sym.Add((UIntPtr)address, sym);
        }

        public void Add(string sym, void* address, UIntPtr length)
        {
            if (sym_to_offset.ContainsKey(sym))
            {
                System.Diagnostics.Debugger.Log(0, "libjit", "SymbolTable.Add: Warning: duplicate symbol: " +
                    sym);
            }
            else
            {
                sym_to_offset.Add(sym, (UIntPtr)address);
                sym_to_length.Add(sym, length);
            }
            
            if (!offset_to_sym.ContainsKey((UIntPtr)address))
                offset_to_sym.Add((UIntPtr)address, sym);
        }

        public void AddStaticField(ulong address, ulong length)
        {
            static_fields_addresses.Add(address);
            static_fields_lengths.Add(length);
        }

        public void* GetAddress(string sym)
        {
            foreach (SymbolProvider sp in symbol_providers)
            {
                var ret = sp.GetAddress(sym);
                if (ret != null)
                    return ret;
            }

            if (sym_to_offset.ContainsKey(sym))
                return (void*)sym_to_offset[sym];
            else
                return (void*)0;
        }

        public UIntPtr GetLength(string sym)
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
                return (UIntPtr)0;
        }

        public string GetSymbol(void* address)
        {
            foreach (SymbolProvider sp in symbol_providers)
            {
                string ret = sp.GetSymbol(address);
                if (ret != null)
                    return ret;
            }

            return (string)offset_to_sym[(UIntPtr)address];
        }

        public string GetSymbolAndOffset(void* address, out UIntPtr offset)
        {
            foreach (SymbolProvider sp in symbol_providers)
            {
                string ret = sp.GetSymbolAndOffset(address, out offset);
                if (ret != null)
                    return ret;
            }

            if (offset_to_sym.ContainsKey((UIntPtr)address))
            {
                offset = (UIntPtr)0;
                return offset_to_sym[(UIntPtr)address];
            }

            offset_to_sym.Add((UIntPtr)address, "probe");
            int idx = offset_to_sym.IndexOfKey((UIntPtr)address);
            offset_to_sym.RemoveAt(idx);

            if (idx == 0)
            {
                offset = (UIntPtr)address;
                return "offset_0";
            }

            var sym_addr = offset_to_sym.Keys[idx - 1];
            offset = libsupcs.OtherOperations.Sub((UIntPtr)address, sym_addr);
            return offset_to_sym[sym_addr];
        }
    }
}
