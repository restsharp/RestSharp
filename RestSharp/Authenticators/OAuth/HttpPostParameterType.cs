using System.Runtime.Serialization;

namespace RestSharp.Authenticators.OAuth
{
    [DataContract]
    internal enum HttpPostParameterType
    {
        Field,
        File
    }
}
