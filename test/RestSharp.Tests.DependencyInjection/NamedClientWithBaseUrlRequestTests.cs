using Microsoft.Extensions.DependencyInjection;
using RestSharp.Extensions.DependencyInjection;
using RestSharp.Tests.Shared;
using RestSharp.Tests.Shared.Server;

namespace RestSharp.Tests.DependencyInjection;

public sealed class NamedClientWithBaseUrlRequestTests : RequestTestsBase, IClassFixture<WireMockTestServer>, IDisposable {
    readonly ServiceProvider _provider;

    const string ClientName = "test";

    public NamedClientWithBaseUrlRequestTests(WireMockTestServer server) : base(false) {
        var services = new ServiceCollection();
        services.AddRestClient(ClientName, new Uri(server.Url!));
        _provider = services.BuildServiceProvider();
    }

    public void Dispose() => _provider.Dispose();

    protected override IRestClient GetClient() => _provider.GetRequiredService<IRestClientFactory>().CreateClient(ClientName);
}