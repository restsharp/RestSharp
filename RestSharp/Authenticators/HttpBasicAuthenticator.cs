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

using System;
using System.Linq;
using System.Text;

namespace RestSharp
{
	public class HttpBasicAuthenticator : IAuthenticator
	{
		private readonly string _username;
		private readonly string _password;

		public HttpBasicAuthenticator(string username, string password) {
			_password = password;
			_username = username;
		}

		public void Authenticate(IRestClient client, IRestRequest request) {
			// NetworkCredentials always makes two trips, even if with PreAuthenticate,
			// it is also unsafe for many partial trust scenarios
			// request.Credentials = Credentials;
			// thanks TweetSharp!

			// request.Credentials = new NetworkCredential(_username, _password);

			// only add the Authorization parameter if it hasn't been added by a previous Execute
			if (!request.Parameters.Any(p => p.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase)))
			{
				var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", _username, _password)));
				var authHeader = string.Format("Basic {0}", token);
				request.AddParameter("Authorization", authHeader, ParameterType.HttpHeader);
			}
		}
	}
}
