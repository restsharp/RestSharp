using System;

namespace RestSharp.Authenticators.OAuth
{
#if !SILVERLIGHT && !WINDOWS_PHONE && !DNXCORE50
    [Serializable]
#endif
    internal enum HttpPostParameterType
    {
        Field,
        File
    }
}
