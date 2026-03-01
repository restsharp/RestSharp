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
        response.MergedParameters = new RequestParameters(request.Parameters.Union(DefaultParameters));
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
        var timedOut = exception is OperationCanceledException && TimedOut();

        var response = new RestResponse(request) {
            ResponseStatus = exception is OperationCanceledException
                ? timedOut ? ResponseStatus.TimedOut : ResponseStatus.Aborted
                : ResponseStatus.Error,
            ErrorMessage   = timedOut ? "The request timed out." : exception.GetBaseException().Message,
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
#if NET
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
            await authenticator.Authenticate(this, request, cancellationToken).ConfigureAwait(false);
        }

        var contentToDispose = new List<RequestContent>();
        var initialContent   = new RequestContent(this, request);
        contentToDispose.Add(initialContent);

        var httpMethod = AsHttpMethod(request.Method);
        var url        = new Uri(this.BuildUriString(request));

        using var timeoutCts = new CancellationTokenSource(request.Timeout ?? Options.Timeout ?? _defaultTimeout);
        using var cts        = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);

        var ct = cts.Token;

        // Make sure we have a cookie container if not provided in the request
        var cookieContainer = request.CookieContainer ??= new();
        AddPendingCookies(cookieContainer, url, request);

        var headers = new RequestHeaders()
            .AddHeaders(request.Parameters)
            .AddHeaders(DefaultParameters)
            .AddAcceptHeader(AcceptedContentTypes)
            .AddCookieHeaders(url, cookieContainer)
            .AddCookieHeaders(url, Options.CookieContainer);

        var message = new HttpRequestMessage(httpMethod, url);
        message.Content              = initialContent.BuildContent();
        message.Headers.Host         = Options.BaseHost;
        message.Headers.CacheControl = request.CachePolicy ?? Options.CachePolicy;
        message.Version              = request.Version;
        message.AddHeaders(headers);

#pragma warning disable CS0618 // Type or member is obsolete
        if (request.OnBeforeRequest != null) await request.OnBeforeRequest(message).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete
        await OnBeforeHttpRequest(request, message, cancellationToken).ConfigureAwait(false);

        var (responseMessage, finalUrl, error) = await SendWithRedirectsAsync(
            message, url, httpMethod, request, cookieContainer, contentToDispose, ct
        ).ConfigureAwait(false);

        DisposeContent(contentToDispose);

        if (error != null) {
            return new(null, finalUrl, null, error, timeoutCts.Token);
        }

