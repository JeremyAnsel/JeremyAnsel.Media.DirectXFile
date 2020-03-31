using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace JeremyAnsel.Media.DirectXFile
{
    //internal sealed class MSZipStream : Stream
    //{
    //    private readonly Stream baseStream;

    //    private long basePosition;

    //    private Stream currentDeflateStream;

    //    public MSZipStream(Stream stream)
    //    {
    //        this.baseStream = stream ?? throw new ArgumentNullException(nameof(stream));
    //        this.basePosition = stream.Position;
    //    }

    //    public override bool CanRead
    //    {
    //        get
    //        {
    //            return true;
    //        }
    //    }

    //    public override bool CanSeek
    //    {
    //        get
    //        {
    //            return false;
    //        }
    //    }

    //    public override bool CanWrite
    //    {
    //        get
    //        {
    //            return false;
    //        }
    //    }

    //    public override long Length
    //    {
    //        get
    //        {
    //            throw new NotSupportedException();
    //        }
    //    }

    //    public override long Position
    //    {
    //        get
    //        {
    //            throw new NotSupportedException();
    //        }

    //        set
    //        {
    //            throw new NotSupportedException();
    //        }
    //    }

    //    public override void Flush()
    //    {
    //        throw new NotSupportedException();
    //    }

    //    public override int Read(byte[] buffer, int offset, int count)
    //    {
    //        var reader = new BinaryReader(this.baseStream);

    //        for (int i = 0; i < count; i++)
    //        {
    //            if (this.currentDeflateStream == null)
    //            {
    //                if (this.basePosition + 4 >= this.baseStream.Length)
    //                {
    //                    return i;
    //                    //throw new EndOfStreamException();
    //                }

    //                this.baseStream.Seek(this.basePosition, SeekOrigin.Begin);
    //                this.basePosition += 4 + reader.ReadUInt16();

    //                int s0 = this.baseStream.ReadByte();
    //                int s1 = this.baseStream.ReadByte();

    //                if (s0 != 'C' || s1 != 'K')
    //                {
    //                    throw new InvalidDataException();
    //                }

    //                this.currentDeflateStream = new DeflateStream(this.baseStream, CompressionMode.Decompress, true);
    //            }

    //            int b = this.currentDeflateStream.ReadByte();

    //            if (b == -1)
    //            {
    //                if (this.currentDeflateStream != null)
    //                {
    //                    this.currentDeflateStream.Dispose();
    //                    this.currentDeflateStream = null;
    //                }

    //                if (this.basePosition + 4 >= this.baseStream.Length)
    //                {
    //                    return i;
    //                    //throw new EndOfStreamException();
    //                }

    //                this.baseStream.Seek(this.basePosition, SeekOrigin.Begin);
    //                this.basePosition += 4 + reader.ReadUInt16();

    //                int s0 = this.baseStream.ReadByte();
    //                int s1 = this.baseStream.ReadByte();

    //                if (s0 != 'C' || s1 != 'K')
    //                {
    //                    throw new InvalidDataException();
    //                }

    //                this.currentDeflateStream = new DeflateStream(this.baseStream, CompressionMode.Decompress, true);
    //                b = this.currentDeflateStream.ReadByte();

    //                if (b == -1)
    //                {
    //                    return i;
    //                    //throw new EndOfStreamException();
    //                }
    //            }

    //            buffer[offset + i] = (byte)b;
    //        }

    //        return count;
    //    }

    //    public override long Seek(long offset, SeekOrigin origin)
    //    {
    //        throw new NotSupportedException();
    //    }

    //    public override void SetLength(long value)
    //    {
    //        throw new NotSupportedException();
    //    }

    //    public override void Write(byte[] buffer, int offset, int count)
    //    {
    //        throw new NotSupportedException();
    //    }

    //    protected override void Dispose(bool disposing)
    //    {
    //        if (this.currentDeflateStream != null)
    //        {
    //            this.currentDeflateStream.Dispose();
    //            this.currentDeflateStream = null;
    //        }
    //    }
    //}
}
