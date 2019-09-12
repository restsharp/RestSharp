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
            ICollection<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Accept", "application/json"),
                new KeyValuePair<string, string>("Accept-Language", "en-us,en;q=0.5"),
                new KeyValuePair<string, string>("Keep-Alive", "300"),
                new KeyValuePair<string, string>("Accept", "application/json")
            };

            RestRequest request = new RestRequest();

            ArgumentException exception = Assert.Throws<ArgumentException>(() => request.AddHeaders(headers));
            Assert.AreEqual("Duplicate header names exist: ACCEPT", exception.Message);
        }
    }
}
