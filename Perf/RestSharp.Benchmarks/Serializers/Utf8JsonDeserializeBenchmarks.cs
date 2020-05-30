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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using AutoFixture;
using BenchmarkDotNet.Attributes;
using RestSharp.Serializers.Utf8Json;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;
using Utf8Json;

namespace RestSharp.Benchmarks.Serializers
{
    [MemoryDiagnoser]
    public class Utf8JsonDeserializeBenchmarks
    {
        [Params(1, 10, 20)]
        public int N { get; set; }
        private readonly Utf8JsonSerializer _utf8SonSerializer = new Utf8JsonSerializer();
        private readonly Utf8JsonSerializerOptimized _utf8SonSerializerOptimized = new Utf8JsonSerializerOptimized();
        private readonly Fixture _fixture = new Fixture();
        private RestResponse _fakeResponse;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var dummyData = _fixture.CreateMany<TestClass>(N).ToList();
            _fakeResponse = new RestResponse();
            _fakeResponse.RawBytes = JsonSerializer.Serialize(dummyData);
            _fakeResponse.Content = Encoding.UTF8.GetString(_fakeResponse.RawBytes);
        }

        [Benchmark(Baseline = true)]
        public List<TestClass> Deserialize() => _utf8SonSerializer.Deserialize<List<TestClass>>(_fakeResponse);
        
        [Benchmark]
        public List<TestClass> DeserializeOptimized() => _utf8SonSerializerOptimized.Deserialize<List<TestClass>>(_fakeResponse);
    }
}