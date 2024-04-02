namespace RestSharp.Tests;

public class OptionsTests {
    [Fact]
    public void Ensure_no_httpclient_follow_redirect() {
        var value   = false;
        var options = new RestClientOptions { FollowRedirects = true, ConfigureMessageHandler = Configure};
        var _  = new RestClient(options);
        value.Should().BeFalse();

        HttpMessageHandler Configure(HttpMessageHandler handler) {
            value = (handler as HttpClientHandler)!.AllowAutoRedirect;
            return handler;
        }
    }
}
