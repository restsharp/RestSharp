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

using System.Text;
using RestSharp.Authenticators;
using RestSharp.Serializers;

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
        IRestClient UseSerializer<T>() where T : class, IRestSerializer, new();

        IAuthenticator? Authenticator { get; set; }

        RestResponse<T> Deserialize<T>(RestResponse response);

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
        /// A specialized method to download files.
        /// </summary>
        /// <param name="request">Pre-configured request instance.</param>
        /// <returns>The downloaded file.</returns>
        byte[] DownloadData(IRestRequest request);

        /// <summary>
        /// Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<RestResponse<T>> ExecuteAsync<T>(IRestRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="httpMethod">Override the request method</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<RestResponse<T>> ExecuteAsync<T>(IRestRequest request, Method httpMethod, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="httpMethod">Override the request method</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<RestResponse> ExecuteAsync(IRestRequest request, Method httpMethod, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the request asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<RestResponse> ExecuteAsync(IRestRequest request, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Executes a GET-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<RestResponse<T>> ExecuteGetAsync<T>(IRestRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a POST-style request asynchronously, authenticating if needed
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">The cancellation token</param>
        Task<RestResponse<T>> ExecutePostAsync<T>(IRestRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a GET-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<RestResponse> ExecuteGetAsync(IRestRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a POST-style asynchronously, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<RestResponse> ExecutePostAsync(IRestRequest request, CancellationToken cancellationToken = default);
    }
}