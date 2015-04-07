using Newtonsoft.Json;
using RestSharp.IntegrationTests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RestSharp.IntegrationTests
{
    [Trait("Integration", "POST-PUT Style Tests")]
    public class PostPutTests
    {
        const string baseUrl = "http://localhost:8080/";

        [Fact]
        
        public async void Can_Execute_POST()
        {
            using (SimpleServer.Create(baseUrl, Handlers.EchoPostObject()))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("users/123");

                request.Method = Method.POST;
                request.AddParameter("First", "Jon");
                request.AddParameter("Last", "Doe");

                var response = await client.ExecuteTaskAsync(request);

                var name = JsonConvert.DeserializeObject<Name>(response.Content);

                Assert.NotNull(response.Content);
                Assert.Equal("Jon", name.First);
                Assert.Equal("Doe", name.Last);
            }
        }

        [Fact]
        
        public async void Can_Execute_PostAsync()
        {
            using (SimpleServer.Create(baseUrl, Handlers.EchoPostObject()))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("users/123");

                request.RequestFormat = DataFormat.Json;
                request.AddBody(new Name() { First="Jon", Last="Doe" });

                var response = await client.ExecutePostTaskAsync<Name>(request);

                Console.WriteLine("Name: " + response.Data.First + response.Data.Last);

                Assert.NotNull(response.Data);
                Assert.Equal("Jon", response.Data.First);
                Assert.Equal("Doe", response.Data.Last);
            }
        }

        [Fact]
        
        public async void Can_Execute_POST_With_Body()
        {
            using (SimpleServer.Create(baseUrl, Handlers.EchoPostObject()))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("users/123");

                request.Method = Method.POST;
                request.AddBody(new Name() { First = "Jon", Last = "Doe" });

                var response = await client.ExecuteTaskAsync<Name>(request);

                Console.WriteLine("Name: " + response.Data.First + response.Data.Last);

                Assert.NotNull(response.Data);
                Assert.Equal("Jon", response.Data.First);
                Assert.Equal("Doe", response.Data.Last);
            }
        }

        [Fact]
        
        public async void Can_Execute_POST_With_Form_Parameters()
        {
            using (SimpleServer.Create(baseUrl, Handlers.EchoPostObject()))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("users/123");

                request.Method = Method.POST;
                request.AddParameter("First", "Jon");
                request.AddParameter("Last", "Doe");

                var response = await client.ExecuteTaskAsync<Name>(request);

                Assert.NotNull(response.Data);
                Assert.Equal("Jon", response.Data.First);
                Assert.Equal("Doe", response.Data.Last);
            }
        }

        [Fact]
        
        public async void Can_Execute_POST_With_Files()
        {
            byte[] bytes = await FileTests.LoadSourceFile();
            
            using (SimpleServer.Create(baseUrl, Handlers.EchoFileObject(bytes)))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("users/123");

                request.Method = Method.POST;
                request.AddFile("koala", bytes, "koala.jpg");

                var response = await client.ExecuteTaskAsync<bool>(request);
                Assert.True(response.Data, "Byte arrays were not equal");
            }
        }

        [Fact]
        
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

                var response = await client.ExecuteTaskAsync<Name>(request);

                Console.WriteLine("Name: " + response.Data.First + response.Data.Last);

                Assert.NotNull(response.Data);
                Assert.Equal("Jon", response.Data.First);
                Assert.Equal("Doe", response.Data.Last);
            }
        }


    }
}
