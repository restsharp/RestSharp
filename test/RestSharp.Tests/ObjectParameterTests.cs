using System.Collections;
using System.Globalization;

namespace RestSharp.Tests;

public partial class ObjectParameterTests {
    public ObjectParameterTests() => Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

    [Fact]
    public void Can_Add_Object_with_IntegerArray_property() {
        var request = new RestRequest();
        var items = new[] { 2, 3, 4 };
        request.AddObject(new { Items = items });
        request.Parameters.First().Should().Be(new GetOrPostParameter("Items", string.Join(",", items)));
    }

    [Fact]
    public void Can_Add_Object_Static_with_Integer_property() {
        const int item = 1230;
        var @object = new { Item = item };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Item), "1230"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_Integer_as_Object_property() {
        const int item = 1230;
        var @object = new { Item = (object)item };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Item), "1230"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_IntegerArray_property() {
        var items = new[] { 1, 2, 3 };
        var @object = new { Items = items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "1,2,3"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_IntegerArray_as_ObjectArray_property() {
        var items = new object[] { 1, 2, 3 };
        var @object = new { Items = items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "1,2,3"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_IntegerArray_as_Object_property() {
        var items   = new[] { 1, 2, 3 };
        var @object = new { Items = (object)items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "1,2,3"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_IntegerArray_as_ObjectArray_as_Object_property() {
        var items = new object[] { 1, 2, 3 };
        var @object = new { Items = (object)items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "1,2,3"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_IntegerArray_as_Enumerable_property() {
        var items = new[] { 1, 2, 3 };
        var @object = new { Items = (IEnumerable)items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "1,2,3"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_IntegerArray_as_Enumerable_as_Object_property() {
        IEnumerable items = new[] { 1, 2, 3 };
        var @object = new { Items = (object)items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "1,2,3"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_String_property() {
        const string item = "Hello world";
        var @object = new { Item = item };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Item), item));
    }

    [Fact]
    public void Can_Add_Object_Static_with_String_as_Object_property() {
        const string item = "Hello world";
        var @object = new { Item = (object)item };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Item), item));
    }

    [Fact]
    public void Can_Add_Object_Static_with_StringArray_property() {
        var items = new[] { "Hello", "world", "from", "C#" };
        var @object = new { Items = items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "Hello,world,from,C#"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_StringArray_as_ObjectArray_property() {
        var items = new object[] { "Hello", "world", "from", "C#" };
        var @object = new { Items = items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "Hello,world,from,C#"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_StringArray_as_Object_property() {
        var items = new[] { "Hello", "world", "from", "C#" };
        var @object = new { Items = (object)items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "Hello,world,from,C#"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_StringArray_as_ObjectArray_as_Object_property() {
        var items = new object[] { "Hello", "world", "from", "C#" };
        var @object = new { Items = (object)items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "Hello,world,from,C#"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_StringArray_as_Enumerable_property() {
        var items = new[] { "Hello", "world", "from", "C#" };
        var @object = new { Items = (IEnumerable)items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "Hello,world,from,C#"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_StringArray_as_Enumerable_as_Object_property() {
        IEnumerable items = new[] { "Hello", "world", "from", "C#" };
        var @object = new { Items = (object)items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "Hello,world,from,C#"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_DateTime_property() {
        var item = DateTime.Parse("09/08/2025 13:35:23");
        var @object = new { Item = item };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Item), "09/08/2025 13:35:23"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_DateTime_as_Object_property() {
        var item = DateTime.Parse("04/06/2006 19:56:44");
        var @object = new { Item = (object)item };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Item), "04/06/2006 19:56:44"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_DateTimeArray_property() {
        var items = new[] { DateTime.Parse("01/01/2023 00:00:00"), DateTime.Parse("02/03/2024 14:30:00") };
        var @object = new { Items = items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "01/01/2023 00:00:00,02/03/2024 14:30:00"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_DateTimeArray_as_ObjectArray_property() {
        var items = new object[] { DateTime.Parse("01/01/2023 00:00:00"), DateTime.Parse("02/03/2024 14:30:00") };
        var @object = new { Items = items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "01/01/2023 00:00:00,02/03/2024 14:30:00"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_DateTimeArray_as_Object_property() {
        var items = new[] { DateTime.Parse("01/01/2023 00:00:00"), DateTime.Parse("02/03/2024 14:30:00") };
        var @object = new { Items = (object)items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "01/01/2023 00:00:00,02/03/2024 14:30:00"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_DateTimeArray_as_ObjectArray_as_Object_property() {
        var items = new object[] { DateTime.Parse("01/01/2023 00:00:00"), DateTime.Parse("02/03/2024 14:30:00") };
        var @object = new { Items = (object)items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "01/01/2023 00:00:00,02/03/2024 14:30:00"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_DateTimeArray_as_Enumerable_property() {
        var items = new[] { DateTime.Parse("01/01/2023 00:00:00"), DateTime.Parse("02/03/2024 14:30:00") };
        var @object = new { Items = (IEnumerable)items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "01/01/2023 00:00:00,02/03/2024 14:30:00"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_DateTimeArray_as_Enumerable_as_Object_property() {
        IEnumerable items = new[] { DateTime.Parse("01/01/2023 00:00:00"), DateTime.Parse("02/03/2024 14:30:00") };
        var @object = new { Items = (object)items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "01/01/2023 00:00:00,02/03/2024 14:30:00"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_ObjectArray_property() {
        var items = new object[] { "Hello world", 120, DateTime.Parse("06/06/2006 17:49:21"), Guid.Parse("1970a57f-d7f8-45d7-a269-f20e329d9432") };
        var @object = new { Items = items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "Hello world,120,06/06/2006 17:49:21,1970a57f-d7f8-45d7-a269-f20e329d9432"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_ObjectArray_as_Object_property() {
        var items = new object[] { "Hello world", 120, DateTime.Parse("06/06/2006 17:49:21"), Guid.Parse("1970a57f-d7f8-45d7-a269-f20e329d9432") };
        var @object = new { Items = (object)items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "Hello world,120,06/06/2006 17:49:21,1970a57f-d7f8-45d7-a269-f20e329d9432"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_ObjectArray_as_Enumerable_property() {
        var items = new object[] { "Hello world", 120, DateTime.Parse("06/06/2006 17:49:21"), Guid.Parse("1970a57f-d7f8-45d7-a269-f20e329d9432") };
        var @object = new { Items = (IEnumerable)items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "Hello world,120,06/06/2006 17:49:21,1970a57f-d7f8-45d7-a269-f20e329d9432"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_ObjectArray_as_Enumerable_as_Object_property() {
        IEnumerable items = new object[] { "Hello world", 120, DateTime.Parse("06/06/2006 17:49:21"), Guid.Parse("1970a57f-d7f8-45d7-a269-f20e329d9432") };
        var @object = new { Items = (object)items };
        var request = new RestRequest().AddObjectStatic(@object);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(@object.Items), "Hello world,120,06/06/2006 17:49:21,1970a57f-d7f8-45d7-a269-f20e329d9432"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_custom_property_name() {
        var item = new object[] { "Hello world", Array.Empty<Guid>() };
        var namedData = new NamedData(item);
        var request = new RestRequest().AddObjectStatic(namedData);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter("CustomName", "Hello world,Guid[] Array"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_DateTime_using_custom_property_format() {
        var item = DateTime.Parse("05/02/2020 09:12:33");
        var namedData = new FormattedData<DateTime>(item);
        var request = new RestRequest().AddObjectStatic(namedData);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(FormattedData<DateTime>.FormattedParameter), "09:12 AM"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_DateTime_as_Object_using_custom_property_format() {
        var item = DateTime.Parse("05/02/2020 09:12:33");
        var namedData = new FormattedData<object>(item);
        var request = new RestRequest().AddObjectStatic(namedData);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(FormattedData<object>.FormattedParameter), "09:12 AM"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_DateTimeArray_using_custom_property_format() {
        var item = new[] { DateTime.Parse("03/03/2019 12:11:00"), DateTime.Parse("10/05/2049 10:12:53"), DateTime.Parse("04/06/2025 23:44:59") };
        var namedData = new FormattedData<DateTime[]>(item);
        var request = new RestRequest().AddObjectStatic(namedData);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(FormattedData<DateTime[]>.FormattedParameter), "12:11 PM,10:12 AM,11:44 PM"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_DateTimeArray_as_ObjectArray_using_custom_property_format() {
        var item = new object[] { DateTime.Parse("03/03/2019 12:11:00"), DateTime.Parse("10/05/2049 10:12:53"), DateTime.Parse("04/06/2025 23:44:59") };
        var namedData = new FormattedData<object[]>(item);
        var request = new RestRequest().AddObjectStatic(namedData);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(FormattedData<object[]>.FormattedParameter), "12:11 PM,10:12 AM,11:44 PM"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_DateTimeArray_as_Object_using_custom_property_format() {
        var item = new[] { DateTime.Parse("03/03/2019 12:11:00"), DateTime.Parse("10/05/2049 10:12:53"), DateTime.Parse("04/06/2025 23:44:59") };
        var namedData = new FormattedData<object>(item);
        var request = new RestRequest().AddObjectStatic(namedData);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(FormattedData<object>.FormattedParameter), "12:11 PM,10:12 AM,11:44 PM"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_DateTimeArray_as_ObjectArray_as_Object_using_custom_property_format() {
        var item = new object[] { DateTime.Parse("03/03/2019 12:11:00"), DateTime.Parse("10/05/2049 10:12:53"), DateTime.Parse("04/06/2025 23:44:59") };
        var namedData = new FormattedData<object>(item);
        var request = new RestRequest().AddObjectStatic(namedData);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(FormattedData<object>.FormattedParameter), "12:11 PM,10:12 AM,11:44 PM"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_DateTimeArray_as_Enumerable_using_custom_property_format() {
        var item = new[] { DateTime.Parse("03/03/2019 12:11:00"), DateTime.Parse("10/05/2049 10:12:53"), DateTime.Parse("04/06/2025 23:44:59") };
        var namedData = new FormattedData<IEnumerable>(item);
        var request = new RestRequest().AddObjectStatic(namedData);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(FormattedData<IEnumerable>.FormattedParameter), "12:11 PM,10:12 AM,11:44 PM"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_DateTimeArray_as_ObjectArray_as_Enumerable_using_custom_property_format() {
        var item = new object[] { DateTime.Parse("03/03/2019 12:11:00"), DateTime.Parse("10/05/2049 10:12:53"), DateTime.Parse("04/06/2025 23:44:59") };
        var namedData = new FormattedData<IEnumerable>(item);
        var request = new RestRequest().AddObjectStatic(namedData);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(FormattedData<IEnumerable>.FormattedParameter), "12:11 PM,10:12 AM,11:44 PM"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_StringArray_using_Csv_format() {
        var items = new[] { "Hello", "world", "from", ".NET" };
        var namedData = new CsvData<string[]>(items);
        var request = new RestRequest().AddObjectStatic(namedData);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(CsvData<string[]>.Csv), "Hello,world,from,.NET"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_StringArray_as_ObjectArray_using_Csv_format() {
        var items = new object[] { "Hello", "world", "from", ".NET" };
        var namedData = new CsvData<object[]>(items);
        var request = new RestRequest().AddObjectStatic(namedData);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(CsvData<object[]>.Csv), "Hello,world,from,.NET"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_StringArray_as_Object_using_Csv_format() {
        var items = new[] { "Hello", "world", "from", ".NET" };
        var namedData = new CsvData<object>(items);
        var request = new RestRequest().AddObjectStatic(namedData);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(CsvData<object>.Csv), "Hello,world,from,.NET"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_StringArray_as_ObjectArray_as_Object_using_Csv_format() {
        var items = new object[] { "Hello", "world", "from", ".NET" };
        var namedData = new CsvData<object>(items);
        var request = new RestRequest().AddObjectStatic(namedData);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(CsvData<object>.Csv), "Hello,world,from,.NET"));
    }

    [Fact]
    public void Can_Add_Object_Static_with_StringArray_using_Array_format() {
        var items = new[] { "Hello", "world", "from", ".NET" };
        var namedData = new ArrayData<string[]>(items);
        var request = new RestRequest().AddObjectStatic(namedData);

        request
            .Parameters
            .Should()
            .BeEquivalentTo(new[] {
                new GetOrPostParameter($"{nameof(ArrayData<string[]>.Array)}[]", "Hello"),
                new GetOrPostParameter($"{nameof(ArrayData<string[]>.Array)}[]", "world"),
                new GetOrPostParameter($"{nameof(ArrayData<string[]>.Array)}[]", "from"),
                new GetOrPostParameter($"{nameof(ArrayData<string[]>.Array)}[]", ".NET")
            });
    }

    [Fact]
    public void Can_Add_Object_Static_with_StringArray_as_ObjectArray_using_Array_format() {
        var items = new object[] { "Hello", "world", "from", ".NET" };
        var namedData = new ArrayData<object[]>(items);
        var request = new RestRequest().AddObjectStatic(namedData);

        request
            .Parameters
            .Should()
            .BeEquivalentTo(new[] {
                new GetOrPostParameter($"{nameof(ArrayData<object[]>.Array)}[]", "Hello"),
                new GetOrPostParameter($"{nameof(ArrayData<object[]>.Array)}[]", "world"),
                new GetOrPostParameter($"{nameof(ArrayData<object[]>.Array)}[]", "from"),
                new GetOrPostParameter($"{nameof(ArrayData<object[]>.Array)}[]", ".NET")
            });
    }

    [Fact]
    public void Can_Add_Object_Static_with_StringArray_as_Object_using_Array_format() {
        var items = new[] { "Hello", "world", "from", ".NET" };
        var namedData = new ArrayData<object>(items);
        var request = new RestRequest().AddObjectStatic(namedData);

        request
            .Parameters
            .Should()
            .BeEquivalentTo(new[] {
                new GetOrPostParameter($"{nameof(ArrayData<object>.Array)}[]", "Hello"),
                new GetOrPostParameter($"{nameof(ArrayData<object>.Array)}[]", "world"),
                new GetOrPostParameter($"{nameof(ArrayData<object>.Array)}[]", "from"),
                new GetOrPostParameter($"{nameof(ArrayData<object>.Array)}[]", ".NET")
            });
    }

    [Fact]
    public void Can_Add_Object_Static_with_StringArray_as_ObjectArray_as_Object_using_Array_format() {
        var items = new object[] { "Hello", "world", "from", ".NET" };
        var namedData = new ArrayData<object>(items);
        var request = new RestRequest().AddObjectStatic(namedData);

        request
            .Parameters
            .Should()
            .BeEquivalentTo(new[] {
                new GetOrPostParameter($"{nameof(ArrayData<object>.Array)}[]", "Hello"),
                new GetOrPostParameter($"{nameof(ArrayData<object>.Array)}[]", "world"),
                new GetOrPostParameter($"{nameof(ArrayData<object>.Array)}[]", "from"),
                new GetOrPostParameter($"{nameof(ArrayData<object>.Array)}[]", ".NET")
            });
    }

    [Fact]
    public void RefStructs_are_ignored() {
        const string value = "Hello world";
        var stringValue = new StringValue(value);
        var request = new RestRequest().AddObjectStatic(stringValue);
        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(StringValue.Value), value));
    }

    [Fact]
    public void Properties_are_filtered() {
        var @object = new { Name = "Hello world", Age = 12, Guid = Guid.Parse("72df165c-0cef-4654-987f-cd844f1e5ce9"), Ignore = "Ignored" };
        var request = new RestRequest().AddObjectStatic(@object, nameof(@object.Name), nameof(@object.Age), nameof(@object.Guid));
        request
            .Parameters
            .Should()
            .BeEquivalentTo(new[] {
                new GetOrPostParameter(nameof(@object.Name), "Hello world"),
                new GetOrPostParameter(nameof(@object.Age), "12"),
                new GetOrPostParameter(nameof(@object.Guid), "72df165c-0cef-4654-987f-cd844f1e5ce9")
            });
    }

    public sealed record StringValue(string Value) {
        // ReSharper disable once UnusedMember.Global
        public ReadOnlySpan<char> AsSpan => Value.AsSpan();
    }
}
