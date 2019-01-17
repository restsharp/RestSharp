using NUnit.Framework;

namespace RestSharp.Tests
{
    public class RestRequestTests
    {
        [Test]
        public void RestRequest_Request_Property()
        {
            var request = new RestRequest("resource");

            Assert.AreEqual("resource", request.Resource);
        }
    }
}
