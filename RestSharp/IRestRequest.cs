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
using System.Net;
using RestSharp.Serializers;

namespace RestSharp
{
	public interface IRestRequest
	{
		/// <summary>
		/// Serializer to use when writing JSON request bodies. Used if RequestFormat is Json.
		/// By default the included JsonSerializer is used (currently using JSON.NET default serialization).
		/// </summary>
		ISerializer JsonSerializer { get; set; }

		/// <summary>
		/// Serializer to use when writing XML request bodies. Used if RequestFormat is Xml.
		/// By default the included XmlSerializer is used.
		/// </summary>
		ISerializer XmlSerializer { get; set; }

		/// <summary>
		/// Set this to write response to Stream rather than reading into memory.
		/// </summary>
		Action<Stream> ResponseWriter { get; set; }

		/// <summary>
		/// Container of all HTTP parameters to be passed with the request. 
		/// See AddParameter() for explanation of the types of parameters that can be passed
		/// </summary>
		List<Parameter> Parameters { get; }

		/// <summary>
		/// Container of all the files to be uploaded with the request.
		/// </summary>
		List<FileParameter> Files { get; }

		/// <summary>
		/// Determines what HTTP method to use for this request. Supported methods: GET, POST, PUT, DELETE, HEAD, OPTIONS
		/// Default is GET
		/// </summary>
		Method Method { get; set; }

		/// <summary>
		/// The Resource URL to make the request against.
		/// Tokens are substituted with UrlSegment parameters and match by name.
		/// Should not include the scheme or domain. Do not include leading slash.
		/// Combined with RestClient.BaseUrl to assemble final URL:
		/// {BaseUrl}/{Resource} (BaseUrl is scheme + domain, e.g. http://example.com)
		/// </summary>
		/// <example>
		/// // example for url token replacement
		/// request.Resource = "Products/{ProductId}";
		///	request.AddParameter("ProductId", 123, ParameterType.UrlSegment);
		/// </example>
		string Resource { get; set; }

		/// <summary>
		/// Serializer to use when writing XML request bodies. Used if RequestFormat is Xml.
		/// By default XmlSerializer is used.
		/// </summary>
		DataFormat RequestFormat { get; set; }

		/// <summary>
		/// Used by the default deserializers to determine where to start deserializing from.
		/// Can be used to skip container or root elements that do not have corresponding deserialzation targets.
		/// </summary>
		string RootElement { get; set; }

		/// <summary>
		/// Used by the default deserializers to explicitly set which date format string to use when parsing dates.
		/// </summary>
		string DateFormat { get; set; }

		/// <summary>
		/// Used by XmlDeserializer. If not specified, XmlDeserializer will flatten response by removing namespaces from element names.
		/// </summary>
		string XmlNamespace { get; set; }

		/// <summary>
		/// In general you would not need to set this directly. Used by the NtlmAuthenticator. 
		/// </summary>
		ICredentials Credentials { get; set; }

		/// <summary>
		/// Timeout in milliseconds to be used for the request. This timeout value overrides a timeout set on the RestClient.
		/// </summary>
		int Timeout { get; set; }

		/// <summary>
		/// How many attempts were made to send this Request?
		/// </summary>
		/// <remarks>
		/// This Number is incremented each time the RestClient sends the request.
		/// Useful when using Asynchronous Execution with Callbacks
		/// </remarks>
		int Attempts { get; }

#if FRAMEWORK
		/// <summary>
		/// Adds a file to the Files collection to be included with a POST or PUT request 
		/// (other methods do not support file uploads).
		/// </summary>
		/// <param name="name">The parameter name to use in the request</param>
		/// <param name="path">Full path to file to upload</param>
		/// <returns>This request</returns>
		IRestRequest AddFile (string name, string path);

		/// <summary>
		/// Adds the bytes to the Files collection with the specified file name
		/// </summary>
		/// <param name="name">The parameter name to use in the request</param>
		/// <param name="bytes">The file data</param>
		/// <param name="fileName">The file name to use for the uploaded file</param>
		/// <returns>This request</returns>
		IRestRequest AddFile (string name, byte[] bytes, string fileName);

