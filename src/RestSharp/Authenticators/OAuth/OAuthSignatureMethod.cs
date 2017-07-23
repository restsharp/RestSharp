using System;
#if !NETSTANDARD1_4
using System.Runtime.Serialization;
#endif

namespace RestSharp.Authenticators.OAuth
{
#if !SILVERLIGHT && !WINDOWS_PHONE && !WINDOWS_UWP && !NETSTANDARD1_4
    [Serializable]
#endif
#if WINDOWS_UWP
    [DataContract]
#endif
    public enum OAuthSignatureMethod
    {
        HmacSha1,
        HmacSha256,
        PlainText,
        RsaSha1
    }
}
