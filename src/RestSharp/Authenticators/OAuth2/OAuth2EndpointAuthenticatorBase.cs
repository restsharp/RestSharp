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
/// Base class for OAuth 2.0 authenticators that call a token endpoint.
/// Handles HttpClient lifecycle, thread-safe token caching with double-check locking,
/// token response parsing, error handling, and the OnTokenRefreshed callback.
/// </summary>
public abstract class OAuth2EndpointAuthenticatorBase : IAuthenticator, IDisposable {
    readonly HttpClient _tokenClient;
    readonly bool _disposeClient;
    readonly SemaphoreSlim _lock = new(1, 1);

    protected OAuth2TokenRequest TokenRequest { get; }

    string? _accessToken;
    DateTimeOffset _tokenExpiry = DateTimeOffset.MinValue;

    protected OAuth2EndpointAuthenticatorBase(OAuth2TokenRequest request) {
        TokenRequest = request;

        if (request.HttpClient != null) {
            _tokenClient = request.HttpClient;
            _disposeClient = false;
        }
        else {
            _tokenClient = new HttpClient();
            _disposeClient = true;
        }
    }

    protected void SetInitialToken(string accessToken, DateTimeOffset expiresAt) {
        _accessToken = accessToken;
        _tokenExpiry = expiresAt;
    }

    public async ValueTask Authenticate(IRestClient client, RestRequest request, CancellationToken cancellationToken = default) {
        var token = await GetOrRefreshTokenAsync(cancellationToken).ConfigureAwait(false);
        request.AddOrUpdateParameter(new HeaderParameter(KnownHeaders.Authorization, $"Bearer {token}"));
    }

    /// <summary>
    /// Build the grant-specific form parameters for the token request.
    /// </summary>
    protected abstract Dictionary<string, string> BuildRequestParameters();

    /// <summary>
    /// Called after a successful token response. Override to handle grant-specific
    /// fields such as refresh token rotation.
    /// </summary>
    protected virtual void OnTokenResponse(OAuth2TokenResponse response) { }

    async Task<string> GetOrRefreshTokenAsync(CancellationToken cancellationToken) {
        if (_accessToken != null && DateTimeOffset.UtcNow < _tokenExpiry)
            return _accessToken;

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try {
            if (_accessToken != null && DateTimeOffset.UtcNow < _tokenExpiry)
                return _accessToken;

            var parameters = BuildRequestParameters();

            if (TokenRequest.ExtraParameters != null) {
                foreach (var kvp in TokenRequest.ExtraParameters)
                    parameters[kvp.Key] = kvp.Value;
            }

            using var content = new FormUrlEncodedContent(parameters);
            using var response = await _tokenClient.PostAsync(TokenRequest.TokenEndpointUrl, content, cancellationToken).ConfigureAwait(false);

            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Token request failed with status {response.StatusCode}: {body}");

            var tokenResponse = JsonSerializer.Deserialize<OAuth2TokenResponse>(body);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                throw new InvalidOperationException($"Token endpoint returned an invalid response: {body}");

            _accessToken = tokenResponse.AccessToken;
            _tokenExpiry = tokenResponse.ExpiresIn.HasValue
                ? DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn.Value) - TokenRequest.ExpiryBuffer
                : DateTimeOffset.MaxValue;

            OnTokenResponse(tokenResponse);
            TokenRequest.OnTokenRefreshed?.Invoke(tokenResponse);

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
