using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
namespace RestSharp.Tests
{
    [TestFixture]
    public class ParametersTests
    {
        const string BaseUrl = "http://localhost:8888/";
        
        [Test]
        public void AddDefaultHeadersUsingDictionary()
        {
            var headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"},
                {"Accept", "application/json"},
                {"Content-Encoding", "gzip, deflate"}
            };

            var expected = headers.Select(x => new Parameter(x.Key, x.Value, ParameterType.HttpHeader));

            var client = new RestClient(BaseUrl);
            client.AddDefaultHeaders(headers);
            
            expected.Should().BeSubsetOf(client.DefaultParameters);
        }
    }
}