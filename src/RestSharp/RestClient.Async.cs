using System.Net;

namespace RestSharp; 

public partial class RestClient {
    /// <summary>
    /// Executes a GET-style request asynchronously, authenticating if needed
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task<IRestResponse<T>> ExecuteGetAsync<T>(IRestRequest request, CancellationToken cancellationToken = default)
        => ExecuteAsync<T>(request, Method.Get, cancellationToken);

    /// <summary>
    /// Executes a POST-style request asynchronously, authenticating if needed
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public Task<IRestResponse<T>> ExecutePostAsync<T>(IRestRequest request, CancellationToken cancellationToken = default)
        => ExecuteAsync<T>(request, Method.Post, cancellationToken);

    /// <summary>
    /// Executes a GET-style asynchronously, authenticating if needed
    /// </summary>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task<IRestResponse> ExecuteGetAsync(IRestRequest request, CancellationToken cancellationToken = default)
        => ExecuteAsync(request, Method.Get, cancellationToken);

    /// <summary>
    /// Executes a POST-style asynchronously, authenticating if needed
    /// </summary>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task<IRestResponse> ExecutePostAsync(IRestRequest request, CancellationToken cancellationToken = default)
        => ExecuteAsync(request, Method.Post, cancellationToken);

    /// <summary>
    /// Executes the request asynchronously, authenticating if needed
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<IRestResponse<T>> ExecuteAsync<T>(IRestRequest request, CancellationToken cancellationToken = default) {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var taskCompletionSource = new TaskCompletionSource<IRestResponse<T>>();

        try {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, BaseUrl);

            var body = request.Parameters.FirstOrDefault(x => x.Type == ParameterType.RequestBody);
            
            requestMessage.Content = new StringContent()
            var response = await HttpClient.SendAsync(requestMessage, CancellationToken.None);
            // var async = ExecuteAsync<T>(
            //     request,
            //     (response, _) => {
            //         if (cancellationToken.IsCancellationRequested)
            //             taskCompletionSource.TrySetCanceled();
            //         // Don't run TrySetException, since we should set Error properties and swallow exceptions
            //         // to be consistent with sync methods
            //         else
            //             taskCompletionSource.TrySetResult(response);
            //     }
            // );

            // var registration =
            //     cancellationToken.Register(
            //         () => {
            //             async.Abort();
            //             taskCompletionSource.TrySetCanceled();
            //         }
            //     );

            // taskCompletionSource.Task.ContinueWith(t => registration.Dispose(), cancellationToken);
        }
        catch (Exception ex) {
            taskCompletionSource.TrySetException(ex);
        }
    }

    /// <summary>
    /// Executes the request asynchronously, authenticating if needed
    /// </summary>
    /// <param name="request">Request to be executed</param>
    /// <param name="httpMethod">Override the request method</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task<IRestResponse> ExecuteAsync(
        IRestRequest      request,
        Method            httpMethod,
        CancellationToken cancellationToken = default
    ) {
        Ensure.NotNull(request, nameof(request));

        request.Method = httpMethod;
        return ExecuteAsync(request, cancellationToken);
    }

    /// <summary>
    /// Executes the request asynchronously, authenticating if needed
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="request">Request to be executed</param>
    /// <param name="httpMethod">Override the request method</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task<IRestResponse<T>> ExecuteAsync<T>(
        IRestRequest      request,
        Method            httpMethod,
        CancellationToken cancellationToken = default
    ) {
        Ensure.NotNull(request, nameof(request));

        request.Method = httpMethod;
        return ExecuteAsync<T>(request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IRestResponse> ExecuteAsync(IRestRequest request, CancellationToken token = default) {
        Ensure.NotNull(request, nameof(request));

        var taskCompletionSource = new TaskCompletionSource<IRestResponse>();

        try {
            // var async = ExecuteAsync(
            //     request,
            //     (response, _) => {
            //         if (token.IsCancellationRequested)
            //             taskCompletionSource.TrySetCanceled();
            //         // Don't run TrySetException, since we should set Error
            //         // properties and swallow exceptions to be consistent
            //         // with sync methods
            //         else
            //             taskCompletionSource.TrySetResult(response);
            //     }
            // );
            //
            // var registration =
            //     token.Register(
            //         () => {
            //             async.Abort();
            //             taskCompletionSource.TrySetCanceled();
            //         }
            //     );
            //
            // taskCompletionSource.Task.ContinueWith(t => registration.Dispose(), token);
        }
        catch (Exception ex) {
            taskCompletionSource.TrySetException(ex);
        }

        return taskCompletionSource.Task;
    }

    // RestRequestAsyncHandle ExecuteAsync(
    //     IRestRequest                                              request,
    //     Action<IRestResponse, RestRequestAsyncHandle>             callback,
    //     string                                                    httpMethod,
    //     Func<IHttp, Action<HttpResponse>, string, HttpWebRequest> getWebRequest
    // ) {
    //     request.SerializeRequestBody(Serializers, request.XmlSerializer, request.JsonSerializer);
    //
    //     AuthenticateIfNeeded(request);
    //
    //     var http = ConfigureHttp(request);
    //
    //     request.OnBeforeRequest?.Invoke(http);
    //
    //     var asyncHandle = new RestRequestAsyncHandle();
    //
    //     Action<HttpResponse> responseCb = ProcessResponse;
    //
    //     if (UseSynchronizationContext && SynchronizationContext.Current != null) {
    //         var ctx = SynchronizationContext.Current;
    //         var cb  = responseCb;
    //
    //         responseCb = resp => ctx.Post(s => cb(resp), null);
    //     }
    //
    //     asyncHandle.WebRequest = getWebRequest(http, responseCb, httpMethod);
    //
    //     return asyncHandle;
    //
    //     void ProcessResponse(IHttpResponse httpResponse) {
    //         var restResponse = RestResponse.FromHttpResponse(httpResponse, request);
    //         restResponse.Request.IncreaseNumAttempts();
    //         callback(restResponse, asyncHandle);
    //     }
    // }

    // void DeserializeResponse<T>(
    //     IRestRequest                                     request,
    //     Action<IRestResponse<T>, RestRequestAsyncHandle> callback,
    //     IRestResponse                                    response,
    //     RestRequestAsyncHandle                           asyncHandle
    // )
    //     => callback(Deserialize<T>(request, response), asyncHandle);
    
    static void ThrowIfError(IRestResponse response) {
        var exception = response.ResponseStatus switch {
            ResponseStatus.Aborted   => new WebException("Request aborted", response.ErrorException),
            ResponseStatus.Error     => response.ErrorException,
            ResponseStatus.TimedOut  => new TimeoutException("Request timed out", response.ErrorException),
            ResponseStatus.None      => null,
            ResponseStatus.Completed => null,
            _                        => throw response.ErrorException ?? new ArgumentOutOfRangeException()
        };

        if (exception != null)
            throw exception;
    }
}