namespace RestSharp
{
    public static class RestClientJsonRequest
    {
        public static IRestResponse<TResponse> Get<TRequest, TResponse>(
            this IRestClient client,
            JsonRequest<TRequest, TResponse> request
        ) where TResponse : new()
            => request.UpdateResponse(client.Execute<TResponse>(request, Method.GET));

        public static IRestResponse<TResponse> Post<TRequest, TResponse>(
            this IRestClient client,
            JsonRequest<TRequest, TResponse> request
        ) where TResponse : new()
            => request.UpdateResponse(client.Execute<TResponse>(request, Method.POST));

        public static IRestResponse<TResponse> Put<TRequest, TResponse>(
            this IRestClient client,
            JsonRequest<TRequest, TResponse> request
        ) where TResponse : new()
            => request.UpdateResponse(client.Execute<TResponse>(request, Method.PUT));

        public static IRestResponse<TResponse> Head<TRequest, TResponse>(
            this IRestClient client,
            JsonRequest<TRequest, TResponse> request
        ) where TResponse : new()
            => request.UpdateResponse(client.Execute<TResponse>(request, Method.HEAD));

        public static IRestResponse<TResponse> Options<TRequest, TResponse>(
            this IRestClient client,
            JsonRequest<TRequest, TResponse> request
        ) where TResponse : new()
            => request.UpdateResponse(client.Execute<TResponse>(request, Method.OPTIONS));

        public static IRestResponse<TResponse> Patch<TRequest, TResponse>(
            this IRestClient client,
            JsonRequest<TRequest, TResponse> request
        ) where TResponse : new()
            => request.UpdateResponse(client.Execute<TResponse>(request, Method.PATCH));

        public static IRestResponse<TResponse> Delete<TRequest, TResponse>(
            this IRestClient client,
            JsonRequest<TRequest, TResponse> request
        ) where TResponse : new()
            => request.UpdateResponse(client.Execute<TResponse>(request, Method.DELETE));
    }
}