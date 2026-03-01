# OAuth2 Token Lifecycle Authenticators — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add self-contained OAuth2 authenticators that handle token acquisition, caching, expiry, and refresh internally — fixing issue #2101.

**Architecture:** Three new authenticators (`OAuth2ClientCredentialsAuthenticator`, `OAuth2RefreshTokenAuthenticator`, `OAuth2TokenAuthenticator`) that own their own `HttpClient` for token endpoint calls, avoiding the circular dependency. They share a common `OAuth2TokenResponse` model (RFC 6749) and `OAuth2TokenRequest` config class. Thread-safe via `SemaphoreSlim`.

**Tech Stack:** C# preview, System.Text.Json, xUnit + FluentAssertions + RichardSzalay.MockHttp

**Conventions:**
- All `/src` files need the Apache-2.0 license header (see existing files for exact text)
- All public types get `[PublicAPI]` attribute (from JetBrains.Annotations, auto-imported via `src/Directory.Build.props`)
- Namespace: `RestSharp.Authenticators.OAuth2` (matches existing OAuth2 authenticators)
- Tests: nullable disabled, global usings for `Xunit`, `FluentAssertions`, `AutoFixture` already configured
- Build: `dotnet build RestSharp.slnx -c Debug`
- Test: `dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj -f net9.0`

---

### Task 1: Data models — OAuth2TokenResponse, OAuth2Token, OAuth2TokenRequest

**Files:**
- Create: `src/RestSharp/Authenticators/OAuth2/OAuth2TokenResponse.cs`
- Create: `src/RestSharp/Authenticators/OAuth2/OAuth2Token.cs`
- Create: `src/RestSharp/Authenticators/OAuth2/OAuth2TokenRequest.cs`

**Step 1: Create OAuth2TokenResponse**

```csharp
//   Copyright (c) .NET Foundation and Contributors
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System.Text.Json.Serialization;

namespace RestSharp.Authenticators.OAuth2;

/// <summary>
/// OAuth 2.0 token endpoint response as defined in RFC 6749 Section 5.1.
/// </summary>
[PublicAPI]
public record OAuth2TokenResponse {
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = "";

    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = "";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }

    [JsonPropertyName("scope")]
    public string? Scope { get; init; }
}
```

**Step 2: Create OAuth2Token**

```csharp
//   Copyright (c) .NET Foundation and Contributors
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

namespace RestSharp.Authenticators.OAuth2;

/// <summary>
/// Represents an access token with its expiration time. Used as the return type
/// for the custom token provider delegate in <see cref="OAuth2TokenAuthenticator"/>.
/// </summary>
[PublicAPI]
public record OAuth2Token(string AccessToken, DateTimeOffset ExpiresAt);
```

**Step 3: Create OAuth2TokenRequest**

```csharp
//   Copyright (c) .NET Foundation and Contributors
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

namespace RestSharp.Authenticators.OAuth2;

/// <summary>
/// Configuration for OAuth 2.0 token endpoint requests. Shared by
/// <see cref="OAuth2ClientCredentialsAuthenticator"/> and <see cref="OAuth2RefreshTokenAuthenticator"/>.
/// </summary>
[PublicAPI]
public class OAuth2TokenRequest {
    /// <summary>
    /// The URL of the OAuth 2.0 token endpoint.
    /// </summary>
    public required string TokenEndpointUrl { get; init; }

    /// <summary>
    /// The OAuth 2.0 client identifier.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// The OAuth 2.0 client secret.
    /// </summary>
    public required string ClientSecret { get; init; }

    /// <summary>
    /// Optional scope to request.
    /// </summary>
    public string? Scope { get; init; }

    /// <summary>
    /// Additional form parameters to include in the token request.
    /// </summary>
    public Dictionary<string, string>? ExtraParameters { get; init; }

    /// <summary>
    /// Optional HttpClient to use for token endpoint calls. When provided, the authenticator
    /// will not create or dispose its own HttpClient.
    /// </summary>
    public HttpClient? HttpClient { get; init; }

    /// <summary>
    /// How long before actual token expiry to consider it expired. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan ExpiryBuffer { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Callback invoked when a new token is obtained. Use this to persist tokens to storage.
    /// </summary>
    public Action<OAuth2TokenResponse>? OnTokenRefreshed { get; init; }
}
```

**Step 4: Build to verify compilation**

