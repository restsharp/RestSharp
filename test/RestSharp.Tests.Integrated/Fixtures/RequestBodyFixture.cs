using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Tests.Integrated.Fixtures;

public sealed class RequestBodyFixture : IDisposable {
    public SimpleServer Server { get; }

    public RequestBodyFixture() => Server = SimpleServer.Create(Handlers.Generic<RequestBodyCapturer>());
    
    public void Dispose() => Server.Dispose();
}