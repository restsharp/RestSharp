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
        // Make sure we only read the headers so we can stream the content body efficiently
        request.CompletionOption = HttpCompletionOption.ResponseHeadersRead;
        var response = await ExecuteRequestAsync(request, cancellationToken).ConfigureAwait(false);

        var exception = response.Exception ?? response.ResponseMessage?.MaybeException();

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

        var httpMethod = AsHttpMethod(request.Method);
        var url        = this.BuildUri(request);
        var originalUrl = url;

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
            // Make sure we have a cookie container if not provided in the request
            var cookieContainer = request.CookieContainer ??= new CookieContainer();

            var headers = new RequestHeaders()
                .AddHeaders(request.Parameters)
                .AddHeaders(DefaultParameters)
                .AddAcceptHeader(AcceptedContentTypes)
                .AddCookieHeaders(url, cookieContainer)
                .AddCookieHeaders(url, Options.CookieContainer);

            bool foundCookies = false;
            HttpResponseMessage? responseMessage = null;

            do {
                using var requestContent = new RequestContent(this, request);
                using var content = requestContent.BuildContent();

                // If we found coookies during a redirect,
                // we need to update the Cookie headers:
                if (foundCookies) {
                    headers.AddCookieHeaders(url, cookieContainer);
                }
                using var message = PrepareRequestMessage(httpMethod, url, content, headers);

                if (request.OnBeforeRequest != null) await request.OnBeforeRequest(message).ConfigureAwait(false);

                responseMessage = await HttpClient.SendAsync(message, request.CompletionOption, ct).ConfigureAwait(false);

                if (request.OnAfterRequest != null) await request.OnAfterRequest(responseMessage).ConfigureAwait(false);

                if (!IsRedirect(responseMessage)) {
                    // || !Options.FollowRedirects) {
                    break;
                }

                var location = responseMessage.Headers.Location;

                if (location == null) {
                    break;
                }

                if (!location.IsAbsoluteUri) {
                    location = new Uri(url, location);
                }

                // Mirror HttpClient redirection behavior as of 07/25/2023:
                // Per https://tools.ietf.org/html/rfc7231#section-7.1.2, a redirect location without a
                // fragment should inherit the fragment from the original URI.
                string requestFragment = originalUrl.Fragment;
                if (!string.IsNullOrEmpty(requestFragment)) {
                    string redirectFragment = location.Fragment;
                    if (string.IsNullOrEmpty(redirectFragment)) {
                        location = new UriBuilder(location) { Fragment = requestFragment }.Uri;
                    }
                }

                // Disallow automatic redirection from secure to non-secure schemes
                // From HttpClient's RedirectHandler:
                //if (HttpUtilities.IsSupportedSecureScheme(requestUri.Scheme) && !HttpUtilities.IsSupportedSecureScheme(location.Scheme)) {
                //    if (NetEventSource.Log.IsEnabled()) {
                //        TraceError($"Insecure https to http redirect from '{requestUri}' to '{location}' blocked.", response.RequestMessage!.GetHashCode());
                //    }
                //    break;
                //}

                if (responseMessage.StatusCode == HttpStatusCode.RedirectMethod) {
                    httpMethod = HttpMethod.Get;
                }
                // Based on Wikipedia https://en.wikipedia.org/wiki/HTTP_302:
                //  Many web browsers implemented this code in a manner that violated this standard, changing
                //  the request type of the new request to GET, regardless of the type employed in the original request
                //  (e.g. POST). For this reason, HTTP/1.1 (RFC 2616) added the new status codes 303 and 307 to disambiguate
                //  between the two behaviours, with 303 mandating the change of request type to GET, and 307 preserving the
                //  request type as originally sent. Despite the greater clarity provided by this disambiguation, the 302 code
                //  is still employed in web frameworks to preserve compatibility with browsers that do not implement the HTTP/1.1
                //  specification.

                // NOTE: Given the above, it is not surprising that HttpClient when AllowRedirect = true
                // solves this problem by a helper method:
                if (RedirectRequestRequiresForceGet(responseMessage.StatusCode, httpMethod)) {
                    httpMethod = HttpMethod.Get;
                    // HttpClient sets request.Content to null here:
                    // TODO: However... should we be allowed to modify Request like that here?
                    message.Content = null;
                    // HttpClient Redirect handler also does this:
                    //if (message.Headers.TansferEncodingChunked == true) {
                    //    request.Headers.TransferEncodingChunked = false;
                    //}
                }

                url = location;

                if (responseMessage.Headers.TryGetValues(KnownHeaders.SetCookie, out var cookiesHeader1)) {
                    foundCookies = true;
                    // ReSharper disable once PossibleMultipleEnumeration
                    cookieContainer.AddCookies(url, cookiesHeader1);
                    // ReSharper disable once PossibleMultipleEnumeration
                    Options.CookieContainer?.AddCookies(url, cookiesHeader1);
                }
            } while (true);

            // Parse all the cookies from the response and update the cookie jar with cookies
            if (responseMessage.Headers.TryGetValues(KnownHeaders.SetCookie, out var cookiesHeader2)) {
                // ReSharper disable once PossibleMultipleEnumeration
                cookieContainer.AddCookies(url, cookiesHeader2);
                // ReSharper disable once PossibleMultipleEnumeration
                Options.CookieContainer?.AddCookies(url, cookiesHeader2);
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

    /// <summary>
    /// Based on .net core RedirectHandler class: 
    /// https://github.com/dotnet/runtime/blob/main/src/libraries/System.Net.Http/src/System/Net/Http/SocketsHttpHandler/RedirectHandler.cs
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="httpMethod"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private bool RedirectRequestRequiresForceGet(HttpStatusCode statusCode, HttpMethod httpMethod) {
        return statusCode switch {
            HttpStatusCode.Moved or HttpStatusCode.Found or HttpStatusCode.MultipleChoices 
                => httpMethod == HttpMethod.Post,
            HttpStatusCode.SeeOther => httpMethod != HttpMethod.Get && httpMethod != HttpMethod.Head,
            _ => false,
        };
    }

    HttpRequestMessage PrepareRequestMessage(HttpMethod httpMethod, Uri url, HttpContent content, RequestHeaders headers) {
        var message = new HttpRequestMessage(httpMethod, url) { Content = content };
        message.Headers.Host         = Options.BaseHost;
        message.Headers.CacheControl = Options.CachePolicy;
        message.AddHeaders(headers);

        return message;
    }

    static bool IsRedirect(HttpResponseMessage responseMessage)
        => responseMessage.StatusCode switch {
            HttpStatusCode.MovedPermanently  => true,
            HttpStatusCode.SeeOther          => true,
            HttpStatusCode.TemporaryRedirect => true,
            HttpStatusCode.Redirect          => true,
#if NET
            HttpStatusCode.PermanentRedirect => true,
#endif
            _ => false
        };

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
