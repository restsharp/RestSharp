using AutoFixture;
using BenchmarkDotNet.Attributes;
using RestSharp.Serializers.NewtonsoftJson;

namespace RestSharp.Benchmarks.Serializers
{
    [MemoryDiagnoser]
    public class JsonNetSerializeBenchmarks
    {
        readonly JsonNetSerializer _serializer = new();

        List<TestClass> _fakeData;
        
        [Params(1, 10, 20)]
        public int N { get; set; }

        [GlobalSetup]
        public void GlobalSetup() => _fakeData = new Fixture().CreateMany<TestClass>(N).ToList();

        [Benchmark(Baseline = true)]
        public string Serialize() => _serializer.Serialize(_fakeData);
    }
}
