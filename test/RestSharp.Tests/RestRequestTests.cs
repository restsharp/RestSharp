using System.Linq;
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

            var parameters = request.Parameters.ToList();
            Assert.AreEqual("/api/get", request.Resource);
            Assert.AreEqual(2, parameters.Count);
            Assert.AreEqual("query", parameters[0].Name);
            Assert.AreEqual("Id%3d198", parameters[0].Value);
            Assert.AreEqual(ParameterType.QueryStringWithoutEncode, parameters[0].Type);
            Assert.AreEqual("another", parameters[1].Name);
            Assert.AreEqual("notencoded", parameters[1].Value);
            Assert.AreEqual(ParameterType.QueryStringWithoutEncode, parameters[1].Type);
        }
    }
}