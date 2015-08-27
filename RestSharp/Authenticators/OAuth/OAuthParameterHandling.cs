using System;

namespace RestSharp.Authenticators.OAuth
{
#if !SILVERLIGHT && !WINDOWS_PHONE && !PCL
    [Serializable]
#endif
    public enum OAuthParameterHandling
    {
        HttpAuthorizationHeader,
        UrlOrPostParameters
    }
}
