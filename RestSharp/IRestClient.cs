using System;
namespace RestSharp
{
	public interface IRestClient
	{
		IAuthenticator Authenticator { get; set; }
		X Execute<X>(RestRequest request) where X : new();
		RestResponse Execute(RestRequest request);
	}
}
