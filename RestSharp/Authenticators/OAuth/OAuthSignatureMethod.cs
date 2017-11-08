using System.Runtime.Serialization;

namespace RestSharp.Authenticators.OAuth
{
    [DataContract]
    public enum OAuthSignatureMethod
    {
        HmacSha1,
        HmacSha256,
        PlainText,
        RsaSha1
    }
}
