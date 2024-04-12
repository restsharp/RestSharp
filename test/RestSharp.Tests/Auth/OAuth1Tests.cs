using System.Xml.Serialization;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;
using RestSharp.Tests.Shared.Extensions;

#pragma warning disable CS8618

namespace RestSharp.Tests.Auth;

public class OAuth1Tests {
    [XmlRoot("queue")]
    class Queue {
        [XmlElement("etag")]
        public string Etag { get; set; }

        public List<QueueItem> Items { get; set; }
    }

    [XmlRoot("queue_item")]
    class QueueItem {
        [XmlElement("id")]
        public string Id { get; set; }

        [XmlElement("position")]
        public int Position { get; set; }
    }

    [Fact]
    public async Task Can_Authenticate_OAuth1_With_Querystring_Parameters() {
        const string consumerKey    = "enterConsumerKeyHere";
        const string consumerSecret = "enterConsumerSecretHere";
        const string baseUrl        = "http://restsharp.org";

        var expected = new List<string> {
            "oauth_consumer_key",
            "oauth_nonce",
            "oauth_signature_method",
            "oauth_timestamp",
            "oauth_version",
            "oauth_signature"
        };

        using var client        = new RestClient(baseUrl);
        var       request       = new RestRequest();
        var       authenticator = OAuth1Auth.ForRequestToken(consumerKey, consumerSecret);
        authenticator.ParameterHandling = OAuthParameterHandling.UrlOrPostParameters;
        await authenticator.Authenticate(client, request);

        var requestUri = client.BuildUri(request);
        var actual     = requestUri.ParseQuery().Select(x => x.Key).ToList();

        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [MemberData(nameof(EncodeParametersTestData))]
    public void Properly_Encodes_Parameter_Names(IList<(string, string)> parameters, string expected) {
        var postData = new WebPairCollection();
        postData.AddRange(parameters.Select(x => new WebPair(x.Item1, x.Item2)));
        var sortedParams = OAuthTools.SortParametersExcludingSignature(postData);

        sortedParams.First().Should().Be(expected);
    }

    public static IEnumerable<object[]> EncodeParametersTestData => new List<object[]> {
        new object[] {
            new List<(string, string)> { ("name[first]", "Chuck"), ("name[last]", "Testa") },
            "name%5Bfirst%5D=Chuck"
        },
        new object[] {
            new List<(string, string)> { ("country", "España") },
            "country=Espa%C3%B1a"
        }
    };

    [Fact]
    public void Use_RFC_3986_Encoding_For_Auth_Signature_Base() {
        // reserved characters for 2396 and 3986
        // http://www.ietf.org/rfc/rfc2396.txt
        string[] reserved2396Characters = { ";", "/", "?", ":", "@", "&", "=", "+", "$", "," };
        // http://www.ietf.org/rfc/rfc3986.txt
        string[] additionalReserved3986Characters = { "!", "*", "'", "(", ")" };

        var reservedCharacterString = string.Join(
            string.Empty,
            reserved2396Characters.Union(additionalReserved3986Characters)
        );

        // act
        var escapedString = OAuthTools.UrlEncodeRelaxed(reservedCharacterString);

        // assert
        Assert.Equal("%3B%2F%3F%3A%40%26%3D%2B%24%2C%2521%252A%2527%2528%2529", escapedString);
    }
}