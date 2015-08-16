// GZipStream.cs
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
// Time-stamp: <2009-August-12 15:35:30>
//
// ------------------------------------------------------------------
//
// This module defines the GZipStream class, which can be used as a replacement for
// the System.IO.Compression.GZipStream class in the .NET BCL.  NB: The design is not
// completely OO clean: there is some intelligence in the ZlibBaseStream that reads the 
// GZip header.  
//
// ------------------------------------------------------------------


#if WINDOWS_PHONE

using System;
using System.IO;
using System.Text;

namespace RestSharp.Compression.ZLib
{
    /// <summary>
    /// A class for compressing and decompressing GZIP streams.
    /// </summary>
    /// <remarks>
    ///
    /// <para>
    /// The GZipStream is a <see
    /// href="http://en.wikipedia.org/wiki/Decorator_pattern">Decorator</see> on a <see 
    /// cref="Stream"/>.  It adds GZIP compression or decompression to any stream.
    /// </para>
    ///
    /// <para> Like the <c>Compression.GZipStream</c> in the .NET Base
    /// Class Library, the Ionic.Zlib.GZipStream can compress while writing, or decompress
    /// while reading, but not vice versa.  The compression method used is GZIP, which is
    /// documented in <see href="http://www.ietf.org/rfc/rfc1952.txt">IETF RFC 1952</see>,
    /// "GZIP file format specification version 4.3".</para>
    ///
    /// <para> A GZipStream can be used to decompress data (through Read()) or to compress
    /// data (through Write()), but not both.  </para>
    ///
    /// <para> If you wish to use the GZipStream to compress data, you must wrap it around a
    /// write-able stream. As you call Write() on the GZipStream, the data will be
    /// compressed into the GZIP format.  If you want to decompress data, you must wrap the
    /// GZipStream around a readable stream that contains an IETF RFC 1952-compliant stream.
    /// The data will be decompressed as you call Read() on the GZipStream.  </para>
    ///
    /// <para> Though the GZIP format allows data from multiple files to be concatenated
    /// together, this stream handles only a single segment of GZIP format, typically
    /// representing a single file.  </para>
    ///
    /// <para>
    /// This class is similar to <see cref="ZlibStream"/> and <see cref="DeflateStream"/>.
    /// <c>ZlibStream</c> handles RFC1950-compliant streams.  <see cref="DeflateStream"/>
    /// handles RFC1951-compliant streams. This class handles RFC1952-compliant streams.
    /// </para>
    ///
    /// </remarks>
    ///
    /// <seealso cref="DeflateStream" />
    /// <seealso cref="ZlibStream" />
    internal class GZipStream : Stream
    {
        // GZip format
        // source: http://tools.ietf.org/html/rfc1952
        // 
        //  header id:           2 bytes    1F 8B
        //  compress method      1 byte     8= DEFLATE (none other supported)
        //  flag                 1 byte     bitfield (See below)
        //  mtime                4 bytes    time_t (seconds since jan 1, 1970 UTC of the file.
        //  xflg                 1 byte     2 = max compress used , 4 = max speed (can be ignored)
        //  OS                   1 byte     OS for originating archive. set to 0xFF in compression. 
        //  extra field length   2 bytes    optional - only if FEXTRA is set.
        //  extra field          varies     
        //  filename             varies     optional - if FNAME is set.  zero terminated. ISO-8859-1.
        //  file comment         varies     optional - if FCOMMENT is set. zero terminated. ISO-8859-1.
        //  crc16                1 byte     optional - present only if FHCRC bit is set
        //  compressed data      varies
        //  CRC32                4 bytes   
        //  isize                4 bytes    data size modulo 2^32
        // 
        //     FLG (FLaGs)
        //                bit 0   FTEXT - indicates file is ASCII text (can be safely ignored)
        //                bit 1   FHCRC - there is a CRC16 for the header immediately following the header
        //                bit 2   FEXTRA - extra fields are present
        //                bit 3   FNAME - the zero-terminated filename is present. encoding; ISO-8859-1.
        //                bit 4   FCOMMENT  - a zero-terminated file comment is present. encoding: ISO-8859-1
        //                bit 5   reserved
        //                bit 6   reserved
        //                bit 7   reserved
        // 
        // On consumption: 
        // Extra field is a bunch of nonsense and can be safely ignored. 
        // Header CRC and OS, likewise.
        // 
        // on generation:
        // all optional fields get 0, except for the OS, which gets 255.
        // 

