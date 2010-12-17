using System;
using RestSharp.Authenticators.OAuth;
using RestSharp.Contrib;

namespace RestSharp.Authenticators
{
    public class OAuth1Authenticator : IAuthenticator
    {
        public virtual OAuthParameterHandling ParameterHandling { get; set; }
        public virtual OAuthSignatureMethod SignatureMethod { get; set; }
        public virtual OAuthSignatureTreatment SignatureTreatment { get; set; }

        internal virtual OAuthType Type { get; set; }
        internal virtual string ConsumerKey { get; set; }
        internal virtual string ConsumerSecret { get; set; }
        internal virtual string Token { get; set; }
        internal virtual string TokenSecret { get; set; }
        internal virtual string Verifier { get; set; }
        internal virtual string Version { get; set; }
        internal virtual string CallbackUrl { get; set; }
        internal virtual string SessionHandle { get; set; }
        internal virtual string ClientUsername { get; set; }
        internal virtual string ClientPassword { get; set; }

        public static OAuth1Authenticator ForRequestToken(string consumerKey, string consumerSecret)
        {
            var authenticator = new OAuth1Authenticator
                                    {
                                        ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                                        SignatureMethod = OAuthSignatureMethod.HmacSha1,
                                        SignatureTreatment = OAuthSignatureTreatment.Escaped,
                                        ConsumerKey = consumerKey,
                                        ConsumerSecret = consumerSecret
                                    };
            return authenticator;
        }

        public static OAuth1Authenticator ForRequestToken(string consumerKey, string consumerSecret, string callbackUrl)
        {
            var authenticator = ForRequestToken(consumerKey, consumerSecret);
            authenticator.CallbackUrl = callbackUrl;
            return authenticator;
        }

        public static OAuth1Authenticator ForAccessToken(string consumerKey, string consumerSecret, string token, string tokenSecret)
        {
            var authenticator = new OAuth1Authenticator
                                    {
                                        ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                                        SignatureMethod = OAuthSignatureMethod.HmacSha1,
                                        SignatureTreatment = OAuthSignatureTreatment.Escaped,
                                        ConsumerKey = consumerKey,
                                        ConsumerSecret = consumerSecret,
                                        Token = token,
                                        TokenSecret = tokenSecret
                                    };
            return authenticator;
        }

        public static OAuth1Authenticator ForAccessToken(string consumerKey, string consumerSecret, string token, string tokenSecret, string verifier)
        {
            var authenticator = ForAccessToken(consumerKey, consumerSecret, token, tokenSecret);
            authenticator.Verifier = verifier;
            return authenticator;
        }

        public static OAuth1Authenticator ForAccessTokenRefresh(string consumerKey, string consumerSecret, string token, string tokenSecret, string sessionHandle)
        {
            var authenticator = ForAccessToken(consumerKey, consumerSecret, token, tokenSecret);
            authenticator.SessionHandle = sessionHandle;
            return authenticator;
        }

        public static OAuth1Authenticator ForAccessTokenRefresh(string consumerKey, string consumerSecret, string token, string tokenSecret, string verifier, string sessionHandle)
        {
            var authenticator = ForAccessToken(consumerKey, consumerSecret, token, tokenSecret);
            authenticator.SessionHandle = sessionHandle;
            authenticator.Verifier = verifier;
            return authenticator;
        }

        public static OAuth1Authenticator ForClientAuthentication(string consumerKey, string consumerSecret, string username, string password)
        {
            var authenticator = new OAuth1Authenticator
            {
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                SignatureTreatment = OAuthSignatureTreatment.Escaped,
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                ClientUsername = username,
                ClientPassword = password
            };
            return authenticator;
        }

        public static OAuth1Authenticator ForProtectedResource(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            var authenticator = new OAuth1Authenticator
            {
                Type = OAuthType.ProtectedResource,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                SignatureTreatment = OAuthSignatureTreatment.Escaped,
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                Token = accessToken,
                TokenSecret = accessTokenSecret
            };
            return authenticator;
        }

        public void Authenticate(RestClient client, RestRequest request)
        {
            var workflow = new OAuthWorkflow
            {
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerSecret,
                ParameterHandling = ParameterHandling,
                SignatureMethod = SignatureMethod,
                SignatureTreatment = SignatureTreatment,
                Verifier = Verifier,
                Version = Version,
                CallbackUrl = CallbackUrl,
                SessionHandle = SessionHandle,
                Token = Token,
                TokenSecret = TokenSecret,
                ClientUsername = ClientUsername,
                ClientPassword = ClientPassword
            };

            AddOAuthData(client, request, workflow);
        }

        private void AddOAuthData(RestClient client, RestRequest request, OAuthWorkflow workflow)
        {
            var url = client.BuildUri(request).ToString();

            OAuthWebQueryInfo oauth;
            var method = request.Method.ToString().ToUpperInvariant();

            var parameters = new WebParameterCollection();

            switch (Type)
            {
                case OAuthType.RequestToken:
                    workflow.RequestTokenUrl = url;
                    oauth = workflow.BuildRequestTokenInfo(method, parameters);
                    break;
                case OAuthType.AccessToken:
                    workflow.AccessTokenUrl = url;
                    oauth = workflow.BuildAccessTokenInfo(method, parameters);
                    break;
                case OAuthType.ClientAuthentication:
                    workflow.AccessTokenUrl = url;
                    oauth = workflow.BuildClientAuthAccessTokenInfo(method, parameters);
                    break;
                case OAuthType.ProtectedResource:
                    oauth = workflow.BuildProtectedResourceInfo(method, parameters, url);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            foreach(var parameter in parameters)
            {
                request.AddParameter(parameter.Name, parameter.Value);
            }
            request.AddParameter("oauth_signature", HttpUtility.UrlDecode(oauth.Signature));
        }
    }
}
