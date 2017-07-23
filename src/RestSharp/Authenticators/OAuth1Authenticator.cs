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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp.Authenticators.OAuth;
using RestSharp.Authenticators.OAuth.Extensions;

#if !SILVERLIGHT && !WINDOWS_PHONE
using RestSharp.Extensions.MonoHttp;
#endif

#if WINDOWS_PHONE
using System.Net;
#endif

#if SILVERLIGHT
using System.Windows.Browser;
#else

#endif

namespace RestSharp.Authenticators
{
    /// <seealso href="http://tools.ietf.org/html/rfc5849"/>
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

        public static OAuth1Authenticator ForRequestToken(string consumerKey, string consumerSecret, OAuthSignatureMethod signatureMethod = OAuthSignatureMethod.HmacSha1)
        {
            OAuth1Authenticator authenticator = new OAuth1Authenticator
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
            OAuth1Authenticator authenticator = ForRequestToken(consumerKey, consumerSecret);

            authenticator.CallbackUrl = callbackUrl;

            return authenticator;
        }

        public static OAuth1Authenticator ForAccessToken(string consumerKey, string consumerSecret, string token,
            string tokenSecret, OAuthSignatureMethod signatureMethod = OAuthSignatureMethod.HmacSha1)
        {
            OAuth1Authenticator authenticator = new OAuth1Authenticator
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
            OAuth1Authenticator authenticator = ForAccessToken(consumerKey, consumerSecret, token, tokenSecret);

            authenticator.Verifier = verifier;

            return authenticator;
        }

        public static OAuth1Authenticator ForAccessTokenRefresh(string consumerKey, string consumerSecret, string token,
            string tokenSecret, string sessionHandle)
        {
            OAuth1Authenticator authenticator = ForAccessToken(consumerKey, consumerSecret, token, tokenSecret);

            authenticator.SessionHandle = sessionHandle;

            return authenticator;
        }

        public static OAuth1Authenticator ForAccessTokenRefresh(string consumerKey, string consumerSecret, string token,
            string tokenSecret, string verifier, string sessionHandle)
        {
            OAuth1Authenticator authenticator = ForAccessToken(consumerKey, consumerSecret, token, tokenSecret);

            authenticator.SessionHandle = sessionHandle;
            authenticator.Verifier = verifier;

            return authenticator;
        }

        public static OAuth1Authenticator ForClientAuthentication(string consumerKey, string consumerSecret,
            string username, string password, OAuthSignatureMethod signatureMethod = OAuthSignatureMethod.HmacSha1)
        {
            OAuth1Authenticator authenticator = new OAuth1Authenticator
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
            string accessToken, string accessTokenSecret, OAuthSignatureMethod signatureMethod = OAuthSignatureMethod.HmacSha1)
        {
            OAuth1Authenticator authenticator = new OAuth1Authenticator
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

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            OAuthWorkflow workflow = new OAuthWorkflow
                                     {
                                         ConsumerKey = this.ConsumerKey,
                                         ConsumerSecret = this.ConsumerSecret,
                                         ParameterHandling = this.ParameterHandling,
                                         SignatureMethod = this.SignatureMethod,
                                         SignatureTreatment = this.SignatureTreatment,
                                         Verifier = this.Verifier,
                                         Version = this.Version,
                                         CallbackUrl = this.CallbackUrl,
                                         SessionHandle = this.SessionHandle,
                                         Token = this.Token,
                                         TokenSecret = this.TokenSecret,
                                         ClientUsername = this.ClientUsername,
                                         ClientPassword = this.ClientPassword
                                     };

            this.AddOAuthData(client, request, workflow);
        }

        private void AddOAuthData(IRestClient client, IRestRequest request, OAuthWorkflow workflow)
        {
            string url = client.BuildUri(request)
                               .ToString();
            int queryStringStart = url.IndexOf('?');

            if (queryStringStart != -1)
            {
                url = url.Substring(0, queryStringStart);
            }

            OAuthWebQueryInfo oauth;
            string method = request.Method.ToString()
                                   .ToUpperInvariant();
            WebParameterCollection parameters = new WebParameterCollection();

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

            switch (this.Type)
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

            switch (this.ParameterHandling)
            {
                case OAuthParameterHandling.HttpAuthorizationHeader:
                    parameters.Add("oauth_signature", oauth.Signature);
                    request.AddHeader("Authorization", this.GetAuthorizationHeader(parameters));
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
            StringBuilder sb = new StringBuilder("OAuth ");

            if (!this.Realm.IsNullOrBlank())
            {
                sb.Append("realm=\"{0}\",".FormatWith(OAuthTools.UrlEncodeRelaxed(this.Realm)));
            }

            parameters.Sort((l, r) => l.Name.CompareTo(r.Name));

            int parameterCount = 0;
            List<WebPair> oathParameters =
                parameters.Where(p => !p.Name.IsNullOrBlank() &&
                                      !p.Value.IsNullOrBlank() &&
                                      (p.Name.StartsWith("oauth_") || p.Name.StartsWith("x_auth_")))
                          .ToList();

            foreach (WebPair parameter in oathParameters)
            {
                parameterCount++;

                string format = parameterCount < oathParameters.Count
                    ? "{0}=\"{1}\","
                    : "{0}=\"{1}\"";

                sb.Append(format.FormatWith(parameter.Name, parameter.Value));
            }

            string authorization = sb.ToString();

            return authorization;
        }
    }
}
