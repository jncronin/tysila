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
using System.IO;
using System.Text;
using libtysila5.ir;

namespace libtysila5.target
{
    public abstract partial class Target
    {
        public static Dictionary<string, Target> targets =
            new Dictionary<string, Target>(
                new libtysila5.GenericEqualityComparer<string>());

        public static Dictionary<int, string> pt_names
            = new Dictionary<int, string>(
                new GenericEqualityComparer<int>());

        public static Dictionary<int, string> rt_map
           = new Dictionary<int, string>(
               new GenericEqualityComparer<int>());

        protected static Dictionary<int, string> insts =
            new Dictionary<int, string>(
                new GenericEqualityComparer<int>());

        internal Dictionary<int, long> ct_regs =
            new Dictionary<int, long>(
                new GenericEqualityComparer<int>());

        public Dictionary<string, ulong> cc_callee_preserves_map
            = new Dictionary<string, ulong>(
                new GenericEqualityComparer<string>());
        public Dictionary<string, ulong> cc_caller_preserves_map
            = new Dictionary<string, ulong>(
                new GenericEqualityComparer<string>());
        public Dictionary<string, Dictionary<int, int[]>> cc_map
            = new Dictionary<string, Dictionary<int, int[]>>(
                new GenericEqualityComparer<string>());
        public Dictionary<string, Dictionary<int, int>> cc_classmap
            = new Dictionary<string, Dictionary<int, int>>(
                new GenericEqualityComparer<string>());
        public Dictionary<string, Dictionary<int, int[]>> retcc_map
            = new Dictionary<string, Dictionary<int, int[]>>(
                new GenericEqualityComparer<string>());
        public Dictionary<string, Dictionary<int, int>> retcc_classmap
            = new Dictionary<string, Dictionary<int, int>>(
                new GenericEqualityComparer<string>());

        protected internal abstract int GetCondCode(MCInst i);
        protected internal abstract Reg GetMoveSrc(MCInst i);
        protected internal abstract Reg GetMoveDest(MCInst i);
        protected internal abstract bool IsBranch(MCInst i);
        protected internal abstract bool IsCall(MCInst i);
        protected internal abstract int GetBranchDest(MCInst i);
        protected internal abstract void SetBranchDest(MCInst i, int d);
        protected internal abstract Reg GetLVLocation(int lv_loc, int lv_size, Code c);
        protected internal abstract Reg GetLALocation(int la_loc, int la_size, Code c);
        protected internal abstract MCInst[] SetupStack(int lv_size);
        protected internal abstract MCInst[] CreateMove(Reg src, Reg dest);
        protected internal abstract binary_library.IRelocationType GetDataToDataReloc();
        protected internal abstract binary_library.IRelocationType GetDataToCodeReloc();

        protected internal abstract Code AssembleBoxedMethod(metadata.MethodSpec ms);
        protected internal abstract Code AssembleBoxRetTypeMethod(metadata.MethodSpec ms);
        protected internal abstract bool NeedsBoxRetType(metadata.MethodSpec ms);
        protected internal abstract void AssemblePass(Code c);

        protected internal virtual int GetCTFromTypeForCC(metadata.TypeSpec t) { return ir.Opcode.GetCTFromType(t); }

        protected internal virtual void AddExtraVTableFields(metadata.TypeSpec ts, IList<byte> d, ref ulong offset) { }
        protected internal virtual int ExtraVTableFieldsPointerLength { get { return 0; } }

        public virtual void InitIntcalls() { }

        TargetOptions opts = new TargetOptions();
        public TargetOptions Options { get { return opts; } }

        public virtual string GetCType(metadata.TypeSpec ts)
        {
            int b;
            return GetCType(ts, out b);
        }

