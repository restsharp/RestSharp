using System;
using System.Collections.Generic;
using System.Text;

namespace RestSharp.Interceptors {
    /// <summary>
    /// Interceptor called when before the Response has been deserialized
    /// </summary>
    public interface IBeforeDeserializeInterceptor {
        /// <summary>
        /// Function called before the Response has been deserialized
        /// </summary>
        /// <param name="request">RestResponse With Raw Response Content</param>
        /// <returns>Result of Execution</returns>
        public ValueTask InterceptBeforeDeserialize(RestResponse request);
    }
}
