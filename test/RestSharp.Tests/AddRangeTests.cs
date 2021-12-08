namespace RestSharp.Tests;

public class AddRangeTests {
    [Fact]
    public void ShouldParseOutLongRangeSpecifier() {
        var        restClient = new RestClient("http://localhost");
        var        req        = new RestRequest("bob", Method.Get);
        const long start      = (long)int.MaxValue + 1;
        const long end        = start + 1;

        req.AddHeader("Range", $"pages={start}-{end}");
        restClient.Execute(req);
    }

    [Fact]
    public void ShouldParseOutRangeSpecifier() {
        var restClient = new RestClient("http://localhost");
        var req        = new RestRequest("bob", Method.Get);

        req.AddHeader("Range", "pages=1-2");
        restClient.Execute(req);
    }
}