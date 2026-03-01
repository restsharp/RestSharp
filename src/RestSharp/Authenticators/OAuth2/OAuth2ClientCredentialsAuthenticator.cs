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
/// OAuth 2.0 Client Credentials authenticator. Automatically obtains and caches access tokens
/// from the token endpoint using the client_credentials grant type.
/// Uses its own HttpClient for token endpoint calls, avoiding circular dependencies with RestClient.
/// Thread-safe for concurrent request usage.
/// </summary>
[PublicAPI]
public class OAuth2ClientCredentialsAuthenticator(OAuth2TokenRequest request)
    : OAuth2EndpointAuthenticatorBase(request) {
    protected override Dictionary<string, string> BuildRequestParameters() {
        var parameters = new Dictionary<string, string> {
            ["grant_type"]    = "client_credentials",
            ["client_id"]     = TokenRequest.ClientId,
            ["client_secret"] = TokenRequest.ClientSecret
        };

        if (TokenRequest.Scope != null)
            parameters["scope"] = TokenRequest.Scope;

        return parameters;
    }
}
