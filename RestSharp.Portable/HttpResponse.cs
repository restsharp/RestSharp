#region License
//   Copyright 2010 John Sheehan
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
#endregion

using System;
using System.Collections.Generic;
using System.Net;
using RestSharp.Extensions;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestSharp
{
	/// <summary>
	/// HTTP response data
	/// </summary>
	public class HttpResponse : IHttpResponse
	{
		private string _content;

		/// <summary>
		/// Default constructor
		/// </summary>
		public HttpResponse()
		{
			Headers = new List<HttpHeader>();
			Cookies = new List<HttpCookie>();

            this.ResponseStatus = ResponseStatus.None;
		}

        /// <summary>
        /// The System.Net.CookieContainer to be used for the request
        /// </summary>
        public CookieContainer CookieContainer { get; set; }

		/// <summary>
		/// MIME content type of response
		/// </summary>
		public MediaTypeHeaderValue ContentType { get; set; }
		/// <summary>
		/// Length in bytes of the response content
		/// </summary>
		public long? ContentLength { get; set; }
		/// <summary>
		/// Encoding of the response content
		/// </summary>
		public ICollection<string> ContentEncoding { get; set; }
		/// <summary>
		/// Lazy-loaded string representation of response content
		/// </summary>
		public string Content
		{
			get
			{
				if (_content == null)
				{
					_content = RawBytes.AsString();
				}
				return _content;
			}
		}
		/// <summary>
		/// HTTP response status code
		/// </summary>
		public HttpStatusCode StatusCode { get; set; }
		/// <summary>
		/// Description of HTTP status returned
		/// </summary>
		public string StatusDescription { get; set; }
		/// <summary>
		/// Response content
		/// </summary>
		public byte[] RawBytes { get; set; }
		/// <summary>
		/// The URL that actually responded to the content (different from request if redirected)
		/// </summary>
		public Uri ResponseUri { get; set; }
		/// <summary>
		/// HttpWebResponse.Server
		/// </summary>
        public HttpHeaderValueCollection<ProductInfoHeaderValue> Server { get; set; }
		/// <summary>
		/// Headers returned by server with the response
		/// </summary>
		public IList<HttpHeader> Headers { get; private set; }
		/// <summary>
		/// Cookies returned by server with the response
		/// </summary>
		public IList<HttpCookie> Cookies { get; private set; }

		private ResponseStatus _responseStatus = ResponseStatus.None;
		/// <summary>
		/// Status of the request. Will return Error for transport errors.
		/// HTTP errors will still return ResponseStatus.Completed, check StatusCode instead
		/// </summary>
		public ResponseStatus ResponseStatus
		{
			get
			{
				return _responseStatus;
			}
			set
			{
				_responseStatus = value;
			}
		}

		/// <summary>
		/// Transport or other non-HTTP error generated while attempting request
		/// </summary>
		public string ErrorMessage { get; set; }

		/// <summary>
		/// Exception thrown when error is encountered.
		/// </summary>
		public Exception ErrorException { get; set; }

        internal async Task ConvertFromResponseMessage(HttpResponseMessage responseMessage)
        {
            this.Server = responseMessage.Headers.Server;
            this.StatusCode = responseMessage.StatusCode;
            this.StatusDescription = responseMessage.ReasonPhrase;
            this.ResponseUri = responseMessage.RequestMessage.RequestUri;
            this.ResponseStatus = ResponseStatus.Completed;

            if (responseMessage.Content != null)
            {
                this.ContentEncoding = responseMessage.Content.Headers.ContentEncoding;
                this.ContentType = responseMessage.Content.Headers.ContentType;
                this.ContentLength = responseMessage.Content.Headers.ContentLength;

                byte[] bytes = await responseMessage.Content.ReadAsByteArrayAsync();
                this.RawBytes = bytes;

                //if (http.ResponseWriter == null)
                //{
                //    byte[] bytes = await responseMessage.Content.ReadAsByteArrayAsync();
                //    this.RawBytes = bytes;
                //}
                //else
                //{
                //    http.ResponseWriter(await responseMessage.Content.ReadAsStreamAsync());
                //}
            }
            
            //TODO: Do we need to copy the request CookieContainer over?
            if (this.CookieContainer == null) { this.CookieContainer = new CookieContainer(); }

            var cookies = this.CookieContainer.GetCookies(responseMessage.RequestMessage.RequestUri);

            foreach (Cookie cookie in cookies)
            {
                this.Cookies.Add(new HttpCookie
                {
                    Comment = cookie.Comment,
                    CommentUri = cookie.CommentUri,
                    Discard = cookie.Discard,
                    Domain = cookie.Domain,
                    Expired = cookie.Expired,
                    Expires = cookie.Expires,
                    HttpOnly = cookie.HttpOnly,
                    Name = cookie.Name,
                    Path = cookie.Path,
                    Port = cookie.Port,
                    Secure = cookie.Secure,
                    TimeStamp = cookie.TimeStamp,
                    Value = cookie.Value,
                    Version = cookie.Version
                });
            }

            foreach (var header in responseMessage.Headers)
            {
                this.Headers.Add(new HttpHeader { Name = header.Key, Value = header.Value });
            }
        }
	}
}
