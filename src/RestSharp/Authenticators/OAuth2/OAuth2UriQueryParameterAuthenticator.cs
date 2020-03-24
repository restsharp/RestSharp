//   Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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

// ReSharper disable CheckNamespace

namespace RestSharp.Authenticators
{
    /// <summary>
    ///     The OAuth 2 authenticator using URI query parameter.
    /// </summary>
    /// <remarks>
    ///     Based on http://tools.ietf.org/html/draft-ietf-oauth-v2-10#section-5.1.2
    /// </remarks>
    public class OAuth2UriQueryParameterAuthenticator : AuthenticatorBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OAuth2UriQueryParameterAuthenticator" /> class.
        /// </summary>
        /// <param name="accessToken">
        ///     The access token.
        /// </param>
        public OAuth2UriQueryParameterAuthenticator(string accessToken)
            : base(accessToken) { }

        protected override Parameter GetAuthenticationParameter(string accessToken)
            => new Parameter("oauth_token", accessToken, ParameterType.GetOrPost);
    }
}