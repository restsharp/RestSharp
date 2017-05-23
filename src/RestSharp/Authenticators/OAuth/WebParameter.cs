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
    internal class WebParameter : WebPair
    {
        public WebParameter(string name, string value)
            : base(name, value) { }
    }
}
