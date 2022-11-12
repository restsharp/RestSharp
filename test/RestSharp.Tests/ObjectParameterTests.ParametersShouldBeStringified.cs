using RestSharp.Tests.SampleData;
using System.Globalization;

namespace RestSharp.Tests;

public partial class ObjectParameterTests {
    sealed partial class ParametersShouldBeStringified : TestData {
        static readonly object[] TestNoAttributes =
            new object[] {
                    (RestRequest req) => {
                        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

                        return req.AddObjectStatic(new NoAttributes(
                            Int32: 1,
                            String: "Hello world",
                            DateTime: DateTime.Parse("11/11/2022 14:19:00"),
                            Object: Guid.Parse("c3e00f68-a4b3-4bbc-bcbd-5844d5490822"),
                            Ints: new[] { 2, 3, 4, 5 },
                            Strings: new[] { "Hello", "world", "from", "C#" },
                            DateTimes: new[] { DateTime.Parse("04/04/2002 13:45:55"), DateTime.Parse("10/10/2022 14:19:00"), DateTime.Parse("01/01/2000 00:00:01") },
                            Objects: new object[] { "Test this", 120, 33.333, DateTime.Parse("02/02/1992 13:13:13"), Guid.Parse("6c880d41-1f23-49e3-ade2-a5c4c12bdd9c") },
                            Enumerable: new object[] { "Test that", 12.333m, DateTime.Parse("10/10/2010 14:14:22"), Guid.Parse("1ff6942d-079f-4cb3-a617-67406183849f") },
                            NestedStrings: new string[][] { new[] { "Hello", "world" }, new[] { "from", "C#" } },
                            NestedInts: new int[][]{ new[] { 1, 2, 3 }, new[] { 4, 5, 6 } },
                            NestedDateTimes: new DateTime[][]{ new[] { DateTime.Parse("09/08/1988 14:19:00"), DateTime.Parse("09/08/1922 22:19:33") }, new[] { DateTime.Parse("04/05/2006 03:03:03") } },
                            NestedObjects: new object[][]{ new object[] { 1, 2, 3 }, new[] { "Hello", "world" }, new object[] { Guid.Parse("b030a2bd-9382-4c6d-8322-11e461bea03e") }, new object[] { 'A', 1, "Make" } },
                            NestedEnumerables: new object[][]{ new object[] { 'a', 'b', "letter c", 4, DateTime.Parse("07/04/1978 09:30:58"), Guid.Parse("71690435-5b08-415f-a551-766143197e31") }, new object[] { new object[] { "Awesome", new[] { 1, 2, 3 } } } }));
                    },
                    new GetOrPostParameter[] {
                        new("Int32", "1"),
                        new("String", "Hello world"),
                        new("DateTime", "11/11/2022 14:19:00"),
                        new("Object", "c3e00f68-a4b3-4bbc-bcbd-5844d5490822"),
                        new("Ints", "2,3,4,5"),
                        new("Strings", "Hello,world,from,C#"),
                        new("DateTimes", "04/04/2002 13:45:55,10/10/2022 14:19:00,01/01/2000 00:00:01"),
                        new("Objects", "Test this,120,33.333,02/02/1992 13:13:13,6c880d41-1f23-49e3-ade2-a5c4c12bdd9c"),
                        new("Enumerable", "Test that,12.333,10/10/2010 14:14:22,1ff6942d-079f-4cb3-a617-67406183849f"),
                        new("NestedStrings", "Hello,world,from,C#"),
                        new("NestedInts", "1,2,3,4,5,6"),
                        new("NestedDateTimes", "09/08/1988 14:19:00,09/08/1922 22:19:33,04/05/2006 03:03:03"),
                        new("NestedObjects", "1,2,3,Hello,world,b030a2bd-9382-4c6d-8322-11e461bea03e,A,1,Make"),
                        new("NestedEnumerables", "a,b,letter c,4,07/04/1978 09:30:58,71690435-5b08-415f-a551-766143197e31,Awesome,1,2,3")
                    }
                };

