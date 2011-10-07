using System;

namespace RestSharp.Authenticators.OAuth
{
#if !SILVERLIGHT && !WINDOWS_PHONE
	[Serializable]
#endif
	internal enum HttpPostParameterType
	{
		Field,
		File
	}
}