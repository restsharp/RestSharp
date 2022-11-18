//  Copyright (c) .NET Foundation and Contributors
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
using System.Net.Http.Headers;
using System.Text;
using RestSharp.Authenticators;
using RestSharp.Extensions;

// ReSharper disable VirtualMemberCallInConstructor
#pragma warning disable 618

namespace RestSharp;

/// <summary>
/// Client to translate RestRequests into Http requests and process response result
/// </summary>
public partial class RestClient : IDisposable {
    /// <summary>
    /// Content types that will be sent in the Accept header. The list is populated from the known serializers.
    /// If you need to send something else by default, set this property to a different value.
    /// </summary>
    public string[] AcceptedContentTypes { get; set; } = null!;

    /// <summary>
    /// Function to calculate the response status. By default, the status will be Completed if it was successful, or NotFound.
    /// </summary>
    public CalculateResponseStatus CalculateResponseStatus { get; set; } = httpResponse
        => httpResponse.IsSuccessStatusCode || httpResponse.StatusCode == HttpStatusCode.NotFound
            ? ResponseStatus.Completed
            : ResponseStatus.Error;

    HttpClient HttpClient { get; }

    public RestClientOptions Options { get; }

    public RestClient(RestClientOptions options, Action<HttpRequestHeaders>? configureDefaultHeaders = null) {
        UseDefaultSerializers();

        Options            = options;
        _disposeHttpClient = true;

        var handler = new HttpClientHandler();
        ConfigureHttpMessageHandler(handler);

        var finalHandler = Options.ConfigureMessageHandler?.Invoke(handler) ?? handler;

        HttpClient = new HttpClient(finalHandler);
        ConfigureHttpClient(HttpClient);
        configureDefaultHeaders?.Invoke(HttpClient.DefaultRequestHeaders);
    }

    /// <summary>
    /// Creates an instance of RestClient using the default <see cref="RestClientOptions"/>
    /// </summary>
    public RestClient() : this(new RestClientOptions()) { }

    /// <inheritdoc />
    /// <summary>
    /// Creates an instance of RestClient using a specific BaseUrl for requests made by this client instance
    /// </summary>
    /// <param name="baseUrl">Base URI for the new client</param>
    public RestClient(Uri baseUrl) : this(new RestClientOptions { BaseUrl = baseUrl }) { }

    /// <inheritdoc />
    /// <summary>
    /// Creates an instance of RestClient using a specific BaseUrl for requests made by this client instance
    /// </summary>
    /// <param name="baseUrl">Base URI for this new client as a string</param>
    public RestClient(string baseUrl) : this(new Uri(Ensure.NotEmptyString(baseUrl, nameof(baseUrl)))) { }

    /// <summary>
    /// Creates an instance of RestClient using a shared HttpClient and does not allocate one internally.
    /// </summary>
    /// <param name="httpClient">HttpClient to use</param>
    /// <param name="disposeHttpClient">True to dispose of the client, false to assume the caller does (defaults to false)</param>
    public RestClient(HttpClient httpClient, bool disposeHttpClient = false) {
        UseDefaultSerializers();

        HttpClient         = httpClient;
        Options            = new RestClientOptions();
        _disposeHttpClient = disposeHttpClient;

        if (httpClient.BaseAddress != null) {
            Options.BaseUrl = httpClient.BaseAddress;
        }
    }

    /// <summary>
    /// Creates an instance of RestClient using a shared HttpClient and specific RestClientOptions and does not allocate one internally.
    /// </summary>
    /// <param name="httpClient">HttpClient to use</param>
    /// <param name="options">RestClient options to use</param>
    /// <param name="disposeHttpClient">True to dispose of the client, false to assume the caller does (defaults to false)</param>
    public RestClient(HttpClient httpClient, RestClientOptions options, bool disposeHttpClient = false) {
        UseDefaultSerializers();

        HttpClient         = httpClient;
        Options            = options;
        _disposeHttpClient = disposeHttpClient;

        if (httpClient.BaseAddress != null && options.BaseUrl == null) {
            Options.BaseUrl = httpClient.BaseAddress;
        }

        ConfigureHttpClient(HttpClient);
    }

