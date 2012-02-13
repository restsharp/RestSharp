using System;

namespace RestSharp
{
    public static class RestClientExtensions
    {
        /// <summary>
        /// Executes the request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <param name="client">The IRestClient this method extends</param>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion</param>
        public static RestRequestAsyncHandle ExecuteAsync(this IRestClient client, IRestRequest request, Action<RestResponse> callback)
        {
            return client.ExecuteAsync(request, (response, handle) => callback(response));
        }

        /// <summary>
        /// Executes the request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <param name="client">The IRestClient this method extends</param>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion providing access to the async handle</param>
        public static RestRequestAsyncHandle ExecuteAsync<T>(this IRestClient client, IRestRequest request, Action<RestResponse<T>> callback) where T : new()
        {
            return client.ExecuteAsync<T>(request, (response, asyncHandle) => callback(response));
        }
    }
}