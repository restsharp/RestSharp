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
    /// Creates a new instance of <see cref="OAuth2TokenRequest"/>.
    /// </summary>
    /// <param name="tokenEndpointUrl">The URL of the OAuth 2.0 token endpoint.</param>
    /// <param name="clientId">The OAuth 2.0 client identifier.</param>
    /// <param name="clientSecret">The OAuth 2.0 client secret.</param>
    public OAuth2TokenRequest(string tokenEndpointUrl, string clientId, string clientSecret) {
        TokenEndpointUrl = Ensure.NotNull(tokenEndpointUrl, nameof(tokenEndpointUrl));
        ClientId         = Ensure.NotNull(clientId, nameof(clientId));
        ClientSecret     = Ensure.NotNull(clientSecret, nameof(clientSecret));
    }

    /// <summary>
    /// The URL of the OAuth 2.0 token endpoint.
    /// </summary>
    public string TokenEndpointUrl { get; }

    /// <summary>
    /// The OAuth 2.0 client identifier.
    /// </summary>
    public string ClientId { get; }

    /// <summary>
    /// The OAuth 2.0 client secret.
    /// </summary>
    public string ClientSecret { get; }

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
