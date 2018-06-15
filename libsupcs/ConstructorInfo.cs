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
    class ConstructorInfo : System.Reflection.ConstructorInfo
    {
        TysosMethod _meth;
        TysosType _type;

        internal ConstructorInfo(TysosMethod meth, TysosType type) {
            if (meth == null)
                throw new Exception("libsupcs.ConstructorInfo constructor called with meth as null");
            if (type == null)
                throw new Exception("libsupcs.ConstructorInfo constructor called with type as null");
            _meth = meth; _type = type;
        }

        public override object Invoke(System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object[] parameters, System.Globalization.CultureInfo culture)
        {
            if (_meth == null)
                throw new Exception("libsupcs.ConstructorInfo.Invoke: _meth is null");
            if (_type == null)
                throw new Exception("libsupcs.ConstructorInfo.Invoke: _type is null");
            object obj = _type.Create();
            _meth.Invoke(obj, invokeAttr, binder, parameters, culture);
            return obj;
        }

        public override System.Reflection.MethodAttributes Attributes
        {
            get { return _meth.Attributes; }
        }

        public override System.Reflection.MethodImplAttributes GetMethodImplementationFlags()
        {
            return _meth.GetMethodImplementationFlags();
        }

        public override System.Reflection.ParameterInfo[] GetParameters()
        {
            return _meth.GetParameters();
        }

        public override object Invoke(object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object[] parameters, System.Globalization.CultureInfo culture)
        {
            _meth.Invoke(obj, invokeAttr, binder, parameters, culture);
            return obj;
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get { return _meth.MethodHandle; }
        }

        public override Type DeclaringType
        {
            get { return _meth.DeclaringType; }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return _meth.GetCustomAttributes(attributeType, inherit);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return _meth.GetCustomAttributes(inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return _meth.IsDefined(attributeType, inherit);
        }

        public override string Name
        {
            get { return _meth.Name; }
        }

        public override Type ReflectedType
        {
            get { return _meth.ReflectedType; }
        }
    }
}
