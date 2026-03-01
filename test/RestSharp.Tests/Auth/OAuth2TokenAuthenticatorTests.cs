using RestSharp.Authenticators.OAuth2;

namespace RestSharp.Tests.Auth;

public class OAuth2TokenAuthenticatorTests : IDisposable {
    readonly List<OAuth2TokenAuthenticator> _authenticators = new();

    OAuth2TokenAuthenticator CreateAuthenticator(
        Func<CancellationToken, Task<OAuth2Token>> getToken,
        string tokenType = "Bearer"
    ) {
        var auth = new OAuth2TokenAuthenticator(getToken, tokenType);
        _authenticators.Add(auth);
        return auth;
    }

    [Fact]
    public async Task Should_call_delegate_and_set_authorization_header() {
        var authenticator = CreateAuthenticator(
            _ => Task.FromResult(new OAuth2Token("my-token", DateTimeOffset.UtcNow.AddHours(1)))
        );

        var request = new RestRequest();
        await authenticator.Authenticate(null!, request);

        var authHeader = request.Parameters.FirstOrDefault(p => p.Name == KnownHeaders.Authorization);
        authHeader.Should().NotBeNull();
        authHeader.Value.Should().Be("Bearer my-token");
    }

    [Fact]
    public async Task Should_cache_token_across_calls() {
        var callCount = 0;

        var authenticator = CreateAuthenticator(_ => {
            Interlocked.Increment(ref callCount);
            return Task.FromResult(new OAuth2Token("my-token", DateTimeOffset.UtcNow.AddHours(1)));
        });

        var request1 = new RestRequest();
        await authenticator.Authenticate(null!, request1);

        var request2 = new RestRequest();
        await authenticator.Authenticate(null!, request2);

        callCount.Should().Be(1);
    }

    [Fact]
    public async Task Should_re_invoke_delegate_when_token_expired() {
        var callCount = 0;

        var authenticator = CreateAuthenticator(_ => {
            var count = Interlocked.Increment(ref callCount);

            var token = count == 1
                ? new OAuth2Token("first-token", DateTimeOffset.UtcNow.AddSeconds(-1))
                : new OAuth2Token("second-token", DateTimeOffset.UtcNow.AddHours(1));

            return Task.FromResult(token);
        });

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
    public async Task Should_use_custom_token_type() {
        var authenticator = CreateAuthenticator(
            _ => Task.FromResult(new OAuth2Token("my-token", DateTimeOffset.UtcNow.AddHours(1))),
            tokenType: "MAC"
        );

        var request = new RestRequest();
        await authenticator.Authenticate(null!, request);

        var authHeader = request.Parameters.FirstOrDefault(p => p.Name == KnownHeaders.Authorization);
        authHeader.Should().NotBeNull();
        authHeader.Value.Should().Be("MAC my-token");
    }

    public void Dispose() {
        foreach (var auth in _authenticators)
            auth.Dispose();
    }
}
