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

// ReSharper disable VirtualMemberCallInConstructor
#pragma warning disable 618

namespace RestSharp;

/// <summary>
/// Client to translate RestRequests into Http requests and process response result
/// </summary>
public partial class RestClient : IDisposable {
    public CookieContainer CookieContainer { get; }

    /// <summary>
    /// Content types that will be sent in the Accept header. The list is populated from the known serializers.
    /// If you need to send something else by default, set this property to a different value.
    /// </summary>
    public string[] AcceptedContentTypes { get; set; } = null!;

    HttpClient HttpClient { get; }

    internal RestClientOptions Options { get; }

    public RestClient(RestClientOptions options) {
        UseDefaultSerializers();

        Options            = options;
        CookieContainer    = Options.CookieContainer ?? new CookieContainer();
        _disposeHttpClient = true;

        var handler = new HttpClientHandler();
        ConfigureHttpMessageHandler(handler);

        var finalHandler = Options.ConfigureMessageHandler?.Invoke(handler) ?? handler;

        HttpClient = new HttpClient(finalHandler);

        ConfigureHttpClient(HttpClient);
    }

    /// <summary>
    /// Creates an instance of RestClient using the default <see cref="RestClientOptions"/>
    /// </summary>
    public RestClient() : this(new RestClientOptions()) { }

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

    public RestClient(HttpClient httpClient, RestClientOptions? options = null, bool disposeHttpClient = false) {
        if (Options?.CookieContainer != null) {
            throw new ArgumentException("Custom cookie container cannot be added to the HttpClient instance", nameof(options.CookieContainer));
        }

        UseDefaultSerializers();

        HttpClient         = httpClient;
        Options            = options ?? new RestClientOptions();
        CookieContainer    = new CookieContainer();
        _disposeHttpClient = disposeHttpClient;

        ConfigureHttpClient(HttpClient);
    }

    /// <summary>
    /// Creates a new instance of RestSharp using the message handler provided. By default, HttpClient disposes the provided handler
    /// when the client itself is disposed. If you want to keep the handler not disposed, set disposeHandler argument to false.
    /// </summary>
    /// <param name="handler">Message handler instance to use for HttpClient</param>
    /// <param name="disposeHandler">Dispose the handler when disposing RestClient, true by default</param>
    public RestClient(HttpMessageHandler handler, bool disposeHandler = true) : this(new HttpClient(handler, disposeHandler)) { }

    void ConfigureHttpClient(HttpClient httpClient) {
        if (Options.Timeout > 0)
            httpClient.Timeout = TimeSpan.FromMilliseconds(Options.Timeout);
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Options.UserAgent);
    }

    void ConfigureHttpMessageHandler(HttpClientHandler handler) {
        handler.Credentials            = Options.Credentials;
        handler.UseDefaultCredentials  = Options.UseDefaultCredentials;
        handler.CookieContainer        = CookieContainer;
        handler.AutomaticDecompression = Options.AutomaticDecompression;
        handler.PreAuthenticate        = Options.PreAuthenticate;
        handler.AllowAutoRedirect      = Options.FollowRedirects;
        handler.Proxy                  = Options.Proxy;

        if (Options.RemoteCertificateValidationCallback != null)
            handler.ServerCertificateCustomValidationCallback =
                (request, cert, chain, errors) => Options.RemoteCertificateValidationCallback(request, cert, chain, errors);

        if (Options.ClientCertificates != null) {
            handler.ClientCertificates.AddRange(Options.ClientCertificates);
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        }

        if (Options.MaxRedirects.HasValue)
            handler.MaxAutomaticRedirections = Options.MaxRedirects.Value;
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
            !MultiParameterTypes.Contains(parameter.Type) &&
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
        => AcceptedContentTypes = Serializers.SelectMany(x => x.Value.SupportedContentTypes).Distinct().ToArray();

    void DoBuildUriValidations(RestRequest request) {
        if (Options.BaseUrl == null && !request.Resource.ToLowerInvariant().StartsWith("http"))
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "Request resource doesn't contain a valid scheme for an empty base URL of the client"
            );
    }

    public void Dispose() {
        if (_disposeHttpClient) HttpClient.Dispose();
    }

    readonly bool _disposeHttpClient;
}