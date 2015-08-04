using NUnit.Framework;

namespace RestSharp.Tests
{
    [TestFixture]
    public class AddRangeTests
    {
        [Test]
        public void ShouldParseOutRangeSpecifier()
        {
            var restClient = new RestClient("http://localhost");
            var req = new RestRequest("bob", Method.GET);
            
            req.AddHeader("Range", "pages=1-2");
            restClient.Execute(req);
        }
    }
}