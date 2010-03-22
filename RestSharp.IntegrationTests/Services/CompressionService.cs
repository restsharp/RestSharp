using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Kayak.Framework;

namespace RestSharp.IntegrationTests.Services
{
	public class CompressionService : KayakService
	{
		[Path("/Compression/GZip")]
		public void GZip() {
			Response.Headers.Add("Content-encoding", "gzip");
			Response.Write("This content is compressed with GZip!");
			var stream = new GZipStream(Response.OutputStream, CompressionMode.Compress); 
		}

		//[Path("/Compression/Deflate")]
		//public string Deflate() {
		//    Response.AppendHeader("Content-encoding", "deflate");
		//    Response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress); 
		//    return "This content is compressed with Deflate!";
		//}

		[Path("/Compression/None")]
		public void None() {
			Response.Write("This content is uncompressed!");
		}
	}
}
