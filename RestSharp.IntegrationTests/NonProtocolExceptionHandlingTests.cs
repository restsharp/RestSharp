using System;
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

        public class StupidClass
        {
            public string Property { get; set; }
        }

        [Fact]
        public void Task_Handles_Non_Existent_Domain()
        {
            var client = new RestClient("http://192.168.1.200:8001");
            var request = new RestRequest("/")
            {
                RequestFormat = DataFormat.Json,
                Method = Method.GET
            };

            var task = client.ExecuteTaskAsync<StupidClass>(request);
            task.Wait();

            var response = task.Result;

            Assert.IsType(typeof(WebException), response.ErrorException);
            Assert.Equal("Unable to connect to the remote server", response.ErrorException.Message);
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
                var request = new RestRequest("404") { Timeout = 500 };
                var response = client.Execute(request);

                Assert.NotNull(response.ErrorException);
                Assert.IsAssignableFrom(typeof(WebException), response.ErrorException);
                Assert.Contains("The operation has timed out", response.ErrorException.Message);
            }
        }

        [Fact]
        public void Handles_Server_Timeout_Error_Async()
        {
            const string baseUrl = "http://localhost:8888/";
            var resetEvent = new ManualResetEvent(false);

            using (SimpleServer.Create(baseUrl, TimeoutHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("404") { Timeout = 500 };
                IRestResponse response = null;

                client.ExecuteAsync(request, responseCb =>
                                             {
                                                 response = responseCb;
                                                 resetEvent.Set();
                                             });

                resetEvent.WaitOne();

                Assert.NotNull(response);
                Assert.Equal(response.ResponseStatus, ResponseStatus.TimedOut);
                Assert.NotNull(response.ErrorException);
                Assert.IsAssignableFrom(typeof(WebException), response.ErrorException);
                Assert.Equal(response.ErrorException.Message, "The request timed-out.");
            }
        }

        [Fact]
        public void Handles_Server_Timeout_Error_AsyncTask()
        {
            const string baseUrl = "http://localhost:8888/";

            using (SimpleServer.Create(baseUrl, TimeoutHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("404") { Timeout = 500 };

                var task =  client.ExecuteTaskAsync(request);
                task.Wait();
                IRestResponse response = task.Result;
                Assert.NotNull(response);
                Assert.Equal(response.ResponseStatus, ResponseStatus.TimedOut);

                Assert.NotNull(response.ErrorException);
                Assert.IsAssignableFrom(typeof(WebException), response.ErrorException);
                Assert.Equal(response.ErrorException.Message, "The request timed-out.");
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
                var request = new RestRequest("404") { Timeout = 500 };
                var response = client.Execute<Response>(request);

                Assert.Null(response.Data);
                Assert.NotNull(response.ErrorException);
                Assert.IsAssignableFrom(typeof(WebException), response.ErrorException);
                Assert.Contains("The operation has timed out", response.ErrorException.Message);
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
