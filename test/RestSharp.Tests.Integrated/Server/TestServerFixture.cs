namespace RestSharp.Tests.Integrated.Server;

public class TestServerFixture : IAsyncLifetime {
    public HttpServer Server { get; } = new();

    public Task InitializeAsync() => Server.Start();

    public Task DisposeAsync() => Server.Stop();
}

[CollectionDefinition(nameof(TestServerCollection))]
public class TestServerCollection : ICollectionFixture<TestServerFixture> { }

