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
using System.Web;
using RestSharp.Authenticators.OAuth;
using RestSharp.Authenticators.OAuth.Extensions;

namespace RestSharp.Authenticators
{
    /// <seealso href="http://tools.ietf.org/html/rfc5849" />
    public class OAuth1Authenticator : IAuthenticator
    {
        public virtual string Realm { get; set; }

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

        public void Authenticate(IRestClient client, IRestRequest request)
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

        public static OAuth1Authenticator ForRequestToken(string consumerKey, string consumerSecret,
            OAuthSignatureMethod signatureMethod = OAuthSignatureMethod.HmacSha1)
        {
            var authenticator = new OAuth1Authenticator
            {
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                SignatureMethod = signatureMethod,
                SignatureTreatment = OAuthSignatureTreatment.Escaped,
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                Type = OAuthType.RequestToken
            };

            return authenticator;
        }

        public static OAuth1Authenticator ForRequestToken(string consumerKey, string consumerSecret, string callbackUrl)
        {
            var authenticator = ForRequestToken(consumerKey, consumerSecret);

            authenticator.CallbackUrl = callbackUrl;

            return authenticator;
        }

        public static OAuth1Authenticator ForAccessToken(string consumerKey, string consumerSecret, string token,
            string tokenSecret, OAuthSignatureMethod signatureMethod = OAuthSignatureMethod.HmacSha1)
        {
            var authenticator = new OAuth1Authenticator
            {
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                SignatureMethod = signatureMethod,
                SignatureTreatment = OAuthSignatureTreatment.Escaped,
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                Token = token,
                TokenSecret = tokenSecret,
                Type = OAuthType.AccessToken
            };

            return authenticator;
        }

        public static OAuth1Authenticator ForAccessToken(string consumerKey, string consumerSecret, string token,
            string tokenSecret, string verifier)
        {
            var authenticator = ForAccessToken(consumerKey, consumerSecret, token, tokenSecret);

            authenticator.Verifier = verifier;

            return authenticator;
        }

        public static OAuth1Authenticator ForAccessTokenRefresh(string consumerKey, string consumerSecret, string token,
            string tokenSecret, string sessionHandle)
        {
            var authenticator = ForAccessToken(consumerKey, consumerSecret, token, tokenSecret);

            authenticator.SessionHandle = sessionHandle;

            return authenticator;
        }

        public static OAuth1Authenticator ForAccessTokenRefresh(string consumerKey, string consumerSecret, string token,
            string tokenSecret, string verifier, string sessionHandle)
        {
            var authenticator = ForAccessToken(consumerKey, consumerSecret, token, tokenSecret);

            authenticator.SessionHandle = sessionHandle;
            authenticator.Verifier = verifier;

            return authenticator;
        }

        public static OAuth1Authenticator ForClientAuthentication(string consumerKey, string consumerSecret,
            string username, string password, OAuthSignatureMethod signatureMethod = OAuthSignatureMethod.HmacSha1)
        {
            var authenticator = new OAuth1Authenticator
            {
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                SignatureMethod = signatureMethod,
                SignatureTreatment = OAuthSignatureTreatment.Escaped,
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                ClientUsername = username,
                ClientPassword = password,
                Type = OAuthType.ClientAuthentication
            };

            return authenticator;
        }

        public static OAuth1Authenticator ForProtectedResource(string consumerKey, string consumerSecret,
            string accessToken, string accessTokenSecret,
            OAuthSignatureMethod signatureMethod = OAuthSignatureMethod.HmacSha1)
        {
            var authenticator = new OAuth1Authenticator
            {
                Type = OAuthType.ProtectedResource,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                SignatureMethod = signatureMethod,
                SignatureTreatment = OAuthSignatureTreatment.Escaped,
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                Token = accessToken,
                TokenSecret = accessTokenSecret
            };

            return authenticator;
        }

        private void AddOAuthData(IRestClient client, IRestRequest request, OAuthWorkflow workflow)
        {
            var url = client.BuildUri(request)
                .ToString();
            var queryStringStart = url.IndexOf('?');

            if (queryStringStart != -1)
                url = url.Substring(0, queryStringStart);

            OAuthWebQueryInfo oauth;
            var method = request.Method.ToString()
                .ToUpperInvariant();
            var parameters = new WebParameterCollection();

            // include all GET and POST parameters before generating the signature
            // according to the RFC 5849 - The OAuth 1.0 Protocol
            // http://tools.ietf.org/html/rfc5849#section-3.4.1
            // if this change causes trouble we need to introduce a flag indicating the specific OAuth implementation level,
            // or implement a seperate class for each OAuth version
            if (!request.AlwaysMultipartFormData && !request.Files.Any())
            {
                parameters.AddRange(
                    client.DefaultParameters
                        .Where(p => p.Type == ParameterType.GetOrPost || p.Type == ParameterType.QueryString)
                        .Select(p => new WebPair(p.Name, p.Value.ToString())));

                parameters.AddRange(
                    request.Parameters
                        .Where(p => p.Type == ParameterType.GetOrPost || p.Type == ParameterType.QueryString)
                        .Select(p => new WebPair(p.Name, p.Value.ToString())));
            }
            else
            {
                // if we are sending a multipart request, only the "oauth_" parameters should be included in the signature

                parameters.AddRange(
                    client.DefaultParameters
                        .Where(p => (p.Type == ParameterType.GetOrPost || p.Type == ParameterType.QueryString)
                                    && p.Name.StartsWith("oauth_"))
                        .Select(p => new WebPair(p.Name, p.Value.ToString())));

                parameters.AddRange(
                    request.Parameters
                        .Where(p => (p.Type == ParameterType.GetOrPost || p.Type == ParameterType.QueryString)
                                    && p.Name.StartsWith("oauth_"))
                        .Select(p => new WebPair(p.Name, p.Value.ToString())));
            }

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

            switch (ParameterHandling)
            {
                case OAuthParameterHandling.HttpAuthorizationHeader:
                    parameters.Add("oauth_signature", oauth.Signature);
                    request.AddHeader("Authorization", GetAuthorizationHeader(parameters));
                    break;

                case OAuthParameterHandling.UrlOrPostParameters:
                    parameters.Add("oauth_signature", oauth.Signature);
                    request.Parameters.AddRange(
                        parameters.Where(p => !p.Name.IsNullOrBlank() &&
                                              (p.Name.StartsWith("oauth_") || p.Name.StartsWith("x_auth_")))
                            .Select(p => new Parameter
                            {
                                Name = p.Name,
                                Value = HttpUtility.UrlDecode(p.Value),
                                Type = ParameterType.GetOrPost
                            }));
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string GetAuthorizationHeader(WebPairCollection parameters)
        {
            var sb = new StringBuilder("OAuth ");

            if (!Realm.IsNullOrBlank())
                sb.Append("realm=\"{0}\",".FormatWith(OAuthTools.UrlEncodeRelaxed(Realm)));

            parameters.Sort((l, r) => l.Name.CompareTo(r.Name));

            var parameterCount = 0;
            var oathParameters =
                parameters.Where(p => !p.Name.IsNullOrBlank() &&
                                      !p.Value.IsNullOrBlank() &&
                                      (p.Name.StartsWith("oauth_") || p.Name.StartsWith("x_auth_")))
                    .ToList();

            foreach (var parameter in oathParameters)
            {
                parameterCount++;

                var format = parameterCount < oathParameters.Count
                    ? "{0}=\"{1}\","
                    : "{0}=\"{1}\"";

                sb.Append(format.FormatWith(parameter.Name, parameter.Value));
            }

            var authorization = sb.ToString();

            return authorization;
        }
    }
}