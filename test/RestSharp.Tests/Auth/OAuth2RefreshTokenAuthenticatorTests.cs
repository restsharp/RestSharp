using System.Net;
using RestSharp.Authenticators.OAuth2;
using RichardSzalay.MockHttp;

namespace RestSharp.Tests.Auth;

public class OAuth2RefreshTokenAuthenticatorTests : IDisposable {
    const string TokenEndpoint  = "https://auth.example.com/token";
    const string ClientId       = "my-client";
    const string ClientSecret   = "my-secret";
    const string InitialAccess  = "initial-access-token";
    const string InitialRefresh = "initial-refresh-token";

    readonly MockHttpMessageHandler _mockHttp = new();

    static string TokenJson(
        string accessToken  = "new-access-token",
        int expiresIn       = 3600,
        string refreshToken = null
    ) {
        var refreshPart = refreshToken != null
            ? $""","refresh_token":"{refreshToken}" """
            : "";
        return $$"""{"access_token":"{{accessToken}}","token_type":"Bearer","expires_in":{{expiresIn}}{{refreshPart}}}""";
    }

    OAuth2TokenRequest CreateRequest(
        TimeSpan? expiryBuffer = null,
        Action<OAuth2TokenResponse> onTokenRefreshed = null
    ) =>
        new(TokenEndpoint, ClientId, ClientSecret) {
            HttpClient       = new HttpClient(_mockHttp),
            ExpiryBuffer     = expiryBuffer ?? TimeSpan.Zero,
            OnTokenRefreshed = onTokenRefreshed
        };

    [Fact]
    public async Task Should_use_initial_token_when_not_expired() {
        // No mock response needed — the token endpoint should not be called
        using var authenticator = new OAuth2RefreshTokenAuthenticator(
            CreateRequest(),
            InitialAccess,
            InitialRefresh,
            DateTimeOffset.UtcNow.AddHours(1)
        );

        var request = new RestRequest();
        await authenticator.Authenticate(null!, request);

        var authHeader = request.Parameters.FirstOrDefault(p => p.Name == KnownHeaders.Authorization);
        authHeader.Should().NotBeNull();
        authHeader.Value.Should().Be($"Bearer {InitialAccess}");
    }

    [Fact]
    public async Task Should_refresh_when_token_expired() {
        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .Respond("application/json", TokenJson());

        using var authenticator = new OAuth2RefreshTokenAuthenticator(
            CreateRequest(),
            InitialAccess,
            InitialRefresh,
            DateTimeOffset.MinValue
        );

        var request = new RestRequest();
        await authenticator.Authenticate(null!, request);

        var authHeader = request.Parameters.FirstOrDefault(p => p.Name == KnownHeaders.Authorization);
        authHeader.Should().NotBeNull();
        authHeader.Value.Should().Be("Bearer new-access-token");
    }

    [Fact]
    public async Task Should_send_refresh_token_in_request() {
        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .WithFormData("grant_type", "refresh_token")
            .WithFormData("refresh_token", InitialRefresh)
            .WithFormData("client_id", ClientId)
            .WithFormData("client_secret", ClientSecret)
            .Respond("application/json", TokenJson());

        using var authenticator = new OAuth2RefreshTokenAuthenticator(
            CreateRequest(),
            InitialAccess,
            InitialRefresh,
            DateTimeOffset.MinValue
        );

        var request = new RestRequest();
        await authenticator.Authenticate(null!, request);

        var authHeader = request.Parameters.FirstOrDefault(p => p.Name == KnownHeaders.Authorization);
        authHeader.Should().NotBeNull();
        authHeader.Value.Should().Be("Bearer new-access-token");
    }

    [Fact]
    public async Task Should_update_refresh_token_when_rotated() {
        var callCount = 0;
        const string rotatedRefresh = "rotated-refresh-token";

        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .Respond(_ => {
                var count = Interlocked.Increment(ref callCount);

                var json = count == 1
                    ? TokenJson("first-access", 0, rotatedRefresh)
                    : TokenJson("second-access", 3600);

                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });

        using var authenticator = new OAuth2RefreshTokenAuthenticator(
            CreateRequest(),
            InitialAccess,
            InitialRefresh,
            DateTimeOffset.MinValue
        );

        // First call: expires immediately (expiresIn=0), returns rotated refresh token
        var request1 = new RestRequest();
        await authenticator.Authenticate(null!, request1);

        var authHeader1 = request1.Parameters.FirstOrDefault(p => p.Name == KnownHeaders.Authorization);
        authHeader1.Value.Should().Be("Bearer first-access");

        // Second call: token expired (expiresIn was 0), should use rotated refresh token
        var request2 = new RestRequest();
        await authenticator.Authenticate(null!, request2);

        var authHeader2 = request2.Parameters.FirstOrDefault(p => p.Name == KnownHeaders.Authorization);
        authHeader2.Value.Should().Be("Bearer second-access");

        callCount.Should().Be(2);
    }

    [Fact]
    public async Task Should_invoke_callback_on_refresh() {
        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .Respond("application/json", TokenJson());

        OAuth2TokenResponse capturedResponse = null;

        using var authenticator = new OAuth2RefreshTokenAuthenticator(
            CreateRequest(onTokenRefreshed: r => capturedResponse = r),
            InitialAccess,
            InitialRefresh,
            DateTimeOffset.MinValue
        );

        var request = new RestRequest();
        await authenticator.Authenticate(null!, request);

        capturedResponse.Should().NotBeNull();
        capturedResponse.AccessToken.Should().Be("new-access-token");
        capturedResponse.ExpiresIn.Should().Be(3600);
        capturedResponse.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public async Task Should_throw_on_error_response() {
        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .Respond(HttpStatusCode.Unauthorized, "application/json", """{"error":"invalid_grant"}""");

        using var authenticator = new OAuth2RefreshTokenAuthenticator(
            CreateRequest(),
            InitialAccess,
            InitialRefresh,
            DateTimeOffset.MinValue
        );

        var request = new RestRequest();
        var act = () => authenticator.Authenticate(null!, request).AsTask();

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*Unauthorized*");
    }

    public void Dispose() => _mockHttp.Dispose();
}
