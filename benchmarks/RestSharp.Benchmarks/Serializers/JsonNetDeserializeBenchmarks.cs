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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoFixture;
using BenchmarkDotNet.Attributes;
using RestSharp.Serializers.NewtonsoftJson;
using Utf8Json;

namespace RestSharp.Benchmarks.Serializers
{
    [MemoryDiagnoser]
    public class JsonNetDeserializeBenchmarks
    {
        readonly JsonNetSerializer _serializer = new JsonNetSerializer();

        RestResponse _fakeResponse;

        [Params(1, 10, 20)]
        public int N { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            var fakeData = new Fixture().CreateMany<TestClass>(N).ToList();
            _fakeResponse         = new RestResponse {RawBytes = JsonSerializer.Serialize(fakeData)};
            _fakeResponse.Content = Encoding.UTF8.GetString(_fakeResponse.RawBytes);
        }

        [Benchmark(Baseline = true)]
        public List<TestClass> Deserialize() => _serializer.Deserialize<List<TestClass>>(_fakeResponse);
    }
}
