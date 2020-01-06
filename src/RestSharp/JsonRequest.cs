using System;
using System.Collections.Generic;
using System.Net;
using RestSharp.Extensions;

namespace RestSharp
{
    public class JsonRequest<TRequest, TResponse> : RestRequest
    {
        readonly List<Action<IRestResponse<TResponse>>> _changeResponse = new List<Action<IRestResponse<TResponse>>>();

        readonly Dictionary<HttpStatusCode, Func<TResponse>> _customResponses =
            new Dictionary<HttpStatusCode, Func<TResponse>>();

        public JsonRequest(string resource, TRequest request) : base(resource)
        {
            AddJsonBody(request);
            _changeResponse.Add(ApplyCustomResponse);
        }

        public JsonRequest<TRequest, TResponse> ResponseForStatusCode(HttpStatusCode statusCode, TResponse response)
            => this.With(x => _customResponses.Add(statusCode, () => response));

        public JsonRequest<TRequest, TResponse> ResponseForStatusCode(
            HttpStatusCode statusCode,
            Func<TResponse> getResponse
        )
            => this.With(x => _customResponses.Add(statusCode, getResponse));

        public JsonRequest<TRequest, TResponse> ChangeResponse(Action<IRestResponse<TResponse>> change)
            => this.With(x => x._changeResponse.Add(change));

        void ApplyCustomResponse(IRestResponse<TResponse> response)
        {
            if (_customResponses.TryGetValue(response.StatusCode, out var getResponse))
                response.Data = getResponse();
        }

        internal IRestResponse<TResponse> UpdateResponse(IRestResponse<TResponse> response)
        {
            _changeResponse.ForEach(x => x(response));
            return response;
        }
    }
}