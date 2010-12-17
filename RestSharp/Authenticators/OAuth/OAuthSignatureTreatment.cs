using System;

namespace RestSharp.Authenticators.OAuth
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public enum OAuthSignatureTreatment
    {
        Escaped,
        Unescaped
    }
}