//   Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using RestSharp.Authenticators.OAuth;
using RestSharp.Authenticators.OAuth.Extensions;
using RestSharp.Extensions;

// ReSharper disable CheckNamespace

namespace RestSharp.Authenticators
{
    /// <seealso href="http://tools.ietf.org/html/rfc5849">RFC: The OAuth 1.0 Protocol</seealso>
    /// <see cref="StringBuilder"/>
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
                ConsumerKey        = ConsumerKey,
                ConsumerSecret     = ConsumerSecret,
                ParameterHandling  = ParameterHandling,
                SignatureMethod    = SignatureMethod,
                SignatureTreatment = SignatureTreatment,
                Verifier           = Verifier,
                Version            = Version,
                CallbackUrl        = CallbackUrl,
                SessionHandle      = SessionHandle,
                Token              = Token,
                TokenSecret        = TokenSecret,
                ClientUsername     = ClientUsername,
                ClientPassword     = ClientPassword
            };

            AddOAuthData(client, request, workflow);
        }

        public static OAuth1Authenticator ForRequestToken(
            string consumerKey,
            string consumerSecret,
            OAuthSignatureMethod signatureMethod = OAuthSignatureMethod.HmacSha1
        )
        {
            var authenticator = new OAuth1Authenticator
            {
                ParameterHandling  = OAuthParameterHandling.HttpAuthorizationHeader,
                SignatureMethod    = signatureMethod,
                SignatureTreatment = OAuthSignatureTreatment.Escaped,
                ConsumerKey        = consumerKey,
                ConsumerSecret     = consumerSecret,
                Type               = OAuthType.RequestToken
            };

            return authenticator;
        }

        public static OAuth1Authenticator ForRequestToken(string consumerKey, string consumerSecret, string callbackUrl)
        {
            var authenticator = ForRequestToken(consumerKey, consumerSecret);

            authenticator.CallbackUrl = callbackUrl;

            return authenticator;
        }

        public static OAuth1Authenticator ForAccessToken(
            string consumerKey,
            string consumerSecret,
            string token,
            string tokenSecret,
            OAuthSignatureMethod signatureMethod = OAuthSignatureMethod.HmacSha1
        )
            => new OAuth1Authenticator
            {
                ParameterHandling  = OAuthParameterHandling.HttpAuthorizationHeader,
                SignatureMethod    = signatureMethod,
                SignatureTreatment = OAuthSignatureTreatment.Escaped,
                ConsumerKey        = consumerKey,
                ConsumerSecret     = consumerSecret,
                Token              = token,
                TokenSecret        = tokenSecret,
                Type               = OAuthType.AccessToken
            };

        public static OAuth1Authenticator ForAccessToken(
            string consumerKey,
            string consumerSecret,
            string token,
            string tokenSecret,
            string verifier
        )
        {
            var authenticator = ForAccessToken(consumerKey, consumerSecret, token, tokenSecret);

            authenticator.Verifier = verifier;

            return authenticator;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        /// <param name="token"></param>
        /// <param name="tokenSecret"></param>
        /// <param name="sessionHandle"></param>
        /// <returns></returns>
        public static OAuth1Authenticator ForAccessTokenRefresh(
            string consumerKey,
            string consumerSecret,
            string token,
            string tokenSecret,
            string sessionHandle
        )
        {
            var authenticator = ForAccessToken(consumerKey, consumerSecret, token, tokenSecret);

            authenticator.SessionHandle = sessionHandle;

            return authenticator;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        /// <param name="token"></param>
        /// <param name="tokenSecret"></param>
        /// <param name="verifier"></param>
        /// <param name="sessionHandle"></param>
        /// <returns></returns>
        public static OAuth1Authenticator ForAccessTokenRefresh(
            string consumerKey,
            string consumerSecret,
            string token,
            string tokenSecret,
            string verifier,
            string sessionHandle
        )
        {
            var authenticator = ForAccessToken(consumerKey, consumerSecret, token, tokenSecret);

            authenticator.SessionHandle = sessionHandle;
            authenticator.Verifier      = verifier;

            return authenticator;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="signatureMethod"></param>
        /// <returns></returns>
        public static OAuth1Authenticator ForClientAuthentication(
            string consumerKey,
            string consumerSecret,
            string username,
            string password,
            OAuthSignatureMethod signatureMethod = OAuthSignatureMethod.HmacSha1
        )
            => new OAuth1Authenticator
            {
                ParameterHandling  = OAuthParameterHandling.HttpAuthorizationHeader,
                SignatureMethod    = signatureMethod,
                SignatureTreatment = OAuthSignatureTreatment.Escaped,
                ConsumerKey        = consumerKey,
                ConsumerSecret     = consumerSecret,
                ClientUsername     = username,
                ClientPassword     = password,
                Type               = OAuthType.ClientAuthentication
            };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        /// <param name="accessToken"></param>
        /// <param name="accessTokenSecret"></param>
        /// <param name="signatureMethod"></param>
        /// <returns></returns>
        public static OAuth1Authenticator ForProtectedResource(
            string consumerKey,
            string consumerSecret,
            string accessToken,
            string accessTokenSecret,
            OAuthSignatureMethod signatureMethod = OAuthSignatureMethod.HmacSha1
        )
            => new OAuth1Authenticator
            {
                Type               = OAuthType.ProtectedResource,
                ParameterHandling  = OAuthParameterHandling.HttpAuthorizationHeader,
                SignatureMethod    = signatureMethod,
                SignatureTreatment = OAuthSignatureTreatment.Escaped,
                ConsumerKey        = consumerKey,
                ConsumerSecret     = consumerSecret,
                Token              = accessToken,
                TokenSecret        = accessTokenSecret
            };

        void AddOAuthData(IRestClient client, IRestRequest request, OAuthWorkflow workflow)
        {
            var requestUrl = client.BuildUriWithoutQueryParameters(request);

            if (requestUrl.Contains('?'))
                throw new ApplicationException(
                    "Using query parameters in the base URL is not supported for OAuth calls. Consider using AddDefaultQueryParameter instead."
                );

            var url              = client.BuildUri(request).ToString();
            var queryStringStart = url.IndexOf('?');

            if (queryStringStart != -1)
                url = url.Substring(0, queryStringStart);

            var method = request.Method.ToString().ToUpperInvariant();

            var parameters = new WebPairCollection();

            // include all GET and POST parameters before generating the signature
            // according to the RFC 5849 - The OAuth 1.0 Protocol
            // http://tools.ietf.org/html/rfc5849#section-3.4.1
            // if this change causes trouble we need to introduce a flag indicating the specific OAuth implementation level,
            // or implement a separate class for each OAuth version
            static bool BaseQuery(Parameter x)
                => x.Type == ParameterType.GetOrPost || x.Type == ParameterType.QueryString || x.Type == ParameterType.QueryStringWithoutEncode;

            var query =
                request.AlwaysMultipartFormData || request.Files.Count > 0
                    ? x => BaseQuery(x) && x.Name.StartsWith("oauth_")
                    : (Func<Parameter, bool>) BaseQuery;

            parameters.AddRange(client.DefaultParameters.Where(query).ToWebParameters());
            parameters.AddRange(request.Parameters.Where(query).ToWebParameters());

            if (Type == OAuthType.RequestToken)
                workflow.RequestTokenUrl = url;
            else
                workflow.AccessTokenUrl = url;

            var oauth = Type switch
            {
                OAuthType.RequestToken         => workflow.BuildRequestTokenInfo(method, parameters),
                OAuthType.AccessToken          => workflow.BuildAccessTokenSignature(method, parameters),
                OAuthType.ClientAuthentication => workflow.BuildClientAuthAccessTokenSignature(method, parameters),
                OAuthType.ProtectedResource    => workflow.BuildProtectedResourceSignature(method, parameters, url),
                _                              => throw new ArgumentOutOfRangeException()
            };

            parameters.Add("oauth_signature", oauth);

            var oauthParameters = ParameterHandling switch
            {
                OAuthParameterHandling.HttpAuthorizationHeader => CreateHeaderParameters(),
                OAuthParameterHandling.UrlOrPostParameters     => CreateUrlParameters(),
                _ =>
                throw new ArgumentOutOfRangeException()
            };

            request.AddOrUpdateParameters(oauthParameters);

            IEnumerable<Parameter> CreateHeaderParameters()
                => new[] {new Parameter("Authorization", GetAuthorizationHeader(), ParameterType.HttpHeader)};

            IEnumerable<Parameter> CreateUrlParameters()
                => parameters.Where(p => !p.Name.IsEmpty() && (p.Name.StartsWith("oauth_") || p.Name.StartsWith("x_auth_")))
                    .Select(p => new Parameter(p.Name, HttpUtility.UrlDecode(p.Value), ParameterType.GetOrPost));

            string GetAuthorizationHeader()
            {
                var oathParameters =
                    parameters
                        .OrderBy(x => x, WebPair.Comparer)
                        .Where(
                            p =>
                                !p.Name.IsEmpty() && !p.Value.IsEmpty() &&
                                (p.Name.StartsWith("oauth_") || p.Name.StartsWith("x_auth_"))
                        )
                        .Select(x => $"{x.Name}=\"{x.Value}\"")
                        .ToList();

                if (!Realm.IsEmpty())
                    oathParameters.Insert(0, $"realm=\"{OAuthTools.UrlEncodeRelaxed(Realm)}\"");

                return "OAuth " + string.Join(",", oathParameters);
            }
        }
    }

    internal static class ParametersExtensions
    {
        internal static IEnumerable<WebPair> ToWebParameters(this IEnumerable<Parameter> p) => p.Select(x => new WebPair(x.Name, x.Value.ToString()));
    }
}