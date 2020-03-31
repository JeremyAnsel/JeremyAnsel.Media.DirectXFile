using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JeremyAnsel.Media.DirectXFile
{
    public sealed class XAnimationKey
    {
        public string Name { get; set; }

        public XAnimationKeyType KeyType { get; set; }

        public List<Tuple<int, float[]>> Keys { get; } = new List<Tuple<int, float[]>>();

        internal static XAnimationKey Read(XFileTokenReader tokenReader)
        {
            var key = new XAnimationKey();

            if (tokenReader.FileReader.GetNextToken() == XToken.Name)
            {
                key.Name = tokenReader.ReadName();
            }

            tokenReader.ReadAssert(XToken.OpenedBrace);

            key.KeyType = (XAnimationKeyType)tokenReader.ReadIntegerFromList();

            int keysCount = tokenReader.ReadIntegerFromList();
            key.Keys.Capacity = keysCount;

            for (int i = 0; i < keysCount; i++)
            {
                int time = tokenReader.ReadIntegerFromList();
                int valuesCount = tokenReader.ReadIntegerFromList();
                float[] values = tokenReader.ReadFloatArrayFromList(valuesCount);

                tokenReader.ReadSeparator();

                key.Keys.Add(Tuple.Create(time, values));
            }

            tokenReader.ReadAssert(XToken.ClosedBrace);

            return key;
        }
    }
}
