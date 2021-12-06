namespace RestSharp.Tests;

public class RequestConfiguratorTests {
    [Fact]
    public void ConfiguresTheHttpProtocolVersion() {
        var executed = false;

        var restClient = new RestClient("http://localhost");
        restClient.ConfigureWebRequest(r => executed = true);

        var req = new RestRequest("bob", Method.GET);

        restClient.Execute(req);

        Assert.True(executed);
    }
}