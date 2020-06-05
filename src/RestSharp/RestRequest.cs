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
using JetBrains.Annotations;
using RestSharp.Extensions;
using RestSharp.Serialization.Xml;
using RestSharp.Serializers;

// ReSharper disable IntroduceOptionalParameters.Global

namespace RestSharp
{
    /// <summary>
    /// Container for data used to make requests
    /// </summary>
    [PublicAPI]
    public class RestRequest : IRestRequest
    {
        static readonly Regex PortSplitRegex = new Regex(@":\d+");

        readonly IList<DecompressionMethods> _allowedDecompressionMethods;

        Action<Stream, IHttpResponse> _advancedResponseWriter;

        Action<Stream> _responseWriter;

        /// <summary>
        /// Default constructor
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
        /// Sets Method property to value of method
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
                => query.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
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

        /// <inheritdoc />
        public IList<DecompressionMethods> AllowedDecompressionMethods => _allowedDecompressionMethods.Any()
            ? _allowedDecompressionMethods
            : new[] {DecompressionMethods.None, DecompressionMethods.Deflate, DecompressionMethods.GZip};

        /// <inheritdoc />
        public bool AlwaysMultipartFormData { get; set; }

        /// <inheritdoc />
        public ISerializer JsonSerializer { get; set; }

        /// <inheritdoc />
        public IXmlSerializer XmlSerializer { get; set; }

        /// <inheritdoc />
        public RequestBody Body { get; set; }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool UseDefaultCredentials { get; set; }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public IRestRequest AddFile(string name, byte[] bytes, string fileName, string contentType = null)
            => AddFile(FileParameter.Create(name, bytes, fileName, contentType));

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        [Obsolete("Use AddXmlBody")]
        public IRestRequest AddBody(object obj, string xmlNamespace)
            => RequestFormat switch
            {
                DataFormat.Json => AddJsonBody(obj),
                DataFormat.Xml  => AddXmlBody(obj, xmlNamespace),
                _               => this
            };

        /// <inheritdoc />
        [Obsolete("Use AddXmlBody or AddJsonBody")]
        public IRestRequest AddBody(object obj)
            => RequestFormat switch
            {
                DataFormat.Json => AddJsonBody(obj),
                DataFormat.Xml  => AddXmlBody(obj),
                _               => this
            };

        /// <inheritdoc />
        public IRestRequest AddJsonBody(object obj)
        {
            RequestFormat = DataFormat.Json;

            return AddParameter(new JsonParameter("", obj));
        }

        /// <inheritdoc />
        public IRestRequest AddJsonBody(object obj, string contentType)
        {
            RequestFormat = DataFormat.Json;

            return AddParameter(new JsonParameter(contentType, obj, contentType));
        }

        /// <inheritdoc />
        public IRestRequest AddXmlBody(object obj) => AddXmlBody(obj, "");

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public IRestRequest AddObject(object obj) => this.With(x => x.AddObject(obj, new string[] { }));

        /// <inheritdoc />
        public IRestRequest AddParameter(Parameter p) => this.With(x => x.Parameters.Add(p));

        /// <inheritdoc />
        public IRestRequest AddParameter(string name, object value) => AddParameter(new Parameter(name, value, ParameterType.GetOrPost));

        /// <inheritdoc />
        public IRestRequest AddParameter(string name, object value, ParameterType type) => AddParameter(new Parameter(name, value, type));

        /// <inheritdoc />
        public IRestRequest AddParameter(string name, object value, string contentType, ParameterType type)
            => AddParameter(new Parameter(name, value, contentType, type));

        /// <inheritdoc />
        public IRestRequest AddOrUpdateParameter(Parameter parameter)
        {
            var p = Parameters
                .FirstOrDefault(x => x.Name == parameter.Name && x.Type == parameter.Type);

            if (p != null) Parameters.Remove(p);

            Parameters.Add(parameter);
            return this;
        }

        /// <inheritdoc />
        public IRestRequest AddOrUpdateParameters(IEnumerable<Parameter> parameters)
        {
            foreach (var parameter in parameters)
                AddOrUpdateParameter(parameter);

            return this;
        }

