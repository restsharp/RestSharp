using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using RestSharp.Authenticators;

namespace RestSharp.Tests
{
    [TestFixture]
    public class HttpBasicAuthenticatorTests
    {
        [SetUp]
        public void Setup()
        {
            _username = "username";
            _password = "password";

            _authenticator = new HttpBasicAuthenticator(_username, _password);
        }

        string _username;
        string _password;

        HttpBasicAuthenticator _authenticator;

        [TestCase("Authorization")]
        [TestCase("authorization")]
        [TestCase("AUTHORIZATION")]
        public void Authenticate_ShouldNotAddAuthorizationParameter_IfAlreadyAssigned(string parameterName)
        {
            // Arrange
            var mockClient  = new Mock<IRestClient>();
            var mockRequest = new Mock<IRestRequest>();

            mockRequest.SetupGet(x => x.Parameters)
                .Returns(
                    new List<Parameter> {new Parameter(parameterName, null, default)}
                );

            // Act
            _authenticator.Authenticate(mockClient.Object, mockRequest.Object);

            // Assert
            mockRequest.Verify(
                x =>
                    x.AddParameter("Authorization", It.IsAny<string>(), ParameterType.HttpHeader), Times.Never
            );
        }

        [Test]
        public void Authenticate_ShouldAddAuthorizationParameter_IfPreviouslyUnassigned()
        {
            // Arrange
            var mockClient  = new Mock<IRestClient>();
            var mockRequest = new Mock<IRestRequest>();

            mockRequest.SetupGet(x => x.Parameters)
                .Returns(
                    new List<Parameter> {new Parameter("NotMatching", null, default)}
                );

            var expectedToken =
                $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"))}";

            // Act
            _authenticator.Authenticate(mockClient.Object, mockRequest.Object);

            // Assert
            mockRequest.Verify(
                x =>
                    x.AddParameter("Authorization", expectedToken, ParameterType.HttpHeader), Times.Once
            );
        }
    }
}