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

using RestSharp.Validation;

namespace RestSharp.Authenticators
{
    /// <summary>
    ///     JSON WEB TOKEN (JWT) Authenticator class.
    ///     <remarks>https://tools.ietf.org/html/draft-ietf-oauth-json-web-token</remarks>
    /// </summary>
    public class JwtAuthenticator : IAuthenticator
    {
        string _authHeader;

        // ReSharper disable once IntroduceOptionalParameters.Global
        public JwtAuthenticator(string accessToken) => SetBearerToken(accessToken);

        /// <summary>
        /// Set the new bearer token so the request gets the new header value
        /// </summary>
        /// <param name="accessToken"></param>
        public void SetBearerToken(string accessToken)
        {
            Ensure.NotEmpty(accessToken, nameof(accessToken));

            _authHeader = $"Bearer {accessToken}";
        }

        public void Authenticate(IRestClient client, IRestRequest request)
            => request.AddOrUpdateParameter("Authorization", _authHeader, ParameterType.HttpHeader);
    }
}