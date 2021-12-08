namespace RestSharp; 

public partial class RestClient {
    /// <inheritdoc />
    public byte[] DownloadData(IRestRequest request) {
        var response = Execute(request);

        return response.ResponseStatus == ResponseStatus.Error && Options.ThrowOnAnyError
            ? throw response.ErrorException!
            : response.RawBytes;
    }


    /// <inheritdoc />
    public virtual IRestResponse Execute(IRestRequest request) {
        var method = Enum.GetName(typeof(Method), request.Method);

        return request.Method switch {
            Method.Copy  => Execute(request, method, DoExecuteAsPost),
            Method.Post  => Execute(request, method, DoExecuteAsPost),
            Method.Put   => Execute(request, method, DoExecuteAsPost),
            Method.Patch => Execute(request, method, DoExecuteAsPost),
            Method.Merge => Execute(request, method, DoExecuteAsPost),
            _            => Execute(request, method, DoExecuteAsGet)
        };
    }

    /// <inheritdoc />
    public IRestResponse ExecuteAsGet(IRestRequest request, string httpMethod) => Execute(request, httpMethod, DoExecuteAsGet);

    /// <inheritdoc />
    public IRestResponse ExecuteAsPost(IRestRequest request, string httpMethod) {
        request.Method = Method.Post; // Required by RestClient.BuildUri... 

        return Execute(request, httpMethod, DoExecuteAsPost);
    }

    /// <inheritdoc />
    public virtual IRestResponse<T> Execute<T>(IRestRequest request, Method httpMethod) {
        Ensure.NotNull(request, nameof(request)).Method = httpMethod;
        return Execute<T>(request);
    }

    /// <inheritdoc />
    public virtual IRestResponse<T> Execute<T>(IRestRequest request) => Deserialize<T>(request, Execute(request));

    /// <inheritdoc />
    public IRestResponse<T> ExecuteAsGet<T>(IRestRequest request, string httpMethod) => Deserialize<T>(request, ExecuteAsGet(request, httpMethod));

    /// <inheritdoc />
    public IRestResponse<T> ExecuteAsPost<T>(IRestRequest request, string httpMethod) => Deserialize<T>(request, ExecuteAsPost(request, httpMethod));

    IRestResponse Execute(
        IRestRequest                      request,
        string                            httpMethod,
        Func<Http, string, HttpResponse> getResponse
    ) {
        request.SerializeRequestBody(Serializers);

        AuthenticateIfNeeded(request);

        IRestResponse response = new RestResponse();

        try {
            var http = ConfigureHttp(request);

            request.OnBeforeRequest?.Invoke(http);

            response = RestResponse.FromHttpResponse(getResponse(http, httpMethod), request);
        }
        catch (Exception ex) {
            if (ThrowOnAnyError) throw;

            response.ResponseStatus = ResponseStatus.Error;
            response.ErrorMessage   = ex.Message;
            response.ErrorException = ex;
        }

        response.Request = request;
        response.Request.IncreaseNumAttempts();

        return response;
    }

    static HttpResponse DoExecuteAsGet(Http http, string method) => http.AsGet(method);

    static HttpResponse DoExecuteAsPost(Http http, string method) => http.AsPost(method);
}