        /// <summary>
        /// The Comment on the GZIP stream.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The GZIP format allows for each file to optionally have an associated comment stored with the
        /// file.  The comment is encoded with the ISO-8859-1 code page.  To include a comment in
        /// a GZIP stream you create, set this property before calling Write() for the first time
        /// on the GZipStream.  
        /// </para>
        ///
        /// <para>
        /// When using GZipStream to decompress, you can retrieve this property after the first
        /// call to Read().  If no comment has been set in the GZIP bytestream, the Comment
        /// property will return null (Nothing in VB).
        /// </para>
        /// </remarks>
        public string Comment
        {
            get { return this.comment; }
            set
            {
                if (this.disposed)
                    throw new ObjectDisposedException("GZipStream");

                this.comment = value;
            }
        }

        /// <summary>
        /// The FileName for the GZIP stream.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The GZIP format optionally allows each file to have an associated filename.  When
        /// compressing data (through Write()), set this FileName before calling Write() the first
        /// time on the GZipStream.  The actual filename is encoded into the GZIP bytestream with
        /// the ISO-8859-1 code page, according to RFC 1952. It is the application's responsibility to 
        /// insure that the FileName can be encoded and decoded correctly with this code page. 
        /// </para>
        /// <para>
        /// When decompressing (through Read()), you can retrieve this value any time after the
        /// first Read().  In the case where there was no filename encoded into the GZIP
        /// bytestream, the property will return null (Nothing in VB).
        /// </para>
        /// </remarks>
        public string FileName
        {
            get { return this.fileName; }
            set
            {
                if (this.disposed)
                    throw new ObjectDisposedException("GZipStream");

                this.fileName = value;

                if (this.fileName == null)
                    return;

                if (this.fileName.IndexOf("/") != -1)
                {
                    this.fileName = this.fileName.Replace("/", "\\");
                }

                if (this.fileName.EndsWith("\\"))
                    throw new Exception("Illegal filename");

                if (this.fileName.IndexOf("\\") != -1)
                {
                    // trim any leading path
                    this.fileName = Path.GetFileName(this.fileName);
                }
            }
        }

        // / <summary>
        // / The last modified time for the GZIP stream.
        // / </summary>
        // /
        // / <remarks> GZIP allows the storage of a last modified time with each GZIP entry.
        // / When compressing data, you can set this before the first call to Write().  When
        // / decompressing, you can retrieve this value any time after the first call to
        // / Read().  </remarks>
        //public DateTime? LastModified;

        /// <summary>
        /// The CRC on the GZIP stream. 
        /// </summary>
        /// <remarks>
        /// This is used for internal error checking. You probably don't need to look at this property.
        /// </remarks>
        public int Crc32 { get; private set; }

        internal ZlibBaseStream BaseStream;
        bool disposed;
        bool firstReadDone;
        string fileName;
        string comment;

