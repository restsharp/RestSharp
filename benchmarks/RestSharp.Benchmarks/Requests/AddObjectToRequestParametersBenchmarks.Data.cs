namespace RestSharp.Benchmarks.Requests {
    public partial class AddObjectToRequestParametersBenchmarks {
        sealed record Data(string String, int Int32, string[] Strings, int[] Ints);
    }
}
