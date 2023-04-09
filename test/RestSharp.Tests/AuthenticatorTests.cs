using System.Net;
using RestSharp.Authenticators;
using RichardSzalay.MockHttp;

namespace RestSharp.Tests;

public class AuthenticatorTests {
    [Fact]
    public async Task Should_add_authorization_header() {
        const string auth = "LetMeIn";
        const string url  = "https://dummy.org";

        var mockHttp = new MockHttpMessageHandler();

        mockHttp
            .When(HttpMethod.Get, url)
            .WithHeaders(KnownHeaders.Authorization, auth)
            .Respond(HttpStatusCode.OK);

        var options = new RestClientOptions(url) {
            ConfigureMessageHandler = _ => mockHttp,
            Authenticator           = new TestAuthenticator(ParameterType.HttpHeader, KnownHeaders.Authorization, auth)
        };
        var client   = new RestClient(options);
        var response = await client.ExecuteGetAsync(new RestRequest());
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_add_authorization_form_parameter() {
        const string auth = "LetMeIn";
        const string url  = "https://dummy.org";

        var mockHttp = new MockHttpMessageHandler();

        mockHttp
            .When(HttpMethod.Post, url)
            .WithFormData("token", auth)
            .Respond(HttpStatusCode.OK);

        var options = new RestClientOptions(url) {
            ConfigureMessageHandler = _ => mockHttp,
            Authenticator           = new TestAuthenticator(ParameterType.GetOrPost, "token", auth)
        };
        var client   = new RestClient(options);
        var response = await client.ExecutePostAsync(new RestRequest());
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    class TestAuthenticator : IAuthenticator {
        readonly ParameterType _type;
        readonly string        _name;
        readonly string        _value;

        public TestAuthenticator(ParameterType type, string name, string value) {
            _type  = type;
            _name  = name;
            _value = value;
        }

        public ValueTask Authenticate(IRestClient client, RestRequest request) {
            request.AddParameter(_name, _value, _type);
            return default;
        }
    }
}
