using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace JeremyAnsel.Media.DirectXFile
{
    internal sealed class XFileTextReader : IXFileReader
    {
        private readonly StreamReader reader;
        private XToken token;
        private bool isNextTokenRead;
        private readonly bool useDouble;

        private string currentString;
        private bool isInStringReading;

        public XFileTextReader(StreamReader reader, bool useDouble)
        {
            this.reader = reader;
            this.useDouble = useDouble;
        }

        public XToken GetCurrentToken()
        {
            return this.token;
        }

        public XToken GetNextToken()
        {
            if (this.isNextTokenRead)
            {
                return this.token;
            }

            try
            {
                XToken token = this.ParseToken();
                this.token = token;
                this.isNextTokenRead = true;
                return token;
            }
            catch (EndOfStreamException)
            {
                return (XToken)(-1);
            }
        }

        public XToken ReadToken()
        {
            if (this.isNextTokenRead)
            {
                this.isNextTokenRead = false;
                return this.token;
            }

            try
            {
                XToken token = this.ParseToken();
                this.token = token;
                return token;
            }
            catch (EndOfStreamException)
            {
                return (XToken)(-1);
            }
        }

        public string ReadName()
        {
            return this.currentString;
        }

        public string ReadString()
        {
            string str = this.ParseString();
            return str;
        }

        public int ReadInteger()
        {
            throw new NotImplementedException();
        }

        public uint ReadUnsignedInteger()
        {
            throw new NotImplementedException();
        }

        public float ReadFloat()
        {
            throw new NotImplementedException();
        }

        public Guid ReadGuid()
        {
            string str = this.ParseString();
            string end = this.ParseString();

            if (end != ">")
            {
                throw new InvalidDataException();
            }

            return Guid.Parse(str);
        }

        public void ClearList()
        {
        }

        public int IntegerListCount()
        {
            return -1;
        }

        public int FloatListCount()
        {
            return -1;
        }

        public int ReadIntegerFromList()
        {
            if (this.ReadToken() != XToken.Name)
            {
                throw new InvalidDataException();
            }

            string value = this.currentString;
            XToken token = this.ReadToken();

            if (token != XToken.Semicolon && token != XToken.Comma)
            {
                throw new InvalidDataException();
            }

            token = this.GetNextToken();

            if (token == XToken.Semicolon || token == XToken.Comma)
            {
                this.ReadToken();
            }

            return int.Parse(value, CultureInfo.InvariantCulture);
        }

        public uint ReadUnsignedIntegerFromList()
        {
            if (this.ReadToken() != XToken.Name)
            {
                throw new InvalidDataException();
            }

            string value = this.currentString;
            XToken token = this.ReadToken();

            if (token != XToken.Semicolon && token != XToken.Comma)
            {
                throw new InvalidDataException();
            }

            token = this.GetNextToken();

            if (token == XToken.Semicolon || token == XToken.Comma)
            {
                this.ReadToken();
            }

            return uint.Parse(value, CultureInfo.InvariantCulture);
        }

        public float ReadFloatFromList()
        {
            if (this.ReadToken() != XToken.Name)
            {
                throw new InvalidDataException();
            }

            string value = this.currentString;
            XToken token = this.ReadToken();

            if (token != XToken.Semicolon && token != XToken.Comma)
            {
                throw new InvalidDataException();
            }

            token = this.GetNextToken();

            if (token == XToken.Semicolon || token == XToken.Comma)
            {
                this.ReadToken();
            }

            if (this.useDouble)
            {
                return (float)double.Parse(value, CultureInfo.InvariantCulture);
            }
            else
            {
                return float.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        private XToken ParseToken()
        {
            string str = this.ParseString();

            if (string.IsNullOrEmpty(str))
            {
                return XToken.None;
            }

            switch (str)
            {
                case ",":
                    return XToken.Comma;

                case ";":
                    return XToken.Semicolon;

                case "(":
                    return XToken.OpenedParen;

                case ")":
                    return XToken.ClosedParen;

                case "{":
                    return XToken.OpenedBrace;

                case "}":
                    return XToken.ClosedBrace;

                case "[":
                    return XToken.OpenedBracket;

                case "]":
                    return XToken.ClosedBracket;

                case "\"":
                    return XToken.String;

                case "<":
                    return XToken.Guid;

                case "template":
                    return XToken.Template;

                case "array":
                    return XToken.Array;

                case "WORD":
                    return XToken.UnsignedWord;

                case "DWORD":
                    return XToken.UnsignedDword;

                case "FLOAT":
                    return XToken.Float;

                case "STRING":
                    return XToken.String;
            }

            this.currentString = str;
            return XToken.Name;
        }

        private void ParseWhiteSpace()
        {
            int c;
            while ((c = this.reader.Peek()) != -1)
            {
                if ((char)c == '#' || (char)c == '/')
                {
                    this.reader.ReadLine();
                }
                else if (char.IsWhiteSpace((char)c))
                {
                    this.reader.Read();
                }
                else
                {
                    break;
                }
            }
        }

        private string ParseString()
        {
            this.ParseWhiteSpace();

            var sb = new StringBuilder();
            int c = this.reader.Peek();

            if (c == -1)
            {
                return string.Empty;
            }

            switch ((char)c)
            {
                case ',':
                case ';':
                case '(':
                case ')':
                case '{':
                case '}':
                case '[':
                case ']':
                case '<':
                case '>':
                    c = this.reader.Read();
                    return new string((char)c, 1);

                case '"':
                    if (!this.isInStringReading)
                    {
                        this.isInStringReading = true;
                        c = this.reader.Read();
                        return new string((char)c, 1);
                    }

                    break;
            }

            while ((c = this.reader.Peek()) != -1)
            {
                if (this.isInStringReading)
                {
                    switch (c)
                    {
                        case '"':
                            this.reader.Read();
                            this.isInStringReading = false;
                            return sb.ToString();
                    }
                }
                else
                {
                    if (char.IsWhiteSpace((char)c))
                    {
                        break;
                    }

                    switch (c)
                    {
                        case '#':
                        case '/':
                        case ',':
                        case ';':
                        case '(':
                        case ')':
                        case '{':
                        case '}':
                        case '[':
                        case ']':
                        case '<':
                        case '>':
                        case '"':
                            return sb.ToString();
                    }
                }

                c = this.reader.Read();
                sb.Append((char)c);
            }

            return sb.ToString();
        }
    }
}