        public virtual string GetCType(metadata.TypeSpec ts, out int bytesize)
        {
            switch(ts.stype)
            {
                case metadata.TypeSpec.SpecialType.None:
                    switch(ts.SimpleType)
                    {
                        case 0x02:
                        case 0x08:
                            bytesize = 4;
                            return "int32_t";
                        case 0x03:
                        case 0x06:
                            bytesize = 2;
                            return "int16_t";
                        case 0x04:
                            bytesize = 1;
                            return "int8_t";
                        case 0x05:
                            bytesize = 1;
                            return "uint8_t";
                        case 0x07:
                            bytesize = 2;
                            return "uint16_t";
                        case 0x09:
                            bytesize = 4;
                            return "uint32_t";
                        case 0x0a:
                            bytesize = 8;
                            return "int64_t";
                        case 0x0b:
                            bytesize = 8;
                            return "uint64_t";
                        case 0x0c:
                            bytesize = 4;
                            return "float";
                        case 0x0d:
                            bytesize = 8;
                            return "double";
                        case 0x18:
                            bytesize = GetPointerSize();
                            return "INTPTR";
                        case 0x19:
                            bytesize = GetPointerSize();
                            return "UINTPTR";
                    }
                    break;
            }
            bytesize = GetPointerSize();
            return "INTPTR";
        }

        protected internal virtual bool HasSideEffects(MCInst i)
        { return IsCall(i); }

        public binary_library.IBinaryFile bf;
        public binary_library.ISection text_section;
        public StringTable st;
        public SignatureTable sigt;
        public Requestor r;

        protected internal virtual bool NeedsMregLiveness(MCInst i)
        {
            if (i.p.Length == 0)
                return false;
            if (i.p[0].t == ir.Opcode.vl_str)
            {
                switch (i.p[0].v)
                {
                    case Generic.g_precall:
                    case Generic.g_postcall:
                        return true;
                }
            }
            return false;
        }

        protected internal abstract MCInst SaveRegister(Reg r);
        protected internal abstract MCInst RestoreRegister(Reg r);

        public string name;
        public int ptype;
        protected internal int psize;
        public Trie<InstructionHandler> instrs = new Trie<InstructionHandler>();
        public Reg[] regs;

        /** <summary>Return a hardware register that can be used for
                a particular stack location, or -1 if the actual
                stack should be used.</summary>*/
        public virtual int GetRegStackLoc(int stack_loc, int ct) { return -1; }

        static Target()
        {
            init_targets();
            init_pt();
            init_rtmap();
        }

        public int RationaliseCT(int ct)
        {
            switch (ct)
            {
                case Opcode.ct_intptr:
                case Opcode.ct_object:
                case Opcode.ct_ref:
                    return ptype;
                default:
                    return ct;
            }
        }

        public class DoubleReg : Reg, IEquatable<Reg>
        {
            public Reg a, b;

            public DoubleReg(Reg _a, Reg _b)
            {
                type = rt_multi;
                a = _a;
                b = _b;
                size = a.size + b.size;

                mask = a.mask | b.mask;
            }

            public override string ToString()
            {
                return a.ToString() + ":" + b.ToString();
            }

            public override bool Equals(Reg other)
            {
                var dr = other as DoubleReg;
                if (dr == null)
                    return false;
                if (a.Equals(dr.a) == false)
                    return false;
                return b.Equals(dr.b);
            }

            public override Reg SubReg(int sroffset, int srsize)
            {
                if (sroffset == 0)
                    return a;
                else if (sroffset == a.size)
                    return b;
                else
                    throw new NotSupportedException();
            }
        }

        public class ContentsReg : Reg, IEquatable<Reg>
        {
            public Reg basereg;
            public long disp;

            public ContentsReg()
            {
                type = rt_contents;
            }

            public override string ToString()
            {
                if (disp == 0)
                    return "[" + basereg.ToString() + "]";
                else if (disp > 0)
                    return "[" + basereg.ToString() + " + " + disp.ToString() + "]";
                else
                    return "[" + basereg.ToString() + " - " + (-disp).ToString() + "]";
            }

