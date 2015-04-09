using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace RestSharp
{
    public interface IRequestMessage
    {
        HttpRequestMessage Instance {get;}
        void Configure(HttpMethod method, IHttpRequest request);
    }
}
