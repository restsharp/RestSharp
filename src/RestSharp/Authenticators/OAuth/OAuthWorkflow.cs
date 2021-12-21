//  Copyright © 2009-2021 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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
    public string?                 Version            { get; set; }
    public string?                 ConsumerKey        { get; set; }
    public string?                 ConsumerSecret     { get; set; }
    public string?                 Token              { get; set; }
    public string?                 TokenSecret        { get; set; }
    public string?                 CallbackUrl        { get; set; }
    public string?                 Verifier           { get; set; }
    public string?                 SessionHandle      { get; set; }
    public OAuthSignatureMethod    SignatureMethod    { get; set; }
    public OAuthSignatureTreatment SignatureTreatment { get; set; }
    public OAuthParameterHandling  ParameterHandling  { get; set; }
    public string?                 ClientUsername     { get; set; }
    public string?                 ClientPassword     { get; set; }
    public string?                 RequestTokenUrl    { get; set; }
    public string?                 AccessTokenUrl     { get; set; }

    /// <summary>
    /// Generates an OAuth signature to pass to an
    /// <see cref="IAuthenticator" /> for the purpose of requesting an
    /// unauthorized request token.
    /// </summary>
    /// <param name="method">The HTTP method for the intended request</param>
    /// <param name="parameters">Any existing, non-OAuth query parameters desired in the request</param>
    /// <returns></returns>
    public OAuthParameters BuildRequestTokenInfo(string method, WebPairCollection parameters) {
        ValidateTokenRequestState();

        var allParameters = new WebPairCollection();
        allParameters.AddRange(parameters);

        var timestamp = OAuthTools.GetTimestamp();
        var nonce     = OAuthTools.GetNonce();

        var authParameters = GenerateAuthParameters(timestamp, nonce);
        allParameters.AddRange(authParameters);

        var signatureBase = OAuthTools.ConcatenateRequestElements(method, Ensure.NotNull(RequestTokenUrl, nameof(RequestTokenUrl)), allParameters);

        return new OAuthParameters {
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
        ValidateAccessRequestState();

        var allParameters = new WebPairCollection();
        allParameters.AddRange(parameters);

        var uri       = new Uri(Ensure.NotEmptyString(AccessTokenUrl, nameof(AccessTokenUrl)));
        var timestamp = OAuthTools.GetTimestamp();
        var nonce     = OAuthTools.GetNonce();

        var authParameters = GenerateAuthParameters(timestamp, nonce);
        allParameters.AddRange(authParameters);

        var signatureBase = OAuthTools.ConcatenateRequestElements(method, uri.ToString(), allParameters);

        return new OAuthParameters {
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
        ValidateClientAuthAccessRequestState();

        var allParameters = new WebPairCollection();
        allParameters.AddRange(parameters);

        var uri       = new Uri(Ensure.NotNull(AccessTokenUrl, nameof(AccessTokenUrl)));
        var timestamp = OAuthTools.GetTimestamp();
        var nonce     = OAuthTools.GetNonce();

        var authParameters = GenerateXAuthParameters(timestamp, nonce);
        allParameters.AddRange(authParameters);

        var signatureBase = OAuthTools.ConcatenateRequestElements(method, uri.ToString(), allParameters);

        return new OAuthParameters {
            Signature  = OAuthTools.GetSignature(SignatureMethod, SignatureTreatment, signatureBase, ConsumerSecret),
            Parameters = authParameters
        };
    }

    public OAuthParameters BuildProtectedResourceSignature(string method, WebPairCollection parameters, string url) {
        ValidateProtectedResourceState();

        var allParameters = new WebPairCollection();
        allParameters.AddRange(parameters);

        // Include url parameters in query pool
        var uri           = new Uri(url);
        var urlParameters = HttpUtility.ParseQueryString(uri.Query);

        allParameters.AddRange(urlParameters.AllKeys.Select(x => new WebPair(x!, urlParameters[x]!)));

        var timestamp = OAuthTools.GetTimestamp();
        var nonce     = OAuthTools.GetNonce();

        var authParameters = GenerateAuthParameters(timestamp, nonce);
        allParameters.AddRange(authParameters);

        var signatureBase = OAuthTools.ConcatenateRequestElements(method, url, allParameters);

        return new OAuthParameters {
            Signature  = OAuthTools.GetSignature(SignatureMethod, SignatureTreatment, signatureBase, ConsumerSecret, TokenSecret),
            Parameters = authParameters
        };
    }

    void ValidateTokenRequestState() {
        Ensure.NotEmpty(RequestTokenUrl, nameof(RequestTokenUrl));
        Ensure.NotEmpty(ConsumerKey, nameof(ConsumerKey));
    }

    void ValidateAccessRequestState() {
        Ensure.NotEmpty(AccessTokenUrl, nameof(AccessTokenUrl));
        Ensure.NotEmpty(ConsumerKey, nameof(ConsumerKey));
        Ensure.NotEmpty(Token, nameof(Token));
    }

    void ValidateClientAuthAccessRequestState() {
        Ensure.NotEmpty(AccessTokenUrl, nameof(AccessTokenUrl));
        Ensure.NotEmpty(ConsumerKey, nameof(ConsumerKey));
        Ensure.NotEmpty(ClientUsername, nameof(ClientUsername));
    }

    void ValidateProtectedResourceState() {
        Ensure.NotEmpty(ConsumerKey, nameof(ConsumerKey));
    }

    WebPairCollection GenerateAuthParameters(string timestamp, string nonce)
        => new WebPairCollection {
                new("oauth_consumer_key", Ensure.NotNull(ConsumerKey, nameof(ConsumerKey)), true),
                new("oauth_nonce", nonce),
                new("oauth_signature_method", SignatureMethod.ToRequestValue()),
                new("oauth_timestamp", timestamp),
                new("oauth_version", Version ?? "1.0")
            }.AddNotEmpty("oauth_token", Token!, true)
            .AddNotEmpty("oauth_callback", CallbackUrl!, true)
            .AddNotEmpty("oauth_verifier", Verifier!)
            .AddNotEmpty("oauth_session_handle", SessionHandle!);

    WebPairCollection GenerateXAuthParameters(string timestamp, string nonce)
        => new WebPairCollection {
            new("x_auth_username", Ensure.NotNull(ClientUsername, nameof(ClientUsername))),
            new("x_auth_password", Ensure.NotNull(ClientPassword, nameof(ClientPassword))),
            new("x_auth_mode", "client_auth"),
            new("oauth_consumer_key", Ensure.NotNull(ConsumerKey, nameof(ConsumerKey)), true),
            new("oauth_signature_method", SignatureMethod.ToRequestValue()),
            new("oauth_timestamp", timestamp),
            new("oauth_nonce", nonce),
            new("oauth_version", Version ?? "1.0")
        };

    internal class OAuthParameters {
        public WebPairCollection Parameters { get; init; } = null!;
        public string            Signature  { get; init; } = null!;
    }
}