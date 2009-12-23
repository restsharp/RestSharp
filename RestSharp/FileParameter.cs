using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp
{
	public class FileParameter
	{
		public byte[] Data { get; set; }
		public string FileName { get; set; }
		public string ContentType { get; set; }
	}
}
