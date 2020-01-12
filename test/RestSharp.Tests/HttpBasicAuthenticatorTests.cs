using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RestSharp.Authenticators;
using Shouldly;

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

        [Test]
        public void Authenticate_ShouldAddAuthorizationParameter_IfPreviouslyUnassigned()
        {
            // Arrange
            var client  = new RestClient();
            var request = new RestRequest();

            request.AddParameter(new Parameter("NotMatching", null, default));

            var expectedToken =
                $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"))}";

            // Act
            _authenticator.Authenticate(client, request);

            // Assert
            request.Parameters.Single(x => x.Name == "Authorization").Value.ShouldBe(expectedToken);
        }
    }
}