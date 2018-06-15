/* Copyright (C) 2011 by John Cronin
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

namespace libsupcs
{
    class Locale
    {
        [AlwaysCompile]
        [WeakLinkage]
        [libsupcs.MethodAlias("_ZW22System#2EGlobalization11CompareInfo_21construct_compareinfo_Rv_P2u1tu1S")]
        static void CompareInfo_construct_compareinfo(System.Globalization.CompareInfo ci, string locale)
        {
            /* Mono's implementation of this (mono/metadata/locales.c) does nothing, so we do the same */
        }

        [AlwaysCompile]
        [WeakLinkage]
        [libsupcs.MethodAlias("_ZW22System#2EGlobalization11CompareInfo_22free_internal_collator_Rv_P1u1t")]
        static void CompareInfo_free_internal_collator(System.Globalization.CompareInfo ci)
        {
            /* Mono's implementation of this (mono/metadata/locales.c) does nothing, so we do the same */
        }

        [AlwaysCompile]
        [WeakLinkage]
        [libsupcs.MethodAlias("_ZW22System#2EGlobalization11CompareInfo_16internal_compare_Ri_P8u1tu1Siiu1SiiV14CompareOptions")]
        static int CompareInfo_internal_compare(System.Globalization.CompareInfo ci, string str1, int offset1,
            int length1, string str2, int offset2, int length2, System.Globalization.CompareOptions options)
        {
            /* Based off the mono implementation */

            int length = length1;
            if(length2 > length)
                length = length2;

            if((offset1 + length) > str1.Length)
                throw new Exception("Trying to compare more characters than exist in str1");
            if((offset2 + length) > str2.Length)
                throw new Exception("Trying to compare more characters than exist in str2");

            for(int i = 0; i < length; i++)
            {
                int cc = compare_char(str1[offset1 + i], str2[offset2 + i], options);
                if(cc != 0)
                    return cc;
            }

            return 0;
        }

        private static int compare_char(char a, char b, System.Globalization.CompareOptions options)
        {
            int result;

            if ((options & System.Globalization.CompareOptions.Ordinal) == System.Globalization.CompareOptions.Ordinal)
                return (int)a - (int)b;
            if ((options & System.Globalization.CompareOptions.IgnoreCase) == System.Globalization.CompareOptions.IgnoreCase)
            {
                char al = char.ToLower(a);
                char bl = char.ToLower(b);

                result = (int)al - (int)bl;
            }
            else
                result = (int)a - (int)b;

            if (result < 0)
                return -1;
            if (result > 0)
                return 1;
            return 0;
        }
    }
}
