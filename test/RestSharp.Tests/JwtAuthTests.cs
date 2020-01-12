using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using RestSharp.Authenticators;

namespace RestSharp.Tests
{
    [TestFixture]
    public class JwtAuthTests
    {
        readonly string _testJwt;
        readonly string _expectedAuthHeaderContent;

        public JwtAuthTests()
        {
            Thread.CurrentThread.CurrentCulture   = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InstalledUICulture;

            _testJwt = "eyJ0eXAiOiJKV1QiLA0KICJhbGciOiJIUzI1NiJ9"  + "." +
                "eyJpc3MiOiJqb2UiLA0KICJleHAiOjEzMDA4MTkzODAsDQo" +
                "gImh0dHA6Ly9leGFtcGxlLmNvbS9pc19yb290Ijp0cnVlfQ" + "." +
                "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";

            _expectedAuthHeaderContent = $"Bearer {_testJwt}";
        }

        [Test]
        public void Can_Set_ValidFormat_Auth_Header()
        {
            var client  = new RestClient {Authenticator = new JwtAuthenticator(_testJwt)};
            var request = new RestRequest();

            //In real case client.Execute(request) will invoke Authenticate method
            client.Authenticator.Authenticate(client, request);

            var authParam = request.Parameters.Single(p => p.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase));

            Assert.True(authParam.Type == ParameterType.HttpHeader);
            Assert.AreEqual(_expectedAuthHeaderContent, authParam.Value);
        }

        [Test]
        public void Check_Only_Header_Authorization()
        {
            var client  = new RestClient {Authenticator = new JwtAuthenticator(_testJwt)};
            var request = new RestRequest();

            // Paranoid server needs "two-factor authentication": jwt header and query param key for example
            request.AddParameter("Authorization", "manualAuth", ParameterType.QueryString);

            // In real case client.Execute(request) will invoke Authenticate method
            client.Authenticator.Authenticate(client, request);

            var paramList = request.Parameters.FindAll(p => p.Name.Equals("Authorization"));

            Assert.AreEqual(2, paramList.Count);

            var queryAuthParam  = paramList.Single(p => p.Type.Equals(ParameterType.QueryString));
            var headerAuthParam = paramList.Single(p => p.Type.Equals(ParameterType.HttpHeader));

            Assert.AreEqual("manualAuth", queryAuthParam.Value);
            Assert.AreEqual(_expectedAuthHeaderContent, headerAuthParam.Value);
        }

        [Test]
        public void Set_Auth_Header_Only_Once()
        {
            var client  = new RestClient();
            var request = new RestRequest();

            request.AddHeader("Authorization", "second_header_auth_token");

            client.Authenticator = new JwtAuthenticator(_testJwt);

            //In real case client.Execute(...) will invoke Authenticate method
            client.Authenticator.Authenticate(client, request);

            var paramList = request.Parameters.FindAll(p => p.Name.Equals("Authorization"));

            Assert.AreEqual(1, paramList.Count);

            var authParam = paramList[0];

            Assert.True(authParam.Type == ParameterType.HttpHeader);
            Assert.AreEqual(_expectedAuthHeaderContent, authParam.Value);
            Assert.AreNotEqual("Bearer second_header_auth_token", authParam.Value);
        }

        [Test]
        public void Updates_Auth_Header()
        {
            var client  = new RestClient();
            var request = new RestRequest();

            var authenticator = new JwtAuthenticator(_expectedAuthHeaderContent);

            client.Authenticator = authenticator;
            client.Authenticator.Authenticate(client, request);
            
            authenticator.SetBearerToken("second_header_auth_token");
            client.Authenticator.Authenticate(client, request);

            var paramList = request.Parameters.FindAll(p => p.Name.Equals("Authorization"));

            Assert.AreEqual(1, paramList.Count);

            var authParam = paramList[0];

            Assert.True(authParam.Type == ParameterType.HttpHeader);
            Assert.AreNotEqual(_expectedAuthHeaderContent, authParam.Value);
            Assert.AreEqual("Bearer second_header_auth_token", authParam.Value);
        }

        [Test]
        public void Throw_Argument_Null_Exception()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new JwtAuthenticator(null));

            Assert.AreEqual("accessToken", exception.ParamName);
        }
    }
}