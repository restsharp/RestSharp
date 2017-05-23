// Inflate.cs
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
// Time-stamp: <2009-May-31 09:15:22>
//
// ------------------------------------------------------------------
//
// This module defines classes for decompression. This code is derived
// from the jzlib implementation of zlib, but significantly modified.
// The object model is not the same, and many of the behaviors are
// different.  Nonetheless, in keeping with the license for jzlib, I am
// reproducing the copyright to that code here.
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
namespace RestSharp.Compression.ZLib
{
    sealed internal class InflateBlocks
    {
        private const int MANY = 1440;

        // And'ing with mask[n] masks the lower n bits
        // UPGRADE_NOTE: Final was removed from the declaration of 'inflate_mask'.
        // "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        private static readonly int[] inflateMask =
        {
            0x00000000,
            0x00000001,
            0x00000003,
            0x00000007,
            0x0000000f,
            0x0000001f,
            0x0000003f,
            0x0000007f,
            0x000000ff,
            0x000001ff,
            0x000003ff,
            0x000007ff,
            0x00000fff,
            0x00001fff,
            0x00003fff,
            0x00007fff,
            0x0000ffff
        };

        // Table for deflate from PKZIP's appnote.txt.
        // UPGRADE_NOTE: Final was removed from the declaration of 'border'.
        // "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal static readonly int[] Border = { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };

        private const int TYPE = 0; // get type bits (3, including end bit)
        private const int LENS = 1; // get lengths for stored
        private const int STORED = 2; // processing stored block
        private const int TABLE = 3; // get table lengths
        private const int BTREE = 4; // get bit lengths tree for a dynamic block
        private const int DTREE = 5; // get length, distance trees for a dynamic block
        private const int CODES = 6; // processing fixed or dynamic block
        private const int DRY = 7; // output remaining window bytes
        private const int DONE = 8; // finished last block, done
        private const int BAD = 9; // ot a data error--stuck here

        internal int Mode; // current inflate_block mode 
        internal int Left; // if STORED, bytes left to copy 
        internal int Table; // table lengths (14 bits) 
        internal int Index; // index into blens (or border) 
        internal int[] Blens; // bit lengths of codes 
        internal int[] Bb = new int[1]; // bit length tree depth 
        internal int[] Tb = new int[1]; // bit length decoding tree 

        internal InflateCodes Codes = new InflateCodes(); // if CODES, current state 
        internal int Last; // true if this block is the last block 
        internal ZlibCodec Codec; // pointer back to this zlib stream

        // mode independent information 
        internal int Bitk; // bits in bit buffer 
        internal int Bitb; // bit buffer 
        internal int[] Hufts; // single malloc for tree space 
        internal byte[] Window; // sliding window 
        internal int End; // one byte after sliding window 
        internal int Read; // window read pointer 
        internal int Write; // window write pointer 
        internal object Checkfn; // check function 
        internal long Check; // check on output 

        internal InfTree Inftree = new InfTree();

        internal InflateBlocks(ZlibCodec codec, object checkfn, int w)
        {
            this.Codec = codec;
            this.Hufts = new int[MANY * 3];
            this.Window = new byte[w];
            this.End = w;
            this.Checkfn = checkfn;
            this.Mode = TYPE;
            this.Reset(null);
        }

        internal void Reset(long[] c)
        {
            if (c != null)
                c[0] = this.Check;

            if (this.Mode == BTREE || this.Mode == DTREE) { }

            if (this.Mode == CODES) { }

            this.Mode = TYPE;
            this.Bitk = 0;
            this.Bitb = 0;
            this.Read = this.Write = 0;

            if (this.Checkfn != null)
                this.Codec.Adler32 = this.Check = Adler.Adler32(0L, null, 0, 0);
        }

