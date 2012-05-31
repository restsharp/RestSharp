﻿#region License
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
using System.Security.Cryptography.X509Certificates;
using RestSharp.Deserializers;
using RestSharp.Extensions;
using System.Text;
using System.Reflection;

namespace RestSharp
{
	/// <summary>
	/// Client to translate RestRequests into Http requests and process response result
	/// </summary>
	public partial class RestClient : IRestClient
	{
		public IHttpFactory HttpFactory = new SimpleFactory<Http>();

		/// <summary>
		/// Default constructor that registers default content handlers
		/// </summary>
		public RestClient()
		{
#if WINDOWS_PHONE
			UseSynchronizationContext = true;
#endif
			ContentHandlers = new Dictionary<string, IDeserializer>();
			AcceptTypes = new List<string>();
			DefaultParameters = new List<Parameter>();

			// register default handlers
			AddHandler("application/json", new JsonDeserializer());
			AddHandler("application/xml", new XmlDeserializer());
			AddHandler("text/json", new JsonDeserializer());
			AddHandler("text/x-json", new JsonDeserializer());
			AddHandler("text/javascript", new JsonDeserializer());
			AddHandler("text/xml", new XmlDeserializer());
			AddHandler("*", new XmlDeserializer());

			// silverlight friendly way to get current version
			var assembly = Assembly.GetExecutingAssembly();
			AssemblyName assemblyName = new AssemblyName(assembly.FullName);
			var version = assemblyName.Version;

			UserAgent = "RestSharp " + version.ToString();
			FollowRedirects = true;
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
		/// Parameters included with every request made with this instance of RestClient
		/// If specified in both client and request, the request wins
		/// </summary>
		public IList<Parameter> DefaultParameters { get; private set; }

		/// <summary>
		/// Add a parameter to use on every request made with this client instance
		/// </summary>
		/// <param name="p">Parameter to add</param>
		/// <returns></returns>
		public void AddDefaultParameter(Parameter p)
		{
			if (p.Type == ParameterType.RequestBody)
			{
				throw new NotSupportedException("Cannot set request body from default headers. Use Request.AddBody() instead.");
			}

			DefaultParameters.Add(p);
		}

		/// <summary>
		/// Adds a HTTP parameter (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT)
		/// Used on every request made by this client instance
		/// </summary>
		/// <param name="name">Name of the parameter</param>
		/// <param name="value">Value of the parameter</param>
		/// <returns>This request</returns>
		public void AddDefaultParameter(string name, object value)
		{
			AddDefaultParameter(new Parameter { Name = name, Value = value, Type = ParameterType.GetOrPost });
		}

		/// <summary>
		/// Adds a parameter to the request. There are four types of parameters:
		///	- GetOrPost: Either a QueryString value or encoded form value based on method
		///	- HttpHeader: Adds the name/value pair to the HTTP request's Headers collection
		///	- UrlSegment: Inserted into URL if there is a matching url token e.g. {AccountId}
		///	- RequestBody: Used by AddBody() (not recommended to use directly)
		/// </summary>
		/// <param name="name">Name of the parameter</param>
		/// <param name="value">Value of the parameter</param>
		/// <param name="type">The type of parameter to add</param>
		/// <returns>This request</returns>
		public void AddDefaultParameter(string name, object value, ParameterType type)
		{
			AddDefaultParameter(new Parameter { Name = name, Value = value, Type = type });
		}

		/// <summary>
		/// Shortcut to AddDefaultParameter(name, value, HttpHeader) overload
		/// </summary>
		/// <param name="name">Name of the header to add</param>
		/// <param name="value">Value of the header to add</param>
		/// <returns></returns>
		public void AddDefaultHeader(string name, string value)
		{
			AddDefaultParameter(name, value, ParameterType.HttpHeader);
		}

		/// <summary>
		/// Shortcut to AddDefaultParameter(name, value, UrlSegment) overload
		/// </summary>
		/// <param name="name">Name of the segment to add</param>
		/// <param name="value">Value of the segment to add</param>
		/// <returns></returns>
		public void AddDefaultUrlSegment(string name, string value)
		{
			AddDefaultParameter(name, value, ParameterType.UrlSegment);
		}

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
			AcceptTypes.Remove(contentType);
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
			if (string.IsNullOrEmpty(contentType) && ContentHandlers.ContainsKey("*"))
			{
				return ContentHandlers["*"];
			}

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
		/// Maximum number of redirects to follow if FollowRedirects is true
		/// </summary>
		public int? MaxRedirects { get; set; }

#if FRAMEWORK
		/// <summary>
		/// X509CertificateCollection to be sent with request
		/// </summary>
		public X509CertificateCollection ClientCertificates { get; set; }
#endif

		/// <summary>
		/// Default is true. Determine whether or not requests that result in 
		/// HTTP status codes of 3xx should follow returned redirect
		/// </summary>
		public bool FollowRedirects { get; set; }

		/// <summary>
		/// The CookieContainer used for requests made by this client instance
		/// </summary>
		public CookieContainer CookieContainer { get; set; }

		/// <summary>
		/// UserAgent to use for requests made by this client instance
		/// </summary>
		public string UserAgent { get; set; }

		/// <summary>
		/// Timeout in milliseconds to use for requests made by this client instance
		/// </summary>
		public int Timeout { get; set; }

		/// <summary>
		/// Whether to invoke async callbacks using the SynchronizationContext.Current captured when invoked
		/// </summary>
		public bool UseSynchronizationContext { get; set; }

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

		private void AuthenticateIfNeeded(RestClient client, IRestRequest request)
		{
			if (Authenticator != null)
			{
				Authenticator.Authenticate(client, request);
			}
		}

		/// <summary>
		/// Assembles URL to call based on parameters, method and resource
		/// </summary>
		/// <param name="request">RestRequest to execute</param>
		/// <returns>Assembled System.Uri</returns>
		public Uri BuildUri(IRestRequest request)
		{
			var assembled = request.Resource;
			var urlParms = request.Parameters.Where(p => p.Type == ParameterType.UrlSegment);
			foreach (var p in urlParms)
			{
				assembled = assembled.Replace("{" + p.Name + "}", p.Value.ToString().UrlEncode());
			}

			if (!string.IsNullOrEmpty(assembled) && assembled.StartsWith("/"))
			{
				assembled = assembled.Substring(1);
			}

			if (!string.IsNullOrEmpty(BaseUrl))
			{
				if (string.IsNullOrEmpty(assembled))
				{
					assembled = BaseUrl;
				}
				else
				{
					assembled = string.Format("{0}/{1}", BaseUrl, assembled);
				}
			}

			if (request.Method != Method.POST && request.Method != Method.PUT && request.Method != Method.PATCH)
			{
				// build and attach querystring if this is a get-style request
				if (request.Parameters.Any(p => p.Type == ParameterType.GetOrPost))
				{
					if (assembled.EndsWith("/"))
					{
						assembled = assembled.Substring(0, assembled.Length - 1);
					}

					var data = EncodeParameters(request);
					assembled = string.Format("{0}?{1}", assembled, data);
				}
			}

			return new Uri(assembled);
		}

		private string EncodeParameters(IRestRequest request)
		{
			var querystring = new StringBuilder();
			foreach (var p in request.Parameters.Where(p => p.Type == ParameterType.GetOrPost))
			{
				if (querystring.Length > 1)
					querystring.Append("&");
				querystring.AppendFormat("{0}={1}", p.Name.UrlEncode(), (p.Value.ToString()).UrlEncode());
			}

			return querystring.ToString();
		}

		private void ConfigureHttp(IRestRequest request, IHttp http)
		{
			http.CookieContainer = CookieContainer;

			// move RestClient.DefaultParameters into Request.Parameters
			foreach(var p in DefaultParameters)
			{
				if(request.Parameters.Any(p2 => p2.Name == p.Name && p2.Type == p.Type))
				{
					continue;
				}

				request.AddParameter(p);
			}

			http.Url = BuildUri(request);
			
			if(UserAgent.HasValue())
			{
				http.UserAgent = UserAgent;
			}

			http.Timeout = request.Timeout == 0 ? Timeout : request.Timeout;

#if !SILVERLIGHT
			http.FollowRedirects = FollowRedirects;
#endif
#if FRAMEWORK
			if (ClientCertificates != null)
			{
				http.ClientCertificates = ClientCertificates;
			}

			http.MaxRedirects = MaxRedirects;
#endif

			if(request.Credentials != null)
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

			foreach(var header in headers)
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

			foreach(var cookie in cookies)
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

			foreach(var parameter in @params)
			{
				http.Parameters.Add(parameter);
			}

			foreach(var file in request.Files)
			{
				http.Files.Add(new HttpFile { Name = file.Name, ContentType = file.ContentType, Writer = file.Writer, FileName = file.FileName, ContentLength = file.ContentLength });
			}

			var body = (from p in request.Parameters
						where p.Type == ParameterType.RequestBody
						select p).FirstOrDefault();

			if(body != null)
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
			restResponse.ErrorException = httpResponse.ErrorException;
			restResponse.ErrorMessage = httpResponse.ErrorMessage;
			restResponse.RawBytes = httpResponse.RawBytes;
			restResponse.ResponseStatus = httpResponse.ResponseStatus;
			restResponse.ResponseUri = httpResponse.ResponseUri;
			restResponse.Server = httpResponse.Server;
			restResponse.StatusCode = httpResponse.StatusCode;
			restResponse.StatusDescription = httpResponse.StatusDescription;
            restResponse.UserState = httpResponse.UserState;

			foreach (var header in httpResponse.Headers)
			{
				restResponse.Headers.Add(new Parameter { Name = header.Name, Value = header.Value, Type = ParameterType.HttpHeader });
			}

			foreach (var cookie in httpResponse.Cookies)
			{
				restResponse.Cookies.Add(new RestResponseCookie {
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

			return restResponse;
		}

		private IRestResponse<T> Deserialize<T>(IRestRequest request, IRestResponse raw)
		{
			request.OnBeforeDeserialization(raw);

			IDeserializer handler = GetHandler(raw.ContentType);
			handler.RootElement = request.RootElement;
			handler.DateFormat = request.DateFormat;
			handler.Namespace = request.XmlNamespace;

			IRestResponse<T> response = new RestResponse<T>();
			try
			{
			    response = raw.toAsyncResponse<T>();
				response.Data = handler.Deserialize<T>(raw);
			}
			catch (Exception ex)
			{
				response.ResponseStatus = ResponseStatus.Error;
				response.ErrorMessage = ex.Message;
				response.ErrorException = ex;
			}

			return response;
		}
	}
}
