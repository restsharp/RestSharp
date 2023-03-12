namespace RestSharp.Tests;

public partial class ObjectParameterTests {
    sealed record NamedData([property: RequestProperty(Name = "CustomName")] object NamedParameter);
}
