using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JeremyAnsel.Media.DirectXFile
{
    internal sealed class XFileTokenReader
    {
        public XFileTokenReader(IXFileReader fileReader)
        {
            this.FileReader = fileReader;
        }

        public IXFileReader FileReader { get; private set; }

        public void ReadAssert(XToken token)
        {
            if (this.FileReader.ReadToken() != token)
            {
                throw new InvalidDataException();
            }
        }

        public string ReadName()
        {
            if (this.FileReader.GetNextToken() == XToken.None)
            {
                this.FileReader.ReadToken();
            }
            else
            {
                this.ReadAssert(XToken.Name);
            }

            return this.FileReader.ReadName();
        }

        public string ReadString()
        {
            this.ReadAssert(XToken.String);
            string str = this.FileReader.ReadString();

            XToken token = this.FileReader.ReadToken();
            if (token != XToken.Semicolon && token != XToken.Comma)
            {
                throw new InvalidDataException();
            }

            return str;
        }

        public int ReadInteger()
        {
            this.ReadAssert(XToken.Integer);
            return this.FileReader.ReadInteger();
        }

        public Guid ReadGuid()
        {
            this.ReadAssert(XToken.Guid);
            return this.FileReader.ReadGuid();
        }

        internal int[] ReadIntegerArray(int count)
        {
            var array = new int[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = this.FileReader.ReadInteger();
            }

            return array;
        }

        internal float[] ReadFloatArray(int count)
        {
            var array = new float[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = this.FileReader.ReadFloat();
            }

            return array;
        }

        public int ReadIntegerFromList()
        {
            return this.FileReader.ReadIntegerFromList();
        }

        public uint ReadUnsignedIntegerFromList()
        {
            return this.FileReader.ReadUnsignedIntegerFromList();
        }

        public int[] ReadIntegerArrayFromList(int count)
        {
            var array = new int[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = this.ReadIntegerFromList();
            }

            return array;
        }

        public uint[] ReadUnsignedIntegerArrayFromList(int count)
        {
            var array = new uint[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = this.ReadUnsignedIntegerFromList();
            }

            return array;
        }

        public float ReadFloatFromList()
        {
            return this.FileReader.ReadFloatFromList();
        }

        public float[] ReadFloatArrayFromList(int count)
        {
            var array = new float[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = this.ReadFloatFromList();
            }

            return array;
        }

        public void ReadSeparator()
        {
            XToken token = this.FileReader.GetNextToken();

            if (token == XToken.Semicolon || token == XToken.Comma)
            {
                this.FileReader.ReadToken();
            }
        }
    }
}
