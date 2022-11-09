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

namespace RestSharp;

public class RestClientOptions {
    static readonly Version Version = new AssemblyName(typeof(RestClientOptions).Assembly.FullName!).Version!;

    static readonly string DefaultUserAgent = $"RestSharp/{Version}";

    public RestClientOptions() { }

    public RestClientOptions(Uri baseUrl) => BaseUrl = baseUrl;

    public RestClientOptions(string baseUrl) : this(new Uri(Ensure.NotEmptyString(baseUrl, nameof(baseUrl)))) { }

    /// <summary>
    /// Explicit Host header value to use in requests independent from the request URI.
    /// If null, default host value extracted from URI is used.
    /// </summary>
    public Uri? BaseUrl { get; set; }

    public Func<HttpMessageHandler, HttpMessageHandler>? ConfigureMessageHandler { get; set; }

    /// <summary>
    /// In general you would not need to set this directly. Used by the NtlmAuthenticator.
    /// </summary>
    public ICredentials? Credentials { get; set; }

    /// <summary>
    /// Determine whether or not the "default credentials" (e.g. the user account under which the current process is
    /// running) will be sent along to the server. The default is false.
    /// </summary>
    public bool UseDefaultCredentials { get; set; }

    /// <summary>
    /// Set to true if you need the Content-Type not to have the charset 
    /// </summary>
    public bool DisableCharset { get; set; }

#if NETSTANDARD
    public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.GZip;
#else
    public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.All;
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
    /// Maximum request duration in milliseconds. When the request timeout is specified using <seealso cref="RestRequest.Timeout"/>,
    /// the lowest value between the client timeout and request timeout will be used.
    /// </summary>
    public int MaxTimeout { get; set; }

    public Encoding Encoding { get; set; } = Encoding.UTF8;

    [Obsolete("Use MaxTimeout instead")]
    public int Timeout {
        get => MaxTimeout;
        set => MaxTimeout = value;
    }

    /// <summary>
    /// Flag to send authorisation header with the HttpWebRequest
    /// </summary>
    public bool PreAuthenticate { get; set; }

    /// <summary>
    /// Modifies the default behavior of RestSharp to swallow exceptions.
    /// When set to <code>true</code>, a <see cref="DeserializationException"/> will be thrown
    /// in case RestSharp fails to deserialize the response.
    /// </summary>
    public bool ThrowOnDeserializationError { get; set; }

    /// <summary>
    /// Modifies the default behavior of RestSharp to swallow exceptions.
    /// When set to <code>true</code>, RestSharp will consider the request as unsuccessful
    /// in case it fails to deserialize the response.
    /// </summary>
    public bool FailOnDeserializationError { get; set; } = true;

    /// <summary>
    /// Modifies the default behavior of RestSharp to swallow exceptions.
    /// When set to <code>true</code>, exceptions will be re-thrown.
    /// </summary>
    public bool ThrowOnAnyError { get; set; }

    /// <summary>
    /// Callback function for handling the validation of remote certificates. Useful for certificate pinning and
    /// overriding certificate errors in the scope of a request.
    /// </summary>
    public RemoteCertificateValidationCallback? RemoteCertificateValidationCallback { get; set; }

    public string? BaseHost { get; set; }

    /// <summary>
    /// By default, RestSharp doesn't allow multiple parameters to have the same name.
    /// This properly allows to override the default behavior.
    /// </summary>
    public bool AllowMultipleDefaultParametersWithSameName { get; set; }
}
