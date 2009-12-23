using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp
{
	public class HttpFile
	{
		public string FileName { get; set; }
		public byte[] Data { get; set; }
		public string ContentType { get; set; }
	}
}
