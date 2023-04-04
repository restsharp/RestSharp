using System;
using System.Collections.Generic;
using System.Text;

namespace RestSharp.Interceptors {
    /// <summary>
    /// Interceptor which will be executed after the response is comming back from server
    /// </summary>
    public interface IAfterRequestInterceptor {
        /// <summary>
        /// Interceptor Method which will be called when a message is comming back from server before it becomes deserialized
        /// </summary>
        /// <param name="responseMessage">Pure Http Response</param>
        /// <returns></returns>
        public ValueTask InterceptAfterRequest(HttpResponseMessage responseMessage);
    }
}
