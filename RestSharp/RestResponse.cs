using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace RestSharp
{
	public class RestResponse
	{
		public string ContentType { get; set; }
		public long ContentLength { get; set; }
		public string ContentEncoding { get; set; }
		public string Content { get; set; }
		public HttpStatusCode StatusCode { get; set; }
		public string StatusDescription { get; set; }
		public Uri ResponseUri { get; set; }
		public string Server { get; set; }
	}
}
