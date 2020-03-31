using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JeremyAnsel.Media.DirectXFile
{
    internal enum XToken
    {
        None = 0,
        Name = 1,
        String = 2,
        Integer = 3,
        Guid = 5,
        IntegerList = 6,
        FloatList = 7,
        OpenedBrace = 10,
        ClosedBrace = 11,
        OpenedParen = 12,
        ClosedParen = 13,
        OpenedBracket = 14,
        ClosedBracket = 15,
        OpenedAngle = 16,
        ClosedAngle = 17,
        Dot = 18,
        Comma = 19,
        Semicolon = 20,
        Template = 31,
        UnsignedWord = 40,
        UnsignedDword = 41,
        Float = 42,
        Double = 43,
        SignedChar = 44,
        UnsignedChar = 45,
        SignedWord = 46,
        SignedDword = 47,
        Void = 48,
        LPStr = 49,
        Unicode = 50,
        CString = 51,
        Array = 52,
    }
}
