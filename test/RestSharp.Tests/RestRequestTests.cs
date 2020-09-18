using System.Linq;
using FluentAssertions;
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
            Assert.AreEqual(2, request.Parameters.Count);
            Assert.AreEqual("query", parameters[0].Name);
            Assert.AreEqual("Id%3d198", parameters[0].Value);
            Assert.AreEqual(ParameterType.QueryStringWithoutEncode, parameters[0].Type);
            Assert.AreEqual("another", parameters[1].Name);
            Assert.AreEqual("notencoded", parameters[1].Value);
            Assert.AreEqual(ParameterType.QueryStringWithoutEncode, parameters[1].Type);
        }

        [Test]
        public void RestRequest_Parameters_ShouldProvideIdenticallyLists()
        {
            var request = new RestRequest().AddQueryParameter("one", "one");
            var firstParameters = request.Parameters;
            request = request.AddQueryParameter("two", "two");
            var secondParameters = request.Parameters;

            firstParameters.Should().HaveCount(2).And.Subject.Should().BeEquivalentTo(secondParameters);
        }
    }
}