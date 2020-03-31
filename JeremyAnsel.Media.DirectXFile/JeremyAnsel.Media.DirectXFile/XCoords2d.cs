using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace JeremyAnsel.Media.DirectXFile
{
    public sealed class XCoords2d
    {
        public float U { get; set; }

        public float V { get; set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "({0}, {1})", this.U, this.V);
        }
    }
}
