namespace RestSharp.Tests;

public partial class ObjectParameterTests {
    sealed record ArrayData<TEnumerable>([property: RequestProperty(ArrayQueryType = RequestArrayQueryType.ArrayParameters)] TEnumerable Array) where TEnumerable : notnull;
}
