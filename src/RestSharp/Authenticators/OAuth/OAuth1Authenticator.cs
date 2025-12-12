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

using RestSharp.Authenticators.OAuth;
using RestSharp.Extensions;
using System.Web;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

// ReSharper disable NotResolvedInText
// ReSharper disable CheckNamespace

namespace RestSharp.Authenticators;

/// <seealso href="http://tools.ietf.org/html/rfc5849">RFC: The OAuth 1.0 Protocol</seealso>
// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class OAuth1Authenticator : IAuthenticator {
    public virtual string?                 Realm              { get; set; }
    public virtual OAuthParameterHandling  ParameterHandling  { get; set; }
    public virtual OAuthSignatureMethod    SignatureMethod    { get; set; }
    public virtual OAuthSignatureTreatment SignatureTreatment { get; set; }
    public virtual OAuthType               Type               { get; set; }
    public virtual string?                 ConsumerKey        { get; set; }
    public virtual string?                 ConsumerSecret     { get; set; }
    public virtual string?                 Token              { get; set; }
    public virtual string?                 TokenSecret        { get; set; }
    public virtual string?                 Verifier           { get; set; }
    public virtual string?                 Version            { get; set; }
    public virtual string?                 CallbackUrl        { get; set; }
    public virtual string?                 SessionHandle      { get; set; }
    public virtual string?                 ClientUsername     { get; set; }
    public virtual string?                 ClientPassword     { get; set; }

    public ValueTask Authenticate(IRestClient client, RestRequest request) {
        var workflow = new OAuthWorkflow {
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

        AddOAuthData(client, request, workflow, Type, Realm);
        return default;
    }

    /// <summary>
    /// Creates an authenticator to retrieve a request token.
    /// </summary>
    /// <param name="consumerKey">Consumer or API key</param>
    /// <param name="consumerSecret">Consumer or API secret</param>
    /// <param name="signatureMethod">Signature method, default is HMAC SHA1</param>
    /// <returns>Authenticator instance</returns>
    [PublicAPI]
    public static OAuth1Authenticator ForRequestToken(
        string               consumerKey,
        string?              consumerSecret,
        OAuthSignatureMethod signatureMethod = OAuthSignatureMethod.HmacSha1
    )
        => new() {
            ParameterHandling  = OAuthParameterHandling.HttpAuthorizationHeader,
            SignatureMethod    = signatureMethod,
            SignatureTreatment = OAuthSignatureTreatment.Escaped,
            ConsumerKey        = consumerKey,
            ConsumerSecret     = consumerSecret,
            Type               = OAuthType.RequestToken
        };

    /// <summary>
    /// Creates an authenticator to retrieve a request token with custom callback.
    /// </summary>
    /// <param name="consumerKey">Consumer or API key</param>
    /// <param name="consumerSecret">Consumer or API secret</param>
    /// <param name="callbackUrl">URL to where the user will be redirected to after authhentication</param>
    /// <returns>Authenticator instance</returns>
    [PublicAPI]
    public static OAuth1Authenticator ForRequestToken(string consumerKey, string? consumerSecret, string callbackUrl) {
        var authenticator = ForRequestToken(consumerKey, consumerSecret);

        authenticator.CallbackUrl = callbackUrl;

        return authenticator;
    }

    /// <summary>
    /// Creates an authenticator to retrieve an access token using the request token.
    /// </summary>
    /// <param name="consumerKey">Consumer or API key</param>
    /// <param name="consumerSecret">Consumer or API secret</param>
    /// <param name="token">Request token</param>
    /// <param name="tokenSecret">Request token secret</param>
    /// <param name="signatureMethod">Signature method, default is HMAC SHA1</param>
    /// <returns>Authenticator instance</returns>
    [PublicAPI]
    public static OAuth1Authenticator ForAccessToken(
        string               consumerKey,
        string?              consumerSecret,
        string               token,
        string               tokenSecret,
        OAuthSignatureMethod signatureMethod = OAuthSignatureMethod.HmacSha1
    )
        => new() {
            ParameterHandling  = OAuthParameterHandling.HttpAuthorizationHeader,
            SignatureMethod    = signatureMethod,
            SignatureTreatment = OAuthSignatureTreatment.Escaped,
            ConsumerKey        = consumerKey,
            ConsumerSecret     = consumerSecret,
            Token              = token,
            TokenSecret        = tokenSecret,
            Type               = OAuthType.AccessToken
        };

    /// <summary>
    /// Creates an authenticator to retrieve an access token using the request token and a verifier.
    /// </summary>
    /// <param name="consumerKey">Consumer or API key</param>
    /// <param name="consumerSecret">Consumer or API secret</param>
    /// <param name="token">Request token</param>
    /// <param name="tokenSecret">Request token secret</param>
    /// <param name="verifier">Verifier received from the API server</param>
    /// <returns>Authenticator instance</returns>
    [PublicAPI]
    public static OAuth1Authenticator ForAccessToken(
        string  consumerKey,
        string? consumerSecret,
        string  token,
        string  tokenSecret,
        string  verifier
    ) {
        var authenticator = ForAccessToken(consumerKey, consumerSecret, token, tokenSecret);

        authenticator.Verifier = verifier;

        return authenticator;
    }

    [PublicAPI]
    public static OAuth1Authenticator ForAccessTokenRefresh(
        string  consumerKey,
        string? consumerSecret,
        string  token,
        string  tokenSecret,
        string  sessionHandle
    ) {
        var authenticator = ForAccessToken(consumerKey, consumerSecret, token, tokenSecret);

        authenticator.SessionHandle = sessionHandle;

        return authenticator;
    }

    [PublicAPI]
    public static OAuth1Authenticator ForAccessTokenRefresh(
        string  consumerKey,
        string? consumerSecret,
        string  token,
        string  tokenSecret,
        string  verifier,
        string  sessionHandle
    ) {
        var authenticator = ForAccessToken(consumerKey, consumerSecret, token, tokenSecret);

        authenticator.SessionHandle = sessionHandle;
        authenticator.Verifier      = verifier;

        return authenticator;
    }

    [PublicAPI]
    public static OAuth1Authenticator ForClientAuthentication(
        string               consumerKey,
        string?              consumerSecret,
        string               username,
        string               password,
        OAuthSignatureMethod signatureMethod = OAuthSignatureMethod.HmacSha1
    )
        => new() {
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
    /// Creates an authenticator to make calls to protected resources using the access token.
    /// </summary>
    /// <param name="consumerKey">Consumer or API key</param>
    /// <param name="consumerSecret">Consumer or API secret</param>
    /// <param name="accessToken">Access token</param>
    /// <param name="accessTokenSecret">Access token secret</param>
    /// <param name="signatureMethod">Signature method, default is HMAC SHA1</param>
    /// <returns>Authenticator instance</returns>
    [PublicAPI]
    public static OAuth1Authenticator ForProtectedResource(
        string               consumerKey,
        string?              consumerSecret,
        string               accessToken,
        string               accessTokenSecret,
        OAuthSignatureMethod signatureMethod = OAuthSignatureMethod.HmacSha1
    )
        => new() {
            Type               = OAuthType.ProtectedResource,
            ParameterHandling  = OAuthParameterHandling.HttpAuthorizationHeader,
            SignatureMethod    = signatureMethod,
            SignatureTreatment = OAuthSignatureTreatment.Escaped,
            ConsumerKey        = consumerKey,
            ConsumerSecret     = consumerSecret,
            Token              = accessToken,
            TokenSecret        = accessTokenSecret
        };

    internal static void AddOAuthData(
        IRestClient            client,
        RestRequest            request,
        OAuthWorkflow          workflow,
        OAuthType              type,
        string?                realm
    ) {
        var requestUrl = client.BuildUriWithoutQueryParameters(request).AbsoluteUri;

        if (requestUrl.Contains('?'))
            throw new ApplicationException(
                "Using query parameters in the base URL is not supported for OAuth calls. Consider using AddDefaultQueryParameter instead."
            );

        var url              = client.BuildUri(request).ToString();
        var queryStringStart = url.IndexOf('?');

        if (queryStringStart != -1) url = url[..queryStringStart];

        var method     = request.Method.ToString().ToUpperInvariant();
        var parameters = new WebPairCollection();

        var query =
            request.AlwaysMultipartFormData || request.Files.Count > 0
                ? x => BaseQuery(x) && x.Name != null && x.Name.StartsWith("oauth_")
                : (Func<Parameter, bool>)BaseQuery;

        parameters.AddRange(client.DefaultParameters.Where(query).ToWebParameters());
        parameters.AddRange(request.Parameters.Where(query).ToWebParameters());

        workflow.RequestUrl = url;

        var oauth = type switch {
            OAuthType.RequestToken         => workflow.BuildRequestTokenSignature(method, parameters),
            OAuthType.AccessToken          => workflow.BuildAccessTokenSignature(method, parameters),
            OAuthType.ClientAuthentication => workflow.BuildClientAuthAccessTokenSignature(method, parameters),
            OAuthType.ProtectedResource    => workflow.BuildProtectedResourceSignature(method, parameters),
            _                              => throw new ArgumentOutOfRangeException(nameof(Type))
        };

        oauth.Parameters.Add("oauth_signature", oauth.Signature);

        var oauthParameters = workflow.ParameterHandling switch {
            OAuthParameterHandling.HttpAuthorizationHeader => CreateHeaderParameters(),
            OAuthParameterHandling.UrlOrPostParameters     => CreateUrlParameters(),
            _                                              => throw new ArgumentOutOfRangeException(nameof(ParameterHandling))
        };

        request.AddOrUpdateParameters(oauthParameters);
        return;

        // include all GET and POST parameters before generating the signature
        // according to the RFC 5849 - The OAuth 1.0 Protocol
        // http://tools.ietf.org/html/rfc5849#section-3.4.1
        // if this change causes trouble we need to introduce a flag indicating the specific OAuth implementation level,
        // or implement a separate class for each OAuth version
        static bool BaseQuery(Parameter x) => x.Type is ParameterType.GetOrPost or ParameterType.QueryString;

        IEnumerable<Parameter> CreateHeaderParameters() => [new HeaderParameter(KnownHeaders.Authorization, GetAuthorizationHeader())];

        IEnumerable<Parameter> CreateUrlParameters() => oauth.Parameters.Select(p => new GetOrPostParameter(p.Name, HttpUtility.UrlDecode(p.Value)));

        string GetAuthorizationHeader() {
            var oathParameters =
                oauth.Parameters
                    .OrderBy(x => x, WebPair.Comparer)
                    .Select(x => x.GetQueryParameter(true))
                    .ToList();

            if (!realm.IsEmpty()) oathParameters.Insert(0, $"realm=\"{OAuthTools.UrlEncodeRelaxed(realm)}\"");

            return $"OAuth {string.Join(",", oathParameters)}";
        }
    }
}

static class ParametersExtensions {
    internal static IEnumerable<WebPair> ToWebParameters(this IEnumerable<Parameter> p)
        => p.Select(x => new WebPair(Ensure.NotNull(x.Name, "Parameter name"), x.Value?.ToString()));
}