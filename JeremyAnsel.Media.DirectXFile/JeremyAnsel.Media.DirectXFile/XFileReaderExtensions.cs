using System;
using System.Collections.Generic;
using System.Text;

namespace JeremyAnsel.Media.DirectXFile
{
    internal static class XFileReaderExtensions
    {
        public static XToken[] ReadTokenArray(this IXFileReader reader, int count)
        {
            var array = new XToken[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = reader.ReadToken();
            }

            return array;
        }
    }
}