    /// <summary>
    /// Creates a new instance of RestSharp using the message handler provided. By default, HttpClient disposes the provided handler
    /// when the client itself is disposed. If you want to keep the handler not disposed, set disposeHandler argument to false.
    /// </summary>
    /// <param name="handler">Message handler instance to use for HttpClient</param>
    /// <param name="disposeHandler">Dispose the handler when disposing RestClient, true by default</param>
    public RestClient(HttpMessageHandler handler, bool disposeHandler = true) : this(new HttpClient(handler, disposeHandler), true) { }

    void ConfigureHttpClient(HttpClient httpClient) {
        if (Options.MaxTimeout > 0) httpClient.Timeout = TimeSpan.FromMilliseconds(Options.MaxTimeout);

        if (Options.UserAgent != null && httpClient.DefaultRequestHeaders.UserAgent.All(x => x.Product?.Name != Options.UserAgent)) {
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Options.UserAgent);
        }

        if (Options.Expect100Continue != null) httpClient.DefaultRequestHeaders.ExpectContinue = Options.Expect100Continue;
    }

    void ConfigureHttpMessageHandler(HttpClientHandler handler) {
        handler.UseCookies             = false;
        handler.Credentials            = Options.Credentials;
        handler.UseDefaultCredentials  = Options.UseDefaultCredentials;
        handler.AutomaticDecompression = Options.AutomaticDecompression;
        handler.PreAuthenticate        = Options.PreAuthenticate;
        handler.AllowAutoRedirect      = Options.FollowRedirects;

        if (handler.SupportsProxy) handler.Proxy = Options.Proxy;

        if (Options.RemoteCertificateValidationCallback != null)
            handler.ServerCertificateCustomValidationCallback =
                (request, cert, chain, errors) => Options.RemoteCertificateValidationCallback(request, cert, chain, errors);

        if (Options.ClientCertificates != null) {
            handler.ClientCertificates.AddRange(Options.ClientCertificates);
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        }

        if (Options.MaxRedirects.HasValue) handler.MaxAutomaticRedirections = Options.MaxRedirects.Value;
    }

    internal Func<string, string> Encode { get; set; } = s => s.UrlEncode();

    internal Func<string, Encoding, string> EncodeQuery { get; set; } = (s, encoding) => s.UrlEncode(encoding)!;

    /// <summary>
    /// Authenticator that will be used to populate request with necessary authentication data
    /// </summary>
    public IAuthenticator? Authenticator { get; set; }

    public ParametersCollection DefaultParameters { get; } = new();

    /// <summary>
    /// Add a parameter to use on every request made with this client instance
    /// </summary>
    /// <param name="parameter">Parameter to add</param>
    /// <returns></returns>
    public RestClient AddDefaultParameter(Parameter parameter) {
        if (parameter.Type == ParameterType.RequestBody)
            throw new NotSupportedException(
                "Cannot set request body using default parameters. Use Request.AddBody() instead."
            );

        if (!Options.AllowMultipleDefaultParametersWithSameName &&
            !MultiParameterTypes.Contains(parameter.Type)       &&
            DefaultParameters.Any(x => x.Name == parameter.Name)) {
            throw new ArgumentException("A default parameters with the same name has already been added", nameof(parameter));
        }

        DefaultParameters.AddParameter(parameter);

        return this;
    }

    static readonly ParameterType[] MultiParameterTypes = { ParameterType.QueryString, ParameterType.GetOrPost };

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

    internal void AssignAcceptedContentTypes()
        => AcceptedContentTypes = Serializers.SelectMany(x => x.Value.AcceptedContentTypes).Distinct().ToArray();

    void DoBuildUriValidations(RestRequest request) {
        if (Options.BaseUrl == null && !request.Resource.ToLowerInvariant().StartsWith("http"))
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "Request resource doesn't contain a valid scheme for an empty base URL of the client"
            );
    }

    readonly bool _disposeHttpClient;
    bool          _disposed;

    protected virtual void Dispose(bool disposing) {
        if (disposing && !_disposed) {
            _disposed = true;
            if (_disposeHttpClient) HttpClient.Dispose();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
