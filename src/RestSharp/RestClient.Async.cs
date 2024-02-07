﻿//  Copyright (c) .NET Foundation and Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Net;
using RestSharp.Extensions;

namespace RestSharp;

public partial class RestClient {
    /// <inheritdoc />
    public async Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default) {
        using var internalResponse = await ExecuteRequestAsync(request, cancellationToken).ConfigureAwait(false);

        var response = internalResponse.Exception == null
            ? await RestResponse.FromHttpResponse(
                    internalResponse.ResponseMessage!,
                    request,
                    Options.Encoding,
                    internalResponse.CookieContainer?.GetCookies(internalResponse.Url),
                    Options.CalculateResponseStatus,
                    cancellationToken
                )
                .ConfigureAwait(false)
            : GetErrorResponse(request, internalResponse.Exception, internalResponse.TimeoutToken);

        return Options.ThrowOnAnyError ? response.ThrowIfError() : response;
    }

    /// <inheritdoc />
    [PublicAPI]
    public async Task<Stream?> DownloadStreamAsync(RestRequest request, CancellationToken cancellationToken = default) {
        return await DownloadStreamInternalAsync(request, null, cancellationToken);
    }

    /// <inheritdoc />
    [PublicAPI]
    public async Task<Stream?> DownloadStreamAsync(RestRequest request, Action<RestResponse> errorHandler, CancellationToken cancellationToken = default) {
        return await DownloadStreamInternalAsync(request, errorHandler, cancellationToken);
    }

    async Task<Stream?> DownloadStreamInternalAsync(RestRequest request, Action<RestResponse>? errorHandler, CancellationToken cancellationToken = default) {
        // Make sure we only read the headers so we can stream the content body efficiently
        request.CompletionOption = HttpCompletionOption.ResponseHeadersRead;
        var internalResponse = await ExecuteRequestAsync(request, cancellationToken).ConfigureAwait(false);

        if (errorHandler != null) {
            if (internalResponse.Exception != null) {
                var response = GetErrorResponse(request, internalResponse.Exception, internalResponse.TimeoutToken);

                errorHandler(response);
            }
            else if (internalResponse.ResponseMessage!.IsSuccessStatusCode == false) {
                var response = await RestResponse.FromHttpResponse(
                    internalResponse.ResponseMessage!,
                    request,
                    Options.Encoding,
                    internalResponse.CookieContainer?.GetCookies(internalResponse.Url),
                    Options.CalculateResponseStatus,
                    cancellationToken
                )
                .ConfigureAwait(false);

                errorHandler(response);
            }
        }

        var exception = internalResponse.Exception ?? internalResponse.ResponseMessage?.MaybeException();

        if (exception != null) {
            return Options.ThrowOnAnyError ? throw exception : null;
        }

        if (internalResponse.ResponseMessage == null) return null;

        return await internalResponse.ResponseMessage.ReadResponseStream(request.ResponseWriter, cancellationToken).ConfigureAwait(false);
    }

    static RestResponse GetErrorResponse(RestRequest request, Exception exception, CancellationToken timeoutToken) {
        var response = new RestResponse(request) {
            ResponseStatus = exception is OperationCanceledException
                ? TimedOut() ? ResponseStatus.TimedOut : ResponseStatus.Aborted
                : ResponseStatus.Error,
            ErrorMessage   = exception.Message,
            ErrorException = exception
        };

        return response;

        bool TimedOut() => timeoutToken.IsCancellationRequested || exception.Message.Contains("HttpClient.Timeout");
    }

    async Task<HttpResponse> ExecuteRequestAsync(RestRequest request, CancellationToken cancellationToken) {
        Ensure.NotNull(request, nameof(request));

        // Make sure we are not disposed of when someone tries to call us!
        if (_disposed) {
            throw new ObjectDisposedException(nameof(RestClient));
        }

        await OnBeforeSerialization(request).ConfigureAwait(false);   
        request.ValidateParameters();
        var authenticator = request.Authenticator ?? Options.Authenticator;

        if (authenticator != null) {
            await authenticator.Authenticate(this, request).ConfigureAwait(false);
        }

        using var requestContent = new RequestContent(this, request);

        var httpMethod = AsHttpMethod(request.Method);
        var url        = this.BuildUri(request);

        using var message    = new HttpRequestMessage(httpMethod, url) { Content = requestContent.BuildContent() };
        message.Headers.Host         = Options.BaseHost;
        message.Headers.CacheControl = request.CachePolicy ?? Options.CachePolicy;

        using var timeoutCts = new CancellationTokenSource(request.Timeout > 0 ? request.Timeout : int.MaxValue);
        using var cts        = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);

        var ct = cts.Token;

        
        HttpResponseMessage? responseMessage;
        // Make sure we have a cookie container if not provided in the request
        CookieContainer cookieContainer = request.CookieContainer ??= new CookieContainer();
        
        var headers = new RequestHeaders()
            .AddHeaders(request.Parameters)
            .AddHeaders(DefaultParameters)
            .AddAcceptHeader(AcceptedContentTypes)
            .AddCookieHeaders(url, cookieContainer)
            .AddCookieHeaders(url, Options.CookieContainer);

        message.AddHeaders(headers);
        if (request.OnBeforeRequest != null) await request.OnBeforeRequest(message).ConfigureAwait(false);
        await OnBeforeRequest(message).ConfigureAwait(false);
        
        try {
            responseMessage = await HttpClient.SendAsync(message, request.CompletionOption, ct).ConfigureAwait(false);
            // Parse all the cookies from the response and update the cookie jar with cookies
            if (responseMessage.Headers.TryGetValues(KnownHeaders.SetCookie, out var cookiesHeader)) {
                // ReSharper disable once PossibleMultipleEnumeration
                cookieContainer.AddCookies(url, cookiesHeader);
                // ReSharper disable once PossibleMultipleEnumeration
                Options.CookieContainer?.AddCookies(url, cookiesHeader);
            }
        }
        catch (Exception ex) {
            return new HttpResponse(null, url, null, ex, timeoutCts.Token);
        }
        if (request.OnAfterRequest != null) await request.OnAfterRequest(responseMessage).ConfigureAwait(false);
        await OnAfterRequest(responseMessage).ConfigureAwait(false);
        return new HttpResponse(responseMessage, url, cookieContainer, null, timeoutCts.Token);
        
    }

    /// <summary>
    /// Will be called before the Request becomes Serialized
    /// </summary>
    /// <param name="request">RestRequest before it will be serialized</param>
    async Task OnBeforeSerialization(RestRequest request) {
        foreach (var interceptor in Options.Interceptors) {
            await interceptor.InterceptBeforeSerialization(request); //.ThrowExceptionIfAvailable();
        }
    }
    /// <summary>
    /// Will be called before the Request will be sent
    /// </summary>
    /// <param name="requestMessage">HttpRequestMessage ready to be sent</param>
    async Task OnBeforeRequest(HttpRequestMessage requestMessage) {
        foreach (var interceptor in Options.Interceptors) {
            await interceptor.InterceptBeforeRequest(requestMessage);
        }
    }
    /// <summary>
    /// Will be called after the Response has been received from Server
    /// </summary>
    /// <param name="responseMessage">HttpResponseMessage as received from server</param>
    async Task OnAfterRequest(HttpResponseMessage responseMessage) {
        foreach (var interceptor in Options.Interceptors) {
            await interceptor.InterceptAfterRequest(responseMessage);
        }
    }

    record HttpResponse(
        HttpResponseMessage? ResponseMessage,
        Uri                  Url,
        CookieContainer?     CookieContainer,
        Exception?           Exception,
        CancellationToken    TimeoutToken
    ) : IDisposable {
        public void Dispose() => ResponseMessage?.Dispose();
    }

    static HttpMethod AsHttpMethod(Method method)
        => method switch {
            Method.Get     => HttpMethod.Get,
            Method.Post    => HttpMethod.Post,
            Method.Put     => HttpMethod.Put,
            Method.Delete  => HttpMethod.Delete,
            Method.Head    => HttpMethod.Head,
            Method.Options => HttpMethod.Options,
#if NET
            Method.Patch => HttpMethod.Patch,
#else
            Method.Patch => new HttpMethod("PATCH"),
#endif
            Method.Merge  => new HttpMethod("MERGE"),
            Method.Copy   => new HttpMethod("COPY"),
            Method.Search => new HttpMethod("SEARCH"),
            _             => throw new ArgumentOutOfRangeException(nameof(method))
        };
}
