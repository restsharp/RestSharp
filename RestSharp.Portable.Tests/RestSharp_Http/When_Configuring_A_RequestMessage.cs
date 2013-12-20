using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RestSharp.Tests
{
    [Trait("Unit", "When Configuring A RequestMessage")]
    public class When_Configuring_A_RequestMessage
    {
        [Fact]
        public void Then_RequestUri_Is_Set()
        {
            var uri = new Uri("http://example.com");

            var httpRequest = new HttpRequest();
            httpRequest.Url = uri;

            var requestMessage = new DefaultRequestMessage(HttpMethod.Get, httpRequest);

            Assert.Same(uri, requestMessage.Instance.RequestUri);
        }

        [Fact]
        public void Then_Method_Is_Set()
        {
            var httpRequest = new HttpRequest();

            var requestMessage = new DefaultRequestMessage(HttpMethod.Get, httpRequest);

            Assert.Same(HttpMethod.Get, requestMessage.Instance.Method);
        }

        [Fact]
        public void Then_Restricted_Headers_Are_Ignored()
        {
            var httpRequest = new HttpRequest();

            //Host is a restricted header (I think) and should not get copied into the Request Message
            httpRequest.Headers.Add(new HttpHeader() { Name = "Host", Value = new List<string>() { "" } });

            var requestMessage = new DefaultRequestMessage(HttpMethod.Get, httpRequest);

            Assert.False(requestMessage.Instance.Headers.Contains("Host"));
        }

        [Fact]
        public void Then_Having_Only_Files_Causes_MultipartMime_Header()
        {
            var httpRequest = new HttpRequest();
            httpRequest.Files.Add(new HttpFile() { Name="test", FileName="test.jpg", Data=new byte[0] });

            var baseMultiPartMediaType = "multipart/form-data";

            var requestMessage = new DefaultRequestMessage(HttpMethod.Post, httpRequest);

            Assert.IsType<MultipartFormDataContent>(requestMessage.Instance.Content);
            Assert.Contains(baseMultiPartMediaType, requestMessage.Instance.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public void Then_Having_One_File_Attachs_One_File_To_The_Request()
        {
            var httpRequest = new HttpRequest();
            httpRequest.Files.Add(new HttpFile() { Name = "test", FileName = "test.jpg", Data = new byte[0] });

            var requestMessage = new DefaultRequestMessage(HttpMethod.Post, httpRequest);

            var content = (MultipartFormDataContent)requestMessage.Instance.Content;

            Assert.True(content.OfType<ByteArrayContent>().Any());
            Assert.False(content.OfType<FormUrlEncodedContent>().Any());
            Assert.False(content.OfType<StringContent>().Any());
            Assert.IsType<ByteArrayContent>(content.FirstOrDefault());
        }

        [Fact]
        public void Then_Forcing_Multipart_Causes_MultipartMime_Header()
        {
            var httpRequest = new HttpRequest();
            httpRequest.AlwaysMultipartFormData = true;

            var baseMultiPartMediaType = "multipart/form-data";

            var requestMessage = new DefaultRequestMessage(HttpMethod.Post, httpRequest);

            Assert.IsType<MultipartFormDataContent>(requestMessage.Instance.Content);
            Assert.Contains(baseMultiPartMediaType, requestMessage.Instance.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public void Then_Having_Files_And_RequestBody_And_Parameters_Causes_MultipartMime_Header()
        {
            var httpRequest = new HttpRequest();
            httpRequest.Files.Add(new HttpFile() { Name = "test", FileName = "test.jpg", Data = new byte[0] });
            httpRequest.RequestBody = "{'unit':'test'}";
            httpRequest.Parameters.Add(new KeyValuePair<string, string>("Unit", "Test"));

            var baseMultiPartMediaType = "multipart/form-data";

            var requestMessage = new DefaultRequestMessage(HttpMethod.Post, httpRequest);

            Assert.IsType<MultipartFormDataContent>(requestMessage.Instance.Content);
            Assert.Contains(baseMultiPartMediaType, requestMessage.Instance.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public void Then_Having_Files_And_RequestBody_And_Parameters_Causes_Three_Items_In_The_Multipart_Content()
        {
            var httpRequest = new HttpRequest();
            httpRequest.Files.Add(new HttpFile() { Name = "test", FileName = "test.jpg", Data = new byte[0] });
            httpRequest.RequestBody = "{'unit':'test'}";
            httpRequest.Parameters.Add(new KeyValuePair<string, string>("Unit", "Test"));

            var requestMessage = new DefaultRequestMessage(HttpMethod.Post, httpRequest);

            var content = (MultipartFormDataContent)requestMessage.Instance.Content;

            Assert.True(content.OfType<ByteArrayContent>().Any());
            Assert.True(content.OfType<FormUrlEncodedContent>().Any());
            Assert.True(content.OfType<StringContent>().Any());
        }

        [Fact]
        public void Then_Having_Only_Parameters_Causes_FormEncoded_Header()
        {
            var httpRequest = new HttpRequest();
            httpRequest.Parameters.Add(new KeyValuePair<string, string>("Unit", "Test"));

            var mediaType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var requestMessage = new DefaultRequestMessage(HttpMethod.Post, httpRequest);

            Assert.IsType<FormUrlEncodedContent>(requestMessage.Instance.Content);
            Assert.Equal(mediaType, requestMessage.Instance.Content.Headers.ContentType);
        }

        [Fact]
        public async void Then_Having_One_Parameter_Encodes_One_Parameter_In_The_Request()
        {
            var httpRequest = new HttpRequest();
            httpRequest.Parameters.Add(new KeyValuePair<string, string>("Unit", "Test"));

            var mediaType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var requestMessage = new DefaultRequestMessage(HttpMethod.Post, httpRequest);

            var content = requestMessage.Instance.Content as FormUrlEncodedContent;
            var param = await content.ReadAsFormDataAsync();

            Assert.Equal(1, param.Count);
        }

        [Fact]
        public void Then_Having_Only_RequestBody_Causes_Text_Header()
        {
            var httpRequest = new HttpRequest();
            httpRequest.RequestBody = "{'unit':'test'}";

            var baseTestPlainMediaType = "text/plain";

            var requestMessage = new DefaultRequestMessage(HttpMethod.Post, httpRequest);

            Assert.IsType<StringContent>(requestMessage.Instance.Content);
            Assert.Contains(baseTestPlainMediaType, requestMessage.Instance.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async void Then_Having_Only_A_RequestBody_Adds_The_Body_In_The_Request()
        {
            string body = "{'unit':'test'}";

            var httpRequest = new HttpRequest();
            httpRequest.RequestBody = body;

            var requestMessage = new DefaultRequestMessage(HttpMethod.Post, httpRequest);

            var content = requestMessage.Instance.Content as StringContent;

            Assert.Equal(body, await content.ReadAsStringAsync());            
        }

        [Fact]
        public void Then_Having_Only_RequestBody_And_ContentType_Sets_Current_MediaType()
        {
            var httpRequest = new HttpRequest();
            httpRequest.RequestContentType = "application/json";
            httpRequest.RequestBody = "{'unit':'test'}";

            var mediaType = new MediaTypeHeaderValue("application/json");

            var requestMessage = new DefaultRequestMessage(HttpMethod.Post, httpRequest);

            Assert.IsType<StringContent>(requestMessage.Instance.Content);
            Assert.Equal(mediaType, requestMessage.Instance.Content.Headers.ContentType);
        }
    }
}
