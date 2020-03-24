//   Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

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