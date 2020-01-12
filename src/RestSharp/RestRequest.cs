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
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using RestSharp.Extensions;
using RestSharp.Serialization.Xml;
using RestSharp.Serializers;

// ReSharper disable IntroduceOptionalParameters.Global

namespace RestSharp
{
    /// <summary>
    ///     Container for data used to make requests
    /// </summary>
    public class RestRequest : IRestRequest
    {
        static readonly Regex PortSplitRegex = new Regex(@":\d+");

        readonly IList<DecompressionMethods> _allowedDecompressionMethods;

        Action<Stream, IHttpResponse> _advancedResponseWriter;

        Action<Stream> _responseWriter;

        /// <summary>
        ///     Default constructor
        /// </summary>
        public RestRequest()
        {
            RequestFormat                = DataFormat.Xml;
            Method                       = Method.GET;
            Parameters                   = new List<Parameter>();
            Files                        = new List<FileParameter>();
            _allowedDecompressionMethods = new List<DecompressionMethods>();

            OnBeforeDeserialization = r => { };
            OnBeforeRequest = h => { };
	   }

        /// <summary>
        ///     Sets Method property to value of method
        /// </summary>
        /// <param name="method">Method to use for this request</param>
        public RestRequest(Method method) : this() => Method = method;

        public RestRequest(string resource, Method method) : this(resource, method, DataFormat.Xml) { }

        public RestRequest(string resource, DataFormat dataFormat) : this(resource, Method.GET, dataFormat) { }

        public RestRequest(string resource) : this(resource, Method.GET, DataFormat.Xml) { }

        public RestRequest(string resource, Method method, DataFormat dataFormat) : this()
        {
            Resource      = resource ?? "";
            Method        = method;
            RequestFormat = dataFormat;

            var queryStringStart = Resource.IndexOf('?');

            if (queryStringStart >= 0 && Resource.IndexOf('=') > queryStringStart)
            {
                var queryParams = ParseQuery(Resource.Substring(queryStringStart + 1));
                Resource = Resource.Substring(0, queryStringStart);

                foreach (var param in queryParams)
                    AddQueryParameter(param.Name, param.Value, false);
            }

            static IEnumerable<NameValuePair> ParseQuery(string query)
                => query.Split('&')
                    .Select(
                        x =>
                        {
                            var position = x.IndexOf('=');

                            return position > 0
                                ? new NameValuePair(x.Substring(0, position), x.Substring(position + 1))
                                : new NameValuePair(x, string.Empty);
                        }
                    );
        }

        public RestRequest(Uri resource, Method method, DataFormat dataFormat)
            : this(
                resource.IsAbsoluteUri
                    ? resource.AbsoluteUri
                    : resource.OriginalString, method, dataFormat
            ) { }

        public RestRequest(Uri resource, Method method) : this(resource, method, DataFormat.Xml) { }

        public RestRequest(Uri resource) : this(resource, Method.GET, DataFormat.Xml) { }

        /// <summary>
        ///     Gets or sets a user-defined state object that contains information about a request and which can be later
        ///     retrieved when the request completes.
        /// </summary>
        public object UserState { get; set; }

        /// <summary>
        ///     List of Allowed Decompresison Methods
        /// </summary>
        public IList<DecompressionMethods> AllowedDecompressionMethods => _allowedDecompressionMethods.Any()
            ? _allowedDecompressionMethods
            : new[] {DecompressionMethods.None, DecompressionMethods.Deflate, DecompressionMethods.GZip};

        /// <summary>
        ///     Always send a multipart/form-data request - even when no Files are present.
        /// </summary>
        public bool AlwaysMultipartFormData { get; set; }

        /// <summary>
        ///     Serializer to use when writing JSON request bodies. Used if RequestFormat is Json.
        ///     By default the included JsonSerializer is used (currently using JSON.NET default serialization).
        /// </summary>
        public ISerializer JsonSerializer { get; set; }

