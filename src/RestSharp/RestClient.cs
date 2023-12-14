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

using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using RestSharp.Authenticators;
using RestSharp.Serializers;

// ReSharper disable VirtualMemberCallInConstructor
#pragma warning disable 618

namespace RestSharp;

public delegate void ConfigureHeaders(HttpRequestHeaders headers);

public delegate void ConfigureSerialization(SerializerConfig config);

public delegate void ConfigureRestClient(RestClientOptions options);

/// <summary>
/// Client to translate RestRequests into Http requests and process response result
/// </summary>
public partial class RestClient : IRestClient {
    /// <summary>
    /// Content types that will be sent in the Accept header. The list is populated from the known serializers.
    /// If you need to send something else by default, set this property to a different value.
    /// </summary>
    public string[] AcceptedContentTypes { get; set; }

    internal HttpClient HttpClient { get; }

    /// <inheritdoc />>
    public ReadOnlyRestClientOptions Options { get; }

    /// <inheritdoc />>
    public RestSerializers Serializers { get; private set; }

    /// <inheritdoc/>
    public DefaultParameters DefaultParameters { get; }

    [Obsolete("Use RestClientOptions.Authenticator instead")]
    public IAuthenticator? Authenticator => Options.Authenticator;

    // set => Options.Authenticator = value;

    /// <summary>
    /// Creates an instance of RestClient using the provided <see cref="RestClientOptions"/>
    /// </summary>
    /// <param name="options">Client options</param>
    /// <param name="configureDefaultHeaders">Delegate to add default headers to the wrapped HttpClient instance</param>
    /// <param name="configureSerialization">Delegate to configure serialization</param>
    /// <param name="useClientFactory">Set to true if you wish to reuse the <seealso cref="HttpClient"/> instance</param>
    public RestClient(
        RestClientOptions       options,
        ConfigureHeaders?       configureDefaultHeaders = null,
        ConfigureSerialization? configureSerialization  = null,
        bool                    useClientFactory        = false
    ) {
        if (useClientFactory && options.BaseUrl == null) {
            throw new ArgumentException("BaseUrl must be set when using a client factory");
        }

        ConfigureSerializers(configureSerialization);
        Options = new ReadOnlyRestClientOptions(options);
        DefaultParameters = new DefaultParameters(Options);

        if (useClientFactory) {
            _disposeHttpClient = false;
            HttpClient         = SimpleClientFactory.GetClient(options.BaseUrl!, GetClient);
        }
        else {
            _disposeHttpClient = true;
            HttpClient         = GetClient();
        }

        HttpClient GetClient() {
            var handler = new HttpClientHandler();
            ConfigureHttpMessageHandler(handler, Options);
            var finalHandler = options.ConfigureMessageHandler?.Invoke(handler) ?? handler;

            var httpClient = new HttpClient(finalHandler);
            ConfigureHttpClient(httpClient, options);
            ConfigureDefaultParameters(options);
            configureDefaultHeaders?.Invoke(httpClient.DefaultRequestHeaders);
            return httpClient;
        }
    }

    static RestClientOptions ConfigureOptions(RestClientOptions options, ConfigureRestClient? configureRestClient) {
        configureRestClient?.Invoke(options);
        return options;
    }

    /// <summary>
    /// Creates an instance of RestClient using the default <see cref="RestClientOptions"/>
    /// </summary>
    /// <param name="configureRestClient">Delegate to configure the client options</param>
    /// <param name="configureDefaultHeaders">Delegate to add default headers to the wrapped HttpClient instance</param>
    /// <param name="configureSerialization">Delegate to configure serialization</param>
    /// <param name="useClientFactory">Set to true if you wish to reuse the <seealso cref="HttpClient"/> instance</param>
    public RestClient(
        ConfigureRestClient?    configureRestClient     = null,
        ConfigureHeaders?       configureDefaultHeaders = null,
        ConfigureSerialization? configureSerialization  = null,
        bool                    useClientFactory        = false
    )
        : this(ConfigureOptions(new RestClientOptions(), configureRestClient), configureDefaultHeaders, configureSerialization, useClientFactory) { }

