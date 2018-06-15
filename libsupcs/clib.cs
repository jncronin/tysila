/* Copyright (C) 2014 by John Cronin
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

/* Standard C library functions */

namespace libsupcs
{
    class clib
    {
        [libsupcs.MethodAlias("__mbstrlen")]
        [libsupcs.MethodAlias("mbstrlen")]
        [libsupcs.MethodAlias("strlen")]
        [libsupcs.WeakLinkage]
        [libsupcs.AlwaysCompile]
        unsafe internal static int MbStrLen(sbyte* value)
        {
            /* For ASCII strings only!  This doesn't properly handle UTF-8 yet */
            int length = 0;

            while (*value != 0x00)
            {
                length++;
                value++;
            }

            return length;
        }

        [libsupcs.MethodAlias("wmemset")]
        [libsupcs.WeakLinkage]
        [libsupcs.AlwaysCompile]
        unsafe internal static char* wmemset(char* wcs, char wc, int n)
        {
            char* dest = wcs;
            for (int i = 0; i < n; i++)
                *dest++ = wc;
            return wcs;
        }

        [libsupcs.MethodAlias("wcslen")]
        [libsupcs.WeakLinkage]
        [libsupcs.AlwaysCompile]
        unsafe internal static int wcslen(char* s)
        {
            int length = 0;

            while (*s != 0x00)
            {
                length++;
                s++;
            }

            return length;
        }

        [libsupcs.MethodAlias("memset")]
        [libsupcs.WeakLinkage]
        [libsupcs.AlwaysCompile]
        unsafe internal static byte* memset(byte* s, int c, int n)
        {
            byte* dest = s;
            for (int i = 0; i < n; i++)
                *dest++ = (byte)c;
            return s;
        }

        [libsupcs.MethodAlias("memcmp")]
        [libsupcs.WeakLinkage]
        [libsupcs.AlwaysCompile]
        unsafe internal static int memcmp(byte *s1, byte *s2, int n)
        {
            for(int i = 0; i < n; i++)
            {
                int v = s1[i] - s2[i];
                if (v != 0)
                    return v;
            }
            return 0;
        }

        [libsupcs.MethodAlias("memcpy")]
        [libsupcs.WeakLinkage]
        [libsupcs.AlwaysCompile]
        unsafe internal static byte* memcpy(byte* dest, byte* src, int n)
        {
            byte* d = dest;
            while (n-- > 0)
                *dest++ = *src++;
            return d;
        }

        [libsupcs.MethodAlias("memmove")]
        [libsupcs.WeakLinkage]
        [libsupcs.AlwaysCompile]
        unsafe internal static byte* memmove(byte* dest, byte* src, int n)
        {
            byte* d = dest;
            byte* s = src;

            if (d > s)
            {
                /* Perform a backwards copy */
                d += n;
                s += n;
                while (n-- > 0)
                {
                    *--d = *--s;
                }
            }
            else
            {
                /* Normal memcpy-like copy */
                while (n-- > 0)
                    *d++ = *s++;
            }
            return dest;
        }

        [libsupcs.MethodAlias("mbstowcs")]
        [libsupcs.WeakLinkage]
        [libsupcs.AlwaysCompile]
        unsafe static void MbsToWcs(char* dest, sbyte* src, int length)
        {
            for (int i = 0; i < length; i++)
            {
                /* For ASCII strings only!  This doesn't properly handle UTF-8 yet */
                *dest = (char)*src;

                dest++;
                src++;
            }
        }

        [libsupcs.MethodAlias("abort")]
        [libsupcs.WeakLinkage]
        [libsupcs.AlwaysCompile]
        static void Abort()
        {
            while (true) ;
        }
    }
}
