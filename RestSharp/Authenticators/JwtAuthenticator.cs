#region License

//   Author: Roman Kravchik
//   Based on HttpBasicAuthenticator class by John Sheehan
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

#endregion

using System;
using System.Linq;
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
        bool _forceTokenUpdate;

        // ReSharper disable once IntroduceOptionalParameters.Global
        public JwtAuthenticator(string accessToken) : this(accessToken, false)
        {
        }

        public JwtAuthenticator(string accessToken, bool forceTokenUpdate)
        {
            SetBearerToken(accessToken);
            _forceTokenUpdate = forceTokenUpdate;
        }

        /// <summary>
        /// Set the new bearer token so the request gets the new header value
        /// </summary>
        /// <param name="accessToken"></param>
        public void SetBearerToken(string accessToken)
        {
            Ensure.NotEmpty(accessToken, nameof(accessToken));

            _authHeader = $"Bearer {accessToken}";
            _forceTokenUpdate = true;
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            if (_forceTokenUpdate)
            {
                request.AddOrUpdateParameter("Authorization", _authHeader, ParameterType.HttpHeader);
                return;
            }

            // only add the Authorization parameter if it hasn't been added by a previous Execute
            if (!request.Parameters.Any(p => p.Type.Equals(ParameterType.HttpHeader) &&
                                             p.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase)))
                request.AddParameter("Authorization", _authHeader, ParameterType.HttpHeader);
        }
    }
}