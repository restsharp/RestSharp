namespace RestSharp.Tests;

public class OptionsTests {
    [Fact]
    public void Ensure_follow_redirect() {
        var value   = false;
        var options = new RestClientOptions { FollowRedirects = true, ConfigureMessageHandler = Configure};
        var _  = new RestClient(options);
        value.Should().BeTrue();

        HttpMessageHandler Configure(HttpMessageHandler handler) {
            value = (handler as HttpClientHandler)!.AllowAutoRedirect;
            return handler;
        }
    }
}