            public override bool Equals(Reg other)
            {
                var cr = other as ContentsReg;
                if (cr == null)
                    return false;
                if (basereg.Equals(cr.basereg) == false)
                    return false;
                return disp == cr.disp;
            }

            public override Reg SubReg(int sroffset, int srsize)
            {
                if (sroffset + srsize > size)
                    throw new NotSupportedException();

                return new ContentsReg
                {
                    basereg = basereg,
                    disp = disp + sroffset,
                    size = srsize
                };
            }
        }

        public class AddrAndContentsReg : Reg, IEquatable<Reg>
        {
            public Reg Addr, Contents;

            public override bool Equals(Reg other)
            {
                var acr = other as AddrAndContentsReg;
                if (acr == null)
                    return false;
                if (Addr.Equals(acr.Addr) == false)
                    return false;
                return Contents.Equals(acr.Contents);
            }
        }

        public class Reg : IEquatable<Reg>
        {
            public string name;
            public int id;
            public int type;
            public int size;
            public ulong mask;
            public int stack_loc;

            public virtual Reg SubReg(int sroffset, int srsize)
            {
                throw new NotSupportedException();
            }

            public override string ToString()
            {
                if (name == "stack")
                    return "stack(" + stack_loc.ToString() + ")";
                else
                    return name;
            }

            public virtual bool Equals(Reg other)
            {
                if (other.type != type)
                    return false;
                if (other.id != id)
                    return false;
                if (other.stack_loc != stack_loc)
                    return false;
                return true;
            }

            public static implicit operator Param(Reg r)
            {
                return new Param { t = Opcode.vl_mreg, mreg = r };
            }
        }

        protected virtual Reg GetRegLoc(ir.Param csite,
            ref int stack_loc,
            int cc_next,
            int ct,
            metadata.TypeSpec ts,
            string cc)
        {
            throw new NotSupportedException("Architecture does not support ct: " + Opcode.ct_names[ct]);
        }

        internal static List<MCInst> handle_mclabel(
           Target t,
           List<cil.CilNode.IRNode> nodes,
           int start, int count, Code c)
        {
            var n = nodes[start];

            List<MCInst> r = new List<MCInst>();
            r.Add(new MCInst
            {
                parent = n,
                p = new Param[]
            {
                new Param { t = ir.Opcode.vl_str, v = Generic.g_mclabel, str = "mclabel" },
                new Param { t = ir.Opcode.vl_br_target, v = n.imm_l }
            }
            });

            return r;
        }

        internal int GetSize(metadata.TypeSpec ts)
        {
            switch (ts.stype)
            {
                case metadata.TypeSpec.SpecialType.None:
                    if (ts.m.is_corlib)
                    {
                        var simple = ts.m.simple_type_idx[ts.tdrow];
                        if (simple != -1)
                            return GetSTypeSize(simple);
                    }
                    if (ts.IsValueType)
                    {
                        return layout.Layout.GetTypeSize(ts, this, false);
                    }
                    return GetCTSize(Opcode.ct_object);

                default:
                    return GetCTSize(Opcode.ct_object);
            }
        }

        private int GetSTypeSize(int stype)
        {
            switch (stype)
            {
                case 0x02:
                    return 1;
                case 0x03:
                    return 2;
                case 0x04:
                case 0x05:
                    return 1;
                case 0x06:
                case 0x07:
                    return 2;
                case 0x08:
                case 0x09:
                    return 4;
                case 0x0a:
                case 0x0b:
                    return 8;
                case 0x0c:
                    return 4;
                case 0x0d:
                    return 8;
                case 0x0e:
                case 0x16:
                case 0x1c:
                case 0x18:
                case 0x19:
                case 0x1b:
                case 0x11:
                    return GetPointerSize();
                default:
                    throw new NotSupportedException();
            }
        }

        internal int GetSize(int ptype, uint token)
        {
            throw new NotSupportedException();
            if (ptype == 0x11)
                throw new NotImplementedException();
            else
            {
                return GetCTSize(ir.Opcode.GetCTFromType(ptype));
            }
        }