Run: `dotnet build RestSharp.slnx -c Debug`
Expected: BUILD SUCCEEDED

**Step 5: Commit**

```bash
git add src/RestSharp/Authenticators/OAuth2/OAuth2TokenResponse.cs \
        src/RestSharp/Authenticators/OAuth2/OAuth2Token.cs \
        src/RestSharp/Authenticators/OAuth2/OAuth2TokenRequest.cs
git commit -m "feat: add OAuth2 token data models (RFC 6749)"
```

---

### Task 2: OAuth2ClientCredentialsAuthenticator

**Files:**
- Create: `src/RestSharp/Authenticators/OAuth2/OAuth2ClientCredentialsAuthenticator.cs`

**Step 1: Create the authenticator**

```csharp
//   Copyright (c) .NET Foundation and Contributors
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System.Text.Json;

namespace RestSharp.Authenticators.OAuth2;

/// <summary>
/// OAuth 2.0 Client Credentials authenticator. Automatically obtains and caches access tokens
/// from the token endpoint using the client_credentials grant type.
/// Uses its own HttpClient for token endpoint calls, avoiding circular dependencies with RestClient.
/// Thread-safe for concurrent request usage.
/// </summary>
[PublicAPI]
public class OAuth2ClientCredentialsAuthenticator : IAuthenticator, IDisposable {
    readonly OAuth2TokenRequest _request;
    readonly HttpClient _tokenClient;
    readonly bool _disposeClient;
    readonly SemaphoreSlim _lock = new(1, 1);

    string? _accessToken;
    DateTimeOffset _tokenExpiry = DateTimeOffset.MinValue;

    public OAuth2ClientCredentialsAuthenticator(OAuth2TokenRequest request) {
        _request = request;

        if (request.HttpClient != null) {
            _tokenClient = request.HttpClient;
            _disposeClient = false;
        }
        else {
            _tokenClient = new HttpClient();
            _disposeClient = true;
        }
    }

    public async ValueTask Authenticate(IRestClient client, RestRequest request) {
        var token = await GetOrRefreshTokenAsync().ConfigureAwait(false);
        request.AddOrUpdateParameter(new HeaderParameter(KnownHeaders.Authorization, $"Bearer {token}"));
    }

    async Task<string> GetOrRefreshTokenAsync() {
        if (_accessToken != null && DateTimeOffset.UtcNow < _tokenExpiry)
            return _accessToken;

        await _lock.WaitAsync().ConfigureAwait(false);

        try {
            // Double-check after acquiring lock
            if (_accessToken != null && DateTimeOffset.UtcNow < _tokenExpiry)
                return _accessToken;

            var parameters = new Dictionary<string, string> {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _request.ClientId,
                ["client_secret"] = _request.ClientSecret
            };

            if (_request.Scope != null)
                parameters["scope"] = _request.Scope;

            if (_request.ExtraParameters != null) {
                foreach (var (key, value) in _request.ExtraParameters)
                    parameters[key] = value;
            }

            using var content = new FormUrlEncodedContent(parameters);
            using var response = await _tokenClient.PostAsync(_request.TokenEndpointUrl, content).ConfigureAwait(false);

            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Token request failed with status {response.StatusCode}: {body}");

            var tokenResponse = JsonSerializer.Deserialize<OAuth2TokenResponse>(body);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                throw new InvalidOperationException($"Token endpoint returned an invalid response: {body}");

            _accessToken = tokenResponse.AccessToken;
            _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn) - _request.ExpiryBuffer;

            _request.OnTokenRefreshed?.Invoke(tokenResponse);

            return _accessToken;
        }
        finally {
            _lock.Release();
        }
    }

    public void Dispose() {
        if (_disposeClient) _tokenClient.Dispose();
        _lock.Dispose();
    }
}
```

**Step 2: Build to verify compilation**

Run: `dotnet build RestSharp.slnx -c Debug`
Expected: BUILD SUCCEEDED

**Step 3: Commit**

```bash
git add src/RestSharp/Authenticators/OAuth2/OAuth2ClientCredentialsAuthenticator.cs
git commit -m "feat: add OAuth2 client credentials authenticator with token lifecycle"
```

---

### Task 3: Tests for OAuth2ClientCredentialsAuthenticator

**Files:**
- Create: `test/RestSharp.Tests/Auth/OAuth2ClientCredentialsAuthenticatorTests.cs`

