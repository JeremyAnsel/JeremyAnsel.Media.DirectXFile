using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JeremyAnsel.Media.DirectXFile
{
    public sealed class XAnimation
    {
        public string Name { get; set; } = string.Empty;

        public string? FrameReference { get; set; }

        public List<XAnimationKey> Keys { get; } = new List<XAnimationKey>();

        public int OpenClosedOption { get; set; }

        public int PositionQualityOption { get; set; }

        internal static XAnimation Read(XFileTokenReader tokenReader)
        {
            var animation = new XAnimation();

            if (tokenReader.FileReader.GetNextToken() == XToken.Name)
            {
                animation.Name = tokenReader.ReadName();
            }

            tokenReader.ReadAssert(XToken.OpenedBrace);

            XToken token;
            while ((token = tokenReader.FileReader.GetNextToken()) != XToken.ClosedBrace)
            {
                if (token == XToken.OpenedBrace)
                {
                    if (animation.FrameReference != null)
                    {
                        throw new InvalidDataException();
                    }

                    tokenReader.ReadAssert(XToken.OpenedBrace);
                    animation.FrameReference = tokenReader.ReadName();
                    tokenReader.ReadAssert(XToken.ClosedBrace);
                    continue;
                }

                string identifier = tokenReader.ReadName();

                switch (identifier)
                {
                    case "AnimationKey":
                        {
                            XAnimationKey key = XAnimationKey.Read(tokenReader);
                            animation.Keys.Add(key);
                            break;
                        }

                    case "AnimationOptions":
                        {
                            if (tokenReader.FileReader.GetNextToken() == XToken.Name)
                            {
                                tokenReader.ReadName();
                            }

                            tokenReader.ReadAssert(XToken.OpenedBrace);

                            animation.OpenClosedOption = tokenReader.ReadIntegerFromList();
                            animation.PositionQualityOption = tokenReader.ReadIntegerFromList();

                            tokenReader.ReadAssert(XToken.ClosedBrace);
                            break;
                        }

                    default:
                        throw new NotImplementedException();
                }
            }

            tokenReader.ReadAssert(XToken.ClosedBrace);

            return animation;
        }
    }
}