        /// <summary>
        ///     Serializer to use when writing XML request bodies. Used if RequestFormat is Xml.
        ///     By default the included XmlSerializer is used.
        /// </summary>
        public IXmlSerializer XmlSerializer { get; set; }
        
        /// <summary>
        ///     Serialized request body to be accessed in authenticators
        /// </summary>
        public RequestBody Body { get; set; }

        /// <summary>
        ///     Set this to write response to Stream rather than reading into memory.
        /// </summary>
        public Action<Stream> ResponseWriter
        {
            get => _responseWriter;
            set
            {
                if (AdvancedResponseWriter != null)
                    throw new ArgumentException(
                        "AdvancedResponseWriter is not null. Only one response writer can be used."
                    );

                _responseWriter = value;
            }
        }

        /// <summary>
        ///     Set this to handle the response stream yourself, based on the response details
        /// </summary>
        public Action<Stream, IHttpResponse> AdvancedResponseWriter
        {
            get => _advancedResponseWriter;
            set
            {
                if (ResponseWriter != null)
                    throw new ArgumentException("ResponseWriter is not null. Only one response writer can be used.");

                _advancedResponseWriter = value;
            }
        }

        /// <summary>
        ///     Determine whether or not the "default credentials" (e.g. the user account under which the current process is
        ///     running)
        ///     will be sent along to the server. The default is false.
        /// </summary>
        public bool UseDefaultCredentials { get; set; }