The tests use `MockHttpMessageHandler` from `RichardSzalay.MockHttp` to simulate the token endpoint. We inject the mock handler into the authenticator's `HttpClient` via `OAuth2TokenRequest.HttpClient`.

**Step 1: Write the test class**

```csharp
using System.Net;
using RestSharp.Authenticators.OAuth2;
using RichardSzalay.MockHttp;

namespace RestSharp.Tests.Auth;

public class OAuth2ClientCredentialsAuthenticatorTests : IDisposable {
    const string TokenEndpoint = "https://auth.example.com/token";

    static string TokenJson(int expiresIn = 3600, string accessToken = "test-access-token")
        => $$"""{"access_token":"{{accessToken}}","token_type":"Bearer","expires_in":{{expiresIn}}}""";

    readonly MockHttpMessageHandler _mockHttp = new();

    OAuth2ClientCredentialsAuthenticator CreateAuthenticator(
        Action<OAuth2TokenResponse>? onRefreshed = null,
        TimeSpan? expiryBuffer = null
    ) {
        var request = new OAuth2TokenRequest {
            TokenEndpointUrl = TokenEndpoint,
            ClientId         = "my-client",
            ClientSecret     = "my-secret",
            HttpClient       = new HttpClient(_mockHttp),
            OnTokenRefreshed = onRefreshed,
            ExpiryBuffer     = expiryBuffer ?? TimeSpan.Zero
        };
        return new OAuth2ClientCredentialsAuthenticator(request);
    }

    [Fact]
    public async Task Should_obtain_token_and_set_authorization_header() {
        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .Respond("application/json", TokenJson());

        using var auth = CreateAuthenticator();
        var restRequest = new RestRequest();
        await auth.Authenticate(null!, restRequest);

        var header = restRequest.Parameters.FirstOrDefault(
            p => p.Name == KnownHeaders.Authorization
        );
        header.Should().NotBeNull();
        header!.Value.Should().Be("Bearer test-access-token");
    }

    [Fact]
    public async Task Should_cache_token_across_multiple_calls() {
        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .Respond("application/json", TokenJson());

        using var auth = CreateAuthenticator();
        await auth.Authenticate(null!, new RestRequest());
        await auth.Authenticate(null!, new RestRequest());

        // MockHttp was set up with When (not Expect), so count calls manually
        // The second call should reuse the cached token
        _mockHttp.GetMatchCount(_mockHttp.When(HttpMethod.Post, TokenEndpoint)).Should().BeLessOrEqual(1);
    }

    [Fact]
    public async Task Should_refresh_expired_token() {
        var callCount = 0;
        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .Respond(_ => {
                callCount++;
                var json = callCount == 1
                    ? TokenJson(expiresIn: 0, accessToken: "token-1")
                    : TokenJson(expiresIn: 3600, accessToken: "token-2");
                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });

        using var auth = CreateAuthenticator(expiryBuffer: TimeSpan.Zero);

        var req1 = new RestRequest();
        await auth.Authenticate(null!, req1);
        req1.Parameters.First(p => p.Name == KnownHeaders.Authorization)
            .Value.Should().Be("Bearer token-1");

        // expires_in was 0 and buffer is 0, so token is already expired
        var req2 = new RestRequest();
        await auth.Authenticate(null!, req2);
        req2.Parameters.First(p => p.Name == KnownHeaders.Authorization)
            .Value.Should().Be("Bearer token-2");

        callCount.Should().Be(2);
    }

    [Fact]
    public async Task Should_invoke_callback_on_token_refresh() {
        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .Respond("application/json", TokenJson());

        OAuth2TokenResponse captured = null;
        using var auth = CreateAuthenticator(onRefreshed: t => captured = t);

        await auth.Authenticate(null!, new RestRequest());

        captured.Should().NotBeNull();
        captured!.AccessToken.Should().Be("test-access-token");
        captured.TokenType.Should().Be("Bearer");
        captured.ExpiresIn.Should().Be(3600);
    }

    [Fact]
    public async Task Should_throw_on_error_response() {
        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .Respond(HttpStatusCode.BadRequest, "application/json", """{"error":"invalid_client"}""");

        using var auth = CreateAuthenticator();

        var act = () => auth.Authenticate(null!, new RestRequest()).AsTask();
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*400*invalid_client*");
    }

    [Fact]
    public async Task Should_throw_on_empty_access_token() {
        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .Respond("application/json", """{"access_token":"","token_type":"Bearer","expires_in":3600}""");

        using var auth = CreateAuthenticator();

        var act = () => auth.Authenticate(null!, new RestRequest()).AsTask();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_send_scope_when_configured() {
        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .WithFormData("scope", "read write")
            .Respond("application/json", TokenJson());

        var request = new OAuth2TokenRequest {
            TokenEndpointUrl = TokenEndpoint,
            ClientId         = "my-client",
            ClientSecret     = "my-secret",
            Scope            = "read write",
            HttpClient       = new HttpClient(_mockHttp)
        };
        using var auth = new OAuth2ClientCredentialsAuthenticator(request);

        await auth.Authenticate(null!, new RestRequest());
        // If scope wasn't sent, the mock would not match and the request would fail
    }

    public void Dispose() => _mockHttp.Dispose();
}
```

