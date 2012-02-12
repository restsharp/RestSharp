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
    }
}