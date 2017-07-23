using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using RestSharp.Authenticators;
using Xunit;

namespace RestSharp.Tests
{
    public class JwtAuthTests
    {
        private readonly string testJwt;

        private readonly string expectedAuthHeaderContent;

        public JwtAuthTests()
        {
            this.testJwt = "eyJ0eXAiOiJKV1QiLA0KICJhbGciOiJIUzI1NiJ9" + "." +
                           "eyJpc3MiOiJqb2UiLA0KICJleHAiOjEzMDA4MTkzODAsDQo" +
                           "gImh0dHA6Ly9leGFtcGxlLmNvbS9pc19yb290Ijp0cnVlfQ" + "." +
                           "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";

            this.expectedAuthHeaderContent = string.Format("Bearer {0}", this.testJwt);
        }

        [Fact]
        public void Throw_Argument_Null_Exception()
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new JwtAuthenticator(null));

            Assert.Equal("accessToken", exception.ParamName);
        }

        [Fact]
        public void Can_Set_ValidFormat_Auth_Header()
        {
            RestClient client = new RestClient { Authenticator = new JwtAuthenticator(this.testJwt) };
            RestRequest request = new RestRequest();

            //In real case client.Execute(request) will invoke Authenticate method
            client.Authenticator.Authenticate(client, request);

            Parameter authParam = request.Parameters.Single(p => p.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase));

            Assert.True(authParam.Type == ParameterType.HttpHeader);
            Assert.Equal(this.expectedAuthHeaderContent, authParam.Value);
        }

        [Fact]
        public void Check_Only_Header_Authorization()
        {
            RestClient client = new RestClient { Authenticator = new JwtAuthenticator(this.testJwt) };
            RestRequest request = new RestRequest();

            //Paranoic server needs "two-factor authentication": jwt header and query param key for example
            request.AddParameter("Authorization", "manualAuth", ParameterType.QueryString);

            //In real case client.Execute(request) will invoke Authenticate method
            client.Authenticator.Authenticate(client, request);

            List<Parameter> paramList = request.Parameters.FindAll(p => p.Name.Equals("Authorization"));

            Assert.Equal(2, paramList.Count);

            Parameter queryAuthParam = paramList.Single(p => p.Type.Equals(ParameterType.QueryString));
            Parameter headerAuthParam = paramList.Single(p => p.Type.Equals(ParameterType.HttpHeader));

            Assert.Equal("manualAuth", queryAuthParam.Value);
            Assert.Equal(this.expectedAuthHeaderContent, headerAuthParam.Value);
        }

        [Fact]
        public void Set_Auth_Header_Only_Once()
        {
            RestClient client = new RestClient();
            RestRequest request = new RestRequest();

            request.AddHeader("Authorization", this.expectedAuthHeaderContent);

            client.Authenticator = new JwtAuthenticator("second_header_auth_token");

            //In real case client.Execute(...) will invoke Authenticate method
            client.Authenticator.Authenticate(client, request);

            List<Parameter> paramList = request.Parameters.FindAll(p => p.Name.Equals("Authorization"));

            Assert.Equal(1, paramList.Count);

            Parameter authParam = paramList[0];

            Assert.True(authParam.Type == ParameterType.HttpHeader);
            Assert.Equal(this.expectedAuthHeaderContent, authParam.Value);
            Assert.NotEqual("second_header_auth_token", authParam.Value);
        }
    }
}
