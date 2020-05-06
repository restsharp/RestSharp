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
using RestSharp.Validation;

namespace RestSharp
{
    public partial class RestClient
    {
        /// <inheritdoc />
        public byte[] DownloadData(IRestRequest request) => DownloadData(request, false);

        /// <inheritdoc />
        public byte[] DownloadData(IRestRequest request, bool throwOnError)
        {
            var response = Execute(request);

            return response.ResponseStatus == ResponseStatus.Error && throwOnError
                ? throw response.ErrorException
                : response.RawBytes;
        }

        /// <inheritdoc />
        public virtual IRestResponse Execute(IRestRequest request, Method httpMethod)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            request.Method = httpMethod;
            return Execute(request);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public IRestResponse ExecuteAsGet(IRestRequest request, string httpMethod) => Execute(request, httpMethod, DoExecuteAsGet);

        /// <inheritdoc />
        public IRestResponse ExecuteAsPost(IRestRequest request, string httpMethod)
        {
            request.Method = Method.POST; // Required by RestClient.BuildUri... 

            return Execute(request, httpMethod, DoExecuteAsPost);
        }

        /// <inheritdoc />
        public virtual IRestResponse<T> Execute<T>(IRestRequest request, Method httpMethod)
        {
            Ensure.NotNull(request, nameof(request));

            request.Method = httpMethod;
            return Execute<T>(request);
        }

        /// <inheritdoc />
        public virtual IRestResponse<T> Execute<T>(IRestRequest request)
            => Deserialize<T>(request, Execute(request));

        /// <inheritdoc />
        public IRestResponse<T> ExecuteAsGet<T>(IRestRequest request, string httpMethod)
            => Deserialize<T>(request, ExecuteAsGet(request, httpMethod));

        /// <inheritdoc />
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