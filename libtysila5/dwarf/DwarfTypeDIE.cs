using System;
using System.Collections.Generic;
using System.Text;

namespace libtysila5.dwarf
{
    public class DwarfTypeDIE : DwarfParentDIE
    {
        public metadata.TypeSpec ts { get; set; }
        public override void WriteToOutput(DwarfSections ds, IList<byte> d)
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
                        WriteBaseType(ts.SimpleType, ds, d);
                    }
                    else if(ts.IsValueType && (ts.m == dcu.m))
                    {
                        if (ts.m == dcu.m)
                        {
                            // structure_type
                            d.Add(14);
                            w(d, ts.Name, ds.smap);
                            w(d, (uint)t.GetSize(ts));

                            base.WriteToOutput(ds, d);
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
                            d.Add(13);
                            w(d, ts.Name, ds.smap);
                            w(d, (uint)t.GetSize(ts));

                            base.WriteToOutput(ds, d);
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

        private void WriteBaseType(int st, DwarfSections ds, IList<byte> d)
        {
            switch(st)
            {
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

                case 0x0e:
                    // ValueType
                    // class_type
                    d.Add(13);
                    w(d, "String", ds.smap);
                    w(d, 0);        // size - TODO
                    base.WriteToOutput(ds, d);

                    break;

                case 0x11:
                    // ValueType
                    // class_type
                    d.Add(13);
                    w(d, "ValueType", ds.smap);
                    w(d, (uint)t.GetSize(ts));
                    base.WriteToOutput(ds, d);

                    break;

                case 0x18:
                    // IntPtr
                    d.Add(15);
                    w(d, "IntPtr", ds.smap);
                    d.Add((byte)t.psize);
                    d.Add(0x05);    // signed
                    break;

                case 0x1c:
                    // Object
                    // class_type
                    d.Add(13);
                    w(d, "Object", ds.smap);
                    w(d, (uint)t.GetSize(ts));
                    base.WriteToOutput(ds, d);

                    break;



                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class DwarfMemberDIE : DwarfDIE
    {
        public string Name { get; set; }
        public int FieldOffset { get; set; }
        public DwarfTypeDIE FieldType { get; set; }

        public override void WriteToOutput(DwarfSections ds, IList<byte> d)
        {
            d.Add(18);
            w(d, Name, ds.smap);
            dcu.fmap[d.Count] = FieldType;
            for (int i = 0; i < 4; i++)
                d.Add(0);
            w(d, (uint)FieldOffset);
        }
    }
}
