using System.Net;
using RestSharp.Authenticators.OAuth2;
using RichardSzalay.MockHttp;

namespace RestSharp.Tests.Auth;

public class OAuth2ClientCredentialsAuthenticatorTests : IDisposable {
    const string TokenEndpoint = "https://auth.example.com/token";
    const string ClientId      = "my-client";
    const string ClientSecret  = "my-secret";

    readonly MockHttpMessageHandler _mockHttp = new();

    static string TokenJson(string accessToken = "test-access-token", int expiresIn = 3600)
        => $$"""{"access_token":"{{accessToken}}","token_type":"Bearer","expires_in":{{expiresIn}}}""";

    OAuth2TokenRequest CreateRequest(
        string scope           = null,
        TimeSpan? expiryBuffer = null,
        Action<OAuth2TokenResponse> onTokenRefreshed = null
    ) {
        var request = new OAuth2TokenRequest(TokenEndpoint, ClientId, ClientSecret) {
            HttpClient   = new HttpClient(_mockHttp),
            ExpiryBuffer = expiryBuffer ?? TimeSpan.Zero
        };

        if (scope != null || onTokenRefreshed != null) {
            return new OAuth2TokenRequest(TokenEndpoint, ClientId, ClientSecret) {
                HttpClient       = new HttpClient(_mockHttp),
                ExpiryBuffer     = expiryBuffer ?? TimeSpan.Zero,
                Scope            = scope,
                OnTokenRefreshed = onTokenRefreshed
            };
        }

        return request;
    }

    [Fact]
    public async Task Should_obtain_token_and_set_authorization_header() {
        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .Respond("application/json", TokenJson());

        using var authenticator = new OAuth2ClientCredentialsAuthenticator(CreateRequest());

        var request = new RestRequest();
        await authenticator.Authenticate(null!, request);

        var authHeader = request.Parameters.FirstOrDefault(p => p.Name == KnownHeaders.Authorization);
        authHeader.Should().NotBeNull();
        authHeader.Value.Should().Be("Bearer test-access-token");
    }

    [Fact]
    public async Task Should_cache_token_across_multiple_calls() {
        var callCount = 0;

        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .Respond(_ => {
                Interlocked.Increment(ref callCount);
                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(TokenJson(), System.Text.Encoding.UTF8, "application/json")
                };
            });

        using var authenticator = new OAuth2ClientCredentialsAuthenticator(CreateRequest());

        var request1 = new RestRequest();
        await authenticator.Authenticate(null!, request1);

        var request2 = new RestRequest();
        await authenticator.Authenticate(null!, request2);

        callCount.Should().Be(1);
    }

    [Fact]
    public async Task Should_refresh_expired_token() {
        var callCount = 0;

        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .Respond(_ => {
                var token = Interlocked.Increment(ref callCount) == 1
                    ? TokenJson("first-token", 0)
                    : TokenJson("second-token", 3600);

                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(token, System.Text.Encoding.UTF8, "application/json")
                };
            });

        using var authenticator = new OAuth2ClientCredentialsAuthenticator(CreateRequest());

        var request1 = new RestRequest();
        await authenticator.Authenticate(null!, request1);

        var authHeader1 = request1.Parameters.FirstOrDefault(p => p.Name == KnownHeaders.Authorization);
        authHeader1.Value.Should().Be("Bearer first-token");

        var request2 = new RestRequest();
        await authenticator.Authenticate(null!, request2);

        var authHeader2 = request2.Parameters.FirstOrDefault(p => p.Name == KnownHeaders.Authorization);
        authHeader2.Value.Should().Be("Bearer second-token");

        callCount.Should().Be(2);
    }

    [Fact]
    public async Task Should_invoke_callback_on_token_refresh() {
        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .Respond("application/json", TokenJson());

        OAuth2TokenResponse capturedResponse = null;
        using var authenticator = new OAuth2ClientCredentialsAuthenticator(
            CreateRequest(onTokenRefreshed: r => capturedResponse = r)
        );

        var request = new RestRequest();
        await authenticator.Authenticate(null!, request);

        capturedResponse.Should().NotBeNull();
        capturedResponse.AccessToken.Should().Be("test-access-token");
        capturedResponse.ExpiresIn.Should().Be(3600);
        capturedResponse.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public async Task Should_throw_on_error_response() {
        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .Respond(HttpStatusCode.BadRequest, "application/json", """{"error":"invalid_client"}""");

        using var authenticator = new OAuth2ClientCredentialsAuthenticator(CreateRequest());

        var request = new RestRequest();
        var act = () => authenticator.Authenticate(null!, request).AsTask();

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*BadRequest*");
    }

    [Fact]
    public async Task Should_throw_on_empty_access_token() {
        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .Respond("application/json", """{"access_token":"","token_type":"Bearer","expires_in":3600}""");

        using var authenticator = new OAuth2ClientCredentialsAuthenticator(CreateRequest());

        var request = new RestRequest();
        var act = () => authenticator.Authenticate(null!, request).AsTask();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*invalid response*");
    }

    [Fact]
    public async Task Should_send_scope_when_configured() {
        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .WithFormData("scope", "read write")
            .Respond("application/json", TokenJson());

        using var authenticator = new OAuth2ClientCredentialsAuthenticator(
            CreateRequest(scope: "read write")
        );

        var request = new RestRequest();
        await authenticator.Authenticate(null!, request);

        var authHeader = request.Parameters.FirstOrDefault(p => p.Name == KnownHeaders.Authorization);
        authHeader.Should().NotBeNull();
        authHeader.Value.Should().Be("Bearer test-access-token");
    }

    public void Dispose() => _mockHttp.Dispose();
}
