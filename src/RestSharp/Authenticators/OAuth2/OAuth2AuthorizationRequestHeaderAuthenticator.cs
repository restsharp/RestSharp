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
    ///     The OAuth 2 authenticator using the authorization request header field.
    /// </summary>
    /// <remarks>
    ///     Based on http://tools.ietf.org/html/draft-ietf-oauth-v2-10#section-5.1.1
    /// </remarks>
    public class OAuth2AuthorizationRequestHeaderAuthenticator : AuthenticatorBase
    {
        readonly string _tokenType;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OAuth2AuthorizationRequestHeaderAuthenticator" /> class.
        /// </summary>
        /// <param name="accessToken">
        ///     The access token.
        /// </param>
        public OAuth2AuthorizationRequestHeaderAuthenticator(string accessToken)
            : this(accessToken, "OAuth") { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OAuth2AuthorizationRequestHeaderAuthenticator" /> class.
        /// </summary>
        /// <param name="accessToken">
        ///     The access token.
        /// </param>
        /// <param name="tokenType">
        ///     The token type.
        /// </param>
        public OAuth2AuthorizationRequestHeaderAuthenticator(string accessToken, string tokenType)
            : base(accessToken)
            => _tokenType = tokenType;

        protected override Parameter GetAuthenticationParameter(string accessToken)
            => new Parameter("Authorization", $"{_tokenType} {accessToken}", ParameterType.HttpHeader);
    }
}