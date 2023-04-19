using System;
using System.Collections.Generic;
using System.Text;

namespace RestSharp.Interceptors {
    public interface IBeforeSerializationInterceptor {
        /// <summary>
        /// Interceptor Function which will be called before before the Serialization
        /// </summary>
        /// <param name="request">Outgoing request</param>
        /// <returns>Result of the Excecution</returns>
        public ValueTask InterceptBeforeSerialization(RestRequest request);
    }
}
