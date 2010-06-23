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

namespace RestSharp
{
	/// <summary>
	/// Tries to Authenticate with the credentials of the currently logged in user
	/// </summary>
	public class NtlmAuthenticator : IAuthenticator
	{
		public void Authenticate(RestRequest request) {
			request.Credentials = System.Net.CredentialCache.DefaultCredentials;
		}
	}
}

#endif