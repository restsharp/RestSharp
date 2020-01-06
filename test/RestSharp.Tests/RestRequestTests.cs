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

        [Test]
        public void RestRequest_Test_Already_Encoded()
        {
            var request = new RestRequest("/api/get?query=Id%3d198&another=notencoded");

            Assert.AreEqual("/api/get", request.Resource);
            Assert.AreEqual(2, request.Parameters.Count);
            Assert.AreEqual("query", request.Parameters[0].Name);
            Assert.AreEqual("Id%3d198", request.Parameters[0].Value);
            Assert.AreEqual(ParameterType.QueryStringWithoutEncode, request.Parameters[0].Type);
            Assert.AreEqual("another", request.Parameters[1].Name);
            Assert.AreEqual("notencoded", request.Parameters[1].Value);
            Assert.AreEqual(ParameterType.QueryStringWithoutEncode, request.Parameters[1].Type);
        }
    }
}