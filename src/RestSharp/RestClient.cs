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
using RestSharp.Serializers.Json;
using RestSharp.Serializers.Xml;

// ReSharper disable VirtualMemberCallInConstructor
#pragma warning disable 618

namespace RestSharp;

/// <summary>
/// Client to translate RestRequests into Http requests and process response result
/// </summary>
public partial class RestClient {
    public CookieContainer CookieContainer { get; }

    public string[] AcceptedContentTypes { get; private set; } = null!;

    HttpClient HttpClient { get; }

    internal RestClientOptions Options { get; }

    /// <summary>
    /// Default constructor that registers default content handlers
    /// </summary>
    public RestClient() : this(new RestClientOptions()) { }

    public RestClient(HttpClient httpClient, RestClientOptions? options = null) {
        UseDefaultSerializers();

        HttpClient      = httpClient;
        Options         = options ?? new RestClientOptions();
        CookieContainer = Options.CookieContainer ?? new CookieContainer();

        if (Options.Timeout > 0)
            HttpClient.Timeout = TimeSpan.FromMilliseconds(Options.Timeout);
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Options.UserAgent);
    }

    public RestClient(HttpMessageHandler handler) : this(new HttpClient(handler)) { }

    public RestClient(RestClientOptions options) {
        UseDefaultSerializers();

        Options         = options;
        CookieContainer = Options.CookieContainer ?? new CookieContainer();

        var handler = new HttpClientHandler {
            Credentials            = Options.Credentials,
            UseDefaultCredentials  = Options.UseDefaultCredentials,
            CookieContainer        = CookieContainer,
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

    public ParametersCollection DefaultParameters { get; } = new();

    /// <summary>
    /// Adds cookie to the <seealso cref="HttpClient"/> cookie container.
    /// </summary>
    /// <param name="name">Cookie name</param>
    /// <param name="value">Cookie value</param>
    /// <returns></returns>
    public RestClient AddCookie(string name, string value) {
        lock (CookieContainer) {
            CookieContainer.Add(new Cookie(name, value));
        }

        return this;
    }

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
                "Request resource doesn't contain a valid scheme for an empty client base URL"
            );
    }
}