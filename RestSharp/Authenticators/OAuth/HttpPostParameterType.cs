using System;

namespace RestSharp.Authenticators.OAuth
{
#if !SILVERLIGHT && !WINDOWS_PHONE && !PocketPC && !PORTABLE
    [Serializable]
#endif
    internal enum HttpPostParameterType
    {
        Field,
        File
    }
}
