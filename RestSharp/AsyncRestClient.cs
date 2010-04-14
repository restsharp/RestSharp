using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp
{
    public delegate RestResponse RequestExecuteCaller(RestRequest request);
    public delegate RestResponse<T> RequestExecuteCaller<T>(RestRequest request);

    /// <summary>
    /// Experimental, not tested on Silverlight or Compact Framework
    /// </summary>
    public class AsyncRestClient : RestClient, IAsyncRestClient
    {
        public AsyncRestClient() : base()     {    }
        public AsyncRestClient(string baseUrl) : base(baseUrl) {  }


        private RequestExecuteCaller callback;
        private object genericCallback;

        public IAsyncResult BeginExecute(RestRequest request, AsyncCallback callback, object state)
        {
            this.callback = new RequestExecuteCaller(this.Execute);
            return this.callback.BeginInvoke(request, callback, state);
        }

        public RestResponse EndExecute(IAsyncResult asyncResult)
        {
            return this.callback.EndInvoke(asyncResult);
        }

        public IAsyncResult BeginExecute<T>(RestRequest request, AsyncCallback callback, object state) where T : new()
        {
            this.genericCallback = new RequestExecuteCaller<T>(this.Execute<T>);
            var cb = this.genericCallback as RequestExecuteCaller<T>;
            return cb.BeginInvoke(request, callback, state);
        }

        public RestResponse<T> EndExecute<T>(IAsyncResult asyncResult) where T : new()
        {
            var cb = this.genericCallback as RequestExecuteCaller<T>;
            return cb.EndInvoke(asyncResult);
        }
    }
}
