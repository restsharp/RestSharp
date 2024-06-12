using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;

namespace RestSharp.Tests.Auth;

public class OAuth1SignatureTests {
    readonly OAuthWorkflow _workflow = new() {
        ParameterHandling = OAuthParameterHandling.UrlOrPostParameters,
        Token             = "370773112-GmHxMAgYyLbNEtIKZeRNFsMKPR9EyMZeS9weJAEb",
        TokenSecret       = "LswwdoUaIvS8ltyTt5jkRh4J50vUPVVHtR2YPi5kE",
        ConsumerKey       = "xvz1evFS4wEEPTGEFPHBog",
        ConsumerSecret    = "kAcSOqF21Fu85e7zjz7ZN2U4ZRhfV3WpwPAoE3Z7kBw",
        SignatureMethod   = OAuthSignatureMethod.HmacSha1,
        Version           = "1.0",
        GetNonce          = () => "kYjzVBB8Y0ZFabxSWbWovY3uYSQ2pTgmZeNu2VS4cg",
        GetTimestamp      = () => "1318622958"
    };

    readonly RestClient _client = new("https://api.twitter.com/1.1");

    readonly RestRequest _request = new RestRequest("statuses/update.json", Method.Post)
        .AddParameter("status", "Hello Ladies + Gentlemen, a signed OAuth request!")
        .AddParameter("include_entities", "true");

    [Fact]
    public void Adds_correct_signature() {
        OAuth1Authenticator.AddOAuthData(_client, _request, _workflow, OAuthType.ProtectedResource, null);

        var signature = _request.Parameters.First(x => x.Name == "oauth_signature").Value;
        signature.Should().Be("hCtSmYh+iHYCEqBWrE7C7hYmtUk=");
    }

    [Fact]
    public void Generates_correct_signature_base() {
        const string method = "POST";

        var requestParameters = _request.Parameters.ToWebParameters().ToArray();
        var parameters        = new WebPairCollection();
        parameters.AddRange(requestParameters);
        var url = _client.BuildUri(_request).ToString();
        _workflow.RequestUrl = url;
        var oauthParameters = _workflow.BuildProtectedResourceSignature(method, parameters);
        oauthParameters.Parameters.AddRange(requestParameters);

        var signatureBase = OAuthTools.ConcatenateRequestElements(method, url, oauthParameters.Parameters);

        signatureBase.Should()
            .Be(
                "POST&https%3A%2F%2Fapi.twitter.com%2F1.1%2Fstatuses%2Fupdate.json&include_entities%3Dtrue%26oauth_consumer_key%3Dxvz1evFS4wEEPTGEFPHBog%26oauth_nonce%3DkYjzVBB8Y0ZFabxSWbWovY3uYSQ2pTgmZeNu2VS4cg%26oauth_signature_method%3DHMAC-SHA1%26oauth_timestamp%3D1318622958%26oauth_token%3D370773112-GmHxMAgYyLbNEtIKZeRNFsMKPR9EyMZeS9weJAEb%26oauth_version%3D1.0%26status%3DHello%2520Ladies%2520%252B%2520Gentlemen%252C%2520a%2520signed%2520OAuth%2520request%2521"
            );
    }
}