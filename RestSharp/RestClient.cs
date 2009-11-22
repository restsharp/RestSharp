#region License
//   Copyright 2009 John Sheehan
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
using System.Linq;
using RestSharp.Deserializers;

namespace RestSharp
{
	public class RestClient : IRestClient
	{
		private readonly IHttp _http;
		private readonly IDeserializer _jsonDeserializer;
		private readonly IDeserializer _xmlDeserializer;

		public RestClient() 
			: this(new Http(), new JsonDeserializer(), new XmlDeserializer()) {
		}

		public RestClient(IHttp http, IDeserializer jsonDeserializer, IDeserializer xmlDeserializer) {
			_http = http;
			_jsonDeserializer = jsonDeserializer;
			_xmlDeserializer = xmlDeserializer;
		}

		public IAuthenticator Authenticator { get; set; }

		public RestResponse Execute(RestRequest request) {
			Authenticator.Authenticate(request);
			var response = GetResponse(request);
			return response;
		}

		public X Execute<X>(RestRequest request) where X : new() {
			Authenticator.Authenticate(request);

			// make request
			var response = GetResponse(request);

			// handle response
			X returnVal = default(X);

			if (request.ResponseFormat == ResponseFormat.Auto) {
				switch (request.ContentType) {
					case "application/json":
						request.ResponseFormat = ResponseFormat.Json;
						break;
					case "text/xml":
						request.ResponseFormat = ResponseFormat.Xml;
						break;
				}
			}

			switch (request.ResponseFormat) {
				case ResponseFormat.Json:
					returnVal = DeserializeJsonTo<X>(response.Content);
					break;
				case ResponseFormat.Xml:
					returnVal = DeserializeXmlTo<X>(response.Content, request.RootElement, request.XmlNamespace);
					break;
			}

			return returnVal;
		}

		private RestResponse GetResponse(RestRequest request) {
			if (request.Credentials != null) {
				_http.Credentials = request.Credentials;
			}

			var headers = from p in request.Parameters
						  where p.Type == ParameterType.HttpHeader
						  select new {
							  Name = p.Name,
							  Value = p.Value.ToString()
						  };

			foreach (var header in headers) {
				_http.Headers.Add(header.Name, header.Value);
			}

			var @params = request.Parameters
									.Where(p => p.Type == ParameterType.GetOrPost)
									.ToDictionary(k => k.Name, e => e.Value.ToString());

			var response = new RestResponse();

			try {
				switch (request.Verb) {
					case Method.GET:
						response = _http.Get(request.GetUri(), @params);
						break;
					case Method.POST:
						response = _http.Post(request.GetUri(), @params);
						break;
					case Method.PUT:
						response = _http.Put(request.GetUri(), @params);
						break;
					case Method.DELETE:
						response = _http.Delete(request.GetUri(), @params);
						break;
					case Method.HEAD:
						response = _http.Head(request.GetUri(), @params);
						break;
					case Method.OPTIONS:
						response = _http.Options(request.GetUri(), @params);
						break;
				}

				response.ResponseStatus = ResponseStatus.Success;
			}
			catch (Exception ex) {
				response = new RestResponse { 
								ErrorMessage = ex.Message,
								ResponseStatus = ResponseStatus.Error
						   };
			}

			return response;
		}

		private X DeserializeJsonTo<X>(string content) where X : new() {
			var deserializer = new JsonDeserializer();
			return deserializer.Deserialize<X>(content);
		}

		private X DeserializeXmlTo<X>(string content, string rootElement, string xmlNamespace) where X : new() {
			_xmlDeserializer.Namespace = xmlNamespace;
			_xmlDeserializer.RootElement = rootElement;
			return _xmlDeserializer.Deserialize<X>(content);
		}
	}
}
