using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RestSharp.Tests
{
    public class RequestHeaderTests
    {
        [Test]
        public void AddHeaders_SameCaseDuplicatesExist_ThrowsException()
        {
            var headers = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Accept", "application/json"),
                new KeyValuePair<string, string>("Accept-Language", "en-us,en;q=0.5"),
                new KeyValuePair<string, string>("Keep-Alive", "300"),
                new KeyValuePair<string, string>("Accept", "application/json")
            };

            var request = new RestRequest();

            var exception = Assert.Throws<ArgumentException>(() => request.AddHeaders(headers));
            Assert.AreEqual("Duplicate header names exist: ACCEPT", exception.Message);
        }

        [Test]
        public void AddHeaders_DifferentCaseDuplicatesExist_ThrowsException()
        {
            var headers = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Accept", "application/json"),
                new KeyValuePair<string, string>("Accept-Language", "en-us,en;q=0.5"),
                new KeyValuePair<string, string>("Keep-Alive", "300"),
                new KeyValuePair<string, string>("acCEpt", "application/json")
            };

            var request = new RestRequest();

            var exception = Assert.Throws<ArgumentException>(() => request.AddHeaders(headers));
            Assert.AreEqual("Duplicate header names exist: ACCEPT", exception.Message);
        }

        [Test]
        public void AddHeaders_NoDuplicatesExist_Has3Headers()
        {
            var headers = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Accept", "application/json"),
                new KeyValuePair<string, string>("Accept-Language", "en-us,en;q=0.5"),
                new KeyValuePair<string, string>("Keep-Alive", "300")
            };

            var request = new RestRequest();
            request.AddHeaders(headers);

            var httpParameters = request.Parameters.Where(parameter => parameter.Type == ParameterType.HttpHeader);

            Assert.AreEqual(3, httpParameters.Count());
        }

        [Test]
        public void AddHeaders_NoDuplicatesExistUsingDictionary_Has3Headers()
        {
            var headers = new Dictionary<string, string>()
            {
                { "Accept", "application/json" },
                { "Accept-Language", "en-us,en;q=0.5" },
                { "Keep-Alive", "300" }
            };

            var request = new RestRequest();
            request.AddHeaders(headers);

            var httpParameters = request.Parameters.Where(parameter => parameter.Type == ParameterType.HttpHeader);

            Assert.AreEqual(3, httpParameters.Count());
        }
    }
}
