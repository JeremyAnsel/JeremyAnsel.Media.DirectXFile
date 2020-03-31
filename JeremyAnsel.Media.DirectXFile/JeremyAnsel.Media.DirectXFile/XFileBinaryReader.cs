using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JeremyAnsel.Media.DirectXFile
{
    internal sealed class XFileBinaryReader : IXFileReader
    {
        private readonly BinaryReader reader;
        private XToken token;
        private bool isNextTokenRead;
        private readonly bool useDouble;

        private List<int> intList;
        private List<float> floatList;
        private int listIndex;

        public XFileBinaryReader(BinaryReader reader, bool useDouble)
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
                XToken token = (XToken)this.reader.ReadInt16();
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
                XToken token = (XToken)this.reader.ReadInt16();
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
            int count = this.ReadInteger();
            return new string(this.reader.ReadChars(count));
        }

        public string ReadString()
        {
            int count = this.ReadInteger();
            return new string(this.reader.ReadChars(count));
        }

        public int ReadInteger()
        {
            if (this.isNextTokenRead)
            {
                byte[] b = new byte[4];
                for (int i = 0; i < 2; i++)
                {
                    BitConverter.GetBytes((ushort)this.ReadToken())
                        .CopyTo(b, i * 2);
                }

                return BitConverter.ToInt32(b, 0);
            }
            else
            {
                return this.reader.ReadInt32();
            }
        }

        public uint ReadUnsignedInteger()
        {
            if (this.isNextTokenRead)
            {
                byte[] b = new byte[4];
                for (int i = 0; i < 2; i++)
                {
                    BitConverter.GetBytes((ushort)this.ReadToken())
                        .CopyTo(b, i * 2);
                }

                return BitConverter.ToUInt32(b, 0);
            }
            else
            {
                return this.reader.ReadUInt32();
            }
        }

        public float ReadFloat()
        {
            if (this.isNextTokenRead)
            {
                if (this.useDouble)
                {
                    byte[] b = new byte[8];
                    for (int i = 0; i < 4; i++)
                    {
                        BitConverter.GetBytes((ushort)this.ReadToken())
                            .CopyTo(b, i * 2);
                    }

                    return (float)BitConverter.ToDouble(b, 0);
                }
                else
                {
                    byte[] b = new byte[4];
                    for (int i = 0; i < 2; i++)
                    {
                        BitConverter.GetBytes((ushort)this.ReadToken())
                            .CopyTo(b, i * 2);
                    }

                    return BitConverter.ToSingle(b, 0);
                }
            }
            else
            {
                if (this.useDouble)
                {
                    return (float)this.reader.ReadDouble();
                }
                else
                {
                    return this.reader.ReadSingle();
                }
            }
        }

        public Guid ReadGuid()
        {
            if (this.isNextTokenRead)
            {
                byte[] b = new byte[16];
                for (int i = 0; i < 8; i++)
                {
                    BitConverter.GetBytes((ushort)this.ReadToken())
                        .CopyTo(b, i * 2);
                }

                return new Guid(b);
            }
            else
            {
                return new Guid(this.reader.ReadBytes(16));
            }
        }

        public void ClearList()
        {
            this.intList = null;
            this.floatList = null;
            this.listIndex = 0;
        }

        public int IntegerListCount()
        {
            if (this.intList == null)
            {
                return -1;
            }

            return this.intList.Count;
        }

        public int FloatListCount()
        {
            if (this.floatList == null)
            {
                return -1;
            }

            return this.floatList.Count;
        }

        private List<int> ReadIntegerList()
        {
            XToken token = this.ReadToken();

            if (token != XToken.IntegerList && token != XToken.None)
            {
                throw new InvalidDataException();
            }

            int count = this.reader.ReadInt32();
            var list = new List<int>(count);

            for (int i = 0; i < count; i++)
            {
                list.Add(this.reader.ReadInt32());
            }

            return list;
        }

        public int ReadIntegerFromList()
        {
            if (this.floatList != null)
            {
                //throw new InvalidDataException();
                this.floatList = null;
                this.listIndex = 0;
            }

            if (this.listIndex == 0)
            {
                if (this.intList != null)
                {
                    throw new InvalidDataException();
                }

                this.intList = this.ReadIntegerList();
            }

            int value;

            if (this.intList.Count == 0)
            {
                value = this.ReadInteger();
            }
            else
            {
                value = this.intList[this.listIndex++];
            }

            if (this.listIndex == this.intList.Count)
            {
                this.intList = null;
                this.listIndex = 0;
            }

            return value;
        }

        public uint ReadUnsignedIntegerFromList()
        {
            return (uint)this.ReadIntegerFromList();
        }

        private List<float> ReadFloatList()
        {
            if (this.ReadToken() != XToken.FloatList && this.token != XToken.None)
            {
                throw new InvalidDataException();
            }

            int count = this.reader.ReadInt32();
            var list = new List<float>(count);

            for (int i = 0; i < count; i++)
            {
                float value = this.ReadFloat();
                list.Add(value);
            }

            return list;
        }

        public float ReadFloatFromList()
        {
            if (this.intList != null)
            {
                //throw new InvalidDataException();
                this.intList = null;
                this.listIndex = 0;
            }

            if (this.listIndex == 0)
            {
                if (this.floatList != null)
                {
                    throw new InvalidDataException();
                }

                this.floatList = this.ReadFloatList();
            }

            float value = this.floatList[this.listIndex++];

            if (this.listIndex == this.floatList.Count)
            {
                this.floatList = null;
                this.listIndex = 0;
            }

            return value;
        }

        public string ReadRawString(int count)
        {
            byte[] bytes = this.reader.ReadBytes(count);

            if (bytes.Length != count)
            {
                throw new EndOfStreamException();
            }

            return Encoding.ASCII.GetString(bytes);
        }

        public string ReadNullTerminatedString()
        {
            byte b3 = this.reader.ReadByte();
            byte b2 = this.reader.ReadByte();
            byte b1;

            var c = new List<byte>();

            while ((b1 = this.reader.ReadByte()) != 0 || b2 > 63)
            {
                c.Add(b3);

                b3 = b2;
                b2 = b1;
            }

            c.Add(b3);
            string s = Encoding.ASCII.GetString(c.ToArray());

            this.token = (XToken)b2;
            this.isNextTokenRead = true;

            return s;
        }
    }
}