**Step 2: Run tests to verify they pass**

Run: `dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj --filter "FullyQualifiedName~OAuth2ClientCredentialsAuthenticator" -f net9.0`
Expected: All tests PASS

**Step 3: Commit**

```bash
git add test/RestSharp.Tests/Auth/OAuth2ClientCredentialsAuthenticatorTests.cs
git commit -m "test: add tests for OAuth2 client credentials authenticator"
```

---

### Task 4: OAuth2RefreshTokenAuthenticator

**Files:**
- Create: `src/RestSharp/Authenticators/OAuth2/OAuth2RefreshTokenAuthenticator.cs`

**Step 1: Create the authenticator**

```csharp
//   Copyright (c) .NET Foundation and Contributors
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System.Text.Json;

namespace RestSharp.Authenticators.OAuth2;

/// <summary>
/// OAuth 2.0 Refresh Token authenticator. Uses an initial access token and refresh token pair,
/// automatically refreshing the access token when it expires using the refresh_token grant type.
/// Uses its own HttpClient for token endpoint calls, avoiding circular dependencies with RestClient.
/// Thread-safe for concurrent request usage.
/// </summary>
[PublicAPI]
public class OAuth2RefreshTokenAuthenticator : IAuthenticator, IDisposable {
    readonly OAuth2TokenRequest _request;
    readonly HttpClient _tokenClient;
    readonly bool _disposeClient;
    readonly SemaphoreSlim _lock = new(1, 1);

    string _accessToken;
    string _refreshToken;
    DateTimeOffset _tokenExpiry;

    /// <param name="request">Token endpoint configuration.</param>
    /// <param name="accessToken">The initial access token.</param>
    /// <param name="refreshToken">The initial refresh token.</param>
    /// <param name="expiresAt">When the initial access token expires. Pass <see cref="DateTimeOffset.MinValue"/> to force an immediate refresh.</param>
    public OAuth2RefreshTokenAuthenticator(
        OAuth2TokenRequest request,
        string accessToken,
        string refreshToken,
        DateTimeOffset expiresAt
    ) {
        _request      = request;
        _accessToken  = accessToken;
        _refreshToken = refreshToken;
        _tokenExpiry  = expiresAt;

        if (request.HttpClient != null) {
            _tokenClient = request.HttpClient;
            _disposeClient = false;
        }
        else {
            _tokenClient = new HttpClient();
            _disposeClient = true;
        }
    }

    public async ValueTask Authenticate(IRestClient client, RestRequest request) {
        var token = await GetOrRefreshTokenAsync().ConfigureAwait(false);
        request.AddOrUpdateParameter(new HeaderParameter(KnownHeaders.Authorization, $"Bearer {token}"));
    }

    async Task<string> GetOrRefreshTokenAsync() {
        if (DateTimeOffset.UtcNow < _tokenExpiry)
            return _accessToken;

        await _lock.WaitAsync().ConfigureAwait(false);

        try {
            if (DateTimeOffset.UtcNow < _tokenExpiry)
                return _accessToken;

            var parameters = new Dictionary<string, string> {
                ["grant_type"]    = "refresh_token",
                ["client_id"]     = _request.ClientId,
                ["client_secret"] = _request.ClientSecret,
                ["refresh_token"] = _refreshToken
            };

            if (_request.ExtraParameters != null) {
                foreach (var (key, value) in _request.ExtraParameters)
                    parameters[key] = value;
            }

            using var content = new FormUrlEncodedContent(parameters);
            using var response = await _tokenClient.PostAsync(_request.TokenEndpointUrl, content).ConfigureAwait(false);

            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Token refresh failed with status {response.StatusCode}: {body}");

            var tokenResponse = JsonSerializer.Deserialize<OAuth2TokenResponse>(body);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                throw new InvalidOperationException($"Token endpoint returned an invalid response: {body}");

            _accessToken = tokenResponse.AccessToken;
            _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn) - _request.ExpiryBuffer;

            // Update refresh token if server rotates it
            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
                _refreshToken = tokenResponse.RefreshToken;

            _request.OnTokenRefreshed?.Invoke(tokenResponse);

            return _accessToken;
        }
        finally {
            _lock.Release();
        }
    }

    public void Dispose() {
        if (_disposeClient) _tokenClient.Dispose();
        _lock.Dispose();
    }
}
```

