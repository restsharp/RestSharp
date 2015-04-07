using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace RestSharp
{
    public interface IHttpClient
    {
        HttpClient Instance { get; }

        void Configure(IHttpRequest request);
    }
}
