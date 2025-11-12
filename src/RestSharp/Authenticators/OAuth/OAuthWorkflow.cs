//  Copyright (c) .NET Foundation and Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Web;
using RestSharp.Authenticators.OAuth.Extensions;

namespace RestSharp.Authenticators.OAuth;

/// <summary>
/// A class to encapsulate OAuth authentication flow.
/// </summary>
sealed class OAuthWorkflow {
    public string?                 Version            { get; init; }
    public string?                 ConsumerKey        { get; init; }
    public string?                 ConsumerSecret     { get; init; }
    public string?                 Token              { get; init; }
    public string?                 TokenSecret        { get; init; }
    public string?                 CallbackUrl        { get; init; }
    public string?                 Verifier           { get; init; }
    public string?                 SessionHandle      { get; init; }
    public OAuthSignatureMethod    SignatureMethod    { get; init; }
    public OAuthSignatureTreatment SignatureTreatment { get; init; }
    public OAuthParameterHandling  ParameterHandling  { get; set; }
    public string?                 ClientUsername     { get; init; }
    public string?                 ClientPassword     { get; init; }
    public string?                 RequestUrl         { get; set; }

    internal Func<string> GetTimestamp { get; init; } = OAuthTools.GetTimestamp;
    internal Func<string> GetNonce     { get; init; } = OAuthTools.GetNonce;

    /// <summary>
    /// Generates an OAuth signature to pass to an
    /// <see cref="IAuthenticator" /> for the purpose of requesting an
    /// unauthorized request token.
    /// </summary>
    /// <param name="method">The HTTP method for the intended request</param>
    /// <param name="parameters">Any existing, non-OAuth query parameters desired in the request</param>
    /// <returns></returns>
    public OAuthParameters BuildRequestTokenSignature(string method, WebPairCollection parameters) {
        Ensure.NotEmptyString(ConsumerKey, nameof(ConsumerKey));

        var allParameters = new WebPairCollection();
        allParameters.AddRange(parameters);

        var uri       = new Uri(Ensure.NotEmptyString(RequestUrl, nameof(RequestUrl)));
        var timestamp = GetTimestamp();
        var nonce     = GetNonce();

        var authParameters = GenerateAuthParameters(timestamp, nonce);
        allParameters.AddRange(authParameters);

        var signatureBase = OAuthTools.ConcatenateRequestElements(method, uri.ToString(), allParameters);

        return new() {
            Signature  = OAuthTools.GetSignature(SignatureMethod, SignatureTreatment, signatureBase, ConsumerSecret),
            Parameters = authParameters
        };
    }

    /// <summary>
    /// Generates an OAuth signature to pass to the
    /// <see cref="IAuthenticator" /> for the purpose of exchanging a request token
    /// for an access token authorized by the user at the Service Provider site.
    /// </summary>
    /// <param name="method">The HTTP method for the intended request</param>
    /// <param name="parameters">Any existing, non-OAuth query parameters desired in the request</param>
    public OAuthParameters BuildAccessTokenSignature(string method, WebPairCollection parameters) {
        Ensure.NotEmptyString(ConsumerKey, nameof(ConsumerKey));
        Ensure.NotEmptyString(Token, nameof(Token));

        var allParameters = new WebPairCollection();
        allParameters.AddRange(parameters);

        var uri       = new Uri(Ensure.NotEmptyString(RequestUrl, nameof(RequestUrl)));
        var timestamp = GetTimestamp();
        var nonce     = GetNonce();

        var authParameters = GenerateAuthParameters(timestamp, nonce);
        allParameters.AddRange(authParameters);

        var signatureBase = OAuthTools.ConcatenateRequestElements(method, uri.ToString(), allParameters);

        return new() {
            Signature  = OAuthTools.GetSignature(SignatureMethod, SignatureTreatment, signatureBase, ConsumerSecret, TokenSecret),
            Parameters = authParameters
        };
    }

