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

namespace RestSharp.Authenticators
{
    /// <summary>
    /// JSON WEB TOKEN (JWT) Authenticator class. 
    /// <remarks>https://tools.ietf.org/html/draft-ietf-oauth-json-web-token</remarks>
    /// </summary>
    public class JwtAuthenticator : IAuthenticator
    {
        private readonly string authHeader;

        public JwtAuthenticator(string accessToken)
        {
            if (accessToken == null)
            {
                throw new ArgumentNullException("accessToken");
            }

            this.authHeader = string.Format("Bearer {0}", accessToken);
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            // only add the Authorization parameter if it hasn't been added by a previous Execute
            if (!request.Parameters.Any(p => p.Type.Equals(ParameterType.HttpHeader) &&
                                             p.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase)))
            {
                request.AddParameter("Authorization", this.authHeader, ParameterType.HttpHeader);
            }
        }
    }
}
