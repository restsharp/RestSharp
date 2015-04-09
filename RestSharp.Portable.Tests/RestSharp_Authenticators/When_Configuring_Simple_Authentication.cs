using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RestSharp.Tests.RestSharp_Authenticators
{
    [Trait("Unit", "When Configuring Simple Authentication")]
    public class When_Configuring_Simple_Authentication
    {
        string _username = "unit";
        string _password = "test";
        SimpleAuthenticator _auth;
        RestRequest _request;

        public When_Configuring_Simple_Authentication()
        {
            _auth = new SimpleAuthenticator("username", _username, "password", _password);
            _request = new RestRequest();
        }

        [Fact]
        public void Then_Authenticating_Creates_Two_New_HttpHeader_Parameters()
        {
            _auth.Authenticate(null, _request);

            var usernameHeader = _request.Parameters.FirstOrDefault(p => p.Name.Equals("username", StringComparison.OrdinalIgnoreCase));
            var passwordHeader = _request.Parameters.FirstOrDefault(p => p.Name.Equals("password", StringComparison.OrdinalIgnoreCase));

            Assert.NotNull(usernameHeader);
            Assert.Equal("username", usernameHeader.Name);
            Assert.Equal(_username, usernameHeader.Value);
            Assert.Equal(ParameterType.GetOrPost, usernameHeader.Type);

            Assert.NotNull(passwordHeader);
            Assert.Equal("password", passwordHeader.Name);
            Assert.Equal(_password, passwordHeader.Value);
            Assert.Equal(ParameterType.GetOrPost, passwordHeader.Type);
        }

    }
}
