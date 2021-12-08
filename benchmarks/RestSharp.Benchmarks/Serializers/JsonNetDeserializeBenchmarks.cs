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
        readonly JsonNetSerializer _serializer = new();

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
