using System;
using System.Collections.Generic;
using System.Text;

namespace libtysila5.dwarf
{
    /** <summary>A DIE defining a method</summary> */
    public class DwarfMethodDIE : DwarfParentDIE
    {
        public metadata.MethodSpec ms { get; set; }
        public Code cil { get; set; }
        public binary_library.ISymbol sym { get; set; }

        public int SourceFileId { get; set; }
        public int StartLine { get; set; }
        public int StartColumn { get; set; }

        public override void WriteToOutput(DwarfSections ds, IList<byte> d, DwarfDIE parent)
        {
            var ms = cil.ms;

            int abbrev = 5;
            if (ms.ReturnType != null)
                abbrev += 1;
            if (!ms.IsStatic)
            {
                abbrev += 2;
                if (ms.IsVirtual)
                    abbrev += 2;
            }

            d.Add((byte)abbrev);
            w(d, ms.Name, ds.smap);
            var low_r = ds.bf.CreateRelocation();
            low_r.Type = t.GetDataToDataReloc();
            low_r.Offset = (ulong)d.Count;
            low_r.References = ds.bf.FindSymbol(ms.MangleMethod());
            low_r.DefinedIn = ds.info;
            ds.bf.AddRelocation(low_r);
            wp(d);  // low_pc
            wp(d, low_r.References.Size);  // high_pc

            // Update first/last sym if necessary
            if (dcu.first_sym == null || low_r.References.Offset < dcu.first_sym.Offset)
                dcu.first_sym = low_r.References;
            if (dcu.last_sym == null || low_r.References.Offset > dcu.last_sym.Offset)
                dcu.last_sym = low_r.References;

            var mflags = ms.m.GetIntEntry(metadata.MetadataStream.tid_MethodDef,
                ms.mdrow, 2);
            var access = mflags & 0x07;
            if (access == 0x6)
                d.Add(0x1); // public
            else if (access >= 0x4)
                d.Add(0x2); // protected
            else
                d.Add(0x3); // private

            w(d, ms.MangleMethod(), ds.smap);

            if(ms.ReturnType != null)
            {
                dcu.fmap[d.Count] = (ms.ReturnType.stype == metadata.TypeSpec.SpecialType.None && !ms.ReturnType.IsValueType) ?
                    dcu.GetTypeDie(ms.ReturnType.Pointer) :
                    dcu.GetTypeDie(ms.ReturnType);
                
                // add return type
                for (int i = 0; i < 4; i++)
                    d.Add(0);
            }

            int fparam_ref_loc = 0;
            if(!ms.IsStatic)
            {
                // reference for first parameter
                fparam_ref_loc = d.Count;
                
                for (int i = 0; i < 4; i++)
                    d.Add(0);
            }

            if (ms.IsVirtual)
            {
                d.Add(0x1); // virtual
            }

            // Add parameters
            int sig_idx = ms.mdrow == 0 ? ms.msig :
                 (int)ms.m.GetIntEntry(metadata.MetadataStream.tid_MethodDef, ms.mdrow, 4);

            var pc_nonthis = ms.m.GetMethodDefSigParamCount(sig_idx);
            var rt_idx = ms.m.GetMethodDefSigRetTypeIndex(sig_idx);
            ms.m.GetTypeSpec(ref rt_idx, ms.gtparams, ms.gmparams);

            if (ms.m.GetMethodDefSigHasNonExplicitThis(ms.msig))
            {
                var fparam = new DwarfParamDIE();
                fparam.dcu = dcu;
                fparam.t = t;
                fparam.IsThis = true;
                fparam.ts = ms.type.Pointer;

                Children.Add(fparam);
            }

            for(int i = 0; i < pc_nonthis; i++)
            {
                var fparam = new DwarfParamDIE();
                fparam.dcu = dcu;
                fparam.t = t;
                fparam.IsThis = false;
                fparam.ts = ms.m.GetTypeSpec(ref rt_idx, ms.gtparams, ms.gmparams);

                if (fparam.ts.stype == metadata.TypeSpec.SpecialType.None &&
                    !fparam.ts.IsValueType)
                    fparam.ts = fparam.ts.Pointer;

                Children.Add(fparam);
            }

            // Get param names
            if (ms.mdrow != 0)
            {
                int param_start = (int)ms.m.GetIntEntry(metadata.MetadataStream.tid_MethodDef,
                    ms.mdrow, 5);

                int param_last_row = ms.m.GetRowCount(metadata.MetadataStream.tid_Param);
                int next_param = int.MaxValue;
                if(ms.mdrow < ms.m.GetRowCount(metadata.MetadataStream.tid_MethodDef))
                {
                    next_param = (int)ms.m.GetIntEntry(metadata.MetadataStream.tid_MethodDef,
                        ms.mdrow + 1, 5) - 1;
                }
                int param_end = param_last_row > next_param ? next_param : param_last_row;

                for(int i = param_start; i <= param_end; i++)
                {
                    var seq = ms.m.GetIntEntry(metadata.MetadataStream.tid_Param,
                        i, 1);
                    var name = ms.m.GetStringEntry(metadata.MetadataStream.tid_Param,
                        i, 2);

                    seq--;
                    if (ms.m.GetMethodDefSigHasNonExplicitThis(ms.msig))
                        seq++;

                    ((DwarfParamDIE)Children[(int)seq]).name = name;
                }
            }

            if (!ms.IsStatic)
            {
                // Patch the .this pointer to the first child
                dcu.fmap[fparam_ref_loc] = Children[0];
            }

            // Param locations
            if(cil != null && cil.la_locs != null && cil.la_locs.Length == Children.Count)
            {
                for (int i = 0; i < cil.la_locs.Length; i++)
                    ((DwarfParamDIE)Children[i]).loc = cil.la_locs[i];
            }

            // Get param names
            if (ms.mdrow != 0)
            {
                string[] pnames = new string[cil.lv_types.Length];

                if(ms.m.pdb != null)
                {
                    for(int i = 1; i < ms.m.pdb.table_rows[(int)metadata.MetadataStream.TableId.LocalScope]; i++)
                    {
                        var lv_mdrow = (int)ms.m.pdb.GetIntEntry((int)metadata.MetadataStream.TableId.LocalScope,
                            i, 0);
                        if(lv_mdrow == ms.mdrow)
                        {
                            var lv_start = (int)ms.m.pdb.GetIntEntry((int)metadata.MetadataStream.TableId.LocalScope,
                                i, 2);

                            int lv_last_row = ms.m.pdb.GetRowCount((int)metadata.MetadataStream.TableId.LocalVariable);
                            int next_lv = int.MaxValue;
                            if(i < ms.m.pdb.GetRowCount((int)metadata.MetadataStream.TableId.LocalScope))
                            {
                                next_lv = (int)ms.m.pdb.GetIntEntry((int)metadata.MetadataStream.TableId.LocalScope,
                                    i + 1, 2) - 1;
                            }
                            int lv_end = lv_last_row > next_lv ? next_lv : lv_last_row;

                            for(int j = lv_start; j <= lv_end; j++)
                            {
                                var pindex = (int)ms.m.pdb.GetIntEntry((int)metadata.MetadataStream.TableId.LocalVariable,
                                    j, 1);
                                var pname = ms.m.pdb.GetStringEntry((int)metadata.MetadataStream.TableId.LocalVariable,
                                    j, 2);

                                pnames[pindex] = pname;
                            }
                        }
                    }
                }

                for (int i = 0; i < pnames.Length; i++)
                {
                    var pname = pnames[i];
                    if (pname != null)
                    {
                        var ptype = cil.lv_types[i];
                        var ploc = cil.lv_locs[i];

                        var vparam = new DwarfVarDIE();
                        vparam.dcu = dcu;
                        vparam.t = t;
                        vparam.ts = ptype;
                        vparam.name = pname;
                        vparam.loc = ploc;

                        if (vparam.ts.stype == metadata.TypeSpec.SpecialType.None &&
                            !vparam.ts.IsValueType)
                            vparam.ts = vparam.ts.Pointer;

                        Children.Add(vparam);
                    }
                }
            }

            base.WriteToOutput(ds, d, parent);
        }
    }

