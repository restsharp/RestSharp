using Kayak.Framework;
using System.IO;
using System;

namespace RestSharp.IntegrationTests.Services
{
	public class ErrorHandlingService : KayakService
	{
		[Path("/ErrorHandling/NotFound")]
		public void Error()
		{
			Response.StatusCode = 400;
			Response.Headers.Add("Content-Type", "application/xml");
			Response.Write(File.ReadAllText(Environment.CurrentDirectory + "\\Assets\\Error.Xml"));
			
			
		}

		[Path("/ErrorHandling/Success")]
		public void Success()
		{
			Response.Write(File.ReadAllText(Environment.CurrentDirectory + "\\Assets\\Success.Xml"));
		}
	}
}