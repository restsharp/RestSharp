using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using RestSharp.Extensions;

namespace RestSharp
{
    public class HttpClientWrapper : IHttpClient
    {
        HttpClient _instance;
        IMessageHandler _handler;

        public HttpClientWrapper()
        {

        }

        public HttpClientWrapper(IMessageHandler handler)
        {
            _handler = handler;
        }

        public System.Net.Http.HttpClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HttpClient(_handler.Instance);
                }
                return _instance;
            }
        }

        public void Configure(IHttpRequest request)
        {
            if (request.UserAgent.HasValue())
            {
                this.Instance.DefaultRequestHeaders.Add("user-agent", request.UserAgent);
            }

            if (request.Timeout != 0)
            {
                this.Instance.Timeout = new TimeSpan(0, 0, 0, 0, request.Timeout);
            }

            //ServicePointManager.Expect100Continue = false;
        }
    }
}
