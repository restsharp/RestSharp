using System;
using RestSharp.Validation;

namespace RestSharp
{
    public partial class RestClient
    {
        /// <summary>
        ///     Executes the specified request and downloads the response data
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <returns>Response data</returns>
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
        public byte[] DownloadData(IRestRequest request) => DownloadData(request, false);

        /// <summary>
        ///     Executes the specified request and downloads the response data
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <param name="throwOnError">Throw an exception if download fails.</param>
        /// <returns>Response data</returns>
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
        public byte[] DownloadData(IRestRequest request, bool throwOnError)
        {
            var response = Execute(request);

            return response.ResponseStatus == ResponseStatus.Error && throwOnError
                ? throw response.ErrorException
                : response.RawBytes;
        }

        /// <summary>
        ///     Executes the request and returns a response, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <param name="httpMethod">Override the http method in the request</param>
        /// <returns>RestResponse</returns>
        public virtual IRestResponse Execute(IRestRequest request, Method httpMethod)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            request.Method = httpMethod;
            return Execute(request);
        }

        /// <summary>
        ///     Executes the request and returns a response, authenticating if needed
        /// </summary>
        /// <param name="request">Request to be executed</param>
        /// <returns>RestResponse</returns>
        public virtual IRestResponse Execute(IRestRequest request)
        {
            var method = Enum.GetName(typeof(Method), request.Method);

            return request.Method switch
            {
                Method.COPY  => Execute(request, method, DoExecuteAsPost),
                Method.POST  => Execute(request, method, DoExecuteAsPost),
                Method.PUT   => Execute(request, method, DoExecuteAsPost),
                Method.PATCH => Execute(request, method, DoExecuteAsPost),
                Method.MERGE => Execute(request, method, DoExecuteAsPost),
                _            => Execute(request, method, DoExecuteAsGet)
            };
        }

        public IRestResponse ExecuteAsGet(IRestRequest request, string httpMethod) => Execute(request, httpMethod, DoExecuteAsGet);

        public IRestResponse ExecuteAsPost(IRestRequest request, string httpMethod)
        {
            request.Method = Method.POST; // Required by RestClient.BuildUri... 

            return Execute(request, httpMethod, DoExecuteAsPost);
        }

        public virtual IRestResponse<T> Execute<T>(IRestRequest request, Method httpMethod)
        {
            Ensure.NotNull(request, nameof(request));

            request.Method = httpMethod;
            return Execute<T>(request);
        }

        /// <summary>
        ///     Executes the specified request and deserializes the response content using the appropriate content handler
        /// </summary>
        /// <typeparam name="T">Target deserialization type</typeparam>
        /// <param name="request">Request to execute</param>
        /// <returns>RestResponse[[T]] with deserialized data in Data property</returns>
        public virtual IRestResponse<T> Execute<T>(IRestRequest request)
            => Deserialize<T>(request, Execute(request));

        public IRestResponse<T> ExecuteAsGet<T>(IRestRequest request, string httpMethod)
            => Deserialize<T>(request, ExecuteAsGet(request, httpMethod));

        public IRestResponse<T> ExecuteAsPost<T>(IRestRequest request, string httpMethod)
            => Deserialize<T>(request, ExecuteAsPost(request, httpMethod));

        IRestResponse Execute(
            IRestRequest request,
            string httpMethod,
            Func<IHttp, string, HttpResponse> getResponse
        )
        {
            request.SerializeRequestBody(Serializers, request.XmlSerializer, request.JsonSerializer);
            
            AuthenticateIfNeeded(request);

            IRestResponse response = new RestResponse();

            try
            {
                var http = ConfigureHttp(request);
                
                request.OnBeforeRequest?.Invoke(http);

                response = RestResponse.FromHttpResponse(getResponse(http, httpMethod), request);
            }
            catch (Exception ex)
            {
                if (ThrowOnAnyError) throw;
                
                response.ResponseStatus = ResponseStatus.Error;
                response.ErrorMessage   = ex.Message;
                response.ErrorException = ex;
            }

            response.Request = request;
            response.Request.IncreaseNumAttempts();

            return response;
        }

        static HttpResponse DoExecuteAsGet(IHttp http, string method) => http.AsGet(method);

        static HttpResponse DoExecuteAsPost(IHttp http, string method) => http.AsPost(method);
    }
}