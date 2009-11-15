using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp
{
	public class SimpleAuthenticator : IAuthenticator
	{
		public string _usernameKey { get; set; }
		public string _username { get; set; }
		public string _passwordKey { get; set; }
		public string _password { get; set; }

		public SimpleAuthenticator(string UsernameKey, string Username, string PasswordKey, string Password) {
			_password = Password;
			_passwordKey = PasswordKey;
			_username = Username;
			_usernameKey = UsernameKey;
		}

		public void Authenticate(RestRequest request) {
			request.AddParameter(_usernameKey, _username);
			request.AddParameter(_passwordKey, _password);
		}
	}
}
