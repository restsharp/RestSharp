using System;
using System.Linq;

namespace RestSharp
{
	/// <summary>
	/// Base class for OAuth 2 Authenticators.
	/// </summary>
	/// <remarks>
	/// Since there are many ways to authenticate in OAuth2,
	/// this is used as a base class to differentiate between 
	/// other authenticators.
	/// 
	/// Any other OAuth2 authenticators must derive from this
	/// abstract class.
	/// </remarks>
	public abstract class OAuth2Authenticator : IAuthenticator
	{
		/// <summary>
		/// Access token to be used when authenticating.
		/// </summary>
		private readonly string _accessToken;

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth2Authenticator"/> class.
		/// </summary>
		/// <param name="accessToken">
		/// The access token.
		/// </param>
		public OAuth2Authenticator(string accessToken)
		{
			_accessToken = accessToken;
		}

		/// <summary>
		/// Gets the access token.
		/// </summary>
		public string AccessToken
		{
			get { return _accessToken; }
		}

		public abstract void Authenticate(IRestClient client, IRestRequest request);
	}

	/// <summary>
	/// The OAuth 2 authenticator using URI query parameter.
	/// </summary>
	/// <remarks>
	/// Based on http://tools.ietf.org/html/draft-ietf-oauth-v2-10#section-5.1.2
	/// </remarks>
	public class OAuth2UriQueryParameterAuthenticator : OAuth2Authenticator
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth2UriQueryParameterAuthenticator"/> class.
		/// </summary>
		/// <param name="accessToken">
		/// The access token.
		/// </param>
		public OAuth2UriQueryParameterAuthenticator(string accessToken)
			: base(accessToken)
		{
		}

		public override void Authenticate(IRestClient client, IRestRequest request)
		{
			request.AddParameter("oauth_token", AccessToken, ParameterType.GetOrPost);
		}
	}

	/// <summary>
	/// The OAuth 2 authenticator using the authorization request header field.
	/// </summary>
	/// <remarks>
	/// Based on http://tools.ietf.org/html/draft-ietf-oauth-v2-10#section-5.1.1
	/// </remarks>
	public class OAuth2AuthorizationRequestHeaderAuthenticator : OAuth2Authenticator
	{
		/// <summary>
		/// Stores the Authorization header value as "[tokenType] accessToken". used for performance.
		/// </summary>
		private readonly string _authorizationValue;

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth2AuthorizationRequestHeaderAuthenticator"/> class.
		/// </summary>
		/// <param name="accessToken">
		/// The access token.
		/// </param>
		public OAuth2AuthorizationRequestHeaderAuthenticator(string accessToken)
			: this(accessToken, "OAuth")
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth2AuthorizationRequestHeaderAuthenticator"/> class.
		/// </summary>
		/// <param name="accessToken">
		/// The access token.
		/// </param>
		/// <param name="tokenType">
		/// The token type.
		/// </param>
		public OAuth2AuthorizationRequestHeaderAuthenticator(string accessToken, string tokenType)
			: base(accessToken)
		{
			// Conatenate during constructor so that it is only done once. can improve performance.
			_authorizationValue = tokenType + " " + accessToken;
		}

		public override void Authenticate(IRestClient client, IRestRequest request)
		{
			// only add the Authorization parameter if it hasn't been added.
			if (!request.Parameters.Any(p => p.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase)))
			{
				request.AddParameter("Authorization", _authorizationValue, ParameterType.HttpHeader);
			}
		}
	}
}