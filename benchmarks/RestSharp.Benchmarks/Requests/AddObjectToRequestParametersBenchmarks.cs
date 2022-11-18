using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using System.Globalization;

namespace RestSharp.Benchmarks.Requests {
    [MemoryDiagnoser, RankColumn, Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public partial class AddObjectToRequestParametersBenchmarks {
        Data _data;

        [GlobalSetup]
        public void GlobalSetup() {
            const string @string = "random string";
            const int arraySize = 10_000;
            var strings = new string[arraySize];
            Array.Fill(strings, @string);
            var ints = new int[arraySize];
            Array.Fill(ints, int.MaxValue);

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            var dateTime = DateTime.Parse("01/01/2013 03:03:12");

            _data = new Data(@string, int.MaxValue, strings, ints, dateTime, strings);
        }

        [Benchmark(Baseline = true)]
        public void AddObject() => new RestRequest().AddObject(_data);

        [Benchmark]
        public void AddObjectStatic() => new RestRequest().AddObjectStatic(_data);

    }
}
