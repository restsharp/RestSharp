namespace RestSharp.Tests;

public class OptionsTests {
    [Fact]
    public void Ensure_follow_redirect() {
        var       value   = false;
        var       options = new RestClientOptions { FollowRedirects = true, ConfigureMessageHandler = Configure };
        using var _       = new RestClient(options);
        value.Should().BeTrue();
        return;

        HttpMessageHandler Configure(HttpMessageHandler handler) {
            switch (handler) {
                case HttpClientHandler httpClientHandler:
                    value = httpClientHandler.AllowAutoRedirect;
                    break;
                case WinHttpHandler winHttpHandler:
#pragma warning disable CA1416
                    value = winHttpHandler.AutomaticRedirection;
#pragma warning restore CA1416
                    break;
            }

            return handler;
        }
    }
}