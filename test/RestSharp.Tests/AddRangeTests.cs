namespace RestSharp.Tests;

public class AddRangeTests {
    [Fact]
    public async Task ShouldParseOutLongRangeSpecifier() {
        var        restClient = new RestClient("http://localhost");
        var        req        = new RestRequest("bob", Method.Get);
        const long start      = (long)int.MaxValue + 1;
        const long end        = start + 1;

        req.AddHeader("Range", $"pages={start}-{end}");
        await restClient.ExecuteAsync(req);
    }

    [Fact]
    public async Task ShouldParseOutRangeSpecifier() {
        var restClient = new RestClient("http://localhost");
        var req        = new RestRequest("bob", Method.Get);

        req.AddHeader("Range", "pages=1-2");
        await restClient.ExecuteAsync(req);
    }
}