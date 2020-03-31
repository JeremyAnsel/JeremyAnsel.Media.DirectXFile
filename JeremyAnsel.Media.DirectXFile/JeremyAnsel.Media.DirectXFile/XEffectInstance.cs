using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JeremyAnsel.Media.DirectXFile
{
    public sealed class XEffectInstance
    {
        public string Name { get; set; }

        public string EffectFilename { get; set; }

        public List<Tuple<string, int>> IntegerParameters { get; } = new List<Tuple<string, int>>();

        public List<Tuple<string, float[]>> FloatParameters { get; } = new List<Tuple<string, float[]>>();

        public List<Tuple<string, string>> StringParameters { get; } = new List<Tuple<string, string>>();

        internal static XEffectInstance Read(XFileTokenReader tokenReader)
        {
            var effect = new XEffectInstance();

            if (tokenReader.FileReader.GetNextToken() == XToken.Name)
            {
                effect.Name = tokenReader.ReadName();
            }

            tokenReader.ReadAssert(XToken.OpenedBrace);

            effect.EffectFilename = tokenReader.ReadString();

            while (tokenReader.FileReader.GetNextToken() != XToken.ClosedBrace)
            {
                string identifier = tokenReader.ReadName();

                switch (identifier)
                {
                    case "EffectParamDWord":
                        effect.ReadEffectParamDWord(tokenReader);
                        break;

                    case "EffectParamFloats":
                        effect.ReadEffectParamFloats(tokenReader);
                        break;

                    case "EffectParamString":
                        effect.ReadEffectParamString(tokenReader);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            tokenReader.ReadAssert(XToken.ClosedBrace);

            return effect;
        }

        private void ReadEffectParamDWord(XFileTokenReader tokenReader)
        {
            tokenReader.ReadAssert(XToken.OpenedBrace);

            string paramName = tokenReader.ReadString();
            int value = tokenReader.ReadIntegerFromList();
            tokenReader.ReadSeparator();

            this.IntegerParameters.Add(Tuple.Create(paramName, value));

            tokenReader.ReadAssert(XToken.ClosedBrace);
        }

        private void ReadEffectParamFloats(XFileTokenReader tokenReader)
        {
            tokenReader.ReadAssert(XToken.OpenedBrace);

            string paramName = tokenReader.ReadString();
            int valuesCount = tokenReader.ReadIntegerFromList();
            float[] values = tokenReader.ReadFloatArrayFromList(valuesCount);
            tokenReader.ReadSeparator();

            this.FloatParameters.Add(Tuple.Create(paramName, values));

            tokenReader.ReadAssert(XToken.ClosedBrace);
        }

        private void ReadEffectParamString(XFileTokenReader tokenReader)
        {
            tokenReader.ReadAssert(XToken.OpenedBrace);

            string paramName = tokenReader.ReadString();
            string value = tokenReader.ReadString();

            this.StringParameters.Add(Tuple.Create(paramName, value));

            tokenReader.ReadAssert(XToken.ClosedBrace);
        }
    }
}
