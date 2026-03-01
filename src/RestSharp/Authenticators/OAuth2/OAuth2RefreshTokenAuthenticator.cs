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
                foreach (var kvp in _request.ExtraParameters)
                    parameters[kvp.Key] = kvp.Value;
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
