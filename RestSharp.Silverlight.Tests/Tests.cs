using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace RestSharp.Silverlight.Tests
{
	[TestClass]
	public class Tests
	{
		[TestMethod]
		public void TestMethod1()
		{
			var client = new RestClient("https://api.twilio.com");
			client.UserAgent = "foo";
			client.Authenticator = new HttpBasicAuthenticator("ACec243f00aaaa1b67fa32ae88178ebc2c", "09772adb6b86ba9341168d5277d18fb2");

			var request = new RestRequest("2010-10-01/Accounts/ACec243f00aaaa1b67fa32ae88178ebc2c");

			client.ExecuteAsync(request, r =>
			{
				Debug.WriteLine(r.Content);
			});
		}
	}
}