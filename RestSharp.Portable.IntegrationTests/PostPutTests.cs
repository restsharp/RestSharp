using Newtonsoft.Json;
using RestSharp.IntegrationTests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Xunit;
using NUnit;
using NUnit.Framework;

namespace RestSharp.IntegrationTests
{
    //[Trait("Integration", "POST-PUT Style Tests")]
    [Category("Integation [POST-PUT Style Tests]")]
    public class PostPutTests
    {
        const string baseUrl = "http://localhost:8080/";

        //[Fact]
        [Test]
        public async void Can_Execute_POST()
        {
            using (SimpleServer.Create(baseUrl, Handlers.EchoPostObject()))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("users/123");

                request.Method = Method.POST;
                request.AddParameter("First", "Jon");
                request.AddParameter("Last", "Doe");

                var response = await client.ExecuteAsync(request);

                var name = JsonConvert.DeserializeObject<Name>(response.Content);

                Assert.NotNull(response.Content);
                Assert.AreEqual("Jon", name.First);
                Assert.AreEqual("Doe", name.Last);
            }
        }

        //[Fact]
        [Test]
        public async void Can_Execute_PostAsync()
        {
            using (SimpleServer.Create(baseUrl, Handlers.EchoPostObject()))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("users/123");

                request.RequestFormat = DataFormat.Json;
                request.AddBody(new Name() { First="Jon", Last="Doe" });

                var response = await client.ExecutePostAsync<Name>(request);

                Console.WriteLine("Name: " + response.Data.First + response.Data.Last);

                Assert.NotNull(response.Data);
                Assert.AreEqual("Jon", response.Data.First);
                Assert.AreEqual("Doe", response.Data.Last);
            }
        }

        //[Fact]
        [Test]
        public async void Can_Execute_POST_With_Body()
        {
            using (SimpleServer.Create(baseUrl, Handlers.EchoPostObject()))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("users/123");

                request.Method = Method.POST;
                request.AddBody(new Name() { First = "Jon", Last = "Doe" });

                var response = await client.ExecuteAsync<Name>(request);

                Console.WriteLine("Name: " + response.Data.First + response.Data.Last);

                Assert.NotNull(response.Data);
                Assert.AreEqual("Jon", response.Data.First);
                Assert.AreEqual("Doe", response.Data.Last);
            }
        }

        //[Fact]
        [Test]
        public async void Can_Execute_POST_With_Form_Parameters()
        {
            using (SimpleServer.Create(baseUrl, Handlers.EchoPostObject()))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("users/123");

                request.Method = Method.POST;
                request.AddParameter("First", "Jon");
                request.AddParameter("Last", "Doe");

                var response = await client.ExecuteAsync<Name>(request);

                Assert.NotNull(response.Data);
                Assert.AreEqual("Jon", response.Data.First);
                Assert.AreEqual("Doe", response.Data.Last);
            }
        }

        //[Fact]
        [Test]
        public async void Can_Execute_POST_With_Files()
        {
            byte[] bytes = await FileTests.LoadSourceFile();
            
            using (SimpleServer.Create(baseUrl, Handlers.EchoFileObject(bytes)))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("users/123");

                request.Method = Method.POST;
                request.AddFile("koala", bytes, "koala.jpg");

                var response = await client.ExecuteAsync<bool>(request);
                Assert.True(response.Data, "Byte arrays were not equal");
            }
        }

        //[Fact]
        [Test]
        public async void Can_Execute_POST_With_Multipart()
        {
            byte[] bytes = await FileTests.LoadSourceFile();

            using (SimpleServer.Create(baseUrl, Handlers.EchoMultiPartObject(bytes)))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("users/123");

                request.Method = Method.POST;
                request.AddParameter("First", "Jon");
                request.AddParameter("Last", "Doe");
                request.AddFile("koala", bytes, "koala.jpg");

                var response = await client.ExecuteAsync<Name>(request);

                Console.WriteLine("Name: " + response.Data.First + response.Data.Last);

                Assert.NotNull(response.Data);
                Assert.AreEqual("Jon", response.Data.First);
                Assert.AreEqual("Doe", response.Data.Last);
            }
        }


    }
}