#pragma warning disable CS0618 // Type or member is obsolete
        if (request.OnAfterRequest != null) await request.OnAfterRequest(responseMessage!).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete
        await OnAfterHttpRequest(request, responseMessage!, cancellationToken).ConfigureAwait(false);
        return new(responseMessage, finalUrl, cookieContainer, null, timeoutCts.Token);
    }

    async Task<(HttpResponseMessage? Response, Uri FinalUrl, Exception? Error)> SendWithRedirectsAsync(
        HttpRequestMessage message,
        Uri url,
        HttpMethod httpMethod,
        RestRequest request,
        CookieContainer cookieContainer,
        List<RequestContent> contentToDispose,
        CancellationToken ct
    ) {
        var redirectOptions = Options.RedirectOptions;
        var redirectCount   = 0;
        var originalUrl     = url;

        try {
            while (true) {
                var responseMessage = await HttpClient.SendAsync(message, request.CompletionOption, ct).ConfigureAwait(false);

                ParseResponseCookies(responseMessage, url, cookieContainer);

                if (!ShouldFollowRedirect(redirectOptions, responseMessage, redirectCount)) {
                    return (responseMessage, url, null);
                }

                var redirectUrl = ResolveRedirectUrl(url, responseMessage, redirectOptions);

                if (redirectUrl == null) {
                    return (responseMessage, url, null);
                }

                var newMethod        = GetRedirectMethod(httpMethod, responseMessage.StatusCode);
                var verbChangedToGet = newMethod == HttpMethod.Get && httpMethod != HttpMethod.Get;

                responseMessage.Dispose();

                var previousMessage = message;
                url        = redirectUrl;
                httpMethod = newMethod;
                redirectCount++;

                message = CreateRedirectMessage(
                    httpMethod, url, originalUrl, request, redirectOptions, cookieContainer, contentToDispose, verbChangedToGet
                );
                previousMessage.Dispose();
            }
        }
        catch (Exception ex) {
            return (null, url, ex);
        }
        finally {
            message.Dispose();
        }
    }

    static void AddPendingCookies(CookieContainer cookieContainer, Uri url, RestRequest request) {
        foreach (var cookie in request.PendingCookies) {
            try {
                cookieContainer.Add(url, cookie);
            }
            catch (CookieException) {
                // Do not fail request if we cannot parse a cookie
            }
        }

        request.ClearPendingCookies();
    }

    void ParseResponseCookies(HttpResponseMessage responseMessage, Uri url, CookieContainer cookieContainer) {
        if (!responseMessage.Headers.TryGetValues(KnownHeaders.SetCookie, out var cookiesHeader)) return;

        // ReSharper disable once PossibleMultipleEnumeration
        cookieContainer.AddCookies(url, cookiesHeader);
        // ReSharper disable once PossibleMultipleEnumeration
        Options.CookieContainer?.AddCookies(url, cookiesHeader);
    }

    static bool ShouldFollowRedirect(RedirectOptions options, HttpResponseMessage response, int redirectCount)
        => options.FollowRedirects
            && options.RedirectStatusCodes.Contains(response.StatusCode)
            && response.Headers.Location != null
            && redirectCount < options.MaxRedirects;

    static Uri? ResolveRedirectUrl(Uri currentUrl, HttpResponseMessage response, RedirectOptions options) {
        var location    = response.Headers.Location!;
        var redirectUrl = location.IsAbsoluteUri ? location : new Uri(currentUrl, location);

        if (options.ForwardQuery && string.IsNullOrEmpty(redirectUrl.Query) && !string.IsNullOrEmpty(currentUrl.Query)) {
            var builder = new UriBuilder(redirectUrl) { Query = currentUrl.Query.TrimStart('?') };
            redirectUrl = builder.Uri;
        }

        // Block HTTPS -> HTTP unless explicitly allowed
        if (currentUrl.Scheme == "https" && redirectUrl.Scheme == "http" && !options.FollowRedirectsToInsecure) {
            return null;
        }

        return redirectUrl;
    }

    HttpRequestMessage CreateRedirectMessage(
        HttpMethod httpMethod,
        Uri url,
        Uri originalUrl,
        RestRequest request,
        RedirectOptions redirectOptions,
        CookieContainer cookieContainer,
        List<RequestContent> contentToDispose,
        bool verbChangedToGet
    ) {
        var redirectMessage = new HttpRequestMessage(httpMethod, url);
        redirectMessage.Version              = request.Version;
        redirectMessage.Headers.Host         = Options.BaseHost;
        redirectMessage.Headers.CacheControl = request.CachePolicy ?? Options.CachePolicy;

        if (!verbChangedToGet && redirectOptions.ForwardBody) {
            var redirectContent = new RequestContent(this, request);
            contentToDispose.Add(redirectContent);
            redirectMessage.Content = redirectContent.BuildContent();
        }

        var redirectHeaders = BuildRedirectHeaders(url, originalUrl, redirectOptions, request, cookieContainer);
        redirectMessage.AddHeaders(redirectHeaders);

        return redirectMessage;
    }

    RequestHeaders BuildRedirectHeaders(
        Uri url, Uri originalUrl, RedirectOptions redirectOptions, RestRequest request, CookieContainer cookieContainer
    ) {
        var redirectHeaders = new RequestHeaders();

        if (redirectOptions.ForwardHeaders) {
            redirectHeaders
                .AddHeaders(request.Parameters)
                .AddHeaders(DefaultParameters)
                .AddAcceptHeader(AcceptedContentTypes);

            if (!ShouldForwardAuthorization(url, originalUrl, redirectOptions)) {
                redirectHeaders.RemoveHeader(KnownHeaders.Authorization);
            }
        }
        else {
            redirectHeaders.AddAcceptHeader(AcceptedContentTypes);
        }

        // Always remove existing Cookie headers before adding fresh ones from the container
        redirectHeaders.RemoveHeader(KnownHeaders.Cookie);

        if (redirectOptions.ForwardCookies) {
            redirectHeaders
                .AddCookieHeaders(url, cookieContainer)
                .AddCookieHeaders(url, Options.CookieContainer);
        }

        return redirectHeaders;
    }

    static bool ShouldForwardAuthorization(Uri redirectUrl, Uri originalUrl, RedirectOptions options) {
        if (!options.ForwardAuthorization) return false;

        // Never forward credentials from HTTPS to HTTP (they would be sent in plaintext)
        if (originalUrl.Scheme == "https" && redirectUrl.Scheme == "http") return false;

        // Compare full authority (host + port) to match browser same-origin policy
        var isSameOrigin = string.Equals(redirectUrl.Authority, originalUrl.Authority, StringComparison.OrdinalIgnoreCase);

        return isSameOrigin || options.ForwardAuthorizationToExternalHost;
    }

    static HttpMethod GetRedirectMethod(HttpMethod originalMethod, HttpStatusCode statusCode) {
        // 307 and 308: always preserve the original method
        if (statusCode is HttpStatusCode.TemporaryRedirect or (HttpStatusCode)308) {
            return originalMethod;
        }

        // 303: all methods except GET and HEAD become GET
        if (statusCode == HttpStatusCode.SeeOther) {
            return originalMethod == HttpMethod.Get || originalMethod == HttpMethod.Head
                ? originalMethod
                : HttpMethod.Get;
        }

        // 301 and 302: POST becomes GET (matches browser/HttpClient behavior), others preserved
        return originalMethod == HttpMethod.Post ? HttpMethod.Get : originalMethod;
    }

    static void DisposeContent(List<RequestContent> contentList) {
        foreach (var content in contentList) {
            content.Dispose();
        }
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
