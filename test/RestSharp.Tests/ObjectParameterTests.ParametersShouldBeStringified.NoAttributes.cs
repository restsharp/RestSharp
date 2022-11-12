using System.Collections;

namespace RestSharp.Tests;

public partial class ObjectParameterTests {
    sealed partial class ParametersShouldBeStringified {
        sealed record NoAttributes(
            int Int32,
            string String,
            DateTime DateTime,
            object Object,
            IEnumerable<int> Ints,
            IEnumerable<string> Strings,
            IEnumerable<DateTime> DateTimes,
            IEnumerable<object> Objects,
            IEnumerable Enumerable,
            IEnumerable<IEnumerable<string>> NestedStrings,
            IEnumerable<IEnumerable<int>> NestedInts,
            IEnumerable<IEnumerable<DateTime>> NestedDateTimes,
            IEnumerable<IEnumerable<object>> NestedObjects,
            IEnumerable<IEnumerable> NestedEnumerables) :
            ITestCase;
    }
}