namespace RestSharp.Tests.Integrated.Interceptor;

class TestInterceptor : Interceptors.Interceptor {
    internal bool BeforeRequestCalled         { get; private set; }
    internal bool BeforeHttpRequestCalled     { get; private set; }
    internal bool AfterHttpRequestCalled      { get; private set; }
    internal bool AfterRequestCalled          { get; private set; }
    internal bool BeforeDeserializationCalled { get; private set; }

    internal Action<RestRequest>?         BeforeRequestAction         { get; set; }
    internal Action<HttpRequestMessage>?  BeforeHttpRequestAction     { get; set; }
    internal Action<HttpResponseMessage>? AfterHttpRequestAction      { get; set; }
    internal Action<RestResponse>?        AfterRequestAction          { get; set; }
    internal Action<RestResponse>?        BeforeDeserializationAction { get; set; }

    public override ValueTask BeforeHttpRequest(HttpRequestMessage req, CancellationToken cancellationToken) {
        BeforeHttpRequestCalled = true;
        BeforeHttpRequestAction?.Invoke(req);
        return base.BeforeHttpRequest(req, cancellationToken);
    }

    public override ValueTask AfterHttpRequest(HttpResponseMessage responseMessage, CancellationToken cancellationToken) {
        AfterHttpRequestCalled = true;
        AfterHttpRequestAction?.Invoke(responseMessage);
        return base.AfterHttpRequest(responseMessage, cancellationToken);
    }

    public override ValueTask AfterRequest(RestResponse response, CancellationToken cancellationToken) {
        AfterRequestCalled = true;
        AfterRequestAction?.Invoke(response);
        return base.AfterRequest(response, cancellationToken);
    }

    public override ValueTask BeforeRequest(RestRequest request, CancellationToken cancellationToken) {
        BeforeRequestCalled = true;
        BeforeRequestAction?.Invoke(request);
        return base.BeforeRequest(request, cancellationToken);
    }

    public override ValueTask BeforeDeserialization(RestResponse response, CancellationToken cancellationToken) {
        BeforeDeserializationCalled = true;
        BeforeDeserializationAction?.Invoke(response);
        return base.BeforeDeserialization(response, cancellationToken);
    }
}
