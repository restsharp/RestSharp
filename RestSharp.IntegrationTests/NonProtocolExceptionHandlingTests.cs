using System.Net;
using System.Threading;
using RestSharp.IntegrationTests.Helpers;
using Xunit;

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
            const string baseUrl = "http://localhost:8888/";

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

        [Fact(Skip = "There's an issue with this test. Looks like the behavior changed a bit for timeouts.")]
        // The asserts get trapped by a catch block and then added to the response.
        // Then the second assert is hit and it just hangs indefinitely.
        // Not sure why it can't break out.
        public void Handles_Server_Timeout_Error_Async()
        {
            const string baseUrl = "http://localhost:8888/";
            var resetEvent = new ManualResetEvent(false);
            IRestResponse response = null;

            using (SimpleServer.Create(baseUrl, TimeoutHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("404"); // {ReadWriteTimeout = 500}; // {Timeout = 500};

                client.ExecuteAsync(request, responseParam =>
                                             {
                                                 response = responseParam;
                                                 resetEvent.Set();
                                             });

                resetEvent.WaitOne();

                Assert.NotNull(response);
                Assert.NotNull(response.ErrorException);
                Assert.IsAssignableFrom(typeof(WebException), response.ErrorException);
                Assert.Equal(response.ErrorException.Message, "The operation has timed out");
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
            const string baseUrl = "http://localhost:8888/";

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
            Thread.Sleep(101000);
        }
    }
}
