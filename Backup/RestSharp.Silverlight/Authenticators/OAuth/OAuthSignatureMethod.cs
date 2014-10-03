using System;

namespace RestSharp.Authenticators.OAuth
{
#if !SILVERLIGHT && !WINDOWS_PHONE && !PocketPC
	[Serializable]
#endif
	public enum OAuthSignatureMethod
	{
		HmacSha1,
		PlainText,
		RsaSha1
	}
}