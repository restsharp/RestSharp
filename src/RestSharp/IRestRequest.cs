#region License

//   Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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
using RestSharp.Serialization.Xml;
using RestSharp.Serializers;

namespace RestSharp
{
    public interface IRestRequest
    {
        /// <summary>
        ///     Always send a multipart/form-data request - even when no Files are present.
        /// </summary>
        bool AlwaysMultipartFormData { get; set; }

        /// <summary>
        ///     Serializer to use when writing JSON request bodies. Used if RequestFormat is Json.
        ///     By default the included JsonSerializer is used (currently using SimpleJson default serialization).
        /// </summary>
        ISerializer JsonSerializer { get; set; }

        /// <summary>
        ///     Serializer to use when writing XML request bodies. Used if RequestFormat is Xml.
        ///     By default the included XmlSerializer is used.
        /// </summary>
        IXmlSerializer XmlSerializer { get; set; }

        /// <summary>
        ///     Set this to handle the response stream yourself, based on the response details
        /// </summary>
        Action<Stream, IHttpResponse> AdvancedResponseWriter { get; set; }

        /// <summary>
        ///     Set this to write response to Stream rather than reading into memory.
        /// </summary>
        Action<Stream> ResponseWriter { get; set; }

        /// <summary>
        ///     Container of all HTTP parameters to be passed with the request.
        ///     See AddParameter() for explanation of the types of parameters that can be passed
        /// </summary>
        List<Parameter> Parameters { get; }

        /// <summary>
        ///     Container of all the files to be uploaded with the request.
        /// </summary>
        List<FileParameter> Files { get; }

        /// <summary>
        ///     Determines what HTTP method to use for this request. Supported methods: GET, POST, PUT, DELETE, HEAD, OPTIONS
        ///     Default is GET
        /// </summary>
        Method Method { get; set; }

        /// <summary>
        ///     The Resource URL to make the request against.
        ///     Tokens are substituted with UrlSegment parameters and match by name.
        ///     Should not include the scheme or domain. Do not include leading slash.
        ///     Combined with RestClient.BaseUrl to assemble final URL:
        ///     {BaseUrl}/{Resource} (BaseUrl is scheme + domain, e.g. http://example.com)
        /// </summary>
        /// <example>
        ///     // example for url token replacement
        ///     request.Resource = "Products/{ProductId}";
        ///     request.AddParameter("ProductId", 123, ParameterType.UrlSegment);
        /// </example>
        string Resource { get; set; }

        /// <summary>
        /// Serializer to use when writing request bodies.
        /// </summary>
        [Obsolete("Use AddJsonBody or AddXmlBody to tell RestSharp how to serialize the request body")]
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
        /// Used by XmlDeserializer. If not specified, XmlDeserializer will flatten response by removing namespaces from
        /// element names.
        /// </summary>
        string XmlNamespace { get; set; }

        /// <summary>
        /// In general you would not need to set this directly. Used by the NtlmAuthenticator.
        /// </summary>
        [Obsolete("Use one of authenticators provided")]
        ICredentials Credentials { get; set; }

        /// <summary>
        /// Timeout in milliseconds to be used for the request. This timeout value overrides a timeout set on the RestClient.
        /// </summary>
        int Timeout { get; set; }

        /// <summary>
        /// The number of milliseconds before the writing or reading times out. This timeout value overrides a timeout set on
        /// the RestClient.
        /// </summary>
        int ReadWriteTimeout { get; set; }

        /// <summary>
        /// How many attempts were made to send this Request?
        /// </summary>
        /// <remarks>
        /// This number is incremented each time the RestClient sends the request.
        /// </remarks>
        int Attempts { get; }

        /// <summary>
        /// Determine whether or not the "default credentials" (e.g. the user account under which the current process is
        /// running) will be sent along to the server. The default is false.
        /// </summary>
        bool UseDefaultCredentials { get; set; }

        /// <summary>
        /// List of allowed decompression methods
        /// </summary>
        IList<DecompressionMethods> AllowedDecompressionMethods { get; }

        /// <summary>
        /// When supplied, the function will be called before calling the deserializer
        /// </summary>
        Action<IRestResponse> OnBeforeDeserialization { get; set; }
        
        /// <summary>
        /// When supplied, the function will be called before making a request
        /// </summary>
        Action<IHttp> OnBeforeRequest { get; set; }
        
        /// <summary>
        ///     Serialized request body to be accessed in authenticators
        /// </summary>
        RequestBody Body { get; set; }

        /// <summary>
        /// Adds a file to the Files collection to be included with a POST or PUT request
        /// (other methods do not support file uploads).
        /// </summary>
        /// <param name="name">The parameter name to use in the request</param>
        /// <param name="path">Full path to file to upload</param>
        /// <param name="contentType">The MIME type of the file to upload</param>
        /// <returns>This request</returns>
        IRestRequest AddFile(string name, string path, string contentType = null);

