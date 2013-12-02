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
using System.IO;
using System.Linq;
using System.Net;
//using System.Security.Cryptography.X509Certificates;
using System.Text;
using RestSharp.Extensions;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;

#if WINDOWS_PHONE
using RestSharp.Compression.ZLib;
#endif

namespace RestSharp
{
	/// <summary>
	/// HttpWebRequest wrapper
	/// </summary>
	public partial class Http : IHttp, IHttpFactory
    {
        #region Private Members

        //private const string _lineBreak = "\r\n";
		//private static readonly Encoding _defaultEncoding = Encoding.UTF8;

        //private const string FormBoundary = "-----------------------------28947758029299";
        private readonly IDictionary<string, Action<HttpRequestMessage, IEnumerable<string>>> _restrictedHeaderActions;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public Http()
        {
            Headers = new List<HttpHeader>();
            Files = new List<HttpFile>();
            //Parameters = new List<HttpParameter>();
            Parameters = new List<KeyValuePair<string,string>>();
            Cookies = new List<HttpCookie>();

            _restrictedHeaderActions = new Dictionary<string, Action<HttpRequestMessage, IEnumerable<string>>>(StringComparer.OrdinalIgnoreCase);

            AddSharedHeaderActions();
            AddSyncHeaderActions();
        }
        #endregion

        #region Protected Properties

        /// <summary>
		/// True if this HTTP request has any HTTP parameters
		/// </summary>
		protected bool HasParameters
		{
			get
			{
				return Parameters.Any();
			}
		}

		/// <summary>
		/// True if this HTTP request has any HTTP cookies
		/// </summary>
		protected bool HasCookies
		{
			get
			{
				return Cookies.Any();
			}
		}

		/// <summary>
		/// True if a request body has been specified
		/// </summary>
		protected bool HasBody
		{
			get
			{
				return RequestBodyBytes != null || !string.IsNullOrEmpty(RequestBody);
			}
		}

		/// <summary>
		/// True if files have been set to be uploaded
		/// </summary>
		protected bool HasFiles
		{
			get
			{
				return Files.Any();
			}
		}

        #endregion

        #region Public Properties

        /// <summary>
		/// Always send a multipart/form-data request - even when no Files are present.
		/// </summary>
		public bool AlwaysMultipartFormData { get; set; }
		
		/// <summary>
		/// UserAgent to be sent with request
		/// </summary>
		public string UserAgent { get; set; }
		/// <summary>
		/// Timeout in milliseconds to be used for the request
		/// </summary>
		public int Timeout { get; set; }
		/// <summary>
		/// System.Net.ICredentials to be sent with request
		/// </summary>
		public ICredentials Credentials { get; set; }
		/// <summary>
		/// The System.Net.CookieContainer to be used for the request
		/// </summary>
		public CookieContainer CookieContainer { get; set; }
		/// <summary>
		/// The method to use to write the response instead of reading into RawBytes
		/// </summary>
		public Action<Stream> ResponseWriter { get; set; }
		/// <summary>
		/// Collection of files to be sent with request
		/// </summary>
		public IList<HttpFile> Files { get; private set; }
		/// <summary>
		/// Whether or not HTTP 3xx response redirects should be automatically followed
		/// </summary>
		public bool FollowRedirects { get; set; }
		/// <summary>
		/// X509CertificateCollection to be sent with request
		/// </summary>
		//public X509CertificateCollection ClientCertificates { get; set; }

		/// <summary>
		/// Maximum number of automatic redirects to follow if FollowRedirects is true
		/// </summary>
		public int? MaxRedirects { get; set; }
		/// <summary>
		/// Determine whether or not the "default credentials" (e.g. the user account under which the current process is running)
		/// will be sent along to the server.
		/// </summary>
		public bool UseDefaultCredentials { get; set; }
		/// <summary>
		/// HTTP headers to be sent with request
		/// </summary>
		public IList<HttpHeader> Headers { get; private set; }
		/// <summary>
		/// HTTP parameters (QueryString or Form values) to be sent with request
		/// </summary>
		//public IList<HttpParameter> Parameters { get; private set; }
        public IList<KeyValuePair<string, string>> Parameters { get; private set; }
		/// <summary>
		/// HTTP cookies to be sent with request
		/// </summary>
		public IList<HttpCookie> Cookies { get; private set; }
		/// <summary>
		/// Request body to be sent with request
		/// </summary>
		public string RequestBody { get; set; }
		/// <summary>
		/// Content type of the request body.
		/// </summary>
		public string RequestContentType { get; set; }
		/// <summary>
		/// An alternative to RequestBody, for when the caller already has the byte array.
		/// </summary>
		public byte[] RequestBodyBytes { get; set; }
		/// <summary>
		/// URL to call for this request
		/// </summary>
		public Uri Url { get; set; }
		/// <summary>
		/// Proxy info to be sent with request
		/// </summary>
		public IWebProxy Proxy { get; set; }

