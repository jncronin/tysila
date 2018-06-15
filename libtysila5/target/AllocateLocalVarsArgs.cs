using System;
using System.Collections.Generic;
using System.Text;

namespace libtysila5.target
{
    partial class Target
    {
        public void AllocateLocalVarsArgs(Code c)
        {
            /* Generate list of locations of local vars */
            var m = c.ms.m;
            var ms = c.ms;
            int idx = c.lvar_sig_tok;
            int lv_count = m.GetLocalVarCount(ref idx);

            int[] lv_locs = new int[lv_count];
            c.lv_locs = new Reg[lv_count];
            c.lv_sizes = new int[lv_count];
            c.lv_types = new metadata.TypeSpec[lv_count];
            int cur_loc = GetCCStackReserve(c.ms.CallingConvention);
            for (int i = 0; i < lv_count; i++)
            {
                var type = m.GetTypeSpec(ref idx, c.ms.gtparams,
                    c.ms.gmparams);
                int t_size = GetSize(type);
                lv_locs[i] = cur_loc;

                t_size = util.util.align(t_size, GetPointerSize());
                c.lv_sizes[i] = t_size;

                c.lv_types[i] = type;
                c.lv_locs[i] = GetLVLocation(cur_loc, t_size, c);

                cur_loc += t_size;

                // align to pointer size
                int diff = cur_loc % GetPointerSize();
                if (diff != 0)
                    cur_loc = cur_loc - diff + GetPointerSize();
            }

            /* Do the same for local args */
            int la_count = m.GetMethodDefSigParamCountIncludeThis(
                c.ms.msig);
            int[] la_locs = new int[la_count];
            c.la_locs = new Reg[la_count];
            c.la_needs_assign = new bool[la_count];
            int la_count2 = m.GetMethodDefSigParamCount(
                c.ms.msig);
            int laidx = 0;
            //cur_loc = 0;

            var cc = cc_map[c.ms.CallingConvention];
            var cc_class_map = cc_classmap[c.ms.CallingConvention];
            bool has_hidden = ir.Opcode.GetCTFromType(c.ret_ts) == ir.Opcode.ct_vt;
            int stack_loc = 0;
            metadata.TypeSpec hidden_ts = null;
            if (has_hidden)
                hidden_ts = m.SystemIntPtr;
            var la_phys_locs = GetRegLocs(new ir.Param
            {
                m = m,
                ms = c.ms,
            }, ref stack_loc, cc, cc_class_map,
            c.ms.CallingConvention,
            out c.la_sizes, out c.la_types,
            hidden_ts);

            if(has_hidden)
            {
                // Strip hidden argument off the values we return
                Reg[] la_phys_locs_new = new Reg[la_phys_locs.Length - 1];
                for (int i = 0; i < la_phys_locs_new.Length; i++)
                    la_phys_locs_new[i] = la_phys_locs[i + 1];
                la_phys_locs = la_phys_locs_new;

                int[] la_sizes = new int[la_phys_locs.Length];
                for (int i = 0; i < la_phys_locs_new.Length; i++)
                    la_sizes[i] = c.la_sizes[i + 1];
                c.la_sizes = la_sizes;

                metadata.TypeSpec[] la_types = new metadata.TypeSpec[la_phys_locs.Length];
                for (int i = 0; i < la_phys_locs_new.Length; i++)
                    la_types[i] = c.la_types[i + 1];
                c.la_types = la_types;
            }
            c.incoming_args = la_phys_locs;

            if (la_count != la_count2)
            {
                var this_size = GetCTSize(ir.Opcode.ct_object);
                c.la_locs[laidx] = GetLALocation(cur_loc, this_size, c);
                c.la_sizes[laidx] = this_size;

                // value type methods have mptr to type as their this pointer
                if (ms.type.IsValueType)
                {
                    c.la_types[laidx] = ms.type.ManagedPointer;
                }
                else
                    c.la_types[laidx] = ms.type;

                la_locs[laidx] = cur_loc;
                //cur_loc += this_size;

                laidx++;
            }
            idx = m.GetMethodDefSigRetTypeIndex(
                ms.msig);
            // pass by rettype
            m.GetTypeSpec(ref idx, c.ms.gtparams, c.ms.gmparams);

            for (int i = 0; i < la_count; i++)
            {
                var mreg = la_phys_locs[i];
                metadata.TypeSpec type = m.SystemObject;
                if (i > 0 || !m.GetMethodDefSigHasNonExplicitThis(c.ms.msig))
                    type = m.GetTypeSpec(ref idx, c.ms.gtparams, c.ms.gmparams);

                if (mreg.type == rt_stack)
                {
                    la_phys_locs[i] = GetLALocation(mreg.stack_loc, util.util.align(c.la_sizes[i], GetPointerSize()), c);
                    c.la_locs[i] = la_phys_locs[i];
                    c.la_needs_assign[i] = false;
                }
                else if(mreg.type == rt_contents)
                {
                    c.la_locs[i] = la_phys_locs[i];
                    c.la_needs_assign[i] = false;
                }
                else
                {
                    c.la_needs_assign[i] = true;

                    var la_size = util.util.align(GetSize(type), GetPointerSize());
                    c.la_locs[i] = GetLVLocation(cur_loc, la_size, c);

                    la_locs[i] = cur_loc;
                    cur_loc += la_size;
                }
            }

            if (has_hidden)
                cur_loc += psize;
            c.lv_total_size = cur_loc;
        }

        internal virtual int GetCCStackReserve(string cc)
        {
            return 0;
        }
    }
}
