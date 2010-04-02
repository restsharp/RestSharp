using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp
{
    public interface IAsyncRestClient : IRestClient
    {
        IAsyncResult BeginExecute(RestRequest request, AsyncCallback callback, object state);
        RestResponse EndExecute(IAsyncResult asyncResult);

        IAsyncResult BeginExecute<T>(RestRequest request, AsyncCallback callback, object state)  where T : new();
        RestResponse<T> EndExecute<T>(IAsyncResult asyncResult)  where T : new();
    }
}
