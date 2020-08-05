//   Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Cache;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RestSharp.Authenticators;
using RestSharp.Deserializers;
using RestSharp.Serialization;
#pragma warning disable 618

namespace RestSharp
{
    [PublicAPI]
    public partial interface IRestClient
    {
        /// <summary>
        /// Replace the default serializer with a custom one
        /// </summary>
        /// <param name="serializerFactory">Function that returns the serializer instance</param>
        IRestClient UseSerializer(Func<IRestSerializer> serializerFactory);

        /// <summary>
        /// Replace the default serializer with a custom one
        /// </summary>
        /// <typeparam name="T">The type that implements <see cref="IRestSerializer"/></typeparam>
        /// <returns></returns>
        IRestClient UseSerializer<T>() where T : IRestSerializer, new();

        CookieContainer? CookieContainer { get; set; }

        bool AutomaticDecompression { get; set; }

        int? MaxRedirects { get; set; }

        string UserAgent { get; set; }

        int Timeout { get; set; }

        int ReadWriteTimeout { get; set; }

        bool UseSynchronizationContext { get; set; }

        IAuthenticator? Authenticator { get; set; }

        Uri? BaseUrl { get; set; }

        Encoding Encoding { get; set; }

        /// <summary>
        /// Modifies the default behavior of RestSharp to swallow exceptions.
        /// When set to <code>true</code>, a <see cref="DeserializationException"/> will be thrown
        /// in case RestSharp fails to deserialize the response.
        /// </summary>
        bool ThrowOnDeserializationError { get; set; }

        /// <summary>
        /// Modifies the default behavior of RestSharp to swallow exceptions.
        /// When set to <code>true</code>, RestSharp will consider the request as unsuccessful
        /// in case it fails to deserialize the response.
        /// </summary>
        bool FailOnDeserializationError { get; set; }

        /// <summary>
        /// Modifies the default behavior of RestSharp to swallow exceptions.
        /// When set to <code>true</code>, exceptions will be re-thrown.
        /// </summary>
        bool ThrowOnAnyError { get; set; }

        string? ConnectionGroupName { get; set; }

        /// <summary>
        /// Flag to send authorisation header with the HttpWebRequest
        /// </summary>
        bool PreAuthenticate { get; set; }

        /// <summary>
        /// Flag to reuse same connection in the HttpWebRequest
        /// </summary>
        bool UnsafeAuthenticatedConnectionSharing { get; set; }

        /// <summary>
        /// A list of parameters that will be set for all requests made
        /// by the RestClient instance.
        /// </summary>
        IList<Parameter> DefaultParameters { get; }

        /// <summary>
        /// Explicit Host header value to use in requests independent from the request URI.
        /// If null, default host value extracted from URI is used.
        /// </summary>
        string? BaseHost { get; set; }

        /// <summary>
        /// By default, RestSharp doesn't allow multiple parameters to have the same name.
        /// This properly allows to override the default behavior.
        /// </summary>
        bool AllowMultipleDefaultParametersWithSameName { get; set; }

        /// <summary>
        /// X509CertificateCollection to be sent with request
        /// </summary>
        X509CertificateCollection? ClientCertificates { get; set; }

        IWebProxy? Proxy { get; set; }

        RequestCachePolicy? CachePolicy { get; set; }

        bool Pipelined { get; set; }

        bool FollowRedirects { get; set; }

        /// <summary>
        /// Callback function for handling the validation of remote certificates. Useful for certificate pinning and
        /// overriding certificate errors in the scope of a request.
        /// </summary>
        RemoteCertificateValidationCallback? RemoteCertificateValidationCallback { get; set; }

        IRestResponse<T> Deserialize<T>(IRestResponse response);

        /// <summary>
        /// Allows to use a custom way to encode URL parameters
        /// </summary>
        /// <param name="encoder">A delegate to encode URL parameters</param>
        /// <example>client.UseUrlEncoder(s => HttpUtility.UrlEncode(s));</example>
        /// <returns></returns>
        IRestClient UseUrlEncoder(Func<string, string> encoder);

        /// <summary>
        /// Allows to use a custom way to encode query parameters
        /// </summary>
        /// <param name="queryEncoder">A delegate to encode query parameters</param>
        /// <example>client.UseUrlEncoder((s, encoding) => HttpUtility.UrlEncode(s, encoding));</example>
        /// <returns></returns>
        IRestClient UseQueryEncoder(Func<string, Encoding, string> queryEncoder);

