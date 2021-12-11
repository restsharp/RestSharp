using System.Text.RegularExpressions;
using RestSharp.Extensions;

namespace RestSharp;

/// <summary>
/// Container for data used to make requests
/// </summary>
public class RestRequest : IRestRequest {
    static readonly Regex PortSplitRegex = new(@":\d+");

    Action<Stream, HttpResponse>? _advancedResponseWriter;
    Action<Stream>?               _responseWriter;

    /// <summary>
    /// Default constructor
    /// </summary>
    public RestRequest() {
        RequestFormat = DataFormat.Json;
        Method        = Method.Get;
        Parameters    = new List<Parameter>();
        Files         = new List<FileParameter>();
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
                AddQueryParameter(param.Key, param.Value, false);
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

    /// <inheritdoc />
    public bool AlwaysMultipartFormData { get; set; }

    /// <inheritdoc />
    public RequestBody? Body { get; set; }

    /// <inheritdoc />
    public Action<Stream> ResponseWriter {
        get => _responseWriter;
        set {
            if (AdvancedResponseWriter != null)
                throw new ArgumentException(
                    "AdvancedResponseWriter is not null. Only one response writer can be used."
                );

            _responseWriter = value;
        }
    }

    /// <inheritdoc />
    public Action<Stream, HttpResponse> AdvancedResponseWriter {
        get => _advancedResponseWriter;
        set {
            if (ResponseWriter != null)
                throw new ArgumentException("ResponseWriter is not null. Only one response writer can be used.");

            _advancedResponseWriter = value;
        }
    }

    /// <inheritdoc />
    public IRestRequest AddFile(string name, string path, string? contentType = null) => AddFile(FileParameter.FromFile(path, name, contentType));

    /// <inheritdoc />
    public IRestRequest AddFile(string name, byte[] bytes, string fileName, string? contentType = null)
        => AddFile(FileParameter.Create(name, bytes, fileName, contentType));

    /// <inheritdoc />
    public IRestRequest AddFile(
        string       name,
        Func<Stream> getFile,
        string       fileName,
        long         contentLength,
        string?      contentType = null
    )
        => AddFile(FileParameter.Create(name, getFile, contentLength, fileName, contentType));

    /// <inheritdoc />
    public IRestRequest AddFileBytes(
        string name,
        byte[] bytes,
        string filename,
        string contentType = "application/x-gzip"
    )
        => AddFile(FileParameter.Create(name, bytes, filename, contentType));

    /// <inheritdoc />
    public IRestRequest AddBody(object obj, string xmlNamespace)
        => RequestFormat switch {
            DataFormat.Json => AddJsonBody(obj),
            DataFormat.Xml  => AddXmlBody(obj, xmlNamespace),
            _               => this
        };

    /// <inheritdoc />
    public IRestRequest AddBody(object obj)
        => RequestFormat switch {
            DataFormat.Json => AddJsonBody(obj),
            DataFormat.Xml  => AddXmlBody(obj),
            _               => AddParameter("", obj.ToString())
        };

    /// <inheritdoc />
    public IRestRequest AddJsonBody(object obj) {
        RequestFormat = DataFormat.Json;

        return AddParameter(new JsonParameter("", obj));
    }

    /// <inheritdoc />
    public IRestRequest AddJsonBody(object obj, string contentType) {
        RequestFormat = DataFormat.Json;

        return AddParameter(new JsonParameter(contentType, obj, contentType));
    }

    /// <inheritdoc />
    public IRestRequest AddXmlBody(object obj) => AddXmlBody(obj, "");

    /// <inheritdoc />
    public IRestRequest AddXmlBody(object obj, string xmlNamespace) {
        RequestFormat = DataFormat.Xml;
        AddParameter(new XmlParameter("", obj, xmlNamespace));
        return this;
    }

    /// <inheritdoc />
    public IRestRequest AddObject(object obj, params string[] includedProperties) {
        // automatically create parameters from object props
        var type  = obj.GetType();
        var props = type.GetProperties();

        foreach (var prop in props) {
            if (!IsAllowedProperty(prop.Name))
                continue;

            var val = prop.GetValue(obj, null);

            if (val == null)
                continue;

            var propType = prop.PropertyType;

            if (propType.IsArray) {
                var elementType = propType.GetElementType();
                var array       = (Array)val;

                if (array.Length > 0 && elementType != null) {
                    // convert the array to an array of strings
                    var values = array.Cast<object>().Select(item => item.ToString());

                    val = string.Join(",", values);
                }
            }

            AddParameter(prop.Name, val);
        }

        return this;

        bool IsAllowedProperty(string propertyName)
            => includedProperties.Length == 0 || includedProperties.Length > 0 && includedProperties.Contains(propertyName);
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
    public IRestRequest AddOrUpdateParameter(Parameter parameter) {
        var p = Parameters
            .FirstOrDefault(x => x.Name == parameter.Name && x.Type == parameter.Type);

        if (p != null) Parameters.Remove(p);

        Parameters.Add(parameter);
        return this;
    }

    /// <inheritdoc />
    public IRestRequest AddOrUpdateParameters(IEnumerable<Parameter> parameters) {
        foreach (var parameter in parameters)
            AddOrUpdateParameter(parameter);

        return this;
    }

    /// <inheritdoc />
    public IRestRequest AddOrUpdateParameter(string name, object value) => AddOrUpdateParameter(new Parameter(name, value, ParameterType.GetOrPost));

    /// <inheritdoc />
    public IRestRequest AddOrUpdateParameter(string name, object value, ParameterType type) => AddOrUpdateParameter(new Parameter(name, value, type));

    /// <inheritdoc />
    public IRestRequest AddOrUpdateParameter(string name, object value, string contentType, ParameterType type)
        => AddOrUpdateParameter(new Parameter(name, value, contentType, type));

    /// <inheritdoc />
    public IRestRequest AddHeader(string name, string value) {
        CheckAndThrowsForInvalidHost(name, value);
        return AddParameter(name, value, ParameterType.HttpHeader);
    }

    /// <inheritdoc />
    public IRestRequest AddOrUpdateHeader(string name, string value) {
        CheckAndThrowsForInvalidHost(name, value);
        return AddOrUpdateParameter(name, value, ParameterType.HttpHeader);
    }

    /// <inheritdoc />
    public IRestRequest AddHeaders(ICollection<KeyValuePair<string, string>> headers) {
        CheckAndThrowsDuplicateKeys(headers);

        foreach (var pair in headers) {
            AddHeader(pair.Key, pair.Value);
        }

        return this;
    }

    /// <inheritdoc />
    public IRestRequest AddOrUpdateHeaders(ICollection<KeyValuePair<string, string>> headers) {
        CheckAndThrowsDuplicateKeys(headers);

        foreach (var pair in headers) {
            AddOrUpdateHeader(pair.Key, pair.Value);
        }

        return this;
    }

    /// <inheritdoc />
    public IRestRequest AddCookie(string name, string value) => AddParameter(name, value, ParameterType.Cookie);

    /// <inheritdoc />
    public IRestRequest AddUrlSegment(string name, string value) => AddParameter(name, value, ParameterType.UrlSegment);

    /// <inheritdoc />
    public IRestRequest AddUrlSegment(string name, string value, bool encode) {
        var parameter = new Parameter(name, value, ParameterType.UrlSegment, encode);
        return AddParameter(parameter);
    }

    /// <inheritdoc />
    public IRestRequest AddQueryParameter(string name, string value) => AddParameter(name, value, ParameterType.QueryString);

    /// <inheritdoc />
    public IRestRequest AddQueryParameter(string name, string value, bool encode) {
        var parameter = new Parameter(name, value, ParameterType.QueryString, encode);
        return AddParameter(parameter);
    }

    /// <inheritdoc />
    public List<Parameter> Parameters { get; }

    /// <inheritdoc />
    public List<FileParameter> Files { get; }

    /// <inheritdoc />
    public Method Method { get; set; }
    
    public int Timeout { get; set; }

    /// <inheritdoc />
    public string Resource { get; set; } = "";

    /// <inheritdoc />
    public DataFormat RequestFormat { get; set; }

    /// <inheritdoc />
    public string? RootElement { get; set; }

    /// <inheritdoc />
    public Action<RestResponse>? OnBeforeDeserialization { get; set; }

    /// <inheritdoc />
    public Action<Http>? OnBeforeRequest { get; set; }

    /// <inheritdoc />
    public string? DateFormat { get; set; }

    /// <inheritdoc />
    public string? XmlNamespace { get; set; }

    /// <inheritdoc />
    public void IncreaseNumAttempts() => Attempts++;

    /// <inheritdoc />
    public int Attempts { get; private set; }

    /// <inheritdoc />
    public IRestRequest AddUrlSegment(string name, object value) => AddParameter(name, value, ParameterType.UrlSegment);

    IRestRequest AddFile(FileParameter file) => this.With(x => x.Files.Add(file));

    static void CheckAndThrowsForInvalidHost(string name, string value) {
        static bool InvalidHost(string host) => Uri.CheckHostName(PortSplitRegex.Split(host)[0]) == UriHostNameType.Unknown;

        if (name == "Host" && InvalidHost(value))
            throw new ArgumentException("The specified value is not a valid Host header string.", nameof(value));
    }

    static void CheckAndThrowsDuplicateKeys(ICollection<KeyValuePair<string, string>> headers) {
        var duplicateKeys = headers
            .GroupBy(pair => pair.Key.ToUpperInvariant())
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateKeys.Any())
            throw new ArgumentException($"Duplicate header names exist: {string.Join(", ", duplicateKeys)}");
    }
}