using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RestSharp.Tests
{
    [Trait("Unit", "When Making An Http Request")]
    public class When_Making_An_Http_Request
    {
        [Fact]
        public void Then_Client_Raises_Exception_On_Dns_Resolution_Failure()
        {
            //test fails to fail if the ISP catches and redirects DNS failures
            var httpRequest = new HttpRequest();
            httpRequest.Url = new Uri("http://nonexistantdomainimguessing.org");

            var handler = new DefaultMessageHandler(httpRequest);
            handler.Instance.AllowAutoRedirect = false;

            // TODO this cannot be less than one, so we need to make sure
            // we go back and expose the AllowAutoRedirect property

            handler.Instance.AllowAutoRedirect = false;
            handler.Instance.MaxAutomaticRedirections = 1;

            var requestMessage = new DefaultRequestMessage(HttpMethod.Get, httpRequest);

            var client = new HttpClient(handler.Instance);

            var task = client.SendAsync(requestMessage.Instance).ContinueWith(t =>
            {
                Assert.True(t.IsFaulted);
            });
            task.Wait();
        }

        [Fact]
        public async void Then_Receive_A_Success_Response_With_Body()
        {
            var request = new HttpRequest();
            request.Url = new Uri("http://www.example.com");

            var handler = new FakeDefaultMessageHandler();

            Http http = new Http(request, handler, new DefaultRequestMessage(), new HttpClientWrapper(handler));

            var result = await http.AsGetAsync(HttpMethod.Get);

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public void Then_Process_A_BadRequest_Response_With_Body()
        {
            var request = new HttpRequest();
            request.Url = new Uri("http://www.example.com");

            var handler = new FakeDefaultMessageHandler();

            Http http = new Http(request, handler, new DefaultRequestMessage(), new HttpClientWrapper(handler));

            var task = http.AsGetAsync(HttpMethod.Get).ContinueWith(t =>
            {
                Assert.Equal(HttpStatusCode.BadRequest, t.Result.StatusCode);
            });
            task.Wait();

            
        }
    }

    public class FakeDefaultMessageHandler : DefaultMessageHandler
    {
        FakeMessageHandler _instance;
        public override HttpClientHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FakeMessageHandler();
                }

                return _instance;
            }
        }
    }

    public class FakeMessageHandler : HttpClientHandler 
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            throw new TimeoutException();
            //var response = new HttpResponseMessage(HttpStatusCode.OK);
            //response.RequestMessage = request;

            //return Task.FromResult(response);
        }

    }
}
