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
/// OAuth 2.0 Refresh Token authenticator. Uses an initial access token and refresh token pair,
/// automatically refreshing the access token when it expires using the refresh_token grant type.
/// Uses its own HttpClient for token endpoint calls, avoiding circular dependencies with RestClient.
/// Thread-safe for concurrent request usage.
/// </summary>
[PublicAPI]
public class OAuth2RefreshTokenAuthenticator : OAuth2EndpointAuthenticatorBase {
    string _refreshToken;

    /// <param name="request">Token endpoint configuration.</param>
    /// <param name="accessToken">The initial access token.</param>
    /// <param name="refreshToken">The initial refresh token.</param>
    /// <param name="expiresAt">When the initial access token expires. Pass <see cref="DateTimeOffset.MinValue"/> to force an immediate refresh.</param>
    public OAuth2RefreshTokenAuthenticator(
        OAuth2TokenRequest request,
        string accessToken,
        string refreshToken,
        DateTimeOffset expiresAt
    ) : base(request) {
        _refreshToken = refreshToken;
        SetInitialToken(accessToken, expiresAt);
    }

    protected override Dictionary<string, string> BuildRequestParameters()
        => new() {
            ["grant_type"]    = "refresh_token",
            ["client_id"]     = TokenRequest.ClientId,
            ["client_secret"] = TokenRequest.ClientSecret,
            ["refresh_token"] = _refreshToken
        };

    protected override void OnTokenResponse(OAuth2TokenResponse response) {
        if (!string.IsNullOrEmpty(response.RefreshToken))
            _refreshToken = response.RefreshToken;
    }
}
