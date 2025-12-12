namespace RestSharp.Tests.Parameters;

public class HeaderRangeTests {
    [Fact]
    public async Task ShouldParseOutLongRangeSpecifier() {
        using var restClient = new RestClient("http://localhost");
        var req = new RestRequest("bob");
        const long start = (long)int.MaxValue + 1;
        const long end = start + 1;

        req.AddHeader("Range", $"pages={start}-{end}");
        await restClient.ExecuteAsync(req);
    }

    [Fact]
    public async Task ShouldParseOutRangeSpecifier() {
        using var restClient = new RestClient("http://localhost");
        var req = new RestRequest("bob");

        req.AddHeader("Range", "pages=1-2");
        await restClient.ExecuteAsync(req);
    }
}