        static readonly object[] TestWithAttributes =
            new object[] {
                (RestRequest req) => {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

                    return req.AddObjectStatic(new WithAttributes(
                        Int32: 132,
                        String: "Goodbye world",
                        DateTime: DateTime.Parse("03/03/2003 16:45:33"),
                        Object: new object[] { 12f, 13.333d, 23.666m, new[] { 44f, 66f } },
                        Ints: new[] { 3, 4, 5, 6, 100, 1001 },
                        Strings: new[] { "Goodbye", "world", "from", ".NET" },
                        DateTimes: new[] { DateTime.Parse("11/10/2033 14:25:00"), DateTime.Parse("11/11/2011 19:45:00"), DateTime.Parse("04/04/2006 09:00:00") },
                        Objects: new object[]{ Guid.Parse("f90b8c60-a7f7-4cea-bea4-360a5dcd5ec9"), Guid.Parse("7b692a2a-2398-4b60-a374-dc64aff3feb0"), new[] { Guid.Parse("0dbf0389-5337-436d-8293-ca3a276abd72"), Guid.Parse("89f66976-8d82-4972-a062-f7a3f141446f") } },
                        Enumerable: new object[]{ 100, 333, 1_000_000, 123.33f, 33_012_123.99d, 777.21m, new[] { 11.333399f, 23f, 30f } },
                        NestedStrings: new string[][]{ new[] { "I", "am", "being", "tested" }, new[] { "from", "C#", ".NET" } },
                        NestedInts: new int[][] { new[] { 10, 20, 30 }, new[] { 33, 66, 99 } },
                        NestedDateTimes: new DateTime[][]{ new[] { DateTime.Parse("09/04/2008 03:33:03"), DateTime.Parse("10/03/03 12:09:33") }, new[] { DateTime.Parse("05/06/2013 14:45:00") } },
                        NestedObjects: new object[][] { new[] { "Hello", "world" }, new object[] { 1, 2, 3 }, new object[] { Guid.Parse("de5bb701-3830-48c6-a44c-fcce3384f03e") }, new object[] { "Good", 'C', 300m } },
                        NestedEnumerables: new object[][] { new object[] { "This", Guid.Parse("4439bad3-4e69-41f7-bc8b-a7a6c4e6d81a"), DateTime.Parse("08/07/2014 09:08:02") }, new object[] { 1, 2, 3, new[] { 'A', 'B' } } }));
                },
                new GetOrPostParameter[] {
                    new("Integer", "00132"),
                    new("Text", "Goodbye world"),
                    new("Date", "Monday, 03 March 2003"),
                    new("FloatingPointNumbersCsv", "12.00,13.33,23.67,44.00,66.00"),
                    new("IntegersCsv", "003,004,005,006,100,1001"),
                    new("TextsCsv", "Goodbye,world,from,.NET"),
                    new("TimesArray[]", "02:25 PM"),
                    new("TimesArray[]", "07:45 PM"),
                    new("TimesArray[]", "09:00 AM"),
                    new("GuidsArray[]", "f90b8c60a7f74ceabea4360a5dcd5ec9"),
                    new("GuidsArray[]", "7b692a2a23984b60a374dc64aff3feb0"),
                    new("GuidsArray[]", "0dbf03895337436d8293ca3a276abd72"),
                    new("GuidsArray[]", "89f669768d824972a062f7a3f141446f"),
                    new("CurrencyAmountsCsv", "¤100.00,¤333.00,¤1,000,000.00,¤123.33,¤33,012,123.99,¤777.21,¤11.33,¤23.00,¤30.00"),
                    new("FlattenedTextsArray[]", "I"),
                    new("FlattenedTextsArray[]", "am"),
                    new("FlattenedTextsArray[]", "being"),
                    new("FlattenedTextsArray[]", "tested"),
                    new("FlattenedTextsArray[]", "from"),
                    new("FlattenedTextsArray[]", "C#"),
                    new("FlattenedTextsArray[]", ".NET"),
                    new("FlattenedIntsCsv", "10,20,30,33,66,99"),
                    new("FlattenedTimesArray[]", "03:33"),
                    new("FlattenedTimesArray[]", "12:09"),
                    new("FlattenedTimesArray[]", "02:45"),
                    new("FlattenedObjectsCsv", "Hello,world,1,2,3,de5bb701-3830-48c6-a44c-fcce3384f03e,Good,C,300"),
                    new("FlattenedObjectsArray[]", "This"),
                    new("FlattenedObjectsArray[]", "4439bad3-4e69-41f7-bc8b-a7a6c4e6d81a"),
                    new("FlattenedObjectsArray[]", "08/07/2014 09:08:02"),
                    new("FlattenedObjectsArray[]", "1"),
                    new("FlattenedObjectsArray[]", "2"),
                    new("FlattenedObjectsArray[]", "3"),
                    new("FlattenedObjectsArray[]", "A"),
                    new("FlattenedObjectsArray[]", "B")
                }
            };

        private protected override IEnumerable<object[]> GetData() =>
            new object[][] {
                TestNoAttributes,
                TestWithAttributes
            };
    }
}