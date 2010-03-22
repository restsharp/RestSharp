using System;
using Kayak;
using Kayak.Framework;

namespace RestSharp.IntegrationTests
{
	public abstract class TestBase : IDisposable
	{
		readonly KayakServer _server = new KayakServer();
		protected const string BaseUrl = "http://localhost:8080";

		protected TestBase() {
			// setup
			_server.UseFramework();
			_server.Start();
			_server.MapDirectory("/Assets", Environment.CurrentDirectory + @"\Assets");
		}

		public void Dispose() {
			// tear down
			_server.Stop();
		}

	}
}
