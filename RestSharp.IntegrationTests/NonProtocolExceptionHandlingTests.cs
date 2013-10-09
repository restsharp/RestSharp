using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using RestSharp.IntegrationTests.Helpers;
using System.Net;
using System.Threading;

namespace RestSharp.IntegrationTests
{
    public class NonProtocolExceptionHandlingTests
    {

        /// <summary>
        /// Success of this test is based largely on the behavior of your current DNS.
        /// For example, if you're using OpenDNS this will test will fail; ResponseStatus will be Completed.
        /// </summary>
        [Fact]
        public void Handles_Non_Existent_Domain()
        {
            var client = new RestClient("http://nonexistantdomainimguessing.org");
            var request = new RestRequest("foo");
            var response = client.Execute(request);
            
            Assert.Equal(ResponseStatus.Error, response.ResponseStatus);
        }

        /// <summary>
        /// Tests that RestSharp properly handles a non-protocol error.
        /// Simulates a server timeout, then verifies that the ErrorException
        /// property is correctly populated.
        /// </summary>
        [Fact]
        public void Handles_Server_Timeout_Error()
        {
            const string baseUrl = "http://localhost:8080/";
            using (SimpleServer.Create(baseUrl, TimeoutHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("404");
                var response = client.Execute(request);

                Assert.NotNull(response.ErrorException);
                Assert.IsAssignableFrom(typeof(WebException), response.ErrorException);
                Assert.Equal(response.ErrorException.Message, "The operation has timed out");                

            }
        }

        [Fact]
        public void Handles_Server_Timeout_Error_Async()
        {            
            const string baseUrl = "http://localhost:8080/";
            var resetEvent = new ManualResetEvent(false); 
            
            using (SimpleServer.Create(baseUrl, TimeoutHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("404");
                client.ExecuteAsync(request, response => {

                    Assert.NotNull(response.ErrorException);
                    Assert.IsAssignableFrom(typeof(WebException), response.ErrorException);
                    Assert.Equal(response.ErrorException.Message, "The operation has timed out");
                    resetEvent.Set();
                });
                resetEvent.WaitOne();
            }
        }

        /// <summary>
        /// Tests that RestSharp properly handles a non-protocol error.   
        /// Simulates a server timeout, then verifies that the ErrorException
        /// property is correctly populated.
        /// </summary>
        [Fact]
        public void Handles_Server_Timeout_Error_With_Deserializer()
        {
            const string baseUrl = "http://localhost:8080/";
            using (SimpleServer.Create(baseUrl, TimeoutHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("404");
                var response = client.Execute<Response>(request);

                Assert.Null(response.Data);
                Assert.NotNull(response.ErrorException);
                Assert.IsAssignableFrom(typeof(WebException), response.ErrorException);
                Assert.Equal(response.ErrorException.Message, "The operation has timed out");

            }
        }

        
        /// <summary>
        /// Simulates a long server process that should result in a client timeout
        /// </summary>
        /// <param name="context"></param>
        public static void TimeoutHandler(HttpListenerContext context)
        {
            System.Threading.Thread.Sleep(101000);
        }


    }
}