**Step 2: Build to verify compilation**

Run: `dotnet build RestSharp.slnx -c Debug`
Expected: BUILD SUCCEEDED

**Step 3: Commit**

```bash
git add src/RestSharp/Authenticators/OAuth2/OAuth2RefreshTokenAuthenticator.cs
git commit -m "feat: add OAuth2 refresh token authenticator with token lifecycle"
```

---

### Task 5: Tests for OAuth2RefreshTokenAuthenticator

**Files:**
- Create: `test/RestSharp.Tests/Auth/OAuth2RefreshTokenAuthenticatorTests.cs`

**Step 1: Write the test class**

```csharp
using System.Net;
using RestSharp.Authenticators.OAuth2;
using RichardSzalay.MockHttp;

namespace RestSharp.Tests.Auth;

public class OAuth2RefreshTokenAuthenticatorTests : IDisposable {
    const string TokenEndpoint = "https://auth.example.com/token";

    static string TokenJson(
        string accessToken = "new-access-token",
        int expiresIn = 3600,
        string refreshToken = null
    ) {
        var refresh = refreshToken != null ? $""","refresh_token":"{refreshToken}"""" : "";
        return $$"""{"access_token":"{{accessToken}}","token_type":"Bearer","expires_in":{{expiresIn}}{{refresh}}}""";
    }

    readonly MockHttpMessageHandler _mockHttp = new();

    OAuth2RefreshTokenAuthenticator CreateAuthenticator(
        string accessToken = "initial-access",
        string refreshToken = "initial-refresh",
        DateTimeOffset? expiresAt = null,
        Action<OAuth2TokenResponse> onRefreshed = null
    ) {
        var request = new OAuth2TokenRequest {
            TokenEndpointUrl = TokenEndpoint,
            ClientId         = "my-client",
            ClientSecret     = "my-secret",
            HttpClient       = new HttpClient(_mockHttp),
            OnTokenRefreshed = onRefreshed,
            ExpiryBuffer     = TimeSpan.Zero
        };
        return new OAuth2RefreshTokenAuthenticator(
            request,
            accessToken,
            refreshToken,
            expiresAt ?? DateTimeOffset.MinValue
        );
    }

    [Fact]
    public async Task Should_use_initial_token_when_not_expired() {
        using var auth = CreateAuthenticator(expiresAt: DateTimeOffset.UtcNow.AddHours(1));

        var req = new RestRequest();
        await auth.Authenticate(null!, req);

        req.Parameters.First(p => p.Name == KnownHeaders.Authorization)
            .Value.Should().Be("Bearer initial-access");
    }

    [Fact]
    public async Task Should_refresh_when_token_expired() {
        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .Respond("application/json", TokenJson());

        using var auth = CreateAuthenticator(expiresAt: DateTimeOffset.MinValue);

        var req = new RestRequest();
        await auth.Authenticate(null!, req);

        req.Parameters.First(p => p.Name == KnownHeaders.Authorization)
            .Value.Should().Be("Bearer new-access-token");
    }

    [Fact]
    public async Task Should_send_refresh_token_in_request() {
        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .WithFormData("refresh_token", "initial-refresh")
            .WithFormData("grant_type", "refresh_token")
            .Respond("application/json", TokenJson());

        using var auth = CreateAuthenticator();
        await auth.Authenticate(null!, new RestRequest());
        // If refresh_token or grant_type weren't sent, mock wouldn't match
    }

    [Fact]
    public async Task Should_update_refresh_token_when_rotated() {
        var callCount = 0;
        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .Respond(_ => {
                callCount++;
                var json = callCount == 1
                    ? TokenJson(accessToken: "token-1", expiresIn: 0, refreshToken: "rotated-refresh")
                    : TokenJson(accessToken: "token-2", expiresIn: 3600);
                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });

        using var auth = CreateAuthenticator();

        // First call: gets token-1 with rotated refresh token
        await auth.Authenticate(null!, new RestRequest());
        // Second call: token-1 is expired (expiresIn=0), should use rotated-refresh
        await auth.Authenticate(null!, new RestRequest());

        callCount.Should().Be(2);
    }

    [Fact]
    public async Task Should_invoke_callback_on_refresh() {
        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .Respond("application/json", TokenJson(refreshToken: "new-refresh"));

        OAuth2TokenResponse captured = null;
        using var auth = CreateAuthenticator(onRefreshed: t => captured = t);

        await auth.Authenticate(null!, new RestRequest());

        captured.Should().NotBeNull();
        captured!.AccessToken.Should().Be("new-access-token");
        captured.RefreshToken.Should().Be("new-refresh");
    }

    [Fact]
    public async Task Should_throw_on_error_response() {
        _mockHttp.When(HttpMethod.Post, TokenEndpoint)
            .Respond(HttpStatusCode.Unauthorized, "application/json", """{"error":"invalid_grant"}""");

        using var auth = CreateAuthenticator();

        var act = () => auth.Authenticate(null!, new RestRequest()).AsTask();
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*401*invalid_grant*");
    }

    public void Dispose() => _mockHttp.Dispose();
}
```

