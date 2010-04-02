using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp
{
    public delegate RestResponse RequestExecuteCallback(RestRequest request);
    public delegate RestResponse<T> RequestExecuteCallback<T>(RestRequest request);

    public class AsyncRestClient : RestClient, IAsyncRestClient
    {
        public AsyncRestClient() : base()     {    }
        public AsyncRestClient(string baseUrl) : base(baseUrl) {  }


        private RequestExecuteCallback callback;
        private object genericCallback;

        public IAsyncResult BeginExecute(RestRequest request, AsyncCallback callback, object state)
        {
            this.callback = new RequestExecuteCallback(this.Execute);
            return this.callback.BeginInvoke(request, callback, state);
        }

        public RestResponse EndExecute(IAsyncResult asyncResult)
        {
            return this.callback.EndInvoke(asyncResult);
        }

        public IAsyncResult BeginExecute<T>(RestRequest request, AsyncCallback callback, object state) where T : new()
        {
            this.genericCallback = new RequestExecuteCallback<T>(this.Execute<T>);
            var cb = this.genericCallback as RequestExecuteCallback<T>;
            return cb.BeginInvoke(request, callback, state);
        }

        public RestResponse<T> EndExecute<T>(IAsyncResult asyncResult) where T : new()
        {
            var cb = this.genericCallback as RequestExecuteCallback<T>;
            return cb.EndInvoke(asyncResult);
        }
    }
}