        #endregion

        #region Public Static Methods

        ///<summary>
        /// Creates an IHttp
        ///</summary>
        ///<returns></returns>
        public IHttp Create()
        {
            return new Http();
        }

        #endregion

        #region Public Methods

        public async Task<HttpResponse> DeleteAsync()
        {
            return await GetStyleMethodInternalAsync("DELETE");
        }

        public async Task<HttpResponse> GetAsync()
        {
            return await GetStyleMethodInternalAsync("GET");
        }

        public async Task<HttpResponse> HeadAsync()
        {
            return await GetStyleMethodInternalAsync("HEAD");
        }

        public async Task<HttpResponse> OptionsAsync()
        {
            return await GetStyleMethodInternalAsync("OPTIONS");
        }

        public async Task<HttpResponse> PostAsync()
        {
            return await PutPostInternalAsync("POST");
        }

        public async Task<HttpResponse> PutAsync()
        {
            return await PutPostInternalAsync("PUT");
        }

        public async Task<HttpResponse> PatchAsync()
        {
            return await PutPostInternalAsync("PATCH");
        }

        /// <summary>
        /// Execute an async GET-style request with the specified HTTP Method.  
        /// </summary>
        /// <param name="httpMethod">The HTTP method to execute.</param>
        /// <returns></returns>
        public async Task<HttpResponse> AsGetAsync(string httpMethod)
        {
            return await GetStyleMethodInternalAsync(httpMethod.ToUpperInvariant());
        }

        /// <summary>
        /// Execute an async POST-style request with the specified HTTP Method.  
        /// </summary>
        /// <param name="httpMethod">The HTTP method to execute.</param>
        /// <returns></returns>
        public async Task<HttpResponse> AsPostAsync(string httpMethod)
        {
            return await PutPostInternalAsync(httpMethod.ToUpperInvariant());
        }

        #endregion

        #region Private Static Methods

        //private static MediaTypeHeaderValue GetMultipartFormContentType()
        //{
        //    return new MediaTypeHeaderValue(string.Format("multipart/form-data; boundary={0}", FormBoundary));
        //}

        //private static string GetMultipartFileHeader(HttpFile file)
        //{
        //    return string.Format("--{0}{4}Content-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"{4}Content-Type: {3}{4}{4}",
        //        FormBoundary, file.Name, file.FileName, file.ContentType ?? "application/octet-stream", _lineBreak);
        //}

        //private static string GetMultipartFormData(HttpParameter param)
        //{
        //    return string.Format("--{0}{3}Content-Disposition: form-data; name=\"{1}\"{3}{3}{2}{3}",
        //        FormBoundary, param.Name, param.Value, _lineBreak);
        //}

        //private static string GetMultipartFooter()
        //{
        //    return string.Format("--{0}--{1}", FormBoundary, _lineBreak);
        //}

        #endregion

        #region Private Methods

        /// <summary>
        /// Make requests to GET style HTTP verbs
        /// </summary>
        /// <remarks>Supports HTTP requests made using the DELETE, GET, HEAD and OPTIONS methods</remarks>
        /// <param name="method"></param>
        /// <returns></returns>
        private async Task<HttpResponse> GetStyleMethodInternalAsync(string method)
        {
            var handler = CreateClientHandler();
            var requestMessage = CreateRequestMessage(method);
            var client = CreateClient(handler);

            if (HasBody && (method == "DELETE" || method == "OPTIONS"))
            {
                requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(RequestContentType);
                requestMessage.Content = new StringContent(RequestBody);
            }

            var responseMessage = await client.SendAsync(requestMessage);

            var httpResponse = new HttpResponse();	
            httpResponse.ResponseStatus = ResponseStatus.None;

            ExtractResponseData(httpResponse, responseMessage);
            return httpResponse;
        }

