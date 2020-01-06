using System.Linq;
using NUnit.Framework;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;

namespace RestSharp.Tests
{
    [TestFixture]
    public class OAuth1AuthenticatorTests
    {
        [SetUp]
        public void Setup()
            => _authenticator = new OAuth1Authenticator
            {
                CallbackUrl        = "CallbackUrl",
                ClientPassword     = "ClientPassword",
                Type               = OAuthType.ClientAuthentication,
                ClientUsername     = "ClientUsername",
                ConsumerKey        = "ConsumerKey",
                ConsumerSecret     = "ConsumerSecret",
                Realm              = "Realm",
                SessionHandle      = "SessionHandle",
                SignatureMethod    = OAuthSignatureMethod.PlainText,
                SignatureTreatment = OAuthSignatureTreatment.Escaped,
                Token              = "Token",
                TokenSecret        = "TokenSecret",
                Verifier           = "Verifier",
                Version            = "Version"
            };

        OAuth1Authenticator _authenticator;

        [Test]
        public void Authenticate_ShouldAddAuthorizationAsTextValueToRequest_OnHttpAuthorizationHeaderHandling()
        {
            // Arrange
            const string url = "https://no-query.string";

            var client  = new RestClient(url);
            var request = new RestRequest();

            _authenticator.ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader;

            // Act
            _authenticator.Authenticate(client, request);

            // Assert
            var authParameter = request.Parameters.Single(x => x.Name == "Authorization");
            var value         = (string) authParameter.Value;

            Assert.IsTrue(value.Contains("OAuth"));
            Assert.IsTrue(value.Contains("realm=\"Realm\""));
            Assert.IsTrue(value.Contains("oauth_timestamp="));
            Assert.IsTrue(value.Contains("oauth_signature=\"ConsumerSecret"));
            Assert.IsTrue(value.Contains("oauth_nonce="));
            Assert.IsTrue(value.Contains("oauth_consumer_key=\"ConsumerKey\""));
            Assert.IsTrue(value.Contains("oauth_signature_method=\"PLAINTEXT\""));
            Assert.IsTrue(value.Contains("oauth_version=\"Version\""));
            Assert.IsTrue(value.Contains("x_auth_mode=\"client_auth\""));
            Assert.IsTrue(value.Contains("x_auth_username=\"ClientUsername\""));
            Assert.IsTrue(value.Contains("x_auth_password=\"ClientPassword\""));
        }

        [Test]
        public void Authenticate_ShouldAddSignatureToRequestAsSeparateParameters_OnUrlOrPostParametersHandling()
        {
            // Arrange
            const string url = "https://no-query.string";

            var client  = new RestClient(url);
            var request = new RestRequest();
            request.AddQueryParameter("queryparameter", "foobartemp");

            _authenticator.ParameterHandling = OAuthParameterHandling.UrlOrPostParameters;

            // Act
            _authenticator.Authenticate(client, request);

            // Assert
            var parameters = request.Parameters;

            Assert.IsNotNull(
                parameters.FirstOrDefault(
                    x => x.Type       == ParameterType.GetOrPost && x.Name == "x_auth_username" && (string) x.Value == "ClientUsername" &&
                        x.ContentType == null
                )
            );

            Assert.IsNotNull(
                parameters.FirstOrDefault(
                    x => x.Type       == ParameterType.GetOrPost && x.Name == "x_auth_password" && (string) x.Value == "ClientPassword" &&
                        x.ContentType == null
                )
            );

            Assert.IsNotNull(
                parameters.FirstOrDefault(
                    x => x.Type == ParameterType.GetOrPost && x.Name == "x_auth_mode" && (string) x.Value == "client_auth" && x.ContentType == null
                )
            );

            Assert.IsNotNull(
                parameters.FirstOrDefault(
                    x => x.Type       == ParameterType.GetOrPost && x.Name == "oauth_consumer_key" && (string) x.Value == "ConsumerKey" &&
                        x.ContentType == null
                )
            );

            Assert.IsNotNull(
                parameters.FirstOrDefault(
                    x => x.Type       == ParameterType.GetOrPost && x.Name == "oauth_signature" && !string.IsNullOrWhiteSpace((string) x.Value) &&
                        x.ContentType == null
                )
            );

            Assert.IsNotNull(
                parameters.FirstOrDefault(
                    x => x.Type       == ParameterType.GetOrPost && x.Name == "oauth_signature_method" && (string) x.Value == "PLAINTEXT" &&
                        x.ContentType == null
                )
            );

            Assert.IsNotNull(
                parameters.FirstOrDefault(
                    x => x.Type == ParameterType.GetOrPost && x.Name == "oauth_version" && (string) x.Value == "Version" && x.ContentType == null
                )
            );

            Assert.IsNotNull(
                parameters.FirstOrDefault(
                    x => x.Type       == ParameterType.GetOrPost && x.Name == "oauth_nonce" && !string.IsNullOrWhiteSpace((string) x.Value) &&
                        x.ContentType == null
                )
            );

            Assert.IsNotNull(
                parameters.FirstOrDefault(
                    x => x.Type       == ParameterType.GetOrPost && x.Name == "oauth_timestamp" && !string.IsNullOrWhiteSpace((string) x.Value) &&
                        x.ContentType == null
                )
            );
        }
    }
}