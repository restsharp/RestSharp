#if FRAMEWORK
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using RestSharp.Deserializers;

namespace RestSharp
{
	public partial class RestClient
	{

		/// <summary>
		/// Proxy to use for requests made by this client instance.
		/// Passed on to underying WebRequest if set.
		/// </summary>
		public IWebProxy Proxy { get; set; }

		/// <summary>
		/// Executes the specified request and downloads the response data
		/// </summary>
		/// <param name="request">Request to execute</param>
		/// <returns>Response data</returns>
		public byte[] DownloadData(IRestRequest request)
		{
			var response = Execute(request);
			return response.RawBytes;
		}

		/// <summary>
		/// Executes the request and returns a response, authenticating if needed
		/// </summary>
		/// <param name="request">Request to be executed</param>
		/// <returns>RestResponse</returns>
		public virtual RestResponse Execute(IRestRequest request)
		{
			AuthenticateIfNeeded(this, request);

			// add Accept header based on registered deserializers
			var accepts = string.Join(", ", AcceptTypes.ToArray());
			AddDefaultParameter("Accept", accepts, ParameterType.HttpHeader);

			var response = new RestResponse();
			try
			{
				response = GetResponse(request);
				response.Request = request;
				response.Request.IncreaseNumAttempts();

			}
			catch (Exception ex)
			{
				response.ResponseStatus = ResponseStatus.Error;
				response.ErrorMessage = ex.Message;
				response.ErrorException = ex;
			}

			return response;
		}

		/// <summary>
		/// Executes the specified request and deserializes the response content using the appropriate content handler
		/// </summary>
		/// <typeparam name="T">Target deserialization type</typeparam>
		/// <param name="request">Request to execute</param>
		/// <returns>RestResponse[[T]] with deserialized data in Data property</returns>
		public virtual RestResponse<T> Execute<T>(IRestRequest request) where T : new()
		{
			var raw = Execute(request);
			return Deserialize<T>(request, raw);
		}

		public virtual RestResponse<T> Get<T>(IRestRequest request) where T : new()
		{
			request.Method = Method.GET;
			return Execute<T>(request);
		}

		public virtual RestResponse<T> Post<T>(IRestRequest request) where T : new()
		{
			request.Method = Method.POST;
			return Execute<T>(request);
		}

		public virtual RestResponse<T> Put<T>(IRestRequest request) where T : new()
		{
			request.Method = Method.PUT;
			return Execute<T>(request);
		}

		public virtual RestResponse<T> Head<T>(IRestRequest request) where T : new()
		{
			request.Method = Method.HEAD;
			return Execute<T>(request);
		}

		public virtual RestResponse<T> Options<T>(IRestRequest request) where T : new()
		{
			request.Method = Method.OPTIONS;
			return Execute<T>(request);
		}

		public virtual RestResponse<T> Patch<T>(IRestRequest request) where T : new()
		{
			request.Method = Method.PATCH;
			return Execute<T>(request);
		}

		public virtual RestResponse<T> Delete<T>(IRestRequest request) where T : new()
		{
			request.Method = Method.DELETE;
			return Execute<T>(request);
		}

		public virtual RestResponse Get(IRestRequest request)
		{
			request.Method = Method.GET;
			return Execute(request);
		}

		public virtual RestResponse Post(IRestRequest request)
		{
			request.Method = Method.POST;
			return Execute(request);
		}

		public virtual RestResponse Put(IRestRequest request)
		{
			request.Method = Method.PUT;
			return Execute(request);
		}

		public virtual RestResponse Head(IRestRequest request)
		{
			request.Method = Method.HEAD;
			return Execute(request);
		}

		public virtual RestResponse Options(IRestRequest request)
		{
			request.Method = Method.OPTIONS;
			return Execute(request);
		}

		public virtual RestResponse Patch(IRestRequest request)
		{
			request.Method = Method.PATCH;
			return Execute(request);
		}

		public virtual RestResponse Delete(IRestRequest request)
		{
			request.Method = Method.DELETE;
			return Execute(request);
		}
		
		private RestResponse GetResponse(IRestRequest request)
		{
			var http = HttpFactory.Create();

			ConfigureHttp(request, http);
			ConfigureProxy(http);

			var httpResponse = new HttpResponse();

			switch (request.Method) {
				case Method.GET:
					httpResponse = http.Get();
					break;
				case Method.POST:
					httpResponse = http.Post();
					break;
				case Method.PUT:
					httpResponse = http.Put();
					break;
				case Method.DELETE:
					httpResponse = http.Delete();
					break;
				case Method.HEAD:
					httpResponse = http.Head();
					break;
				case Method.OPTIONS:
					httpResponse = http.Options();
					break;
				case Method.PATCH:
					httpResponse = http.Patch();
					break;
			}

			var restResponse = ConvertToRestResponse(httpResponse);
			return restResponse;
		}

		private void ConfigureProxy(IHttp http)
		{
			if (Proxy != null)
			{
				http.Proxy = Proxy;
			}
		}
	}
}
#endif