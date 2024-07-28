using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace JeremyAnsel.Media.DirectXFile
{
    public sealed class XMesh
    {
        public string Name { get; set; } = string.Empty;

        public List<XVector> Vertices { get; } = new List<XVector>();

        public List<List<int>> FacesIndices { get; } = new List<List<int>>();

        public List<XMaterial> Materials { get; } = new List<XMaterial>();

        public List<int> MaterialsFacesIndices { get; } = new List<int>();

        public List<XVector> Normals { get; } = new List<XVector>();

        public List<List<int>> FacesNormalsIndices { get; } = new List<List<int>>();

        public List<XCoords2d> TextureCoords { get; } = new List<XCoords2d>();

        public int OriginalVerticesCount { get; set; }

        [SuppressMessage("Performance", "CA1819:Les propriétés ne doivent pas retourner de tableaux", Justification = "Reviewed.")]
        public int[]? VertexDuplicationIndices { get; set; }

        public List<Tuple<int, XColorRgba>> VertexColors { get; } = new List<Tuple<int, XColorRgba>>();

        public int MaxSkinWeightsPerVertex { get; set; }

        public int MaxSkinWeightsPerFace { get; set; }

        public int BonesCount { get; set; }

        public List<XSkinWeights> SkinWeights { get; } = new List<XSkinWeights>();

        public uint FVF { get; set; }

        public List<uint> FVFData { get; } = new List<uint>();

        public List<XVertexElement> VertexElements { get; } = new List<XVertexElement>();

        [SuppressMessage("Performance", "CA1819:Les propriétés ne doivent pas retourner de tableaux", Justification = "Reviewed.")]
        public uint[]? VertexElementsData { get; set; }

        public override string ToString()
        {
            return this.Name;
        }

        internal static XMesh Read(XFileTokenReader tokenReader)
        {
            var mesh = new XMesh();

            if (tokenReader.FileReader.GetNextToken() == XToken.Name)
            {
                mesh.Name = tokenReader.ReadName();
            }

            tokenReader.ReadAssert(XToken.OpenedBrace);

            int nVertices = tokenReader.ReadIntegerFromList();
            mesh.Vertices.Capacity = nVertices;

            for (int i = 0; i < nVertices; i++)
            {
                var vector = new XVector
                {
                    X = tokenReader.ReadFloatFromList(),
                    Y = tokenReader.ReadFloatFromList(),
                    Z = tokenReader.ReadFloatFromList()
                };

                mesh.Vertices.Add(vector);
            }

            int nFaces = tokenReader.ReadIntegerFromList();
            mesh.FacesIndices.Capacity = nFaces;

            for (int faceIndex = 0; faceIndex < nFaces; faceIndex++)
            {
                int indicesCount = tokenReader.ReadIntegerFromList();
                var vertices = new List<int>(indicesCount);

                for (int i = 0; i < indicesCount; i++)
                {
                    vertices.Add(tokenReader.ReadIntegerFromList());
                }

                mesh.FacesIndices.Add(vertices);
            }

            bool meshMaterialListRead = false;
            bool meshNormalsRead = false;
            bool meshTextureCoordsRead = false;
            bool vertexDuplicationIndicesRead = false;
            bool meshVertexColorsRead = false;
            bool xSkinMeshHeaderRead = false;
            bool fvfDataRead = false;
            bool vertexElementsRead = false;

            while (tokenReader.FileReader.GetNextToken() != XToken.ClosedBrace)
            {
                string identifier = tokenReader.ReadName();

                if (string.IsNullOrEmpty(identifier))
                {
                    if (tokenReader.FileReader is XFileBinaryReader binaryReader)
                    {
                        identifier = binaryReader.ReadNullTerminatedString();
                    }
                }

                switch (identifier)
                {
                    case "MeshMaterialList":
                        if (meshMaterialListRead)
                        {
                            throw new InvalidDataException();
                        }

                        mesh.ReadMeshMaterialList(tokenReader);
                        meshMaterialListRead = true;
                        break;

                    case "MeshNormals":
                        if (meshNormalsRead)
                        {
                            //throw new InvalidDataException();
                            mesh.Normals.Clear();
                            mesh.FacesNormalsIndices.Clear();
                        }

                        mesh.ReadMeshNormals(tokenReader);
                        meshNormalsRead = true;
                        break;

                    case "MeshTextureCoords":
                        if (meshTextureCoordsRead)
                        {
                            //throw new InvalidDataException();
                            mesh.TextureCoords.Clear();
                        }

                        mesh.ReadMeshTextureCoords(tokenReader);
                        meshTextureCoordsRead = true;
                        break;

                    case "VertexDuplicationIndices":
                        if (vertexDuplicationIndicesRead)
                        {
                            throw new InvalidDataException();
                        }

                        mesh.ReadVertexDuplicationIndices(tokenReader);
                        vertexDuplicationIndicesRead = true;
                        break;

                    case "MeshVertexColors":
                        if (meshVertexColorsRead)
                        {
                            throw new InvalidDataException();
                        }

                        mesh.ReadMeshVertexColors(tokenReader);
                        meshVertexColorsRead = true;
                        break;

                    case "XSkinMeshHeader":
                        if (xSkinMeshHeaderRead)
                        {
                            throw new InvalidDataException();
                        }

                        mesh.ReadXSkinMeshHeader(tokenReader);
                        xSkinMeshHeaderRead = true;
                        break;

                    case "SkinWeights":
                        {
                            XSkinWeights skin = XSkinWeights.Read(tokenReader);
                            mesh.SkinWeights.Add(skin);
                            break;
                        }

                    case "FVFData":
                        {
                            if (fvfDataRead)
                            {
                                throw new InvalidDataException();
                            }

                            mesh.ReadFVFData(tokenReader);
                            fvfDataRead = true;
                            break;
                        }

                    case "DeclData":
                        {
                            if (vertexElementsRead)
                            {
                                throw new InvalidDataException();
                            }

                            mesh.ReadVertexElements(tokenReader);
                            vertexElementsRead = true;
                            break;
                        }

                    case "":
                        throw new InvalidDataException();

                    default:
                        throw new NotImplementedException();
                }
            }

            tokenReader.ReadAssert(XToken.ClosedBrace);

            return mesh;
        }

        private void ReadMeshMaterialList(XFileTokenReader tokenReader)
        {
            if (tokenReader.FileReader.GetNextToken() == XToken.Name)
            {
                tokenReader.ReadName();
            }

            tokenReader.ReadAssert(XToken.OpenedBrace);

            int nMaterials = tokenReader.ReadIntegerFromList();

            int nFaceIndexes = tokenReader.ReadIntegerFromList();
            this.MaterialsFacesIndices.Capacity = nFaceIndexes;

            for (int i = 0; i < nFaceIndexes; i++)
            {
                this.MaterialsFacesIndices.Add(tokenReader.ReadIntegerFromList());
            }

            for (int i = 0; i < nMaterials; i++)
            {
                XToken token = tokenReader.FileReader.GetNextToken();

                if (token == XToken.OpenedBrace)
                {
                    tokenReader.ReadAssert(XToken.OpenedBrace);
                    string reference = tokenReader.ReadName();
                    tokenReader.ReadAssert(XToken.ClosedBrace);

                    XMaterial material = new XMaterial
                    {
                        Name = reference,
                        IsReference = true
                    };

                    this.Materials.Add(material);
                }
                else
                {
                    string identifier = tokenReader.ReadName();

                    switch (identifier)
                    {
                        case "Material":
                            {
                                XMaterial material = XMaterial.Read(tokenReader);
                                this.Materials.Add(material);
                                break;
                            }

                        default:
                            throw new InvalidDataException();
                    }
                }
            }

            tokenReader.ReadAssert(XToken.ClosedBrace);
        }

        private void ReadMeshNormals(XFileTokenReader tokenReader)
        {
            if (tokenReader.FileReader.GetNextToken() == XToken.Name)
            {
                tokenReader.ReadName();
            }

            tokenReader.ReadAssert(XToken.OpenedBrace);

            int nNormals = tokenReader.ReadIntegerFromList();
            this.Normals.Capacity = nNormals;

            for (int i = 0; i < nNormals; i++)
            {
                var vector = new XVector
                {
                    X = tokenReader.ReadFloatFromList(),
                    Y = tokenReader.ReadFloatFromList(),
                    Z = tokenReader.ReadFloatFromList()
                };

                this.Normals.Add(vector);
            }

            tokenReader.ReadSeparator();

            int nFacesNormals = tokenReader.ReadIntegerFromList();
            this.FacesNormalsIndices.Capacity = nFacesNormals;

            for (int faceIndex = 0; faceIndex < nFacesNormals; faceIndex++)
            {
                int indicesCount = tokenReader.ReadIntegerFromList();
                var vertices = new List<int>(indicesCount);

                for (int i = 0; i < indicesCount; i++)
                {
                    int v = tokenReader.ReadIntegerFromList();
                    vertices.Add(v);
                }

                this.FacesNormalsIndices.Add(vertices);
            }

            tokenReader.ReadSeparator();
            tokenReader.ReadAssert(XToken.ClosedBrace);
        }

        private void ReadMeshTextureCoords(XFileTokenReader tokenReader)
        {
            if (tokenReader.FileReader.GetNextToken() == XToken.Name)
            {
                tokenReader.ReadName();
            }

            tokenReader.ReadAssert(XToken.OpenedBrace);

            int nTextureCoords = tokenReader.ReadIntegerFromList();
            this.TextureCoords.Capacity = nTextureCoords;

            for (int i = 0; i < nTextureCoords; i++)
            {
                var coord = new XCoords2d
                {
                    U = tokenReader.ReadFloatFromList(),
                    V = tokenReader.ReadFloatFromList()
                };

                this.TextureCoords.Add(coord);
            }

            tokenReader.ReadAssert(XToken.ClosedBrace);
        }

        private void ReadVertexDuplicationIndices(XFileTokenReader tokenReader)
        {
            if (tokenReader.FileReader.GetNextToken() == XToken.Name)
            {
                tokenReader.ReadName();
            }

            tokenReader.ReadAssert(XToken.OpenedBrace);

            int indicesCount = tokenReader.ReadIntegerFromList();
            this.OriginalVerticesCount = tokenReader.ReadIntegerFromList();
            this.VertexDuplicationIndices = tokenReader.ReadIntegerArrayFromList(indicesCount);

            tokenReader.ReadAssert(XToken.ClosedBrace);
        }

        private void ReadMeshVertexColors(XFileTokenReader tokenReader)
        {
            if (tokenReader.FileReader.GetNextToken() == XToken.Name)
            {
                tokenReader.ReadName();
            }

            tokenReader.ReadAssert(XToken.OpenedBrace);

            int vertexCount = tokenReader.ReadIntegerFromList();

            for (int i = 0; i < vertexCount; i++)
            {
                int index = tokenReader.ReadIntegerFromList();
                XColorRgba color = new XColorRgba
                {
                    Red = tokenReader.ReadFloatFromList(),
                    Green = tokenReader.ReadFloatFromList(),
                    Blue = tokenReader.ReadFloatFromList(),
                    Alpha = tokenReader.ReadFloatFromList(),
                };

                tokenReader.ReadSeparator();

                this.VertexColors.Add(Tuple.Create(index, color));
            }

            tokenReader.ReadSeparator();

            tokenReader.ReadAssert(XToken.ClosedBrace);
        }

        private void ReadXSkinMeshHeader(XFileTokenReader tokenReader)
        {
            if (tokenReader.FileReader.GetNextToken() == XToken.Name)
            {
                tokenReader.ReadName();
            }

            tokenReader.ReadAssert(XToken.OpenedBrace);

            this.MaxSkinWeightsPerVertex = tokenReader.ReadIntegerFromList();
            this.MaxSkinWeightsPerFace = tokenReader.ReadIntegerFromList();
            this.BonesCount = tokenReader.ReadIntegerFromList();

            tokenReader.ReadAssert(XToken.ClosedBrace);
        }

        private void ReadFVFData(XFileTokenReader tokenReader)
        {
            if (tokenReader.FileReader.GetNextToken() == XToken.Name)
            {
                tokenReader.ReadName();
            }

            tokenReader.ReadAssert(XToken.OpenedBrace);

            this.FVF = tokenReader.ReadUnsignedIntegerFromList();

            int count = tokenReader.ReadIntegerFromList();

            for (int i = 0; i < count; i++)
            {
                this.FVFData.Add(tokenReader.ReadUnsignedIntegerFromList());
            }

            tokenReader.ReadSeparator();

            tokenReader.ReadAssert(XToken.ClosedBrace);
        }

        private void ReadVertexElements(XFileTokenReader tokenReader)
        {
            if (tokenReader.FileReader.GetNextToken() == XToken.Name)
            {
                tokenReader.ReadName();
            }

            tokenReader.ReadAssert(XToken.OpenedBrace);

            int nElements = tokenReader.ReadIntegerFromList();
            this.VertexElements.Capacity = nElements;

            for (int i = 0; i < nElements; i++)
            {
                var element = new XVertexElement
                {
                    DataType = (XVertexElementDataType)tokenReader.ReadIntegerFromList(),
                    Method = (XVertexElementMethod)tokenReader.ReadIntegerFromList(),
                    Usage = (XVertexElementUsage)tokenReader.ReadIntegerFromList(),
                    UsageIndex = tokenReader.ReadIntegerFromList(),
                };

                this.VertexElements.Add(element);
            }

            int nDWords = tokenReader.ReadIntegerFromList();
            this.VertexElementsData = tokenReader.ReadUnsignedIntegerArrayFromList(nDWords);

            tokenReader.ReadSeparator();

            tokenReader.ReadAssert(XToken.ClosedBrace);
        }
    }
}
