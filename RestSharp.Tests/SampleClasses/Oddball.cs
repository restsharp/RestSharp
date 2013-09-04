using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Tests.SampleClasses
{
	public class Oddball
	{
		public string Sid { get; set; }
		public string FriendlyName { get; set; }

		[Deserializers.DeserializeAs(Name = "oddballPropertyName")]
		public string GoodPropertyName { get; set; }
	}
}