        internal int Process(int r)
        {
            int b; // bit buffer
            int k; // bits in bit buffer
            int p; // input data pointer
            int n; // bytes available there
            int q; // output window write pointer
            int m; // bytes to end of window or read pointer

            // copy input/output information to locals (UPDATE macro restores)

            p = this.Codec.NextIn;
            n = this.Codec.AvailableBytesIn;
            b = this.Bitb;
            k = this.Bitk;
            q = this.Write;
            m = q < this.Read ? this.Read - q - 1 : this.End - q;

            // process input based on current state
            while (true)
            {
                int t; // temporary storage

                switch (this.Mode)
                {
                    case TYPE:
                        while (k < (3))
                        {
                            if (n != 0)
                            {
                                r = ZlibConstants.Z_OK;
                            }
                            else
                            {
                                this.Bitb = b;
                                this.Bitk = k;
                                this.Codec.AvailableBytesIn = n;
                                this.Codec.TotalBytesIn += p - this.Codec.NextIn;
                                this.Codec.NextIn = p;
                                this.Write = q;

                                return this.Flush(r);
                            }

                            n--;
                            b |= (this.Codec.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        t = b & 7;
                        this.Last = t & 1;

                        switch (SharedUtils.UrShift(t, 1))
                        {
                            case 0:  // stored 
                                b = SharedUtils.UrShift(b, (3));
                                k -= (3);
                                t = k & 7; // go to byte boundary
                                b = SharedUtils.UrShift(b, (t));
                                k -= (t);
                                this.Mode = LENS; // get length of stored block
                                break;

                            case 1:  // fixed
                                int[] bl = new int[1];
                                int[] bd = new int[1];
                                int[][] tl = new int[1][];
                                int[][] td = new int[1][];
                                InfTree.inflate_trees_fixed(bl, bd, tl, td, this.Codec);
                                this.Codes.Init(bl[0], bd[0], tl[0], 0, td[0], 0);
                                b = SharedUtils.UrShift(b, (3));
                                k -= (3);
                                this.Mode = CODES;
                                break;

                            case 2:  // dynamic
                                b = SharedUtils.UrShift(b, (3));
                                k -= (3);
                                this.Mode = TABLE;
                                break;

                            case 3:  // illegal
                                b = SharedUtils.UrShift(b, (3));
                                k -= (3);
                                this.Mode = BAD;
                                this.Codec.Message = "invalid block type";
                                r = ZlibConstants.Z_DATA_ERROR;
                                this.Bitb = b;
                                this.Bitk = k;
                                this.Codec.AvailableBytesIn = n;
                                this.Codec.TotalBytesIn += p - this.Codec.NextIn;
                                this.Codec.NextIn = p;
                                this.Write = q;
                                return this.Flush(r);
                        }
                        break;

                    case LENS:
                        while (k < (32))
                        {
                            if (n != 0)
                            {
                                r = ZlibConstants.Z_OK;
                            }
                            else
                            {
                                this.Bitb = b;
                                this.Bitk = k;
                                this.Codec.AvailableBytesIn = n;
                                this.Codec.TotalBytesIn += p - this.Codec.NextIn;
                                this.Codec.NextIn = p;
                                this.Write = q;
                                return this.Flush(r);
                            }

                            n--;
                            b |= (this.Codec.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        if (((SharedUtils.UrShift((~b), 16)) & 0xffff) != (b & 0xffff))
                        {
                            this.Mode = BAD;
                            this.Codec.Message = "invalid stored block lengths";
                            r = ZlibConstants.Z_DATA_ERROR;
                            this.Bitb = b;
                            this.Bitk = k;
                            this.Codec.AvailableBytesIn = n;
                            this.Codec.TotalBytesIn += p - this.Codec.NextIn;
                            this.Codec.NextIn = p;
                            this.Write = q;
                            return this.Flush(r);
                        }

                        this.Left = (b & 0xffff);
                        b = k = 0; // dump bits
                        this.Mode = this.Left != 0 ? STORED : (this.Last != 0 ? DRY : TYPE);
                        break;

                    case STORED:
                        if (n == 0)
                        {
                            this.Bitb = b;
                            this.Bitk = k;
                            this.Codec.AvailableBytesIn = n;
                            this.Codec.TotalBytesIn += p - this.Codec.NextIn;
                            this.Codec.NextIn = p;
                            this.Write = q;
                            return this.Flush(r);
                        }

                        if (m == 0)
                        {
                            if (q == this.End && this.Read != 0)
                            {
                                q = 0;
                                m = q < this.Read ? this.Read - q - 1 : this.End - q;
                            }

                            if (m == 0)
                            {
                                this.Write = q;
                                r = this.Flush(r);
                                q = this.Write;
                                m = q < this.Read ? this.Read - q - 1 : this.End - q;

                                if (q == this.End && this.Read != 0)
                                {
                                    q = 0;
                                    m = q < this.Read ? this.Read - q - 1 : this.End - q;
                                }

                                if (m == 0)
                                {
                                    this.Bitb = b;
                                    this.Bitk = k;
                                    this.Codec.AvailableBytesIn = n;
                                    this.Codec.TotalBytesIn += p - this.Codec.NextIn;
                                    this.Codec.NextIn = p;
                                    this.Write = q;

                                    return this.Flush(r);
                                }
                            }
                        }

                        r = ZlibConstants.Z_OK;
                        t = this.Left;

                        if (t > n)
                            t = n;

                        if (t > m)
                            t = m;

                        Array.Copy(this.Codec.InputBuffer, p, this.Window, q, t);
                        p += t;
                        n -= t;
                        q += t;
                        m -= t;

                        if ((this.Left -= t) != 0)
                            break;

                        this.Mode = this.Last != 0 ? DRY : TYPE;
                        break;

                    case TABLE:
                        while (k < (14))
                        {
                            if (n != 0)
                            {
                                r = ZlibConstants.Z_OK;
                            }
                            else
                            {
                                this.Bitb = b;
                                this.Bitk = k;
                                this.Codec.AvailableBytesIn = n;
                                this.Codec.TotalBytesIn += p - this.Codec.NextIn;
                                this.Codec.NextIn = p;
                                this.Write = q;

                                return this.Flush(r);
                            }

                            n--;
                            b |= (this.Codec.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        this.Table = t = (b & 0x3fff);

                        if ((t & 0x1f) > 29 || ((t >> 5) & 0x1f) > 29)
                        {
                            this.Mode = BAD;
                            this.Codec.Message = "too many length or distance symbols";
                            r = ZlibConstants.Z_DATA_ERROR;
                            this.Bitb = b;
                            this.Bitk = k;
                            this.Codec.AvailableBytesIn = n;
                            this.Codec.TotalBytesIn += p - this.Codec.NextIn;
                            this.Codec.NextIn = p;
                            this.Write = q;

                            return this.Flush(r);
                        }

                        t = 258 + (t & 0x1f) + ((t >> 5) & 0x1f);

                        if (this.Blens == null || this.Blens.Length < t)
                        {
                            this.Blens = new int[t];
                        }
                        else
                        {
                            for (int i = 0; i < t; i++)
                            {
                                this.Blens[i] = 0;
                            }
                        }
                        {
                            b = SharedUtils.UrShift(b, (14));
                            k -= (14);
                        }

                        this.Index = 0;
                        this.Mode = BTREE;

                        goto case BTREE;

                    case BTREE:
                        while (this.Index < 4 + (SharedUtils.UrShift(this.Table, 10)))
                        {
                            while (k < (3))
                            {
                                if (n != 0)
                                {
                                    r = ZlibConstants.Z_OK;
                                }
                                else
                                {
                                    this.Bitb = b;
                                    this.Bitk = k;
                                    this.Codec.AvailableBytesIn = n;
                                    this.Codec.TotalBytesIn += p - this.Codec.NextIn;
                                    this.Codec.NextIn = p;
                                    this.Write = q;

                                    return this.Flush(r);
                                }

                                n--;
                                b |= (this.Codec.InputBuffer[p++] & 0xff) << k;
                                k += 8;
                            }

                            this.Blens[Border[this.Index++]] = b & 7;

                            {
                                b = SharedUtils.UrShift(b, (3));
                                k -= (3);
                            }
                        }

                        while (this.Index < 19)
                        {
                            this.Blens[Border[this.Index++]] = 0;
                        }

                        this.Bb[0] = 7;
                        t = this.Inftree.inflate_trees_bits(this.Blens, this.Bb, this.Tb, this.Hufts, this.Codec);

                        if (t != ZlibConstants.Z_OK)
                        {
                            r = t;

                            if (r == ZlibConstants.Z_DATA_ERROR)
                            {
                                this.Blens = null;
                                this.Mode = BAD;
                            }

                            this.Bitb = b;
                            this.Bitk = k;
                            this.Codec.AvailableBytesIn = n;
                            this.Codec.TotalBytesIn += p - this.Codec.NextIn;
                            this.Codec.NextIn = p;
                            this.Write = q;
                            return this.Flush(r);
                        }

                        this.Index = 0;
                        this.Mode = DTREE;

                        goto case DTREE;

                    case DTREE:
                        while (true)
                        {
                            t = this.Table;

                            if (!(this.Index < 258 + (t & 0x1f) + ((t >> 5) & 0x1f)))
                            {
                                break;
                            }

                            int c;

                            t = this.Bb[0];

                            while (k < (t))
                            {
                                if (n != 0)
                                {
                                    r = ZlibConstants.Z_OK;
                                }
                                else
                                {
                                    this.Bitb = b;
                                    this.Bitk = k;
                                    this.Codec.AvailableBytesIn = n;
                                    this.Codec.TotalBytesIn += p - this.Codec.NextIn;
                                    this.Codec.NextIn = p;
                                    this.Write = q;

                                    return this.Flush(r);
                                }

                                n--;
                                b |= (this.Codec.InputBuffer[p++] & 0xff) << k;
                                k += 8;
                            }

                            if (this.Tb[0] == -1)
                            {
                                //System.err.println("null...");
                            }

                            t = this.Hufts[(this.Tb[0] + (b & inflateMask[t])) * 3 + 1];
                            c = this.Hufts[(this.Tb[0] + (b & inflateMask[t])) * 3 + 2];

                            if (c < 16)
                            {
                                b = SharedUtils.UrShift(b, (t));
                                k -= (t);
                                this.Blens[this.Index++] = c;
                            }
                            else
                            {
                                // c == 16..18
                                int i = c == 18 ? 7 : c - 14;
                                int j = c == 18 ? 11 : 3;

                                while (k < (t + i))
                                {
                                    if (n != 0)
                                    {
                                        r = ZlibConstants.Z_OK;
                                    }
                                    else
                                    {
                                        this.Bitb = b;
                                        this.Bitk = k;
                                        this.Codec.AvailableBytesIn = n;
                                        this.Codec.TotalBytesIn += p - this.Codec.NextIn;
                                        this.Codec.NextIn = p;
                                        this.Write = q;

                                        return this.Flush(r);
                                    }

                                    n--;
                                    b |= (this.Codec.InputBuffer[p++] & 0xff) << k;
                                    k += 8;
                                }

                                b = SharedUtils.UrShift(b, (t));
                                k -= (t);
                                j += (b & inflateMask[i]);
                                b = SharedUtils.UrShift(b, (i));
                                k -= (i);

                                i = this.Index;
                                t = this.Table;

                                if (i + j > 258 + (t & 0x1f) + ((t >> 5) & 0x1f) || (c == 16 && i < 1))
                                {
                                    this.Blens = null;
                                    this.Mode = BAD;
                                    this.Codec.Message = "invalid bit length repeat";
                                    r = ZlibConstants.Z_DATA_ERROR;
                                    this.Bitb = b;
                                    this.Bitk = k;
                                    this.Codec.AvailableBytesIn = n;
                                    this.Codec.TotalBytesIn += p - this.Codec.NextIn;
                                    this.Codec.NextIn = p;
                                    this.Write = q;

                                    return this.Flush(r);
                                }

                                c = c == 16 ? this.Blens[i - 1] : 0;

                                do
                                {
                                    this.Blens[i++] = c;
                                }
                                while (--j != 0);

                                this.Index = i;
                            }
                        }

                        this.Tb[0] = -1;
                        {
                            int[] bl = { 9 };  // must be <= 9 for lookahead assumptions
                            int[] bd = { 6 }; // must be <= 9 for lookahead assumptions
                            int[] tl = new int[1];
                            int[] td = new int[1];

                            t = this.Table;
                            t = this.Inftree.inflate_trees_dynamic(257 + (t & 0x1f), 1 + ((t >> 5) & 0x1f), this.Blens, bl, bd, tl, td, this.Hufts, this.Codec);

                            if (t != ZlibConstants.Z_OK)
                            {
                                if (t == ZlibConstants.Z_DATA_ERROR)
                                {
                                    this.Blens = null;
                                    this.Mode = BAD;
                                }

                                r = t;
                                this.Bitb = b;
                                this.Bitk = k;
                                this.Codec.AvailableBytesIn = n;
                                this.Codec.TotalBytesIn += p - this.Codec.NextIn;
                                this.Codec.NextIn = p;
                                this.Write = q;

                                return this.Flush(r);
                            }

                            this.Codes.Init(bl[0], bd[0], this.Hufts, tl[0], this.Hufts, td[0]);
                        }

                        this.Mode = CODES;

                        goto case CODES;

                    case CODES:
                        this.Bitb = b;
                        this.Bitk = k;
                        this.Codec.AvailableBytesIn = n;
                        this.Codec.TotalBytesIn += p - this.Codec.NextIn;
                        this.Codec.NextIn = p;
                        this.Write = q;

                        if ((r = this.Codes.Process(this, r)) != ZlibConstants.Z_STREAM_END)
                            return this.Flush(r);

                        r = ZlibConstants.Z_OK;
                        p = this.Codec.NextIn;
                        n = this.Codec.AvailableBytesIn;
                        b = this.Bitb;
                        k = this.Bitk;
                        q = this.Write;
                        m = q < this.Read ? this.Read - q - 1 : this.End - q;

                        if (this.Last == 0)
                        {
                            this.Mode = TYPE;
                            break;
                        }

                        this.Mode = DRY;
                        goto case DRY;

                    case DRY:
                        this.Write = q;
                        r = this.Flush(r);
                        q = this.Write;

                        //m = q < this.Read ? this.Read - q - 1 : this.End - q;

                        if (this.Read != this.Write)
                        {
                            this.Bitb = b;
                            this.Bitk = k;
                            this.Codec.AvailableBytesIn = n;
                            this.Codec.TotalBytesIn += p - this.Codec.NextIn;
                            this.Codec.NextIn = p;
                            this.Write = q;

                            return this.Flush(r);
                        }

                        this.Mode = DONE;

                        goto case DONE;

                    case DONE:
                        r = ZlibConstants.Z_STREAM_END;
                        this.Bitb = b;
                        this.Bitk = k;
                        this.Codec.AvailableBytesIn = n;
                        this.Codec.TotalBytesIn += p - this.Codec.NextIn;
                        this.Codec.NextIn = p;
                        this.Write = q;

                        return this.Flush(r);

                    case BAD:
                        r = ZlibConstants.Z_DATA_ERROR;
                        this.Bitb = b;
                        this.Bitk = k;
                        this.Codec.AvailableBytesIn = n;
                        this.Codec.TotalBytesIn += p - this.Codec.NextIn;
                        this.Codec.NextIn = p;
                        this.Write = q;

                        return this.Flush(r);

                    default:
                        r = ZlibConstants.Z_STREAM_ERROR;
                        this.Bitb = b;
                        this.Bitk = k;
                        this.Codec.AvailableBytesIn = n;
                        this.Codec.TotalBytesIn += p - this.Codec.NextIn;
                        this.Codec.NextIn = p;
                        this.Write = q;

                        return this.Flush(r);
                }
            }
        }

        internal void Free()
        {
            this.Reset(null);
            this.Window = null;
            this.Hufts = null;
            //ZFREE(z, s);
        }

        internal void SetDictionary(byte[] d, int start, int n)
        {
            Array.Copy(d, start, this.Window, 0, n);
            this.Read = this.Write = n;
        }

        // Returns true if inflate is currently at the end of a block generated
        // by Z_SYNC_FLUSH or Z_FULL_FLUSH. 
        internal int SyncPoint()
        {
            return this.Mode == LENS ? 1 : 0;
        }

        // copy as much as possible from the sliding window to the output area
        internal int Flush(int r)
        {
            int n;
            int p;
            int q;

            // local copies of source and destination pointers
            p = this.Codec.NextOut;
            q = this.Read;

            // compute number of bytes to copy as far as end of window
            n = (q <= this.Write ? this.Write : this.End) - q;

            if (n > this.Codec.AvailableBytesOut)
                n = this.Codec.AvailableBytesOut;

            if (n != 0 && r == ZlibConstants.Z_BUF_ERROR)
                r = ZlibConstants.Z_OK;

            // update counters
            this.Codec.AvailableBytesOut -= n;
            this.Codec.TotalBytesOut += n;

            // update check information
            if (this.Checkfn != null)
                this.Codec.Adler32 = this.Check = Adler.Adler32(this.Check, this.Window, q, n);

            // copy as far as end of window
            Array.Copy(this.Window, q, this.Codec.OutputBuffer, p, n);
            p += n;
            q += n;

            // see if more to copy at beginning of window
            if (q == this.End)
            {
                // wrap pointers
                q = 0;

                if (this.Write == this.End)
                    this.Write = 0;

                // compute bytes to copy
                n = this.Write - q;

                if (n > this.Codec.AvailableBytesOut)
                    n = this.Codec.AvailableBytesOut;

                if (n != 0 && r == ZlibConstants.Z_BUF_ERROR)
                    r = ZlibConstants.Z_OK;

                // update counters
                this.Codec.AvailableBytesOut -= n;
                this.Codec.TotalBytesOut += n;

                // update check information
                if (this.Checkfn != null)
                    this.Codec.Adler32 = this.Check = Adler.Adler32(this.Check, this.Window, q, n);

                // copy
                Array.Copy(this.Window, q, this.Codec.OutputBuffer, p, n);
                p += n;
                q += n;
            }

            // update pointers
            this.Codec.NextOut = p;
            this.Read = q;

            // done
            return r;
        }
    }

    sealed internal class InflateCodes
    {
        // UPGRADE_NOTE: Final was removed from the declaration of 'inflate_mask'.
        // "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        private static readonly int[] inflateMask =
        {
            0x00000000,
            0x00000001,
            0x00000003,
            0x00000007,
            0x0000000f,
            0x0000001f,
            0x0000003f,
            0x0000007f,
            0x000000ff,
            0x000001ff,
            0x000003ff,
            0x000007ff,
            0x00000fff,
            0x00001fff,
            0x00003fff,
            0x00007fff,
            0x0000ffff
        };

        // waiting for "i:"=input,
        //             "o:"=output,
        //             "x:"=nothing
        private const int START = 0; // x: set up for LEN
        private const int LEN = 1; // i: get length/literal/eob next
        private const int LENEXT = 2; // i: getting length extra (have base)
        private const int DIST = 3; // i: get distance next
        private const int DISTEXT = 4; // i: getting distance extra
        private const int COPY = 5; // o: copying bytes in window, waiting for space
        private const int LIT = 6; // o: got literal, waiting for output space
        private const int WASH = 7; // o: got eob, possibly still output waiting
        private const int END = 8; // x: got eob and all data flushed
        private const int BADCODE = 9; // x: got error

        internal int Mode; // current inflate_codes mode

        // mode dependent information
        internal int Len;

        internal int[] Tree; // pointer into tree
        internal int TreeIndex;
        internal int Need; // bits needed
        internal int Lit;

        // if EXT or COPY, where and how much
        internal int GetRenamed; // bits to get for extra
        internal int Dist; // distance back to copy from

        internal byte Lbits; // ltree bits decoded per branch
        internal byte Dbits; // dtree bits decoder per branch
        internal int[] Ltree; // literal/length/eob tree
        internal int LtreeIndex; // literal/length/eob tree
        internal int[] Dtree; // distance tree
        internal int DtreeIndex; // distance tree

        //internal InflateCodes() { }

        internal void Init(int bl, int bd, int[] tl, int tlIndex, int[] td, int tdIndex)
        {
            this.Mode = START;
            this.Lbits = (byte) bl;
            this.Dbits = (byte) bd;
            this.Ltree = tl;
            this.LtreeIndex = tlIndex;
            this.Dtree = td;
            this.DtreeIndex = tdIndex;
            this.Tree = null;
        }

        internal int Process(InflateBlocks blocks, int r)
        {
            int b; // bit buffer
            int k; // bits in bit buffer
            int p; // input data pointer
            int n; // bytes available there
            int q; // output window write pointer
            int m; // bytes to end of window or read pointer

            ZlibCodec z = blocks.Codec;

            // copy input/output information to locals (UPDATE macro restores)
            p = z.NextIn;
            n = z.AvailableBytesIn;
            b = blocks.Bitb;
            k = blocks.Bitk;
            q = blocks.Write;
            m = q < blocks.Read ? blocks.Read - q - 1 : blocks.End - q;

            // process input and output based on current state
            while (true)
            {
                int j; // temporary storage
                int tindex; // temporary pointer
                int e; // extra bits or operation

                switch (this.Mode)
                {
                    // waiting for "i:"=input, "o:"=output, "x:"=nothing
                    case START:  // x: set up for LEN
                        if (m >= 258 && n >= 10)
                        {
                            blocks.Bitb = b;
                            blocks.Bitk = k;
                            z.AvailableBytesIn = n;
                            z.TotalBytesIn += p - z.NextIn;
                            z.NextIn = p;
                            blocks.Write = q;
                            r = this.InflateFast(this.Lbits, this.Dbits, this.Ltree, this.LtreeIndex, this.Dtree, this.DtreeIndex, blocks, z);
                            p = z.NextIn;
                            n = z.AvailableBytesIn;
                            b = blocks.Bitb;
                            k = blocks.Bitk;
                            q = blocks.Write;
                            m = q < blocks.Read ? blocks.Read - q - 1 : blocks.End - q;

                            if (r != ZlibConstants.Z_OK)
                            {
                                this.Mode = (r == ZlibConstants.Z_STREAM_END) ? WASH : BADCODE;

                                break;
                            }
                        }

                        this.Need = this.Lbits;
                        this.Tree = this.Ltree;
                        this.TreeIndex = this.LtreeIndex;
                        this.Mode = LEN;

                        goto case LEN;

                    case LEN:  // i: get length/literal/eob next
                        j = this.Need;

                        while (k < (j))
                        {
                            if (n != 0)
                                r = ZlibConstants.Z_OK;
                            else
                            {
                                blocks.Bitb = b;
                                blocks.Bitk = k;
                                z.AvailableBytesIn = n;
                                z.TotalBytesIn += p - z.NextIn;
                                z.NextIn = p;
                                blocks.Write = q;

                                return blocks.Flush(r);
                            }

                            n--;
                            b |= (z.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        tindex = (this.TreeIndex + (b & inflateMask[j])) * 3;

                        b = SharedUtils.UrShift(b, (this.Tree[tindex + 1]));
                        k -= (this.Tree[tindex + 1]);
                        e = this.Tree[tindex];

                        if (e == 0)
                        {
                            // literal
                            this.Lit = this.Tree[tindex + 2];
                            this.Mode = LIT;

                            break;
                        }

                        if ((e & 16) != 0)
                        {
                            // length
                            this.GetRenamed = e & 15;
                            this.Len = this.Tree[tindex + 2];
                            this.Mode = LENEXT;

                            break;
                        }

                        if ((e & 64) == 0)
                        {
                            // next table
                            this.Need = e;
                            this.TreeIndex = tindex / 3 + this.Tree[tindex + 2];

                            break;
                        }

                        if ((e & 32) != 0)
                        {
                            // end of block
                            this.Mode = WASH;

                            break;
                        }

                        this.Mode = BADCODE; // invalid code
                        z.Message = "invalid literal/length code";
                        r = ZlibConstants.Z_DATA_ERROR;
                        blocks.Bitb = b;
                        blocks.Bitk = k;
                        z.AvailableBytesIn = n;
                        z.TotalBytesIn += p - z.NextIn;
                        z.NextIn = p;
                        blocks.Write = q;

                        return blocks.Flush(r);

                    case LENEXT:  // i: getting length extra (have base)
                        j = this.GetRenamed;

                        while (k < (j))
                        {
                            if (n != 0)
                                r = ZlibConstants.Z_OK;
                            else
                            {

                                blocks.Bitb = b;
                                blocks.Bitk = k;
                                z.AvailableBytesIn = n;
                                z.TotalBytesIn += p - z.NextIn;
                                z.NextIn = p;
                                blocks.Write = q;

                                return blocks.Flush(r);
                            }

                            n--;
                            b |= (z.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        this.Len += (b & inflateMask[j]);

                        b >>= j;
                        k -= j;

                        this.Need = this.Dbits;
                        this.Tree = this.Dtree;
                        this.TreeIndex = this.DtreeIndex;
                        this.Mode = DIST;

                        goto case DIST;

                    case DIST:  // i: get distance next
                        j = this.Need;

                        while (k < (j))
                        {
                            if (n != 0)
                                r = ZlibConstants.Z_OK;
                            else
                            {
                                blocks.Bitb = b;
                                blocks.Bitk = k;
                                z.AvailableBytesIn = n;
                                z.TotalBytesIn += p - z.NextIn;
                                z.NextIn = p;
                                blocks.Write = q;

                                return blocks.Flush(r);
                            }

                            n--;
                            b |= (z.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        tindex = (this.TreeIndex + (b & inflateMask[j])) * 3;

                        b >>= this.Tree[tindex + 1];
                        k -= this.Tree[tindex + 1];
                        e = (this.Tree[tindex]);

                        if ((e & 16) != 0)
                        {
                            // distance
                            this.GetRenamed = e & 15;
                            this.Dist = this.Tree[tindex + 2];
                            this.Mode = DISTEXT;

                            break;
                        }

                        if ((e & 64) == 0)
                        {
                            // next table
                            this.Need = e;
                            this.TreeIndex = tindex / 3 + this.Tree[tindex + 2];

                            break;
                        }

                        this.Mode = BADCODE; // invalid code
                        z.Message = "invalid distance code";
                        r = ZlibConstants.Z_DATA_ERROR;
                        blocks.Bitb = b;
                        blocks.Bitk = k;
                        z.AvailableBytesIn = n;
                        z.TotalBytesIn += p - z.NextIn;
                        z.NextIn = p;
                        blocks.Write = q;

                        return blocks.Flush(r);

                    case DISTEXT:  // i: getting distance extra
                        j = this.GetRenamed;

                        while (k < (j))
                        {
                            if (n != 0)
                                r = ZlibConstants.Z_OK;
                            else
                            {
                                blocks.Bitb = b;
                                blocks.Bitk = k;
                                z.AvailableBytesIn = n;
                                z.TotalBytesIn += p - z.NextIn;
                                z.NextIn = p;
                                blocks.Write = q;

                                return blocks.Flush(r);
                            }

                            n--;
                            b |= (z.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        this.Dist += (b & inflateMask[j]);

                        b >>= j;
                        k -= j;

                        this.Mode = COPY;

                        goto case COPY;

                    case COPY:  // o: copying bytes in window, waiting for space
                        int f = q - this.Dist; // pointer to copy strings from

                        while (f < 0)
                        {
                            // modulo window size-"while" instead
                            f += blocks.End; // of "if" handles invalid distances
                        }

                        while (this.Len != 0)
                        {
                            if (m == 0)
                            {
                                if (q == blocks.End && blocks.Read != 0)
                                {
                                    q = 0;
                                    m = q < blocks.Read ? blocks.Read - q - 1 : blocks.End - q;
                                }

                                if (m == 0)
                                {
                                    blocks.Write = q;
                                    r = blocks.Flush(r);
                                    q = blocks.Write;
                                    m = q < blocks.Read ? blocks.Read - q - 1 : blocks.End - q;

                                    if (q == blocks.End && blocks.Read != 0)
                                    {
                                        q = 0;
                                        m = q < blocks.Read ? blocks.Read - q - 1 : blocks.End - q;
                                    }

                                    if (m == 0)
                                    {
                                        blocks.Bitb = b;
                                        blocks.Bitk = k;
                                        z.AvailableBytesIn = n;
                                        z.TotalBytesIn += p - z.NextIn;
                                        z.NextIn = p;
                                        blocks.Write = q;

                                        return blocks.Flush(r);
                                    }
                                }
                            }

                            blocks.Window[q++] = blocks.Window[f++];
                            m--;

                            if (f == blocks.End)
                                f = 0;

                            this.Len--;
                        }

                        this.Mode = START;
                        break;

                    case LIT:  // o: got literal, waiting for output space
                        if (m == 0)
                        {
                            if (q == blocks.End && blocks.Read != 0)
                            {
                                q = 0;
                                m = q < blocks.Read ? blocks.Read - q - 1 : blocks.End - q;
                            }

                            if (m == 0)
                            {
                                blocks.Write = q;
                                r = blocks.Flush(r);
                                q = blocks.Write;
                                m = q < blocks.Read ? blocks.Read - q - 1 : blocks.End - q;

                                if (q == blocks.End && blocks.Read != 0)
                                {
                                    q = 0;
                                    m = q < blocks.Read ? blocks.Read - q - 1 : blocks.End - q;
                                }

                                if (m == 0)
                                {
                                    blocks.Bitb = b;
                                    blocks.Bitk = k;
                                    z.AvailableBytesIn = n;
                                    z.TotalBytesIn += p - z.NextIn;
                                    z.NextIn = p;
                                    blocks.Write = q;

                                    return blocks.Flush(r);
                                }
                            }
                        }

                        r = ZlibConstants.Z_OK;
                        blocks.Window[q++] = (byte) this.Lit;
                        m--;
                        this.Mode = START;

                        break;

                    case WASH:  // o: got eob, possibly more output
                        if (k > 7)
                        {
                            // return unused byte, if any
                            k -= 8;
                            n++;
                            p--; // can always return one
                        }

                        blocks.Write = q;
                        r = blocks.Flush(r);
                        q = blocks.Write;
                        //m = q < blocks.Read ? blocks.Read - q - 1 : blocks.End - q;

                        if (blocks.Read != blocks.Write)
                        {
                            blocks.Bitb = b;
                            blocks.Bitk = k;
                            z.AvailableBytesIn = n;
                            z.TotalBytesIn += p - z.NextIn;
                            z.NextIn = p;
                            blocks.Write = q;

                            return blocks.Flush(r);
                        }

                        this.Mode = END;

                        goto case END;

                    case END:
                        r = ZlibConstants.Z_STREAM_END;
                        blocks.Bitb = b;
                        blocks.Bitk = k;
                        z.AvailableBytesIn = n;
                        z.TotalBytesIn += p - z.NextIn;
                        z.NextIn = p;
                        blocks.Write = q;

                        return blocks.Flush(r);

                    case BADCODE:  // x: got error
                        r = ZlibConstants.Z_DATA_ERROR;
                        blocks.Bitb = b;
                        blocks.Bitk = k;
                        z.AvailableBytesIn = n;
                        z.TotalBytesIn += p - z.NextIn;
                        z.NextIn = p;
                        blocks.Write = q;

                        return blocks.Flush(r);

                    default:
                        r = ZlibConstants.Z_STREAM_ERROR;
                        blocks.Bitb = b;
                        blocks.Bitk = k;
                        z.AvailableBytesIn = n;
                        z.TotalBytesIn += p - z.NextIn;
                        z.NextIn = p;
                        blocks.Write = q;

                        return blocks.Flush(r);
                }
            }
        }

        // Called with number of bytes left to write in window at least 258
        // (the maximum string length) and number of input bytes available
        // at least ten.  The ten bytes are six bytes for the longest length/
        // distance pair plus four bytes for overloading the bit buffer.

        internal int InflateFast(int bl, int bd, int[] tl, int tlIndex, int[] td, int tdIndex, InflateBlocks s, ZlibCodec z)
        {
            int b; // bit buffer
            int k; // bits in bit buffer
            int p; // input data pointer
            int n; // bytes available there
            int q; // output window write pointer
            int m; // bytes to end of window or read pointer
            int ml; // mask for literal/length tree
            int md; // mask for distance tree
            int c; // bytes to copy

            // load input, output, bit values
            p = z.NextIn;
            n = z.AvailableBytesIn;
            b = s.Bitb;
            k = s.Bitk;
            q = s.Write;
            m = q < s.Read ? s.Read - q - 1 : s.End - q;

            // initialize masks
            ml = inflateMask[bl];
            md = inflateMask[bd];

            // do until not enough input or output space for fast loop
            do
            {
                // assume called with m >= 258 && n >= 10
                // get literal/length code
                while (k < (20))
                {
                    // max bits for literal/length code
                    n--;
                    b |= (z.InputBuffer[p++] & 0xff) << k;
                    k += 8;
                }

                int t = b & ml; // temporary pointer
                int[] tp = tl; // temporary pointer
                int tpIndex = tlIndex; // temporary pointer
                int tpIndexT3 = (tpIndex + t) * 3; // (tp_index+t)*3
                int e; // extra bits or operation

                if ((e = tp[tpIndexT3]) == 0)
                {
                    b >>= (tp[tpIndexT3 + 1]);
                    k -= (tp[tpIndexT3 + 1]);

                    s.Window[q++] = (byte) tp[tpIndexT3 + 2];
                    m--;

                    continue;
                }

                do
                {
                    b >>= (tp[tpIndexT3 + 1]);
                    k -= (tp[tpIndexT3 + 1]);

                    if ((e & 16) != 0)
                    {
                        e &= 15;
                        c = tp[tpIndexT3 + 2] + (b & inflateMask[e]);

                        b >>= e;
                        k -= e;

                        // decode distance base of block to copy
                        while (k < (15))
                        {
                            // max bits for distance code
                            n--;
                            b |= (z.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        t = b & md;
                        tp = td;
                        tpIndex = tdIndex;
                        tpIndexT3 = (tpIndex + t) * 3;
                        e = tp[tpIndexT3];

                        do
                        {
                            b >>= (tp[tpIndexT3 + 1]);
                            k -= (tp[tpIndexT3 + 1]);

                            if ((e & 16) != 0)
                            {
                                // get extra bits to add to distance base
                                e &= 15;

                                while (k < (e))
                                {
                                    // get extra bits (up to 13)
                                    n--;
                                    b |= (z.InputBuffer[p++] & 0xff) << k;
                                    k += 8;
                                }

                                int d = tp[tpIndexT3 + 2] + (b & inflateMask[e]); // distance back to copy from

                                b >>= (e);
                                k -= (e);

                                // do the copy
                                m -= c;

                                int r; // copy source pointer

                                if (q >= d)
                                {
                                    // offset before dest
                                    //  just copy
                                    r = q - d;

                                    if (q - r > 0 && 2 > (q - r))
                                    {
                                        s.Window[q++] = s.Window[r++]; // minimum count is three,
                                        s.Window[q++] = s.Window[r++]; // so unroll loop a little
                                        c -= 2;
                                    }
                                    else
                                    {
                                        Array.Copy(s.Window, r, s.Window, q, 2);
                                        q += 2;
                                        r += 2;
                                        c -= 2;
                                    }
                                }
                                else
                                {
                                    // else offset after destination
                                    r = q - d;

                                    do
                                    {
                                        r += s.End; // force pointer in window
                                    }
                                    while (r < 0); // covers invalid distances

                                    e = s.End - r;

                                    if (c > e)
                                    {
                                        // if source crosses,
                                        c -= e; // wrapped copy

                                        if (q - r > 0 && e > (q - r))
                                        {
                                            do
                                            {
                                                s.Window[q++] = s.Window[r++];
                                            }
                                            while (--e != 0);
                                        }
                                        else
                                        {
                                            Array.Copy(s.Window, r, s.Window, q, e);
                                            q += e;
                                            //r += e;
                                            //e = 0;
                                        }

                                        r = 0; // copy rest from start of window
                                    }
                                }

                                // copy all or what's left
                                if (q - r > 0 && c > (q - r))
                                {
                                    do
                                    {
                                        s.Window[q++] = s.Window[r++];
                                    }
                                    while (--c != 0);
                                }
                                else
                                {
                                    Array.Copy(s.Window, r, s.Window, q, c);
                                    q += c;
                                    //r += c;
                                    //c = 0;
                                }
                                break;
                            }

                            if ((e & 64) == 0)
                            {
                                t += tp[tpIndexT3 + 2];
                                t += (b & inflateMask[e]);
                                tpIndexT3 = (tpIndex + t) * 3;
                                e = tp[tpIndexT3];
                            }
                            else
                            {
                                z.Message = "invalid distance code";
                                c = z.AvailableBytesIn - n;
                                c = (k >> 3) < c ? k >> 3 : c;
                                n += c;
                                p -= c;
                                k -= (c << 3);
                                s.Bitb = b;
                                s.Bitk = k;
                                z.AvailableBytesIn = n;
                                z.TotalBytesIn += p - z.NextIn;
                                z.NextIn = p;
                                s.Write = q;

                                return ZlibConstants.Z_DATA_ERROR;
                            }
                        }
                        while (true);

                        break;
                    }

                    if ((e & 64) == 0)
                    {
                        t += tp[tpIndexT3 + 2];
                        t += (b & inflateMask[e]);
                        tpIndexT3 = (tpIndex + t) * 3;

                        if ((e = tp[tpIndexT3]) == 0)
                        {
                            b >>= (tp[tpIndexT3 + 1]);
                            k -= (tp[tpIndexT3 + 1]);
                            s.Window[q++] = (byte) tp[tpIndexT3 + 2];
                            m--;

                            break;
                        }
                    }
                    else if ((e & 32) != 0)
                    {

                        c = z.AvailableBytesIn - n;
                        c = (k >> 3) < c ? k >> 3 : c;
                        n += c;
                        p -= c;
                        k -= (c << 3);
                        s.Bitb = b;
                        s.Bitk = k;
                        z.AvailableBytesIn = n;
                        z.TotalBytesIn += p - z.NextIn;
                        z.NextIn = p;
                        s.Write = q;

                        return ZlibConstants.Z_STREAM_END;
                    }
                    else
                    {
                        z.Message = "invalid literal/length code";
                        c = z.AvailableBytesIn - n;
                        c = (k >> 3) < c ? k >> 3 : c;
                        n += c;
                        p -= c;
                        k -= (c << 3);
                        s.Bitb = b;
                        s.Bitk = k;
                        z.AvailableBytesIn = n;
                        z.TotalBytesIn += p - z.NextIn;
                        z.NextIn = p;
                        s.Write = q;

                        return ZlibConstants.Z_DATA_ERROR;
                    }
                }
                while (true);
            }
            while (m >= 258 && n >= 10);

            // not enough input or output--restore pointers and return
            c = z.AvailableBytesIn - n;
            c = (k >> 3) < c ? k >> 3 : c;
            n += c;
            p -= c;
            k -= (c << 3);

            s.Bitb = b;
            s.Bitk = k;
            z.AvailableBytesIn = n;
            z.TotalBytesIn += p - z.NextIn;
            z.NextIn = p;
            s.Write = q;

            return ZlibConstants.Z_OK;
        }
    }

    internal sealed class InflateManager
    {
        // preset dictionary flag in zlib header
        private const int PRESET_DICT = 0x20;
        private const int Z_DEFLATED = 8;
        private const int METHOD = 0; // waiting for method byte
        private const int FLAG = 1; // waiting for flag byte
        private const int DICT4 = 2; // four dictionary check bytes to go
        private const int DICT3 = 3; // three dictionary check bytes to go
        private const int DICT2 = 4; // two dictionary check bytes to go
        private const int DICT1 = 5; // one dictionary check byte to go
        private const int DICT0 = 6; // waiting for inflateSetDictionary
        private const int BLOCKS = 7; // decompressing blocks
        private const int CHECK4 = 8; // four check bytes to go
        private const int CHECK3 = 9; // three check bytes to go
        private const int CHECK2 = 10; // two check bytes to go
        private const int CHECK1 = 11; // one check byte to go
        private const int DONE = 12; // finished check, done
        private const int BAD = 13; // got an error--stay here

        internal int Mode; // current inflate mode
        internal ZlibCodec Codec; // pointer back to this zlib stream

        // mode dependent information
        internal int Method; // if FLAGS, method byte

        // if CHECK, check values to compare
        internal long[] Was = new long[1]; // computed check value
        internal long Need; // stream check value

        // if BAD, inflateSync's marker bytes count
        internal int Marker;

        // mode independent information
        //internal int nowrap; // flag for no wrapper
        private bool handleRfc1950HeaderBytes = true;

        internal bool HandleRfc1950HeaderBytes
        {
            get { return this.handleRfc1950HeaderBytes; }
            set { this.handleRfc1950HeaderBytes = value; }
        }

        internal int Wbits; // log2(window size)  (8..15, defaults to 15)
        internal InflateBlocks Blocks; // current inflate_blocks state

        public InflateManager() { }

        public InflateManager(bool expectRfc1950HeaderBytes)
        {
            this.handleRfc1950HeaderBytes = expectRfc1950HeaderBytes;
        }

        internal int Reset()
        {
            this.Codec.TotalBytesIn = this.Codec.TotalBytesOut = 0;
            this.Codec.Message = null;
            this.Mode = this.HandleRfc1950HeaderBytes ? METHOD : BLOCKS;
            this.Blocks.Reset(null);

            return ZlibConstants.Z_OK;
        }

        internal int End()
        {
            if (this.Blocks != null)
                this.Blocks.Free();

            this.Blocks = null;

            return ZlibConstants.Z_OK;
        }

        internal int Initialize(ZlibCodec codec, int w)
        {
            this.Codec = codec;
            codec.Message = null;
            this.Blocks = null;

            // handle undocumented nowrap option (no zlib header or check)
            //nowrap = 0;
            //if (w < 0)
            //{
            //    w = - w;
            //    nowrap = 1;
            //}

            // set window size
            if (w < 8 || w > 15)
            {
                this.End();
                throw new ZlibException("Bad window size.");

                //return ZlibConstants.Z_STREAM_ERROR;
            }

            this.Wbits = w;

            this.Blocks = new InflateBlocks(codec, this.HandleRfc1950HeaderBytes ? this : null, 1 << w);

            // reset state
            this.Reset();

            return ZlibConstants.Z_OK;
        }

        internal int Inflate(FlushType flush)
        {
            int r;
            int f = (int) flush;

            if (this.Codec.InputBuffer == null)
                throw new ZlibException("InputBuffer is null. ");

            f = (f == (int) FlushType.Finish)
                ? ZlibConstants.Z_BUF_ERROR
                : ZlibConstants.Z_OK;
            r = ZlibConstants.Z_BUF_ERROR;

            while (true)
            {
                switch (this.Mode)
                {
                    case METHOD:
                        if (this.Codec.AvailableBytesIn == 0)
                            return r;

                        r = f;

                        this.Codec.AvailableBytesIn--;
                        this.Codec.TotalBytesIn++;

                        if (((this.Method = this.Codec.InputBuffer[this.Codec.NextIn++]) & 0xf) != Z_DEFLATED)
                        {
                            this.Mode = BAD;
                            this.Codec.Message = string.Format("unknown compression method (0x{0:X2})", this.Method);
                            this.Marker = 5; // can't try inflateSync

                            break;
                        }

                        if ((this.Method >> 4) + 8 > this.Wbits)
                        {
                            this.Mode = BAD;
                            this.Codec.Message = string.Format("invalid window size ({0})", (this.Method >> 4) + 8);
                            this.Marker = 5; // can't try inflateSync

                            break;
                        }

                        this.Mode = FLAG;
                        goto case FLAG;

                    case FLAG:
                        if (this.Codec.AvailableBytesIn == 0)
                            return r;

                        r = f;

                        this.Codec.AvailableBytesIn--;
                        this.Codec.TotalBytesIn++;
                        int b = (this.Codec.InputBuffer[this.Codec.NextIn++]) & 0xff;

                        if ((((this.Method << 8) + b) % 31) != 0)
                        {
                            this.Mode = BAD;
                            this.Codec.Message = "incorrect header check";
                            this.Marker = 5; // can't try inflateSync

                            break;
                        }

                        if ((b & PRESET_DICT) == 0)
                        {
                            this.Mode = BLOCKS;

                            break;
                        }

                        this.Mode = DICT4;

                        goto case DICT4;

                    case DICT4:
                        if (this.Codec.AvailableBytesIn == 0)
                            return r;

                        r = f;
                        this.Codec.AvailableBytesIn--;
                        this.Codec.TotalBytesIn++;
                        this.Need = ((this.Codec.InputBuffer[this.Codec.NextIn++] & 0xff) << 24) & unchecked((int) 0xff000000L);
                        this.Mode = DICT3;

                        goto case DICT3;

                    case DICT3:
                        if (this.Codec.AvailableBytesIn == 0)
                            return r;

                        r = f;
                        this.Codec.AvailableBytesIn--;
                        this.Codec.TotalBytesIn++;
                        this.Need += (((this.Codec.InputBuffer[this.Codec.NextIn++] & 0xff) << 16) & 0xff0000L);
                        this.Mode = DICT2;

                        goto case DICT2;

                    case DICT2:
                        if (this.Codec.AvailableBytesIn == 0)
                            return r;

                        r = f;
                        this.Codec.AvailableBytesIn--;
                        this.Codec.TotalBytesIn++;
                        this.Need += (((this.Codec.InputBuffer[this.Codec.NextIn++] & 0xff) << 8) & 0xff00L);
                        this.Mode = DICT1;

                        goto case DICT1;

                    case DICT1:
                        if (this.Codec.AvailableBytesIn == 0)
                            return r;

                        //r = f;
                        this.Codec.AvailableBytesIn--;
                        this.Codec.TotalBytesIn++;
                        this.Need += (this.Codec.InputBuffer[this.Codec.NextIn++] & 0xffL);
                        this.Codec.Adler32 = this.Need;
                        this.Mode = DICT0;

                        return ZlibConstants.Z_NEED_DICT;

                    case DICT0:
                        this.Mode = BAD;
                        this.Codec.Message = "need dictionary";
                        this.Marker = 0; // can try inflateSync

                        return ZlibConstants.Z_STREAM_ERROR;

                    case BLOCKS:
                        r = this.Blocks.Process(r);

                        if (r == ZlibConstants.Z_DATA_ERROR)
                        {
                            this.Mode = BAD;
                            this.Marker = 0; // can try inflateSync

                            break;
                        }

                        if (r == ZlibConstants.Z_OK)
                            r = f;

                        if (r != ZlibConstants.Z_STREAM_END)
                            return r;

                        r = f;
                        this.Blocks.Reset(this.Was);

                        if (!this.HandleRfc1950HeaderBytes)
                        {
                            this.Mode = DONE;

                            break;
                        }

                        this.Mode = CHECK4;

                        goto case CHECK4;

                    case CHECK4:
                        if (this.Codec.AvailableBytesIn == 0)
                            return r;

                        r = f;
                        this.Codec.AvailableBytesIn--;
                        this.Codec.TotalBytesIn++;
                        this.Need = ((this.Codec.InputBuffer[this.Codec.NextIn++] & 0xff) << 24) & unchecked((int) 0xff000000L);
                        this.Mode = CHECK3;

                        goto case CHECK3;

                    case CHECK3:
                        if (this.Codec.AvailableBytesIn == 0)
                            return r;

                        r = f;
                        this.Codec.AvailableBytesIn--;
                        this.Codec.TotalBytesIn++;
                        this.Need += (((this.Codec.InputBuffer[this.Codec.NextIn++] & 0xff) << 16) & 0xff0000L);
                        this.Mode = CHECK2;

                        goto case CHECK2;

                    case CHECK2:
                        if (this.Codec.AvailableBytesIn == 0)
                            return r;

                        r = f;
                        this.Codec.AvailableBytesIn--;
                        this.Codec.TotalBytesIn++;
                        this.Need += (((this.Codec.InputBuffer[this.Codec.NextIn++] & 0xff) << 8) & 0xff00L);
                        this.Mode = CHECK1;

                        goto case CHECK1;

                    case CHECK1:
                        if (this.Codec.AvailableBytesIn == 0)
                            return r;

                        r = f;
                        this.Codec.AvailableBytesIn--;
                        this.Codec.TotalBytesIn++;
                        this.Need += (this.Codec.InputBuffer[this.Codec.NextIn++] & 0xffL);

                        unchecked
                        {
                            if (((int) (this.Was[0])) != ((int) (this.Need)))
                            {
                                this.Mode = BAD;
                                this.Codec.Message = "incorrect data check";
                                this.Marker = 5; // can't try inflateSync

                                break;
                            }
                        }

                        this.Mode = DONE;
                        goto case DONE;

                    case DONE:
                        return ZlibConstants.Z_STREAM_END;

                    case BAD:
                        throw new ZlibException(string.Format("Bad state ({0})", this.Codec.Message));
                        //return ZlibConstants.Z_DATA_ERROR;

                    default:
                        throw new ZlibException("Stream error.");
                        //return ZlibConstants.Z_STREAM_ERROR;
                }
            }
        }

        internal int SetDictionary(byte[] dictionary)
        {
            int index = 0;
            int length = dictionary.Length;

            if (this.Mode != DICT0)
                throw new ZlibException("Stream error.");

            if (Adler.Adler32(1L, dictionary, 0, dictionary.Length) != this.Codec.Adler32)
            {
                return ZlibConstants.Z_DATA_ERROR;
            }

            this.Codec.Adler32 = Adler.Adler32(0, null, 0, 0);

            if (length >= (1 << this.Wbits))
            {
                length = (1 << this.Wbits) - 1;
                index = dictionary.Length - length;
            }

            this.Blocks.SetDictionary(dictionary, index, length);
            this.Mode = BLOCKS;

            return ZlibConstants.Z_OK;
        }

        private static readonly byte[] mark = { 0, 0, 0xff, 0xff };

        internal int Sync()
        {
            int n; // number of bytes to look at
            int p; // pointer to bytes
            int m; // number of marker bytes found in a row
            long r, w; // temporaries to save total_in and total_out

            // set up
            if (this.Mode != BAD)
            {
                this.Mode = BAD;
                this.Marker = 0;
            }

            if ((n = this.Codec.AvailableBytesIn) == 0)
                return ZlibConstants.Z_BUF_ERROR;

            p = this.Codec.NextIn;
            m = this.Marker;

            // search
            while (n != 0 && m < 4)
            {
                if (this.Codec.InputBuffer[p] == mark[m])
                {
                    m++;
                }
                else if (this.Codec.InputBuffer[p] != 0)
                {
                    m = 0;
                }
                else
                {
                    m = 4 - m;
                }

                p++;
                n--;
            }

            // restore
            this.Codec.TotalBytesIn += p - this.Codec.NextIn;
            this.Codec.NextIn = p;
            this.Codec.AvailableBytesIn = n;
            this.Marker = m;

            // return no joy or set up to restart on a new block
            if (m != 4)
            {
                return ZlibConstants.Z_DATA_ERROR;
            }

            r = this.Codec.TotalBytesIn;
            w = this.Codec.TotalBytesOut;
            this.Reset();
            this.Codec.TotalBytesIn = r;
            this.Codec.TotalBytesOut = w;
            this.Mode = BLOCKS;

            return ZlibConstants.Z_OK;
        }

        // Returns true if inflate is currently at the end of a block generated
        // by Z_SYNC_FLUSH or Z_FULL_FLUSH. This function is used by one PPP
        // implementation to provide an additional safety check. PPP uses Z_SYNC_FLUSH
        // but removes the length bytes of the resulting empty stored block. When
        // decompressing, PPP checks that at the end of input packet, inflate is
        // waiting for these length bytes.
        internal int SyncPoint(ZlibCodec z)
        {
            return this.Blocks.SyncPoint();
        }
    }
}

#endif
