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
using System.Text;

#if !SILVERLIGHT
using System.Web;
#else
using System.Windows.Browser;
#endif

using RestSharp.Extensions;

#if !SILVERLIGHT

namespace RestSharp
{
	/// <summary>
	/// HttpWebRequest wrapper
	/// </summary>
	public class Http : IHttp
	{
		/// <summary>
		/// True if this HTTP request has any HTTP parameters
		/// </summary>
		protected bool HasParameters {
			get {
				return Parameters.Any();
			}
		}

		/// <summary>
		/// True if this HTTP request has any HTTP cookies
		/// </summary>
		protected bool HasCookies {
			get {
				return Cookies.Any();
			}
		}

		/// <summary>
		/// True if a request body has been specified
		/// </summary>
		protected bool HasBody {
			get {
				return !string.IsNullOrEmpty(RequestBody);
			}
		}

		/// <summary>
		/// True if files have been set to be uploaded
		/// </summary>
		protected bool HasFiles {
			get {
				return Files.Any();
			}
		}

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
		/// Collection of files to be sent with request
		/// </summary>
		public IList<HttpFile> Files { get; private set; }
		/// <summary>
		/// HTTP headers to be sent with request
		/// </summary>
		public IList<HttpHeader> Headers { get; private set; }
		/// <summary>
		/// HTTP parameters (QueryString or Form values) to be sent with request
		/// </summary>
		public IList<HttpParameter> Parameters { get; private set; }
		/// <summary>
		/// HTTP cookies to be sent with request
		/// </summary>
		public IList<HttpCookie> Cookies { get; private set; }
#if !SILVERLIGHT
		/// <summary>
		/// Proxy info to be sent with request
		/// </summary>
		public IWebProxy Proxy { get; set; }
#endif
		/// <summary>
		/// Request body to be sent with request
		/// </summary>
		public string RequestBody { get; set; }
		/// <summary>
		/// Format of the request body. Used to set correct content type on request.
		/// </summary>
		public DataFormat RequestFormat { get; set; }
		/// <summary>
		/// Response returned from making this request
		/// </summary>
		public HttpResponse Response { get; set; }
		/// <summary>
		/// Response stream return from making this request.
		/// </summary>
		public Stream ResponseStream { get; set; }
		/// <summary>
		/// URL to call for this request
		/// </summary>
		public Uri Url { get; set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public Http() {
			Headers = new List<HttpHeader>();
			Files = new List<HttpFile>();
			Parameters = new List<HttpParameter>();
			Cookies = new List<HttpCookie>();
		}

		/// <summary>
		/// Execute a POST request
		/// </summary>
		public void Post() {
			PostPutInternal("POST");
		}

		/// <summary>
		/// Execute a PUT request
		/// </summary>
		public void Put() {
			PostPutInternal("PUT");
		}

		private void PostPutInternal(string method) {

			var webRequest = (HttpWebRequest)WebRequest.Create(Url);
			webRequest.Method = method;

#if !SILVERLIGHT
			webRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.None;

			if (UserAgent.HasValue()) {
				webRequest.UserAgent = UserAgent;
			}

			if (Timeout != 0) {
				webRequest.Timeout = Timeout;
			}

			if (Proxy != null)
			{
				webRequest.Proxy = Proxy;
			}
#endif
			if (Credentials != null) {
				webRequest.Credentials = Credentials;
			}

			AppendHeaders(webRequest);
			AppendCookies(webRequest);

			if (HasFiles) {
				webRequest.ContentType = GetMultipartFormContentType();
				WriteMultipartFormData(webRequest);
			}
			else {
				if (HasParameters) {
					webRequest.ContentType = "application/x-www-form-urlencoded";
					RequestBody = EncodeParameters();
				}
				else if (HasBody) {
					switch (RequestFormat) {
						case DataFormat.Xml:
							webRequest.ContentType = "text/xml";
							break;
						case DataFormat.Json:
							webRequest.ContentType = "application/json";
							break;
					}
				}
			}

			WriteRequestBody(webRequest);
			Response = GetResponse(webRequest);
		}

		private void WriteRequestBody(HttpWebRequest webRequest) {
			if (HasBody) {
				webRequest.ContentLength = RequestBody.Length;

				var requestStream = webRequest.GetRequestStream();
				using (var writer = new StreamWriter(requestStream, Encoding.ASCII)) {
					writer.Write(RequestBody);
				}
			}
		}

		private string _formBoundary = "-----------------------------28947758029299";
		private string GetMultipartFormContentType() {
			return string.Format("multipart/form-data; boundary={0}", _formBoundary);
		}

		private void WriteMultipartFormData(HttpWebRequest webRequest) {
			var boundary = _formBoundary;
			var encoding = Encoding.ASCII;
			using (Stream formDataStream = webRequest.GetRequestStream()) {
				foreach (var file in Files) {
					var fileName = file.FileName;
					var data = file.Data;
					var length = data.Length;
					var contentType = file.ContentType;
					// Add just the first part of this param, since we will write the file data directly to the Stream
					string header = string.Format("--{0}{3}Content-Disposition: form-data; name=\"{1}\"; filename=\"{1}\";{3}Content-Type: {2}{3}{3}",
													boundary,
													fileName,
													contentType ?? "application/octet-stream",
													Environment.NewLine);

					formDataStream.Write(encoding.GetBytes(header), 0, header.Length);
					// Write the file data directly to the Stream, rather than serializing it to a string.
					formDataStream.Write(data, 0, length);
					string lineEnding = Environment.NewLine;
					formDataStream.Write(encoding.GetBytes(lineEnding), 0, lineEnding.Length);
				}

				foreach (var param in Parameters) {
					var postData = string.Format("--{0}{3}Content-Disposition: form-data; name=\"{1}\"{3}{3}{2}{3}",
													boundary,
													param.Name,
													param.Value,
													Environment.NewLine);

					formDataStream.Write(encoding.GetBytes(postData), 0, postData.Length);
				}

				string footer = String.Format("{1}--{0}--{1}", boundary, Environment.NewLine);
				formDataStream.Write(encoding.GetBytes(footer), 0, footer.Length);
			}
		}

		private string EncodeParameters() {
			var querystring = new StringBuilder();
			foreach (var p in Parameters) {
				if (querystring.Length > 1)
					querystring.Append("&");
				querystring.AppendFormat("{0}={1}", HttpUtility.UrlEncode(p.Name), HttpUtility.UrlEncode(p.Value));
			}

			return querystring.ToString();
		}

		/// <summary>
		/// Execute a GET request
		/// </summary>
		public void Get() {
			GetStyleMethodInternal("GET");
		}

		/// <summary>
		/// Execute a HEAD request
		/// </summary>
		public void Head() {
			GetStyleMethodInternal("HEAD");
		}

		/// <summary>
		/// Execute an OPTIONS request
		/// </summary>
		public void Options() {
			GetStyleMethodInternal("OPTIONS");
		}

		/// <summary>
		/// Execute a DELETE request
		/// </summary>
		public void Delete() {
			GetStyleMethodInternal("DELETE");
		}

		private void GetStyleMethodInternal(string method) {
			string url = Url.ToString();
			if (HasParameters) {
				if (url.EndsWith("/")) {
					url = url.Substring(0, url.Length - 1);
				}
				var data = EncodeParameters();
				url = string.Format("{0}?{1}", url, data);
			}

			var webRequest = (HttpWebRequest)WebRequest.Create(url);
			webRequest.Method = method;

#if !SILVERLIGHT
			webRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.None;

			if (UserAgent.HasValue()) {
				webRequest.UserAgent = UserAgent;
			}

			if (Timeout != 0) {
				webRequest.Timeout = Timeout;
			}
#endif

			if (Credentials != null) {
				webRequest.Credentials = Credentials;
			}

#if !SILVERLIGHT
			if (Proxy != null) {
				webRequest.Proxy = Proxy;
			}
#endif
			AppendHeaders(webRequest);
			AppendCookies(webRequest);
			Response = GetResponse(webRequest);
		}

		// handle restricted headers the .NET way - thanks @dimebrain!
		// http://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.headers.aspx
		private void AppendHeaders(HttpWebRequest webRequest) {
			foreach (var header in Headers) {
				if (_restrictedHeaderActions.ContainsKey(header.Name)) {
					_restrictedHeaderActions[header.Name].Invoke(webRequest, header.Value);
				}
				else {
					webRequest.Headers.Add(header.Name, header.Value);
				}
			}
		}

		private void AppendCookies(HttpWebRequest webRequest) {
			webRequest.CookieContainer = new CookieContainer();
			foreach (var httpCookie in Cookies) {
				var cookie = new Cookie {
					Name = httpCookie.Name,
					Value = httpCookie.Value,
					Domain = webRequest.RequestUri.Host
				};
				webRequest.CookieContainer.Add(cookie);
			}
		}

		private readonly IDictionary<string, Action<HttpWebRequest, string>> _restrictedHeaderActions
			= new Dictionary<string, Action<HttpWebRequest, string>>(StringComparer.OrdinalIgnoreCase) {
                      { "Accept",            (r, v) => r.Accept = v },
                      { "Connection",        (r, v) => r.Connection = v },           
                      { "Content-Length",    (r, v) => r.ContentLength = Convert.ToInt64(v) },
                      { "Content-Type",      (r, v) => r.ContentType = v },
                      { "Expect",            (r, v) => r.Expect = v },
                      { "Date",              (r, v) => { /* Set by system */ }},
                      { "Host",              (r, v) => { /* Set by system */ }},
                      { "If-Modified-Since", (r, v) => r.IfModifiedSince = Convert.ToDateTime(v) },
                      { "Range",             (r, v) => { throw new NotImplementedException(/* r.AddRange() */); }},
                      { "Referer",           (r, v) => r.Referer = v },
                      { "Transfer-Encoding", (r, v) => { r.TransferEncoding = v; r.SendChunked = true; } },
                      { "User-Agent",        (r, v) => r.UserAgent = v }             
                  };

		private HttpResponse GetResponse(HttpWebRequest request) {
			var response = new HttpResponse();
			response.ResponseStatus = ResponseStatus.None;

			try {
				var webResponse = GetRawResponse(request);
				using (webResponse) {
					response.ContentType = webResponse.ContentType;
					response.ContentLength = webResponse.ContentLength;
					response.ContentEncoding = webResponse.ContentEncoding;
					response.RawBytes = webResponse.GetResponseStream().ReadAsBytes();
					response.Content = response.RawBytes.ReadAsString();
					response.StatusCode = webResponse.StatusCode;
					response.StatusDescription = webResponse.StatusDescription;
					response.ResponseUri = webResponse.ResponseUri;
					response.Server = webResponse.Server;
					response.ResponseStatus = ResponseStatus.Completed;

					if (webResponse.Cookies != null) {
						foreach (Cookie cookie in webResponse.Cookies) {
							response.Cookies.Add(new HttpCookie { Name = cookie.Name, Value = cookie.Value });
						}
					}

					foreach (var headerName in webResponse.Headers.AllKeys) {
						var headerValue = webResponse.Headers[headerName];
						response.Headers.Add(new HttpHeader { Name = headerName, Value = headerValue });
					}

					webResponse.Close();
				}
			}
			catch (Exception ex) {
				response.ErrorMessage = ex.Message;
				response.ResponseStatus = ResponseStatus.Error;
			}

			return response;
		}

		private HttpWebResponse GetRawResponse(HttpWebRequest request) {
			HttpWebResponse raw = null;
			try {
				raw = (HttpWebResponse)request.GetResponse();
			}
			catch (WebException ex) {
				if (ex.Response is HttpWebResponse) {
					raw = ex.Response as HttpWebResponse;
				}
			}

			return raw;
		}
	}
}
#endif