        /// <summary>
        /// Create a GZipStream using the specified CompressionMode and the specified CompressionLevel,
        /// and explicitly specify whether the stream should be left open after Deflation or Inflation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This constructor allows the application to request that the captive stream remain open after
        /// the deflation or inflation occurs.  By default, after Close() is called on the stream, the 
        /// captive stream is also closed. In some cases this is not desired, for example if the stream 
        /// is a memory stream that will be re-read after compressed data has been written to it.  Specify true for the 
        /// leaveOpen parameter to leave the stream open. 
        /// </para>
        /// <para>
        /// As noted in the class documentation, 
        /// the CompressionMode (Compress or Decompress) also establishes the "direction" of the stream.  
        /// A GZipStream with CompressionMode.Compress works only through Write().  A GZipStream with 
        /// CompressionMode.Decompress works only through Read().
        /// </para>
        /// </remarks>
        /// <example>
        /// This example shows how to use a DeflateStream to compress data.
        /// <code>
        /// using (System.IO.Stream input = System.IO.File.OpenRead(fileToCompress))
        /// {
        ///     using (var raw = System.IO.File.Create(outputFile))
        ///     {
        ///         using (Stream compressor = new GZipStream(raw, CompressionMode.Compress, CompressionLevel.BestCompression, true))
        ///         {
        ///             byte[] buffer = new byte[WORKING_BUFFER_SIZE];
        ///             int n;
        ///             while ((n= input.Read(buffer, 0, buffer.Length)) != 0)
        ///             {
        ///                 compressor.Write(buffer, 0, n);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// <code lang="VB">
        /// Dim outputFile As String = (fileToCompress &amp; ".compressed")
        /// Using input As Stream = File.OpenRead(fileToCompress)
        ///     Using raw As FileStream = File.Create(outputFile)
        ///     Using compressor As Stream = New GZipStream(raw, CompressionMode.Compress, CompressionLevel.BestCompression, True)
        ///         Dim buffer As Byte() = New Byte(4096) {}
        ///         Dim n As Integer = -1
        ///         Do While (n &lt;&gt; 0)
        ///             If (n &gt; 0) Then
        ///                 compressor.Write(buffer, 0, n)
        ///             End If
        ///             n = input.Read(buffer, 0, buffer.Length)
        ///         Loop
        ///     End Using
        ///     End Using
        /// End Using
        /// </code>
        /// </example>
        /// <param name="stream">The stream which will be read or written.</param>
        /// <param name="mode">Indicates whether the GZipStream will compress or decompress.</param>
        /// <param name="leaveOpen">true if the application would like the stream to remain open after inflation/deflation.</param>
        /// <param name="level">A tuning knob to trade speed for effectiveness.</param>
        public GZipStream(Stream stream)
        {
            this.BaseStream = new ZlibBaseStream(stream, ZlibStreamFlavor.Gzip, false);
        }

        #region Zlib properties

        /// <summary>
        /// This property sets the flush behavior on the stream.  
        /// </summary>
        virtual public FlushType FlushMode
        {
            get { return (this.BaseStream.FlushMode); }
            set
            {
                if (this.disposed)
                    throw new ObjectDisposedException("GZipStream");

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
                    throw new ObjectDisposedException("GZipStream");

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

        #region Stream methods

        /// <summary>
        /// Dispose the stream.  
        /// </summary>
        /// <remarks>
        /// This may or may not result in a Close() call on the captive stream. 
        /// See the ctor's with leaveOpen parameters for more information.
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!this.disposed)
                {
                    if (disposing && (this.BaseStream != null))
                    {
                        this.BaseStream.Close();
                        this.Crc32 = this.BaseStream.Crc32;
                    }

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
                    throw new ObjectDisposedException("GZipStream");

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
                    throw new ObjectDisposedException("GZipStream");

                return this.BaseStream.Stream.CanWrite;
            }
        }

