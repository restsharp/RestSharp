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
using RestSharp.Extensions;

namespace RestSharp
{
	/// <summary>
	/// HttpWebRequest wrapper
	/// </summary>
	public partial class Http : IHttp
	{
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
				return !string.IsNullOrEmpty(RequestBody);
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
		/// <summary>
		/// Request body to be sent with request
		/// </summary>
		public string RequestBody { get; set; }
		/// <summary>
		/// Content type of the request body.
		/// </summary>
		public string RequestContentType { get; set; }
		/// <summary>
		/// URL to call for this request
		/// </summary>
		public Uri Url { get; set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public Http()
		{
			Headers = new List<HttpHeader>();
			Files = new List<HttpFile>();
			Parameters = new List<HttpParameter>();
			Cookies = new List<HttpCookie>();

			_restrictedHeaderActions = new Dictionary<string, Action<HttpWebRequest, string>>(StringComparer.OrdinalIgnoreCase);

			AddSharedHeaderActions();
			AddSyncHeaderActions();
		}

		partial void AddSyncHeaderActions();
		partial void AddAsyncHeaderActions();
		private void AddSharedHeaderActions()
		{
			_restrictedHeaderActions.Add("Accept", (r, v) => r.Accept = v);
			_restrictedHeaderActions.Add("Content-Type", (r, v) => r.ContentType = v);
			_restrictedHeaderActions.Add("Date", (r, v) => { /* Set by system */ });
			_restrictedHeaderActions.Add("Host", (r, v) => { /* Set by system */ });
			_restrictedHeaderActions.Add("Range", (r, v) => { /* Ignore */ });
		}

		private const string FormBoundary = "-----------------------------28947758029299";
		private string GetMultipartFormContentType()
		{
			return string.Format("multipart/form-data; boundary={0}", FormBoundary);
		}

		private readonly IDictionary<string, Action<HttpWebRequest, string>> _restrictedHeaderActions;

		// handle restricted headers the .NET way - thanks @dimebrain!
		// http://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.headers.aspx
		private void AppendHeaders(HttpWebRequest webRequest)
		{
			foreach (var header in Headers)
			{
				if (_restrictedHeaderActions.ContainsKey(header.Name))
				{
					_restrictedHeaderActions[header.Name].Invoke(webRequest, header.Value);
				}
				else
				{
#if FRAMEWORK
					webRequest.Headers.Add(header.Name, header.Value);
#else
					webRequest.Headers[header.Name] = header.Value;
#endif
				}
			}
		}

		private void AppendCookies(HttpWebRequest webRequest)
		{
			webRequest.CookieContainer = new CookieContainer();
			foreach (var httpCookie in Cookies)
			{
				var cookie = new Cookie
				{
					Name = httpCookie.Name,
					Value = httpCookie.Value,
					Domain = webRequest.RequestUri.Host
				};
#if FRAMEWORK
				webRequest.CookieContainer.Add(cookie);
#else
				var uri = webRequest.RequestUri;
				webRequest.CookieContainer.Add(new Uri(string.Format("{0}://{1}", uri.Scheme, uri.Host)), cookie);
#endif
			}
		}

		private Uri AssembleUrl()
		{
			string url = Url.ToString();
			if (HasParameters)
			{
				if (url.EndsWith("/"))
				{
					url = url.Substring(0, url.Length - 1);
				}
				var data = EncodeParameters();
				url = string.Format("{0}?{1}", url, data);
			}
			return new Uri(url);
		}

		private string EncodeParameters()
		{
			var querystring = new StringBuilder();
			foreach (var p in Parameters)
			{
				if (querystring.Length > 1)
					querystring.Append("&");
				querystring.AppendFormat("{0}={1}", p.Name.UrlEncode(), p.Value.UrlEncode());
			}

			return querystring.ToString();
		}

		private void PreparePostBody(HttpWebRequest webRequest)
		{
			if (HasParameters)
			{
				webRequest.ContentType = "application/x-www-form-urlencoded";
				RequestBody = EncodeParameters();
			}
			else if (HasBody)
			{
				webRequest.ContentType = RequestContentType;
			}
		}

		private void ExtractResponseData(HttpResponse response, HttpWebResponse webResponse)
		{
			using (webResponse)
			{
#if FRAMEWORK
				response.ContentEncoding = webResponse.ContentEncoding;
				response.Server = webResponse.Server;
#endif
				response.ContentType = webResponse.ContentType;
				response.ContentLength = webResponse.ContentLength;
				response.RawBytes = webResponse.GetResponseStream().ReadAsBytes();
				response.Content = GetString(response.RawBytes);
				response.StatusCode = webResponse.StatusCode;
				response.StatusDescription = webResponse.StatusDescription;
				response.ResponseUri = webResponse.ResponseUri;
				response.ResponseStatus = ResponseStatus.Completed;

				if (webResponse.Cookies != null)
				{
					foreach (Cookie cookie in webResponse.Cookies)
					{
						response.Cookies.Add(new HttpCookie { Name = cookie.Name, Value = cookie.Value });
					}
				}

				foreach (var headerName in webResponse.Headers.AllKeys)
				{
					var headerValue = webResponse.Headers[headerName];
					response.Headers.Add(new HttpHeader { Name = headerName, Value = headerValue });
				}

				webResponse.Close();
			}
		}

		/// <summary>
		/// Converts a byte array to a string, using its byte order mark to convert it to the right encoding.
		/// http://www.shrinkrays.net/code-snippets/csharp/an-extension-method-for-converting-a-byte-array-to-a-string.aspx
		/// </summary>
		/// <param name="buffer">An array of bytes to convert</param>
		/// <returns>The byte as a string.</returns>
		public string GetString(byte[] buffer)
		{
			if (buffer == null || buffer.Length == 0)
				return "";

			// Ansi as default
			Encoding encoding = Encoding.UTF8;

			/*
				EF BB BF		UTF-8 
				FF FE UTF-16	little endian 
				FE FF UTF-16	big endian 
				FF FE 00 00		UTF-32, little endian 
				00 00 FE FF		UTF-32, big-endian 
				*/

			if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
			{
				encoding = Encoding.UTF8;
			}
			else if (buffer[0] == 0xfe && buffer[1] == 0xff)
			{
				encoding = Encoding.Unicode;
			}
			else if (buffer[0] == 0xfe && buffer[1] == 0xff)
			{
				encoding = Encoding.BigEndianUnicode; // utf-16be
			}
#if FRAMEWORK
			else if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0xfe && buffer[3] == 0xff)
			{
				encoding = Encoding.UTF32;
			}
			else if (buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76)
			{
				encoding = Encoding.UTF7;
			}
#endif
			using (MemoryStream stream = new MemoryStream())
			{
				stream.Write(buffer, 0, buffer.Length);
				stream.Seek(0, SeekOrigin.Begin);
				using (StreamReader reader = new StreamReader(stream, encoding))
				{
					return reader.ReadToEnd();
				}
			}
		}
	}
}