        private async Task<HttpResponse> PutPostInternalAsync(string method)
        {
            var handler = CreateClientHandler();
            var requestMessage = CreateRequestMessage(method);
            var client = CreateClient(handler);

            //requestMessage.Content = new StringContent(RequestBody);
            //PreparePostData(requestMessage);
            PreparePostBody(requestMessage);

            var responseMessage = await client.SendAsync(requestMessage);

            var httpResponse = new HttpResponse();
            httpResponse.ResponseStatus = ResponseStatus.None;

            ExtractResponseData(httpResponse, responseMessage);
            return httpResponse;
        }

        private HttpClient CreateClient(HttpClientHandler handler) {
            
            HttpClient client = new HttpClient(handler);

            if (UserAgent.HasValue())
            {
                client.DefaultRequestHeaders.Add("user-agent", UserAgent);
            }

            if (Timeout != 0)
            {
                client.Timeout = new TimeSpan(0,0,0,0,Timeout);
            }

            //ServicePointManager.Expect100Continue = false;

            return client;
        }

        private HttpRequestMessage CreateRequestMessage(string method)
        {
            HttpRequestMessage message = new HttpRequestMessage(new HttpMethod(method), this.Url);
            
            // handle restricted headers the .NET way - thanks @dimebrain!
            // http://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.headers.aspx
            foreach (var header in Headers)
            {
                if (_restrictedHeaderActions.ContainsKey(header.Name))
                {
                    _restrictedHeaderActions[header.Name].Invoke(message, header.Value);
                }
                else
                {
                    message.Headers.Add(header.Name, header.Value);
                }
            }
        
            return message;
        }
        private HttpClientHandler CreateClientHandler()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.UseDefaultCredentials = UseDefaultCredentials;
            handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.None;

            if (Credentials != null)
            {
                handler.Credentials = Credentials;
            }

            //if (ClientCertificates != null)
            //{
            //    handler.ClientCertificateOptions = ClientCertificateOption.Manual;//.ClientCertificates.AddRange(ClientCertificates);

            //    //TODO: add client certificates to the request
            //    //handler.ClientCertificates = 
            //}

            if (Proxy != null)
            {
                handler.Proxy = Proxy;
            }

            if (FollowRedirects && MaxRedirects.HasValue)
            {
                handler.MaxAutomaticRedirections = MaxRedirects.Value;
            }

            handler.CookieContainer = this.CookieContainer ?? new CookieContainer();
            foreach (var httpCookie in Cookies)
            {
                var cookie = new Cookie
                {
                    Name = httpCookie.Name,
                    Value = httpCookie.Value,
                    Domain = this.Url.Host
                };
                handler.CookieContainer.Add(this.Url, cookie);
            }

            this.CookieContainer = handler.CookieContainer;
            
            
            return handler;
        }

        //private HttpContent WriteRequestBody()
        //{
        //    return new 
        //}

        //private string EncodeParameters()
        //{
        //    var querystring = new StringBuilder();
        //    foreach (var p in Parameters)
        //    {
        //        if (querystring.Length > 1)
        //            querystring.Append("&");
        //        querystring.AppendFormat("{0}={1}", p.Name.UrlEncode(), p.Value.UrlEncode());
        //    }

        //    return querystring.ToString();
        //}

        //private void PreparePostData(HttpRequestMessage message)
        //{
            //if (HasFiles || AlwaysMultipartFormData)
            //{
            //    //message.Content.Headers.ContentType = GetMultipartFormContentType();
            //    MultipartFormDataContent content = new MultipartFormDataContent();

            //    WriteMultipartFormData(new MultipartFormDataContent());
            //}

        //    PreparePostBody(message);
        //}

        private void PreparePostBody(HttpRequestMessage message)
        {
            if (HasFiles || AlwaysMultipartFormData)
            {
                //message.Content.Headers.ContentType = GetMultipartFormContentType();
                var content = new MultipartFormDataContent();

                foreach (var file in Files)
                {
                    content.Add(new ByteArrayContent(file.Data), file.Name, file.FileName);
                }

                if (HasParameters) { content.Add(new FormUrlEncodedContent(Parameters)); }
                if (HasBody) { content.Add(new StringContent(RequestBody)); }

                message.Content = content;
            }
            else if (HasParameters)
            {
               message.Content = new FormUrlEncodedContent(Parameters);
                //message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                //RequestBody = EncodeParameters();  //change to FormUrlEncodedContent

                //message.Content = content;
            }
            else if (HasBody)
            {
                message.Content.Headers.ContentType = new MediaTypeHeaderValue(RequestContentType);
                message.Content = new StringContent(RequestBody);
            }
        }

