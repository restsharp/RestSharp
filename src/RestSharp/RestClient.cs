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
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth.Extensions;
using RestSharp.Deserializers;
using RestSharp.Extensions;
using RestSharp.Serialization;
using RestSharp.Serialization.Json;
using RestSharp.Serialization.Xml;
using static System.String;

namespace RestSharp
{
    /// <summary>
    ///     Client to translate RestRequests into Http requests and process response result
    /// </summary>
    public partial class RestClient : IRestClient
    {
        // silverlight friendly way to get current version      
        static readonly Version Version = new AssemblyName(typeof(RestClient).Assembly.FullName).Version;

        static readonly Regex StructuredSyntaxSuffixRegex = new Regex(@"\+\w+$");

        static readonly Regex StructuredSyntaxSuffixWildcardRegex = new Regex(@"^\*\+\w+$");

        static readonly ParameterType[] MultiParameterTypes =
            {ParameterType.QueryString, ParameterType.GetOrPost};

        /// <summary>
        ///     Default constructor that registers default content handlers
        /// </summary>
        public RestClient()
        {
            Encoding               = Encoding.UTF8;
            ContentHandlers        = new Dictionary<string, Func<IDeserializer>>();
            Serializers            = new Dictionary<DataFormat, IRestSerializer>();
            AcceptTypes            = new List<string>();
            DefaultParameters      = new List<Parameter>();
            AutomaticDecompression = true;

            // register default serializers
            UseSerializer<JsonSerializer>();
            UseSerializer<XmlRestSerializer>();

            FollowRedirects = true;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Sets the BaseUrl property for requests made by this client instance
        /// </summary>
        /// <param name="baseUrl"></param>
        public RestClient(Uri baseUrl) : this() => BaseUrl = baseUrl;

        /// <inheritdoc />
        /// <summary>
        ///     Sets the BaseUrl property for requests made by this client instance
        /// </summary>
        /// <param name="baseUrl"></param>
        public RestClient(string baseUrl) : this()
        {
            if (baseUrl.IsEmpty())
                throw new ArgumentNullException(nameof(baseUrl));

            BaseUrl = new Uri(baseUrl);
        }

        IDictionary<string, Func<IDeserializer>> ContentHandlers { get; }
        internal IDictionary<DataFormat, IRestSerializer> Serializers { get; }
        Func<string, string> Encode { get; set; } = s => s.UrlEncode();
        Func<string, Encoding, string> EncodeQuery { get; set; } = (s, encoding) => s.UrlEncode(encoding);
        IList<string> AcceptTypes { get; }
        Action<HttpWebRequest> WebRequestConfigurator { get; set; }

        /// <summary>
        ///     Replace the default serializer with a custom one
        /// </summary>
        /// <param name="serializer">The custom serializer instance</param>
        /// <returns></returns>
        [Obsolete("Use the overload that accepts the delegate factory")]
        public IRestClient UseSerializer(IRestSerializer serializer) => this.With(x => x.UseSerializer(() => serializer));

        /// <summary>
        ///     Allows to use a custom way to encode parameters
        /// </summary>
        /// <param name="encoder">A delegate to encode parameters</param>
        /// <example>client.UseUrlEncoder(s => HttpUtility.UrlEncode(s));</example>
        /// <returns></returns>
        public IRestClient UseUrlEncoder(Func<string, string> encoder) => this.With(x => x.Encode = encoder);

        public IRestClient UseQueryEncoder(Func<string, Encoding, string> queryEncoder) => this.With(x => x.EncodeQuery = queryEncoder);

        /// <summary>
        ///     Enable or disable automatic gzip/deflate decompression
        /// </summary>
        public bool AutomaticDecompression { get; set; }

        /// <summary>
        ///     Maximum number of redirects to follow if FollowRedirects is true
        /// </summary>
        public int? MaxRedirects { get; set; }

        /// <summary>
        ///     X509CertificateCollection to be sent with request
        /// </summary>
        public X509CertificateCollection ClientCertificates { get; set; }

        /// <summary>
        ///     Proxy to use for requests made by this client instance.
        ///     Passed on to underlying WebRequest if set.
        /// </summary>
        public IWebProxy Proxy { get; set; }

        /// <summary>
        ///     The cache policy to use for requests initiated by this client instance.
        /// </summary>
        public RequestCachePolicy CachePolicy { get; set; }

        public bool Pipelined { get; set; }

        /// <summary>
        ///     Default is true. Determine whether or not requests that result in
        ///     HTTP status codes of 3xx should follow returned redirect
        /// </summary>
        public bool FollowRedirects { get; set; }

        /// <summary>
        ///     The CookieContainer used for requests made by this client instance
        /// </summary>
        public CookieContainer CookieContainer { get; set; }

        /// <summary>
        ///     UserAgent to use for requests made by this client instance
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        ///     Timeout in milliseconds to use for requests made by this client instance.
        ///     If not set, the default timeout for HttpWebRequest is used.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        ///     The number of milliseconds before the writing or reading times out.
        /// </summary>
        public int ReadWriteTimeout { get; set; }

        /// <summary>
        ///     Whether to invoke async callbacks using the SynchronizationContext.Current captured when invoked
        /// </summary>
        public bool UseSynchronizationContext { get; set; }

        /// <summary>
        ///     Authenticator to use for requests made by this client instance
        /// </summary>
        public IAuthenticator Authenticator { get; set; }

        /// <summary>
        ///     Combined with Request.Resource to construct URL for request
        ///     Should include scheme and domain without trailing slash.
        /// </summary>
        /// <example>
        ///     client.BaseUrl = new Uri("http://example.com");
        /// </example>
        public virtual Uri BaseUrl { get; set; }

        public Encoding Encoding { get; set; }

        public bool PreAuthenticate { get; set; }

        /// <summary>
        ///     Set to true if you want to get an exception when deserialization fails.
        ///     Default is false.
        /// </summary>
        public bool ThrowOnDeserializationError { get; set; } = false;

        /// <summary>
        ///     Set to false if you want to get ResponseStatus.Completed when deserialization fails.
        ///     Default is true.
        /// </summary>
        public bool FailOnDeserializationError { get; set; } = true;

        /// <summary>
        /// Changes the default behaviour when RestSharp swallows server errors in favour
        /// of setting error properties on the response object. Default is false.
        /// </summary>
        public bool ThrowOnAnyError { get; set; } = false;

        /// <summary>
        ///     Allow high-speed NTLM-authenticated connection sharing
        /// </summary>
        public bool UnsafeAuthenticatedConnectionSharing { get; set; }

        /// <summary>
        ///     The ConnectionGroupName property enables you to associate a request with a connection group.
        /// </summary>
        public string ConnectionGroupName { get; set; }

        /// <summary>
        ///     Callback function for handling the validation of remote certificates. Useful for certificate pinning and
        ///     overriding certificate errors in the scope of a request.
        /// </summary>
        public RemoteCertificateValidationCallback RemoteCertificateValidationCallback { get; set; }

        /// <summary>
        ///     Parameters included with every request made with this instance of RestClient
        ///     If specified in both client and request, the request wins
        /// </summary>
        public IList<Parameter> DefaultParameters { get; }

        /// <summary>
        ///     Explicit Host header value to use in requests independent from the request URI.
        ///     If null, default host value extracted from URI is used.
        /// </summary>
        public string BaseHost { get; set; }

        /// <summary>
        ///     Set to true if you need to add multiple default parameters with the same name.
        ///     Only query and form parameters are supported.
        /// </summary>
        public bool AllowMultipleDefaultParametersWithSameName { get; set; } = false;

        /// <summary>
        ///     Registers a content handler to process response content
        /// </summary>
        /// <param name="contentType">MIME content type of the response content</param>
        /// <param name="deserializerFactory">Deserializer to use to process content</param>
        public void AddHandler(string contentType, Func<IDeserializer> deserializerFactory)
        {
            ContentHandlers[contentType] = deserializerFactory;

            if (contentType == "*" || IsWildcardStructuredSuffixSyntax(contentType)) return;

            if (!AcceptTypes.Contains(contentType))
                AcceptTypes.Add(contentType);

            // add Accept header based on registered deserializers
            var accepts = AcceptTypes.JoinToString(", ");

            this.AddOrUpdateDefaultParameter(new Parameter("Accept", accepts, ParameterType.HttpHeader));
        }

        /// <summary>
        ///     Registers a content handler to process response content
        /// </summary>
        /// <param name="contentType">MIME content type of the response content</param>
        /// <param name="deserializer">Deserializer to use to process content</param>
        [Obsolete("Use the overload that accepts a factory delegate")]
        public void AddHandler(string contentType, IDeserializer deserializer) => AddHandler(contentType, () => deserializer);

        /// <summary>
        ///     Remove a content handler for the specified MIME content type
        /// </summary>
        /// <param name="contentType">MIME content type to remove</param>
        public void RemoveHandler(string contentType)
        {
            ContentHandlers.Remove(contentType);
            AcceptTypes.Remove(contentType);
            this.RemoveDefaultParameter("Accept");
        }

        /// <summary>
        ///     Remove all content handlers
        /// </summary>
        public void ClearHandlers()
        {
            ContentHandlers.Clear();
            AcceptTypes.Clear();
            this.RemoveDefaultParameter("Accept");
        }

        public IRestResponse<T> Deserialize<T>(IRestResponse response) => Deserialize<T>(response.Request, response);

        public void ConfigureWebRequest(Action<HttpWebRequest> configurator) => WebRequestConfigurator = configurator;

        /// <summary>
        ///     Assembles URL to call based on parameters, method and resource
        /// </summary>
        /// <param name="request">RestRequest to execute</param>
        /// <returns>Assembled System.Uri</returns>
        public Uri BuildUri(IRestRequest request)
        {
            DoBuildUriValidations(request);

            var applied = GetUrlSegmentParamsValues(request);

            var mergedUri = MergeBaseUrlAndResource(applied.Uri, applied.Resource);

            var finalUri = ApplyQueryStringParamsValuesToUri(mergedUri, request);

            return new Uri(finalUri);
        }

        string IRestClient.BuildUriWithoutQueryParameters(IRestRequest request)
        {
            DoBuildUriValidations(request);

            var applied = GetUrlSegmentParamsValues(request);

            return MergeBaseUrlAndResource(applied.Uri, applied.Resource);
        }

        /// <summary>
        ///     Replace the default serializer with a custom one
        /// </summary>
        /// <param name="serializerFactory">A function that creates a custom serializer instance</param>
        /// <returns></returns>
        public IRestClient UseSerializer(Func<IRestSerializer> serializerFactory)
        {
            var instance = serializerFactory();
            Serializers[instance.DataFormat] = instance;

            AddHandler(serializerFactory, instance.SupportedContentTypes);

            return this;
        }

        /// <summary>
        ///     Replace the default serializer with a custom one
        /// </summary>
        /// <typeparam name="T">The type that implements IRestSerializer</typeparam>
        /// <returns></returns>
        public IRestClient UseSerializer<T>() where T : IRestSerializer, new() => UseSerializer(() => new T());

        /// <summary>
        ///     Registers a content handler to process response content
        /// </summary>
        /// <param name="contentTypes">A list of MIME content types of the response content</param>
        /// <param name="deserializerFactory">Deserializer factory to use to process content</param>
        public void AddHandler(Func<IDeserializer> deserializerFactory, params string[] contentTypes)
        {
            foreach (var contentType in contentTypes)
                AddHandler(contentType, deserializerFactory);
        }

        /// <summary>
        ///     Registers a content handler to process response content
        /// </summary>
        /// <param name="contentTypes">A list of MIME content types of the response content</param>
        /// <param name="deserializer">Deserializer to use to process content</param>
        [Obsolete("Use the overload that accepts a factory delegate")]
        public void AddHandler(IDeserializer deserializer, params string[] contentTypes)
        {
            foreach (var contentType in contentTypes)
                AddHandler(contentType, deserializer);
        }

        void DoBuildUriValidations(IRestRequest request)
        {
            if (BaseUrl == null && !request.Resource.ToLower().StartsWith("http"))
                throw new ArgumentOutOfRangeException(
                    nameof(request),
                    "Request resource doesn't contain a valid scheme for an empty client base URL"
                );

            var nullValuedParams = request.Parameters
                .Where(p => p.Type == ParameterType.UrlSegment && p.Value == null)
                .Select(p => p.Name)
                .ToArray();

            if (nullValuedParams.Any())
            {
                var names = nullValuedParams.JoinToString(", ", name => $"'{name}'");

                throw new ArgumentException(
                    $"Cannot build uri when url segment parameter(s) {names} value is null.",
                    nameof(request)
                );
            }
        }

        UrlSegmentParamsValues GetUrlSegmentParamsValues(IRestRequest request)
        {
            var assembled = BaseUrl == null ? "" : request.Resource;
            var baseUrl   = BaseUrl ?? new Uri(request.Resource);

            var hasResource = !assembled.IsEmpty();
            var parameters  = request.Parameters.Where(p => p.Type == ParameterType.UrlSegment).ToList();
            parameters.AddRange(DefaultParameters.Where(p => p.Type == ParameterType.UrlSegment));
            var builder = new UriBuilder(baseUrl);

            foreach (var parameter in parameters)
            {
                var paramPlaceHolder = $"{{{parameter.Name}}}";
                var paramValue       = Encode(parameter.Value.ToString());

                if (hasResource)
                    assembled = assembled.Replace(paramPlaceHolder, paramValue);

                builder.Path = builder.Path.UrlDecode().Replace(paramPlaceHolder, paramValue);
            }

            return new UrlSegmentParamsValues(builder.Uri, assembled);
        }

        static string MergeBaseUrlAndResource(Uri baseUrl, string resource)
        {
            var assembled = resource;

            if (!IsNullOrEmpty(assembled) && assembled.StartsWith("/")) assembled = assembled.Substring(1);

            if (baseUrl == null || IsNullOrEmpty(baseUrl.AbsoluteUri)) return assembled;

            var usingBaseUri                                                                  = baseUrl;
            if (!baseUrl.AbsoluteUri.EndsWith("/") && !IsNullOrEmpty(assembled)) usingBaseUri = new Uri(baseUrl.AbsoluteUri + "/");

            return assembled != null ? new Uri(usingBaseUri, assembled).AbsoluteUri : baseUrl.AbsoluteUri;
        }

        string ApplyQueryStringParamsValuesToUri(string mergedUri, IRestRequest request)
        {
            var parameters = GetQueryStringParameters(request).ToList();
            parameters.AddRange(GetDefaultQueryStringParameters(request));

            if (!parameters.Any()) return mergedUri;

            var separator = mergedUri != null && mergedUri.Contains("?") ? "&" : "?";

            return Concat(mergedUri, separator, EncodeParameters(parameters, Encoding));
        }

        IEnumerable<Parameter> GetDefaultQueryStringParameters(IRestRequest request)
            => request.Method != Method.POST && request.Method != Method.PUT && request.Method != Method.PATCH
                ? DefaultParameters
                    .Where(
                        p => p.Type == ParameterType.GetOrPost   ||
                            p.Type  == ParameterType.QueryString ||
                            p.Type  == ParameterType.QueryStringWithoutEncode
                    )
                : DefaultParameters
                    .Where(
                        p => p.Type == ParameterType.QueryString ||
                            p.Type  == ParameterType.QueryStringWithoutEncode
                    );

        static IEnumerable<Parameter> GetQueryStringParameters(IRestRequest request)
            => request.Method != Method.POST && request.Method != Method.PUT && request.Method != Method.PATCH
                ? request.Parameters
                    .Where(
                        p => p.Type == ParameterType.GetOrPost   ||
                            p.Type  == ParameterType.QueryString ||
                            p.Type  == ParameterType.QueryStringWithoutEncode
                    )
                : request.Parameters
                    .Where(
                        p => p.Type == ParameterType.QueryString ||
                            p.Type  == ParameterType.QueryStringWithoutEncode
                    );

        Func<IDeserializer> GetHandler(string contentType)
        {
            if (contentType.IsEmpty() && ContentHandlers.ContainsKey("*"))
                return ContentHandlers["*"];

            if (contentType.IsEmpty())
                return ContentHandlers.First().Value;

            var semicolonIndex = contentType.IndexOf(';');

            if (semicolonIndex > -1)
                contentType = contentType.Substring(0, semicolonIndex);

            if (ContentHandlers.TryGetValue(contentType, out var contentHandler))
                return contentHandler;

            // Avoid unnecessary use of regular expressions in checking for structured syntax suffix by looking for a '+' first
            if (contentType.IndexOf('+') >= 0)
            {
                // https://tools.ietf.org/html/rfc6839#page-4
                var structuredSyntaxSuffixMatch = StructuredSyntaxSuffixRegex.Match(contentType);

                if (structuredSyntaxSuffixMatch.Success)
                {
                    var structuredSyntaxSuffixWildcard = "*" + structuredSyntaxSuffixMatch.Value;
                    if (ContentHandlers.TryGetValue(structuredSyntaxSuffixWildcard, out var contentHandlerWildcard)) return contentHandlerWildcard;
                }
            }

            return ContentHandlers.ContainsKey("*") ? ContentHandlers["*"] : null;
        }

        void AuthenticateIfNeeded(IRestRequest request) => Authenticator?.Authenticate(this, request);

        string EncodeParameters(IEnumerable<Parameter> parameters, Encoding encoding)
            => Join("&", parameters.Select(parameter => EncodeParameter(parameter, encoding)).ToArray());

        string EncodeParameter(Parameter parameter, Encoding encoding)
        {
            return
                parameter.Type == ParameterType.QueryStringWithoutEncode
                    ? $"{parameter.Name}={StringOrEmpty(parameter.Value)}"
                    : $"{EncodeQuery(parameter.Name, encoding)}={EncodeQuery(StringOrEmpty(parameter.Value), encoding)}";

            static string StringOrEmpty(object value) => value == null ? "" : value.ToString();
        }

        IHttp ConfigureHttp(IRestRequest request)
        {
            var http = new Http
            {
                Encoding                = Encoding,
                AlwaysMultipartFormData = request.AlwaysMultipartFormData,
                UseDefaultCredentials   = request.UseDefaultCredentials,
                ResponseWriter          = request.ResponseWriter,
                AdvancedResponseWriter  = request.AdvancedResponseWriter,
                CookieContainer         = CookieContainer,
                AutomaticDecompression  = AutomaticDecompression,
                WebRequestConfigurator  = WebRequestConfigurator,
                Encode                  = Encode
            };

            var requestParameters = new List<Parameter>();
            requestParameters.AddRange(request.Parameters);

            // move RestClient.DefaultParameters into Request.Parameters
            foreach (var defaultParameter in DefaultParameters)
            {
                var parameterExists =
                    request.Parameters.Any(
                        p =>
                            p.Name != null
                            && p.Name.Equals(defaultParameter.Name, StringComparison.InvariantCultureIgnoreCase)
                            && p.Type == defaultParameter.Type
                    );

                if (AllowMultipleDefaultParametersWithSameName)
                {
                    var isMultiParameter = MultiParameterTypes.Any(pt => pt == defaultParameter.Type);
                    parameterExists = !isMultiParameter && parameterExists;
                }

                if (!parameterExists) requestParameters.Add(defaultParameter);
            }

            // Add Accept header based on registered deserializers if none has been set by the caller.
            if (requestParameters.All(
                p => !p.Name.EqualsIgnoreCase("accept")
            ))
            {
                var accepts = Join(", ", AcceptTypes);

                requestParameters.Add(new Parameter("Accept", accepts, ParameterType.HttpHeader));
            }

            http.Url                                  = BuildUri(request);
            http.Host                                 = BaseHost;
            http.PreAuthenticate                      = PreAuthenticate;
            http.UnsafeAuthenticatedConnectionSharing = UnsafeAuthenticatedConnectionSharing;

            var userAgent = UserAgent ?? http.UserAgent;

            http.UserAgent = userAgent.HasValue()
                ? userAgent
                : "RestSharp/" + Version;

            var timeout = request.Timeout != 0
                ? request.Timeout
                : Timeout;

            if (timeout != 0)
                http.Timeout = timeout;

            var readWriteTimeout = request.ReadWriteTimeout != 0
                ? request.ReadWriteTimeout
                : ReadWriteTimeout;

            if (readWriteTimeout != 0)
                http.ReadWriteTimeout = readWriteTimeout;

            http.FollowRedirects = FollowRedirects;

            if (ClientCertificates != null)
                http.ClientCertificates = ClientCertificates;

            http.MaxRedirects = MaxRedirects;
            http.CachePolicy  = CachePolicy;
            http.Pipelined    = Pipelined;

            if (request.Credentials != null)
                http.Credentials = request.Credentials;

            if (!IsNullOrEmpty(ConnectionGroupName))
                http.ConnectionGroupName = ConnectionGroupName;

            http.Headers = requestParameters
                .Where(p => p.Type == ParameterType.HttpHeader)
                .Select(p => new HttpHeader {Name = p.Name, Value = Convert.ToString(p.Value)})
                .ToList();

            http.Cookies = requestParameters
                .Where(p => p.Type == ParameterType.Cookie)
                .Select(p => new HttpCookie {Name = p.Name, Value = Convert.ToString(p.Value)})
                .ToList();

            http.Parameters = requestParameters
                .Where(p => p.Type == ParameterType.GetOrPost && p.Value != null)
                .Select(p => new HttpParameter {Name = p.Name, Value = Convert.ToString(p.Value)})
                .ToList();

            http.Files = request.Files.Select(
                    file => new HttpFile
                    {
                        Name          = file.Name,
                        ContentType   = file.ContentType,
                        Writer        = file.Writer,
                        FileName      = file.FileName,
                        ContentLength = file.ContentLength
                    }
                )
                .ToList();

            if (request.Body != null)
                http.AddBody(request.Body);

            http.AllowedDecompressionMethods = request.AllowedDecompressionMethods;

            var proxy = Proxy ?? WebRequest.DefaultWebProxy;

            try
            {
                proxy ??= WebRequest.GetSystemWebProxy();
            }
            catch (PlatformNotSupportedException)
            {
                // Ignore platform unsupported proxy detection
            }

            http.Proxy = proxy;

            http.RemoteCertificateValidationCallback = RemoteCertificateValidationCallback;

            return http;
        }

        IRestResponse<T> Deserialize<T>(IRestRequest request, IRestResponse raw)
        {
            IRestResponse<T> response = new RestResponse<T>();

            try
            {
                request.OnBeforeDeserialization?.Invoke(raw);

                response = raw.ToAsyncResponse<T>();

                // Only attempt to deserialize if the request has not errored due
                // to a transport or framework exception.  HTTP errors should attempt to 
                // be deserialized 
                if (response.ErrorException == null)
                {
                    var handler = GetHandler(raw.ContentType)();

                    // Only continue if there is a handler defined else there is no way to deserialize the data.
                    // This can happen when a request returns for example a 404 page instead of the requested JSON/XML resource
                    if (handler is IXmlDeserializer xml)
                    {
                        if (request.DateFormat.IsNotEmpty())
                            xml.DateFormat = request.DateFormat;

                        if (request.XmlNamespace.IsNotEmpty())
                            xml.Namespace = request.XmlNamespace;
                    }

                    if (handler is IWithRootElement deserializer && !request.RootElement.IsEmpty())
                        deserializer.RootElement = request.RootElement;

                    if (handler != null)
                        response.Data = handler.Deserialize<T>(raw);
                }
            }
            catch (Exception ex)
            {
                if (ThrowOnAnyError) throw;

                if (FailOnDeserializationError || ThrowOnDeserializationError)
                    response.ResponseStatus = ResponseStatus.Error;

                response.ErrorMessage   = ex.Message;
                response.ErrorException = ex;

                if (ThrowOnDeserializationError)
                    throw new DeserializationException(response, ex);
            }

            response.Request = request;

            return response;
        }

        static bool IsWildcardStructuredSuffixSyntax(string contentType)
        {
            var i = 0;

            // Avoid most unnecessary uses of RegEx by checking for necessary characters explicitly first
            if (contentType[i++] != '*')
                return false;

            if (contentType[i++] != '+')
                return false;

            // If no more characters to check, exit now
            if (i == contentType.Length)
                return false;

            // At this point it is probably using a wildcard structured syntax suffix, but let's confirm.
            return StructuredSyntaxSuffixWildcardRegex.IsMatch(contentType);
        }

        class UrlSegmentParamsValues
        {
            public UrlSegmentParamsValues(Uri builderUri, string assembled)
            {
                Uri      = builderUri;
                Resource = assembled;
            }

            public Uri Uri { get; }
            public string Resource { get; }
        }
    }
}