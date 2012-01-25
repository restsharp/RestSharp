// Zlib.cs
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
// Time-stamp: <2009-August-14 09:51:43>
//
// ------------------------------------------------------------------
//
// This module defines classes for ZLIB compression and
// decompression. This code is derived from the jzlib implementation of
// zlib, but significantly modified.  The object model is not the same,
// and many of the behaviors are new or different.  Nonetheless, in
// keeping with the license for jzlib, the copyright to that code is
// included below.
//
// ------------------------------------------------------------------
// 
// Copyright (c) 2000,2001,2002,2003 ymnk, JCraft,Inc. All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright 
// notice, this list of conditions and the following disclaimer in 
// the documentation and/or other materials provided with the distribution.
// 
// 3. The names of the authors may not be used to endorse or promote products
// derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL JCRAFT,
// INC. OR ANY CONTRIBUTORS TO THIS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
// OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 
// -----------------------------------------------------------------------
//
// This program is based on zlib-1.1.3; credit to authors
// Jean-loup Gailly(jloup@gzip.org) and Mark Adler(madler@alumni.caltech.edu)
// and contributors of zlib.
//
// -----------------------------------------------------------------------

#if WINDOWS_PHONE

using System;
using Interop = System.Runtime.InteropServices;

namespace RestSharp.Compression.ZLib
{
	/// <summary>
	/// A general purpose exception class for exceptions in the Zlib library.
	/// </summary>
	internal class ZlibException : System.Exception
	{
		/// <summary>
		/// The ZlibException class captures exception information generated
		/// by the Zlib library. 
		/// </summary>
		public ZlibException()
			: base()
		{
		}

		/// <summary>
		/// This ctor collects a message attached to the exception.
		/// </summary>
		/// <param name="s"></param>
		public ZlibException(System.String s)
			: base(s)
		{
		}
	}


	internal class SharedUtils
	{
		/// <summary>
		/// Performs an unsigned bitwise right shift with the specified number
		/// </summary>
		/// <param name="number">Number to operate on</param>
		/// <param name="bits">Ammount of bits to shift</param>
		/// <returns>The resulting number from the shift operation</returns>
		public static int URShift(int number, int bits)
		{
			return (int)((uint)number >> bits);
		}

		/// <summary>
		/// Performs an unsigned bitwise right shift with the specified number
		/// </summary>
		/// <param name="number">Number to operate on</param>
		/// <param name="bits">Ammount of bits to shift</param>
		/// <returns>The resulting number from the shift operation</returns>
		public static long URShift(long number, int bits)
		{
			return (long)((UInt64)number >> bits);
		}


#if NOTUSED
        
        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        public static int URShift(int number, long bits)
        {
            return URShift(number, (int)bits);
        }

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        public static long URShift(long number, long bits)
        {
            return URShift(number, (int)bits);
        }

#endif

#if POINTLESS        
        /*******************************/
        /// <summary>Reads a number of characters from the current source Stream and writes the data to the target array at the specified index.</summary>
        /// <param name="sourceStream">The source Stream to read from.</param>
        /// <param name="target">Contains the array of characteres read from the source Stream.</param>
        /// <param name="start">The starting index of the target array.</param>
        /// <param name="count">The maximum number of characters to read from the source Stream.</param>
        /// <returns>The number of characters read. The number will be less than or equal to count depending on the data available in the source Stream. Returns -1 if the end of the stream is reached.</returns>
        public static System.Int32 ReadInput(System.IO.Stream sourceStream, byte[] target, int start, int count)
        {
            // Returns 0 bytes if not enough space in target
            if (target.Length == 0)
                return 0;

            if (count == 0) return 0;

            // why double-buffer?
            //byte[] receiver = new byte[target.Length];
            int bytesRead = sourceStream.Read(target, start, count);

            // Returns -1 if EOF
            //if (bytesRead == 0)
            //    return -1;

            //for (int i = start; i < start + bytesRead; i++)
            //    target[i] = (byte)receiver[i];

            return bytesRead;
        }
#endif


