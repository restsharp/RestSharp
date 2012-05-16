using System.Collections.Generic;
using System.Collections.Specialized;

namespace RestSharp.Authenticators.OAuth
{
	internal class WebParameterCollection : WebPairCollection
	{
		public WebParameterCollection(IEnumerable<WebPair> parameters)
			: base(parameters)
		{
		}

#if !WINDOWS_PHONE && !SILVERLIGHT
		public WebParameterCollection(NameValueCollection collection) : base(collection)
		{
		}
#endif

		public WebParameterCollection()
		{
		}

		public WebParameterCollection(int capacity) : base(capacity)
		{
		}

		public WebParameterCollection(IDictionary<string, string> collection) : base(collection)
		{
		}

		public override void Add(string name, string value)
		{
			var parameter = new WebParameter(name, value);
			base.Add(parameter);
		}
	}
}