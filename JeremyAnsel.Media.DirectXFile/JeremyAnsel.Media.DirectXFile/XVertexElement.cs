using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace JeremyAnsel.Media.DirectXFile
{
    public sealed class XVertexElement
    {
        public XVertexElementDataType DataType { get; set; }

        public XVertexElementMethod Method { get; set; }

        public XVertexElementUsage Usage { get; set; }

        public int UsageIndex { get; set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}", this.DataType, this.Method, this.Usage, this.UsageIndex);
        }
    }
}
