using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
