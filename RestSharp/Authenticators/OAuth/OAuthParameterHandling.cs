using System;

namespace RestSharp.Authenticators.OAuth
{
#if !SILVERLIGHT && !WINDOWS_PHONE && !PocketPC
	[Serializable]
#endif
	public enum OAuthParameterHandling
	{
		HttpAuthorizationHeader,
		UrlOrPostParameters
	}
}