using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace JeremyAnsel.Media.DirectXFile
{
    public sealed class XFile
    {
        public Version? FormatVersion { get; private set; }

        public XFormat Format { get; private set; }

        public bool UseDouble { get; private set; }

        public Version? FileVersion { get; set; }

        public int FileFlags { get; set; }

        public List<Tuple<string, Guid>> Templates { get; } = new List<Tuple<string, Guid>>();

        public List<XMaterial> Materials { get; } = new List<XMaterial>();

        public List<XMesh> Meshes { get; } = new List<XMesh>();

        public List<XFrame> Frames { get; } = new List<XFrame>();

        public List<XAnimationSet> AnimationSets { get; } = new List<XAnimationSet>();

        public int AnimTicksPerSecond { get; set; }

        public static XFile FromFile(string fileName)
        {
            using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                return XFile.FromStream(file);
            }
        }

        public static XFile FromStream(Stream stream)
        {
            var xfile = new XFile();

            xfile.ReadHeader(stream);

            switch (xfile.Format)
            {
                case XFormat.Text:
                    xfile.ReadText(stream);
                    break;

                case XFormat.Binary:
                    xfile.ReadBinary(stream);
                    break;

                case XFormat.TZip:
                    throw new NotSupportedException();

                case XFormat.BZip:
                    throw new NotSupportedException();
            }

            return xfile;
        }

        public static string GenerateTextFromFile(string fileName)
        {
            using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                return XFile.GenerateTextFromStream(file);
            }
        }

        public static string GenerateTextFromStream(Stream stream)
        {
            var xfile = new XFile();
            xfile.ReadHeader(stream);

            var sb = new StringBuilder();

            //try
            //{
            switch (xfile.Format)
            {
                case XFormat.Text:
                    XFileTextGenerator.ReadText(stream, sb);
                    break;

                case XFormat.Binary:
                    XFileTextGenerator.ReadBinary(stream, sb, xfile.UseDouble);
                    break;

                case XFormat.TZip:
                    throw new NotSupportedException();

                case XFormat.BZip:
                    throw new NotSupportedException();
            }
            //}
            //catch (Exception ex)
            //{
            //    sb.AppendLine();
            //    sb.AppendLine();
            //    sb.AppendLine(ex.ToString());
            //}

            return sb.ToString();
        }

        [SuppressMessage("Reliability", "CA2000:Supprimer les objets avant la mise hors de portée", Justification = "Reviewed.")]
        private void ReadHeader(Stream stream)
        {
            var reader = new BinaryReader(stream, Encoding.ASCII);
            var sb = new StringBuilder();

            sb.Clear();
            for (int i = 0; i < 4; i++)
            {
                sb.Append(reader.ReadChar());
            }

            if (sb.ToString() != "xof ")
            {
                throw new InvalidDataException();
            }

            sb.Clear();
            for (int i = 0; i < 2; i++)
            {
                sb.Append(reader.ReadChar());
            }

            string majorFormatVersion = sb.ToString();

            sb.Clear();
            for (int i = 0; i < 2; i++)
            {
                sb.Append(reader.ReadChar());
            }

            string minorFormatVersion = sb.ToString();

            this.FormatVersion = new Version(int.Parse(majorFormatVersion, CultureInfo.InvariantCulture), int.Parse(minorFormatVersion, CultureInfo.InvariantCulture));

            sb.Clear();
            for (int i = 0; i < 4; i++)
            {
                sb.Append(reader.ReadChar());
            }

            string format = sb.ToString();

            switch (format)
            {
                case "txt ":
                    this.Format = XFormat.Text;
                    break;

                case "bin ":
                    this.Format = XFormat.Binary;
                    break;

                case "tzip":
                    this.Format = XFormat.TZip;
                    break;

                case "bzip":
                    this.Format = XFormat.BZip;
                    break;

                default:
                    throw new InvalidDataException();
            }

            sb.Clear();
            for (int i = 0; i < 4; i++)
            {
                sb.Append(reader.ReadChar());
            }

            string floatSize = sb.ToString();

            switch (floatSize)
            {
                case "0032":
                    this.UseDouble = false;
                    break;

                case "0064":
                    this.UseDouble = true;
                    break;

                default:
                    throw new InvalidDataException();
            }
        }

        private void ReadText(Stream stream)
        {
            var reader = new StreamReader(stream, Encoding.ASCII);
            var fileReader = new XFileTextReader(reader, this.UseDouble);

            this.ReadFile(fileReader);
        }

        private void ReadBinary(Stream stream)
        {
            var reader = new BinaryReader(stream, Encoding.ASCII);
            var fileReader = new XFileBinaryReader(reader, this.UseDouble);

            this.ReadFile(fileReader);
        }

        private void ReadFile(IXFileReader fileReader)
        {
            var tokenReader = new XFileTokenReader(fileReader);
            var templateReader = new XFileTemplateReader(tokenReader);

            while (fileReader.GetNextToken() == XToken.Template)
            {
                Tuple<string, Guid> template = templateReader.ReadTemplate();
                this.Templates.Add(template);
            }

            bool headerRead = false;
            bool animTicksPerSecondRead = false;

            while (fileReader.GetNextToken() == XToken.Name)
            {
                string identifier = tokenReader.ReadName();

                switch (identifier)
                {
                    case "Header":
                        {
                            if (headerRead)
                            {
                                throw new InvalidDataException();
                            }

                            tokenReader.ReadAssert(XToken.OpenedBrace);

                            int majorVersion = tokenReader.ReadIntegerFromList();
                            int minorVersion = tokenReader.ReadIntegerFromList();
                            this.FileVersion = new Version(majorVersion, minorVersion);

                            this.FileFlags = tokenReader.ReadIntegerFromList();

                            tokenReader.ReadAssert(XToken.ClosedBrace);
                            headerRead = true;
                            break;
                        }

                    case "Material":
                        {
                            XMaterial material = XMaterial.Read(tokenReader);
                            this.Materials.Add(material);
                            break;
                        }

                    case "Mesh":
                        {
                            XMesh mesh = XMesh.Read(tokenReader);
                            this.Meshes.Add(mesh);
                            break;
                        }

                    case "Frame":
                        {
                            XFrame frame = XFrame.Read(tokenReader);
                            this.Frames.Add(frame);
                            break;
                        }

                    case "AnimationSet":
                        {
                            XAnimationSet animationSet = XAnimationSet.Read(tokenReader);
                            this.AnimationSets.Add(animationSet);
                            break;
                        }

                    case "AnimTicksPerSecond":
                        {
                            if (animTicksPerSecondRead)
                            {
                                throw new InvalidDataException();
                            }

                            tokenReader.ReadAssert(XToken.OpenedBrace);
                            this.AnimTicksPerSecond = tokenReader.ReadIntegerFromList();
                            tokenReader.ReadAssert(XToken.ClosedBrace);
                            animTicksPerSecondRead = true;
                            break;
                        }

                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
