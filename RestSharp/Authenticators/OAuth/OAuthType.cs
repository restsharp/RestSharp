using System.Runtime.Serialization;

namespace RestSharp.Authenticators.OAuth
{
    [DataContract]
    public enum OAuthType
    {
        RequestToken,
        AccessToken,
        ProtectedResource,
        ClientAuthentication
    }
}
