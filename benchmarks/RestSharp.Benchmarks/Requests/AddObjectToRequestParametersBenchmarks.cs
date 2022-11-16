using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace RestSharp.Benchmarks.Requests {
    [MemoryDiagnoser, RankColumn, Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public partial class AddObjectToRequestParametersBenchmarks {
        Data _data;
        string[] _fields;

        [GlobalSetup]
        public void GlobalSetup() {
            const string @string = "random string";
            const int arraySize = 10_000;
            var strings = new string[arraySize];
            Array.Fill(strings, @string);
            var ints = new int[arraySize];
            Array.Fill(ints, int.MaxValue);

            _data = new Data(@string, int.MaxValue, strings, ints);
            _fields = new[] { nameof(Data.String), nameof(Data.Int32), nameof(Data.Strings), nameof(Data.Ints) };
        }

        [Benchmark(Baseline = true)]
        public void AddObject() => new RestRequest().AddObject(_data);

        [Benchmark]
        public void AddObjectStatic() => new RestRequest().AddObjectStatic(_data);

        [Benchmark]
        public void AddObject_Filtered() => new RestRequest().AddObject(_data, _fields);

        [Benchmark]
        public void AddObjectStatic_Filtered() => new RestRequest().AddObjectStatic(_data, _fields);
    }
}
