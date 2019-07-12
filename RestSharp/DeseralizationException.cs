using System;

namespace RestSharp
{
    public class DeserializationException : Exception
    {
        public IRestResponse Response { get; }

        public DeserializationException(IRestResponse response, Exception innerException)
            : base("Error occured while deserializing the response", innerException)
            => Response = response;
    }
}
