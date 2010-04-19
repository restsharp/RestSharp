using System;
using System.Runtime.Remoting.Messaging;

namespace RestSharp
{
    public delegate RestResponse RequestExecuteCaller(RestRequest request);
    public delegate RestResponse<T> RequestExecuteCaller<T>(RestRequest request);

    /// <summary>
    /// Experimental, not tested on Silverlight or Compact Framework
    /// </summary>
    public class AsyncRestClient : RestClient, IAsyncRestClient
    {
        public AsyncRestClient() : base() { }
        public AsyncRestClient(string baseUrl) : base(baseUrl) { }

        public IAsyncResult BeginExecute(RestRequest request, AsyncCallback callback, object state)
        {
            var requestExecuteCaller = new RequestExecuteCaller(this.Execute);
            return requestExecuteCaller.BeginInvoke(request, callback, state);
        }

        public RestResponse EndExecute(IAsyncResult asyncResult)
        {
            var res = (AsyncResult)asyncResult;
            var requestExecuteCaller = (RequestExecuteCaller)res.AsyncDelegate;
            return requestExecuteCaller.EndInvoke(asyncResult);
        }

        public IAsyncResult BeginExecute<T>(RestRequest request, AsyncCallback callback, object state) where T : new()
        {
            var requestExecuteCaller = new RequestExecuteCaller<T>(this.Execute<T>);
            return requestExecuteCaller.BeginInvoke(request, callback, state);
        }

        public RestResponse<T> EndExecute<T>(IAsyncResult asyncResult) where T : new()
        {
            var res = (AsyncResult)asyncResult;
            var requestExecuteCaller = (RequestExecuteCaller<T>)res.AsyncDelegate;
            return requestExecuteCaller.EndInvoke(asyncResult);
        }
    }
}
