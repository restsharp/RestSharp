using System.Collections;

namespace RestSharp.Tests.SampleData;
internal abstract class TestData : IEnumerable<object[]> {
    private protected abstract IEnumerable<object[]> GetData();
    public IEnumerator<object[]> GetEnumerator() => GetData().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