		/// <summary>
		/// Adds the bytes to the Files collection with the specified file name and content type
		/// </summary>
		/// <param name="name">The parameter name to use in the request</param>
		/// <param name="bytes">The file data</param>
		/// <param name="fileName">The file name to use for the uploaded file</param>
		/// <param name="contentType">The MIME type of the file to upload</param>
		/// <returns>This request</returns>
		IRestRequest AddFile (string name, byte[] bytes, string fileName, string contentType);
#endif

		/// <summary>
		/// Serializes obj to format specified by RequestFormat, but passes xmlNamespace if using the default XmlSerializer
		/// </summary>
		/// <param name="obj">The object to serialize</param>
		/// <param name="xmlNamespace">The XML namespace to use when serializing</param>
		/// <returns>This request</returns>
		IRestRequest AddBody (object obj, string xmlNamespace);

		/// <summary>
		/// Serializes obj to data format specified by RequestFormat and adds it to the request body.
		/// </summary>
		/// <param name="obj">The object to serialize</param>
		/// <returns>This request</returns>
		IRestRequest AddBody (object obj);

		/// <summary>
		/// Calls AddParameter() for all public, readable properties specified in the white list
		/// </summary>
		/// <example>
		/// request.AddObject(product, "ProductId", "Price", ...);
		/// </example>
		/// <param name="obj">The object with properties to add as parameters</param>
		/// <param name="whitelist">The names of the properties to include</param>
		/// <returns>This request</returns>
		IRestRequest AddObject (object obj, params string[] whitelist);

		/// <summary>
		/// Calls AddParameter() for all public, readable properties of obj
		/// </summary>
		/// <param name="obj">The object with properties to add as parameters</param>
		/// <returns>This request</returns>
		IRestRequest AddObject (object obj);

		/// <summary>
		/// Add the parameter to the request
		/// </summary>
		/// <param name="p">Parameter to add</param>
		/// <returns></returns>
		IRestRequest AddParameter (Parameter p);

		/// <summary>
		/// Adds a HTTP parameter to the request (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT)
		/// </summary>
		/// <param name="name">Name of the parameter</param>
		/// <param name="value">Value of the parameter</param>
		/// <returns>This request</returns>
		IRestRequest AddParameter (string name, object value);

		/// <summary>
		/// Adds a parameter to the request. There are five types of parameters:
		///	- GetOrPost: Either a QueryString value or encoded form value based on method
		///	- HttpHeader: Adds the name/value pair to the HTTP request's Headers collection
		///	- UrlSegment: Inserted into URL if there is a matching url token e.g. {AccountId}
		///	- Cookie: Adds the name/value pair to the HTTP request's Cookies collection
		///	- RequestBody: Used by AddBody() (not recommended to use directly)
		/// </summary>
		/// <param name="name">Name of the parameter</param>
		/// <param name="value">Value of the parameter</param>
		/// <param name="type">The type of parameter to add</param>
		/// <returns>This request</returns>
		IRestRequest AddParameter (string name, object value, ParameterType type);

		/// <summary>
		/// Shortcut to AddParameter(name, value, HttpHeader) overload
		/// </summary>
		/// <param name="name">Name of the header to add</param>
		/// <param name="value">Value of the header to add</param>
		/// <returns></returns>
		IRestRequest AddHeader (string name, string value);

		/// <summary>
		/// Shortcut to AddParameter(name, value, Cookie) overload
		/// </summary>
		/// <param name="name">Name of the cookie to add</param>
		/// <param name="value">Value of the cookie to add</param>
		/// <returns></returns>
		IRestRequest AddCookie (string name, string value);

		/// <summary>
		/// Shortcut to AddParameter(name, value, UrlSegment) overload
		/// </summary>
		/// <param name="name">Name of the segment to add</param>
		/// <param name="value">Value of the segment to add</param>
		/// <returns></returns>
		IRestRequest AddUrlSegment(string name, string value);

		Action<IRestResponse> OnBeforeDeserialization { get; set; }
		void IncreaseNumAttempts();
	}
}