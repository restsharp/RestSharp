using System.Net;
using System.Text;
using RestSharp.IntegrationTests.Extensions;
using Xunit;

namespace RestSharp.IntegrationTests
{
    public class MultipartFormTests
    {
        [Fact]
        public void Handles_File_Upload()
        {
            const string baseUrl = "http://localhost:8080/";
            using(SimpleServer.Create(baseUrl, EchoHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("echo");

                request.AddFile(stream => stream.WriteStringUtf8("THIS IS A FILE"), "text.txt", "text/plain");
                request.Method = Method.POST;

                var response = client.Execute(request);

                var echo = Encoding.UTF8.GetString(response.RawBytes);

                // derived from: http://www.w3.org/TR/html401/interact/forms.html#h-17.13.4.2
                var expected = string.Format(@"--{0}
Content-Disposition: form-data; name=""text.txt""; filename=""text.txt""
Content-Type: text/plain

THIS IS A FILE
--{0}--
", Http.FormBoundary);

                Assert.Equal(expected, echo);
            }
        }

        [Fact]
        public void Handles_File_Upload_With_Additional_Parameter()
        {
            const string baseUrl = "http://localhost:8080/";
            using(SimpleServer.Create(baseUrl, EchoHandler))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("echo");

                request.AddParameter("email", "email@email.com");
                request.AddFile(stream => stream.WriteStringUtf8("THIS IS A FILE"), "text.txt", "text/plain");
                request.Method = Method.POST;

                var response = client.Execute(request);

                var echo = Encoding.UTF8.GetString(response.RawBytes);

                // derived from: http://www.w3.org/TR/html401/interact/forms.html#h-17.13.4.2
                var expected = string.Format(@"--{0}
Content-Disposition: form-data; name=""text.txt""; filename=""text.txt""
Content-Type: text/plain

THIS IS A FILE
--{0}
Content-Disposition: form-data; name=""email""

email@email.com
--{0}--
", Http.FormBoundary);

                Assert.Equal(expected, echo);
            }
        }

        private static void EchoHandler(HttpListenerContext context)
        {
            context.Request.InputStream.CopyTo(context.Response.OutputStream);
        }
    }
}