        /// <summary>
        /// Flush the stream.
        /// </summary>
        public override void Flush()
        {
            if (this.disposed)
                throw new ObjectDisposedException("GZipStream");

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
                if (this.BaseStream.streamMode == ZlibBaseStream.StreamMode.Reader)
                    return this.BaseStream.z.TotalBytesIn + this.BaseStream.GzipHeaderByteCount;

                return 0;
            }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Read and decompress data from the source stream.
        /// </summary>
        /// <remarks>
        /// With a GZipStream, decompression is done through reading.
        /// </remarks>
        /// <example>
        /// <code>
        /// byte[] working = new byte[WORKING_BUFFER_SIZE];
        /// using (System.IO.Stream input = System.IO.File.OpenRead(_CompressedFile))
        /// {
        ///     using (Stream decompressor= new Ionic.Zlib.GZipStream(input, CompressionMode.Decompress, true))
        ///     {
        ///         using (var output = System.IO.File.Create(_DecompressedFile))
        ///         {
        ///             int n;
        ///             while ((n= decompressor.Read(working, 0, working.Length)) !=0)
        ///             {
        ///                 output.Write(working, 0, n);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <param name="buffer">The buffer into which the decompressed data should be placed.</param>
        /// <param name="offset">the offset within that data array to put the first byte read.</param>
        /// <param name="count">the number of bytes to read.</param>
        /// <returns>the number of bytes actually read</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this.disposed)
                throw new ObjectDisposedException("GZipStream");

            int n = this.BaseStream.Read(buffer, offset, count);

            // Console.WriteLine("GZipStream::Read(buffer, off({0}), c({1}) = {2}", offset, count, n);
            // Console.WriteLine( Util.FormatByteArray(buffer, offset, n) );

            if (!this.firstReadDone)
            {
                this.firstReadDone = true;
                this.FileName = this.BaseStream.GzipFileName;
                this.Comment = this.BaseStream.GzipComment;
            }

            return n;
        }

        /// <summary>
        /// Calling this method always throws a <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="offset">irrelevant; it will always throw!</param>
        /// <param name="origin">irrelevant; it will always throw!</param>
        /// <returns>irrelevant!</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calling this method always throws a NotImplementedException.
        /// </summary>
        /// <param name="value">irrelevant; this method will always throw!</param>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        #endregion

        internal static DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal static Encoding Iso8859Dash1 = Encoding.GetEncoding("iso-8859-1");

        //private int EmitHeader()
        //{
        //    byte[] commentBytes = (Comment == null) ? null : Iso8859Dash1.GetBytes(Comment);
        //    byte[] filenameBytes = (FileName == null) ? null : Iso8859Dash1.GetBytes(FileName);

        //    int cbLength = (Comment == null) ? 0 : commentBytes.Length + 1;
        //    int fnLength = (FileName == null) ? 0 : filenameBytes.Length + 1;

        //    int bufferLength = 10 + cbLength + fnLength;
        //    byte[] header = new byte[bufferLength];
        //    int i = 0;

        //    // ID
        //    header[i++] = 0x1F;
        //    header[i++] = 0x8B;

        //    // compression method
        //    header[i++] = 8;

        //    byte flag = 0;

        //    if (Comment != null)
        //        flag ^= 0x10;

        //    if (FileName != null)
        //        flag ^= 0x8;

        //    // flag
        //    header[i++] = flag;

        //    // mtime
        //    if (!LastModified.HasValue)
        //        LastModified = DateTime.Now;

        //    TimeSpan delta = LastModified.Value - UnixEpoch;
        //    int timet = (int) delta.TotalSeconds;
        //    Array.Copy(BitConverter.GetBytes(timet), 0, header, i, 4);

        //    i += 4;

        //    // xflg
        //    header[i++] = 0;    // this field is totally useless
        //    // OS
        //    header[i++] = 0xFF; // 0xFF == unspecified

        //    // extra field length - only if FEXTRA is set, which it is not.
        //    //header[i++]= 0;
        //    //header[i++]= 0;

        //    // filename
        //    if (fnLength != 0)
        //    {
        //        Array.Copy(filenameBytes, 0, header, i, fnLength - 1);

        //        i += fnLength - 1;
        //        header[i++] = 0; // terminate
        //    }

        //    // comment
        //    if (cbLength != 0)
        //    {
        //        Array.Copy(commentBytes, 0, header, i, cbLength - 1);

        //        i += cbLength - 1;
        //        header[i++] = 0; // terminate
        //    }

        //    BaseStream.stream.Write(header, 0, header.Length);

        //    return header.Length; // bytes written
        //}

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}

#endif
