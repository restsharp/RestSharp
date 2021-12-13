using System.Text.RegularExpressions;
using RestSharp.Extensions;

namespace RestSharp;

/// <summary>
/// Container for data used to make requests
/// </summary>
public class RestRequest {
    readonly Func<HttpResponseMessage, RestResponse>? _advancedResponseHandler;
    readonly Func<Stream, Stream?>?                   _responseWriter;

    /// <summary>
    /// Default constructor
    /// </summary>
    public RestRequest() {
        RequestFormat = DataFormat.Json;
        Method        = Method.Get;
    }

    /// <summary>
    /// Sets Method property to value of method
    /// </summary>
    /// <param name="method">Method to use for this request</param>
    public RestRequest(Method method) : this() => Method = method;

    public RestRequest(string resource, Method method) : this(resource, method, DataFormat.Json) { }

    public RestRequest(string resource, DataFormat dataFormat) : this(resource, Method.Get, dataFormat) { }

    public RestRequest(string? resource, Method method = Method.Get, DataFormat dataFormat = DataFormat.Json) : this() {
        Resource      = resource ?? "";
        Method        = method;
        RequestFormat = dataFormat;

        if (string.IsNullOrWhiteSpace(resource)) return;

        var queryStringStart = Resource.IndexOf('?');

        if (queryStringStart >= 0 && Resource.IndexOf('=') > queryStringStart) {
            var queryParams = ParseQuery(Resource.Substring(queryStringStart + 1));
            Resource = Resource.Substring(0, queryStringStart);

            foreach (var param in queryParams)
                this.AddQueryParameter(param.Key, param.Value, false);
        }

        static IEnumerable<KeyValuePair<string, string>> ParseQuery(string query)
            => query.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(
                    x => {
                        var position = x.IndexOf('=');

                        return position > 0
                            ? new KeyValuePair<string, string>(x.Substring(0, position), x.Substring(position + 1))
                            : new KeyValuePair<string, string>(x, string.Empty);
                    }
                );
    }

    public RestRequest(Uri resource, Method method = Method.Get, DataFormat dataFormat = DataFormat.Json)
        : this(
            resource.IsAbsoluteUri
                ? resource.AbsoluteUri
                : resource.OriginalString,
            method,
            dataFormat
        ) { }

    readonly List<Parameter>     _parameters = new();
    readonly List<FileParameter> _files      = new();

    /// <summary>
    /// Always send a multipart/form-data request - even when no Files are present.
    /// </summary>
    public bool AlwaysMultipartFormData { get; set; }

    /// <summary>
    /// Container of all HTTP parameters to be passed with the request.
    /// See AddParameter() for explanation of the types of parameters that can be passed
    /// </summary>
    public IReadOnlyCollection<Parameter> Parameters => _parameters.AsReadOnly();

    /// <summary>
    /// Container of all the files to be uploaded with the request.
    /// </summary>
    public IReadOnlyCollection<FileParameter> Files => _files.AsReadOnly();

    /// <summary>
    /// Determines what HTTP method to use for this request. Supported methods: GET, POST, PUT, DELETE, HEAD, OPTIONS
    /// Default is GET
    /// </summary>
    public Method Method { get; set; }

    /// <summary>
    /// Custom request timeout
    /// </summary>
    public int Timeout { get; set; }

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
    /// request.AddParameter("ProductId", 123, ParameterType.UrlSegment);
    /// </example>
    public string Resource { get; set; } = "";

    /// <summary>
    /// Serializer to use when writing request bodies.
    /// </summary>
    public DataFormat RequestFormat { get; set; }

    /// <summary>
    /// Used by the default deserializers to determine where to start deserializing from.
    /// Can be used to skip container or root elements that do not have corresponding deserialzation targets.
    /// </summary>
    public string? RootElement { get; set; }

    /// <summary>
    /// When supplied, the function will be called before calling the deserializer
    /// </summary>
    public Action<RestResponse>? OnBeforeDeserialization { get; set; }

    /// <summary>
    /// When supplied, the function will be called before making a request
    /// </summary>
    public Action<HttpRequestMessage>? OnBeforeRequest { get; set; }

    /// <summary>
    /// When supplied, the function will be called after the request is complete
    /// </summary>
    public Action<HttpResponseMessage>? OnAfterRequest { get; set; }

    internal void IncreaseNumAttempts() => Attempts++;

    /// <summary>
    /// How many attempts were made to send this Request?
    /// </summary>
    /// <remarks>
    /// This number is incremented each time the RestClient sends the request.
    /// </remarks>
    public int Attempts { get; private set; }

    /// <summary>
    /// Set this to write response to Stream rather than reading into memory.
    /// </summary>
    public Func<Stream, Stream?>? ResponseWriter {
        get => _responseWriter;
        init {
            if (AdvancedResponseWriter != null)
                throw new ArgumentException(
                    "AdvancedResponseWriter is not null. Only one response writer can be used."
                );

            _responseWriter = value;
        }
    }

    /// <summary>
    /// Set this to handle the response stream yourself, based on the response details
    /// </summary>
    public Func<HttpResponseMessage, RestResponse>? AdvancedResponseWriter {
        get => _advancedResponseHandler;
        init {
            if (ResponseWriter != null)
                throw new ArgumentException("ResponseWriter is not null. Only one response writer can be used.");

            _advancedResponseHandler = value;
        }
    }

    public RestRequest AddParameter(Parameter p) {
        if (p.Type == ParameterType.Cookie)
            throw new InvalidOperationException("Cookie parameters should be added to the RestClient's cookie container");

        return this.With(x => x._parameters.Add(p));
    }

    public void RemoveParameter(Parameter p) => _parameters.Remove(p);

    internal RestRequest AddFile(FileParameter file) => this.With(x => x._files.Add(file));
}