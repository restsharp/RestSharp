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
		public byte[] DownloadData(RestRequest request)
		{
			var response = Execute(request);
			return response.RawBytes;
		}

		/// <summary>
		/// Executes the request and returns a response, authenticating if needed
		/// </summary>
		/// <param name="request">Request to be executed</param>
		/// <returns>RestResponse</returns>
		public RestResponse Execute(RestRequest request)
		{
			AuthenticateIfNeeded(request);

			// add Accept header
			var accepts = string.Join(", ", AcceptTypes.ToArray());
			request.AddParameter("Accept", accepts, ParameterType.HttpHeader);

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
			}

			return response;
		}

		/// <summary>
		/// Executes the specified request and deserializes the response content using the appropriate content handler
		/// </summary>
		/// <typeparam name="T">Target deserialization type</typeparam>
		/// <param name="request">Request to execute</param>
		/// <returns>RestResponse[[T]] with deserialized data in Data property</returns>
		public RestResponse<T> Execute<T>(RestRequest request) where T : new()
		{
			var raw = Execute(request);
			return Deserialize<T>(request, raw);
		}
		
		private RestResponse GetResponse(RestRequest request)
		{
			IHttp http = new Http();
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