using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JeremyAnsel.Media.DirectXFile
{
    internal interface IXFileReader
    {
        XToken GetCurrentToken();

        XToken GetNextToken();

        XToken ReadToken();

        string ReadName();

        string ReadString();

        int ReadInteger();

        uint ReadUnsignedInteger();

        float ReadFloat();

        Guid ReadGuid();

        void ClearList();

        int IntegerListCount();

        int FloatListCount();

        int ReadIntegerFromList();

        uint ReadUnsignedIntegerFromList();

        float ReadFloatFromList();
    }
}
