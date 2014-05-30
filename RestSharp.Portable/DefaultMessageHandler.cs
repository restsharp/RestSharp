using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace RestSharp
{
    public class DefaultMessageHandler : IMessageHandler
    {
        private HttpClientHandler _instance;

        public DefaultMessageHandler() { }

        public DefaultMessageHandler(IHttpRequest request)
        {
            this.Configure(request);
        }

        public virtual HttpClientHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HttpClientHandler();
                }
                return _instance;
            }
        }

        public virtual void Configure(IHttpRequest request)
        {
            this.Instance.UseDefaultCredentials = request.UseDefaultCredentials;
            this.Instance.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.None;

            //TODO: Add PreAuthenticate header
            //this.Instance.PreAuthenticate = http.PreAuthenticate;

            if (request.Credentials != null)
            {
                this.Instance.Credentials = request.Credentials;
            }

            if (request.Proxy != null)
            {
                this.Instance.Proxy = request.Proxy;
            }

            //if (http.FollowRedirects && http.MaxRedirects.HasValue)
            //{
            //    this.Instance.MaxAutomaticRedirections = http.MaxRedirects.Value;
            //}

            if (request.MaxAutomaticRedirects.HasValue)
            {
                this.Instance.MaxAutomaticRedirections = request.MaxAutomaticRedirects.Value;
            }

            this.Instance.CookieContainer = request.CookieContainer ?? new CookieContainer();
            foreach (var httpCookie in request.Cookies)
            {
                var cookie = new Cookie
                {
                    Name = httpCookie.Name,
                    Value = Uri.EscapeDataString(httpCookie.Value),
                    Domain = request.Url.Host
                };
                this.Instance.CookieContainer.Add(request.Url, cookie);
            }

            request.CookieContainer = this.Instance.CookieContainer;
        }
    }
}
