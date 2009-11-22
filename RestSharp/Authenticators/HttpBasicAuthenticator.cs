#region License
//   Copyright 2009 John Sheehan
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

using System.Net;

namespace RestSharp
{
	public class HttpBasicAuthenticator : IAuthenticator
	{
		public string _username { get; set; }
		public string _password { get; set; }

		public HttpBasicAuthenticator(string Username, string Password) {
			_password = Password;
			_username = Username;
		}

		public void Authenticate(RestRequest request) {
			request.Credentials = new NetworkCredential(_username, _password);
		}
	}
}