        /// <inheritdoc />
        public IRestRequest AddOrUpdateParameter(string name, object value)
            => AddOrUpdateParameter(new Parameter(name, value, ParameterType.GetOrPost));

        /// <inheritdoc />
        public IRestRequest AddOrUpdateParameter(string name, object value, ParameterType type)
            => AddOrUpdateParameter(new Parameter(name, value, type));

        /// <inheritdoc />
        public IRestRequest AddOrUpdateParameter(string name, object value, string contentType, ParameterType type)
            => AddOrUpdateParameter(new Parameter(name, value, contentType, type));

        /// <inheritdoc />
        public IRestRequest AddHeader(string name, string value)
        {
            static bool InvalidHost(string host) => Uri.CheckHostName(PortSplitRegex.Split(host)[0]) == UriHostNameType.Unknown;

            if (name == "Host" && InvalidHost(value))
                throw new ArgumentException("The specified value is not a valid Host header string.", nameof(value));

            return AddParameter(name, value, ParameterType.HttpHeader);
        }

        /// <inheritdoc />
        public IRestRequest AddHeaders(ICollection<KeyValuePair<string, string>> headers)
        {
            var duplicateKeys = headers
                .GroupBy(pair => pair.Key.ToUpperInvariant())
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            if (duplicateKeys.Any())
                throw new ArgumentException($"Duplicate header names exist: {string.Join(", ", duplicateKeys)}");

            foreach (var pair in headers)
            {
                AddHeader(pair.Key, pair.Value);
            }

            return this;
        }

        /// <inheritdoc />
        public IRestRequest AddCookie(string name, string value) => AddParameter(name, value, ParameterType.Cookie);

        /// <inheritdoc />
        public IRestRequest AddUrlSegment(string name, string value) => AddParameter(name, value, ParameterType.UrlSegment);

        /// <inheritdoc />
        public IRestRequest AddQueryParameter(string name, string value) => AddParameter(name, value, ParameterType.QueryString);

        /// <inheritdoc />
        public IRestRequest AddQueryParameter(string name, string value, bool encode)
            => AddParameter(name, value, encode ? ParameterType.QueryString : ParameterType.QueryStringWithoutEncode);

        /// <inheritdoc />
        public IRestRequest AddDecompressionMethod(DecompressionMethods decompressionMethod)
        {
            if (!_allowedDecompressionMethods.Contains(decompressionMethod))
                _allowedDecompressionMethods.Add(decompressionMethod);

            return this;
        }

        /// <inheritdoc />
        public List<Parameter> Parameters { get; }

        /// <inheritdoc />
        public List<FileParameter> Files { get; }

        /// <inheritdoc />
        public Method Method { get; set; }

        /// <inheritdoc />
        public string Resource { get; set; }

        /// <inheritdoc />
        public DataFormat RequestFormat { get; set; }

        /// <inheritdoc />
        [Obsolete("Add custom content handler instead. This property will be removed.")]
        public string RootElement { get; set; }

        /// <inheritdoc />
        public Action<IRestResponse> OnBeforeDeserialization { get; set; }

        /// <inheritdoc />
        public Action<IHttp> OnBeforeRequest { get; set; }

        /// <inheritdoc />
        [Obsolete("Add custom content handler instead. This property will be removed.")]
        public string DateFormat { get; set; }

        /// <inheritdoc />
        [Obsolete("Add custom content handler instead. This property will be removed.")]
        public string XmlNamespace { get; set; }

        /// <inheritdoc />
        public ICredentials Credentials { get; set; }

        /// <inheritdoc />
        public int Timeout { get; set; }

        /// <inheritdoc />
        public int ReadWriteTimeout { get; set; }

        /// <inheritdoc />
        public void IncreaseNumAttempts() => Attempts++;

        /// <inheritdoc />
        public int Attempts { get; private set; }

        /// <inheritdoc />
        public IRestRequest AddUrlSegment(string name, object value) => AddParameter(name, value, ParameterType.UrlSegment);

        IRestRequest AddFile(FileParameter file) => this.With(x => x.Files.Add(file));
    }
}
