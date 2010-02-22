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
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using RestSharp.Deserializers;

namespace RestSharp
{
	public class RestClient : IRestClient
	{
		public RestClient() {
			ContentHandlers = new Dictionary<string, IDeserializer>();

			// register default handlers
			AddHandler("application/json", new JsonDeserializer());
			AddHandler("text/xml", new XmlDeserializer());
		}

		public RestClient(string baseUrl) : this() {
			BaseUrl = baseUrl;
		}

		private IDictionary<string, IDeserializer> ContentHandlers { get; set; }

		public void AddHandler(string contentType, IDeserializer deserializer) {
			ContentHandlers.Add(contentType, deserializer);
		}

		public void RemoveHandler(string contentType) {
			ContentHandlers.Remove(contentType);
		}

		public void ClearHandlers() {
			ContentHandlers.Clear();
		}

		IDeserializer GetHandler(string contentType) {
			return ContentHandlers[contentType];
		}

		public IAuthenticator Authenticator { get; set; }
		public IWebProxy Proxy { get; set; }
		public string BaseUrl { get; set; }

		public RestResponse Execute(RestRequest request) {
			AuthenticateIfNeeded(request);

			var response = GetResponse(request);
			return response;
		}

		private void AuthenticateIfNeeded(RestRequest request) {
			if (Authenticator != null) {
				Authenticator.Authenticate(request);
			}
		}

		public byte[] DownloadData(RestRequest request) {
			var response = Execute(request);
			return response.RawBytes;
		}

		public XDocument ExecuteAsXDocument(RestRequest request) {
			var response = Execute(request);
			return XDocument.Parse(response.Content);
		}

		public XmlDocument ExecuteAsXmlDocument(RestRequest request) {
			var response = Execute(request);
			var doc = new XmlDocument();
			doc.LoadXml(response.Content);
			return doc;
		}

		public T Execute<T>(RestRequest request) where T : new() {
			AuthenticateIfNeeded(request);

			var response = GetResponse(request);

			IDeserializer handler = null;

			switch (request.ResponseFormat) {
				case ResponseFormat.AutoDetect:
					handler = GetHandler(response.ContentType);
					break;
				case ResponseFormat.Json:
					handler = new JsonDeserializer();
					handler.DateFormat = request.DateFormat;
					break;
				case ResponseFormat.Xml:
					handler = new XmlDeserializer();
					handler.RootElement = request.RootElement;
					handler.Namespace = request.XmlNamespace;
					handler.DateFormat = request.DateFormat;
					break;
			}

			return handler != null ? handler.Deserialize<T>(response) : default(T);
		}

		private RestResponse GetResponse(RestRequest request) {
			IHttp http = new Http();
			http.Url = BuildUri(request);

			if (request.Credentials != null) {
				http.Credentials = request.Credentials;
			}

			if (Proxy != null) {
				http.Proxy = Proxy;
			}

			var headers = from p in request.Parameters
						  where p.Type == ParameterType.HttpHeader
						  select new HttpHeader {
							  Name = p.Name,
							  Value = p.Value.ToString()
						  };

			foreach (var header in headers) {
				http.Headers.Add(header);
			}

			var @params = from p in request.Parameters
						  where p.Type == ParameterType.GetOrPost
								&& p.Value != null
						  select new HttpParameter {
							  Name = p.Name,
							  Value = p.Value.ToString()
						  };

			foreach (var parameter in @params) {
				http.Parameters.Add(parameter);
			}

			foreach (var file in request.Files) {
				http.Files.Add(new HttpFile { ContentType = file.ContentType, Data = file.Data, FileName = file.FileName });
			}

			var body = (from p in request.Parameters
						where p.Type == ParameterType.RequestBody
						select p).FirstOrDefault();

			if (body != null) {
				http.RequestBody = body.Value.ToString();
				http.RequestFormat = request.RequestFormat;
			}

			var httpResponse = new HttpResponse();

			switch (request.Verb) {
				case Method.GET:
					httpResponse = http.Get();
					break;
				case Method.POST:
					httpResponse = http.Post();
					break;
				case Method.PUT:
					httpResponse = http.Put();
					break;
				case Method.DELETE:
					httpResponse = http.Delete();
					break;
				case Method.HEAD:
					httpResponse = http.Head();
					break;
				case Method.OPTIONS:
					httpResponse = http.Options();
					break;
			}

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

			return restResponse;
		}

		private Uri BuildUri(RestRequest request) {
			Uri url = null;

			switch (request.UrlMode) {
				case UrlMode.AsIs:
					url = new Uri(string.Format("{0}/{1}", BaseUrl, request.Action));
					break;
				case UrlMode.ReplaceValues:
					string assembled = request.ActionFormat;
					var urlParms = request.Parameters.Where(p => p.Type == ParameterType.UrlSegment);
					foreach (var p in urlParms) {
						assembled = assembled.Replace("{" + p.Name + "}", p.Value.ToString());
					}

					url = new Uri(string.Format("{0}/{1}", BaseUrl, assembled));
					break;
			}

			return url;
		}
	}
}
