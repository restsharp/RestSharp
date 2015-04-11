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

namespace RestSharp.Authenticators
{
    public class SimpleAuthenticator : IAuthenticator
    {
        private readonly string _usernameKey;
        private readonly string _username;
        private readonly string _passwordKey;
        private readonly string _password;

        public SimpleAuthenticator(string usernameKey, string username, string passwordKey, string password)
        {
            this._usernameKey = usernameKey;
            this._username = username;
            this._passwordKey = passwordKey;
            this._password = password;
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            request.AddParameter(this._usernameKey, this._username);
            request.AddParameter(this._passwordKey, this._password);
        }
    }
}