		/// <summary>Reads a number of characters from the current source TextReader and writes the data to the target array at the specified index.</summary>
		/// <param name="sourceTextReader">The source TextReader to read from</param>
		/// <param name="target">Contains the array of characteres read from the source TextReader.</param>
		/// <param name="start">The starting index of the target array.</param>
		/// <param name="count">The maximum number of characters to read from the source TextReader.</param>
		/// <returns>The number of characters read. The number will be less than or equal to count depending on the data available in the source TextReader. Returns -1 if the end of the stream is reached.</returns>
		public static System.Int32 ReadInput(System.IO.TextReader sourceTextReader, byte[] target, int start, int count)
		{
			// Returns 0 bytes if not enough space in target
			if (target.Length == 0) return 0;

			char[] charArray = new char[target.Length];
			int bytesRead = sourceTextReader.Read(charArray, start, count);

			// Returns -1 if EOF
			if (bytesRead == 0) return -1;

			for (int index = start; index < start + bytesRead; index++)
				target[index] = (byte)charArray[index];

			return bytesRead;
		}


		internal static byte[] ToByteArray(System.String sourceString)
		{
			return System.Text.UTF8Encoding.UTF8.GetBytes(sourceString);
		}


		internal static char[] ToCharArray(byte[] byteArray)
		{
			return System.Text.UTF8Encoding.UTF8.GetChars(byteArray);
		}

	}

	/// <summary>
	/// Computes an Adler-32 checksum. 
	/// </summary>
	/// <remarks>
	/// The Adler checksum is similar to a CRC checksum, but faster to compute, though less
	/// reliable.  It is used in producing RFC1950 compressed streams.  The Adler checksum
	/// is a required part of the "ZLIB" standard.  Applications will almost never need to
	/// use this class directly.
	/// </remarks>
	internal sealed class Adler
	{
		// largest prime smaller than 65536
		private static int BASE = 65521;
		// NMAX is the largest n such that 255n(n+1)/2 + (n+1)(BASE-1) <= 2^32-1
		private static int NMAX = 5552;

		static internal long Adler32(long adler, byte[] buf, int index, int len)
		{
			if (buf == null)
			{
				return 1L;
			}

			long s1 = adler & 0xffff;
			long s2 = (adler >> 16) & 0xffff;
			int k;

			while (len > 0)
			{
				k = len < NMAX ? len : NMAX;
				len -= k;
				while (k >= 16)
				{
					s1 += (buf[index++] & 0xff); s2 += s1;
					s1 += (buf[index++] & 0xff); s2 += s1;
					s1 += (buf[index++] & 0xff); s2 += s1;
					s1 += (buf[index++] & 0xff); s2 += s1;
					s1 += (buf[index++] & 0xff); s2 += s1;
					s1 += (buf[index++] & 0xff); s2 += s1;
					s1 += (buf[index++] & 0xff); s2 += s1;
					s1 += (buf[index++] & 0xff); s2 += s1;
					s1 += (buf[index++] & 0xff); s2 += s1;
					s1 += (buf[index++] & 0xff); s2 += s1;
					s1 += (buf[index++] & 0xff); s2 += s1;
					s1 += (buf[index++] & 0xff); s2 += s1;
					s1 += (buf[index++] & 0xff); s2 += s1;
					s1 += (buf[index++] & 0xff); s2 += s1;
					s1 += (buf[index++] & 0xff); s2 += s1;
					s1 += (buf[index++] & 0xff); s2 += s1;
					k -= 16;
				}
				if (k != 0)
				{
					do
					{
						s1 += (buf[index++] & 0xff); s2 += s1;
					}
					while (--k != 0);
				}
				s1 %= BASE;
				s2 %= BASE;
			}
			return (s2 << 16) | s1;
		}

		/*
		private java.util.zip.Adler32 adler=new java.util.zip.Adler32();
		long adler32(long value, byte[] buf, int index, int len){
		if(value==1) {adler.reset();}
		if(buf==null) {adler.reset();}
		else{adler.update(buf, index, len);}
		return adler.getValue();
		}
		*/
	}

}

#endif