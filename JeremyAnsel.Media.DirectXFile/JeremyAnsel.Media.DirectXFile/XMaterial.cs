using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace JeremyAnsel.Media.DirectXFile
{
    public sealed class XMaterial
    {
        public string Name { get; set; }

        public bool IsReference { get; set; }

        public XColorRgba FaceColor { get; set; }

        public float Power { get; set; }

        public XColorRgb SpecularColor { get; set; }

        public XColorRgb EmissiveColor { get; set; }

        public string Filename { get; set; }

        public XEffectInstance EffectInstance { get; set; }

        public override string ToString()
        {
            if (this.IsReference)
            {
                return string.Format(CultureInfo.InvariantCulture, "Ref {0}", this.Name);
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", this.Name, this.FaceColor, this.Filename);
            }
        }

        internal static XMaterial Read(XFileTokenReader tokenReader)
        {
            var material = new XMaterial();

            if (tokenReader.FileReader.GetNextToken() == XToken.Name)
            {
                material.Name = tokenReader.ReadName();
            }

            tokenReader.ReadAssert(XToken.OpenedBrace);

            material.FaceColor = new XColorRgba
            {
                Red = tokenReader.ReadFloatFromList(),
                Green = tokenReader.ReadFloatFromList(),
                Blue = tokenReader.ReadFloatFromList(),
                Alpha = tokenReader.ReadFloatFromList()
            };

            material.Power = tokenReader.ReadFloatFromList();

            material.SpecularColor = new XColorRgb
            {
                Red = tokenReader.ReadFloatFromList(),
                Green = tokenReader.ReadFloatFromList(),
                Blue = tokenReader.ReadFloatFromList()
            };

            material.EmissiveColor = new XColorRgb
            {
                Red = tokenReader.ReadFloatFromList(),
                Green = tokenReader.ReadFloatFromList(),
                Blue = tokenReader.ReadFloatFromList()
            };

            bool textureFilenameRead = false;
            bool effectInstanceRead = false;

            while (tokenReader.FileReader.GetNextToken() != XToken.ClosedBrace)
            {
                string identifier = tokenReader.ReadName();

                switch (identifier)
                {
                    case "TextureFilename":
                        if (textureFilenameRead)
                        {
                            throw new InvalidDataException();
                        }

                        material.ReadTextureFilename(tokenReader);
                        textureFilenameRead = true;
                        break;

                    case "EffectInstance":
                        if (effectInstanceRead)
                        {
                            throw new InvalidDataException();
                        }

                        material.EffectInstance = XEffectInstance.Read(tokenReader);
                        effectInstanceRead = true;
                        break;

                    default:
                        throw new InvalidDataException();
                }
            }

            tokenReader.ReadAssert(XToken.ClosedBrace);

            return material;
        }

        private void ReadTextureFilename(XFileTokenReader tokenReader)
        {
            tokenReader.ReadAssert(XToken.OpenedBrace);
            this.Filename = tokenReader.ReadString();
            tokenReader.ReadAssert(XToken.ClosedBrace);
        }
    }
}
