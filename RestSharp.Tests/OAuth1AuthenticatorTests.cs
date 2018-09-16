using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
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
        {
            authenticator = new OAuth1Authenticator
            {
                CallbackUrl = "CallbackUrl",
                ClientPassword = "ClientPassword",
                Type = OAuthType.ClientAuthentication,
                ClientUsername = "ClientUsername",
                ConsumerKey = "ConsumerKey",
                ConsumerSecret = "ConsumerSecret",
                Realm = "Realm",
                SessionHandle = "SessionHandle",
                SignatureMethod = OAuthSignatureMethod.PlainText,
                SignatureTreatment = OAuthSignatureTreatment.Escaped,
                Token = "Token",
                TokenSecret = "TokenSecret",
                Verifier = "Verifier",
                Version = "Version"
            };

            mockClient = new Mock<IRestClient>();
            mockClient.SetupGet(x => x.DefaultParameters).Returns(new List<Parameter>());

            mockRequest = new Mock<RestRequest>();

            mockWorkflow = new Mock<OAuthWorkflow>();
        }

        private OAuth1Authenticator authenticator;

        private Mock<RestRequest> mockRequest;
        private Mock<IRestClient> mockClient;
        private Mock<OAuthWorkflow> mockWorkflow;

        [Test]
        public void Authenticate_ShouldAddAuthorizationAsTextValueToRequest_OnHttpAuthorizationHeaderHandling()
        {
            // Arrange
            var uri = new Uri("https://no-query.string");
            mockClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>())).Returns(uri);

            authenticator.ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader;

            // Act
            authenticator.Authenticate(mockClient.Object, mockRequest.Object);

            // Assert
            var authParameter = mockRequest.Object.Parameters.Single(x => x.Name == "Authorization");
            var value = (string)authParameter.Value;

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
            var uri = new Uri("https://no-query.string?queryparameter=foobartemp");
            mockClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>())).Returns(uri);

            authenticator.ParameterHandling = OAuthParameterHandling.UrlOrPostParameters;

            // Act
            authenticator.Authenticate(mockClient.Object, mockRequest.Object);

            // Assert
            var parameters = mockRequest.Object.Parameters;
            
            Assert.IsNotNull(parameters.FirstOrDefault(x => x.Type == ParameterType.GetOrPost && x.Name == "x_auth_username" && (string)x.Value == "ClientUsername" && x.ContentType == null));
            Assert.IsNotNull(parameters.FirstOrDefault(x => x.Type == ParameterType.GetOrPost && x.Name == "x_auth_password" && (string)x.Value == "ClientPassword" && x.ContentType == null));
            Assert.IsNotNull(parameters.FirstOrDefault(x => x.Type == ParameterType.GetOrPost && x.Name == "x_auth_mode" && (string)x.Value == "client_auth" && x.ContentType == null));
            Assert.IsNotNull(parameters.FirstOrDefault(x => x.Type == ParameterType.GetOrPost && x.Name == "oauth_consumer_key" && (string)x.Value == "ConsumerKey" && x.ContentType == null));
            Assert.IsNotNull(parameters.FirstOrDefault(x => x.Type == ParameterType.GetOrPost && x.Name == "oauth_signature" && !string.IsNullOrWhiteSpace((string)x.Value) && x.ContentType == null));
            Assert.IsNotNull(parameters.FirstOrDefault(x => x.Type == ParameterType.GetOrPost && x.Name == "oauth_signature_method" && (string)x.Value == "PLAINTEXT" && x.ContentType == null));
            Assert.IsNotNull(parameters.FirstOrDefault(x => x.Type == ParameterType.GetOrPost && x.Name == "oauth_version" && (string)x.Value == "Version" && x.ContentType == null));
            Assert.IsNotNull(parameters.FirstOrDefault(x => x.Type == ParameterType.GetOrPost && x.Name == "oauth_nonce" && !string.IsNullOrWhiteSpace((string)x.Value) && x.ContentType == null));
            Assert.IsNotNull(parameters.FirstOrDefault(x => x.Type == ParameterType.GetOrPost && x.Name == "oauth_timestamp" && !string.IsNullOrWhiteSpace((string)x.Value) && x.ContentType == null));
        }

        [Test]
        public void AddOAuthData_ProtectedResource_ShouldRetainQueryParamsFromUrl()
        {
            authenticator.Type = OAuthType.ProtectedResource;

            var uri = new Uri("https://no-query.string?queryparameter=foobartemp");
            mockClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>())).Returns(uri);

            var mockCall = mockWorkflow
                .Setup(x => x.BuildProtectedResourceInfo(It.IsAny<string>(), It.IsAny<WebParameterCollection>(), It.IsAny<string>()))
                .Callback<string, WebParameterCollection, string>((methodValue, webParamsValue, urlValue) =>
                {
                    Assert.AreEqual(uri, urlValue);
                })
                .Returns(new OAuthWebQueryInfo());

            mockClient.SetupGet(x => x.DefaultParameters).Returns(new List<Parameter>());

            authenticator.AddOAuthData(mockClient.Object, mockRequest.Object, mockWorkflow.Object);
        }
    }
}