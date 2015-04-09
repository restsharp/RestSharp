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
    [Trait("Unit", "When Configuring A MessageHandler")]
    public class When_Configuring_A_MessageHandler
    {
        [Fact]
        public void Then_Handler_Sets_Credentials() {

            var credential = new NetworkCredential("Unit","Test");

            var http = new HttpRequest();
            http.Credentials = credential;

            var handler = new DefaultMessageHandler(http);

            Assert.Same(credential, handler.Instance.Credentials);
        }

        [Fact]
        public void Then_Handler_Sets_Proxy()
        {
            var proxy = new WebProxy("http://example.com", false);

            var request = new HttpRequest();
            request.Proxy = proxy;

            var handler = new DefaultMessageHandler(request);

            Assert.NotNull(handler.Instance.Proxy);
            Assert.IsAssignableFrom<IWebProxy>(proxy);
        }

        [Fact]
        public void Then_Handler_Sets_MaxAutomaticRedirections()
        {
            var request = new HttpRequest();
            request.MaxAutomaticRedirects = 5;

            var handler = new DefaultMessageHandler(request);

            Assert.Equal(5, handler.Instance.MaxAutomaticRedirections);
            Assert.True(handler.Instance.AllowAutoRedirect);
        }

        [Fact]
        public void Then_Handler_Sets_Cookies()
        {

            var request = new HttpRequest();
            request.Url = new Uri("http://example.com");
            request.Cookies.Add(new HttpCookie() { Name = "Unit", Value = "Test" });

            var handler = new DefaultMessageHandler(request);

            Assert.Equal(1, handler.Instance.CookieContainer.Count);
        }
    }
}