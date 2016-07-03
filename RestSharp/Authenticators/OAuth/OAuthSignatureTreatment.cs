using System;
#if !NETCORE1
using System.Runtime.Serialization;
#endif

namespace RestSharp.Authenticators.OAuth
{
#if !SILVERLIGHT && !WINDOWS_PHONE && !WINDOWS_UWP && !NETCORE1
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
