using System;
using System.Collections.Generic;
using System.Text;

namespace RestSharp.Interceptors {
   
    /// <summary>
    /// Interceptor, which intercepts outgoing requests and incoming responses
    /// </summary>
    public interface IInterceptor: IBeforeSerializationInterceptor,IBeforeRequestInterceptor, IAfterRequestInterceptor, IBeforeDeserializeInterceptor {

        
    }
}