    /// <inheritdoc />
    /// <summary>
    /// Creates an instance of RestClient using a specific BaseUrl for requests made by this client instance
    /// </summary>
    /// <param name="baseUrl">Base URI for the new client</param>
    /// <param name="configureRestClient">Delegate to configure the client options</param>
    /// <param name="configureDefaultHeaders">Delegate to add default headers to the wrapped HttpClient instance</param>
    /// <param name="configureSerialization">Delegate to configure serialization</param>
    /// <param name="useClientFactory">Set to true if you wish to reuse the <seealso cref="HttpClient"/> instance</param>
    public RestClient(
        Uri                     baseUrl,
        ConfigureRestClient?    configureRestClient     = null,
        ConfigureHeaders?       configureDefaultHeaders = null,
        ConfigureSerialization? configureSerialization  = null,
        bool                    useClientFactory        = false
    )
        : this(
            ConfigureOptions(new RestClientOptions { BaseUrl = baseUrl }, configureRestClient),
            configureDefaultHeaders,
            configureSerialization,
            useClientFactory
        ) { }

    /// <summary>
    /// Creates an instance of RestClient using a specific BaseUrl for requests made by this client instance
    /// </summary>
    /// <param name="baseUrl">Base URI for this new client as a string</param>
    /// <param name="configureRestClient">Delegate to configure the client options</param>
    /// <param name="configureDefaultHeaders">Delegate to add default headers to the wrapped HttpClient instance</param>
    /// <param name="configureSerialization">Delegate to configure serialization</param>
    public RestClient(
        string                  baseUrl,
        ConfigureRestClient?    configureRestClient     = null,
        ConfigureHeaders?       configureDefaultHeaders = null,
        ConfigureSerialization? configureSerialization  = null
    )
        : this(new Uri(Ensure.NotEmptyString(baseUrl, nameof(baseUrl))), configureRestClient, configureDefaultHeaders, configureSerialization) { }

    /// <summary>
    /// Creates an instance of RestClient using a shared HttpClient and specific RestClientOptions and does not allocate one internally.
    /// </summary>
    /// <param name="httpClient">HttpClient to use</param>
    /// <param name="options">RestClient options to use</param>
    /// <param name="disposeHttpClient">True to dispose of the client, false to assume the caller does (defaults to false)</param>
    /// <param name="configureSerialization">Delegate to configure serialization</param>
    public RestClient(
        HttpClient              httpClient,
        RestClientOptions?      options,
        bool                    disposeHttpClient      = false,
        ConfigureSerialization? configureSerialization = null
    ) {
        ConfigureSerializers(configureSerialization);

        HttpClient         = httpClient;
        _disposeHttpClient = disposeHttpClient;

        if (httpClient.BaseAddress != null && options != null && options.BaseUrl == null) {
            options.BaseUrl = httpClient.BaseAddress;
        }

        var opt = options ?? new RestClientOptions();
        Options           = new ReadOnlyRestClientOptions(opt);
        DefaultParameters = new DefaultParameters(Options);

        if (options != null) {
            ConfigureHttpClient(httpClient, options);
            ConfigureDefaultParameters(options);
        }
    }

    /// <summary>
    /// Creates an instance of RestClient using a shared HttpClient and does not allocate one internally.
    /// </summary>
    /// <param name="httpClient">HttpClient to use</param>
    /// <param name="disposeHttpClient">True to dispose of the client, false to assume the caller does (defaults to false)</param>
    /// <param name="configureRestClient">Delegate to configure the client options</param>
    /// <param name="configureSerialization">Delegate to configure serialization</param>
    public RestClient(
        HttpClient              httpClient,
        bool                    disposeHttpClient      = false,
        ConfigureRestClient?    configureRestClient    = null,
        ConfigureSerialization? configureSerialization = null
    )
        : this(httpClient, ConfigureOptions(new RestClientOptions(), configureRestClient), disposeHttpClient, configureSerialization) { }

