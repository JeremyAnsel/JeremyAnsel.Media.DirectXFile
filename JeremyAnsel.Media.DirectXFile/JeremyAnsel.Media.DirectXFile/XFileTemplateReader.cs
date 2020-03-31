using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JeremyAnsel.Media.DirectXFile
{
    internal sealed class XFileTemplateReader
    {
        private readonly XFileTokenReader tokenReader;

        public XFileTemplateReader(XFileTokenReader tokenReader)
        {
            this.tokenReader = tokenReader;
        }

        public Tuple<string, Guid> ReadTemplate()
        {
            this.tokenReader.ReadAssert(XToken.Template);
            string name = this.ReadTemplateName();
            this.tokenReader.ReadAssert(XToken.OpenedBrace);
            Guid guid = this.ReadTemplateClassId();
            this.ReadTemplateParts();
            this.tokenReader.ReadAssert(XToken.ClosedBrace);
            return Tuple.Create(name, guid);
        }

        public void ReadTemplateParts()
        {
            this.ReadTemplateMembersPart();

            XToken token = this.tokenReader.FileReader.GetNextToken();

            if (token == XToken.OpenedBracket)
            {
                this.tokenReader.ReadAssert(XToken.OpenedBracket);
                this.ReadTemplateOptionInfo();
                this.tokenReader.ReadAssert(XToken.ClosedBracket);
            }
        }

        public void ReadTemplateMembersPart()
        {
            XToken token = this.tokenReader.FileReader.GetNextToken();

            if (IsTemplateMembers(token))
            {
                this.ReadTemplateMembersList();
            }
        }

        public void ReadTemplateOptionInfo()
        {
            XToken token = this.tokenReader.FileReader.GetNextToken();

            if (token == XToken.Dot)
            {
                this.ReadTemplateEllipsis();
            }
            else
            {
                this.ReadTemplateOptionList();
            }
        }

        public void ReadTemplateMembersList()
        {
            do
            {
                this.ReadTemplateMembers();
            }
            while (IsTemplateMembers(this.tokenReader.FileReader.GetNextToken()));
        }

        public static bool IsTemplateMembers(XToken token)
        {
            return token == XToken.Name || token == XToken.Array || IsTemplatePrimitiveType(token);
        }

        public void ReadTemplateMembers()
        {
            XToken token = this.tokenReader.FileReader.GetNextToken();

            if (token == XToken.Name)
            {
                this.ReadTemplateReference();
            }
            else if (token == XToken.Array)
            {
                this.ReadTemplateArray();
            }
            else
            {
                this.ReadTemplatePrimitive();
            }
        }

        public Tuple<XToken, string> ReadTemplatePrimitive()
        {
            XToken type = this.ReadTemplatePrimitiveType();
            string name = this.ReadTemplateOptionalName();
            this.tokenReader.ReadAssert(XToken.Semicolon);
            return Tuple.Create(type, name);
        }

        public Tuple<Tuple<XToken, string>, string, List<Tuple<int, string>>> ReadTemplateArray()
        {
            this.tokenReader.ReadAssert(XToken.Array);
            Tuple<XToken, string> type = this.ReadTemplateArrayDataType();
            string name = this.ReadTemplateName();
            List<Tuple<int, string>> list = this.ReadTemplateDimensionList();
            this.tokenReader.ReadAssert(XToken.Semicolon);
            return Tuple.Create(type, name, list);
        }

        public Tuple<string, string> ReadTemplateReference()
        {
            string name = this.ReadTemplateName();
            string optionalName = this.ReadTemplateOptionalName();
            this.tokenReader.ReadAssert(XToken.Semicolon);
            return Tuple.Create(name, optionalName);
        }

        public Tuple<XToken, string> ReadTemplateArrayDataType()
        {
            XToken token = this.tokenReader.FileReader.GetNextToken();
            string dataName = string.Empty;
            XToken dataToken = XToken.None;

            if (token == XToken.Name)
            {
                dataName = this.ReadTemplateName();
            }
            else
            {
                dataToken = this.ReadTemplatePrimitiveType();
            }

            return Tuple.Create(dataToken, dataName);
        }

        public static bool IsTemplatePrimitiveType(XToken token)
        {
            switch (token)
            {
                case XToken.UnsignedWord:
                case XToken.UnsignedDword:
                case XToken.Float:
                case XToken.Double:
                case XToken.SignedChar:
                case XToken.UnsignedChar:
                case XToken.SignedWord:
                case XToken.SignedDword:
                case XToken.LPStr:
                case XToken.Unicode:
                case XToken.CString:
                case XToken.String:
                    return true;
            }

            return false;
        }

        public XToken ReadTemplatePrimitiveType()
        {
            XToken token = this.tokenReader.FileReader.ReadToken();

            if (!IsTemplatePrimitiveType(token))
            {
                throw new InvalidDataException();
            }

            return token;
        }

        public List<Tuple<int, string>> ReadTemplateDimensionList()
        {
            var dimensions = new List<Tuple<int, string>>();

            do
            {
                Tuple<int, string> dimension = this.ReadTemplateDimension();
                dimensions.Add(dimension);
            }
            while (this.tokenReader.FileReader.GetNextToken() == XToken.OpenedBracket);

            return dimensions;
        }

        public Tuple<int, string> ReadTemplateDimension()
        {
            this.tokenReader.ReadAssert(XToken.OpenedBracket);
            Tuple<int, string> dimensionSize = this.ReadTemplateDimensionSize();
            this.tokenReader.ReadAssert(XToken.ClosedBracket);
            return dimensionSize;
        }

        public Tuple<int, string> ReadTemplateDimensionSize()
        {
            XToken token = this.tokenReader.FileReader.GetNextToken();
            int size = 0;
            string name = string.Empty;

            if (token == XToken.Integer)
            {
                size = this.tokenReader.ReadInteger();
            }
            else if (token == XToken.Name)
            {
                name = this.tokenReader.ReadName();
            }
            else
            {
                throw new InvalidDataException();
            }

            return Tuple.Create(size, name);
        }

        public List<Tuple<string, Guid>> ReadTemplateOptionList()
        {
            var options = new List<Tuple<string, Guid>>();

            do
            {
                Tuple<string, Guid> option = this.ReadTemplateOptionPart();
                options.Add(option);
            }
            while (this.tokenReader.FileReader.GetNextToken() == XToken.Name);

            return options;
        }

        public Tuple<string, Guid> ReadTemplateOptionPart()
        {
            string name = this.ReadTemplateName();
            Guid guid = this.ReadTemplateOptionalClassId();
            return Tuple.Create(name, guid);
        }

        public string ReadTemplateName()
        {
            return this.tokenReader.ReadName();
        }

        public string ReadTemplateOptionalName()
        {
            XToken token = this.tokenReader.FileReader.GetNextToken();

            if (token == XToken.Name)
            {
                return this.tokenReader.ReadName();
            }
            else
            {
                return string.Empty;
            }
        }

        public Guid ReadTemplateClassId()
        {
            return this.tokenReader.ReadGuid();
        }

        public Guid ReadTemplateOptionalClassId()
        {
            XToken token = this.tokenReader.FileReader.GetNextToken();

            if (token == XToken.Guid)
            {
                return this.tokenReader.ReadGuid();
            }
            else
            {
                return Guid.Empty;
            }
        }

        public void ReadTemplateEllipsis()
        {
            this.tokenReader.ReadAssert(XToken.Dot);
            this.tokenReader.ReadAssert(XToken.Dot);
            this.tokenReader.ReadAssert(XToken.Dot);
        }
    }
}
