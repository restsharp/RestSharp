using System;

namespace RestSharp
{
    public class DeserializationException : Exception
    {
        public DeserializationException(IRestResponse response, Exception innerException)
            : base("Error occured while deserializing the response", innerException)
            => Response = response;

        public IRestResponse Response { get; }
    }
}