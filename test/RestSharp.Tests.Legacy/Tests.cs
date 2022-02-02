namespace RestSharp.Tests.Legacy;

public class Tests {
    [Fact]
    public async Task Test1() {
        var options = new RestClientOptions("https://enlgqo6062j5.x.pipedream.net/");
        var client  = new RestClient(options);

        var request = new RestRequest("resource");
        request.AddHeader("Content-Type", "application/force-download");
        var response = await client.GetAsync(request);
    }
}