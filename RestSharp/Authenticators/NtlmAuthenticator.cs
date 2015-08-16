#region License

//   Copyright 2010 John Sheehan
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

#if FRAMEWORK

using System;
using System.Net;

namespace RestSharp.Authenticators
{
    /// <summary>
    /// Tries to Authenticate with the credentials of the currently logged in user, or impersonate a user
    /// </summary>
    public class NtlmAuthenticator : IAuthenticator
    {
        private readonly ICredentials credentials;

        /// <summary>
        /// Authenticate with the credentials of the currently logged in user
        /// </summary>
        public NtlmAuthenticator()
            : this(CredentialCache.DefaultCredentials) { }

        /// <summary>
        /// Authenticate by impersonation
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public NtlmAuthenticator(string username, string password)
            : this(new NetworkCredential(username, password)) { }

        /// <summary>
        /// Authenticate by impersonation, using an existing <c>ICredentials</c> instance
        /// </summary>
        /// <param name="credentials"></param>
        public NtlmAuthenticator(ICredentials credentials)
        {
            if (credentials == null)
            {
                throw new ArgumentNullException("credentials");
            }

            this.credentials = credentials;
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            request.Credentials = this.credentials;
        }
    }
}

#endif
