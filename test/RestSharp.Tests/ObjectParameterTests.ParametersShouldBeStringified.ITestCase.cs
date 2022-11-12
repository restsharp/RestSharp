using System.Collections;

namespace RestSharp.Tests;

public partial class ObjectParameterTests {
    sealed partial class ParametersShouldBeStringified {
        interface ITestCase {
            public int Int32 { get; }
            public string String { get; }
            public DateTime DateTime { get; }
            public object Object { get; }
            public IEnumerable<int> Ints { get; }
            public IEnumerable<string> Strings { get; }
            public IEnumerable<DateTime> DateTimes { get; }
            public IEnumerable<object> Objects { get; }
            public IEnumerable Enumerable { get; }
            public IEnumerable<IEnumerable<string>> NestedStrings { get; }
            public IEnumerable<IEnumerable<int>> NestedInts { get; }
            public IEnumerable<IEnumerable<DateTime>> NestedDateTimes { get; }
            public IEnumerable<IEnumerable<object>> NestedObjects { get; }
            public IEnumerable<IEnumerable> NestedEnumerables { get; }
        }
    }
}