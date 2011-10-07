using System;

namespace RestSharp.Authenticators.OAuth
{
#if !SILVERLIGHT && !WINDOWS_PHONE
	[Serializable]
#endif
	public enum OAuthParameterHandling
	{
		HttpAuthorizationHeader,
		UrlOrPostParameters
	}
}