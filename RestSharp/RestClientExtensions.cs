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

		public static RestRequestAsyncHandle GetAsync<T>(this IRestClient client, IRestRequest request, Action<RestResponse<T>, RestRequestAsyncHandle> callback) where T : new()
		{
			request.Method = Method.GET;
			return client.ExecuteAsync<T>(request, callback);
		}

		public static RestRequestAsyncHandle PostAsync<T>(this IRestClient client, IRestRequest request, Action<RestResponse<T>, RestRequestAsyncHandle> callback) where T : new()
		{
			request.Method = Method.POST;
			return client.ExecuteAsync<T>(request, callback);
		}

		public static RestRequestAsyncHandle PutAsync<T>(this IRestClient client, IRestRequest request, Action<RestResponse<T>, RestRequestAsyncHandle> callback) where T : new()
		{
			request.Method = Method.PUT;
			return client.ExecuteAsync<T>(request, callback);
		}

		public static RestRequestAsyncHandle HeadAsync<T>(this IRestClient client, IRestRequest request, Action<RestResponse<T>, RestRequestAsyncHandle> callback) where T : new()
		{
			request.Method = Method.HEAD;
			return client.ExecuteAsync<T>(request, callback);
		}

		public static RestRequestAsyncHandle OptionsAsync<T>(this IRestClient client, IRestRequest request, Action<RestResponse<T>, RestRequestAsyncHandle> callback) where T : new()
		{
			request.Method = Method.OPTIONS;
			return client.ExecuteAsync<T>(request, callback);
		}

		public static RestRequestAsyncHandle PatchAsync<T>(this IRestClient client, IRestRequest request, Action<RestResponse<T>, RestRequestAsyncHandle> callback) where T : new()
		{
			request.Method = Method.PATCH;
			return client.ExecuteAsync<T>(request, callback);
		}

		public static RestRequestAsyncHandle DeleteAsync<T>(this IRestClient client, IRestRequest request, Action<RestResponse<T>, RestRequestAsyncHandle> callback) where T : new()
		{
			request.Method = Method.DELETE;
			return client.ExecuteAsync<T>(request, callback);
		}

		public static RestRequestAsyncHandle GetAsync(this IRestClient client, IRestRequest request, Action<RestResponse, RestRequestAsyncHandle> callback)
		{
			request.Method = Method.GET;
			return client.ExecuteAsync(request, callback);
		}

		public static RestRequestAsyncHandle PostAsync(this IRestClient client, IRestRequest request, Action<RestResponse, RestRequestAsyncHandle> callback)
		{
			request.Method = Method.POST;
			return client.ExecuteAsync(request, callback);
		}

		public static RestRequestAsyncHandle PutAsync(this IRestClient client, IRestRequest request, Action<RestResponse, RestRequestAsyncHandle> callback)
		{
			request.Method = Method.PUT;
			return client.ExecuteAsync(request, callback);
		}

		public static RestRequestAsyncHandle HeadAsync(this IRestClient client, IRestRequest request, Action<RestResponse, RestRequestAsyncHandle> callback)
		{
			request.Method = Method.HEAD;
			return client.ExecuteAsync(request, callback);
		}

		public static RestRequestAsyncHandle OptionsAsync(this IRestClient client, IRestRequest request, Action<RestResponse, RestRequestAsyncHandle> callback)
		{
			request.Method = Method.OPTIONS;
			return client.ExecuteAsync(request, callback);
		}

		public static RestRequestAsyncHandle PatchAsync(this IRestClient client, IRestRequest request, Action<RestResponse, RestRequestAsyncHandle> callback)
		{
			request.Method = Method.PATCH;
			return client.ExecuteAsync(request, callback);
		}

		public static RestRequestAsyncHandle DeleteAsync(this IRestClient client, IRestRequest request, Action<RestResponse, RestRequestAsyncHandle> callback)
		{
			request.Method = Method.DELETE;
			return client.ExecuteAsync(request, callback);
		}

#if FRAMEWORK
		public static RestResponse<T> Get<T>(this IRestClient client, IRestRequest request) where T : new()
		{
			request.Method = Method.GET;
			return client.Execute<T>(request);
		}

		public static RestResponse<T> Post<T>(this IRestClient client, IRestRequest request) where T : new()
		{
			request.Method = Method.POST;
			return client.Execute<T>(request);
		}

		public static RestResponse<T> Put<T>(this IRestClient client, IRestRequest request) where T : new()
		{
			request.Method = Method.PUT;
			return client.Execute<T>(request);
		}

		public static RestResponse<T> Head<T>(this IRestClient client, IRestRequest request) where T : new()
		{
			request.Method = Method.HEAD;
			return client.Execute<T>(request);
		}

		public static RestResponse<T> Options<T>(this IRestClient client, IRestRequest request) where T : new()
		{
			request.Method = Method.OPTIONS;
			return client.Execute<T>(request);
		}

		public static RestResponse<T> Patch<T>(this IRestClient client, IRestRequest request) where T : new()
		{
			request.Method = Method.PATCH;
			return client.Execute<T>(request);
		}

		public static RestResponse<T> Delete<T>(this IRestClient client, IRestRequest request) where T : new()
		{
			request.Method = Method.DELETE;
			return client.Execute<T>(request);
		}

		public static RestResponse Get(this IRestClient client, IRestRequest request)
		{
			request.Method = Method.GET;
			return client.Execute(request);
		}

		public static RestResponse Post(this IRestClient client, IRestRequest request)
		{
			request.Method = Method.POST;
			return client.Execute(request);
		}

		public static RestResponse Put(this IRestClient client, IRestRequest request)
		{
			request.Method = Method.PUT;
			return client.Execute(request);
		}

		public static RestResponse Head(this IRestClient client, IRestRequest request)
		{
			request.Method = Method.HEAD;
			return client.Execute(request);
		}

		public static RestResponse Options(this IRestClient client, IRestRequest request)
		{
			request.Method = Method.OPTIONS;
			return client.Execute(request);
		}

		public static RestResponse Patch(this IRestClient client, IRestRequest request)
		{
			request.Method = Method.PATCH;
			return client.Execute(request);
		}

		public static RestResponse Delete(this IRestClient client, IRestRequest request)
		{
			request.Method = Method.DELETE;
			return client.Execute(request);
		}
#endif
	}
}