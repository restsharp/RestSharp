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
using System.IO;

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
	internal class ZlibStream : System.IO.Stream
	{
		internal ZlibBaseStream _baseStream;
		bool _disposed;

		public ZlibStream(System.IO.Stream stream)
		{
			_baseStream = new ZlibBaseStream(stream, ZlibStreamFlavor.ZLIB, false);
		}

		#region Zlib properties

		/// <summary>
		/// This property sets the flush behavior on the stream.  
		/// Sorry, though, not sure exactly how to describe all the various settings.
		/// </summary>
		virtual public FlushType FlushMode
		{
			get { return (this._baseStream._flushMode); }
			set
			{
				if (_disposed) throw new ObjectDisposedException("ZlibStream");
				this._baseStream._flushMode = value;
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
			get
			{
				return this._baseStream._bufferSize;
			}
			set
			{
				if (_disposed) throw new ObjectDisposedException("ZlibStream");
				if (this._baseStream._workingBuffer != null)
					throw new ZlibException("The working buffer is already set.");
				if (value < ZlibConstants.WorkingBufferSizeMin)
					throw new ZlibException(String.Format("Don't be silly. {0} bytes?? Use a bigger buffer.", value));
				this._baseStream._bufferSize = value;
			}
		}

		/// <summary> Returns the total number of bytes input so far.</summary>
		virtual public long TotalIn
		{
			get { return this._baseStream._z.TotalBytesIn; }
		}

		/// <summary> Returns the total number of bytes output so far.</summary>
		virtual public long TotalOut
		{
			get { return this._baseStream._z.TotalBytesOut; }
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
				if (!_disposed)
				{
					if (disposing && (this._baseStream != null))
						this._baseStream.Close();
					_disposed = true;
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
				if (_disposed) throw new ObjectDisposedException("ZlibStream");
				return _baseStream._stream.CanRead;
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
				if (_disposed) throw new ObjectDisposedException("ZlibStream");
				return _baseStream._stream.CanWrite;
			}
		}

		/// <summary>
		/// Flush the stream.
		/// </summary>
		public override void Flush()
		{
			if (_disposed) throw new ObjectDisposedException("ZlibStream");
			_baseStream.Flush();
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
				if (this._baseStream._streamMode == ZlibBaseStream.StreamMode.Writer)
					return this._baseStream._z.TotalBytesOut;
				if (this._baseStream._streamMode == ZlibBaseStream.StreamMode.Reader)
					return this._baseStream._z.TotalBytesIn;
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
			if (_disposed) throw new ObjectDisposedException("ZlibStream");
			return _baseStream.Read(buffer, offset, count);
		}

		/// <summary>
		/// Calling this method always throws a NotImplementedException.
		/// </summary>
		public override long Seek(long offset, System.IO.SeekOrigin origin)
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
			if (_disposed) throw new ObjectDisposedException("ZlibStream");
			_baseStream.Write(buffer, offset, count);
		}
		#endregion


		/// <summary>
		/// Uncompress a byte array into a single string.
		/// </summary>
		/// <seealso cref="ZlibStream.CompressString(String)"/>
		/// <param name="compressed">
		/// A buffer containing ZLIB-compressed data.  
		/// </param>
		public static String UncompressString(byte[] compressed)
		{
			// workitem 8460
			byte[] working = new byte[1024];
			var encoding = System.Text.Encoding.UTF8;
			using (var output = new MemoryStream())
			{
				using (var input = new MemoryStream(compressed))
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
					var sr = new StreamReader(output, encoding);
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
			using (var output = new MemoryStream())
			{
				using (var input = new MemoryStream(compressed))
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


	internal enum ZlibStreamFlavor { ZLIB = 1950, DEFLATE = 1951, GZIP = 1952 }

	internal class ZlibBaseStream : System.IO.Stream
	{
		protected internal ZlibCodec _z = null; // deferred init... new ZlibCodec();

		protected internal StreamMode _streamMode = StreamMode.Undefined;
		protected internal FlushType _flushMode;
		protected internal ZlibStreamFlavor _flavor;
		protected internal bool _leaveOpen;
		protected internal byte[] _workingBuffer;
		protected internal int _bufferSize = ZlibConstants.WorkingBufferSizeDefault;
		protected internal byte[] _buf1 = new byte[1];

		protected internal System.IO.Stream _stream;

		// workitem 7159
		CRC32 crc;
		protected internal string _GzipFileName;
		protected internal string _GzipComment;
		protected internal DateTime _GzipMtime;
		protected internal int _gzipHeaderByteCount;

		internal int Crc32 { get { if (crc == null) return 0; return crc.Crc32Result; } }

		public ZlibBaseStream(System.IO.Stream stream, ZlibStreamFlavor flavor, bool leaveOpen)
			: base()
		{
			this._flushMode = FlushType.None;
			//this._workingBuffer = new byte[WORKING_BUFFER_SIZE_DEFAULT];
			this._stream = stream;
			this._leaveOpen = leaveOpen;
			this._flavor = flavor;
			// workitem 7159
			if (flavor == ZlibStreamFlavor.GZIP)
			{
				crc = new CRC32();
			}
		}

		private ZlibCodec z
		{
			get
			{
				if (_z == null)
				{
					bool wantRfc1950Header = (this._flavor == ZlibStreamFlavor.ZLIB);
					_z = new ZlibCodec();
					_z.InitializeInflate(wantRfc1950Header);
				}
				return _z;
			}
		}



		private byte[] workingBuffer
		{
			get
			{
				if (_workingBuffer == null)
					_workingBuffer = new byte[_bufferSize];
				return _workingBuffer;
			}
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



		public override void Write(System.Byte[] buffer, int offset, int count)
		{
			// workitem 7159
			// calculate the CRC on the unccompressed data  (before writing)
			if (crc != null)
				crc.SlurpBlock(buffer, offset, count);

			if (_streamMode == StreamMode.Undefined)
				_streamMode = StreamMode.Writer;
			else if (_streamMode != StreamMode.Writer)
				throw new ZlibException("Cannot Write after Reading.");

			if (count == 0)
				return;

			// first reference of z property will initialize the private var _z
			z.InputBuffer = buffer;
			_z.NextIn = offset;
			_z.AvailableBytesIn = count;
			bool done = false;
			do
			{
				_z.OutputBuffer = workingBuffer;
				_z.NextOut = 0;
				_z.AvailableBytesOut = _workingBuffer.Length;
				//int rc = (_wantCompress)
				//    ? _z.Deflate(_flushMode)
				//    : _z.Inflate(_flushMode);
				int rc = _z.Inflate(_flushMode);
				if (rc != ZlibConstants.Z_OK && rc != ZlibConstants.Z_STREAM_END)
					throw new ZlibException("inflating: " + _z.Message);

				_stream.Write(_workingBuffer, 0, _workingBuffer.Length - _z.AvailableBytesOut);

				done = _z.AvailableBytesIn == 0 && _z.AvailableBytesOut != 0;

				// If GZIP and de-compress, we're done when 8 bytes remain.
				if (_flavor == ZlibStreamFlavor.GZIP)
					done = (_z.AvailableBytesIn == 8 && _z.AvailableBytesOut != 0);

			}
			while (!done);
		}



		private void finish()
		{
			if (_z == null) return;

			if (_streamMode == StreamMode.Writer)
			{
				bool done = false;
				do
				{
					_z.OutputBuffer = workingBuffer;
					_z.NextOut = 0;
					_z.AvailableBytesOut = _workingBuffer.Length;
					//int rc = (_wantCompress)
					//    ? _z.Deflate(FlushType.Finish)
					//    : _z.Inflate(FlushType.Finish);
					int rc = _z.Inflate(FlushType.Finish);
					if (rc != ZlibConstants.Z_STREAM_END && rc != ZlibConstants.Z_OK)
						throw new ZlibException("inflating: " + _z.Message);

					if (_workingBuffer.Length - _z.AvailableBytesOut > 0)
					{
						_stream.Write(_workingBuffer, 0, _workingBuffer.Length - _z.AvailableBytesOut);
					}

					done = _z.AvailableBytesIn == 0 && _z.AvailableBytesOut != 0;
					// If GZIP and de-compress, we're done when 8 bytes remain.
					if (_flavor == ZlibStreamFlavor.GZIP)
						done = (_z.AvailableBytesIn == 8 && _z.AvailableBytesOut != 0);

				}
				while (!done);

				Flush();

				// workitem 7159
				if (_flavor == ZlibStreamFlavor.GZIP)
				{
					//Console.WriteLine("GZipStream: Last write");
					throw new ZlibException("Writing with decompression is not supported.");
				}
			}
			// workitem 7159
			else if (_streamMode == StreamMode.Reader)
			{
				if (_flavor == ZlibStreamFlavor.GZIP)
				{
					// workitem 8501: handle edge case (decompress empty stream)
					if (_z.TotalBytesOut == 0L)
						return;

					// Read and potentially verify the GZIP trailer: CRC32 and  size mod 2^32
					byte[] trailer = new byte[8];

					if (_z.AvailableBytesIn != 8)
						throw new ZlibException(String.Format("Protocol error. AvailableBytesIn={0}, expected 8",
							 _z.AvailableBytesIn));

					Array.Copy(_z.InputBuffer, _z.NextIn, trailer, 0, trailer.Length);

					Int32 crc32_expected = BitConverter.ToInt32(trailer, 0);
					int crc32_actual = crc.Crc32Result;
					Int32 isize_expected = BitConverter.ToInt32(trailer, 4);
					Int32 isize_actual = (Int32)(_z.TotalBytesOut & 0x00000000FFFFFFFF);

					// Console.WriteLine("GZipStream: slurped trailer  crc(0x{0:X8}) isize({1})", crc32_expected, isize_expected);
					// Console.WriteLine("GZipStream: calc'd data      crc(0x{0:X8}) isize({1})", crc32_actual, isize_actual);

					if (crc32_actual != crc32_expected)
						throw new ZlibException(String.Format("Bad CRC32 in GZIP stream. (actual({0:X8})!=expected({1:X8}))", crc32_actual, crc32_expected));

					if (isize_actual != isize_expected)
						throw new ZlibException(String.Format("Bad size in GZIP stream. (actual({0})!=expected({1}))", isize_actual, isize_expected));
				}
			}
		}


		private void end()
		{
			if (z == null)
				return;
			//if (_wantCompress)
			//{
			//    _z.EndDeflate();
			//}
			//else
			{
				_z.EndInflate();
			}
			_z = null;
		}


		public override void Close()
		{
			if (_stream == null) return;
			try
			{
				finish();
			}
			finally
			{
				end();
				if (!_leaveOpen) _stream.Close();
				_stream = null;
			}
		}

		public override void Flush()
		{
			_stream.Flush();
		}

		public override System.Int64 Seek(System.Int64 offset, System.IO.SeekOrigin origin)
		{
			throw new NotImplementedException();
			//_outStream.Seek(offset, origin);
		}
		public override void SetLength(System.Int64 value)
		{
			_stream.SetLength(value);
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

		private bool nomoreinput = false;



		private string ReadZeroTerminatedString()
		{
			var list = new System.Collections.Generic.List<byte>();
			bool done = false;
			do
			{
				// workitem 7740
				int n = _stream.Read(_buf1, 0, 1);
				if (n != 1)
					throw new ZlibException("Unexpected EOF reading GZIP header.");
				else
				{
					if (_buf1[0] == 0)
						done = true;
					else
						list.Add(_buf1[0]);
				}
			} while (!done);
			byte[] a = list.ToArray();
			return GZipStream.iso8859dash1.GetString(a, 0, a.Length);
		}


		private int _ReadAndValidateGzipHeader()
		{
			int totalBytesRead = 0;
			// read the header on the first read
			byte[] header = new byte[10];
			int n = _stream.Read(header, 0, header.Length);

			// workitem 8501: handle edge case (decompress empty stream)
			if (n == 0)
				return 0;

			if (n != 10)
				throw new ZlibException("Not a valid GZIP stream.");

			if (header[0] != 0x1F || header[1] != 0x8B || header[2] != 8)
				throw new ZlibException("Bad GZIP header.");

			Int32 timet = BitConverter.ToInt32(header, 4);
			_GzipMtime = GZipStream._unixEpoch.AddSeconds(timet);
			totalBytesRead += n;
			if ((header[3] & 0x04) == 0x04)
			{
				// read and discard extra field
				n = _stream.Read(header, 0, 2); // 2-byte length field
				totalBytesRead += n;

				Int16 extraLength = (Int16)(header[0] + header[1] * 256);
				byte[] extra = new byte[extraLength];
				n = _stream.Read(extra, 0, extra.Length);
				if (n != extraLength)
					throw new ZlibException("Unexpected end-of-file reading GZIP header.");
				totalBytesRead += n;
			}
			if ((header[3] & 0x08) == 0x08)
				_GzipFileName = ReadZeroTerminatedString();
			if ((header[3] & 0x10) == 0x010)
				_GzipComment = ReadZeroTerminatedString();
			if ((header[3] & 0x02) == 0x02)
				Read(_buf1, 0, 1); // CRC16, ignore

			return totalBytesRead;
		}



		public override System.Int32 Read(System.Byte[] buffer, System.Int32 offset, System.Int32 count)
		{
			// According to MS documentation, any implementation of the IO.Stream.Read function must:
			// (a) throw an exception if offset & count reference an invalid part of the buffer,
			//     or if count < 0, or if buffer is null
			// (b) return 0 only upon EOF, or if count = 0
			// (c) if not EOF, then return at least 1 byte, up to <count> bytes

			if (_streamMode == StreamMode.Undefined)
			{
				if (!this._stream.CanRead) throw new ZlibException("The stream is not readable.");
				// for the first read, set up some controls.
				_streamMode = StreamMode.Reader;
				// (The first reference to _z goes through the private accessor which
				// may initialize it.)
				z.AvailableBytesIn = 0;
				if (_flavor == ZlibStreamFlavor.GZIP)
				{
					_gzipHeaderByteCount = _ReadAndValidateGzipHeader();
					// workitem 8501: handle edge case (decompress empty stream)
					if (_gzipHeaderByteCount == 0)
						return 0;
				}
			}

			if (_streamMode != StreamMode.Reader)
				throw new ZlibException("Cannot Read after Writing.");

			if (count == 0) return 0;
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			if (offset < buffer.GetLowerBound(0)) throw new ArgumentOutOfRangeException("offset");
			if ((offset + count) > buffer.GetLength(0)) throw new ArgumentOutOfRangeException("count");

			int rc = 0;

			// set up the output of the deflate/inflate codec:
			_z.OutputBuffer = buffer;
			_z.NextOut = offset;
			_z.AvailableBytesOut = count;

			// This is necessary in case _workingBuffer has been resized. (new byte[])
			// (The first reference to _workingBuffer goes through the private accessor which
			// may initialize it.)
			_z.InputBuffer = workingBuffer;

			do
			{
				// need data in _workingBuffer in order to deflate/inflate.  Here, we check if we have any.
				if ((_z.AvailableBytesIn == 0) && (!nomoreinput))
				{
					// No data available, so try to Read data from the captive stream.
					_z.NextIn = 0;
					_z.AvailableBytesIn = _stream.Read(_workingBuffer, 0, _workingBuffer.Length);
					if (_z.AvailableBytesIn == 0)
						nomoreinput = true;

				}
				// we have data in InputBuffer; now compress or decompress as appropriate
				//rc = (_wantCompress)
				//    ? _z.Deflate(_flushMode)
				//    : _z.Inflate(_flushMode);
				rc = _z.Inflate(_flushMode);
				if (nomoreinput && (rc == ZlibConstants.Z_BUF_ERROR))
					return 0;

				if (rc != ZlibConstants.Z_OK && rc != ZlibConstants.Z_STREAM_END)
					throw new ZlibException(String.Format("inflating:  rc={0}  msg={1}", rc, _z.Message));

				if ((nomoreinput || rc == ZlibConstants.Z_STREAM_END) && (_z.AvailableBytesOut == count))
					break; // nothing more to read
			}
			//while (_z.AvailableBytesOut == count && rc == ZlibConstants.Z_OK);
			while (_z.AvailableBytesOut > 0 && !nomoreinput && rc == ZlibConstants.Z_OK);


			// workitem 8557
			// is there more room in output? 
			if (_z.AvailableBytesOut > 0)
			{
				if (rc == ZlibConstants.Z_OK && _z.AvailableBytesIn == 0)
				{
					// deferred
				}

				// are we completely done reading?
				if (nomoreinput)
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


			rc = (count - _z.AvailableBytesOut);

			// calculate CRC after reading
			if (crc != null)
				crc.SlurpBlock(buffer, offset, rc);

			return rc;
		}



		public override System.Boolean CanRead
		{
			get { return this._stream.CanRead; }
		}

		public override System.Boolean CanSeek
		{
			get { return this._stream.CanSeek; }
		}

		public override System.Boolean CanWrite
		{
			get { return this._stream.CanWrite; }
		}

		public override System.Int64 Length
		{
			get { return _stream.Length; }
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