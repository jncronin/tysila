/* Copyright (C) 2012 by John Cronin
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
using System.Runtime.CompilerServices;

namespace libsupcs.x86_64
{
    [ArchDependent("x86_64")]
    public class Cpu
    {
        [Bits64Only]
        public unsafe static void Lidt(ulong addr, ushort limit)
        {
            /* Build a idt_pointer:
             * 
             * least significant 16 bits = limit
             * next 64 bits = address
             */
            
            ulong *idt_ptr = stackalloc ulong[2];
            idt_ptr[1] = (addr >> 48) & 0xffffUL;
            idt_ptr[0] = (addr << 16) | ((ulong)limit & 0xffffUL);
            Lidt(idt_ptr);
        }

        [Bits32Only]
        public unsafe static void Lidt(uint addr, ushort limit)
        {
            /* Build a idt_pointer:
             * 
             * least significant 16 bits = limit
             * next 32 bits = address
             */

            uint *idt_ptr = stackalloc uint[2];
            idt_ptr[1] = (addr >> 16) & 0xffffU;
            idt_ptr[0] = (addr << 16) | ((uint)limit & 0xffffU);
            Lidt(idt_ptr);
        }

        [Bits64Only]
        public unsafe static void Sgdt(out void *addr, out ushort limit)
        {
            byte* ptr = stackalloc byte[10];
            Sgdt(ptr);
            addr = *(void**)(ptr + 2);
            limit = *(ushort*)ptr;
        }

        [InterruptRegisterStructure]
        public struct InterruptRegisters64
        {
            public ulong xmm15;
            public ulong xmm14;
            public ulong xmm13;
            public ulong xmm12;
            public ulong xmm11;
            public ulong xmm10;
            public ulong xmm9;
            public ulong xmm8;
            public ulong r15;
            public ulong r14;
            public ulong r13;
            public ulong r12;
            public ulong r11;
            public ulong r10;
            public ulong r9;
            public ulong r8;
            public ulong xmm7;
            public ulong xmm6;
            public ulong xmm5;
            public ulong xmm4;
            public ulong xmm3;
            public ulong xmm2;
            public ulong xmm1;
            public ulong xmm0;
            public ulong rsi;
            public ulong rdi;
            public ulong rdx;
            public ulong rcx;
            public ulong rbx;
            public ulong rax;
        }

        [InterruptRegisterStructure]
        public struct InterruptRegisters32
        {
            public uint xmm7;
            public uint xmm6;
            public uint xmm5;
            public uint xmm4;
            public uint xmm3;
            public uint xmm2;
            public uint xmm1;
            public uint xmm0;
            public uint rsi;
            public uint rdi;
            public uint rdx;
            public uint rcx;
            public uint rbx;
            public uint rax;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static unsafe extern void Sgdt(void* ptr);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public unsafe static extern void Lidt(void* ptr);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Ltr(ulong selector);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Invlpg(ulong vaddr);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Break();
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Int(byte int_no);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern ulong RdMsr(uint reg_no);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WrMsr(uint reg_no, ulong val);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static unsafe extern void _Cpuid(uint req_no, uint* buf);

        public static uint[] Cpuid(uint req_no)
        {
            uint[] buf = new uint[4];

            unsafe
            {
                _Cpuid(req_no, (uint*)MemoryOperations.GetInternalArray(buf));
            }

            return buf;
        }

        public extern static ulong RBP
        {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
            [MethodImpl(MethodImplOptions.InternalCall)]
            set;
        }

        public extern static ulong RSP
        {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
            [MethodImpl(MethodImplOptions.InternalCall)]
            set;
        }

        public extern static uint Mxcsr
        {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
            [MethodImpl(MethodImplOptions.InternalCall)]
            set;
        }

        public extern static ulong Tsc
        {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

        public extern static ulong Cr0
        {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
            [MethodImpl(MethodImplOptions.InternalCall)]
            set;
        }

        public extern static ulong Cr2
        {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
            [MethodImpl(MethodImplOptions.InternalCall)]
            set;
        }

        public extern static ulong Cr3
        {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
            [MethodImpl(MethodImplOptions.InternalCall)]
            set;
        }

        public extern static ulong Cr4
        {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
            [MethodImpl(MethodImplOptions.InternalCall)]
            set;
        }  

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static void Sti();
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static void Cli();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern unsafe static void* ReadFSData(int offset);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern unsafe static void* ReadGSData(int offset);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern unsafe static void WriteFSData(int offset, void* data);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern unsafe static void WriteGSData(int offset, void* data);
    }
}
