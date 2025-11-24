//  Copyright (c) .NET Foundation and Contributors
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

using RestSharp.Extensions;

// ReSharper disable PossiblyMistakenUseOfCancellationToken

namespace RestSharp;

public partial class RestClient {
    // Default HttpClient timeout 
    readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(100);

    /// <inheritdoc />
    public async Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default) {
        using var internalResponse = await ExecuteRequestAsync(request, cancellationToken).ConfigureAwait(false);

        var response = internalResponse.Exception == null
            ? await RestResponse.FromHttpResponse(
                    internalResponse.ResponseMessage!,
                    request,
                    Options,
                    internalResponse.CookieContainer?.GetCookies(internalResponse.Url),
                    cancellationToken
                )
                .ConfigureAwait(false)
            : GetErrorResponse(request, internalResponse.Exception, internalResponse.TimeoutToken);
        await OnAfterRequest(response, cancellationToken).ConfigureAwait(false);

        return Options.ThrowOnAnyError ? response.ThrowIfError() : response;
    }

    /// <inheritdoc />
    [PublicAPI]
    public async Task<Stream?> DownloadStreamAsync(RestRequest request, CancellationToken cancellationToken = default) {
        // Make sure we only read the headers, so we can stream the content body efficiently
        request.CompletionOption = HttpCompletionOption.ResponseHeadersRead;
        var response = await ExecuteRequestAsync(request, cancellationToken).ConfigureAwait(false);

        var exception = response.Exception ?? response.ResponseMessage?.MaybeException(Options.SetErrorExceptionOnUnsuccessfulStatusCode);

        if (exception != null) {
            return Options.ThrowOnAnyError ? throw exception : null;
        }

        if (response.ResponseMessage == null) return null;

        return await response.ResponseMessage.ReadResponseStream(request.ResponseWriter, cancellationToken).ConfigureAwait(false);
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

    void CombineInterceptors(RestRequest request) {
        if (request.Interceptors == null) {
            if (Options.Interceptors == null) {
                return;
            }

            request.Interceptors = Options.Interceptors.ToList();
            return;
        }

        if (Options.Interceptors != null) {
            request.Interceptors.AddRange(Options.Interceptors);
        }
    }

    async Task<HttpResponse> ExecuteRequestAsync(RestRequest request, CancellationToken cancellationToken) {
        Ensure.NotNull(request, nameof(request));

        // Make sure we are not disposed of when someone tries to call us!
#if NET8_0_OR_GREATER
        ObjectDisposedException.ThrowIf(_disposed, this);
#else
        if (_disposed) {
            throw new ObjectDisposedException(nameof(RestClient));
        }
#endif
        CombineInterceptors(request);
        await OnBeforeRequest(request, cancellationToken).ConfigureAwait(false);
        request.ValidateParameters();
        var authenticator = request.Authenticator ?? Options.Authenticator;

        if (authenticator != null) {
            await authenticator.Authenticate(this, request).ConfigureAwait(false);
        }

        using var requestContent = new RequestContent(this, request);

        var httpMethod = AsHttpMethod(request.Method);
        var url        = this.BuildUri(request);

        using var message = new HttpRequestMessage(httpMethod, url);
        message.Content              = requestContent.BuildContent();
        message.Headers.Host         = Options.BaseHost;
        message.Headers.CacheControl = request.CachePolicy ?? Options.CachePolicy;
        message.Version              = request.Version;

        using var timeoutCts = new CancellationTokenSource(request.Timeout ?? Options.Timeout ?? _defaultTimeout);
        using var cts        = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);

        var ct = cts.Token;

        HttpResponseMessage? responseMessage;
        // Make sure we have a cookie container if not provided in the request
        var cookieContainer = request.CookieContainer ??= new();

        var headers = new RequestHeaders()
            .AddHeaders(request.Parameters)
            .AddHeaders(DefaultParameters)
            .AddAcceptHeader(AcceptedContentTypes)
            .AddCookieHeaders(url, cookieContainer)
            .AddCookieHeaders(url, Options.CookieContainer);

        message.AddHeaders(headers);
#pragma warning disable CS0618 // Type or member is obsolete
        if (request.OnBeforeRequest != null) await request.OnBeforeRequest(message).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete
        await OnBeforeHttpRequest(request, message, cancellationToken).ConfigureAwait(false);

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
            return new(null, url, null, ex, timeoutCts.Token);
        }

#pragma warning disable CS0618 // Type or member is obsolete
        if (request.OnAfterRequest != null) await request.OnAfterRequest(responseMessage).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete
        await OnAfterHttpRequest(request, responseMessage, cancellationToken).ConfigureAwait(false);
        return new(responseMessage, url, cookieContainer, null, timeoutCts.Token);
    }

    static async ValueTask OnBeforeRequest(RestRequest request, CancellationToken cancellationToken) {
        if (request.Interceptors == null) return;

        foreach (var interceptor in request.Interceptors) {
            await interceptor.BeforeRequest(request, cancellationToken).ConfigureAwait(false);
        }
    }

    static async ValueTask OnBeforeHttpRequest(RestRequest request, HttpRequestMessage requestMessage, CancellationToken cancellationToken) {
        if (request.Interceptors == null) return;

        foreach (var interceptor in request.Interceptors) {
            await interceptor.BeforeHttpRequest(requestMessage, cancellationToken).ConfigureAwait(false);
        }
    }

    static async ValueTask OnAfterHttpRequest(RestRequest request, HttpResponseMessage responseMessage, CancellationToken cancellationToken) {
        if (request.Interceptors == null) return;

        foreach (var interceptor in request.Interceptors) {
            await interceptor.AfterHttpRequest(responseMessage, cancellationToken).ConfigureAwait(false);
        }
    }

    static async ValueTask OnAfterRequest(RestResponse response, CancellationToken cancellationToken) {
        if (response.Request.Interceptors == null) return;

        foreach (var interceptor in response.Request.Interceptors) {
            await interceptor.AfterRequest(response, cancellationToken).ConfigureAwait(false);
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

    internal static HttpMethod AsHttpMethod(Method method)
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
            Method.Patch => new("PATCH"),
#endif
            Method.Merge  => new("MERGE"),
            Method.Copy   => new("COPY"),
            Method.Search => new("SEARCH"),
            _             => throw new ArgumentOutOfRangeException(nameof(method))
        };
}