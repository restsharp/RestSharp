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
using System.Web;
using RestSharp.Authenticators.OAuth.Extensions;
using RestSharp.Extensions;
using RestSharp.Validation;

namespace RestSharp.Authenticators.OAuth
{
    /// <summary>
    ///     A class to encapsulate OAuth authentication flow.
    /// </summary>
    sealed class OAuthWorkflow
    {
        public string Version { get; set; }

        public string ConsumerKey { get; set; }

        public string ConsumerSecret { get; set; }

        public string Token { get; set; }

        public string TokenSecret { get; set; }

        public string CallbackUrl { get; set; }

        public string Verifier { get; set; }

        public string SessionHandle { get; set; }

        public OAuthSignatureMethod SignatureMethod { get; set; }

        public OAuthSignatureTreatment SignatureTreatment { get; set; }

        public OAuthParameterHandling ParameterHandling { get; set; }

        public string ClientUsername { get; set; }

        public string ClientPassword { get; set; }

        public string RequestTokenUrl { get; set; }

        public string AccessTokenUrl { get; set; }

        /// <summary>
        ///     Generates an OAuth signature to pass to an
        ///     <see cref="IAuthenticator" /> for the purpose of requesting an
        ///     unauthorized request token.
        /// </summary>
        /// <param name="method">The HTTP method for the intended request</param>
        /// <param name="parameters">Any existing, non-OAuth query parameters desired in the request</param>
        /// <returns></returns>
        public string BuildRequestTokenInfo(string method, WebPairCollection parameters)
        {
            ValidateTokenRequestState();

            parameters ??= new WebPairCollection();

            var timestamp = OAuthTools.GetTimestamp();
            var nonce     = OAuthTools.GetNonce();

            AddAuthParameters(parameters, timestamp, nonce);

            var signatureBase = OAuthTools.ConcatenateRequestElements(method, RequestTokenUrl, parameters);
            return OAuthTools.GetSignature(SignatureMethod, SignatureTreatment, signatureBase, ConsumerSecret);
        }

        /// <summary>
        ///     Generates an OAuth signature to pass to the
        ///     <see cref="IAuthenticator" /> for the purpose of exchanging a request token
        ///     for an access token authorized by the user at the Service Provider site.
        /// </summary>
        /// <param name="method">The HTTP method for the intended request</param>
        /// <param name="parameters">Any existing, non-OAuth query parameters desired in the request</param>
        public string BuildAccessTokenSignature(string method, WebPairCollection parameters)
        {
            ValidateAccessRequestState();

            parameters ??= new WebPairCollection();

            var uri       = new Uri(AccessTokenUrl);
            var timestamp = OAuthTools.GetTimestamp();
            var nonce     = OAuthTools.GetNonce();

            AddAuthParameters(parameters, timestamp, nonce);

            var signatureBase = OAuthTools.ConcatenateRequestElements(method, uri.ToString(), parameters);

            return OAuthTools.GetSignature(
                SignatureMethod, SignatureTreatment, signatureBase,
                ConsumerSecret, TokenSecret
            );
        }

        /// <summary>
        ///     Generates an OAuth signature to pass to an
        ///     <see cref="IAuthenticator" /> for the purpose of exchanging user credentials
        ///     for an access token authorized by the user at the Service Provider site.
        /// </summary>
        /// <param name="method">The HTTP method for the intended request</param>
        /// <param name="parameters">Any existing, non-OAuth query parameters desired in the request</param>
        public string BuildClientAuthAccessTokenSignature(string method, WebPairCollection parameters)
        {
            ValidateClientAuthAccessRequestState();

            if (parameters == null)
                parameters = new WebPairCollection();

            var uri       = new Uri(AccessTokenUrl);
            var timestamp = OAuthTools.GetTimestamp();
            var nonce     = OAuthTools.GetNonce();

            AddXAuthParameters(parameters, timestamp, nonce);

            var signatureBase = OAuthTools.ConcatenateRequestElements(method, uri.ToString(), parameters);

            return OAuthTools.GetSignature(
                SignatureMethod, SignatureTreatment, signatureBase,
                ConsumerSecret
            );
        }

