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
using System.IO;
using RestSharp.Serializers;

namespace RestSharp
{
	/// <summary>
	/// Container for data used to make requests
	/// </summary>
	public class RestRequest
	{
		/// <summary>
		/// Serializer to use when writing JSON request bodies. Used if RequestFormat is Json.
		/// By default the included JsonSerializer is used (currently using JSON.NET default serialization).
		/// </summary>
		public ISerializer JsonSerializer { get; set; }

		/// <summary>
		/// Serializer to use when writing XML request bodies. Used if RequestFormat is Xml.
		/// By default the included XmlSerializer is used.
		/// </summary>
		public ISerializer XmlSerializer { get; set; }

		public RestRequest() {
			Parameters = new List<Parameter>();
			Files = new List<FileParameter>();
			XmlSerializer = new XmlSerializer();
			JsonSerializer = new JsonSerializer();
		}

		/// <summary>
		/// Sets Verb property to value of verb
		/// </summary>
		/// <param name="verb">Verb to use for this request</param>
		public RestRequest(Method verb)
			: this() {
			Verb = verb;
		}

		/// <summary>
		/// Sets Action property
		/// </summary>
		/// <param name="action">Action to use for this request</param>
		public RestRequest(string action)
			: this() {
			Action = action;
		}

		/// <summary>
		/// Sets Action and Verb properties
		/// </summary>
		/// <param name="action">Action to use for this request</param>
		/// <param name="verb">Verb to use for this request</param>
		public RestRequest(string action, Method verb)
			: this() {
			Action = action;
			Verb = verb;
		}

		/// <summary>
		/// Adds a file to the Files collection to be included with a POST or PUT request 
		/// (other verbs do not support file uploads).
		/// </summary>
		/// <param name="path">Full path to file to upload</param>
		/// <returns>This request</returns>
		public RestRequest AddFile(string path) {
			var fileName = Path.GetFileName(path);
			var file = File.ReadAllBytes(path);

			return AddFile(file, fileName);
		}

		/// <summary>
		/// Adds the bytes to the Files collection with the specified file name
		/// </summary>
		/// <param name="bytes">The file data</param>
		/// <param name="fileName">The file name to use for the uploaded file</param>
		/// <returns>This request</returns>
		public RestRequest AddFile(byte[] bytes, string fileName) {
			return AddFile(bytes, fileName, null);
		}

		/// <summary>
		/// Adds the bytes to the Files collection with the specified file name and content type
		/// </summary>
		/// <param name="bytes">The file data</param>
		/// <param name="fileName">The file name to use for the uploaded file</param>
		/// <param name="contentType">The MIME type of the file to upload</param>
		/// <returns>This request</returns>
		public RestRequest AddFile(byte[] bytes, string fileName, string contentType) {
			Files.Add(new FileParameter { Data = bytes, FileName = fileName, ContentType = contentType });
			return this;
		}

		/// <summary>
		/// Serializes obj to format specified by RequestFormat, but passes xmlNamespace if using the default XmlSerializer
		/// </summary>
		/// <param name="obj">The object to serialize</param>
		/// <param name="xmlNamespace">The XML namespace to use when serializing</param>
		/// <returns>This request</returns>
		public RestRequest AddBody(object obj, string xmlNamespace) {
			string serialized;

			switch (RequestFormat) {
				case RequestFormat.Json:
					serialized = JsonSerializer.Serialize(obj);
					break;

				case RequestFormat.Xml:
					XmlSerializer.Namespace = xmlNamespace;
					serialized = XmlSerializer.Serialize(obj);
					break;

				default:
					serialized = "";
					break;
			}

			return AddParameter("", serialized, ParameterType.RequestBody);
		}

		/// <summary>
		/// Serializes obj to format specified by RequestFormat and adds it to the request body.
		/// </summary>
		/// <param name="obj">The object to serialize</param>
		/// <returns>This request</returns>
		public RestRequest AddBody(object obj) {
			return AddBody(obj, "");
		}

		/// <summary>
		/// Calls AddParameter() for all public, readable properties specified in the white list
		/// </summary>
		/// <example>
		/// request.AddObject(product, "ProductId", "Price", ...);
		/// </example>
		/// <param name="obj">The object with properties to add as parameters</param>
		/// <param name="whitelist">The names of the properties to include</param>
		/// <returns>This request</returns>
		public RestRequest AddObject(object obj, params string[] whitelist) {
			// automatically create parameters from object props
			var type = obj.GetType();
			var props = type.GetProperties();

			foreach (var prop in props) {
				bool isAllowed = whitelist.Length == 0 || (whitelist.Length > 0 && whitelist.Contains(prop.Name));

				if (isAllowed) {
					var propType = prop.PropertyType;
					var val = prop.GetValue(obj, null);

					if (val != null) {
						if (propType.IsArray) {
							val = string.Join(",", (string[])val);
						}

						AddParameter(prop.Name, val);
					}
				}
			}

			return this;
		}

