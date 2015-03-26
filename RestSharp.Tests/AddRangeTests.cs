namespace RestSharp.Tests
{
    using Xunit;

    public class AddRangeTests
    {
        [Fact]
        public void ShouldParseOutRangeSpecifier()
        {
            var restClient = new RestClient("http://localhost");
            var req = new RestRequest("bob", Method.GET);
            
            req.AddHeader("Range", "pages=1-2");
            var resp = restClient.Execute(req);
        }
    }
}