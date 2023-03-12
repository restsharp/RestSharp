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
        var internalResponse = await ExecuteRequestAsync(request, cancellationToken).ConfigureAwait(false);

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

        if (request.ResponseWriter != null) {
#if NETSTANDARD || NETFRAMEWORK
            using var stream = await response.ResponseMessage.ReadResponse(cancellationToken).ConfigureAwait(false);
#else
            await using var stream = await response.ResponseMessage.ReadResponse(cancellationToken).ConfigureAwait(false);
#endif
            return request.ResponseWriter(stream!);
        }

        return await response.ResponseMessage.ReadResponse(cancellationToken).ConfigureAwait(false);
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

        using var requestContent = new RequestContent(this, request);

        var authenticator = request.Authenticator ?? Options.Authenticator;
        if (authenticator != null) await authenticator.Authenticate(this, request).ConfigureAwait(false);

        var httpMethod = AsHttpMethod(request.Method);
        var url        = BuildUri(request);
        var message    = new HttpRequestMessage(httpMethod, url) { Content = requestContent.BuildContent() };
        message.Headers.Host         = Options.BaseHost;
        message.Headers.CacheControl = Options.CachePolicy;

        using var timeoutCts = new CancellationTokenSource(request.Timeout > 0 ? request.Timeout : int.MaxValue);
        using var cts        = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);

        var ct = cts.Token;

        try {
            // Make sure we have a cookie container if not provided in the request
            var cookieContainer = request.CookieContainer ??= new CookieContainer();

            var headers = new RequestHeaders()
                .AddHeaders(request.Parameters)
                .AddHeaders(DefaultParameters)
                .AddAcceptHeader(AcceptedContentTypes)
                .AddCookieHeaders(cookieContainer, url);
            message.AddHeaders(headers);

            if (request.OnBeforeRequest != null) await request.OnBeforeRequest(message).ConfigureAwait(false);

            var responseMessage = await HttpClient.SendAsync(message, request.CompletionOption, ct).ConfigureAwait(false);

            // Parse all the cookies from the response and update the cookie jar with cookies
            if (responseMessage.Headers.TryGetValues(KnownHeaders.SetCookie, out var cookiesHeader)) {
                foreach (var header in cookiesHeader) {
                    try {
                        cookieContainer.SetCookies(url, header);
                    }
                    catch (CookieException) {
                        // Do not fail request if we cannot parse a cookie
                    }
                }
            }

            if (request.OnAfterRequest != null) await request.OnAfterRequest(responseMessage).ConfigureAwait(false);

            return new HttpResponse(responseMessage, url, cookieContainer, null, timeoutCts.Token);
        }
        catch (Exception ex) {
            return new HttpResponse(null, url, null, ex, timeoutCts.Token);
        }
    }

    record HttpResponse(
        HttpResponseMessage? ResponseMessage,
        Uri                  Url,
        CookieContainer?     CookieContainer,
        Exception?           Exception,
        CancellationToken    TimeoutToken
    );

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
