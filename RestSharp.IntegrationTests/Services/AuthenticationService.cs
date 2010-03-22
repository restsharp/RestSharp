using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayak.Framework;

namespace RestSharp.IntegrationTests.Services
{
	public class AuthenticationService : KayakService
	{
		[Path("/Authentication/Basic")]
		public void Basic() {
			var header = Request.Headers["Authorization"];
			if (string.IsNullOrEmpty(header)) {
				Response.Write("no authorization provided");
			}

			var parts = Encoding.ASCII.GetString(Convert.FromBase64String(header.Substring("Basic ".Length))).Split(':');
			Response.Write(string.Join("|", parts));
		}
	}
}
