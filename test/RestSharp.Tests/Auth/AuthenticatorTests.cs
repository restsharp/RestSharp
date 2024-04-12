using System.Net;
using RestSharp.Authenticators;
using RestSharp.Tests.Fixtures;
using RichardSzalay.MockHttp;

namespace RestSharp.Tests.Auth;

public class AuthenticatorTests {
    [Fact]
    public async Task Should_add_authorization_header() {
        const string auth = "LetMeIn";

        using var client = MockHttpClient.Create(
            Method.Get,
            request => request.WithHeaders(KnownHeaders.Authorization, auth).Respond(HttpStatusCode.OK),
            opt => opt.Authenticator = new TestAuthenticator(ParameterType.HttpHeader, KnownHeaders.Authorization, auth)
        );
        var response = await client.ExecuteGetAsync(new RestRequest());
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_add_authorization_form_parameter() {
        const string auth = "LetMeIn";
        const string formField = "token";

        using var client = MockHttpClient.Create(
            Method.Post,
            request => request.WithFormData(formField, auth).Respond(HttpStatusCode.OK),
            opt => opt.Authenticator = new TestAuthenticator(ParameterType.GetOrPost, formField, auth)
        );
        var response = await client.ExecutePostAsync(new RestRequest());
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    class TestAuthenticator(ParameterType type, string name, string value) : IAuthenticator {
        public ValueTask Authenticate(IRestClient client, RestRequest request) {
            request.AddParameter(name, value, type);
            return default;
        }
    }
}