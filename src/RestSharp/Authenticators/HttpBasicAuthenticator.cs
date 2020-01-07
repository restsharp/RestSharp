#region License

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

#endregion

using System;
using System.Linq;
using System.Text;

namespace RestSharp.Authenticators
{
    /// <summary>
    /// Allows "basic access authentication" for HTTP requests.
    /// </summary>
    /// <remarks>
    /// Encoding can be specified depending on what your server expect (see https://stackoverflow.com/a/7243567).
    /// UTF-8 is used by default but some servers might expect ISO-8859-1 encoding.
    /// </remarks>
    public class HttpBasicAuthenticator : IAuthenticator
    {
        readonly string _authHeader;

        public HttpBasicAuthenticator(string username, string password)
            : this(username, password, Encoding.UTF8)
        {
        }

        public HttpBasicAuthenticator(string username, string password, Encoding encoding)
        {
            var token = Convert.ToBase64String(encoding.GetBytes($"{username}:{password}"));
            
            _authHeader = $"Basic {token}";
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            // NetworkCredentials always makes two trips, even if with PreAuthenticate,
            // it is also unsafe for many partial trust scenarios
            // request.Credentials = Credentials;
            // thanks TweetSharp!

            // request.Credentials = new NetworkCredential(_username, _password);

            // only add the Authorization parameter if it hasn't been added by a previous Execute
            if (!request.Parameters.Any(p => "Authorization".Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                request.AddParameter("Authorization", _authHeader, ParameterType.HttpHeader);
        }
    }
}