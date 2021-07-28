using System;
using System.Collections.Generic;
using System.Text;

namespace libtysila5.dwarf
{
    public class DwarfNSDIE : DwarfParentDIE
    {
        public string ns { get; set; }

        public override void WriteToOutput(DwarfSections ds, IList<byte> d)
        {
            d.Add((byte)12);
            w(d, ns, ds.smap);

            base.WriteToOutput(ds, d);
        }
    }
}
