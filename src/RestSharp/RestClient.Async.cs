using System.Net;
using System.Net.Http.Headers;
using System.Runtime.Serialization;

namespace RestSharp;

public partial class RestClient {
    /// <summary>
    /// Executes a GET-style request asynchronously, authenticating if needed
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task<RestResponse<T>> ExecuteGetAsync<T>(IRestRequest request, CancellationToken cancellationToken = default)
        => ExecuteAsync<T>(request, Method.Get, cancellationToken);

    /// <summary>
    /// Executes a POST-style request asynchronously, authenticating if needed
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public Task<RestResponse<T>> ExecutePostAsync<T>(IRestRequest request, CancellationToken cancellationToken = default)
        => ExecuteAsync<T>(request, Method.Post, cancellationToken);

    /// <summary>
    /// Executes a GET-style asynchronously, authenticating if needed
    /// </summary>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task<RestResponse> ExecuteGetAsync(IRestRequest request, CancellationToken cancellationToken = default)
        => ExecuteAsync(request, Method.Get, cancellationToken);

    /// <summary>
    /// Executes a POST-style asynchronously, authenticating if needed
    /// </summary>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task<RestResponse> ExecutePostAsync(IRestRequest request, CancellationToken cancellationToken = default)
        => ExecuteAsync(request, Method.Post, cancellationToken);

    /// <summary>
    /// Executes the request asynchronously, authenticating if needed
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<RestResponse<T>> ExecuteAsync<T>(IRestRequest request, CancellationToken cancellationToken = default) {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var response = await ExecuteAsync(request, cancellationToken);
        return Deserialize<T>(request, response);
    }

    /// <summary>
    /// Executes the request asynchronously, authenticating if needed
    /// </summary>
    /// <param name="request">Request to be executed</param>
    /// <param name="httpMethod">Override the request method</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task<RestResponse> ExecuteAsync(
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
    public Task<RestResponse<T>> ExecuteAsync<T>(
        IRestRequest      request,
        Method            httpMethod,
        CancellationToken cancellationToken = default
    ) {
        Ensure.NotNull(request, nameof(request));

        request.Method = httpMethod;
        return ExecuteAsync<T>(request, cancellationToken);
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

    /// <inheritdoc />
    public async Task<RestResponse> ExecuteAsync(IRestRequest request, CancellationToken cancellationToken = default) {
        Ensure.NotNull(request, nameof(request));
        HttpContent? content = null;

        var streams = new List<Stream>();

        if (request.HasFiles() || request.AlwaysMultipartFormData) {
            var mpContent = new MultipartFormDataContent();

            foreach (var file in request.Files) {
                var stream = file.GetFile();
                streams.Add(stream);
                var fileContent = new StreamContent(stream);

                if (file.ContentType != null)
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
                mpContent.Add(fileContent, file.Name, file.FileName);
            }

            content = mpContent;
        }

        var postParameters = request.GetPostParameters();

        if (request.TryGetBodyParameter(out var bodyParameter)) {
            var bodyContent = Serialize(bodyParameter!);

            // we need to send the body
            if (postParameters.Length > 0 || request.Files.Count > 0) {
                // here we must use multipart form data
                var mpContent = content as MultipartFormDataContent ?? new MultipartFormDataContent();
                mpContent.Add(bodyContent);
                content = mpContent;
            }
            else {
                // we don't have parameters, only the body
                content = bodyContent;
            }
        }

        if (postParameters.Length > 0) {
            // it's a form
            if (content is MultipartFormDataContent mpContent) {
                // we got the multipart form already instantiated, just add parameters to it
                foreach (var postParameter in postParameters) {
                    mpContent.Add(new StringContent(postParameter.Value.ToString(), Options.Encoding, postParameter.ContentType), postParameter.Name);
                }
            }
            else {
                // we should not have anything else except the parameters, so we send them as form URL encoded
                var formContent = new FormUrlEncodedContent(
                    request.Parameters
                        .Where(x => x.Type == ParameterType.GetOrPost)
                        .Select(x => new KeyValuePair<string, string>(x.Name!, x.Value!.ToString()))
                );
                content = formContent;
            }
        }

        if (Authenticator != null)
            await Authenticator.Authenticate(this, request);

        var response = new RestResponse();

        var url     = BuildUri(request);
        var message = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        message.Headers.Host         = Options.BaseHost;
        message.Headers.CacheControl = Options.CachePolicy;

        // Add other parameters than the body and files (headers, cookies, URL segments and query)

        var timeoutCts = new CancellationTokenSource(request.Timeout > 0 ? request.Timeout : Int32.MaxValue);
        var cts        = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);
        var ct         = cts.Token;

        try {
            var resp = await HttpClient.SendAsync(message, ct);

            // var http = ConfigureHttp(request);
            // request.OnBeforeRequest?.Invoke(http);

            response = await RestResponse.FromHttpResponse(resp, request, _cookieContainer.GetCookies(url), cancellationToken);
        }
        catch (Exception ex) {
            return ReturnErrorOrThrow(response, ex, timeoutCts.Token);
        }
        finally {
            streams.ForEach(x => x.Dispose());
        }

        response.Request = request;
        response.Request.IncreaseNumAttempts();

        return response;
    }

    StringContent Serialize(Parameter body) {
        if (!Serializers.TryGetValue(body.DataFormat, out var serializer))
            throw new InvalidDataContractException(
                $"Can't find serializer for content type {body.DataFormat}"
            );

        return new StringContent(
            serializer.Serialize(body),
            Options.Encoding,
            body.ContentType ?? serializer.ContentType
        );
    }

    RestResponse ReturnErrorOrThrow(RestResponse response, Exception exception, CancellationToken timeoutToken) {
        if (exception is OperationCanceledException) {
            response.ResponseStatus = timeoutToken.IsCancellationRequested ? ResponseStatus.TimedOut : ResponseStatus.Aborted;
        }
        else {
            response.ResponseStatus = ResponseStatus.Error;
        }

        if (Options.ThrowOnAnyError)
            ThrowIfError(response);

        response.ErrorMessage   = exception.Message;
        response.ErrorException = exception;
        return response;
    }

    static void ThrowIfError(RestResponse response) {
        var exception = response.ResponseStatus switch {
            ResponseStatus.Aborted   => new WebException("Request aborted", response.ErrorException),
            ResponseStatus.Error     => response.ErrorException,
            ResponseStatus.TimedOut  => new TimeoutException("Request timed out", response.ErrorException),
            ResponseStatus.None      => null,
            ResponseStatus.Completed => null,
            _                        => throw response.ErrorException ?? new ArgumentOutOfRangeException(nameof(response.ResponseStatus))
        };

        if (exception != null)
            throw exception;
    }
}