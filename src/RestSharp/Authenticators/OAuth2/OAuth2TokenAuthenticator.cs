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
