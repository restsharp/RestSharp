﻿//  Copyright © 2009-2021 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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

namespace RestSharp;

public partial class RestClient {
    /// <summary>
    /// Executes the request asynchronously, authenticating if needed
    /// </summary>
    /// <param name="request">Request to be executed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default) {
        var internalResponse = await ExecuteInternal(request, cancellationToken).ConfigureAwait(false);

        var response = new RestResponse();

        response = internalResponse.Exception == null
            ? await RestResponse.FromHttpResponse(
                    internalResponse.ResponseMessage!,
                    request,
                    Options.Encoding,
                    CookieContainer.GetCookies(internalResponse.Url),
                    CalculateResponseStatus,
                    cancellationToken
                )
                .ConfigureAwait(false)
            : AddError(response, internalResponse.Exception, internalResponse.TimeoutToken);

        response.Request = request;
        response.Request.IncreaseNumAttempts();

        return Options.ThrowOnAnyError ? ThrowIfError(response) : response;
    }

    async Task<InternalResponse> ExecuteInternal(RestRequest request, CancellationToken cancellationToken) {
        Ensure.NotNull(request, nameof(request));

        using var requestContent = new RequestContent(this, request);

        if (Authenticator != null)
            await Authenticator.Authenticate(this, request).ConfigureAwait(false);

        var httpMethod = AsHttpMethod(request.Method);
        var url        = BuildUri(request);
        var message    = new HttpRequestMessage(httpMethod, url) { Content = requestContent.BuildContent() };
        message.Headers.Host         = Options.BaseHost;
        message.Headers.CacheControl = Options.CachePolicy;

        using var timeoutCts = new CancellationTokenSource(request.Timeout > 0 ? request.Timeout : int.MaxValue);
        using var cts        = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);
        var       ct         = cts.Token;

        try {
            var headers = new RequestHeaders()
                .AddHeaders(request.Parameters)
                .AddHeaders(DefaultParameters)
                .AddAcceptHeader(AcceptedContentTypes);
            message.AddHeaders(headers);

            if (request.OnBeforeRequest != null)
                await request.OnBeforeRequest(message).ConfigureAwait(false);

            var responseMessage = await HttpClient.SendAsync(message, request.CompletionOption, ct).ConfigureAwait(false);

            if (request.OnAfterRequest != null)
                await request.OnAfterRequest(responseMessage).ConfigureAwait(false);

            return new InternalResponse(responseMessage, url, null, timeoutCts.Token);
        }
        catch (Exception ex) {
            return new InternalResponse(null, url, ex, timeoutCts.Token);
        }
    }

    record InternalResponse(HttpResponseMessage? ResponseMessage, Uri Url, Exception? Exception, CancellationToken TimeoutToken);

    /// <summary>
    /// A specialized method to download files as streams.
    /// </summary>
    /// <param name="request">Pre-configured request instance.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The downloaded stream.</returns>
    [PublicAPI]
    public async Task<Stream?> DownloadStreamAsync(RestRequest request, CancellationToken cancellationToken = default) {
        // Make sure we only read the headers so we can stream the content body efficiently
        request.CompletionOption = HttpCompletionOption.ResponseHeadersRead;
        var response = await ExecuteInternal(request, cancellationToken).ConfigureAwait(false);

        if (response.Exception != null) {
            return Options.ThrowOnAnyError ? throw response.Exception : null;
        }

        if (response.ResponseMessage == null) return null;

        if (request.ResponseWriter != null) {
#if NETSTANDARD
            using var stream = await response.ResponseMessage.ReadResponse(cancellationToken).ConfigureAwait(false);
#else
            await using var stream = await response.ResponseMessage.ReadResponse(cancellationToken).ConfigureAwait(false);
#endif
            return request.ResponseWriter(stream!);
        }

        return await response.ResponseMessage.ReadResponse(cancellationToken).ConfigureAwait(false);
    }

    static RestResponse AddError(RestResponse response, Exception exception, CancellationToken timeoutToken) {
        response.ResponseStatus = exception is OperationCanceledException
            ? TimedOut() ? ResponseStatus.TimedOut : ResponseStatus.Aborted
            : ResponseStatus.Error;

        response.ErrorMessage   = exception.Message;
        response.ErrorException = exception;

        return response;

        bool TimedOut() => timeoutToken.IsCancellationRequested || exception.Message.Contains("HttpClient.Timeout");
    }

    internal static RestResponse ThrowIfError(RestResponse response) {
        var exception = response.GetException();
        if (exception != null) throw exception;

        return response;
    }

    static HttpMethod AsHttpMethod(Method method)
        => method switch {
            Method.Get     => HttpMethod.Get,
            Method.Post    => HttpMethod.Post,
            Method.Put     => HttpMethod.Put,
            Method.Delete  => HttpMethod.Delete,
            Method.Head    => HttpMethod.Head,
            Method.Options => HttpMethod.Options,
#if NETSTANDARD
            Method.Patch => new HttpMethod("PATCH"),
#else
            Method.Patch   => HttpMethod.Patch,
#endif
            Method.Merge  => new HttpMethod("MERGE"),
            Method.Copy   => new HttpMethod("COPY"),
            Method.Search => new HttpMethod("SEARCH"),
            _             => throw new ArgumentOutOfRangeException()
        };
}