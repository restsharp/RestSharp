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

using System.Linq;
using System.Net;
using RestSharp.Deserializers;

namespace RestSharp
{
	public class RestClient : IRestClient
	{
		private readonly IDeserializer _jsonDeserializer;
		private readonly IDeserializer _xmlDeserializer;

		public RestClient()
			: this(new JsonDeserializer(), new XmlDeserializer()) {
		}

		public RestClient(IDeserializer jsonDeserializer, IDeserializer xmlDeserializer) {
			_jsonDeserializer = jsonDeserializer;
			_xmlDeserializer = xmlDeserializer;
		}

		public IAuthenticator Authenticator { get; set; }
		public IWebProxy Proxy { get; set; }

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

		public T Execute<T>(RestRequest request) where T : new() {
			AuthenticateIfNeeded(request);

			var response = GetResponse(request);

			var returnVal = default(T);

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
					returnVal = DeserializeJsonTo<T>(response.Content, request.DateFormat);
					break;
				case ResponseFormat.Xml:
					returnVal = DeserializeXmlTo<T>(response.Content, request.RootElement, request.XmlNamespace, request.DateFormat);
					break;
			}

			return returnVal;
		}

		private RestResponse GetResponse(RestRequest request) {
			IHttp http = new Http();
			http.Url = request.GetUri();

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

			var response = new RestResponse();

			switch (request.Verb) {
				case Method.GET:
					response = http.Get();
					break;
				case Method.POST:
					response = http.Post();
					break;
				case Method.PUT:
					response = http.Put();
					break;
				case Method.DELETE:
					response = http.Delete();
					break;
				case Method.HEAD:
					response = http.Head();
					break;
				case Method.OPTIONS:
					response = http.Options();
					break;
			}

			return response;
		}

		private T DeserializeJsonTo<T>(string content, string dateFormat) where T : new() {
			_jsonDeserializer.DateFormat = dateFormat;
			return _jsonDeserializer.Deserialize<T>(content);
		}

		private T DeserializeXmlTo<T>(string content, string rootElement, string xmlNamespace, string dateFormat) where T : new() {
			_xmlDeserializer.Namespace = xmlNamespace;
			_xmlDeserializer.RootElement = rootElement;
			_xmlDeserializer.DateFormat = dateFormat;
			return _xmlDeserializer.Deserialize<T>(content);
		}
	}
}
