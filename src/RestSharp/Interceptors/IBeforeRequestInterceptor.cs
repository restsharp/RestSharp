using System;
using System.Collections.Generic;
using System.Text;

namespace RestSharp.Interceptors {

    /// <summary>
    /// Interceptor which will be executed before the Request becomes sent
    /// </summary>
    public interface IBeforeRequestInterceptor {

        /// <summary>
        /// Interceptor Function called when before the request becomes send
        /// </summary>
        /// <param name="req">Raw Http Request ready to be sent</param>
        /// <returns>Result of the Interceptor Method</returns>
        public ValueTask InterceptBeforeRequest(HttpRequestMessage req);
    }
}
