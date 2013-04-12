using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Globalization;

namespace RestSharp.Tests {
	public class RestRequestTests {
		public RestRequestTests() {
			System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.InstalledUICulture;
		}

		[Fact]
		public void Can_Add_Object_With_IntegerArray_property() {
			var request = new RestRequest();
			request.AddObject(new { Items = new int[] { 2, 3, 4 } });
		}
	}
}