    /// <summary>
    /// Creates a new instance of RestSharp using the message handler provided. By default, HttpClient disposes the provided handler
    /// when the client itself is disposed. If you want to keep the handler not disposed, set disposeHandler argument to false.
    /// </summary>
    /// <param name="handler">Message handler instance to use for HttpClient</param>
    /// <param name="disposeHandler">Dispose the handler when disposing RestClient, true by default</param>
    /// <param name="configureRestClient">Delegate to configure the client options</param>
    /// <param name="configureSerialization">Delegate to configure serialization</param>
    public RestClient(
        HttpMessageHandler      handler,
        bool                    disposeHandler         = true,
        ConfigureRestClient?    configureRestClient    = null,
        ConfigureSerialization? configureSerialization = null
    )
        : this(new HttpClient(handler, disposeHandler), true, configureRestClient, configureSerialization) { }

    static void ConfigureHttpClient(HttpClient httpClient, RestClientOptions options) {
        if (options.MaxTimeout > 0) httpClient.Timeout = TimeSpan.FromMilliseconds(options.MaxTimeout);

        if (options.Expect100Continue != null) httpClient.DefaultRequestHeaders.ExpectContinue = options.Expect100Continue;
    }

    // ReSharper disable once CognitiveComplexity
    static void ConfigureHttpMessageHandler(HttpClientHandler handler, ReadOnlyRestClientOptions options) {
#if NET
        if (!OperatingSystem.IsBrowser()) {
#endif
            handler.UseCookies             = false;
            handler.Credentials            = options.Credentials;
            handler.UseDefaultCredentials  = options.UseDefaultCredentials;
            handler.AutomaticDecompression = options.AutomaticDecompression;
            handler.PreAuthenticate        = options.PreAuthenticate;
            if (options.MaxRedirects.HasValue) handler.MaxAutomaticRedirections = options.MaxRedirects.Value;

            if (options.RemoteCertificateValidationCallback != null)
                handler.ServerCertificateCustomValidationCallback =
                    (request, cert, chain, errors) => options.RemoteCertificateValidationCallback(request, cert, chain, errors);

            if (options.ClientCertificates != null) {
                handler.ClientCertificates.AddRange(options.ClientCertificates);
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            }
#if NET
        }
#endif
        handler.AllowAutoRedirect = options.FollowRedirects;

#if NET
        if (!OperatingSystem.IsBrowser() && !OperatingSystem.IsIOS() && !OperatingSystem.IsTvOS()) {
#endif
            if (handler.SupportsProxy) handler.Proxy = options.Proxy;
#if NET
        }
#endif
    }

    [MemberNotNull(nameof(Serializers))]
    [MemberNotNull(nameof(AcceptedContentTypes))]
    void ConfigureSerializers(ConfigureSerialization? configureSerialization) {
        var serializerConfig = new SerializerConfig();
        serializerConfig.UseDefaultSerializers();
        configureSerialization?.Invoke(serializerConfig);
        Serializers          = new RestSerializers(serializerConfig);
        AcceptedContentTypes = Serializers.GetAcceptedContentTypes();
    }

    void ConfigureDefaultParameters(RestClientOptions options) {
        if (options.UserAgent != null) {
            if (!options.AllowMultipleDefaultParametersWithSameName
                && DefaultParameters.Any(parameter => parameter.Type == ParameterType.HttpHeader && parameter.Name == KnownHeaders.UserAgent))
                DefaultParameters.RemoveParameter(KnownHeaders.UserAgent, ParameterType.HttpHeader);
            DefaultParameters.AddParameter(Parameter.CreateParameter(KnownHeaders.UserAgent, options.UserAgent, ParameterType.HttpHeader));
        }
    }

    readonly bool _disposeHttpClient;

    bool _disposed;

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