        public virtual int GetPointerSize()
        {
            return GetCTSize(ptype);
        }

        internal int GetCTSize(int ct)
        {
            switch (ct)
            {
                case ir.Opcode.ct_int32:
                    return 4;
                case ir.Opcode.ct_int64:
                    return 8;
                case ir.Opcode.ct_intptr:
                case ir.Opcode.ct_object:
                case ir.Opcode.ct_ref:
                    return GetCTSize(ptype);
                case ir.Opcode.ct_float:
                    return 8;
                default:
                    throw new NotSupportedException();
            }
        }

        protected internal virtual Reg[] GetRegLocs(ir.Param csite,
            ref int stack_loc,
            Dictionary<int, int[]> cc,
            Dictionary<int, int> cc_classmap,
            string cc_name,
            out int[] la_sizes,
            out metadata.TypeSpec[] la_types,
            metadata.TypeSpec has_hidden = null)
        {
            var m = csite.m;
            var idx = (int)csite.v2;
            metadata.TypeSpec[] gtparams = null;
            metadata.TypeSpec[] gmparams = null;
            if (csite.ms != null)
            {
                idx = csite.ms.msig;
                m = csite.ms.m;
                gtparams = csite.ms.gtparams;
                gmparams = csite.ms.gmparams;
            }
            Dictionary<int, int> cc_next = new Dictionary<int, int>(
                new GenericEqualityComparer<int>());

            var act_pcount = m.GetMethodDefSigParamCount(idx);
            var pcount = act_pcount;
            bool has_this = m.GetMethodDefSigHasNonExplicitThis(idx);
            idx = m.GetMethodDefSigRetTypeIndex(idx);

            if (has_this)
                pcount++;
            if (has_hidden != null)
                pcount++;

            // Skip rettype
            bool is_req;
            uint token;
            while (m.GetRetTypeCustomMod(ref idx, out is_req, out token)) ;
            m.GetTypeSpec(ref idx, gtparams, gmparams);

            // Read types of parameters
            Target.Reg[] ret = new Target.Reg[pcount];
            la_sizes = new int[pcount];
            la_types = new metadata.TypeSpec[pcount];

            List<metadata.TypeSpec> la_locs = new List<metadata.TypeSpec>();
            if (has_hidden != null)
                la_locs.Add(has_hidden);
            if(has_this)
            {
                // value type methods have mptr to type as their this pointer
                if (csite.ms.type.IsValueType)
                {
                    la_locs.Add(csite.ms.type.ManagedPointer);
                }
                else
                    la_locs.Add(csite.ms.type);
            }
            for(int i = 0; i < act_pcount; i++)
            {
                while (m.GetRetTypeCustomMod(ref idx, out is_req, out token)) ;
                la_locs.Add(m.GetTypeSpec(ref idx, gtparams, gmparams));
            }


            for (int i = 0; i < pcount; i++)
            {
                metadata.TypeSpec v = la_locs[i];

                la_types[i] = v;
                var size = GetSize(v);
                la_sizes[i] = size;

                var ct = GetCTFromTypeForCC(v); ir.Opcode.GetCTFromType(v);
                if (cc_classmap.ContainsKey(ct))
                    ct = cc_classmap[ct];
                else
                    ct = GetCCClassFromCT(ct, size, v, cc_name);

                Reg r = null;

                int cur_cc_next;
                if (cc_next.TryGetValue(ct, out cur_cc_next) == false)
                    cur_cc_next = 0;

                int[] cc_map;
                if (cc.TryGetValue(ct, out cc_map))
                {
                    if (cur_cc_next >= cc_map.Length)
                        cur_cc_next = cc_map.Length - 1;

                    var reg_id = cc_map[cur_cc_next];
                    if (regs[reg_id].type == rt_stack)
                    {
                        Reg rstack = new Reg()
                        {
                            type = rt_stack,
                            id = regs[reg_id].id,
                            size = size,
                            stack_loc = stack_loc,
                            name = regs[reg_id].name,
                            mask = regs[reg_id].mask
                        };
                        stack_loc += size;

                        var diff = stack_loc % size;
                        if (diff != 0)
                            stack_loc = stack_loc + size - diff;
                        r = rstack;
                    }
                    else
                        r = regs[reg_id];

                }
                else
                {
                    r = GetRegLoc(csite, ref stack_loc,
                        cur_cc_next, ct, v, cc_name);

                }
                cc_next[ct] = cur_cc_next + 1;

                ret[i] = r;
            }

            return ret;
        }

