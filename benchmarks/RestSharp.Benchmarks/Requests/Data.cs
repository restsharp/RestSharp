namespace RestSharp.Benchmarks.Requests {
    sealed record Data(
        string String,
        [property: RequestProperty(Name = "PropertyName")] int Int32,
        string[] Strings,
        [property: RequestProperty(Format = "00000", ArrayQueryType = RequestArrayQueryType.ArrayParameters)] int[] Ints,
        [property: RequestProperty(Name = "DateTime", Format = "hh:mm tt")] object DateTime,
        object StringArray);
}
