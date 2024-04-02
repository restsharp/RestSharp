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
using System.Web;
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

    [Flags]
    private enum ExecutionState {
        None = 0x0,
        FoundCookie = 0x1,
        FirstAttempt = 0x2,
        DoNotSendBody = 0x4,
        VerbAltered = 0x8,
        VerbAlterationPrevented = 0x10,
    };

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

        HttpResponseMessage? responseMessage = null;
        // Make sure we have a cookie container if not provided in the request
        var cookieContainer = request.CookieContainer ??= new CookieContainer();

        try {
            var headers = new RequestHeaders()
                .AddHeaders(request.Parameters)
                .AddHeaders(DefaultParameters)
                .AddAcceptHeader(AcceptedContentTypes)
                .AddCookieHeaders(url, cookieContainer)
                .AddCookieHeaders(url, Options.CookieContainer);

            ExecutionState state = ExecutionState.FirstAttempt;
            int redirectCount = 0;

            do {
                // TODO: Is there a more effecient way to do this other than rebuilding the RequestContent
                // every time through this loop?
                using var requestContent = new RequestContent(this, request);
                using var content = requestContent.BuildContent(omitBody: state.HasFlag(ExecutionState.DoNotSendBody));

                // If we found coookies during a redirect,
                // we need to update the Cookie headers:
                if (state.HasFlag(ExecutionState.FoundCookie)) {
                    headers.AddCookieHeaders(url, cookieContainer);
                    // Clear the state:
                    state &= ~ExecutionState.FoundCookie;
                }
                using var message = PrepareRequestMessage(httpMethod, url, content, headers);

                if (state.HasFlag(ExecutionState.FirstAttempt)) {
                    state &= ~ExecutionState.FirstAttempt;
                    try {
                        if (request.OnBeforeRequest != null) await request.OnBeforeRequest(message).ConfigureAwait(false);
                        await OnBeforeRequest(message).ConfigureAwait(false);
                    }
                    catch (Exception e) {
                        throw new RestClientInternalException("RestClient.ExecuteRequestAsync OnBeforeRequest threw an exception: ", e);
                    }
                }

                responseMessage = await HttpClient.SendAsync(message, request.CompletionOption, ct).ConfigureAwait(false);

                if (!IsRedirect(Options.RedirectOptions, responseMessage)) {
                    break;
                }

                var location = responseMessage.Headers.Location;

                if (location == null) {
                    break;
                }

                redirectCount++;
                if (redirectCount >= Options.RedirectOptions.MaxRedirects) {
                    break;
                }

                if (!location.IsAbsoluteUri) {
                    location = new Uri(url, location);
                }

                if (Options.RedirectOptions.ForwardQuery) {
                    string oringalQuery = originalUrl.Query;
                    if (!string.IsNullOrEmpty(oringalQuery)
                        && string.IsNullOrEmpty(location.Query)) {
                        // AddQueryString DOES NOT want the ? in the supplied parameter,
                        // so strip it:
                        if (oringalQuery[0] == '?') {
                            oringalQuery = oringalQuery.Substring(1, oringalQuery.Length - 1);
                        }
                        location = location.AddQueryString(oringalQuery);
                    }
                }

                // Mirror HttpClient redirection behavior as of 07/25/2023:
                // Per https://tools.ietf.org/html/rfc7231#section-7.1.2, a redirect location without a
                // fragment should inherit the fragment from the original URI.
                if (Options.RedirectOptions.ForwardFragment) {
                    string requestFragment = originalUrl.Fragment;
                    if (!string.IsNullOrEmpty(requestFragment)) {
                        string redirectFragment = location.Fragment;
                        if (string.IsNullOrEmpty(redirectFragment)) {
                            location = new UriBuilder(location) { Fragment = requestFragment }.Uri;
                        }
                    }
                }

                // Disallow automatic redirection from secure to non-secure schemes
                // based on the option setting:
                if (HttpUtilities.IsSupportedSecureScheme(originalUrl.Scheme)
                    && !HttpUtilities.IsSupportedSecureScheme(location.Scheme)
                    && !Options.RedirectOptions.FollowRedirectsToInsecure) {
                    // TODO: Log here...
                    break;
                }

                // This is the expected behavior for this status code, but
                // ignore it if requested from the RedirectOptions:
                if (responseMessage.StatusCode == HttpStatusCode.RedirectMethod
                    && Options.RedirectOptions.AllowRedirectMethodStatusCodeToAlterVerb) {
                    httpMethod = HttpMethod.Get;
                    state |= ExecutionState.VerbAltered;
                }
                else if (responseMessage.StatusCode == HttpStatusCode.RedirectMethod) {
                    state |= ExecutionState.VerbAlterationPrevented;
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
                if (!state.HasFlag(ExecutionState.VerbAlterationPrevented)
                    && (
                       state.HasFlag(ExecutionState.VerbAltered)
                       || (Options.RedirectOptions.AllowForcedRedirectVerbChange
                           && RedirectRequestRequiresForceGet(responseMessage.StatusCode, httpMethod)))) {
                    httpMethod = HttpMethod.Get;
                    if (!Options.RedirectOptions.ForceForwardBody) {
                        // HttpClient RedirectHandler sets request.Content to null here:
                        state |= ExecutionState.DoNotSendBody;
                        // HttpClient Redirect handler also forcibly removes
                        // a Transfer-Encoding of chunked in this case.
                        // This makes sense, since without a body, there isn't any chunked (or otherwise) content
                        // to transmit.
                        // NOTE: Although, I'm not sure why it only cares about chunked...
                        Parameter? transferEncoding = request.Parameters.TryFind(KnownHeaders.TransferEncoding);
                        if (transferEncoding != null
                            && transferEncoding.Type == ParameterType.HttpHeader
                            && string.Equals((string)transferEncoding.Value!, "chunked", StringComparison.OrdinalIgnoreCase)) {
                            message.Headers.Remove(KnownHeaders.TransferEncoding);
                        }
                    }
                }

                url = location;

                // Regardless of whether or not we will be forwarding
                // cookies, the CookieContainer will be updated:
                if (responseMessage.Headers.TryGetValues(KnownHeaders.SetCookie, out var cookiesHeader1)) {
                    if (Options.RedirectOptions.ForwardCookies) {
                        state |= ExecutionState.FoundCookie;
                    }
                    // ReSharper disable once PossibleMultipleEnumeration
                    cookieContainer.AddCookies(url, cookiesHeader1);
                    // ReSharper disable once PossibleMultipleEnumeration
                    Options.CookieContainer?.AddCookies(url, cookiesHeader1);
                }

                // Process header related RedirectOptions:
                if (Options.RedirectOptions.ForwardHeaders) {
                    if (!Options.RedirectOptions.ForwardAuthorization) {
                        headers.Parameters.RemoveParameter(KnownHeaders.Authorization);
                    }
                    if (!Options.RedirectOptions.ForwardCookies) {
                        headers.Parameters.RemoveParameter(KnownHeaders.Cookie);
                    }
                }
                else {
                    List<string> headersToRemove = new List<string>();
                    foreach (var param in headers.Parameters) {
                        if (param is HeaderParameter header) {
                            // Keep headers requested to be forwarded:
                            if (string.Compare(param.Name, KnownHeaders.Authorization, StringComparison.InvariantCultureIgnoreCase) == 0
                                && Options.RedirectOptions.ForwardAuthorization) {
                                continue;
                            }
                            if (string.Compare(param.Name, KnownHeaders.Cookie, StringComparison.InvariantCultureIgnoreCase) == 0
                                && Options.RedirectOptions.ForwardCookies) {
                                continue;
                            }
                            // Otherwise: schedule the items for removal:
                            headersToRemove.Add(param.Name!);
                        }
                    }
                    if (headersToRemove.Count > 0) {
                        for (int i = 0; i < headersToRemove.Count; i++) {
                            headers.Parameters.RemoveParameter(headersToRemove[i]);
                        }
                    }
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
        catch (RestClientInternalException e) {
            throw e.InnerException!;
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
    /// From https://github.com/dotnet/runtime/blob/main/src/libraries/System.Net.Http/src/System/Net/Http/SocketsHttpHandler/HttpUtilities.cs
    /// </summary>
    private static class HttpUtilities {
        internal static bool IsSupportedScheme(string scheme) =>
            IsSupportedNonSecureScheme(scheme) ||
            IsSupportedSecureScheme(scheme);

        internal static bool IsSupportedNonSecureScheme(string scheme) =>
            string.Equals(scheme, "http", StringComparison.OrdinalIgnoreCase) || IsNonSecureWebSocketScheme(scheme);

        internal static bool IsSupportedSecureScheme(string scheme) =>
            string.Equals(scheme, "https", StringComparison.OrdinalIgnoreCase) || IsSecureWebSocketScheme(scheme);

        internal static bool IsNonSecureWebSocketScheme(string scheme) =>
            string.Equals(scheme, "ws", StringComparison.OrdinalIgnoreCase);

        internal static bool IsSecureWebSocketScheme(string scheme) =>
            string.Equals(scheme, "wss", StringComparison.OrdinalIgnoreCase);

        internal static bool IsSupportedProxyScheme(string scheme) =>
            string.Equals(scheme, "http", StringComparison.OrdinalIgnoreCase) || string.Equals(scheme, "https", StringComparison.OrdinalIgnoreCase) || IsSocksScheme(scheme);

        internal static bool IsSocksScheme(string scheme) =>
            string.Equals(scheme, "socks5", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(scheme, "socks4a", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(scheme, "socks4", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Based on .net core RedirectHandler class:
    /// https://github.com/dotnet/runtime/blob/main/src/libraries/System.Net.Http/src/System/Net/Http/SocketsHttpHandler/RedirectHandler.cs
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="httpMethod"></param>
    /// <returns>Returns true if statusCode requires a verb change to Get.</returns>
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

    static bool IsRedirect(RestClientRedirectionOptions options, HttpResponseMessage responseMessage) {
        return options.FollowRedirects && options.RedirectStatusCodes.Contains(responseMessage.StatusCode);
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
