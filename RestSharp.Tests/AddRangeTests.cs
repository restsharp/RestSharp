using NUnit.Framework;

namespace RestSharp.Tests
{
    [TestFixture]
    public class AddRangeTests
    {
        [Test]
        public void ShouldParseOutRangeSpecifier()
        {
            RestClient restClient = new RestClient("http://localhost");
            RestRequest req = new RestRequest("bob", Method.GET);

            req.AddHeader("Range", "pages=1-2");
            restClient.Execute(req);
        }
    }
}
