namespace RestSharp.Tests;

public class OptionsTests {
    [Fact]
    public void HttpClient_AllowAutoRedirect_Is_Always_False() {
        var value = true;
        var options = new RestClientOptions { FollowRedirects = true, ConfigureMessageHandler = Configure };
        using var _ = new RestClient(options);
        value.Should().BeFalse("RestSharp handles redirects internally");
        return;

        HttpMessageHandler Configure(HttpMessageHandler handler) {
            value = (handler as HttpClientHandler)!.AllowAutoRedirect;
            return handler;
        }
    }
}