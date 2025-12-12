using Microsoft.Extensions.DependencyInjection;
using RestSharp.Extensions.DependencyInjection;
using RestSharp.Tests.Shared;
using RestSharp.Tests.Shared.Server;

namespace RestSharp.Tests.DependencyInjection;

public sealed class DefaultClientRequestTests : RequestTestsBase, IClassFixture<WireMockTestServer>, IDisposable {
    readonly ServiceProvider _provider;

    public DefaultClientRequestTests(WireMockTestServer server) : base(false) {
        var services = new ServiceCollection();
        services.AddRestClient(new Uri(server.Url!));
        _provider = services.BuildServiceProvider();
    }

    public void Dispose() => _provider.Dispose();

    protected override IRestClient GetClient() => _provider.GetRequiredService<IRestClient>();
}