        //private static void WriteStringTo(Stream stream, string toWrite)
        //{
        //    var bytes = _defaultEncoding.GetBytes(toWrite);
        //    stream.Write(bytes, 0, bytes.Length);
        //}

        //private void WriteMultipartFormData(MultipartFormDataContent content)
        //{
        //    FormUrlEncodedContent c = new FormUrlEncodedContent(Parameters);

        //    foreach (var param in Parameters)
        //    {
                
        //        //WriteStringTo(requestStream, GetMultipartFormData(param));
        //        //content.Add(new StringContent(EncodeParameters()));
        //    }

            //foreach (var file in Files)
            //{
            //    content.Add(new ByteArrayContent(file.Data), file.Name);

                // Add just the first part of this param, since we will write the file data directly to the Stream
                //WriteStringTo(requestStream, GetMultipartFileHeader(file));

                // Write the file data directly to the Stream, rather than serializing it to a string.
                //file.Writer(requestStream);
                //WriteStringTo(requestStream, _lineBreak);
            //}

            //WriteStringTo(requestStream, GetMultipartFooter());
        //}

        private async void ProcessResponseStream(HttpContent content, HttpResponse response)
        {
            if (ResponseWriter == null)
            {
                response.RawBytes = await content.ReadAsByteArrayAsync();
            }
            else
            {
                ResponseWriter(await content.ReadAsStreamAsync());
            }
        }

        private void AddRange(HttpWebRequest r, string range)
        {
            System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(range, "=(\\d+)-(\\d+)$");
            if (!m.Success)
            {
                return;
            }

            int from = Convert.ToInt32(m.Groups[1].Value);
            int to = Convert.ToInt32(m.Groups[2].Value);
            //r.AddRange(from, to);
        }

        #endregion


        partial void AddSyncHeaderActions();
		partial void AddAsyncHeaderActions();
		private void AddSharedHeaderActions()
		{
			//_restrictedHeaderActions.Add("Accept", (r, v) => r.Accept = v);
			//_restrictedHeaderActions.Add("Content-Type", (r, v) => r.ContentType = v);
#if NET4
			_restrictedHeaderActions.Add("Date", (r, v) =>
				{
					DateTime parsed;
					if (DateTime.TryParse(v, out parsed))
					{
						r.Date = parsed;
					}
				});
			_restrictedHeaderActions.Add("Host", (r, v) => r.Host = v);
#else
			_restrictedHeaderActions.Add("Date", (r, v) => { /* Set by system */ });
			_restrictedHeaderActions.Add("Host", (r, v) => { /* Set by system */ });
#endif
            //_restrictedHeaderActions.Add("Range", (r, v) => { AddRange(r, v); });
		}


		private async void ExtractResponseData(HttpResponse response, HttpResponseMessage responseMessage)
		{
            response.ContentEncoding = responseMessage.Content.Headers.ContentEncoding;
            response.ContentType = responseMessage.Content.Headers.ContentType;
            response.ContentLength = responseMessage.Content.Headers.ContentLength;
            response.Server = responseMessage.Headers.Server;

            if (ResponseWriter == null)
            {
                byte[] bytes = await responseMessage.Content.ReadAsByteArrayAsync();
                response.RawBytes = bytes;
            }
            else
            {
                ResponseWriter(await responseMessage.Content.ReadAsStreamAsync());
            }

			response.StatusCode = responseMessage.StatusCode;
			response.StatusDescription = responseMessage.ReasonPhrase;
			response.ResponseUri = responseMessage.RequestMessage.RequestUri;
			response.ResponseStatus = ResponseStatus.Completed;

            
            var c = this.CookieContainer.GetCookies(this.Url);
			if (this.CookieContainer != null)
			{
                var cookies = this.CookieContainer.GetCookies(this.Url);

				foreach (Cookie cookie in cookies)
				{
					response.Cookies.Add(new HttpCookie {
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
			}

			foreach (var header in responseMessage.Headers)
			{
				response.Headers.Add(new HttpHeader { Name = header.Key, Value = header.Value });
			}        			
		}
	}
}