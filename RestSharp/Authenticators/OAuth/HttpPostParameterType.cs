using System;
using System.Runtime.Serialization;

namespace RestSharp.Authenticators.OAuth
{
#if !SILVERLIGHT && !WINDOWS_PHONE && !WINDOWS_UWP && !DNXCORE50
    [Serializable]
#endif
#if WINDOWS_UWP
    [DataContract]
#endif 
    internal enum HttpPostParameterType
    {
        Field,
        File
    }
}
