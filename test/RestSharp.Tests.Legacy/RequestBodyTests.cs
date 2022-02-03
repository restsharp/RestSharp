using System.Diagnostics.CodeAnalysis;

namespace RestSharp.Tests.Legacy;

public class RequestBodyTests {
    [Fact(Skip = "Setting the content type for GET requests doesn't seem to be possible on Windows")]
    [SuppressMessage("Usage", "xUnit1004:Test methods should not be skipped")]
    public async Task GetRequestWithContentType() {
        var options = new RestClientOptions("https://endim2jwvq8mr.x.pipedream.net/");
        var client  = new RestClient(options);

        var request = new RestRequest("resource");
        request.AddHeader("Content-Type", "application/force-download");
        var response = await client.GetAsync(request);
    }
}