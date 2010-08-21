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
using System.Xml;
using System.Xml.Linq;
using RestSharp.Deserializers;

namespace RestSharp
{
	/// <summary>
	/// Client to translate RestRequests into Http requests and process response result
	/// </summary>
	public partial class RestClient : IRestClient
	{
		/// <summary>
		/// Default constructor that registers default content handlers
		/// </summary>
		public RestClient()
		{
			ContentHandlers = new Dictionary<string, IDeserializer>();
			AcceptTypes = new List<string>();

			// register default handlers
			AddHandler("application/json", new JsonDeserializer());
			AddHandler("application/xml", new XmlDeserializer());
			AddHandler("text/json", new JsonDeserializer());
			AddHandler("text/x-json", new JsonDeserializer());
			AddHandler("text/javascript", new JsonDeserializer());
			AddHandler("text/xml", new XmlDeserializer());
			AddHandler("*", new XmlDeserializer());

			UserAgent = "RestSharp Release 1";
		}

		/// <summary>
		/// Sets the BaseUrl property for requests made by this client instance
		/// </summary>
		/// <param name="baseUrl"></param>
		public RestClient(string baseUrl)
			: this()
		{
			BaseUrl = baseUrl;
		}

		private IDictionary<string, IDeserializer> ContentHandlers { get; set; }
		private IList<string> AcceptTypes { get; set; }

		/// <summary>
		/// Registers a content handler to process response content
		/// </summary>
		/// <param name="contentType">MIME content type of the response content</param>
		/// <param name="deserializer">Deserializer to use to process content</param>
		public void AddHandler(string contentType, IDeserializer deserializer)
		{
			ContentHandlers[contentType] = deserializer;
			if (contentType != "*")
			{
				AcceptTypes.Add(contentType);
			}
		}

		/// <summary>
		/// Remove a content handler for the specified MIME content type
		/// </summary>
		/// <param name="contentType">MIME content type to remove</param>
		public void RemoveHandler(string contentType)
		{
			ContentHandlers.Remove(contentType);
		}

		/// <summary>
		/// Remove all content handlers
		/// </summary>
		public void ClearHandlers()
		{
			ContentHandlers.Clear();
			AcceptTypes.Clear();
		}

		/// <summary>
		/// Retrieve the handler for the specified MIME content type
		/// </summary>
		/// <param name="contentType">MIME content type to retrieve</param>
		/// <returns>IDeserializer instance</returns>
		IDeserializer GetHandler(string contentType)
		{
			var semicolonIndex = contentType.IndexOf(';');
			if (semicolonIndex > -1) contentType = contentType.Substring(0, semicolonIndex);
			IDeserializer handler = null;
			if (ContentHandlers.ContainsKey(contentType))
			{
				handler = ContentHandlers[contentType];
			}
			else if (ContentHandlers.ContainsKey("*"))
			{
				handler = ContentHandlers["*"];
			}

			return handler;
		}

		/// <summary>
		/// UserAgent to use for requests made by this client instance
		/// </summary>
		public string UserAgent { get; set; }

		/// <summary>
		/// Timeout in milliseconds to use for requests made by this client instance
		/// </summary>
		public int Timeout { get; set; }

		/// <summary>
		/// Authenticator to use for requests made by this client instance
		/// </summary>
		public IAuthenticator Authenticator { get; set; }

		private string _baseUrl;
		/// <summary>
		/// Combined with Request.Resource to construct URL for request
		/// Should include scheme and domain without trailing slash.
		/// </summary>
		/// <example>
		/// client.BaseUrl = "http://example.com";
		/// </example>
		public string BaseUrl
		{
			get
			{
				return _baseUrl;
			}
			set
			{
				_baseUrl = value;
				if (_baseUrl.EndsWith("/"))
				{
					_baseUrl = _baseUrl.Substring(0, _baseUrl.Length - 1);
				}
			}
		}

		private void AuthenticateIfNeeded(RestRequest request)
		{
			if (Authenticator != null)
			{
				Authenticator.Authenticate(request);
			}
		}

