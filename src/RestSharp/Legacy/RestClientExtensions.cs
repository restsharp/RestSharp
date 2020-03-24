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
using System.Threading.Tasks;

// ReSharper disable CheckNamespace

namespace RestSharp
{
    public partial class RestClientExtensions
    {
        /// <summary>
        ///     Executes the request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <param name="client">The IRestClient this method extends</param>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion</param>
        [Obsolete("Use ExecuteAsync that returns Task")]
        public static RestRequestAsyncHandle ExecuteAsync(
            this IRestClient client,
            IRestRequest request,
            Action<IRestResponse> callback
        )
            => client.ExecuteAsync(request, (response, handle) => callback(response));

        /// <summary>
        ///     Executes the request and callback asynchronously, authenticating if needed
        /// </summary>
        /// <param name="client">The IRestClient this method extends</param>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to be executed</param>
        /// <param name="callback">Callback function to be executed upon completion providing access to the async handle</param>
        [Obsolete("Use ExecuteAsync that returns Task")]
        public static RestRequestAsyncHandle ExecuteAsync<T>(
            this IRestClient client,
            IRestRequest request,
            Action<IRestResponse<T>> callback
        ) where T : new()
            => client.ExecuteAsync<T>(request, (response, asyncHandle) => callback(response));

        [Obsolete("Use GetAsync that returns Task")]
        public static RestRequestAsyncHandle GetAsync<T>(
            this IRestClient client,
            IRestRequest request,
            Action<IRestResponse<T>, RestRequestAsyncHandle> callback
        ) where T : new()
            => client.ExecuteAsync(request, callback, Method.GET);

        [Obsolete("Use PostAsync that returns Task")]
        public static RestRequestAsyncHandle PostAsync<T>(
            this IRestClient client,
            IRestRequest request,
            Action<IRestResponse<T>, RestRequestAsyncHandle> callback
        ) where T : new()
            => client.ExecuteAsync(request, callback, Method.POST);

        [Obsolete("Use PutAsync that returns Task")]
        public static RestRequestAsyncHandle PutAsync<T>(
            this IRestClient client,
            IRestRequest request,
            Action<IRestResponse<T>, RestRequestAsyncHandle> callback
        ) where T : new()
            => client.ExecuteAsync(request, callback, Method.PUT);

        [Obsolete("Use HeadAsync that returns Task")]
        public static RestRequestAsyncHandle HeadAsync<T>(
            this IRestClient client,
            IRestRequest request,
            Action<IRestResponse<T>, RestRequestAsyncHandle> callback
        ) where T : new()
            => client.ExecuteAsync(request, callback, Method.HEAD);

        [Obsolete("Use OptionsAsync that returns Task")]
        public static RestRequestAsyncHandle OptionsAsync<T>(
            this IRestClient client,
            IRestRequest request,
            Action<IRestResponse<T>, RestRequestAsyncHandle> callback
        ) where T : new()
            => client.ExecuteAsync(request, callback, Method.OPTIONS);

        [Obsolete("Use PatchAsync that returns Task")]
        public static RestRequestAsyncHandle PatchAsync<T>(
            this IRestClient client,
            IRestRequest request,
            Action<IRestResponse<T>, RestRequestAsyncHandle> callback
        ) where T : new()
            => client.ExecuteAsync(request, callback, Method.PATCH);

        [Obsolete("Use DeleteAsync that returns Task")]
        public static RestRequestAsyncHandle DeleteAsync<T>(
            this IRestClient client,
            IRestRequest request,
            Action<IRestResponse<T>, RestRequestAsyncHandle> callback
        ) where T : new()
            => client.ExecuteAsync(request, callback, Method.DELETE);

        [Obsolete("Use GetAsync that returns Task")]
        public static RestRequestAsyncHandle GetAsync(
            this IRestClient client,
            IRestRequest request,
            Action<IRestResponse, RestRequestAsyncHandle> callback
        )
            => client.ExecuteAsync(request, callback, Method.GET);

        [Obsolete("Use PostAsync that returns Task")]
        public static RestRequestAsyncHandle PostAsync(
            this IRestClient client,
            IRestRequest request,
            Action<IRestResponse, RestRequestAsyncHandle> callback
        )
            => client.ExecuteAsync(request, callback, Method.POST);

        [Obsolete("Use PutAsync that returns Task")]
        public static RestRequestAsyncHandle PutAsync(
            this IRestClient client,
            IRestRequest request,
            Action<IRestResponse, RestRequestAsyncHandle> callback
        )
            => client.ExecuteAsync(request, callback, Method.PUT);

        [Obsolete("Use HeadAsync that returns Task")]
        public static RestRequestAsyncHandle HeadAsync(
            this IRestClient client,
            IRestRequest request,
            Action<IRestResponse, RestRequestAsyncHandle> callback
        )
            => client.ExecuteAsync(request, callback, Method.HEAD);

        [Obsolete("Use OptionsAsync that returns Task")]
        public static RestRequestAsyncHandle OptionsAsync(
            this IRestClient client,
            IRestRequest request,
            Action<IRestResponse, RestRequestAsyncHandle> callback
        )
            => client.ExecuteAsync(request, callback, Method.OPTIONS);

        [Obsolete("Use PatchAsync that returns Task")]
        public static RestRequestAsyncHandle PatchAsync(
            this IRestClient client,
            IRestRequest request,
            Action<IRestResponse, RestRequestAsyncHandle> callback
        )
            => client.ExecuteAsync(request, callback, Method.PATCH);

        [Obsolete("Use DeleteAsync that returns Task")]
        public static RestRequestAsyncHandle DeleteAsync(
            this IRestClient client,
            IRestRequest request,
            Action<IRestResponse, RestRequestAsyncHandle> callback
        )
            => client.ExecuteAsync(request, callback, Method.DELETE);

        [Obsolete("Use GetAsync")]
        public static Task<T> GetTaskAsync<T>(this IRestClient client, IRestRequest request)
            => client.ExecuteGetAsync<T>(request).ContinueWith(x => x.Result.Data);

        [Obsolete("Use PostAsync")]
        public static Task<T> PostTaskAsync<T>(this IRestClient client, IRestRequest request)
            => client.ExecutePostAsync<T>(request).ContinueWith(x => x.Result.Data);

        [Obsolete("Use PutAsync")]
        public static Task<T> PutTaskAsync<T>(this IRestClient client, IRestRequest request)
            => client.ExecuteAsync<T>(request, Method.PUT).ContinueWith(x => x.Result.Data);

        [Obsolete("Use HeadAsync")]
        public static Task<T> HeadTaskAsync<T>(this IRestClient client, IRestRequest request)
            => client.ExecuteAsync<T>(request, Method.HEAD).ContinueWith(x => x.Result.Data);

        [Obsolete("Use OptionsAsync")]
        public static Task<T> OptionsTaskAsync<T>(this IRestClient client, IRestRequest request)
            => client.ExecuteAsync<T>(request, Method.OPTIONS).ContinueWith(x => x.Result.Data);

        [Obsolete("Use PatchAsync")]
        public static Task<T> PatchTaskAsync<T>(this IRestClient client, IRestRequest request)
            => client.ExecuteAsync<T>(request, Method.PATCH).ContinueWith(x => x.Result.Data);

        [Obsolete("Use DeleteAsync")]
        public static Task<T> DeleteTaskAsync<T>(this IRestClient client, IRestRequest request)
            => client.ExecuteAsync<T>(request, Method.DELETE).ContinueWith(x => x.Result.Data);
    }
}