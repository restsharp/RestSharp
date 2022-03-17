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
// 

using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RestSharp;

public class RestClientOptions {
    /// <summary>
    /// Default constructor for default RestClientOptions
    /// </summary>
    public RestClientOptions() { }

    /// <summary>
    /// Constructor for RestClientOptions using a specific base URL pass in the Uri format.
    /// </summary>
    /// <param name="baseUrl">Base URL to use in Uri format</param>
    public RestClientOptions(Uri baseUrl) => BaseUrl = baseUrl;

    /// <summary>
    /// Constructor for RestClientOptions using a specific base URL pass as a string.
    /// </summary>
    /// <param name="baseUrl">Base URL to use in string format</param>
    public RestClientOptions(string baseUrl) : this(new Uri(Ensure.NotEmptyString(baseUrl, nameof(baseUrl)))) { }

    /// <summary>
    /// Base URI for all requests base with the RestClient. This cannot be changed after the client has been
    /// constructed.
    /// </summary>
    public Uri? BaseUrl { get; internal set; }

    /// <summary>
    /// Optional callback to allow you to configure the underlying HttpMessageHandler for this client when it is created.
    /// </summary>
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

    /// <summary>
    /// Option to enable automatic decompression of responses. Defaults to GZip or higher where possible.
    /// </summary>
#if NETSTANDARD
    public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.GZip;
#else
    public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.All;
#endif

    /// <summary>
    /// Option to control the maximum number of redirects the client will follow before giving up. If not
    /// provided the HttpClient default value of 50 is used.
    /// </summary>
    public int? MaxRedirects { get; set; }

    /// <summary>
    /// X509CertificateCollection to be sent with request
    /// </summary>
    public X509CertificateCollection? ClientCertificates { get; set; }

    /// <summary>
    /// Define the optional web proxy to use for all requests via this client instance
    /// </summary>
    public IWebProxy? Proxy { get; set; }

    /// <summary>
    /// Indicates whether the client will follow redirects or not. Defaults to true.
    /// </summary>
    public bool FollowRedirects { get; set; } = true;

    /// <summary>
    /// Gets or sets a Boolean value that determines whether 100-Continue behavior is used. Default is true.
    /// </summary>
    public bool Expect100Continue { get; set; } = true;

    /// <summary>
    /// Sets the timeout in milliseconds for all requests using this client. You can also set a timeout value on a per
    /// request basis, but bear in mind the shorter of the two values is what will end up being used. So if you need long
    /// timeouts at the request level, you will want to set this to a larger value than the maximum you need per request.
    ///
    /// If this value is left at the default of 0, the build in default HttpClient timeout of 100 seconds is used.
    /// </summary>
    public int Timeout { get; set; }

    /// <summary>
    /// Set the encoding to use for encoding encoding query strings. By default it uses UTF8.
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

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

    /// <summary>
    /// By default, RestSharp doesn't allow multiple parameters to have the same name.
    /// This properly allows to override the default behavior.
    /// </summary>
    public bool AllowMultipleDefaultParametersWithSameName { get; set; }
}