//  Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 

using System;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.IntegrationTests
{
    public class DefaultParameterTests
    {
        SimpleServer _server;

        [SetUp]
        public void SetupServer() => _server = SimpleServer.Create(RequestHandler.Handle);

        [TearDown]
        public void DisposeServer() => _server.Dispose();

        [Test]
        public void Should_add_default_and_request_query_get_parameters()
        {
            var client  = new RestClient(_server.Url).AddDefaultParameter("foo", "bar", ParameterType.QueryString);
            var request = new RestRequest().AddParameter("foo1", "bar1", ParameterType.QueryString);

            client.Get(request);

            var query = RequestHandler.Url.Query;
            query.Should().Contain("foo=bar");
            query.Should().Contain("foo1=bar1");
        }

        [Test]
        public void Should_add_default_and_request_url_get_parameters()
        {
            var client  = new RestClient(_server.Url + "{foo}/").AddDefaultParameter("foo", "bar", ParameterType.UrlSegment);
            var request = new RestRequest("{foo1}").AddParameter("foo1", "bar1", ParameterType.UrlSegment);

            client.Get(request);

            RequestHandler.Url.Segments.Should().BeEquivalentTo("/", "bar/", "bar1");
        }

        [Test]
        public void Should_not_throw_exception_when_name_is_null()
        {
            var client = new RestClient(_server.Url + "{foo}/").AddDefaultParameter("foo", "bar", ParameterType.UrlSegment);
            var request = new RestRequest("{foo1}").AddParameter(null, "value", ParameterType.RequestBody);

            client.Execute(request);
        }

        static class RequestHandler
        {
            public static Uri Url { get; private set; }

            public static void Handle(HttpListenerContext context)
            {
                Url = context.Request.Url;
                Handlers.Echo(context);
            }
        }
    }
}