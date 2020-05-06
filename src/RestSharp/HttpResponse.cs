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
using System.Collections.Generic;
using System.Net;
using JetBrains.Annotations;
using RestSharp.Extensions;

namespace RestSharp
{
    /// <inheritdoc />
    [PublicAPI]
    public class HttpResponse : IHttpResponse
    {
        string _content;

        public HttpResponse()
        {
            ResponseStatus = ResponseStatus.None;
            Headers        = new List<HttpHeader>();
            Cookies        = new List<HttpCookie>();
        }

        /// <inheritdoc />
        public string ContentType { get; set; }

        /// <inheritdoc />
        public long ContentLength { get; set; }

        /// <inheritdoc />
        public string ContentEncoding { get; set; }

        /// <inheritdoc />
        public string Content => _content ??= RawBytes.AsString(ContentEncoding);

        /// <inheritdoc />
        public HttpStatusCode StatusCode { get; set; }

        /// <inheritdoc />
        public string StatusDescription { get; set; }

        /// <inheritdoc />
        public byte[] RawBytes { get; set; }

        /// <inheritdoc />
        public Uri ResponseUri { get; set; }

        /// <inheritdoc />
        public string Server { get; set; }

        /// <inheritdoc />
        public IList<HttpHeader> Headers { get; internal set; }

        /// <inheritdoc />
        public IList<HttpCookie> Cookies { get; }

        /// <inheritdoc />
        public ResponseStatus ResponseStatus { get; set; }

        /// <inheritdoc />
        public string ErrorMessage { get; set; }

        /// <inheritdoc />
        public Exception ErrorException { get; set; }

        /// <inheritdoc />
        public Version ProtocolVersion { get; set; }
    }
}