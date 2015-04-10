// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RebufferableBinaryReader.cs" company="Jake Woods">
//   Copyright (c) 2013 Jake Woods
//   
//   Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
//   and associated documentation files (the "Software"), to deal in the Software without restriction, 
//   including without limitation the rights to use, copy, modify, merge, publish, distribute, 
//   sublicense, and/or sell copies of the Software, and to permit persons to whom the Software 
//   is furnished to do so, subject to the following conditions:
//   
//   The above copyright notice and this permission notice shall be included in all copies 
//   or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//   INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
//   PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR 
//   ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
//   ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// <author>Jake Woods</author>
// <summary>
//   Provides methods to interpret and read a stream as either character or binary
//   data similar to a  and provides the ability to push
//   data onto the front of the stream.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace HttpMultipartParser
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    ///     Provides methods to interpret and read a stream as either character or binary
    ///     data similar to a <see cref="BinaryReader" /> and provides the ability to push
    ///     data onto the front of the stream.
    /// </summary>
    internal class RebufferableBinaryReader : IDisposable
    {
        #region Fields

        /// <summary>
        ///     The size of the buffer to use when reading new data.
        /// </summary>
        private readonly int bufferSize;

        /// <summary>
        ///     The encoding to use for character based operations
        /// </summary>
        private readonly Encoding encoding;

        /// <summary>
        ///     The stream to read raw data from.
        /// </summary>
        private readonly Stream stream;

        /// <summary>
        ///     The stream stack to store buffered data.
        /// </summary>
        private readonly BinaryStreamStack streamStack;

        /// <summary>
        ///     Determines if we have run out of data to read or not.
        /// </summary>
        private bool done;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RebufferableBinaryReader"/> class.
        ///     Default encoding of UTF8 will be used.
        /// </summary>
        /// <param name="input">
        /// The input stream to read from.
        /// </param>
        public RebufferableBinaryReader(Stream input)
            : this(input, new UTF8Encoding(false))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RebufferableBinaryReader"/> class.
        /// </summary>
        /// <param name="input">
        /// The input stream to read from.
        /// </param>
        /// <param name="encoding">
        /// The encoding to use for character based operations.
        /// </param>
        public RebufferableBinaryReader(Stream input, Encoding encoding)
            : this(input, encoding, 4096)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RebufferableBinaryReader"/> class.
        /// </summary>
        /// <param name="input">
        /// The input stream to read from.
        /// </param>
        /// <param name="encoding">
        /// The encoding to use for character based operations.
        /// </param>
        /// <param name="bufferSize">
        /// The buffer size to use for new buffers.
        /// </param>
        public RebufferableBinaryReader(Stream input, Encoding encoding, int bufferSize)
        {
            this.done = false;
            this.stream = input;
            this.streamStack = new BinaryStreamStack(encoding);
            this.encoding = encoding;
            this.bufferSize = bufferSize;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Adds data to the front of the stream. The most recently buffered data will
        ///     be read first.
        /// </summary>
        /// <param name="data">
        /// The data to buffer.
        /// </param>
        public void Buffer(byte[] data)
        {
            this.streamStack.Push(data);
        }

        /// <summary>
        /// Adds the string to the front of the stream. The most recently buffered data will
        ///     be read first.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        public void Buffer(string data)
        {
            this.streamStack.Push(this.encoding.GetBytes(data));
        }

        /// <summary>
        ///     Closes the stream.
        /// </summary>
        public void Dispose()
        {
            this.stream.Close();
        }

        /// <summary>
        ///     Reads a single byte as an integer from the stream. Returns -1 if no
        ///     data is left to read.
        /// </summary>
        /// <returns>
        ///     The <see cref="byte" /> that was read.
        /// </returns>
        public int Read()
        {
            int value = -1;
            while (value == -1)
            {
                if (!this.streamStack.HasData())
                {
                    if (this.StreamData() == 0)
                    {
                        this.done = true;
                        return -1;
                    }
                }

                value = this.streamStack.Read();
            }

            return value;
        }

        /// <summary>
        /// Reads the specified number of bytes from the stream, starting from a
        ///     specified point in the byte array.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to read data into.
        /// </param>
        /// <param name="index">
        /// The index of buffer to start reading into.
        /// </param>
        /// <param name="count">
        /// The number of bytes to read into the buffer.
        /// </param>
        /// <returns>
        /// The number of bytes read into buffer. This might be less than the number of bytes requested if that many bytes are not available,
        ///     or it might be zero if the end of the stream is reached.
        /// </returns>
        public int Read(byte[] buffer, int index, int count)
        {
            int amountRead = 0;
            while (amountRead < count)
            {
                if (!this.streamStack.HasData())
                {
                    if (this.StreamData() == 0)
                    {
                        this.done = true;
                        return amountRead;
                    }
                }

                amountRead += this.streamStack.Read(buffer, index + amountRead, count - amountRead);
            }

            return amountRead;
        }

        /// <summary>
        /// Reads the specified number of characters from the stream, starting from a
        ///     specified point in the byte array.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to read data into.
        /// </param>
        /// <param name="index">
        /// The index of buffer to start reading into.
        /// </param>
        /// <param name="count">
        /// The number of characters to read into the buffer.
        /// </param>
        /// <returns>
        /// The number of characters read into buffer. This might be less than the number of
        ///     characters requested if that many characters are not available,
        ///     or it might be zero if the end of the stream is reached.
        /// </returns>
        public int Read(char[] buffer, int index, int count)
        {
            int amountRead = 0;
            while (amountRead < count)
            {
                if (!this.streamStack.HasData())
                {
                    if (this.StreamData() == 0)
                    {
                        this.done = true;
                        return amountRead;
                    }
                }

                amountRead += this.streamStack.Read(buffer, index + amountRead, count - amountRead);
            }

            return amountRead;
        }

        /// <summary>
        ///     Reads a series of bytes delimited by the byte encoding of newline for this platform.
        ///     the newline bytes will not be included in the return data.
        /// </summary>
        /// <returns>
        ///     A byte array containing all the data up to but not including the next newline in the stack.
        /// </returns>
        public byte[] ReadByteLine()
        {
            var builder = new MemoryStream();
            while (true)
            {
                if (!this.streamStack.HasData())
                {
                    if (this.StreamData() == 0)
                    {
                        this.done = true;
                        return builder.ToArray();
                    }
                }

                bool hitStreamEnd;
                byte[] line = this.streamStack.ReadByteLine(out hitStreamEnd);
                builder.Write(line, 0, line.Length);
                if (!hitStreamEnd)
                {
                    return builder.ToArray();
                }
            }
        }

        /// <summary>
        ///     Reads a line from the stack delimited by the newline for this platform. The newline
        ///     characters will not be included in the stream
        /// </summary>
        /// <returns>
        ///     The <see cref="string" /> containing the line.
        /// </returns>
        public string ReadLine()
        {
            byte[] data = this.ReadByteLine();
            return this.encoding.GetString(data);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines the byte order marking offset (if any) from the
        ///     given buffer.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to examine.
        /// </param>
        /// <returns>
        /// The <see cref="int"/> representing the length of the byte order marking.
        /// </returns>
        private int GetBomOffset(byte[] buffer)
        {
            byte[] bom = this.encoding.GetPreamble();
            bool usesBom = true;
            for (int i = 0; i < bom.Length; ++i)
            {
                if (bom[i] != buffer[i])
                {
                    usesBom = false;
                }
            }

            return usesBom ? bom.Length : 0;
        }

        /// <summary>
        ///     Reads more data from the stream into the stream stack.
        /// </summary>
        /// <returns>
        ///     The number of bytes read into the stream stack as an <see cref="int" />
        /// </returns>
        private int StreamData()
        {
            var buffer = new byte[this.bufferSize];
            int amountRead = this.stream.Read(buffer, 0, buffer.Length);

            // We need to check if our stream is using our encodings
            // BOM, if it is we need to jump it.
            int bomOffset = this.GetBomOffset(buffer);

            // Sometimes we'll get a buffer that's smaller then we expect, chop it down
            // for the reader:
            if (amountRead - bomOffset > 0)
            {
                if (amountRead != buffer.Length || bomOffset > 0)
                {
                    var smallBuffer = new byte[amountRead - bomOffset];
                    System.Buffer.BlockCopy(buffer, bomOffset, smallBuffer, 0, amountRead - bomOffset);
                    this.streamStack.Push(smallBuffer);
                }
                else
                {
                    this.streamStack.Push(buffer);
                }
            }

            return amountRead;
        }

        #endregion
    }
}