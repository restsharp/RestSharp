using System;

namespace RestSharp.Authenticators.OAuth
{
#if !SILVERLIGHT && !WINDOWS_PHONE && !WINDOWS_UWP
    [Serializable]
#endif
    internal enum HttpPostParameterType
    {
        Field,
        File
    }
}
