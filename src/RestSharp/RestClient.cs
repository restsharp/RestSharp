//  Copyright © 2009-2021 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Net;
using System.Text;
using RestSharp.Authenticators;
using RestSharp.Extensions;
using RestSharp.Serializers;
using RestSharp.Serializers.Json;
using RestSharp.Serializers.Xml;

// ReSharper disable VirtualMemberCallInConstructor
#pragma warning disable 618

namespace RestSharp;

/// <summary>
/// Client to translate RestRequests into Http requests and process response result
/// </summary>
public partial class RestClient {
    readonly CookieContainer _cookieContainer;
    // readonly List<Parameter> _defaultParameters = new();

    HttpClient HttpClient { get; }

    internal RestClientOptions Options { get; }

    /// <summary>
    /// Default constructor that registers default content handlers
    /// </summary>
    public RestClient() : this(new RestClientOptions()) { }

    public RestClient(HttpClient httpClient, RestClientOptions? options = null) {
        UseSerializer<SystemTextJsonSerializer>();
        UseSerializer<XmlRestSerializer>();

        HttpClient       = httpClient;
        Options          = options ?? new RestClientOptions();
        _cookieContainer = Options.CookieContainer ?? new CookieContainer();

        if (Options.Timeout > 0)
            HttpClient.Timeout = TimeSpan.FromMilliseconds(Options.Timeout);
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Options.UserAgent);
    }

    public RestClient(HttpMessageHandler handler) : this(new HttpClient(handler)) { }

    public RestClient(RestClientOptions options) {
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
            Proxy                  = Options.Proxy
        };

        if (Options.RemoteCertificateValidationCallback != null)
            handler.ServerCertificateCustomValidationCallback =
                (request, cert, chain, errors) => Options.RemoteCertificateValidationCallback(request, cert, chain, errors);

        if (Options.ClientCertificates != null)
            handler.ClientCertificates.AddRange(Options.ClientCertificates);

        if (Options.MaxRedirects.HasValue)
            handler.MaxAutomaticRedirections = Options.MaxRedirects.Value;

        var finalHandler = Options.ConfigureMessageHandler?.Invoke(handler) ?? handler;

        HttpClient = new HttpClient(finalHandler);

        if (Options.Timeout > 0)
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

    internal Dictionary<DataFormat, SerializerRecord> Serializers { get; } = new();

    Func<string, string> Encode { get; set; } = s => s.UrlEncode();

    Func<string, Encoding, string> EncodeQuery { get; set; } = (s, encoding) => s.UrlEncode(encoding)!;

    /// <summary>
    /// Allows to use a custom way to encode URL parameters
    /// </summary>
    /// <param name="encoder">A delegate to encode URL parameters</param>
    /// <example>client.UseUrlEncoder(s => HttpUtility.UrlEncode(s));</example>
    /// <returns></returns>
    public RestClient UseUrlEncoder(Func<string, string> encoder) => this.With(x => x.Encode = encoder);

    /// <summary>
    /// Allows to use a custom way to encode query parameters
    /// </summary>
    /// <param name="queryEncoder">A delegate to encode query parameters</param>
    /// <example>client.UseUrlEncoder((s, encoding) => HttpUtility.UrlEncode(s, encoding));</example>
    /// <returns></returns>
    public RestClient UseQueryEncoder(Func<string, Encoding, string> queryEncoder) => this.With(x => x.EncodeQuery = queryEncoder);

    /// <summary>
    /// Authenticator that will be used to populate request with necessary authentication data
    /// </summary>
    public IAuthenticator? Authenticator { get; set; }

    // public IReadOnlyCollection<Parameter> DefaultParameters {
    //     get { lock (_defaultParameters) return _defaultParameters; }
    // }
    public ParametersCollection DefaultParameters { get; } = new();

    /// <summary>
    /// Add a parameter to use on every request made with this client instance
    /// </summary>
    /// <param name="p">Parameter to add</param>
    /// <returns></returns>
    public RestClient AddDefaultParameter(Parameter p) {
        switch (p.Type) {
            case ParameterType.RequestBody:
                throw new NotSupportedException(
                    "Cannot set request body from default headers. Use Request.AddBody() instead."
                );
            case ParameterType.Cookie: {
                lock (_cookieContainer) {
                    _cookieContainer.Add(new Cookie(p.Name!, p.Value!.ToString()));
                }

                break;
            }
            default: {
                DefaultParameters.AddParameter(p);
                break;
            }
        }

        return this;
    }

    [PublicAPI]
    public RestResponse<T> Deserialize<T>(RestResponse response) => Deserialize<T>(response.Request!, response);

    public Uri BuildUri(RestRequest request) {
        DoBuildUriValidations(request);

        var (uri, resource) = Options.BaseUrl.GetUrlSegmentParamsValues(request.Resource, Encode, request.Parameters, DefaultParameters);
        var mergedUri = uri.MergeBaseUrlAndResource(resource);
        var finalUri = mergedUri.ApplyQueryStringParamsValuesToUri(
            request.Method,
            Options.Encoding,
            EncodeQuery,
            request.Parameters,
            DefaultParameters
        );
        return finalUri;
    }

    internal Uri BuildUriWithoutQueryParameters(RestRequest request) {
        DoBuildUriValidations(request);
        var (uri, resource) = Options.BaseUrl.GetUrlSegmentParamsValues(request.Resource, Encode, request.Parameters, DefaultParameters);
        return uri.MergeBaseUrlAndResource(resource);
    }

    public string[] AcceptedContentTypes { get; private set; } = null!;

    internal void AssignAcceptedContentTypes()
        => AcceptedContentTypes = Serializers.SelectMany(x => x.Value.SupportedContentTypes).Distinct().ToArray();

    /// <summary>
    /// Replace the default serializer with a custom one
    /// </summary>
    /// <param name="serializerFactory">Function that returns the serializer instance</param>
    public RestClient UseSerializer(Func<IRestSerializer> serializerFactory) {
        var instance = serializerFactory();
        Serializers[instance.DataFormat] = new SerializerRecord(instance.DataFormat, instance.SupportedContentTypes, serializerFactory);
        AssignAcceptedContentTypes();
        return this;
    }

    /// <summary>
    /// Replace the default serializer with a custom one
    /// </summary>
    /// <typeparam name="T">The type that implements <see cref="IRestSerializer"/></typeparam>
    /// <returns></returns>
    public RestClient UseSerializer<T>() where T : class, IRestSerializer, new() => UseSerializer(() => new T());

    void DoBuildUriValidations(RestRequest request) {
        if (Options.BaseUrl == null && !request.Resource.ToLowerInvariant().StartsWith("http"))
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "Request resource doesn't contain a valid scheme for an empty client base URL"
            );

        var nullValuedParams = request.Parameters
            .GetParameters(ParameterType.UrlSegment)
            .Where(p => p.Value == null)
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

    internal RestResponse<T> Deserialize<T>(RestRequest request, RestResponse raw) {
        var response = RestResponse<T>.FromResponse(raw);

        try {
            request.OnBeforeDeserialization?.Invoke(raw);

            // Only attempt to deserialize if the request has not errored due
            // to a transport or framework exception.  HTTP errors should attempt to
            // be deserialized
            if (response.ErrorException == null) {
                var handler = GetContentDeserializer(raw, request.RequestFormat);

                // Only continue if there is a handler defined else there is no way to deserialize the data.
                // This can happen when a request returns for example a 404 page instead of the requested JSON/XML resource
                if (handler is IXmlDeserializer xml && request is RestXmlRequest xmlRequest) {
                    if (xmlRequest.XmlNamespace.IsNotEmpty()) xml.Namespace = xmlRequest.XmlNamespace!;

                    if (xml is IWithDateFormat withDateFormat && xmlRequest.DateFormat.IsNotEmpty())
                        withDateFormat.DateFormat = xmlRequest.DateFormat!;
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

    IDeserializer? GetContentDeserializer(RestResponseBase response, DataFormat requestFormat) {
        var contentType = response.ContentType != null && AcceptedContentTypes.Contains(response.ContentType)
            ? response.ContentType
            : DetectContentType();
        if (contentType.IsEmpty()) return null;

        var serializer = Serializers.FirstOrDefault(x => x.Value.SupportedContentTypes.Contains(contentType));
        var factory    = serializer.Value ?? (Serializers.ContainsKey(requestFormat) ? Serializers[requestFormat] : null);
        return factory?.GetSerializer().Deserializer;

        string? DetectContentType()
            => response.Content!.StartsWith("<") ? ContentType.Xml : response.Content.StartsWith("{") ? ContentType.Json : null;
    }
}