        public virtual int GetCCClassFromCT(int ct, int size, metadata.TypeSpec ts, string cc)
        {
            return ct;
        }

        public virtual bool IsLSB { get { return true; } }

        public virtual byte[] IntPtrArray(byte[] v)
        {
            var ptr_size = GetPointerSize();
            var isize = v.Length;
            var r = new byte[ptr_size];
            for (int i = 0; i < ptr_size; i++)
            {
                if (IsLSB)
                {
                    if (i < isize)
                        r[i] = v[i];
                    else
                        r[i] = 0;
                }
                else
                    throw new NotImplementedException();
            }
            return r;
        }

        public virtual bool IsTypeValid(metadata.TypeSpec ts)
        {
            if (ts == null)
                return true;

            if (ts.HasCustomAttribute("_ZN14libsupcs#2Edll8libsupcs19Bits32OnlyAttribute_7#2Ector_Rv_P1u1t") &&
                GetPointerSize() != 4)
                return false;
            if (ts.HasCustomAttribute("_ZN14libsupcs#2Edll8libsupcs19Bits64OnlyAttribute_7#2Ector_Rv_P1u1t") &&
                GetPointerSize() != 8)
                return false;

            bool is_arch_dependent = false;
            bool is_required_arch = false;
            foreach (var idx in ts.CustomAttributes("_ZN14libsupcs#2Edll8libsupcs22ArchDependentAttribute_7#2Ector_Rv_P2u1tu1S"))
            {
                var sig_idx = ts.m.GetCustomAttrSigIdx(idx);
                var arch = ts.m.ReadCustomAttrString(ref sig_idx);
                is_arch_dependent = true;
                if (arch.Equals(name))
                    is_required_arch = true;
            }

            if (is_arch_dependent && !is_required_arch)
                return false;

            return true;
        }

        public virtual bool IsMethodValid(metadata.MethodSpec ms)
        {
            if (!IsTypeValid(ms.type))
                return false;
            if (ms.HasCustomAttribute("_ZN14libsupcs#2Edll8libsupcs19Bits32OnlyAttribute_7#2Ector_Rv_P1u1t") &&
                GetPointerSize() != 4)
                return false;
            if (ms.HasCustomAttribute("_ZN14libsupcs#2Edll8libsupcs19Bits64OnlyAttribute_7#2Ector_Rv_P1u1t") &&
                GetPointerSize() != 8)
                return false;

            bool is_arch_dependent = false;
            bool is_required_arch = false;
            foreach (var idx in ms.CustomAttributes("_ZN14libsupcs#2Edll8libsupcs22ArchDependentAttribute_7#2Ector_Rv_P2u1tu1S"))
            {
                var sig_idx = ms.m.GetCustomAttrSigIdx(idx);
                var arch = ms.m.ReadCustomAttrString(ref sig_idx);
                is_arch_dependent = true;
                if (arch.Equals(name))
                    is_required_arch = true;
            }

            if (is_arch_dependent && !is_required_arch)
                return false;

            return true;
        }

        public virtual Reg AllocateValueType(Code c, metadata.TypeSpec ts, ref long alloced, ref int cur_stack)
        {
            return AllocateStackLocation(c, GetSize(ts), ref cur_stack);
        }

        public abstract Reg AllocateStackLocation(Code c, int size, ref int cur_stack);
    }
}