		/// <summary>
		/// Calls AddParameter() for all public, readable properties of obj
		/// </summary>
		/// <param name="obj">The object with properties to add as parameters</param>
		/// <returns>This request</returns>
		public RestRequest AddObject(object obj) {
			AddObject(obj, new string[] { });
			return this;
		}

		/// <summary>
		/// Add the parameter to the request
		/// </summary>
		/// <param name="p">Parameter to add</param>
		/// <returns></returns>
		public RestRequest AddParameter(Parameter p) {
			Parameters.Add(p);
			return this;
		}

		/// <summary>
		/// Adds a HTTP parameter to the request (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT)
		/// </summary>
		/// <param name="name">Name of the parameter</param>
		/// <param name="value">Value of the parameter</param>
		/// <returns>This request</returns>
		public RestRequest AddParameter(string name, object value) {
			return AddParameter(new Parameter { Name = name, Value = value, Type = ParameterType.GetOrPost });
		}

		/// <summary>
		/// Adds a parameter to the request. There are four types of parameters:
		///	- GetOrPost: Either a QueryString value or encoded form value based on verb
		///	- HttpHeader: Adds the name/value pair to the HTTP request's Headers collection
		///	- UrlSegment: Inserted into URL if ActionFormat is specified
		///	- RequestBody: Used by AddBody() (not recommended to use directly)
		/// </summary>
		/// <param name="name">Name of the parameter</param>
		/// <param name="value">Value of the parameter</param>
		/// <param name="type">The type of parameter to add</param>
		/// <returns>This request</returns>
		public RestRequest AddParameter(string name, object value, ParameterType type) {
			return AddParameter(new Parameter { Name = name, Value = value, Type = type });
		}

		/// <summary>
		/// Container of all HTTP parameters to be passed with the request. 
		/// See AddParameter() for explanation of the types of parameters that can be passed
		/// </summary>
		public List<Parameter> Parameters { get; private set; }

		/// <summary>
		/// Container of all the files to be uploaded with the request.
		/// </summary>
		public List<FileParameter> Files { get; private set; }

		private Method _verb = Method.GET;
		/// <summary>
		/// Determines what HTTP verb to use for this request. Supported verbs: GET, POST, PUT, DELETE, HEAD, OPTIONS
		/// Default is GET
		/// </summary>
		public Method Verb {
			get { return _verb; }
			set { _verb = value; }
		}

		/// <summary>
		/// The URL to make the request against. Ignored if ActionFormat is set.
		/// Should not include the scheme or domain. Do not include leading slash.
		/// Combined with RestClient.BaseUrl to assemble final URL:
		/// {BaseUrl}/{Action} (BaseUrl is scheme + domain, e.g. http://example.com)
		/// </summary>
		public string Action { get; set; }

		private string _actionFormat;
		/// <summary>
		/// URL format to use for requests that require parameters to be set as part of the URL.
		/// Values are substituted with UrlSegment parameters and match by name.
		/// Combined with RestClient.BaseUrl to assemble final URL:
		/// {BaseUrl}/{Action} (BaseUrl is scheme + domain, e.g. http://example.com)
		/// </summary>
		/// <example>
		/// request.ActionFormat = "Products/{ProductId}";
		///	request.AddParameter("ProductId", 123, ParameterType.UrlSegment);
		/// </example>
		public string ActionFormat {
			get {
				return _actionFormat;
			}
			set {
				_urlMode = UrlMode.ReplaceValues;
				_actionFormat = value;
			}
		}

		private UrlMode _urlMode = UrlMode.AsIs;
		/// <summary>
		/// When an ActionFormat is set UrlMode is set to UrlMode.ReplaceValues so that URL segment parameters are processed.
		/// </summary>
		public UrlMode UrlMode {
			get {
				return _urlMode;
			}
		}

		private RequestFormat _requestFormat = RequestFormat.Xml;
		/// <summary>
		/// Serializer to use when writing XML request bodies. Used if RequestFormat is Xml.
		/// By default XmlSerializer is used.
		/// </summary>
		public RequestFormat RequestFormat {
			get {
				return _requestFormat;
			}
			set {
				_requestFormat = value;
			}
		}

		/// <summary>
		/// Used by the default deserializers to determine where to start deserializing from.
		/// Can be used to skip container or root elements that do not have corresponding deserialzation targets.
		/// </summary>
		public string RootElement { get; set; }

		/// <summary>
		/// In general you would not need to set this directly. Used by the NtlmAuthenticator. 
		/// </summary>
		public ICredentials Credentials { get; set; }
	}
}