    /// <summary>
    /// Generates an OAuth signature to pass to an
    /// <see cref="IAuthenticator" /> for the purpose of exchanging user credentials
    /// for an access token authorized by the user at the Service Provider site.
    /// </summary>
    /// <param name="method">The HTTP method for the intended request</param>
    /// <param name="parameters">Any existing, non-OAuth query parameters desired in the request</param>
    public OAuthParameters BuildClientAuthAccessTokenSignature(string method, WebPairCollection parameters) {
        Ensure.NotEmptyString(ConsumerKey, nameof(ConsumerKey));
        Ensure.NotEmptyString(ClientUsername, nameof(ClientUsername));

        var allParameters = new WebPairCollection();
        allParameters.AddRange(parameters);

        var uri       = new Uri(Ensure.NotEmptyString(RequestUrl, nameof(RequestUrl)));
        var timestamp = GetTimestamp();
        var nonce     = GetNonce();

        var authParameters = GenerateXAuthParameters(timestamp, nonce);
        allParameters.AddRange(authParameters);

        var signatureBase = OAuthTools.ConcatenateRequestElements(method, uri.ToString(), allParameters);

        return new() {
            Signature  = OAuthTools.GetSignature(SignatureMethod, SignatureTreatment, signatureBase, ConsumerSecret),
            Parameters = authParameters
        };
    }

    public OAuthParameters BuildProtectedResourceSignature(string method, WebPairCollection parameters) {
        Ensure.NotEmptyString(ConsumerKey, nameof(ConsumerKey));

        var allParameters = new WebPairCollection();
        allParameters.AddRange(parameters);

        // Include url parameters in query pool
        var uri           = new Uri(Ensure.NotEmptyString(RequestUrl, nameof(RequestUrl)));
        var urlParameters = HttpUtility.ParseQueryString(uri.Query);

        allParameters.AddRange(urlParameters.AllKeys.Select(x => new WebPair(x!, urlParameters[x]!)));

        var timestamp = GetTimestamp();
        var nonce     = GetNonce();

        var authParameters = GenerateAuthParameters(timestamp, nonce);
        allParameters.AddRange(authParameters);

        var signatureBase = OAuthTools.ConcatenateRequestElements(method, uri.ToString(), allParameters);

        return new() {
            Signature  = OAuthTools.GetSignature(SignatureMethod, SignatureTreatment, signatureBase, ConsumerSecret, TokenSecret),
            Parameters = authParameters
        };
    }

    WebPairCollection GenerateAuthParameters(string timestamp, string nonce)
        => new WebPairCollection {
                new("oauth_consumer_key", Ensure.NotNull(ConsumerKey, nameof(ConsumerKey)), true),
                new("oauth_nonce", nonce),
                new("oauth_signature_method", SignatureMethod.ToRequestValue()),
                new("oauth_timestamp", timestamp),
                new("oauth_version", Version ?? "1.0")
            }
            .AddNotEmpty("oauth_token", Token, true)
            .AddNotEmpty("oauth_callback", CallbackUrl, true)
            .AddNotEmpty("oauth_verifier", Verifier)
            .AddNotEmpty("oauth_session_handle", SessionHandle);

    WebPairCollection GenerateXAuthParameters(string timestamp, string nonce)
        => [
            new("x_auth_username", Ensure.NotNull(ClientUsername, nameof(ClientUsername))),
            new("x_auth_password", Ensure.NotNull(ClientPassword, nameof(ClientPassword))),
            new("x_auth_mode", "client_auth"),
            new("oauth_consumer_key", Ensure.NotNull(ConsumerKey, nameof(ConsumerKey)), true),
            new("oauth_signature_method", SignatureMethod.ToRequestValue()),
            new("oauth_timestamp", timestamp),
            new("oauth_nonce", nonce),
            new("oauth_version", Version ?? "1.0")
        ];

    internal class OAuthParameters {
        public WebPairCollection Parameters { get; init; } = null!;
        public string            Signature  { get; init; } = null!;
    }
}