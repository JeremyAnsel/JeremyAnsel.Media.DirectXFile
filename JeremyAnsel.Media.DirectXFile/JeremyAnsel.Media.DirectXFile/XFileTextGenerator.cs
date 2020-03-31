using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;

namespace JeremyAnsel.Media.DirectXFile
{
    internal static class XFileTextGenerator
    {
        [SuppressMessage("Reliability", "CA2000:Supprimer les objets avant la mise hors de portée", Justification = "Reviewed.")]
        public static void ReadText(Stream stream, StringBuilder sb)
        {
            var reader = new StreamReader(stream, Encoding.ASCII);

            sb.Append(reader.ReadToEnd());
        }

        public static void ReadBinary(Stream stream, StringBuilder sb, bool useDouble)
        {
            var reader = new BinaryReader(stream, Encoding.ASCII);
            var fileReader = new XFileBinaryReader(reader, useDouble);
            bool newLine = true;
            bool afterSemicolon = false;

            while (true)
            {
                XToken token = fileReader.GetNextToken();

                if (token == (XToken)(-1))
                {
                    break;
                }

                if (!Enum.IsDefined(typeof(XToken), token))
                {
                    throw new InvalidDataException();
                }

                fileReader.ReadToken();
                sb.Append(' ');
                sb.Append(token);

                switch (token)
                {
                    case XToken.OpenedBrace:
                    case XToken.ClosedBrace:
                        {
                            sb.AppendLine();
                            newLine = true;
                            break;
                        }

                    case XToken.Name:
                        {
                            string name = fileReader.ReadName();

                            if (string.IsNullOrEmpty(name))
                            {
                                name = fileReader.ReadNullTerminatedString();
                            }

                            sb.Append(' ');
                            sb.Append('"');
                            sb.Append(name);
                            sb.Append('"');
                            break;
                        }

                    case XToken.String:
                        {
                            string str = fileReader.ReadString();

                            if (string.IsNullOrEmpty(str))
                            {
                                str = fileReader.ReadNullTerminatedString();
                            }

                            sb.Append(' ');
                            sb.Append('"');
                            sb.Append(str);
                            sb.Append('"');
                            break;
                        }

                    case XToken.Guid:
                        {
                            sb.Append(' ');
                            sb.Append(fileReader.ReadGuid());
                            break;
                        }

                    case XToken.IntegerList:
                        {
                            int count = fileReader.ReadInteger();
                            sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", count);
                            sb.AppendLine();

                            const int w = 16;

                            for (int i = 0; i < count; i++)
                            {
                                if ((i % w) == 0)
                                {
                                    if (i != 0)
                                    {
                                        sb.AppendLine();
                                    }

                                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0}: ", i);
                                }

                                int value = fileReader.ReadInteger();
                                sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", value);
                            }

                            sb.AppendLine();
                            sb.AppendFormat(CultureInfo.InvariantCulture, "{0}:", count);
                            sb.AppendLine();
                            newLine = true;
                            break;
                        }

                    case XToken.FloatList:
                        {
                            int count = fileReader.ReadInteger();
                            sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", count);
                            sb.AppendLine();

                            const int w = 16;

                            for (int i = 0; i < count; i++)
                            {
                                if ((i % w) == 0)
                                {
                                    if (i != 0)
                                    {
                                        sb.AppendLine();
                                    }

                                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0}: ", i);
                                }

                                float value = fileReader.ReadFloat();
                                sb.AppendFormat(CultureInfo.InvariantCulture, " {0:F6}", value);
                            }

                            sb.AppendLine();
                            sb.AppendFormat(CultureInfo.InvariantCulture, "{0}:", count);
                            sb.AppendLine();
                            newLine = true;
                            break;
                        }

                    case XToken.Semicolon:
                    case XToken.Comma:
                        afterSemicolon = true;
                        break;
                }

                if (newLine)
                {
                    switch (token)
                    {
                        case XToken.None:
                        case XToken.OpenedBrace:
                        case XToken.ClosedBrace:
                        case XToken.IntegerList:
                        case XToken.FloatList:
                            break;

                        default:
                            newLine = false;
                            break;
                    }
                }

                if (afterSemicolon)
                {
                    switch (token)
                    {
                        case XToken.Semicolon:
                        case XToken.Comma:
                            break;

                        default:
                            afterSemicolon = false;
                            break;
                    }
                }
            }

            sb.AppendLine();
        }
    }
}
