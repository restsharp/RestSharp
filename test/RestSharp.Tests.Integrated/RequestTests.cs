namespace RestSharp.Tests.Integrated;

public sealed class RequestTests(WireMockTestServer server)
    : RequestTestsBase(false), IClassFixture<WireMockTestServer>, IDisposable {
    readonly RestClient _client = new(server.Url!);
    
    public void Dispose() => _client.Dispose();

    protected override IRestClient GetClient() => _client;
}
