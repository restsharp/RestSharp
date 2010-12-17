using System;

namespace RestSharp.Authenticators.OAuth
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public enum OAuthSignatureMethod
    {
        HmacSha1,
        PlainText,
        RsaSha1
    }
}