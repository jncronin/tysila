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
using libtysila5.layout;
using libtysila5.util;
using metadata;

/* Implements a generic requestor to use for vtables and methods, and a
    default implementation that cache requests */
namespace libtysila5
{
    public abstract class Requestor
    {
        public abstract IndividualRequestor<TypeSpec> VTableRequestor { get; }
        public abstract IndividualRequestor<layout.Layout.MethodSpecWithEhdr> MethodRequestor { get; }
        public abstract IndividualRequestor<layout.Layout.MethodSpecWithEhdr> EHRequestor { get; }
        public abstract IndividualRequestor<layout.Layout.MethodSpecWithEhdr> BoxedMethodRequestor { get; }
        public abstract IndividualRequestor<TypeSpec> StaticFieldRequestor { get; }
        public abstract IndividualRequestor<TypeSpec> DelegateRequestor { get; }

        public virtual bool Empty
        {
            get
            {
                if (!VTableRequestor.Empty)
                    return false;
                if (!MethodRequestor.Empty)
                    return false;
                if (!EHRequestor.Empty)
                    return false;
                if (!StaticFieldRequestor.Empty)
                    return false;
                if (!DelegateRequestor.Empty)
                    return false;
                if (!BoxedMethodRequestor.Empty)
                    return false;
                return true;
            }
        }
    }

    public class CachingRequestor : Requestor
    {
        CachingIndividualRequestor<layout.Layout.MethodSpecWithEhdr> m;
        CachingIndividualRequestor<layout.Layout.MethodSpecWithEhdr> eh;
        CachingIndividualRequestor<layout.Layout.MethodSpecWithEhdr> bm;
        CachingIndividualRequestor<TypeSpec> vt;
        CachingIndividualRequestor<TypeSpec> sf;
        CachingIndividualRequestor<TypeSpec> d;

#if DEBUG
        void CheckInstatiated(TypeSpec t)
        {
            if (t.IsGenericTemplate)
                throw new Exception();
        }
#endif

#if DEBUG
        void CheckGMInstantiated(Layout.MethodSpecWithEhdr t)
        {
            if (t.ms.IsGeneric && t.ms.IsGenericTemplate)
                throw new Exception();
        }
#endif

        public CachingRequestor(MetadataStream mstream = null)
        {
            m = new CachingIndividualRequestor<layout.Layout.MethodSpecWithEhdr>(mstream);
            eh = new CachingIndividualRequestor<layout.Layout.MethodSpecWithEhdr>(mstream);
            bm = new CachingIndividualRequestor<layout.Layout.MethodSpecWithEhdr>(mstream);
            vt = new CachingIndividualRequestor<TypeSpec>(mstream);
            sf = new CachingIndividualRequestor<TypeSpec>(mstream);
            d = new CachingIndividualRequestor<TypeSpec>(mstream);

#if DEBUG
            d.DebugCheck = CheckInstatiated;
            m.DebugCheck = CheckGMInstantiated;
#endif
        }

        public override IndividualRequestor<layout.Layout.MethodSpecWithEhdr> MethodRequestor
        {
            get
            {
                return m;
            }
        }

        public override IndividualRequestor<layout.Layout.MethodSpecWithEhdr> EHRequestor
        {
            get
            {
                return eh;
            }
        }

        public override IndividualRequestor<TypeSpec> VTableRequestor
        {
            get
            {
                return vt;
            }
        }

        public override IndividualRequestor<TypeSpec> StaticFieldRequestor
        {
            get
            {
                return sf;
            }
        }

        public override IndividualRequestor<TypeSpec> DelegateRequestor
        {
            get
            {
                return d;
            }
        }

        public override IndividualRequestor<Layout.MethodSpecWithEhdr> BoxedMethodRequestor
        {
            get
            {
                return bm;
            }
        }
    }

    public abstract class IndividualRequestor<T> where T : IEquatable<T>
    {
        public abstract T GetNext();
        public abstract bool Empty { get; }
        public abstract void Request(T v);
        public abstract void Remove(T v);
    }

    public class CachingIndividualRequestor<T> : IndividualRequestor<T> where T : Spec, IEquatable<T>
    {
        Set<T> done_and_pending = new Set<T>();
        util.Stack<T> pending = new util.Stack<T>();
        MetadataStream m;

#if DEBUG
        public delegate void DebugChecker(T val);
        public DebugChecker DebugCheck { get; internal set; }
#endif

        public CachingIndividualRequestor(MetadataStream mstream = null)
        {
            m = mstream;
        }

        public override bool Empty
        {
            get
            {
                return pending.Count == 0;
            }
        }

        public override T GetNext()
        {
            return pending.Pop();
        }

        public override void Request(T v)
        {
#if DEBUG
            if(DebugCheck != null)
                DebugCheck(v);
#endif
            if (m != null && m != v.Metadata)
            {
                if(!v.IsInstantiatedGenericType && 
                    !v.IsInstantiatedGenericMethod &&
                    !v.IsArray)
                return;
            }

            if(!done_and_pending.Contains(v))
            {
                done_and_pending.Add(v);
                pending.Push(v);
            }
        }

        public override void Remove(T v)
        {
            if (done_and_pending.Contains(v))
                done_and_pending.Remove(v);
        }
    }
}