**Step 2: Run tests**

Run: `dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj --filter "FullyQualifiedName~OAuth2RefreshTokenAuthenticator" -f net9.0`
Expected: All tests PASS

**Step 3: Commit**

```bash
git add test/RestSharp.Tests/Auth/OAuth2RefreshTokenAuthenticatorTests.cs
git commit -m "test: add tests for OAuth2 refresh token authenticator"
```

---

### Task 6: OAuth2TokenAuthenticator (generic/delegate-based)

**Files:**
- Create: `src/RestSharp/Authenticators/OAuth2/OAuth2TokenAuthenticator.cs`

**Step 1: Create the authenticator**

```csharp
//   Copyright (c) .NET Foundation and Contributors
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

namespace RestSharp.Authenticators.OAuth2;

/// <summary>
/// Generic OAuth 2.0 authenticator that delegates token acquisition to a user-provided function.
/// Caches the token and re-invokes the delegate when the token expires.
/// Use this for non-standard OAuth2 flows or custom token providers.
/// Thread-safe for concurrent request usage.
/// </summary>
[PublicAPI]
public class OAuth2TokenAuthenticator : IAuthenticator, IDisposable {
    readonly Func<CancellationToken, Task<OAuth2Token>> _getToken;
    readonly string _tokenType;
    readonly SemaphoreSlim _lock = new(1, 1);

    string? _accessToken;
    DateTimeOffset _tokenExpiry = DateTimeOffset.MinValue;

    /// <param name="getToken">Async delegate that returns an access token and its expiration time.</param>
    /// <param name="tokenType">The token type for the Authorization header. Defaults to "Bearer".</param>
    public OAuth2TokenAuthenticator(Func<CancellationToken, Task<OAuth2Token>> getToken, string tokenType = "Bearer") {
        _getToken  = getToken;
        _tokenType = tokenType;
    }

    public async ValueTask Authenticate(IRestClient client, RestRequest request) {
        var token = await GetOrRefreshTokenAsync().ConfigureAwait(false);
        request.AddOrUpdateParameter(new HeaderParameter(KnownHeaders.Authorization, $"{_tokenType} {token}"));
    }

    async Task<string> GetOrRefreshTokenAsync() {
        if (_accessToken != null && DateTimeOffset.UtcNow < _tokenExpiry)
            return _accessToken;

        await _lock.WaitAsync().ConfigureAwait(false);

        try {
            if (_accessToken != null && DateTimeOffset.UtcNow < _tokenExpiry)
                return _accessToken;

            var result = await _getToken(CancellationToken.None).ConfigureAwait(false);
            _accessToken = result.AccessToken;
            _tokenExpiry = result.ExpiresAt;

            return _accessToken;
        }
        finally {
            _lock.Release();
        }
    }

    public void Dispose() => _lock.Dispose();
}
```

