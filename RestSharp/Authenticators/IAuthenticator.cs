using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp
{
	public interface IAuthenticator
	{
		void Authenticate(RestRequest request);
	}
}