        public string BuildProtectedResourceSignature(string method, WebPairCollection parameters, string url)
        {
            ValidateProtectedResourceState();

            parameters ??= new WebPairCollection();

            // Include url parameters in query pool
            var uri           = new Uri(url);
            var urlParameters = HttpUtility.ParseQueryString(uri.Query);

            foreach (var parameter in urlParameters.AllKeys)
                parameters.Add(parameter, urlParameters[parameter]);

            var timestamp = OAuthTools.GetTimestamp();
            var nonce     = OAuthTools.GetNonce();

            AddAuthParameters(parameters, timestamp, nonce);

            var signatureBase = OAuthTools.ConcatenateRequestElements(method, url, parameters);

            return OAuthTools.GetSignature(
                SignatureMethod, SignatureTreatment, signatureBase,
                ConsumerSecret, TokenSecret
            );
        }

        void ValidateTokenRequestState()
        {
            Ensure.NotEmpty(RequestTokenUrl, nameof(RequestTokenUrl));
            Ensure.NotEmpty(ConsumerKey, nameof(ConsumerKey));
            Ensure.NotEmpty(ConsumerSecret, nameof(ConsumerSecret));
        }

        void ValidateAccessRequestState()
        {
            Ensure.NotEmpty(AccessTokenUrl, nameof(AccessTokenUrl));
            Ensure.NotEmpty(ConsumerKey, nameof(ConsumerKey));
            Ensure.NotEmpty(ConsumerSecret, nameof(ConsumerSecret));
            Ensure.NotEmpty(Token, nameof(Token));
        }

        void ValidateClientAuthAccessRequestState()
        {
            Ensure.NotEmpty(AccessTokenUrl, nameof(AccessTokenUrl));
            Ensure.NotEmpty(ConsumerKey, nameof(ConsumerKey));
            Ensure.NotEmpty(ConsumerSecret, nameof(ConsumerSecret));
            Ensure.NotEmpty(ClientUsername, nameof(ClientUsername));
        }

        void ValidateProtectedResourceState()
        {
            Ensure.NotEmpty(ConsumerKey, nameof(ConsumerKey));
            Ensure.NotEmpty(ConsumerSecret, nameof(ConsumerSecret));
        }

        void AddAuthParameters(ICollection<WebPair> parameters, string timestamp, string nonce)
        {
            var authParameters = new WebPairCollection
            {
                new WebPair("oauth_consumer_key", ConsumerKey),
                new WebPair("oauth_nonce", nonce),
                new WebPair("oauth_signature_method", SignatureMethod.ToRequestValue()),
                new WebPair("oauth_timestamp", timestamp),
                new WebPair("oauth_version", Version ?? "1.0")
            };

            if (!Token.IsEmpty())
                authParameters.Add(new WebPair("oauth_token", Token));

            if (!CallbackUrl.IsEmpty())
                authParameters.Add(new WebPair("oauth_callback", CallbackUrl));

            if (!Verifier.IsEmpty())
                authParameters.Add(new WebPair("oauth_verifier", Verifier));

            if (!SessionHandle.IsEmpty())
                authParameters.Add(new WebPair("oauth_session_handle", SessionHandle));

            foreach (var authParameter in authParameters)
                parameters.Add(authParameter);
        }

        void AddXAuthParameters(ICollection<WebPair> parameters, string timestamp, string nonce)
        {
            var authParameters = new WebPairCollection
            {
                new WebPair("x_auth_username", ClientUsername),
                new WebPair("x_auth_password", ClientPassword),
                new WebPair("x_auth_mode", "client_auth"),
                new WebPair("oauth_consumer_key", ConsumerKey),
                new WebPair("oauth_signature_method", SignatureMethod.ToRequestValue()),
                new WebPair("oauth_timestamp", timestamp),
                new WebPair("oauth_nonce", nonce),
                new WebPair("oauth_version", Version ?? "1.0")
            };

            foreach (var authParameter in authParameters)
                parameters.Add(authParameter);
        }
    }
}