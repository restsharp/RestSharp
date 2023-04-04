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
// 

using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Reflection;
using System.Runtime.Versioning;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using RestSharp.Authenticators;
using RestSharp.Extensions;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace RestSharp;

[GenerateImmutable]
public class RestClientOptions {
    static readonly Version Version = new AssemblyName(typeof(RestClientOptions).Assembly.FullName!).Version!;

    static readonly string DefaultUserAgent = $"RestSharp/{Version}";

    public RestClientOptions() { }

    public RestClientOptions(Uri baseUrl) => BaseUrl = baseUrl;

    public RestClientOptions(string baseUrl) : this(new Uri(Ensure.NotEmptyString(baseUrl, nameof(baseUrl)))) { }

    /// <summary>
    /// Base URL for all requests made with this client instance
    /// </summary>
    public Uri? BaseUrl { get; set; }

    /// <summary>
    /// Custom configuration for the underlying <seealso cref="HttpMessageHandler"/>
    /// </summary>
    public Func<HttpMessageHandler, HttpMessageHandler>? ConfigureMessageHandler { get; set; }

    /// <summary>
    /// Function to calculate the response status. By default, the status will be Completed if it was successful, or NotFound.
    /// </summary>
    public CalculateResponseStatus CalculateResponseStatus { get; set; } = httpResponse
        => httpResponse.IsSuccessStatusCode || httpResponse.StatusCode == HttpStatusCode.NotFound
            ? ResponseStatus.Completed
            : ResponseStatus.Error;

    /// <summary>
    /// Authenticator that will be used to populate request with necessary authentication data
    /// </summary>
    public IAuthenticator? Authenticator { get; set; }

    /// <summary>
    /// Passed to <see cref="HttpMessageHandler"/> <code>Credentials</code> property
    /// </summary>
#if NET
    [UnsupportedOSPlatform("browser")]
#endif
    public ICredentials? Credentials { get; set; }

    /// <summary>
    /// Determine whether or not the "default credentials" (e.g. the user account under which the current process is
    /// running) will be sent along to the server. The default is false.
    /// Passed to <see cref="HttpMessageHandler"/> <code>UseDefaultCredentials</code> property
    /// </summary>
#if NET
    [UnsupportedOSPlatform("browser")]
#endif
    public bool UseDefaultCredentials { get; set; }

    /// <summary>
    /// Set to true if you need the Content-Type not to have the charset 
    /// </summary>
    public bool DisableCharset { get; set; }

    /// <summary>
    /// Set the decompression method to use when making requests
    /// </summary>
#if NET
    [UnsupportedOSPlatform("browser")]
    public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.All;
#else
    public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.GZip;
#endif

    /// <summary>
    /// Set the maximum number of redirects to follow
    /// </summary>
#if NET
    [UnsupportedOSPlatform("browser")]
#endif
    public int? MaxRedirects { get; set; }

    /// <summary>
    /// X509CertificateCollection to be sent with request
    /// </summary>
#if NET
    [UnsupportedOSPlatform("browser")]
#endif
    public X509CertificateCollection? ClientCertificates { get; set; }

    /// <summary>
    /// Set the proxy to use when making requests. Default is null, which will use the default system proxy if one is set.
    /// </summary>
#if NET
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public IWebProxy? Proxy { get; set; }

    /// <summary>
    /// Cache policy to be used for requests using <seealso cref="CacheControlHeaderValue"/>
    /// </summary>
    public CacheControlHeaderValue? CachePolicy { get; set; }

    /// <summary>
    /// Instruct the client to follow redirects. Default is true.
    /// </summary>
    public bool FollowRedirects { get; set; } = true;

    /// <summary>
    /// Gets or sets a value that indicates if the <see langword="Expect" /> header for an HTTP request contains Continue.
    /// </summary>
    public bool? Expect100Continue { get; set; } = null;

    /// <summary>
    /// Value of the User-Agent header to be sent with requests. Default is "RestSharp/{version}"
    /// </summary>
    public string? UserAgent { get; set; } = DefaultUserAgent;

    /// <summary>
    /// Passed to <see cref="HttpMessageHandler"/> <see langword="PreAuthenticate"/> property
    /// </summary>
#if NET
    [UnsupportedOSPlatform("browser")]
#endif
    public bool PreAuthenticate { get; set; }

    /// <summary>
    /// Callback function for handling the validation of remote certificates. Useful for certificate pinning and
    /// overriding certificate errors in the scope of a request.
    /// </summary>
#if NET
    [UnsupportedOSPlatform("browser")]
#endif
    public RemoteCertificateValidationCallback? RemoteCertificateValidationCallback { get; set; }

    /// <summary>
    /// Sets the value of the Host header to be sent with requests.
    /// </summary>
    public string? BaseHost { get; set; }

    /// <summary>
    /// Custom cookie container to be used for requests. RestSharp will not assign the container to the message handler,
    /// but will fetch cookies from it and set them on the request.
    /// </summary>
    public CookieContainer? CookieContainer { get; set; }

    /// <summary>
    /// Maximum request duration in milliseconds. When the request timeout is specified using <seealso cref="RestRequest.Timeout"/>,
    /// the lowest value between the client timeout and request timeout will be used.
    /// </summary>
    public int MaxTimeout { get; set; }

    /// <summary>
    /// Default encoding to use when no encoding is specified in the content type header.
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <summary>
    /// Set to true to throw an exception when a deserialization error occurs. Default is false.
    /// </summary>
    public bool ThrowOnDeserializationError { get; set; }

    /// <summary>
    /// When set to true, the response status will be set to <see cref="ResponseStatus.Error"/>
    /// when a deserialization error occurs. Default is true.
    /// </summary>
    public bool FailOnDeserializationError { get; set; } = true;

    /// <summary>
    /// Set to true to throw an exception when <seealso cref="HttpClient"/> throws an exception when making a request.
    /// Default is false.
    /// </summary>
    public bool ThrowOnAnyError { get; set; }

    /// <summary>
    /// Set to true to allow multiple default parameters with the same name. Default is false.
    /// </summary>
    public bool AllowMultipleDefaultParametersWithSameName { get; set; }

    /// <summary>
    /// Custom function to encode a string for use in a URL.
    /// </summary>
    public Func<string, string> Encode { get; set; } = s => s.UrlEncode();

    /// <summary>
    /// Custom function to encode a string for use in a URL query.
    /// </summary>
    public Func<string, Encoding, string> EncodeQuery { get; set; } = (s, encoding) => s.UrlEncode(encoding)!;
}
