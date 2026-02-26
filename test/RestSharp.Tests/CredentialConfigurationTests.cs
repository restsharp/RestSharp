using System.Net;

namespace RestSharp.Tests;

public class CredentialConfigurationTests {
    [Fact]
    public void Explicit_credentials_are_not_overwritten_by_UseDefaultCredentials() {
        var credentials = new NetworkCredential("user", "password");
        var options = new RestClientOptions("https://dummy.org") {
            Credentials           = credentials,
            UseDefaultCredentials = false
        };

        var handler = new HttpClientHandler();
        RestClient.ConfigureHttpMessageHandler(handler, options);

        handler.Credentials.Should().BeSameAs(credentials);
    }

    [Fact]
    public void DefaultCredentials_set_explicitly_are_not_overwritten() {
        var options = new RestClientOptions("https://dummy.org") {
            Credentials           = CredentialCache.DefaultCredentials,
            UseDefaultCredentials = false
        };

        var handler = new HttpClientHandler();
        RestClient.ConfigureHttpMessageHandler(handler, options);

        handler.Credentials.Should().BeSameAs(CredentialCache.DefaultCredentials);
    }

    [Fact]
    public void UseDefaultCredentials_sets_credentials_when_no_explicit_credentials() {
        var options = new RestClientOptions("https://dummy.org") {
            UseDefaultCredentials = true
        };

        var handler = new HttpClientHandler();
        RestClient.ConfigureHttpMessageHandler(handler, options);

        handler.UseDefaultCredentials.Should().BeTrue();
    }
}
