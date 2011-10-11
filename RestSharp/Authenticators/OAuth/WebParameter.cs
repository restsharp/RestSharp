#if !Smartphone
using System;
using System.Diagnostics;

#endif

namespace RestSharp.Authenticators.OAuth
{
#if !Smartphone
	[DebuggerDisplay("{Name}:{Value}")]
#endif
#if !SILVERLIGHT && !WINDOWS_PHONE
	[Serializable]
#endif
	internal class WebParameter : WebPair
	{
		public WebParameter(string name, string value) : base(name, value)
		{
		}
	}
}