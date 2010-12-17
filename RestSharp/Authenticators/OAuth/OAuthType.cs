using System;

namespace RestSharp.Authenticators.OAuth
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public enum OAuthType
    {
        RequestToken,
        AccessToken,
        ProtectedResource,
        ClientAuthentication
    }
}