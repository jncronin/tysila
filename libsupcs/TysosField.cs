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

/* This defines the TysosField which is a subtype of System.Reflection.FieldInfo
 * 
 * All FieldInfo structures produced by tysila2 follow this layout
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace libsupcs
{
    [VTableAlias("__tysos_field_vt")]
    public class TysosField : System.Reflection.FieldInfo
    {
        internal TysosType OwningType;
        internal TysosType _FieldType;
        string _Name;
        IntPtr Signature;
        IntPtr Sig_references;
        public IntPtr Literal_data;
        public IntPtr Constant_data;
        UInt32 Flags;
        internal Int32 offset;

        public const UInt32 IF_FLAGS = 0xffff;
        public const UInt32 IF_RUNTIME_INTERNAL = 0x10000;

        public UInt32 ImplFlags { get { return Flags >> 16; } }
        public UInt32 FieldFlags { get { return Flags & IF_FLAGS; } }
        public Int32 Offset { get { return offset; } }

        public override System.Reflection.FieldAttributes Attributes
        {
            get { return (System.Reflection.FieldAttributes)FieldFlags; }
        }

        public override RuntimeFieldHandle FieldHandle
        {
            get { throw new NotImplementedException(); }
        }

        public override Type FieldType
        {
            get { return _FieldType; }
        }

        public override object GetValue(object obj)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object obj, object value, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override Type DeclaringType
        {
            get { throw new NotImplementedException(); }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { return _Name; }
        }

        public override Type ReflectedType
        {
            get { throw new NotImplementedException(); }
        }
    }
}
