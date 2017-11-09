using System.Runtime.Serialization;

namespace RestSharp.Authenticators.OAuth
{
    [DataContract]
    public enum OAuthSignatureTreatment
    {
        Escaped,
        Unescaped
    }
}