        /// <summary>
        ///     Adds a file to the Files collection to be included with a POST or PUT request
        ///     (other methods do not support file uploads).
        /// </summary>
        /// <param name="name">The parameter name to use in the request</param>
        /// <param name="path">Full path to file to upload</param>
        /// <param name="contentType">The MIME type of the file to upload</param>
        /// <returns>This request</returns>
        public IRestRequest AddFile(string name, string path, string contentType = null)
        {
            var f          = new FileInfo(path);
            var fileLength = f.Length;

            return AddFile(
                new FileParameter
                {
                    Name          = name,
                    FileName      = Path.GetFileName(path),
                    ContentLength = fileLength,
                    Writer = s =>
                    {
                        using var file = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read));
                        file.BaseStream.CopyTo(s);
                    },
                    ContentType = contentType
                }
            );
        }

        /// <summary>
        ///     Adds the bytes to the Files collection with the specified file name
        /// </summary>
        /// <param name="name">The parameter name to use in the request</param>
        /// <param name="bytes">The file data</param>
        /// <param name="fileName">The file name to use for the uploaded file</param>
        /// <param name="contentType">The MIME type of the file to upload</param>
        /// <returns>This request</returns>
        public IRestRequest AddFile(string name, byte[] bytes, string fileName, string contentType = null)
            => AddFile(FileParameter.Create(name, bytes, fileName, contentType));

        /// <summary>
        ///     Adds the bytes to the Files collection with the specified file name and content type
        /// </summary>
        /// <param name="name">The parameter name to use in the request</param>
        /// <param name="writer">A function that writes directly to the stream.  Should NOT close the stream.</param>
        /// <param name="fileName">The file name to use for the uploaded file</param>
        /// <param name="contentLength">The length (in bytes) of the file content.</param>
        /// <param name="contentType">The MIME type of the file to upload</param>
        /// <returns>This request</returns>
        public IRestRequest AddFile(
            string name,
            Action<Stream> writer,
            string fileName,
            long contentLength,
            string contentType = null
        )
            => AddFile(
                new FileParameter
                {
                    Name          = name,
                    Writer        = writer,
                    FileName      = fileName,
                    ContentLength = contentLength,
                    ContentType   = contentType
                }
            );

        /// <summary>
        ///     Add bytes to the Files collection as if it was a file of specific type
        /// </summary>
        /// <param name="name">A form parameter name</param>
        /// <param name="bytes">The file data</param>
        /// <param name="filename">The file name to use for the uploaded file</param>
        /// <param name="contentType">Specific content type. Es: application/x-gzip </param>
        /// <returns></returns>
        public IRestRequest AddFileBytes(
            string name,
            byte[] bytes,
            string filename,
            string contentType = "application/x-gzip"
        )
        {
            long length = bytes.Length;

            return AddFile(
                new FileParameter
                {
                    Name          = name,
                    FileName      = filename,
                    ContentLength = length,
                    ContentType   = contentType,
                    Writer = s =>
                    {
                        using var file = new StreamReader(new MemoryStream(bytes));

                        file.BaseStream.CopyTo(s);
                    }
                }
            );
        }

        /// <summary>
        ///     Serializes obj to format specified by RequestFormat, but passes xmlNamespace if using the default XmlSerializer
        ///     The default format is XML. Change RequestFormat if you wish to use a different serialization format.
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <param name="xmlNamespace">The XML namespace to use when serializing</param>
        /// <returns>This request</returns>
        [Obsolete("Use AddXmlBody")]
        public IRestRequest AddBody(object obj, string xmlNamespace)
            => RequestFormat switch
            {
                DataFormat.Json => AddJsonBody(obj),
                DataFormat.Xml  => AddXmlBody(obj, xmlNamespace),
                _               => this
            };

        /// <summary>
        ///     Serializes obj to data format specified by RequestFormat and adds it to the request body.
        ///     The default format is XML. Change RequestFormat if you wish to use a different serialization format.
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <returns>This request</returns>
        [Obsolete("Use AddXmlBody or AddJsonBody")]
        public IRestRequest AddBody(object obj)
            => RequestFormat switch
            {
                DataFormat.Json => AddJsonBody(obj),
                DataFormat.Xml  => AddXmlBody(obj),
                _               => this
            };

        /// <summary>
        ///     Serializes obj to JSON format and adds it to the request body.
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <returns>This request</returns>
        public IRestRequest AddJsonBody(object obj)
        {
            RequestFormat = DataFormat.Json;

            return AddParameter(new JsonParameter("", obj));
        }

        /// <summary>
        ///     Serializes obj to XML format and adds it to the request body.
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <returns>This request</returns>
        public IRestRequest AddXmlBody(object obj) => AddXmlBody(obj, "");

        /// <summary>
        ///     Serializes obj to XML format and passes xmlNamespace then adds it to the request body.
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <param name="xmlNamespace">The XML namespace to use when serializing</param>
        /// <returns>This request</returns>
        public IRestRequest AddXmlBody(object obj, string xmlNamespace)
        {
            RequestFormat = DataFormat.Xml;

            if (!string.IsNullOrWhiteSpace(XmlNamespace))
                xmlNamespace = XmlNamespace;
            else if (!string.IsNullOrWhiteSpace(XmlSerializer?.Namespace))
                xmlNamespace = XmlSerializer.Namespace;

            AddParameter(new XmlParameter("", obj, xmlNamespace));

            return this;
        }

        /// <summary>
        ///     Calls AddParameter() for all public, readable properties specified in the includedProperties list
        /// </summary>
        /// <example>
        ///     request.AddObject(product, "ProductId", "Price", ...);
        /// </example>
        /// <param name="obj">The object with properties to add as parameters</param>
        /// <param name="includedProperties">The names of the properties to include</param>
        /// <returns>This request</returns>
        public IRestRequest AddObject(object obj, params string[] includedProperties)
        {
            // automatically create parameters from object props
            var type  = obj.GetType();
            var props = type.GetProperties();

            foreach (var prop in props)
            {
                if (!IsAllowedProperty(prop.Name))
                    continue;

                var val = prop.GetValue(obj, null);

                if (val == null)
                    continue;

                var propType = prop.PropertyType;

                if (propType.IsArray)
                {
                    var elementType = propType.GetElementType();
                    var array       = (Array) val;

                    if (array.Length > 0 && elementType != null)
                    {
                        // convert the array to an array of strings
                        var values = array.Cast<object>().Select(item => item.ToString());

                        val = string.Join(",", values);
                    }
                }

                AddParameter(prop.Name, val);
            }

            return this;

            bool IsAllowedProperty(string propertyName)
                => includedProperties.Length == 0
                    || includedProperties.Length > 0
                    && includedProperties.Contains(propertyName);
        }

        /// <summary>
        ///     Calls AddParameter() for all public, readable properties of obj
        /// </summary>
        /// <param name="obj">The object with properties to add as parameters</param>
        /// <returns>This request</returns>
        public IRestRequest AddObject(object obj) => this.With(x => x.AddObject(obj, new string[] { }));

        /// <summary>
        ///     Add the parameter to the request
        /// </summary>
        /// <param name="p">Parameter to add</param>
        /// <returns></returns>
        public IRestRequest AddParameter(Parameter p) => this.With(x => x.Parameters.Add(p));

        /// <summary>
        ///     Adds a HTTP parameter to the request (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT)
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <returns>This request</returns>
        public IRestRequest AddParameter(string name, object value) => AddParameter(new Parameter(name, value, ParameterType.GetOrPost));

        /// <summary>
        ///     Adds a parameter to the request. There are four types of parameters:
        ///     - GetOrPost: Either a QueryString value or encoded form value based on method
        ///     - HttpHeader: Adds the name/value pair to the HTTP request's Headers collection
        ///     - UrlSegment: Inserted into URL if there is a matching url token e.g. {AccountId}
        ///     - RequestBody: Used by AddBody() (not recommended to use directly)
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <param name="type">The type of parameter to add</param>
        /// <returns>This request</returns>
        public IRestRequest AddParameter(string name, object value, ParameterType type) => AddParameter(new Parameter(name, value, type));

        /// <summary>
        ///     Adds a parameter to the request. There are four types of parameters:
        ///     - GetOrPost: Either a QueryString value or encoded form value based on method
        ///     - HttpHeader: Adds the name/value pair to the HTTP request's Headers collection
        ///     - UrlSegment: Inserted into URL if there is a matching url token e.g. {AccountId}
        ///     - RequestBody: Used by AddBody() (not recommended to use directly)
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <param name="contentType">Content-Type of the parameter</param>
        /// <param name="type">The type of parameter to add</param>
        /// <returns>This request</returns>
        public IRestRequest AddParameter(string name, object value, string contentType, ParameterType type)
            => AddParameter(new Parameter(name, value, contentType, type));

        /// <summary>
        ///     Adds a parameter to the request or updates it with the given argument, if the parameter already exists in the
        ///     request
        /// </summary>
        /// <param name="parameter">Parameter to add</param>
        /// <returns></returns>
        public IRestRequest AddOrUpdateParameter(Parameter parameter)
        {
            var p = Parameters
                .FirstOrDefault(x => x.Name == parameter.Name && x.Type == parameter.Type);

            if (p != null) Parameters.Remove(p);

            Parameters.Add(parameter);
            return this;
        }

        /// <summary>
        ///      Add or update parameters to the request
        /// </summary>
        /// <param name="parameters">Collection of parameters to add</param>
        /// <returns></returns>
        public IRestRequest AddOrUpdateParameters(IEnumerable<Parameter> parameters)
        {
            foreach (var parameter in parameters)
                AddOrUpdateParameter(parameter);

            return this;
        }

        /// <summary>
        ///     Adds a HTTP parameter to the request or updates it with the given argument, if the parameter already exists in the
        ///     request
        ///     (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT)
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <returns>This request</returns>
        public IRestRequest AddOrUpdateParameter(string name, object value)
            => AddOrUpdateParameter(new Parameter(name, value, ParameterType.GetOrPost));

        /// <inheritdoc />
        /// <summary>
        ///     Adds a HTTP parameter to the request or updates it with the given argument, if the parameter already exists in the
        ///     request
        ///     - GetOrPost: Either a QueryString value or encoded form value based on method
        ///     - HttpHeader: Adds the name/value pair to the HTTP request's Headers collection
        ///     - UrlSegment: Inserted into URL if there is a matching url token e.g. {AccountId}
        ///     - RequestBody: Used by AddBody() (not recommended to use directly)
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <param name="type">The type of parameter to add</param>
        /// <returns>This request</returns>
        public IRestRequest AddOrUpdateParameter(string name, object value, ParameterType type)
            => AddOrUpdateParameter(new Parameter(name, value, type));

        /// <summary>
        ///     Adds a HTTP parameter to the request or updates it with the given argument, if the parameter already exists in the
        ///     request
        ///     - GetOrPost: Either a QueryString value or encoded form value based on method
        ///     - HttpHeader: Adds the name/value pair to the HTTP request's Headers collection
        ///     - UrlSegment: Inserted into URL if there is a matching url token e.g. {AccountId}
        ///     - RequestBody: Used by AddBody() (not recommended to use directly)
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <param name="contentType">Content-Type of the parameter</param>
        /// <param name="type">The type of parameter to add</param>
        /// <returns>This request</returns>
        public IRestRequest AddOrUpdateParameter(string name, object value, string contentType, ParameterType type)
            => AddOrUpdateParameter(new Parameter(name, value, contentType, type));

        /// <inheritdoc />
        /// <summary>
        ///     Shortcut to AddParameter(name, value, HttpHeader) overload
        /// </summary>
        /// <param name="name">Name of the header to add</param>
        /// <param name="value">Value of the header to add</param>
        /// <returns></returns>
        public IRestRequest AddHeader(string name, string value)
        {
            static bool InvalidHost(string host) => Uri.CheckHostName(PortSplitRegex.Split(host)[0]) == UriHostNameType.Unknown;

            if (name == "Host" && InvalidHost(value))
                throw new ArgumentException("The specified value is not a valid Host header string.", nameof(value));

            return AddParameter(name, value, ParameterType.HttpHeader);
        }
        
        /// <summary>
        /// Uses AddHeader(name, value) in a convenient way to pass
        /// in multiple headers at once.
        /// </summary>
        /// <param name="headers">Key/Value pairs containing the name: value of the headers</param>
        /// <returns>This request</returns>
        public IRestRequest AddHeaders(ICollection<KeyValuePair<string, string>> headers)
        {
            var duplicateKeys = headers
                .GroupBy(pair => pair.Key.ToUpperInvariant())
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);

            if (duplicateKeys.Count() > 0)
                throw new ArgumentException($"Duplicate header names exist: {string.Join(", ", duplicateKeys)}");

            foreach (var pair in headers)
            {
                AddHeader(pair.Key, pair.Value);
            }

            return this;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Shortcut to AddParameter(name, value, Cookie) overload
        /// </summary>
        /// <param name="name">Name of the cookie to add</param>
        /// <param name="value">Value of the cookie to add</param>
        /// <returns></returns>
        public IRestRequest AddCookie(string name, string value) => AddParameter(name, value, ParameterType.Cookie);

        /// <summary>
        ///     Shortcut to AddParameter(name, value, UrlSegment) overload
        /// </summary>
        /// <param name="name">Name of the segment to add</param>
        /// <param name="value">Value of the segment to add</param>
        /// <returns></returns>
        public IRestRequest AddUrlSegment(string name, string value) => AddParameter(name, value, ParameterType.UrlSegment);

        /// <summary>
        ///     Shortcut to AddParameter(name, value, QueryString) overload
        /// </summary>
        /// <param name="name">Name of the parameter to add</param>
        /// <param name="value">Value of the parameter to add</param>
        /// <returns></returns>
        public IRestRequest AddQueryParameter(string name, string value) => AddParameter(name, value, ParameterType.QueryString);

        /// <summary>
        ///     Shortcut to AddParameter(name, value, QueryString) overload
        /// </summary>
        /// <param name="name">Name of the parameter to add</param>
        /// <param name="value">Value of the parameter to add</param>
        /// <param name="encode">Whether parameter should be encoded or not</param>
        /// <returns></returns>
        public IRestRequest AddQueryParameter(string name, string value, bool encode)
            => AddParameter(name, value, encode ? ParameterType.QueryString : ParameterType.QueryStringWithoutEncode);

        /// <summary>
        ///     Add a Decompression Method to the request
        /// </summary>
        /// <param name="decompressionMethod">None | GZip | Deflate</param>
        /// <returns></returns>
        public IRestRequest AddDecompressionMethod(DecompressionMethods decompressionMethod)
        {
            if (!_allowedDecompressionMethods.Contains(decompressionMethod))
                _allowedDecompressionMethods.Add(decompressionMethod);

            return this;
        }

        /// <summary>
        ///     Container of all HTTP parameters to be passed with the request.
        ///     See AddParameter() for explanation of the types of parameters that can be passed
        /// </summary>
        public List<Parameter> Parameters { get; }

        /// <summary>
        ///     Container of all the files to be uploaded with the request.
        /// </summary>
        public List<FileParameter> Files { get; }

        /// <summary>
        ///     Determines what HTTP method to use for this request. Supported methods: GET, POST, PUT, DELETE, HEAD, OPTIONS
        ///     Default is GET
        /// </summary>
        public Method Method { get; set; }

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
        public string Resource { get; set; }

        /// <summary>
        ///     Determines how to serialize the request body.
        ///     By default Xml is used.
        /// </summary>
        public DataFormat RequestFormat { get; set; }

        /// <summary>
        ///     Used by the default deserializers to determine where to start deserializing from.
        ///     Can be used to skip container or root elements that do not have corresponding deserialzation targets.
        /// </summary>
        [Obsolete("Add custom content handler instead. This property will be removed.")]
        public string RootElement { get; set; }

        /// <summary>
        ///     A function to run prior to deserializing starting (e.g. change settings if error encountered)
        /// </summary>
        public Action<IRestResponse> OnBeforeDeserialization { get; set; }

        /// <summary>
        ///     A function to run after configuration of the HTTP request (e.g. set last minute headers)
        /// </summary>
        public Action<IHttp> OnBeforeRequest { get; set; }

        /// <summary>
        ///     Used by the default deserializers to explicitly set which date format string to use when parsing dates.
        /// </summary>
        [Obsolete("Add custom content handler instead. This property will be removed.")]
        public string DateFormat { get; set; }

        /// <summary>
        ///     Used by XmlDeserializer. If not specified, XmlDeserializer will flatten response by removing namespaces from
        ///     element names.
        /// </summary>
        [Obsolete("Add custom content handler instead. This property will be removed.")]
        public string XmlNamespace { get; set; }

        /// <summary>
        ///     In general you would not need to set this directly. Used by the NtlmAuthenticator.
        /// </summary>
        public ICredentials Credentials { get; set; }

        /// <summary>
        ///     Timeout in milliseconds to be used for the request. This timeout value overrides a timeout set on the RestClient.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        ///     The number of milliseconds before the writing or reading times out.  This timeout value overrides a timeout set on
        ///     the RestClient.
        /// </summary>
        public int ReadWriteTimeout { get; set; }

        /// <summary>
        ///     Internal Method so that RestClient can increase the number of attempts
        /// </summary>
        public void IncreaseNumAttempts() => Attempts++;

        /// <summary>
        ///     How many attempts were made to send this Request?
        /// </summary>
        /// <remarks>
        ///     This Number is incremented each time the RestClient sends the request.
        ///     Useful when using Asynchronous Execution with Callbacks
        /// </remarks>
        public int Attempts { get; private set; }

        /// <summary>
        ///     Shortcut to AddParameter(name, value, UrlSegment) overload
        /// </summary>
        /// <param name="name">Name of the segment to add</param>
        /// <param name="value">Value of the segment to add</param>
        /// <returns></returns>
        public IRestRequest AddUrlSegment(string name, object value) => AddParameter(name, value, ParameterType.UrlSegment);

        IRestRequest AddFile(FileParameter file) => this.With(x => x.Files.Add(file));
    }
}
