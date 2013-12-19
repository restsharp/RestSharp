#if !Smartphone
using System;
using System.Diagnostics;

#endif

namespace RestSharp.Authenticators.OAuth
{
#if !Smartphone && !PocketPC
	[DebuggerDisplay("{Name}:{Value}")]
#endif
#if !SILVERLIGHT && !WINDOWS_PHONE && !PocketPC
	[Serializable]
#endif
	internal class WebParameter : WebPair
	{
		public WebParameter(string name, string value) : base(name, value)
		{
		}
	}
}