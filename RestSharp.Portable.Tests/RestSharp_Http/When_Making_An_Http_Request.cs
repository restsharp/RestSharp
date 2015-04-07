using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RestSharp.Tests
{
    [Trait("Unit", "When Making An Http Request")]
    public class When_Making_An_Http_Request
    {
        [Fact]
        public async void Then_Receive_An_OK_HttpResponse()
        {
            var request = new HttpRequest();
            request.Url = new Uri("http://www.example.com");

            //var handler = new FakeDefaultMessageHandler(() => {
            //    return new HttpResponseMessage(HttpStatusCode.OK);
            //});

            Http http = new Http(request); //, handler, new DefaultRequestMessage(), new HttpClientWrapper(handler));

            var result = await http.AsGetAsync(HttpMethod.Get, CancellationToken.None);

            Assert.IsType<HttpResponse>(result);
            Assert.Equal(ResponseStatus.Completed, result.ResponseStatus);
        }

        [Fact]
        public async void Then_Receive_An_HttpResponse_With_Error_Details_When_DNS_Lookup_Failure()
        {
            var request = new HttpRequest();
            request.Url = new Uri("http://www.nonexistantdomainimguessing.org");

            var handler = new FakeDefaultMessageHandler(() =>
            {
                var exc = new HttpRequestException("An error occurred while sending the request.", new WebException("The remote name could not be resolved: 'www.nonexistantdomainimguessing.org'"));                
                throw exc; //simulate a DNS lookup failure
            });

            Http http = new Http(request, handler, new DefaultRequestMessage(), new HttpClientWrapper(handler));

            var result = await http.AsGetAsync(HttpMethod.Get, CancellationToken.None);

            System.Diagnostics.Debug.WriteLine(result.ToString());
            System.Diagnostics.Debug.WriteLine(result.ErrorException.ToString());
            System.Diagnostics.Debug.WriteLine(result.ResponseStatus);

            Assert.IsType<HttpResponse>(result);
            Assert.IsType<WebException>(result.ErrorException);
            Assert.Equal(ResponseStatus.Error, result.ResponseStatus);
        }
      
        [Fact]
        public async void Then_Receive_An_HttpResponse_With_Error_Details_When_TCP_Request_Timeout()
        {
            var request = new HttpRequest();
            request.Url = new Uri("http://10.255.255.1");

            var handler = new FakeDefaultMessageHandler(() =>
            {
                var exc = new HttpRequestException("An error occurred while sending the request.", new WebException("Unable to connect to the remote server", new SocketException(10060)));
                throw exc; //simulate a TCP request timeout
            });

            Http http = new Http(request, handler, new DefaultRequestMessage(), new HttpClientWrapper(handler));

            var result = await http.AsGetAsync(HttpMethod.Get, CancellationToken.None);

            System.Diagnostics.Debug.WriteLine(result.ToString());
            System.Diagnostics.Debug.WriteLine(result.ErrorException.ToString());
            System.Diagnostics.Debug.WriteLine(result.ResponseStatus);

            Assert.IsType<HttpResponse>(result);
            Assert.IsType<WebException>(result.ErrorException);
            Assert.Equal(ResponseStatus.Error, result.ResponseStatus);
        }

        [Fact]
        public async void Then_Receive_An_HttpResponse_With_Error_Details_When_Task_Timeout()
        {
            var request = new HttpRequest();
            request.Url = new Uri("http://10.255.255.1");
            request.Timeout = 10;

            var handler = new FakeDefaultMessageHandler(() =>
            {
                var exc = new TaskCanceledException("A task was canceled.");
                throw exc; //simulate a Task request timeout
            });

            Http http = new Http(request, handler, new DefaultRequestMessage(), new HttpClientWrapper(handler));

            var result = await http.AsGetAsync(HttpMethod.Get, CancellationToken.None);

            System.Diagnostics.Debug.WriteLine(result.ToString());
            System.Diagnostics.Debug.WriteLine(result.ErrorException.ToString());
            System.Diagnostics.Debug.WriteLine(result.ResponseStatus);

            Assert.IsType<HttpResponse>(result);
            Assert.IsType(typeof(WebException), result.ErrorException);
            Assert.Equal(ResponseStatus.TimedOut, result.ResponseStatus);
        }

        //[Fact]
        public async void Then_Receive_An_HttpResponse_With_Error_Details_When_Request_Is_Canceled()
        {
            var request = new HttpRequest();
            request.Url = new Uri("http://10.255.255.1");
            request.Timeout = 100000;

            var handler = new FakeDefaultMessageHandler(() =>
            {
                int counter = 0;
                while (counter < 1000)
                {
                    System.Threading.Thread.Sleep(100);
                    counter++;
                }
                
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            Http http = new Http(request, handler, new DefaultRequestMessage(), new HttpClientWrapper(handler));

            var cts = new CancellationTokenSource();
            cts.CancelAfter(5000);
            
            var result = await http.AsGetAsync(HttpMethod.Get, cts.Token);            

            System.Diagnostics.Debug.WriteLine(result.ToString());
            System.Diagnostics.Debug.WriteLine(result.ErrorException.ToString());
            System.Diagnostics.Debug.WriteLine(result.ResponseStatus);

            Assert.IsType<HttpResponse>(result);
            Assert.IsType<WebException>(result.ErrorException);
            Assert.Equal(ResponseStatus.Error, result.ResponseStatus);
        }


    }

    public class FakeDefaultMessageHandler : DefaultMessageHandler
    {
        Func<HttpResponseMessage> _handler;
        public FakeDefaultMessageHandler(Func<HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        FakeMessageHandler _instance;
        public override HttpClientHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FakeMessageHandler();
                    _instance.Handler = _handler;
                }

                return _instance;
            }
        }
    }

    public class FakeMessageHandler : HttpClientHandler 
    {
        public Func<HttpResponseMessage> Handler {get;set;}

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return Task.FromResult(this.Handler());
        }

    }
}
