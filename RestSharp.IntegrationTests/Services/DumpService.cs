using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayak.Framework;

namespace RestSharp.IntegrationTests.Services
{
	public class DumpService : KayakService
	{
		[Path("/Dump"), Verb("PUT")]
		public void Dump()
		{
			Response.Write(Request.ContentLength);
		}
	}
}
