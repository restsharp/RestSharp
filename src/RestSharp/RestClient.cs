using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth.Extensions;
using RestSharp.Extensions;
using RestSharp.Serializers;
using RestSharp.Serializers.Json;
using RestSharp.Serializers.Xml;
using static System.String;

// ReSharper disable VirtualMemberCallInConstructor
#pragma warning disable 618

namespace RestSharp;

/// <summary>
/// Client to translate RestRequests into Http requests and process response result
/// </summary>
[PublicAPI]
public partial class RestClient : IRestClient {
    static readonly Regex StructuredSyntaxSuffixRegex = new(@"\+\w+$");

    static readonly Regex StructuredSyntaxSuffixWildcardRegex = new(@"^\*\+\w+$");

    static readonly ParameterType[] MultiParameterTypes = { ParameterType.QueryString, ParameterType.GetOrPost };

    readonly List<string>    _acceptTypes = new();
    readonly CookieContainer _cookieContainer;

    HttpClient HttpClient { get; }

    internal RestClientOptions Options { get; }

    /// <summary>
    /// Default constructor that registers default content handlers
    /// </summary>
    public RestClient() : this(new RestClientOptions()) { }

    public RestClient(RestClientOptions options) {
        // register default serializers
        UseSerializer<SystemTextJsonSerializer>();
        UseSerializer<XmlRestSerializer>();

        Options          = options;
        _cookieContainer = Options.CookieContainer ?? new CookieContainer();

        var handler = new HttpClientHandler {
            Credentials            = Options.Credentials,
            UseDefaultCredentials  = Options.UseDefaultCredentials,
            CookieContainer        = _cookieContainer,
            AutomaticDecompression = Options.AutomaticDecompression,
            PreAuthenticate        = Options.PreAuthenticate,
            AllowAutoRedirect      = Options.FollowRedirects,
            Proxy                  = Options.Proxy,
        };

        if (Options.RemoteCertificateValidationCallback != null)
            handler.ServerCertificateCustomValidationCallback =
                (request, cert, chain, errors) => Options.RemoteCertificateValidationCallback(request, cert, chain, errors);

        if (Options.ClientCertificates != null)
            handler.ClientCertificates.AddRange(Options.ClientCertificates);

        if (Options.MaxRedirects.HasValue)
            handler.MaxAutomaticRedirections = Options.MaxRedirects.Value;

        HttpClient         = new HttpClient(handler);
        HttpClient.Timeout = TimeSpan.FromMilliseconds(Options.Timeout);
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Options.UserAgent);
    }

    /// <inheritdoc />
    /// <summary>
    /// Sets the BaseUrl property for requests made by this client instance
    /// </summary>
    /// <param name="baseUrl"></param>
    public RestClient(Uri baseUrl) : this(new RestClientOptions { BaseUrl = baseUrl }) { }

    /// <inheritdoc />
    /// <summary>
    /// Sets the BaseUrl property for requests made by this client instance
    /// </summary>
    /// <param name="baseUrl"></param>
    public RestClient(string baseUrl) : this(new Uri(Ensure.NotEmptyString(baseUrl, nameof(baseUrl)))) { }

    internal Dictionary<DataFormat, IRestSerializer> Serializers { get; } = new();

    Func<string, string> Encode { get; set; } = s => s.UrlEncode();

    Func<string, Encoding, string> EncodeQuery { get; set; } = (s, encoding) => s.UrlEncode(encoding);

    /// <inheritdoc />
    public IRestClient UseUrlEncoder(Func<string, string> encoder) => this.With(x => x.Encode = encoder);

    /// <inheritdoc />
    public IRestClient UseQueryEncoder(Func<string, Encoding, string> queryEncoder) => this.With(x => x.EncodeQuery = queryEncoder);

    public byte[] DownloadData(IRestRequest request) => throw new NotImplementedException();

    /// <inheritdoc />
    public IAuthenticator? Authenticator { get; set; }

    public IList<Parameter> DefaultParameters { get; } = new List<Parameter>();

    /// <inheritdoc />
    public RestResponse<T> Deserialize<T>(RestResponse response) => Deserialize<T>(response.Request, response);

    /// <inheritdoc />
    public Uri BuildUri(IRestRequest request) {
        DoBuildUriValidations(request);

        var applied = GetUrlSegmentParamsValues(request);

        var mergedUri = MergeBaseUrlAndResource(applied.Uri, applied.Resource);

        var finalUri = ApplyQueryStringParamsValuesToUri(mergedUri, request);

        return new Uri(finalUri);
    }

    internal string? BuildUriWithoutQueryParameters(IRestRequest request) {
        DoBuildUriValidations(request);

        var applied = GetUrlSegmentParamsValues(request);

        return MergeBaseUrlAndResource(applied.Uri, applied.Resource);
    }

    /// <inheritdoc />
    public IRestClient UseSerializer(Func<IRestSerializer> serializerFactory) {
        var instance = serializerFactory();
        Serializers[instance.DataFormat] = instance;
        return this;
    }

    /// <inheritdoc />
    public IRestClient UseSerializer<T>() where T : class, IRestSerializer, new() => UseSerializer(() => new T());

    void DoBuildUriValidations(IRestRequest request) {
        if (Options.BaseUrl == null && !request.Resource.ToLowerInvariant().StartsWith("http"))
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "Request resource doesn't contain a valid scheme for an empty client base URL"
            );

        var nullValuedParams = request.Parameters
            .Where(p => p.Type == ParameterType.UrlSegment && p.Value == null)
            .Select(p => p.Name)
            .ToArray();

        if (nullValuedParams.Any()) {
            var names = nullValuedParams.JoinToString(", ", name => $"'{name}'");

            throw new ArgumentException(
                $"Cannot build uri when url segment parameter(s) {names} value is null.",
                nameof(request)
            );
        }
    }

    UrlSegmentParamsValues GetUrlSegmentParamsValues(IRestRequest request) {
        var assembled = Options.BaseUrl == null ? "" : request.Resource;
        var baseUrl   = Options.BaseUrl ?? new Uri(request.Resource);

        var hasResource = !assembled.IsEmpty();
        var parameters  = request.Parameters.Where(p => p.Type == ParameterType.UrlSegment).ToList();
        parameters.AddRange(Options.DefaultParameters.Where(p => p.Type == ParameterType.UrlSegment));
        var builder = new UriBuilder(baseUrl);

        foreach (var parameter in parameters) {
            var paramPlaceHolder = $"{{{parameter.Name}}}";
            var paramValue       = parameter.Encode ? Encode(parameter.Value!.ToString()) : parameter.Value!.ToString();

            if (hasResource) assembled = assembled.Replace(paramPlaceHolder, paramValue);

            builder.Path = builder.Path.UrlDecode().Replace(paramPlaceHolder, paramValue);
        }

        return new UrlSegmentParamsValues(builder.Uri, assembled);
    }

    static string? MergeBaseUrlAndResource(Uri? baseUrl, string? resource) {
        var assembled = resource;

        if (!IsNullOrEmpty(assembled) && assembled!.StartsWith("/")) assembled = assembled.Substring(1);

        if (baseUrl == null || IsNullOrEmpty(baseUrl.AbsoluteUri)) return assembled;

        var usingBaseUri = baseUrl.AbsoluteUri.EndsWith("/") || IsNullOrEmpty(assembled) ? baseUrl : new Uri(baseUrl.AbsoluteUri + "/");

        return assembled != null ? new Uri(usingBaseUri, assembled).AbsoluteUri : baseUrl.AbsoluteUri;
    }

    string? ApplyQueryStringParamsValuesToUri(string? mergedUri, IRestRequest request) {
        var parameters = GetQueryStringParameters(request).ToList();
        parameters.AddRange(GetDefaultQueryStringParameters(request));

        if (!parameters.Any()) return mergedUri;

        var separator = mergedUri != null && mergedUri.Contains("?") ? "&" : "?";

        return Concat(mergedUri, separator, EncodeParameters(parameters, Options.Encoding));
    }

    IEnumerable<Parameter> GetDefaultQueryStringParameters(IRestRequest request)
        => request.Method != Method.Post && request.Method != Method.Put && request.Method != Method.Patch
            ? Options.DefaultParameters
                .Where(
                    p => p.Type is ParameterType.GetOrPost or ParameterType.QueryString
                )
            : Options.DefaultParameters
                .Where(
                    p => p.Type is ParameterType.QueryString
                );

    static IEnumerable<Parameter> GetQueryStringParameters(IRestRequest request)
        => request.Method != Method.Post && request.Method != Method.Put && request.Method != Method.Patch
            ? request.Parameters
                .Where(
                    p => p.Type is ParameterType.GetOrPost or ParameterType.QueryString
                )
            : request.Parameters
                .Where(
                    p => p.Type is ParameterType.QueryString
                );

    string EncodeParameters(IEnumerable<Parameter> parameters, Encoding encoding)
        => Join("&", parameters.Select(parameter => EncodeParameter(parameter, encoding)).ToArray());

    string EncodeParameter(Parameter parameter, Encoding encoding) {
        return
            !parameter.Encode
                ? $"{parameter.Name}={StringOrEmpty(parameter.Value)}"
                : $"{EncodeQuery(parameter.Name!, encoding)}={EncodeQuery(StringOrEmpty(parameter.Value), encoding)}";

        static string StringOrEmpty(object? value) => value == null ? "" : value.ToString();
    }

    Http ConfigureHttp(IRestRequest request) {
        var http = new Http {
            // Encoding                = Encoding,
            AlwaysMultipartFormData = request.AlwaysMultipartFormData,
            // UseDefaultCredentials   = request.UseDefaultCredentials,
            ResponseWriter         = request.ResponseWriter,
            AdvancedResponseWriter = request.AdvancedResponseWriter,
            // CookieContainer         = CookieContainer,
            // AutomaticDecompression  = AutomaticDecompression,
            // WebRequestConfigurator  = WebRequestConfigurator,
            Encode          = Encode,
            ThrowOnAnyError = Options.ThrowOnAnyError,
        };

        #region Parameters

        var requestParameters = new List<Parameter>();
        requestParameters.AddRange(request.Parameters);

        // move RestClient.DefaultParameters into Request.Parameters
        foreach (var defaultParameter in Options.DefaultParameters) {
            var parameterExists =
                request.Parameters.Any(
                    p =>
                        p.Name != null &&
                        p.Name.Equals(defaultParameter.Name, StringComparison.InvariantCultureIgnoreCase) &&
                        p.Type == defaultParameter.Type
                );

            if (Options.AllowMultipleDefaultParametersWithSameName) {
                var isMultiParameter = MultiParameterTypes.Any(pt => pt == defaultParameter.Type);
                parameterExists = !isMultiParameter && parameterExists;
            }

            if (!parameterExists) requestParameters.Add(defaultParameter);
        }

        // Add Accept header based on registered deserializers if none has been set by the caller.
        if (requestParameters.All(p => !p.Name!.EqualsIgnoreCase("accept"))) {
            var accepts = Join(", ", _acceptTypes);
            requestParameters.Add(new Parameter("Accept", accepts, ParameterType.HttpHeader));
        }

        #endregion

        // http.Url = BuildUri(request);
        // http.Host                                 = BaseHost;
        // http.PreAuthenticate                      = PreAuthenticate;
        // http.UnsafeAuthenticatedConnectionSharing = UnsafeAuthenticatedConnectionSharing;
        // http.UserAgent                            = UserAgent ?? http.UserAgent;

        // var timeout = request.Timeout != 0
        //     ? request.Timeout
        //     : Timeout;
        //
        // if (timeout != 0) http.Timeout = timeout;

        // var readWriteTimeout = request.ReadWriteTimeout != 0
        //     ? request.ReadWriteTimeout
        //     : ReadWriteTimeout;
        //
        // if (readWriteTimeout != 0) http.ReadWriteTimeout = readWriteTimeout;

        // http.FollowRedirects = FollowRedirects;

        // if (ClientCertificates != null) http.ClientCertificates = ClientCertificates;

        // http.MaxRedirects = MaxRedirects;
        // http.CachePolicy  = CachePolicy;
        // http.Pipelined    = Pipelined;

        // if (request.Credentials != null) http.Credentials = request.Credentials;

        http.Headers = requestParameters
            .Where(p => p.Type == ParameterType.HttpHeader)
            .Select(p => new HttpHeader(p.Name!, p.Value))
            .ToList();

        http.Cookies = requestParameters
            .Where(p => p.Type == ParameterType.Cookie)
            .Select(p => new HttpCookie { Name = p.Name!, Value = p.Value?.ToString() ?? "" })
            .ToList();

        http.Parameters = requestParameters
            .Where(p => p.Type == ParameterType.GetOrPost)
            .Select(p => new HttpParameter(p.Name!, p.Value))
            .ToList();

        // http.Files = request.Files.Select(
        //         file => new HttpFile {
        //             Name          = file.Name,
        //             ContentType   = file.ContentType,
        //             Writer        = file.GetFile,
        //             FileName      = file.FileName,
        //             ContentLength = file.ContentLength
        //         }
        //     )
        //     .ToList();
        //
        // if (request.Body != null) http.AddBody(request.Body);

        // http.AllowedDecompressionMethods = request.AllowedDecompressionMethods;

        // var proxy = Proxy ?? WebRequest.DefaultWebProxy;
        //
        // try {
        //     proxy ??= WebRequest.GetSystemWebProxy();
        // }
        // catch (PlatformNotSupportedException) {
        //     // Ignore platform unsupported proxy detection
        // }
        //
        // http.Proxy = proxy;

        // http.RemoteCertificateValidationCallback = RemoteCertificateValidationCallback;

        return http;
    }

    RestResponse<T> Deserialize<T>(IRestRequest request, RestResponse raw) {
        var response = RestResponse<T>.FromResponse(raw);

        try {
            request.OnBeforeDeserialization?.Invoke(raw);

            // Only attempt to deserialize if the request has not errored due
            // to a transport or framework exception.  HTTP errors should attempt to
            // be deserialized
            if (response.ErrorException == null) {
                var serializer = Serializers.FirstOrDefault(x => x.Value.SupportedContentTypes.Contains(raw.ContentType));
                var handler    = serializer.Value;

                // Only continue if there is a handler defined else there is no way to deserialize the data.
                // This can happen when a request returns for example a 404 page instead of the requested JSON/XML resource
                if (handler is IXmlDeserializer xml) {
                    if (request.DateFormat.IsNotEmpty()) xml.DateFormat = request.DateFormat;
                    if (request.XmlNamespace.IsNotEmpty()) xml.Namespace = request.XmlNamespace;
                }

                if (handler is IWithRootElement deserializer && !request.RootElement.IsEmpty()) deserializer.RootElement = request.RootElement;

                if (handler != null) response.Data = handler.Deserialize<T>(raw);
            }
        }
        catch (Exception ex) {
            if (Options.ThrowOnAnyError) throw;

            if (Options.FailOnDeserializationError || Options.ThrowOnDeserializationError) response.ResponseStatus = ResponseStatus.Error;

            response.ErrorMessage   = ex.Message;
            response.ErrorException = ex;

            if (Options.ThrowOnDeserializationError) throw new DeserializationException(response, ex);
        }

        response.Request = request;

        return response;
    }

    static bool IsWildcardStructuredSuffixSyntax(string contentType) {
        var i = 0;

        // Avoid most unnecessary uses of RegEx by checking for necessary characters explicitly first
        if (contentType[i++] != '*') return false;

        if (contentType[i++] != '+') return false;

        // If no more characters to check, exit now
        if (i == contentType.Length) return false;

        // At this point it is probably using a wildcard structured syntax suffix, but let's confirm.
        return StructuredSyntaxSuffixWildcardRegex.IsMatch(contentType);
    }

    class UrlSegmentParamsValues {
        public UrlSegmentParamsValues(Uri builderUri, string assembled) {
            Uri      = builderUri;
            Resource = assembled;
        }

        public Uri    Uri      { get; }
        public string Resource { get; }
    }
}