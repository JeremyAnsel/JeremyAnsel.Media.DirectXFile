using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JeremyAnsel.Media.DirectXFile
{
    public sealed class XFrame
    {
        public string Name { get; set; } = string.Empty;

        public XMatrix4x4? TransformMatrix { get; set; }

        public XFrameCamera? FrameCamera { get; set; }

        public List<XMesh> Meshes { get; } = new List<XMesh>();

        public List<XFrame> Frames { get; } = new List<XFrame>();

        public Dictionary<int, string> MeshesNames { get; } = new Dictionary<int, string>();

        internal static XFrame Read(XFileTokenReader tokenReader)
        {
            var frame = new XFrame();

            if (tokenReader.FileReader.GetNextToken() == XToken.Name || tokenReader.FileReader.GetNextToken() == XToken.None)
            {
                frame.Name = tokenReader.ReadName();

                if (string.IsNullOrEmpty(frame.Name))
                {
                    if (tokenReader.FileReader is XFileBinaryReader binaryReader)
                    {
                        frame.Name = binaryReader.ReadNullTerminatedString();
                    }
                }
            }

            tokenReader.ReadAssert(XToken.OpenedBrace);

            bool frameTransformMatrixRead = false;

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
                    case "FrameMeshName":
                        {
                            tokenReader.ReadAssert(XToken.OpenedBrace);
                            int renderPass = tokenReader.ReadIntegerFromList();
                            string meshName = tokenReader.ReadString();
                            tokenReader.ReadAssert(XToken.ClosedBrace);
                            frame.MeshesNames.Add(renderPass, meshName);
                            break;
                        }

                    case "FrameTransformMatrix":
                        {
                            if (frameTransformMatrixRead)
                            {
                                throw new InvalidDataException();
                            }

                            tokenReader.ReadAssert(XToken.OpenedBrace);

                            frame.TransformMatrix = new XMatrix4x4
                            {
                                Matrix = tokenReader.ReadFloatArrayFromList(16)
                            };

                            tokenReader.ReadAssert(XToken.ClosedBrace);
                            frameTransformMatrixRead = true;
                            break;
                        }

                    case "FrameCamera":
                        {
                            if (frame.FrameCamera != null)
                            {
                                throw new InvalidDataException();
                            }

                            var camera = new XFrameCamera();

                            tokenReader.ReadAssert(XToken.OpenedBrace);
                            camera.RotationScaler = tokenReader.ReadFloatFromList();
                            camera.MoveScaler = tokenReader.ReadFloatFromList();
                            tokenReader.ReadAssert(XToken.ClosedBrace);

                            frame.FrameCamera = camera;
                            break;
                        }

                    case "Mesh":
                        {
                            XMesh mesh = XMesh.Read(tokenReader);
                            frame.Meshes.Add(mesh);
                            break;
                        }

                    case "Frame":
                        {
                            XFrame subFrame = XFrame.Read(tokenReader);
                            frame.Frames.Add(subFrame);
                            break;
                        }

                    case "":
                        throw new InvalidDataException();

                    default:
                        throw new NotImplementedException();
                }
            }

            tokenReader.ReadAssert(XToken.ClosedBrace);

            return frame;
        }
    }
}
