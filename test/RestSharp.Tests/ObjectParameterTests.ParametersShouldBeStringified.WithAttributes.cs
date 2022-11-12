using System.Collections;

namespace RestSharp.Tests;

public partial class ObjectParameterTests {
    sealed partial class ParametersShouldBeStringified {
        sealed record WithAttributes(
            [property: RequestProperty(Name = "Integer", Format = "00000")] int Int32,
            [property: RequestProperty(Name = "Text")] string String,
            [property: RequestProperty(Name = "Date", Format = "dddd, dd MMMM yyyy")] DateTime DateTime,
            [property: RequestProperty(Name = "FloatingPointNumbersCsv", Format = "0.00")] object Object,
            [property: RequestProperty(Name = "IntegersCsv", Format = "000")] IEnumerable<int> Ints,
            [property: RequestProperty(Name = "TextsCsv", ArrayQueryType = RequestArrayQueryType.CommaSeparated)] IEnumerable<string> Strings,
            [property: RequestProperty(Name = "TimesArray", Format = "hh:mm tt", ArrayQueryType = RequestArrayQueryType.ArrayParameters)] IEnumerable<DateTime> DateTimes,
            [property: RequestProperty(Name = "GuidsArray", Format = "N", ArrayQueryType = RequestArrayQueryType.ArrayParameters)] IEnumerable<object> Objects,
            [property: RequestProperty(Name = "CurrencyAmountsCsv", Format = "c2", ArrayQueryType = RequestArrayQueryType.CommaSeparated)] IEnumerable Enumerable,
            [property: RequestProperty(Name = "FlattenedTextsArray", ArrayQueryType = RequestArrayQueryType.ArrayParameters)] IEnumerable<IEnumerable<string>> NestedStrings,
            [property: RequestProperty(Name = "FlattenedIntsCsv", ArrayQueryType = RequestArrayQueryType.CommaSeparated)] IEnumerable<IEnumerable<int>> NestedInts,
            [property: RequestProperty(Name = "FlattenedTimesArray", Format = "hh:mm", ArrayQueryType = RequestArrayQueryType.ArrayParameters)] IEnumerable<IEnumerable<DateTime>> NestedDateTimes,
            [property: RequestProperty(Name = "FlattenedObjectsCsv")] IEnumerable<IEnumerable<object>> NestedObjects,
            [property: RequestProperty(Name = "FlattenedObjectsArray", ArrayQueryType = RequestArrayQueryType.ArrayParameters)] IEnumerable<IEnumerable> NestedEnumerables) :
            ITestCase;
    }
}