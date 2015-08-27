using System;

namespace RestSharp.Authenticators.OAuth
{
#if !SILVERLIGHT && !WINDOWS_PHONE && !PCL
    [Serializable]
#endif
    internal enum HttpPostParameterType
    {
        Field,
        File
    }
}