        /// <summary>
        /// Adds the bytes to the Files collection with the specified file name and content type
        /// </summary>
        /// <param name="name">The parameter name to use in the request</param>
        /// <param name="bytes">The file data</param>
        /// <param name="fileName">The file name to use for the uploaded file</param>
        /// <param name="contentType">The MIME type of the file to upload</param>
        /// <returns>This request</returns>
        IRestRequest AddFile(string name, byte[] bytes, string fileName, string contentType = null);

        /// <summary>
        /// Adds the bytes to the Files collection with the specified file name and content type
        /// </summary>
        /// <param name="name">The parameter name to use in the request</param>
        /// <param name="writer">A function that writes directly to the stream.  Should NOT close the stream.</param>
        /// <param name="fileName">The file name to use for the uploaded file</param>
        /// <param name="contentLength">The length (in bytes) of the file content.</param>
        /// <param name="contentType">The MIME type of the file to upload</param>
        /// <returns>This request</returns>
        IRestRequest AddFile(string name, Action<Stream> writer, string fileName, long contentLength, string contentType = null);

        /// <summary>
        /// Add bytes to the Files collection as if it was a file of specific type
        /// </summary>
        /// <param name="name">A form parameter name</param>
        /// <param name="bytes">The file data</param>
        /// <param name="filename">The file name to use for the uploaded file</param>
        /// <param name="contentType">Specific content type. Es: application/x-gzip </param>
        /// <returns></returns>
        IRestRequest AddFileBytes(string name, byte[] bytes, string filename, string contentType = "application/x-gzip");

        /// <summary>
        /// Serializes obj to format specified by RequestFormat, but passes XmlNamespace if using the default XmlSerializer
        /// The default format is XML. Change RequestFormat if you wish to use a different serialization format.
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <param name="xmlNamespace">The XML namespace to use when serializing</param>
        /// <returns>This request</returns>
        [Obsolete("Use AddJsonBody or AddXmlBody instead")]
        IRestRequest AddBody(object obj, string xmlNamespace);

        /// <summary>
        /// Serializes obj to data format specified by RequestFormat and adds it to the request body.
        /// The default format is XML. Change RequestFormat if you wish to use a different serialization format.
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <returns>This request</returns>
        [Obsolete("Use AddJsonBody or AddXmlBody instead")]
        IRestRequest AddBody(object obj);

        /// <summary>
        /// Instructs RestSharp to send a given object in the request body, serialized as JSON.
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <returns>This request</returns>
        IRestRequest AddJsonBody(object obj);

        /// <summary>
        /// Instructs RestSharp to send a given object in the request body, serialized as JSON.
        /// Allows specifying a custom content type. Usually, this method is used to support PATCH
        /// requests that require application/json-patch+json content type.
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <param name="contentType">Custom content type to override the default application/json</param>
        /// <returns>This request</returns>
        IRestRequest AddJsonBody(object obj, string contentType);

        /// <summary>
        /// Instructs RestSharp to send a given object in the request body, serialized as XML.
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <returns>This request</returns>
        IRestRequest AddXmlBody(object obj);

        /// <summary>
        /// Instructs RestSharp to send a given object in the request body, serialized as XML
        /// but passes XmlNamespace if using the default XmlSerializer.
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <param name="xmlNamespace">The XML namespace to use when serializing</param>
        /// <returns>This request</returns>
        IRestRequest AddXmlBody(object obj, string xmlNamespace);

        /// <summary>
        /// Calls AddParameter() for all public, readable properties specified in the includedProperties list
        /// </summary>
        /// <example>
        /// request.AddObject(product, "ProductId", "Price", ...);
        /// </example>
        /// <param name="obj">The object with properties to add as parameters</param>
        /// <param name="includedProperties">The names of the properties to include</param>
        /// <returns>This request</returns>
        IRestRequest AddObject(object obj, params string[] includedProperties);

        /// <summary>
        /// Calls AddParameter() for all public, readable properties of obj
        /// </summary>
        /// <param name="obj">The object with properties to add as parameters</param>
        /// <returns>This request</returns>
        IRestRequest AddObject(object obj);

        /// <summary>
        /// Add the parameter to the request
        /// </summary>
        /// <param name="p">Parameter to add</param>
        /// <returns></returns>
        IRestRequest AddParameter(Parameter p);

        /// <summary>
        /// Adds a HTTP parameter to the request (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT)
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <returns>This request</returns>
        IRestRequest AddParameter(string name, object value);

        /// <summary>
        /// Adds a parameter to the request. There are five types of parameters:
        /// - GetOrPost: Either a QueryString value or encoded form value based on method
        /// - HttpHeader: Adds the name/value pair to the HTTP request's Headers collection
        /// - UrlSegment: Inserted into URL if there is a matching url token e.g. {AccountId}
        /// - Cookie: Adds the name/value pair to the HTTP request's Cookies collection
        /// - RequestBody: Used by AddBody() (not recommended to use directly)
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <param name="type">The type of parameter to add</param>
        /// <returns>This request</returns>
        IRestRequest AddParameter(string name, object value, ParameterType type);

