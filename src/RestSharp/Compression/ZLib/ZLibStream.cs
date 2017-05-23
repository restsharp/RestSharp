// ZlibStream.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009 Dino Chiesa and Microsoft Corporation.  
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License. 
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------
//
// last saved (in emacs): 
// Time-stamp: <2009-August-19 18:39:59>
//
// ------------------------------------------------------------------
//
// This module defines the ZlibStream class, which is similar in idea to
// the System.IO.Compression.DeflateStream and
// System.IO.Compression.GZipStream classes in the .NET BCL.
//
// ------------------------------------------------------------------


#if WINDOWS_PHONE

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RestSharp.Compression.ZLib
{
    /// <summary>
    /// Represents a Zlib stream for compression or decompression.
    /// </summary>
    /// <remarks>
    ///
    /// <para>
    /// The ZlibStream is a <see
    /// href="http://en.wikipedia.org/wiki/Decorator_pattern">Decorator</see> on a <see
    /// cref="System.IO.Stream"/>.  It adds ZLIB compression or decompression to any
    /// stream.
    /// </para>
    ///
    /// <para> Using this stream, applications can compress or decompress data via
    /// stream <c>Read</c> and <c>Write</c> operations.  Either compresssion or
    /// decompression can occur through either reading or writing. The compression
    /// format used is ZLIB, which is documented in <see
    /// href="http://www.ietf.org/rfc/rfc1950.txt">IETF RFC 1950</see>, "ZLIB Compressed
    /// Data Format Specification version 3.3". This implementation of ZLIB always uses
    /// DEFLATE as the compression method.  (see <see
    /// href="http://www.ietf.org/rfc/rfc1951.txt">IETF RFC 1951</see>, "DEFLATE
    /// Compressed Data Format Specification version 1.3.") </para>
    ///
    /// <para>
    /// The ZLIB format allows for varying compression methods, window sizes, and dictionaries.
    /// This implementation always uses the DEFLATE compression method, a preset dictionary,
    /// and 15 window bits by default.  
    /// </para>
    ///
    /// <para>
    /// This class is similar to <see cref="DeflateStream"/>, except that it adds the
    /// RFC1950 header and trailer bytes to a compressed stream when compressing, or expects
    /// the RFC1950 header and trailer bytes when decompressing.  It is also similar to the
    /// <see cref="GZipStream"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="DeflateStream" />
    /// <seealso cref="GZipStream" />
    internal class ZlibStream : Stream
    {
        internal ZlibBaseStream BaseStream;
        bool disposed;

        public ZlibStream(Stream stream)
        {
            this.BaseStream = new ZlibBaseStream(stream, ZlibStreamFlavor.Zlib, false);
        }

        #region Zlib properties

        /// <summary>
        /// This property sets the flush behavior on the stream.  
        /// Sorry, though, not sure exactly how to describe all the various settings.
        /// </summary>
        virtual public FlushType FlushMode
        {
            get { return (this.BaseStream.FlushMode); }
            set
            {
                if (this.disposed)
                    throw new ObjectDisposedException("ZlibStream");

                this.BaseStream.FlushMode = value;
            }
        }

        /// <summary>
        /// The size of the working buffer for the compression codec. 
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        /// The working buffer is used for all stream operations.  The default size is 1024 bytes.
        /// The minimum size is 128 bytes. You may get better performance with a larger buffer.
        /// Then again, you might not.  You would have to test it.
        /// </para>
        ///
        /// <para>
        /// Set this before the first call to Read()  or Write() on the stream. If you try to set it 
        /// afterwards, it will throw.
        /// </para>
        /// </remarks>
        public int BufferSize
        {
            get { return this.BaseStream.BufferSize; }
            set
            {
                if (this.disposed)
                    throw new ObjectDisposedException("ZlibStream");

                if (this.BaseStream.workingBuffer != null)
                    throw new ZlibException("The working buffer is already set.");

                if (value < ZlibConstants.WORKING_BUFFER_SIZE_MIN)
                    throw new ZlibException(string.Format("Don't be silly. {0} bytes?? Use a bigger buffer.", value));

                this.BaseStream.BufferSize = value;
            }
        }

        /// <summary> Returns the total number of bytes input so far.</summary>
        virtual public long TotalIn
        {
            get { return this.BaseStream.z.TotalBytesIn; }
        }

        /// <summary> Returns the total number of bytes output so far.</summary>
        virtual public long TotalOut
        {
            get { return this.BaseStream.z.TotalBytesOut; }
        }

        #endregion

        #region System.IO.Stream methods

        /// <summary>
        /// Dispose the stream.  
        /// </summary>
        /// <remarks>
        /// This may or may not result in a Close() call on the captive stream. 
        /// See the constructors that have a leaveOpen parameter for more information.
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!this.disposed)
                {
                    if (disposing && (this.BaseStream != null))
                        this.BaseStream.Close();

                    this.disposed = true;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Indicates whether the stream can be read.
        /// </summary>
        /// <remarks>
        /// The return value depends on whether the captive stream supports reading.
        /// </remarks>
        public override bool CanRead
        {
            get
            {
                if (this.disposed)
                    throw new ObjectDisposedException("ZlibStream");

                return this.BaseStream.Stream.CanRead;
            }
        }

        /// <summary>
        /// Indicates whether the stream supports Seek operations.
        /// </summary>
        /// <remarks>
        /// Always returns false.
        /// </remarks>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Indicates whether the stream can be written.
        /// </summary>
        /// <remarks>
        /// The return value depends on whether the captive stream supports writing.
        /// </remarks>
        public override bool CanWrite
        {
            get
            {
                if (this.disposed)
                    throw new ObjectDisposedException("ZlibStream");

                return this.BaseStream.Stream.CanWrite;
            }
        }

        /// <summary>
        /// Flush the stream.
        /// </summary>
        public override void Flush()
        {
            if (this.disposed)
                throw new ObjectDisposedException("ZlibStream");

            this.BaseStream.Flush();
        }

        /// <summary>
        /// Reading this property always throws a NotImplementedException.
        /// </summary>
        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// The position of the stream pointer. 
        /// </summary>
        /// <remarks>
        /// Writing this property always throws a NotImplementedException. Reading will
        /// return the total bytes written out, if used in writing, or the total bytes 
        /// read in, if used in reading.   The count may refer to compressed bytes or 
        /// uncompressed bytes, depending on how you've used the stream.
        /// </remarks>
        public override long Position
        {
            get
            {
                switch (this.BaseStream.streamMode)
                {
                    case ZlibBaseStream.StreamMode.Writer:
                        return this.BaseStream.z.TotalBytesOut;

                    case ZlibBaseStream.StreamMode.Reader:
                        return this.BaseStream.z.TotalBytesIn;
                }

                return 0;
            }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Read data from the stream. 
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        /// If you wish to use the ZlibStream to compress data while reading, you can create a
        /// ZlibStream with CompressionMode.Compress, providing an uncompressed data stream.  Then
        /// call Read() on that ZlibStream, and the data read will be compressed.  If you wish to
        /// use the ZlibStream to decompress data while reading, you can create a ZlibStream with
        /// CompressionMode.Decompress, providing a readable compressed data stream.  Then call
        /// Read() on that ZlibStream, and the data will be decompressed as it is read.
        /// </para>
        ///
        /// <para>
        /// A ZlibStream can be used for Read() or Write(), but not both. 
        /// </para>
        /// </remarks>
        /// <param name="buffer">The buffer into which the read data should be placed.</param>
        /// <param name="offset">the offset within that data array to put the first byte read.</param>
        /// <param name="count">the number of bytes to read.</param>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this.disposed)
                throw new ObjectDisposedException("ZlibStream");

            return this.BaseStream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Calling this method always throws a NotImplementedException.
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calling this method always throws a NotImplementedException.
        /// </summary>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write data to the stream. 
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        /// If you wish to use the ZlibStream to compress data while writing, you can create a
        /// ZlibStream with CompressionMode.Compress, and a writable output stream.  Then call
        /// Write() on that ZlibStream, providing uncompressed data as input.  The data sent to
        /// the output stream will be the compressed form of the data written.  If you wish to use
        /// the ZlibStream to decompress data while writing, you can create a ZlibStream with
        /// CompressionMode.Decompress, and a writable output stream.  Then call Write() on that
        /// stream, providing previously compressed data. The data sent to the output stream will
        /// be the decompressed form of the data written.
        /// </para>
        ///
        /// <para>
        /// A ZlibStream can be used for Read() or Write(), but not both. 
        /// </para>
        /// </remarks>
        /// <param name="buffer">The buffer holding data to write to the stream.</param>
        /// <param name="offset">the offset within that data array to find the first byte to write.</param>
        /// <param name="count">the number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this.disposed)
                throw new ObjectDisposedException("ZlibStream");

            this.BaseStream.Write(buffer, offset, count);
        }

        #endregion

        /// <summary>
        /// Uncompress a byte array into a single string.
        /// </summary>
        /// <seealso cref="ZlibStream.CompressString(String)"/>
        /// <param name="compressed">
        /// A buffer containing ZLIB-compressed data.  
        /// </param>
        public static string UncompressString(byte[] compressed)
        {
            // workitem 8460
            byte[] working = new byte[1024];
            Encoding encoding = Encoding.UTF8;

            using (MemoryStream output = new MemoryStream())
            {
                using (MemoryStream input = new MemoryStream(compressed))
                {
                    using (Stream decompressor = new ZlibStream(input))
                    {
                        int n;

                        while ((n = decompressor.Read(working, 0, working.Length)) != 0)
                        {
                            output.Write(working, 0, n);
                        }
                    }

                    // reset to allow read from start
                    output.Seek(0, SeekOrigin.Begin);

                    StreamReader sr = new StreamReader(output, encoding);

                    return sr.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Uncompress a byte array into a byte array.
        /// </summary>
        /// <seealso cref="ZlibStream.CompressBuffer(byte[])"/>
        /// <seealso cref="ZlibStream.UncompressString(byte[])"/>
        /// <param name="compressed">
        /// A buffer containing ZLIB-compressed data.  
        /// </param>
        public static byte[] UncompressBuffer(byte[] compressed)
        {
            // workitem 8460
            byte[] working = new byte[1024];

            using (MemoryStream output = new MemoryStream())
            {
                using (MemoryStream input = new MemoryStream(compressed))
                {
                    using (Stream decompressor = new ZlibStream(input))
                    {
                        int n;
                        while ((n = decompressor.Read(working, 0, working.Length)) != 0)
                        {
                            output.Write(working, 0, n);
                        }
                    }

                    return output.ToArray();
                }
            }
        }
    }

    internal enum ZlibStreamFlavor
    {
        Zlib = 1950,
        Deflate = 1951,
        Gzip = 1952
    }

    internal class ZlibBaseStream : Stream
    {
        protected internal ZlibCodec z; // deferred init... new ZlibCodec();

        protected internal StreamMode streamMode = StreamMode.Undefined;
        protected internal FlushType FlushMode;
        protected internal ZlibStreamFlavor Flavor;
        protected internal bool LeaveOpen;
        protected internal byte[] workingBuffer;
        protected internal int BufferSize = ZlibConstants.WORKING_BUFFER_SIZE_DEFAULT;
        protected internal byte[] Buf1 = new byte[1];
        protected internal Stream Stream;

        // workitem 7159
        Crc32 crc;
        protected internal string GzipFileName;
        protected internal string GzipComment;
        protected internal DateTime GzipMtime;
        protected internal int GzipHeaderByteCount;

        internal int Crc32 { get { return this.crc == null ? 0 : this.crc.Crc32Result; } }

        public ZlibBaseStream(Stream stream, ZlibStreamFlavor flavor, bool leaveOpen)
        {
            this.FlushMode = FlushType.None;
            //_workingBuffer = new byte[WORKING_BUFFER_SIZE_DEFAULT];
            this.Stream = stream;
            this.LeaveOpen = leaveOpen;
            this.Flavor = flavor;
            // workitem 7159

            if (flavor == ZlibStreamFlavor.Gzip)
            {
                this.crc = new Crc32();
            }
        }

        private ZlibCodec Z
        {
            get
            {
                if (this.z == null)
                {
                    bool wantRfc1950Header = (this.Flavor == ZlibStreamFlavor.Zlib);

                    this.z = new ZlibCodec();
                    this.z.InitializeInflate(wantRfc1950Header);
                }

                return this.z;
            }
        }

        private byte[] WorkingBuffer
        {
            get { return this.workingBuffer ?? (this.workingBuffer = new byte[this.BufferSize]); }
        }

        // workitem 7813 - totally unnecessary
        //         public override void WriteByte(byte b)
        //         {
        //             _buf1[0] = (byte)b;
        //             // workitem 7159
        //             if (crc != null)
        //                 crc.SlurpBlock(_buf1, 0, 1);
        //             Write(_buf1, 0, 1);
        //         }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // workitem 7159
            // calculate the CRC on the unccompressed data  (before writing)
            if (this.crc != null)
                this.crc.SlurpBlock(buffer, offset, count);

            if (this.streamMode == StreamMode.Undefined)
                this.streamMode = StreamMode.Writer;
            else if (this.streamMode != StreamMode.Writer)
                throw new ZlibException("Cannot Write after Reading.");

            if (count == 0)
                return;

            // first reference of z property will initialize the private var _z
            this.z.InputBuffer = buffer;
            this.z.NextIn = offset;
            this.z.AvailableBytesIn = count;

            bool done;

            do
            {
                this.z.OutputBuffer = this.WorkingBuffer;
                this.z.NextOut = 0;
                this.z.AvailableBytesOut = this.workingBuffer.Length;

                //int rc = (_wantCompress)
                //    ? _z.Deflate(_flushMode)
                //    : _z.Inflate(_flushMode);
                int rc = this.z.Inflate(this.FlushMode);

                if (rc != ZlibConstants.Z_OK && rc != ZlibConstants.Z_STREAM_END)
                    throw new ZlibException("inflating: " + this.z.Message);

                this.Stream.Write(this.workingBuffer, 0, this.workingBuffer.Length - this.z.AvailableBytesOut);

                done = this.z.AvailableBytesIn == 0 && this.z.AvailableBytesOut != 0;

                // If GZIP and de-compress, we're done when 8 bytes remain.
                if (this.Flavor == ZlibStreamFlavor.Gzip)
                    done = (this.z.AvailableBytesIn == 8 && this.z.AvailableBytesOut != 0);
            }
            while (!done);
        }

        private void Finish()
        {
            if (this.z == null)
                return;

            switch (this.streamMode)
            {
                case StreamMode.Writer:
                    bool done;

                    do
                    {
                        this.z.OutputBuffer = this.WorkingBuffer;
                        this.z.NextOut = 0;
                        this.z.AvailableBytesOut = this.workingBuffer.Length;

                        //int rc = (_wantCompress)
                        //    ? _z.Deflate(FlushType.Finish)
                        //    : _z.Inflate(FlushType.Finish);

                        int rc = this.z.Inflate(FlushType.Finish);

                        if (rc != ZlibConstants.Z_STREAM_END && rc != ZlibConstants.Z_OK)
                            throw new ZlibException("inflating: " + this.z.Message);

                        if (this.workingBuffer.Length - this.z.AvailableBytesOut > 0)
                        {
                            this.Stream.Write(this.workingBuffer, 0, this.workingBuffer.Length - this.z.AvailableBytesOut);
                        }

                        done = this.z.AvailableBytesIn == 0 && this.z.AvailableBytesOut != 0;

                        // If GZIP and de-compress, we're done when 8 bytes remain.
                        if (this.Flavor == ZlibStreamFlavor.Gzip)
                            done = (this.z.AvailableBytesIn == 8 && this.z.AvailableBytesOut != 0);
                    }
                    while (!done);

                    this.Flush();

                    // workitem 7159
                    if (this.Flavor == ZlibStreamFlavor.Gzip)
                    {
                        //Console.WriteLine("GZipStream: Last write");
                        throw new ZlibException("Writing with decompression is not supported.");
                    }
                    break;

                case StreamMode.Reader:
                    if (this.Flavor == ZlibStreamFlavor.Gzip)
                    {
                        // workitem 8501: handle edge case (decompress empty stream)
                        if (this.z.TotalBytesOut == 0L)
                            return;

                        // Read and potentially verify the GZIP trailer: CRC32 and  size mod 2^32
                        byte[] trailer = new byte[8];

                        if (this.z.AvailableBytesIn != 8)
                        {
                            throw new ZlibException(string.Format("Protocol error. AvailableBytesIn={0}, expected 8", this.z.AvailableBytesIn));
                        }

                        Array.Copy(this.z.InputBuffer, this.z.NextIn, trailer, 0, trailer.Length);

                        int crc32Expected = BitConverter.ToInt32(trailer, 0);
                        int crc32Actual = this.crc.Crc32Result;
                        int isizeExpected = BitConverter.ToInt32(trailer, 4);
                        int isizeActual = (int) (this.z.TotalBytesOut & 0x00000000FFFFFFFF);

                        // Console.WriteLine("GZipStream: slurped trailer  crc(0x{0:X8}) isize({1})", crc32_expected, isize_expected);
                        // Console.WriteLine("GZipStream: calc'd data      crc(0x{0:X8}) isize({1})", crc32_actual, isize_actual);

                        if (crc32Actual != crc32Expected)
                            throw new ZlibException(string.Format("Bad CRC32 in GZIP stream. (actual({0:X8})!=expected({1:X8}))", crc32Actual, crc32Expected));

                        if (isizeActual != isizeExpected)
                            throw new ZlibException(string.Format("Bad size in GZIP stream. (actual({0})!=expected({1}))", isizeActual, isizeExpected));
                    }
                    break;
            }
        }

        private void End()
        {
            if (this.Z == null)
                return;

            //if (_wantCompress)
            //{
            //    _z.EndDeflate();
            //}
            //else
            //{
            this.z.EndInflate();
            //}

            this.z = null;
        }

        public override void Close()
        {
            if (this.Stream == null)
                return;

            try
            {
                this.Finish();
            }
            finally
            {
                this.End();

                if (!this.LeaveOpen)
                    this.Stream.Close();

                this.Stream = null;
            }
        }

        public override void Flush()
        {
            this.Stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
            //_outStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.Stream.SetLength(value);
        }

#if NOT
        public int Read()
        {
            if (Read(_buf1, 0, 1) == 0)
                return 0;

            // calculate CRC after reading
            if (crc!=null)
                crc.SlurpBlock(_buf1,0,1);

            return (_buf1[0] & 0xFF);
        }
#endif

        private bool nomoreinput;

        private string ReadZeroTerminatedString()
        {
            List<byte> list = new List<byte>();
            bool done = false;

            do
            {
                // workitem 7740
                int n = this.Stream.Read(this.Buf1, 0, 1);

                if (n != 1)
                    throw new ZlibException("Unexpected EOF reading GZIP header.");

                if (this.Buf1[0] == 0)
                    done = true;
                else
                    list.Add(this.Buf1[0]);
            } while (!done);

            byte[] a = list.ToArray();

            return GZipStream.Iso8859Dash1.GetString(a, 0, a.Length);
        }

        private int _ReadAndValidateGzipHeader()
        {
            int totalBytesRead = 0;
            // read the header on the first read
            byte[] header = new byte[10];
            int n = this.Stream.Read(header, 0, header.Length);

            // workitem 8501: handle edge case (decompress empty stream)
            if (n == 0)
                return 0;

            if (n != 10)
                throw new ZlibException("Not a valid GZIP stream.");

            if (header[0] != 0x1F || header[1] != 0x8B || header[2] != 8)
                throw new ZlibException("Bad GZIP header.");

            int timet = BitConverter.ToInt32(header, 4);

            this.GzipMtime = GZipStream.UnixEpoch.AddSeconds(timet);
            totalBytesRead += n;

            if ((header[3] & 0x04) == 0x04)
            {
                // read and discard extra field
                n = this.Stream.Read(header, 0, 2); // 2-byte length field
                totalBytesRead += n;

                short extraLength = (short) (header[0] + header[1] * 256);
                byte[] extra = new byte[extraLength];

                n = this.Stream.Read(extra, 0, extra.Length);

                if (n != extraLength)
                    throw new ZlibException("Unexpected end-of-file reading GZIP header.");

                totalBytesRead += n;
            }

            if ((header[3] & 0x08) == 0x08)
                this.GzipFileName = this.ReadZeroTerminatedString();

            if ((header[3] & 0x10) == 0x010)
                this.GzipComment = this.ReadZeroTerminatedString();

            if ((header[3] & 0x02) == 0x02)
                this.Read(this.Buf1, 0, 1); // CRC16, ignore

            return totalBytesRead;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // According to MS documentation, any implementation of the IO.Stream.Read function must:
            // (a) throw an exception if offset & count reference an invalid part of the buffer,
            //     or if count < 0, or if buffer is null
            // (b) return 0 only upon EOF, or if count = 0
            // (c) if not EOF, then return at least 1 byte, up to <count> bytes

            if (this.streamMode == StreamMode.Undefined)
            {
                if (!this.Stream.CanRead)
                    throw new ZlibException("The stream is not readable.");

                // for the first read, set up some controls.
                this.streamMode = StreamMode.Reader;

                // (The first reference to _z goes through the private accessor which
                // may initialize it.)
                this.Z.AvailableBytesIn = 0;

                if (this.Flavor == ZlibStreamFlavor.Gzip)
                {
                    this.GzipHeaderByteCount = this._ReadAndValidateGzipHeader();
                    // workitem 8501: handle edge case (decompress empty stream)

                    if (this.GzipHeaderByteCount == 0)
                        return 0;
                }
            }

            if (this.streamMode != StreamMode.Reader)
                throw new ZlibException("Cannot Read after Writing.");

            if (count == 0)
                return 0;

            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            if (offset < buffer.GetLowerBound(0))
                throw new ArgumentOutOfRangeException("offset");

            if ((offset + count) > buffer.GetLength(0))
                throw new ArgumentOutOfRangeException("count");

            int rc;

            // set up the output of the deflate/inflate codec:
            this.z.OutputBuffer = buffer;
            this.z.NextOut = offset;
            this.z.AvailableBytesOut = count;

            // This is necessary in case _workingBuffer has been resized. (new byte[])
            // (The first reference to _workingBuffer goes through the private accessor which
            // may initialize it.)
            this.z.InputBuffer = this.WorkingBuffer;

            do
            {
                // need data in _workingBuffer in order to deflate/inflate.  Here, we check if we have any.
                if ((this.z.AvailableBytesIn == 0) && (!this.nomoreinput))
                {
                    // No data available, so try to Read data from the captive stream.
                    this.z.NextIn = 0;
                    this.z.AvailableBytesIn = this.Stream.Read(this.workingBuffer, 0, this.workingBuffer.Length);

                    if (this.z.AvailableBytesIn == 0)
                        this.nomoreinput = true;

                }
                // we have data in InputBuffer; now compress or decompress as appropriate
                //rc = (_wantCompress)
                //    ? _z.Deflate(_flushMode)
                //    : _z.Inflate(_flushMode);
                rc = this.z.Inflate(this.FlushMode);

                if (this.nomoreinput && (rc == ZlibConstants.Z_BUF_ERROR))
                    return 0;

                if (rc != ZlibConstants.Z_OK && rc != ZlibConstants.Z_STREAM_END)
                    throw new ZlibException(string.Format("inflating:  rc={0}  msg={1}", rc, this.z.Message));

                if ((this.nomoreinput || rc == ZlibConstants.Z_STREAM_END) && (this.z.AvailableBytesOut == count))
                    break; // nothing more to read
            }
            //while (_z.AvailableBytesOut == count && rc == ZlibConstants.Z_OK);
            while (this.z.AvailableBytesOut > 0 && !this.nomoreinput && rc == ZlibConstants.Z_OK);


            // workitem 8557
            // is there more room in output? 
            if (this.z.AvailableBytesOut > 0)
            {
                if (rc == ZlibConstants.Z_OK && this.z.AvailableBytesIn == 0)
                {
                    // deferred
                }

                // are we completely done reading?
                if (this.nomoreinput)
                {
                    // and in compression?
                    /*if (_wantCompress)
                    {
                        // no more input data available; therefore we flush to
                        // try to complete the read
                        rc = _z.Deflate(FlushType.Finish);

                        if (rc != ZlibConstants.Z_OK && rc != ZlibConstants.Z_STREAM_END)
                            throw new ZlibException(String.Format("Deflating:  rc={0}  msg={1}", rc, _z.Message));
                    }*/
                }
            }


            rc = (count - this.z.AvailableBytesOut);

            // calculate CRC after reading
            if (this.crc != null)
                this.crc.SlurpBlock(buffer, offset, rc);

            return rc;
        }

        public override bool CanRead
        {
            get { return this.Stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return this.Stream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return this.Stream.CanWrite; }
        }

        public override long Length
        {
            get { return this.Stream.Length; }
        }

        public override long Position
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        internal enum StreamMode
        {
            Writer,
            Reader,
            Undefined,
        }
    }
}

#endif
