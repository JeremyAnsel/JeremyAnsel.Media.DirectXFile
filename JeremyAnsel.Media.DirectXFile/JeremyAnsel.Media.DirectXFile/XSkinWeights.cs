using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JeremyAnsel.Media.DirectXFile
{
    public sealed class XSkinWeights
    {
        public string Name { get; set; }

        public string TransformNodeName { get; set; }

        public List<int> VertexIndices { get; } = new List<int>();

        public List<float> Weights { get; } = new List<float>();

        public XMatrix4x4 MatrixOffset { get; set; }

        internal static XSkinWeights Read(XFileTokenReader tokenReader)
        {
            var skin = new XSkinWeights();

            if (tokenReader.FileReader.GetNextToken() == XToken.Name)
            {
                skin.Name = tokenReader.ReadName();
            }

            tokenReader.ReadAssert(XToken.OpenedBrace);

            skin.TransformNodeName = tokenReader.ReadString();

            int nWeights = tokenReader.ReadIntegerFromList();
            skin.VertexIndices.Capacity = nWeights;
            skin.Weights.Capacity = nWeights;

            for (int i = 0; i < nWeights; i++)
            {
                skin.VertexIndices.Add(tokenReader.ReadIntegerFromList());
            }

            tokenReader.ReadSeparator();

            for (int i = 0; i < nWeights; i++)
            {
                skin.Weights.Add(tokenReader.ReadFloatFromList());
            }

            tokenReader.ReadSeparator();

            skin.MatrixOffset = new XMatrix4x4
            {
                Matrix = tokenReader.ReadFloatArrayFromList(16)
            };

            tokenReader.ReadAssert(XToken.ClosedBrace);

            return skin;
        }
    }
}
