using System;
#if !NETSTANDARD
using System.Runtime.Serialization;
#endif

namespace RestSharp.Authenticators.OAuth
{
#if !SILVERLIGHT && !WINDOWS_PHONE && !WINDOWS_UWP && !NETSTANDARD
    [Serializable]
#endif
#if WINDOWS_UWP
    [DataContract]
#endif
    public enum OAuthParameterHandling
    {
        HttpAuthorizationHeader,
        UrlOrPostParameters
    }
}
