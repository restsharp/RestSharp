using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RestSharp.Tests.RestSharp_Authenticators
{
    [Trait("Unit", "When Configuring Basic Authentication")]
    public class When_Configuring_Basic_Authentication
    {
        string _username = "unit";
        string _password = "test";
        HttpBasicAuthenticator _auth;
        RestRequest _request;

        public When_Configuring_Basic_Authentication()
        {
            _auth = new HttpBasicAuthenticator(_username, _password);
            _request = new RestRequest();
        }

        [Fact]
        public void Then_Authenticating_Creates_A_New_HttpHeader_Parameter_When_None_Exists()
        {
            var knownToken = string.Format("Basic {0}", Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", _username, _password))));

            _auth.Authenticate(null, _request);

            var header = _request.Parameters.FirstOrDefault(p => p.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase));

            Assert.NotNull(header);
            Assert.Equal("Authorization", header.Name);
            Assert.Equal(knownToken, header.Value);
            Assert.Equal(ParameterType.HttpHeader, header.Type);
        }

        [Fact]
        public void Then_Authenticating_Does_Not_Create_A_New_HttpHeader_Parameter_When_One_Exists()
        {
            var knownToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", "known", "token")));
            _request.AddHeader("Authorization", knownToken);

            _auth.Authenticate(null, _request);

            var header = _request.Parameters.FirstOrDefault(p => p.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase));

            Assert.NotNull(header);
            Assert.Equal("Authorization", header.Name);
            Assert.Equal(knownToken, header.Value);
            Assert.Equal(ParameterType.HttpHeader, header.Type);
        }
    }
}
