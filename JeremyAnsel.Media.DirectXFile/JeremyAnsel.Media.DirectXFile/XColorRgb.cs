using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace JeremyAnsel.Media.DirectXFile
{
    public sealed class XColorRgb
    {
        public float Red { get; set; }

        public float Green { get; set; }

        public float Blue { get; set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "({0}, {1}, {2})", this.Red, this.Green, this.Blue);
        }
    }
}
