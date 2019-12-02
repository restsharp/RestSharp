using System.Net;
using NUnit.Framework;

namespace RestSharp.Tests
{
    [TestFixture]
    public class DecompressionMethodTests
    {
        [Test]
        public void ShouldDecompressionMethodsContainsDefaultValues()
        {
            var restRequest = new RestRequest();

            Assert.True(restRequest.AllowedDecompressionMethods.Contains(DecompressionMethods.None));
            Assert.True(restRequest.AllowedDecompressionMethods.Contains(DecompressionMethods.Deflate));
            Assert.True(restRequest.AllowedDecompressionMethods.Contains(DecompressionMethods.GZip));
        }

        [Test]
        public void ShouldDecompressionMethodsNotEmptyOrNull()
        {
            var restRequest = new RestRequest();

            Assert.IsNotNull(restRequest.AllowedDecompressionMethods);
            Assert.IsNotEmpty(restRequest.AllowedDecompressionMethods);
        }
    }
}