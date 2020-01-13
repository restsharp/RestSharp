using System;
using System.Net;

// ReSharper disable CheckNamespace

namespace RestSharp
{
    public partial class Http
    {
        [Obsolete]
        public HttpWebRequest DeleteAsync(Action<HttpResponse> action) => GetStyleMethodInternalAsync("DELETE", action);

        [Obsolete]
        public HttpWebRequest GetAsync(Action<HttpResponse> action) => GetStyleMethodInternalAsync("GET", action);

        [Obsolete]
        public HttpWebRequest HeadAsync(Action<HttpResponse> action) => GetStyleMethodInternalAsync("HEAD", action);

        [Obsolete]
        public HttpWebRequest OptionsAsync(Action<HttpResponse> action) => GetStyleMethodInternalAsync("OPTIONS", action);

        [Obsolete]
        public HttpWebRequest PostAsync(Action<HttpResponse> action) => PutPostInternalAsync("POST", action);

        [Obsolete]
        public HttpWebRequest PutAsync(Action<HttpResponse> action) => PutPostInternalAsync("PUT", action);

        [Obsolete]
        public HttpWebRequest PatchAsync(Action<HttpResponse> action) => PutPostInternalAsync("PATCH", action);

        [Obsolete]
        public HttpWebRequest MergeAsync(Action<HttpResponse> action) => PutPostInternalAsync("MERGE", action);

        [Obsolete("Use the WebRequestConfigurator delegate instead of overriding this method")]
        protected virtual HttpWebRequest ConfigureAsyncWebRequest(string method, Uri url) => ConfigureWebRequest(method, url);
    }
}