using System;
using System.Runtime.Serialization;

namespace RestSharp.Authenticators.OAuth
{
#if !SILVERLIGHT && !WINDOWS_PHONE && !WINDOWS_UWP
    [Serializable]
#endif
#if WINDOWS_UWP
    [DataContract]
#endif
    public enum OAuthSignatureTreatment
    {
        Escaped,
        Unescaped
    }
}
