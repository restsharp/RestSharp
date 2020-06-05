using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests
{
    [TestFixture]
    public class NonProtocolExceptionHandlingTests
    {
        // ReSharper disable once ClassNeverInstantiated.Local
        class StupidClass
        {
            public string Property { get; set; }
        }

        /// <summary>
        /// Simulates a long server process that should result in a client timeout
        /// </summary>
        /// <param name="context"></param>
        static void TimeoutHandler(HttpListenerContext context) => Thread.Sleep(101000);

        [SetUp]
        public void Setup() => _server = SimpleServer.Create(TimeoutHandler);

        [TearDown]
        public void Teardown() => _server.Dispose();

        SimpleServer _server;

        /// <summary>
        /// Success of this test is based largely on the behavior of your current DNS.
        /// For example, if you're using OpenDNS this will test will fail; ResponseStatus will be Completed.
        /// </summary>
        [Test]
        public void Handles_Non_Existent_Domain()
        {
            var client   = new RestClient("http://nonexistantdomainimguessing.org");
            var request  = new RestRequest("foo");
            var response = client.Execute(request);

            Assert.AreEqual(ResponseStatus.Error, response.ResponseStatus);
        }

        /// <summary>
        /// Tests that RestSharp properly handles a non-protocol error.
        /// Simulates a server timeout, then verifies that the ErrorException
        /// property is correctly populated.
        /// </summary>
        [Test]
        public void Handles_Server_Timeout_Error()
        {
            var client = new RestClient(_server.Url);

            var request = new RestRequest("404")
            {
                Timeout = 500
            };
            var response = client.Execute(request);

            Assert.NotNull(response.ErrorException);
            Assert.IsInstanceOf<WebException>(response.ErrorException);
            Assert.AreEqual(ResponseStatus.TimedOut, response.ResponseStatus);
        }

        [Test]
        public void Handles_Server_Timeout_Error_Async()
        {
            var resetEvent = new ManualResetEvent(false);

            var client = new RestClient(_server.Url);

            var request = new RestRequest("404")
            {
                Timeout = 500
            };
            IRestResponse response = null;

            client.ExecuteAsync(
                request, responseCb =>
                {
                    response = responseCb;
                    resetEvent.Set();
                }
            );

            resetEvent.WaitOne();

            Assert.NotNull(response);
            Assert.AreEqual(ResponseStatus.TimedOut, response.ResponseStatus);
            Assert.NotNull(response.ErrorException);
            Assert.IsInstanceOf<WebException>(response.ErrorException);
            Assert.IsTrue(response.ErrorException.Message.Contains("timed"));
        }

        [Test]
        public async Task Handles_Server_Timeout_Error_AsyncTask()
        {
            var client   = new RestClient(_server.Url);
            var request  = new RestRequest("404") {Timeout = 500};
            var response = await client.ExecuteAsync(request);

            Assert.NotNull(response);
            Assert.AreEqual(ResponseStatus.TimedOut, response.ResponseStatus);

            Assert.NotNull(response.ErrorException);
            Assert.IsInstanceOf<WebException>(response.ErrorException);
            Assert.IsTrue(response.ErrorException.Message.Contains("timed"));
        }

        /// <summary>
        /// Tests that RestSharp properly handles a non-protocol error.
        /// Simulates a server timeout, then verifies that the ErrorException
        /// property is correctly populated.
        /// </summary>
        [Test]
        public void Handles_Server_Timeout_Error_With_Deserializer()
        {
            var client   = new RestClient(_server.Url);
            var request  = new RestRequest("404") {Timeout = 500};
            var response = client.Execute<Response>(request);

            Assert.Null(response.Data);
            Assert.NotNull(response.ErrorException);
            Assert.IsInstanceOf<WebException>(response.ErrorException);
            Assert.AreEqual(response.ResponseStatus, ResponseStatus.TimedOut);
        }

        [Test]
#if NETCORE
        [Ignore("Not supported for .NET Core")]
#endif
        public async Task Task_Handles_Non_Existent_Domain()
        {
            var client = new RestClient("http://this.cannot.exist:8001");

            var request = new RestRequest("/")
            {
                RequestFormat = DataFormat.Json,
                Method        = Method.GET
            };
            var response = await client.ExecuteAsync<StupidClass>(request);

            Assert.IsInstanceOf<WebException>(response.ErrorException);
            Assert.AreEqual(WebExceptionStatus.NameResolutionFailure, ((WebException) response.ErrorException).Status);
            Assert.AreEqual(ResponseStatus.Error, response.ResponseStatus);
        }
    }
}