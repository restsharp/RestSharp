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