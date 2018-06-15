/* Copyright (C) 2018 by John Cronin
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


/* Managed implementation of number formatting for coreclr */

using System;
using System.Globalization;

namespace libsupcs
{
    unsafe class NumberFormat
    {
        static string uppercaseDigits = "0123456789ABCDEF";
        static string lowercaseDigits = "0123456789abcdef";

        [WeakLinkage]
        [AlwaysCompile]
        [MethodAlias("_ZW6System6Number_12FormatUInt32_Ru1S_P3ju1SU22System#2EGlobalization16NumberFormatInfo")]
        static string FormatUInt32(uint v, string fmt, NumberFormatInfo nfi)
        {
            byte* v2 = stackalloc byte[8];
            *(ulong*)v2 = v;
            return FormatInteger(v2, false, 4, fmt, nfi);
        }

        [WeakLinkage]
        [AlwaysCompile]
        [MethodAlias("_ZW6System6Number_11FormatInt32_Ru1S_P3iu1SU22System#2EGlobalization16NumberFormatInfo")]
        static string FormatInt32(int v, string fmt, NumberFormatInfo nfi)
        {
            byte* v2 = stackalloc byte[8];
            *(long*)v2 = v;
            return FormatInteger(v2, true, 4, fmt, nfi);
        }

        [WeakLinkage]
        [AlwaysCompile]
        [MethodAlias("_ZW6System6Number_12FormatUInt64_Ru1S_P3yu1SU22System#2EGlobalization16NumberFormatInfo")]
        static string FormatUInt64(ulong v, string fmt, NumberFormatInfo nfi)
        {
            byte* v2 = stackalloc byte[8];
            *(ulong*)v2 = v;
            return FormatInteger(v2, false, 8, fmt, nfi);
        }

        [WeakLinkage]
        [AlwaysCompile]
        [MethodAlias("_ZW6System6Number_11FormatInt64_Ru1S_P3xu1SU22System#2EGlobalization16NumberFormatInfo")]
        static string FormatInt64(long v, string fmt, NumberFormatInfo nfi)
        {
            byte* v2 = stackalloc byte[8];
            *(long*)v2 = v;
            return FormatInteger(v2, true, 8, fmt, nfi);
        }

        [WeakLinkage]
        [AlwaysCompile]
        [MethodAlias("_ZW6System6Number_12FormatDouble_Ru1S_P3du1SU22System#2EGlobalization16NumberFormatInfo")]
        static string FormatDouble(double v, string fmt, NumberFormatInfo nfi)
        {
            // we do not currently support doubles longer than int64 or other than fixed point
            // no more than 9 decimal places
            if (double.IsNegativeInfinity(v))
            {
                return nfi != null ? (nfi.NegativeInfinitySymbol ?? ((nfi.NegativeSign ?? "-") + "Inf")) : "-Inf";
            }
            if (double.IsPositiveInfinity(v))
            {
                return nfi != null ? (nfi.PositiveInfinitySymbol ?? "Inf") : "Inf";
            }
            if (double.IsNaN(v))
            {
                return nfi != null ? (nfi.NaNSymbol ?? "NaN") : "NaN";
            }

            // get integral portion
            if (v < long.MinValue || v > long.MaxValue)
                return "<too large>";

            var integral = (long)v;
            var istr = FormatInt64(integral, null, nfi);

            // get decimal portion
            const int MAX_DEC = 9;
            const int MAX_DEC_P10 = 10 ^ MAX_DEC;
            var d = (long)((v - integral) * MAX_DEC_P10);
            var dstr = FormatInt64(d, null, nfi);

            // append them together
            const int MAX_STR = 256;

            char* ret = stackalloc char[MAX_STR];
            int cur_ret = 0;
            foreach (var c in istr)
                ret[cur_ret++] = c;

            if (dstr.Length != 1 || dstr[0] != '0')
            {
                // There is a fractional part of the number
                var dec_pt = nfi != null ? (nfi.NumberDecimalSeparator ?? ".") : ".";
                foreach (var c in dec_pt)
                    ret[cur_ret++] = c;

                // pad with zeros until significant part is reached
                for (int i = 0; i < (dstr.Length - MAX_DEC); i++)
                    ret[cur_ret++] = '0';

                // determine how much of the actual value to print e.g. if it is 0.123456 we only print 6
                //  digits, but dstr is "123456000"
                int to_trim = 0;
                for(int i = dstr.Length - 1; i >= 0; i--)
                {
                    if (dstr[i] == '0')
                        to_trim++;
                    else
                        break;
                }
                for (int i = 0; i < dstr.Length - to_trim; i++)
                    ret[cur_ret++] = dstr[i];
            }

            return new string(ret, 0, cur_ret);
        }