        /// <summary>
        /// Adds a parameter to the request. There are five types of parameters:
        /// - GetOrPost: Either a QueryString value or encoded form value based on method
        /// - HttpHeader: Adds the name/value pair to the HTTP request's Headers collection
        /// - UrlSegment: Inserted into URL if there is a matching url token e.g. {AccountId}
        /// - Cookie: Adds the name/value pair to the HTTP request's Cookies collection
        /// - RequestBody: Used by AddBody() (not recommended to use directly)
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <param name="contentType">Content-Type of the parameter</param>
        /// <param name="type">The type of parameter to add</param>
        /// <returns>This request</returns>
        IRestRequest AddParameter(string name, object value, string contentType, ParameterType type);

        /// <summary>
        /// Adds a parameter to the request or updates it with the given argument, if the parameter already exists in the
        /// request.
        /// </summary>
        /// <param name="parameter">Parameter to add</param>
        /// <returns></returns>
        IRestRequest AddOrUpdateParameter(Parameter parameter);
        
        /// <summary>
        /// Add or update parameters to the request
        /// </summary>
        /// <param name="parameters">Collection of parameters to add</param>
        /// <returns></returns>
        IRestRequest AddOrUpdateParameters(IEnumerable<Parameter> parameters);

        /// <summary>
        /// Adds a HTTP parameter to the request (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT)
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <returns>This request</returns>
        IRestRequest AddOrUpdateParameter(string name, object value);

        /// <summary>
        /// Adds a parameter to the request. There are five types of parameters:
        /// - GetOrPost: Either a QueryString value or encoded form value based on method
        /// - HttpHeader: Adds the name/value pair to the HTTP request Headers collection
        /// - UrlSegment: Inserted into URL if there is a matching url token e.g. {AccountId}
        /// - Cookie: Adds the name/value pair to the HTTP request Cookies collection
        /// - RequestBody: Used by AddBody() (not recommended to use directly)
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <param name="type">The type of parameter to add</param>
        /// <returns>This request</returns>
        IRestRequest AddOrUpdateParameter(string name, object value, ParameterType type);

        /// <summary>
        /// Adds a parameter to the request. There are five types of parameters:
        /// - GetOrPost: Either a QueryString value or encoded form value based on method
        /// - HttpHeader: Adds the name/value pair to the HTTP request Headers collection
        /// - UrlSegment: Inserted into URL if there is a matching url token e.g. {AccountId}
        /// - Cookie: Adds the name/value pair to the HTTP request Cookies collection
        /// - RequestBody: Used by AddBody() (not recommended to use directly)
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <param name="contentType">Content-Type of the parameter</param>
        /// <param name="type">The type of parameter to add</param>
        /// <returns>This request</returns>
        IRestRequest AddOrUpdateParameter(string name, object value, string contentType, ParameterType type);

        /// <summary>
        /// Shortcut to AddParameter(name, value, HttpHeader) overload
        /// </summary>
        /// <param name="name">Name of the header to add</param>
        /// <param name="value">Value of the header to add</param>
        /// <returns></returns>
        IRestRequest AddHeader(string name, string value);
        
        /// <summary>
        /// Uses AddHeader(name, value) in a convenient way to pass
        /// in multiple headers at once.
        /// </summary>
        /// <param name="headers">Key/Value pairs containing the name: value of the headers</param>
        /// <returns>This request</returns>
        IRestRequest AddHeaders(ICollection<KeyValuePair<string, string>> headers);

        /// <summary>
        /// Shortcut to AddParameter(name, value, Cookie) overload
        /// </summary>
        /// <param name="name">Name of the cookie to add</param>
        /// <param name="value">Value of the cookie to add</param>
        /// <returns></returns>
        IRestRequest AddCookie(string name, string value);

        /// <summary>
        /// Shortcut to AddParameter(name, value, UrlSegment) overload
        /// </summary>
        /// <param name="name">Name of the segment to add</param>
        /// <param name="value">Value of the segment to add</param>
        /// <returns></returns>
        IRestRequest AddUrlSegment(string name, string value);

        /// <summary>
        /// Shortcut to AddParameter(name, value, UrlSegment) overload
        /// </summary>
        /// <param name="name">Name of the segment to add</param>
        /// <param name="value">Value of the segment to add</param>
        /// <returns></returns>
        IRestRequest AddUrlSegment(string name, object value);

        /// <summary>
        /// Shortcut to AddParameter(name, value, QueryString) overload
        /// </summary>
        /// <param name="name">Name of the parameter to add</param>
        /// <param name="value">Value of the parameter to add</param>
        /// <returns></returns>
        IRestRequest AddQueryParameter(string name, string value);

        /// <summary>
        /// Shortcut to AddParameter(name, value, QueryString) overload
        /// </summary>
        /// <param name="name">Name of the parameter to add</param>
        /// <param name="value">Value of the parameter to add</param>
        /// <param name="encode">Whether parameter should be encoded or not</param>
        /// <returns></returns>
        IRestRequest AddQueryParameter(string name, string value, bool encode);

        IRestRequest AddDecompressionMethod(DecompressionMethods decompressionMethod);

        void IncreaseNumAttempts();
    }
}