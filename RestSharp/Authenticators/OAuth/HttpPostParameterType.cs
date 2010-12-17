using System;

namespace RestSharp.Authenticators.OAuth
{
#if !SILVERLIGHT
    [Serializable]
#endif
    internal enum HttpPostParameterType
    {
        Field,
        File
    }
}