    public class DwarfParamDIE : DwarfDIE
    {
        public string name { get; set; }
        public metadata.TypeSpec ts { get; set; }
        public bool IsThis { get; set; }
        public target.Target.Reg loc { get; set; }

        public override void WriteToOutput(DwarfSections ds, IList<byte> d, DwarfDIE parent)
        {
            if (IsThis)
            {
                d.Add(19);
                dcu.fmap[d.Count] = dcu.GetTypeDie(ts);
                for (int i = 0; i < 4; i++)
                    d.Add(0);
                // implicit artifical flag
            }
            else
            {
                w(d, 11);
                w(d, name, ds.smap);
                dcu.fmap[d.Count] = dcu.GetTypeDie(ts);
                for (int i = 0; i < 4; i++)
                    d.Add(0);
            }

            // location as exprloc
            var b = new List<byte>();
            if (t.AddDwarfLocation(loc, b))
            {
                DwarfDIE.w(d, (uint)b.Count);
                foreach (var c in b)
                    d.Add(c);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    public class DwarfVarDIE : DwarfDIE
    {
        public string name { get; set; }
        public metadata.TypeSpec ts { get; set; }
        public target.Target.Reg loc { get; set; }

        public override void WriteToOutput(DwarfSections ds, IList<byte> d, DwarfDIE parent)
        {
            w(d, 21);
            w(d, name, ds.smap);
            dcu.fmap[d.Count] = dcu.GetTypeDie(ts);
            for (int i = 0; i < 4; i++)
                d.Add(0);

            // location as exprloc
            var b = new List<byte>();
            if (t.AddDwarfLocation(loc, b))
            {
                DwarfDIE.w(d, (uint)b.Count);
                foreach (var c in b)
                    d.Add(c);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    public class DwarfMethodDefDIE : DwarfDIE
    {
        public DwarfMethodDIE decl { get; set; }

        public override void WriteToOutput(DwarfSections ds, IList<byte> d, DwarfDIE parent)
        {
            w(d, 22);
            dcu.fmap[d.Count] = decl;
            for (int i = 0; i < 4; i++)
                d.Add(0);

            var low_r = ds.bf.CreateRelocation();
            low_r.Type = t.GetDataToDataReloc();
            low_r.Offset = (ulong)d.Count;
            low_r.References = ds.bf.FindSymbol(decl.ms.MangleMethod());
            low_r.DefinedIn = ds.info;
            ds.bf.AddRelocation(low_r);
            wp(d);  // low_pc
            wp(d, low_r.References.Size);  // high_pc

            foreach (var child in decl.Children)
                child.WriteToOutput(ds, d, this);

            d.Add(0);
        }
    }

}