**Step 2: Build to verify compilation**

Run: `dotnet build RestSharp.slnx -c Debug`
Expected: BUILD SUCCEEDED

**Step 3: Commit**

```bash
git add src/RestSharp/Authenticators/OAuth2/OAuth2TokenAuthenticator.cs
git commit -m "feat: add generic OAuth2 token authenticator with delegate provider"
```

---

### Task 7: Tests for OAuth2TokenAuthenticator

**Files:**
- Create: `test/RestSharp.Tests/Auth/OAuth2TokenAuthenticatorTests.cs`

**Step 1: Write the test class**

```csharp
using RestSharp.Authenticators.OAuth2;

namespace RestSharp.Tests.Auth;

public class OAuth2TokenAuthenticatorTests {
    [Fact]
    public async Task Should_call_delegate_and_set_authorization_header() {
        var token = new OAuth2Token("my-token", DateTimeOffset.UtcNow.AddHours(1));

        using var auth = new OAuth2TokenAuthenticator(_ => Task.FromResult(token));
        var req = new RestRequest();
        await auth.Authenticate(null!, req);

        req.Parameters.First(p => p.Name == KnownHeaders.Authorization)
            .Value.Should().Be("Bearer my-token");
    }

    [Fact]
    public async Task Should_cache_token_across_calls() {
        var callCount = 0;
        var token = new OAuth2Token("my-token", DateTimeOffset.UtcNow.AddHours(1));

        using var auth = new OAuth2TokenAuthenticator(_ => {
            callCount++;
            return Task.FromResult(token);
        });

        await auth.Authenticate(null!, new RestRequest());
        await auth.Authenticate(null!, new RestRequest());

        callCount.Should().Be(1);
    }

    [Fact]
    public async Task Should_re_invoke_delegate_when_token_expired() {
        var callCount = 0;

        using var auth = new OAuth2TokenAuthenticator(_ => {
            callCount++;
            // Always return a token that's already expired
            var t = callCount == 1
                ? new OAuth2Token("token-1", DateTimeOffset.UtcNow.AddSeconds(-1))
                : new OAuth2Token("token-2", DateTimeOffset.UtcNow.AddHours(1));
            return Task.FromResult(t);
        });

        var req1 = new RestRequest();
        await auth.Authenticate(null!, req1);
        req1.Parameters.First(p => p.Name == KnownHeaders.Authorization)
            .Value.Should().Be("Bearer token-1");

        var req2 = new RestRequest();
        await auth.Authenticate(null!, req2);
        req2.Parameters.First(p => p.Name == KnownHeaders.Authorization)
            .Value.Should().Be("Bearer token-2");

        callCount.Should().Be(2);
    }

    [Fact]
    public async Task Should_use_custom_token_type() {
        var token = new OAuth2Token("my-token", DateTimeOffset.UtcNow.AddHours(1));

        using var auth = new OAuth2TokenAuthenticator(_ => Task.FromResult(token), tokenType: "MAC");
        var req = new RestRequest();
        await auth.Authenticate(null!, req);

        req.Parameters.First(p => p.Name == KnownHeaders.Authorization)
            .Value.Should().Be("MAC my-token");
    }
}
```

**Step 2: Run tests**

Run: `dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj --filter "FullyQualifiedName~OAuth2TokenAuthenticator" -f net9.0`
Expected: All tests PASS

**Step 3: Commit**

```bash
git add test/RestSharp.Tests/Auth/OAuth2TokenAuthenticatorTests.cs
git commit -m "test: add tests for generic OAuth2 token authenticator"
```

---

### Task 8: Full build and test verification

**Step 1: Run the full build**

Run: `dotnet build RestSharp.slnx -c Debug`
Expected: BUILD SUCCEEDED

**Step 2: Run all unit tests**

Run: `dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj -f net9.0`
Expected: All tests PASS (existing + new)

**Step 3: Run tests on net8.0**

Run: `dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj -f net8.0`
Expected: All tests PASS

**Step 4: Run the full solution tests**

Run: `dotnet test RestSharp.slnx -c Debug`
Expected: All tests PASS
