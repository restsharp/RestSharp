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
        private string username;
        private string password;

        private HttpBasicAuthenticator authenticator;

        [SetUp]
        public void Setup()
        {
            username = "username";
            password = "password";

            authenticator = new HttpBasicAuthenticator(username, password);
        }

        [Test]
        public void Authenticate_ShouldAddAuthorizationParameter_IfPreviouslyUnassigned()
        {
            // Arrange
            var mockClient = new Mock<IRestClient>();
            var mockRequest = new Mock<IRestRequest>();

            mockRequest.SetupGet(x => x.Parameters).Returns(new List<Parameter>
            {
                new Parameter
                {
                    Name = "NotMatching"
                }
            });

            var expectedToken =
                $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", username, password)))}";

            // Act
            authenticator.Authenticate(mockClient.Object, mockRequest.Object);

            // Assert
            mockRequest.Verify(x => 
                x.AddParameter("Authorization", expectedToken, ParameterType.HttpHeader), Times.Once);
        }

        [TestCase("Authorization")]
        [TestCase("authorization")]
        [TestCase("AUTHORIZATION")]
        public void Authenticate_ShouldNotAddAuthorizationParameter_IfAlreadyAssigned(string parameterName)
        {
            // Arrange
            var mockClient = new Mock<IRestClient>();
            var mockRequest = new Mock<IRestRequest>();

            mockRequest.SetupGet(x => x.Parameters).Returns(new List<Parameter>
            {
                new Parameter
                {
                    Name = parameterName
                }
            });
            
            // Act
            authenticator.Authenticate(mockClient.Object, mockRequest.Object);

            // Assert
            mockRequest.Verify(x => 
                x.AddParameter("Authorization", It.IsAny<string>(), ParameterType.HttpHeader), Times.Never);
        }
    }
}