		/// <summary>
		/// Assembles URL to call based on parameters, method and resource
		/// </summary>
		/// <param name="request">RestRequest to execute</param>
		/// <returns>Assembled System.Uri</returns>
		public Uri BuildUri(RestRequest request)
		{
			var assembled = request.Resource;
			var urlParms = request.Parameters.Where(p => p.Type == ParameterType.UrlSegment);
			foreach (var p in urlParms)
			{
				assembled = assembled.Replace("{" + p.Name + "}", p.Value.ToString());
			}

			if (!string.IsNullOrEmpty(assembled) && assembled.StartsWith("/"))
			{
				assembled = assembled.Substring(1);
			}

			return new Uri(string.Format("{0}/{1}", BaseUrl, assembled));
		}

		private void ConfigureHttp(RestRequest request, IHttp http)
		{
			http.Url = BuildUri(request);

			if (UserAgent != null)
			{
				http.UserAgent = UserAgent;
			}

			http.Timeout = request.Timeout == 0 ? Timeout : request.Timeout;

			if (request.Credentials != null)
			{
				http.Credentials = request.Credentials;
			}

			var headers = from p in request.Parameters
						  where p.Type == ParameterType.HttpHeader
						  select new HttpHeader
						  {
							  Name = p.Name,
							  Value = p.Value.ToString()
						  };

			foreach (var header in headers)
			{
				http.Headers.Add(header);
			}

			var cookies = from p in request.Parameters
						  where p.Type == ParameterType.Cookie
						  select new HttpCookie
						  {
							  Name = p.Name,
							  Value = p.Value.ToString()
						  };

			foreach (var cookie in cookies)
			{
				http.Cookies.Add(cookie);
			}

			var @params = from p in request.Parameters
						  where p.Type == ParameterType.GetOrPost
								&& p.Value != null
						  select new HttpParameter
						  {
							  Name = p.Name,
							  Value = p.Value.ToString()
						  };

			foreach (var parameter in @params)
			{
				http.Parameters.Add(parameter);
			}

			foreach (var file in request.Files)
			{
				http.Files.Add(new HttpFile { ContentType = file.ContentType, Data = file.Data, FileName = file.FileName });
			}

			var body = (from p in request.Parameters
						where p.Type == ParameterType.RequestBody
						select p).FirstOrDefault();

			if (body != null)
			{
				http.RequestBody = body.Value.ToString();
				http.RequestContentType = body.Name;
			}
		}

		private RestResponse ConvertToRestResponse(HttpResponse httpResponse)
		{
			var restResponse = new RestResponse();
			restResponse.Content = httpResponse.Content;
			restResponse.ContentEncoding = httpResponse.ContentEncoding;
			restResponse.ContentLength = httpResponse.ContentLength;
			restResponse.ContentType = httpResponse.ContentType;
			restResponse.ErrorMessage = httpResponse.ErrorMessage;
			restResponse.RawBytes = httpResponse.RawBytes;
			restResponse.ResponseStatus = httpResponse.ResponseStatus;
			restResponse.ResponseUri = httpResponse.ResponseUri;
			restResponse.Server = httpResponse.Server;
			restResponse.StatusCode = httpResponse.StatusCode;
			restResponse.StatusDescription = httpResponse.StatusDescription;

			foreach (var header in httpResponse.Headers)
			{
				restResponse.Headers.Add(new Parameter { Name = header.Name, Value = header.Value, Type = ParameterType.HttpHeader });
			}

			foreach (var cookie in httpResponse.Cookies)
			{
				restResponse.Cookies.Add(new Parameter { Name = cookie.Name, Value = cookie.Value, Type = ParameterType.Cookie });
			}

			return restResponse;
		}

		private RestResponse<T> Deserialize<T>(RestRequest request, RestResponse raw) where T : new()
		{
			IDeserializer handler = GetHandler(raw.ContentType);
			handler.RootElement = request.RootElement;
			handler.DateFormat = request.DateFormat;
			handler.Namespace = request.XmlNamespace;

			var response = new RestResponse<T>();
			try
			{
				response = (RestResponse<T>)raw;
				response.Data = handler.Deserialize<T>(raw);
			}
			catch (Exception ex)
			{
				response.ResponseStatus = ResponseStatus.Error;
				response.ErrorMessage = ex.Message;
			}

			return response;
		}

	}
}
