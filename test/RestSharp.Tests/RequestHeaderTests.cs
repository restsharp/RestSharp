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
            var headers = new List<KeyValuePair<string, string>>
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
            var headers = new List<KeyValuePair<string, string>>
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
            var headers = new Dictionary<string, string>
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

        [Test]
        public void AddOrUpdateHeader_ShouldUpdateExistingHeader_WhenHeaderExist()
        {
            // Arrange
            var request = new RestRequest();
            request.AddHeader("Accept", "application/xml");

            // Act
            request.AddOrUpdateHeader("Accept", "application/json");

            // Assert
            var headers = request.Parameters.Where(parameter => parameter.Type == ParameterType.HttpHeader).ToArray();
            
            Assert.AreEqual("application/json", headers.First(parameter => parameter.Name == "Accept").Value);
            Assert.AreEqual(1, headers.Length);
        }
        
        [Test]
        public void AddOrUpdateHeader_ShouldUpdateExistingHeader_WhenHeaderDoesNotExist()
        {
            // Arrange
            var request = new RestRequest();

            // Act
            request.AddOrUpdateHeader("Accept", "application/json");

            // Assert
            var headers = request.Parameters.Where(parameter => parameter.Type == ParameterType.HttpHeader).ToArray();
            
            Assert.AreEqual("application/json", headers.First(parameter => parameter.Name == "Accept").Value);
            Assert.AreEqual(1, headers.Length);
        }
        
        [Test]
        public void AddOrUpdateHeaders_ShouldAddHeaders_WhenNoneExists()
        {
            // Arrange
            var headers = new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "Accept-Language", "en-us,en;q=0.5" },
                { "Keep-Alive", "300" }
            };
            
            var request = new RestRequest();

            // Act
            request.AddOrUpdateHeaders(headers);

            // Assert
            var requestHeaders = request.Parameters.Where(parameter => parameter.Type == ParameterType.HttpHeader).ToArray();
            
            Assert.AreEqual("application/json", requestHeaders.First(parameter => parameter.Name == "Accept").Value);
            Assert.AreEqual("en-us,en;q=0.5", requestHeaders.First(parameter => parameter.Name == "Accept-Language").Value);
            Assert.AreEqual("300", requestHeaders.First(parameter => parameter.Name == "Keep-Alive").Value);
            Assert.AreEqual(3, requestHeaders.Length);
        }
        
        [Test]
        public void AddOrUpdateHeaders_ShouldUpdateHeaders_WhenAllExists()
        {
            // Arrange
            var headers = new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "Keep-Alive", "300" }
            };
            var updatedHeaders = new Dictionary<string, string>
            {
                { "Accept", "application/xml" },
                { "Keep-Alive", "400" }
            };

            var request = new RestRequest();
            request.AddHeaders(headers);

            // Act
            request.AddOrUpdateHeaders(updatedHeaders);

            // Assert
            var requestHeaders = request.Parameters.Where(parameter => parameter.Type == ParameterType.HttpHeader).ToArray();
            
            Assert.AreEqual("application/xml", requestHeaders.First(parameter => parameter.Name == "Accept").Value);
            Assert.AreEqual("400", requestHeaders.First(parameter => parameter.Name == "Keep-Alive").Value);
            Assert.AreEqual(2, requestHeaders.Length);
        }
        
        [Test]
        public void AddOrUpdateHeaders_ShouldAddAndUpdateHeaders_WhenSomeExists()
        {
            // Arrange
            var headers = new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "Keep-Alive", "300" }
            };
            var updatedHeaders = new Dictionary<string, string>
            {
                { "Accept", "application/xml" },
                { "Accept-Language", "en-us,en;q=0.5" }
            };

            var request = new RestRequest();
            request.AddHeaders(headers);

            // Act
            request.AddOrUpdateHeaders(updatedHeaders);

            // Assert
            var requestHeaders = request.Parameters.Where(parameter => parameter.Type == ParameterType.HttpHeader).ToArray();
            
            Assert.AreEqual("application/xml", requestHeaders.First(parameter => parameter.Name == "Accept").Value);
            Assert.AreEqual("en-us,en;q=0.5", requestHeaders.First(parameter => parameter.Name == "Accept-Language").Value);
            Assert.AreEqual("300", requestHeaders.First(parameter => parameter.Name == "Keep-Alive").Value);
            Assert.AreEqual(3, requestHeaders.Length);
        }
    }
}
