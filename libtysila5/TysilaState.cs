using System;
using System.Collections.Generic;
using System.Text;

namespace libtysila5
{
    public class TysilaState
    {
        public binary_library.IBinaryFile bf;
        public binary_library.ISection text_section;
        public StringTable st;
        public SignatureTable sigt;
        public Requestor r;
    }
}