        /// <summary>
        /// Executes the given request and returns an untyped response.
        /// </summary>
        /// <param name="request">Pre-configured request instance.</param>
        /// <returns>Untyped response.</returns>
        IRestResponse Execute(IRestRequest request);

        /// <summary>
        /// Executes the given request and returns an untyped response.
        /// Allows to specify the HTTP method (GET, POST, etc) so you won't need to set it on the request.
        /// </summary>
        /// <param name="request">Pre-configured request instance.</param>
        /// <param name="httpMethod">The HTTP method (GET, POST, etc) to be used when making the request.</param>
        /// <returns>Untyped response.</returns>
        IRestResponse Execute(IRestRequest request, Method httpMethod);

        /// <summary>
        /// Executes the given request and returns a typed response.
        /// RestSharp will deserialize the response and it will be available in the <code>Data</code>
        /// property of the response instance.
        /// </summary>
        /// <param name="request">Pre-configured request instance.</param>
        /// <returns>Typed response.</returns>
        IRestResponse<T> Execute<T>(IRestRequest request);

        /// <summary>
        /// Executes the given request and returns a typed response.
        /// RestSharp will deserialize the response and it will be available in the <code>Data</code>
        /// property of the response instance.
        /// Allows to specify the HTTP method (GET, POST, etc) so you won't need to set it on the request.
        /// </summary>
        /// <param name="request">Pre-configured request instance.</param>
        /// <param name="httpMethod">The HTTP method (GET, POST, etc) to be used when making the request.</param>
        /// <returns>Typed response.</returns>
        IRestResponse<T> Execute<T>(IRestRequest request, Method httpMethod);

        /// <summary>
        /// A specialized method to download files.
        /// </summary>
        /// <param name="request">Pre-configured request instance.</param>
        /// <returns>The downloaded file.</returns>
        byte[] DownloadData(IRestRequest request);

        /// <summary>
        /// Executes the specified request and downloads the response data
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <param name="throwOnError">Throw an exception if download fails.</param>
        /// <returns>Response data</returns>
        [Obsolete("Use ThrowOnAnyError property to instruct RestSharp to rethrow exceptions")]
        byte[] DownloadData(IRestRequest request, bool throwOnError);

        Uri BuildUri(IRestRequest request);

        string BuildUriWithoutQueryParameters(IRestRequest request);

        /// <summary>
        /// Add a delegate to apply custom configuration to HttpWebRequest before making a call
        /// </summary>
        /// <param name="configurator">Configuration delegate for HttpWebRequest</param>
        void ConfigureWebRequest(Action<HttpWebRequest> configurator);
        
        /// <summary>
        /// Adds or replaces a deserializer for the specified content type
        /// </summary>
        /// <param name="contentType">Content type for which the deserializer will be replaced</param>
        /// <param name="deserializerFactory">Custom deserializer factory</param>
        void AddHandler(string contentType, Func<IDeserializer> deserializerFactory);

        /// <summary>
        /// Removes custom deserialzier for the specified content type
        /// </summary>
        /// <param name="contentType">Content type for which deserializer needs to be removed</param>
        void RemoveHandler(string contentType);

        /// <summary>
        /// Remove deserializers for all content types
        /// </summary>
        void ClearHandlers();

        IRestResponse ExecuteAsGet(IRestRequest request, string httpMethod);

        IRestResponse ExecuteAsPost(IRestRequest request, string httpMethod);

        IRestResponse<T> ExecuteAsGet<T>(IRestRequest request, string httpMethod);

        IRestResponse<T> ExecuteAsPost<T>(IRestRequest request, string httpMethod);

        /// <summary>
        /// Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<IRestResponse<T>> ExecuteAsync<T>(IRestRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="httpMethod">Override the request method</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<IRestResponse<T>> ExecuteAsync<T>(IRestRequest request, Method httpMethod, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="httpMethod">Override the request method</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<IRestResponse> ExecuteAsync(IRestRequest request, Method httpMethod, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<IRestResponse> ExecuteAsync(IRestRequest request, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Executes a GET-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<IRestResponse<T>> ExecuteGetAsync<T>(IRestRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a POST-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">The cancellation token</param>
        Task<IRestResponse<T>> ExecutePostAsync<T>(IRestRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a GET-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<IRestResponse> ExecuteGetAsync(IRestRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a POST-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<IRestResponse> ExecutePostAsync(IRestRequest request, CancellationToken cancellationToken = default);
    }
}