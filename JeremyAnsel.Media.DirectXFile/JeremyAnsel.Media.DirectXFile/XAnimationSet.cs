using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JeremyAnsel.Media.DirectXFile
{
    public sealed class XAnimationSet
    {
        public string Name { get; set; } = string.Empty;

        public List<XAnimation> Animations { get; } = new List<XAnimation>();

        internal static XAnimationSet Read(XFileTokenReader tokenReader)
        {
            var animationSet = new XAnimationSet();

            if (tokenReader.FileReader.GetNextToken() == XToken.Name)
            {
                animationSet.Name = tokenReader.ReadName();
            }

            tokenReader.ReadAssert(XToken.OpenedBrace);

            while (tokenReader.FileReader.GetNextToken() != XToken.ClosedBrace)
            {
                string identifier = tokenReader.ReadName();

                switch (identifier)
                {
                    case "Animation":
                        {
                            XAnimation animation = XAnimation.Read(tokenReader);
                            animationSet.Animations.Add(animation);
                            break;
                        }

                    default:
                        throw new InvalidDataException();
                }
            }

            tokenReader.ReadAssert(XToken.ClosedBrace);

            return animationSet;
        }
    }
}