        /* The main format function */
        static string FormatInteger(byte *v, bool is_signed, int blen, string fmt, NumberFormatInfo nfi)
        {
            long s = *(long*)v;
            ulong us = *(ulong*)v;

            bool is_negative = false;
            if(is_signed)
            {
                if (s < 0)
                {
                    is_negative = true;
                    us = (ulong)(-s);
                }
                else
                    us = (ulong)s;
            }

            const int MAX_STR = 256;

            char* ret = stackalloc char[MAX_STR];
            int cur_ret = 0;

            if (fmt == null || fmt.Equals(string.Empty))
                fmt = "G";
            char* f = StringOperations.GetChars(fmt);
            int cur_fmt = 0;
            int l_fmt = fmt.Length;

            char c_f = *f;      // Current formatting character
            int p = -1;         // Current precision (used if 'G' is converted to 'F')

            while(cur_fmt < l_fmt)
            {
                switch(c_f)
                {
                    case 'G':
                    case 'g':
                        {
                            /* Generic format strings are either fixed point or exponential, depending on
                             * the exponent of the number */
                            int sig_digits = get_number_from_fmt_string(f, cur_fmt, l_fmt, out int new_cur_fmt);
                            if(sig_digits == -1)
                            {
                                switch(blen)
                                {
                                    case 4:
                                        sig_digits = 10;
                                        break;
                                    case 8:
                                        sig_digits = 19;
                                        break;
                                    default:
                                        sig_digits = 29;
                                        break;
                                }
                            }
                            int exp = get_exponent_u(us);
                            if(exp < sig_digits && exp >= -4)
                            {
                                c_f = 'F';
                                p = 0;
                            }
                            else
                            {
                                if (c_f == 'G')
                                    c_f = 'E';
                                else
                                    c_f = 'e';
                            }
                        }
                        continue;

                    case 'F':
                    case 'f':
                    case 'N':
                    case 'n':
                        {
                            if (p == -1)
                                p = get_number_from_fmt_string(f, cur_fmt + 1, l_fmt, out cur_fmt);
                            else
                                get_number_from_fmt_string(f, cur_fmt + 1, l_fmt, out cur_fmt);     // ignore p in the string (this is a 'G' converted to 'F')
                            if(p == -1)
                            {
                                if (nfi != null)
                                    p = nfi.NumberDecimalDigits;
                                else
                                    p = 2;
                            }
                            if(is_negative)
                            {
                                if (nfi != null && nfi.NegativeSign != null)
                                    append_string(ret, nfi.NegativeSign, ref cur_ret, MAX_STR);
                                else
                                    append_string(ret, "-", ref cur_ret, MAX_STR);
                            }

                            /* build a string before the decimal point */
                            char* rev_str = stackalloc char[MAX_STR];
                            int cur_rev_str = 0;
                            ulong c_us = us;
                            string[] digits = null;
                            if (nfi != null && nfi.NativeDigits != null)
                                digits = nfi.NativeDigits;
                            while(c_us != 0)
                            {
                                if (digits == null)
                                    rev_str[cur_rev_str++] = (char)('0' + (c_us % 10));
                                else
                                    append_string(rev_str, digits[(int)(c_us % 10)], ref cur_rev_str, MAX_STR);
                                c_us /= 10;
                            }

                            /* append back onto the original string in reverse order */
                            if (cur_rev_str == 0)
                                append_string(ret, "0", ref cur_ret, MAX_STR);
                            else
                            {
                                while(cur_rev_str > 0)
                                {
                                    char c = rev_str[--cur_rev_str];
                                    if(((c_f == 'n') || (c_f == 'N')) && cur_rev_str != 0 && ((cur_rev_str % 3) == 0))
                                    {
                                        if (nfi != null && nfi.NumberGroupSeparator != null)
                                            append_string(ret, nfi.NumberGroupSeparator, ref cur_ret, MAX_STR);
                                        else
                                            append_string(ret, ",", ref cur_ret, MAX_STR);
                                    }
                                    if (cur_ret < MAX_STR)
                                        ret[cur_ret++] = c;
                                }
                            }

                            if(p > 0)
                            {
                                /* Add .0000... after string */
                                if (nfi != null && nfi.NumberDecimalSeparator != null)
                                    append_string(ret, nfi.NumberDecimalSeparator, ref cur_ret, MAX_STR);
                                else
                                    append_string(ret, ".", ref cur_ret, MAX_STR);

                                var cur_p = p;
                                while (p-- > 0)
                                    append_string(ret, "0", ref cur_ret, MAX_STR);
                            }

                            p = -1;     // reset precision so that we look for it again in the next formatting operation - if 'G' then it will be set to zero again
                        }
                        break;

                    case 'X':
                    case 'x':
                        {
                            int dcount = get_number_from_fmt_string(f, cur_fmt + 1, l_fmt, out cur_fmt);
                            if (dcount == -1)
                                dcount = blen * 2;
                            string digits;
                            if (c_f == 'X')
                                digits = uppercaseDigits;
                            else
                                digits = lowercaseDigits;
                            for(int i = dcount - 1; i >= 0; i--)
                            {
                                if(i > (blen * 2))
                                {
                                    if (cur_ret < MAX_STR)
                                        ret[cur_ret++] = '0';
                                }
                                else
                                {
                                    byte b = v[i / 2];
                                    if ((i % 2) == 1)
                                        b >>= 4;
                                    else
                                        b &= 0xf;
                                    if (cur_ret < MAX_STR)
                                        ret[cur_ret++] = digits[b];
                                }
                            }
                        }
                        break;

                    default:
                        /* Add verbatim */
                        if (cur_ret < MAX_STR)
                            ret[cur_ret++] = c_f;
                        cur_fmt++;
                        break;
                }
                c_f = f[cur_fmt];
            }

            return new string(ret, 0, cur_ret);
        }

        private static void append_string(char* ret, string s, ref int cur_ret, int mAX_STR)
        {
            int s_idx = 0;
            while(cur_ret < mAX_STR && s_idx < s.Length)
            {
                ret[cur_ret++] = s[s_idx++];
            }
        }

        private static int get_exponent_u(ulong us)
        {
            int ret = 0;
            while(us >= 10)
            {
                us /= 10;
                ret++;
            }
            return ret;
        }

        private static int get_number_from_fmt_string(char* f, int cur_fmt, int l_fmt, out int new_cur_fmt)
        {
            if (cur_fmt >= l_fmt || !char.IsDigit(f[cur_fmt]))
            {
                new_cur_fmt = cur_fmt;
                return -1;
            }

            int ret = 0;
            while(cur_fmt < l_fmt && char.IsDigit(f[cur_fmt]))
            {
                int d = f[cur_fmt] - '0';
                ret *= 10;
                ret += d;
                cur_fmt++;
            }

            new_cur_fmt = cur_fmt;
            return ret;
        }
    }
}
