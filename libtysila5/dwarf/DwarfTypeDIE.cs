using System;
using System.Collections.Generic;
using System.Text;

namespace libtysila5.dwarf
{
    public class DwarfTypeDIE : DwarfParentDIE
    {
        public metadata.TypeSpec ts { get; set; }
        public string NameOverride { get; set; }
        public override void WriteToOutput(DwarfSections ds, IList<byte> d, DwarfDIE parent)
        {
            int abbrev;

            // decide upon type
            switch(ts.stype)
            {
                case metadata.TypeSpec.SpecialType.Ptr:
                case metadata.TypeSpec.SpecialType.MPtr:
                    d.Add(16);
                    d.Add((byte)t.psize);
                    dcu.fmap[d.Count] = dcu.GetTypeDie(ts.other);
                    for (int i = 0; i < 4; i++) d.Add(0);
                    break;

                case metadata.TypeSpec.SpecialType.Array:
                    throw new NotImplementedException();

                case metadata.TypeSpec.SpecialType.SzArray:
                    throw new NotImplementedException();

                case metadata.TypeSpec.SpecialType.None:
                    if(ts.SimpleType != 0)
                    {
                        // base_type
                        WriteBaseType(ts.SimpleType, ds, d, parent);
                    }
                    else if(ts.IsValueType && (ts.m == dcu.m))
                    {
                        if (ts.m == dcu.m)
                        {
                            // structure_type
                            var source_loc = GetSourceLoc();
                            if (source_loc == null)
                                d.Add(14);
                            else
                                d.Add(24);
                            w(d, ts.Name, ds.smap);
                            w(d, (uint)t.GetSize(ts));

                            if(source_loc != null)
                            {
                                d.Add((byte)source_loc.file);
                                d.Add((byte)source_loc.line);
                                d.Add((byte)source_loc.col);
                            }

                            base.WriteToOutput(ds, d, parent);
                        }
                        else
                        {
                            // structure_type external
                            throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        if (ts.m == dcu.m)
                        {
                            // class_type
                            var source_loc = GetSourceLoc();
                            if (source_loc == null)
                                d.Add(13);
                            else
                                d.Add(23);
                            w(d, ts.Name, ds.smap);
                            w(d, (uint)t.GetSize(ts));

                            if (source_loc != null)
                            {
                                d.Add((byte)source_loc.file);
                                d.Add((byte)source_loc.line);
                                d.Add((byte)source_loc.col);
                            }

                            base.WriteToOutput(ds, d, parent);
                        }
                        else
                        {
                            // class_type external
                            throw new NotImplementedException();
                        }
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        class SourceLoc
        {
            public int file, line, col;
        }

        private SourceLoc GetSourceLoc()
        {
            DwarfMethodDIE first = null, ctor = null;
            foreach(var die in Children)
            {
                if(die is DwarfMethodDIE)
                {
                    var dmdie = die as DwarfMethodDIE;
                    if (first == null && dmdie.SourceFileId != 0)
                        first = dmdie;
                    if(ctor == null && dmdie.ms != null && dmdie.ms.Name == ".ctor" && dmdie.SourceFileId != 0)
                    {
                        ctor = dmdie;
                        break;
                    }
                }
            }
            if (first == null)
                return null;
            var ret = new SourceLoc();
            if(ctor != null)
            {
                ret.file = ctor.SourceFileId;
                ret.line = ctor.StartLine;
                ret.col = ctor.StartColumn;
            }
            else
            {
                ret.file = first.SourceFileId;
                ret.line = first.StartLine;
                ret.col = first.StartColumn;
            }

            return ret;
        }

        private void WriteBaseType(int st, DwarfSections ds, IList<byte> d, DwarfDIE parent)
        {
            if (parent is DwarfNSDIE && ((DwarfNSDIE)parent).ns == "System" && dcu.basetype_dies.ContainsKey(st))
            {
                // These are typedefs to types in the global scope
                d.Add(20);
                w(d, ts.Name, ds.smap);
                dcu.fmap[d.Count] = dcu.basetype_dies[st];
                for (int i = 0; i < 4; i++) d.Add(0);

                if (st == 0x1c)
                    System.Diagnostics.Debugger.Break();
            }
            else
            {
                /* There are a few CLI basetypes that do not have C# equivalents
                 * or this is a string/object in the main namespace */
                switch (st)
                {
                    case 0x11:
                        // ValueType
                        // class_type
                        d.Add(13);
                        w(d, "ValueType", ds.smap);
                        w(d, (uint)t.GetSize(ts));
                        base.WriteToOutput(ds, d, parent);
                        break;

                    case 0x18:
                        // IntPtr
                        d.Add(20);
                        w(d, "IntPtr", ds.smap);
                        dcu.fmap[d.Count] = dcu.basetype_dies[t.psize == 4 ? 0x08 : 0x0a];
                        for (int i = 0; i < 4; i++) d.Add(0);
                        break;

                    case 0x19:
                        // IntPtr
                        d.Add(20);
                        w(d, "UIntPtr", ds.smap);
                        dcu.fmap[d.Count] = dcu.basetype_dies[t.psize == 4 ? 0x09 : 0x0b];
                        for (int i = 0; i < 4; i++) d.Add(0);
                        break;

                    case 0x0e:
                        // String
                        // class_type
                        d.Add(13);
                        w(d, NameOverride ?? "String", ds.smap);
                        w(d, 0);        // size - TODO
                        base.WriteToOutput(ds, d, parent);
                        break;

                    case 0x1c:
                        // Object
                        // class_type
                        d.Add(13);
                        w(d, NameOverride ?? "Object", ds.smap);
                        w(d, (uint)t.GetSize(ts));
                        base.WriteToOutput(ds, d, parent);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }

    public class DwarfMemberDIE : DwarfDIE
    {
        public string Name { get; set; }
        public int FieldOffset { get; set; }
        public DwarfDIE FieldType { get; set; }

        public override void WriteToOutput(DwarfSections ds, IList<byte> d, DwarfDIE parent)
        {
            d.Add(18);
            w(d, Name, ds.smap);
            dcu.fmap[d.Count] = FieldType;
            for (int i = 0; i < 4; i++)
                d.Add(0);
            w(d, (uint)FieldOffset);
        }
    }

    public class DwarfBaseTypeDIE : DwarfParentDIE
    {
        public int stype { get; set; }

        public override void WriteToOutput(DwarfSections ds, IList<byte> d, DwarfDIE parent)
        {
            switch (stype)
            {
                case 0x02:
                    // bool
                    d.Add(15);
                    w(d, "bool", ds.smap);
                    d.Add((byte)t.GetSize(dcu.m.SystemBool));
                    d.Add(0x07);    // unsigned
                    break;

                case 0x03:
                    // Char
                    d.Add(15);
                    w(d, "char", ds.smap);
                    d.Add(2);
                    d.Add(0x06);    // signed char
                    break;

                case 0x04:
                    // I1
                    d.Add(15);
                    w(d, "sbyte", ds.smap);
                    d.Add(1);
                    d.Add(0x05);    // signed
                    break;

                case 0x05:
                    // U1
                    d.Add(15);
                    w(d, "byte", ds.smap);
                    d.Add(1);
                    d.Add(0x07);    // unsigned
                    break;

                case 0x06:
                    // I2
                    d.Add(15);
                    w(d, "short", ds.smap);
                    d.Add(2);
                    d.Add(0x05);    // signed
                    break;

                case 0x07:
                    // U2
                    d.Add(15);
                    w(d, "ushort", ds.smap);
                    d.Add(2);
                    d.Add(0x07);    // unsigned
                    break;

                case 0x08:
                    // I4
                    d.Add(15);
                    w(d, "int", ds.smap);
                    d.Add(4);
                    d.Add(0x05);    // signed
                    break;

                case 0x09:
                    // U4
                    d.Add(15);
                    w(d, "uint", ds.smap);
                    d.Add(4);
                    d.Add(0x07);    // unsigned
                    break;

                case 0x0a:
                    // I8
                    d.Add(15);
                    w(d, "long", ds.smap);
                    d.Add(8);
                    d.Add(0x05);    // signed
                    break;

                case 0x0b:
                    // U8
                    d.Add(15);
                    w(d, "ulong", ds.smap);
                    d.Add(8);
                    d.Add(0x07);    // unsigned
                    break;

                case 0x0c:
                    // R4
                    d.Add(15);
                    w(d, "float", ds.smap);
                    d.Add(4);
                    d.Add(0x04);    // float
                    break;

                case 0x0d:
                    // R8
                    d.Add(15);
                    w(d, "double", ds.smap);
                    d.Add(8);
                    d.Add(0x04);    // float
                    break;
                    

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
