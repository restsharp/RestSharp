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

        [Test]
        public void ShouldParseOutLongRangeSpecifier()
        {
            // This can't be tested wince the test project builds with .Net35


            //RestClient restClient = new RestClient("http://localhost");
            //RestRequest req = new RestRequest("bob", Method.GET);
            //long start = (long)int.MaxValue + 1;
            //long end = start + 1;

            //req.AddHeader("Range", string.Format("pages={0}-{1}", start, end));
            //restClient.Execute(req);
        }
    }
}
