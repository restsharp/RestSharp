using Xunit;

namespace RestSharp.Tests
{
    public class AddRangeTests
    {
        [Fact]
        public void ShouldParseOutRangeSpecifier()
        {
            RestClient restClient = new RestClient("http://localhost");
            RestRequest req = new RestRequest("bob", Method.GET);

            req.AddHeader("Range", "pages=1-2");
            restClient.ExecuteAsync(req, response => { });
        }
    }
}
