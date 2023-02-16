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
using System.Security.Cryptography.X509Certificates;
using System.Text;
using RestSharp.Authenticators;
using RestSharp.Extensions;
using SourceGenerator;

namespace RestSharp;

[GenerateImmutable]
public class RestClientOptions {
    static readonly Version Version = new AssemblyName(typeof(RestClientOptions).Assembly.FullName!).Version!;

    static readonly string DefaultUserAgent = $"RestSharp/{Version}";

    public RestClientOptions() { }

    public RestClientOptions(Uri baseUrl) => BaseUrl = baseUrl;

    public RestClientOptions(string baseUrl) : this(new Uri(Ensure.NotEmptyString(baseUrl, nameof(baseUrl)))) { }

    /// <inheritdoc/>
    public Uri? BaseUrl { get; set; }

    public Func<HttpMessageHandler, HttpMessageHandler>? ConfigureMessageHandler { get; set; }

    /// <summary>
    /// Function to calculate the response status. By default, the status will be Completed if it was successful, or NotFound.
    /// </summary>
    public CalculateResponseStatus CalculateResponseStatus { get; set; } = httpResponse
        => httpResponse.IsSuccessStatusCode || httpResponse.StatusCode == HttpStatusCode.NotFound
            ? ResponseStatus.Completed
            : ResponseStatus.Error;

    volatile IAuthenticator? _authenticator;

    /// <summary>
    /// Authenticator that will be used to populate request with necessary authentication data
    /// </summary>
    public IAuthenticator? Authenticator {
        get => _authenticator;
        set => _authenticator = value;
    }

    /// <summary>
    /// Passed to <see cref="HttpMessageHandler"/> <code>Credentials</code> property
    /// </summary>
    public ICredentials? Credentials { get; set; }

    /// <summary>
    /// Determine whether or not the "default credentials" (e.g. the user account under which the current process is
    /// running) will be sent along to the server. The default is false.
    /// Passed to <see cref="HttpMessageHandler"/> <code>UseDefaultCredentials</code> property
    /// </summary>
    public bool UseDefaultCredentials { get; set; }

    /// <summary>
    /// Set to true if you need the Content-Type not to have the charset 
    /// </summary>
    public bool DisableCharset { get; set; }

#if NET
    public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.All;
#else
    public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.GZip;
#endif

    public int? MaxRedirects { get; set; }

    /// <summary>
    /// X509CertificateCollection to be sent with request
    /// </summary>
    public X509CertificateCollection? ClientCertificates { get; set; }

    public IWebProxy?               Proxy             { get; set; }
    public CacheControlHeaderValue? CachePolicy       { get; set; }
    public bool                     FollowRedirects   { get; set; } = true;
    public bool?                    Expect100Continue { get; set; } = null;
    public string?                  UserAgent         { get; set; } = DefaultUserAgent;

    /// <summary>
    /// Passed to <see cref="HttpMessageHandler"/> <code>PreAuthenticate</code> property
    /// </summary>
    public bool PreAuthenticate { get; set; }

    /// <summary>
    /// Callback function for handling the validation of remote certificates. Useful for certificate pinning and
    /// overriding certificate errors in the scope of a request.
    /// </summary>
    public RemoteCertificateValidationCallback? RemoteCertificateValidationCallback { get; set; }

    public string? BaseHost { get; set; }

    /// <summary>
    /// Maximum request duration in milliseconds. When the request timeout is specified using <seealso cref="RestRequest.Timeout"/>,
    /// the lowest value between the client timeout and request timeout will be used.
    /// </summary>
    public int MaxTimeout { get; set; }

    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <inheritdoc />>
    public bool ThrowOnDeserializationError { get; set; }

    /// <inheritdoc />>
    public bool FailOnDeserializationError { get; set; } = true;

    /// <inheritdoc />>
    public bool ThrowOnAnyError { get; set; }

    /// <inheritdoc />>
    public bool AllowMultipleDefaultParametersWithSameName { get; set; }

    /// <inheritdoc />>
    public Func<string, string> Encode { get; set; } = s => s.UrlEncode();

    /// <inheritdoc />>
    public Func<string, Encoding, string> EncodeQuery { get; set; } = (s, encoding) => s.UrlEncode(encoding)!;
}
