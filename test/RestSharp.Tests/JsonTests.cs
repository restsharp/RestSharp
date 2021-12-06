using System.Collections;
using System.Globalization;
using RestSharp.Serialization.Json;
using RestSharp.Tests.Fixtures;
using RestSharp.Tests.SampleClasses;
using RestSharp.Tests.TestData;

namespace RestSharp.Tests; 

public class JsonTests {
    const string AlternativeCulture = "pt-PT";

    static readonly string CurrentPath = AppDomain.CurrentDomain.BaseDirectory;

    static T GetPayLoad<T>(string fileName) {
        var doc        = File.ReadAllText(Path.Combine(CurrentPath, "SampleData", fileName));
        var response   = new RestResponse { Content = doc };
        var serializer = new JsonSerializer();

        return serializer.Deserialize<T>(response);
    }

    [Fact]
    public void Can_Deserialize_4sq_Json_With_Root_Element_Specified() {
        var doc        = File.ReadAllText(Path.Combine(CurrentPath, "SampleData", "4sq.json"));
        var serializer = new JsonSerializer { RootElement = "response" };
        var output     = serializer.Deserialize<VenuesResponse>(new RestResponse { Content = doc });

        Assert.NotEmpty(output.Groups);
    }

    [Fact]
    public void Can_Deserialize_Custom_Formatted_Date() {
        var          culture   = CultureInfo.InvariantCulture;
        const string format    = "dd yyyy MMM, hh:mm ss tt";
        var          date      = new DateTime(2010, 2, 8, 11, 11, 11);
        var          formatted = new { StartDate = date.ToString(format, culture) };
        var          data      = SimpleJson.SerializeObject(formatted);
        var          response  = new RestResponse { Content = data };

        var serializer = new JsonSerializer {
            DateFormat = format,
            Culture    = culture
        };
        var output = serializer.Deserialize<PersonForJson>(response);

        Assert.Equal(date, output.StartDate);
    }

    [Fact]
    public void Can_Deserialize_Date_With_Milliseconds() {
        const string content    = "{ \"CreatedOn\": \"2018-10-01T14:39:00.123Z\" }";
        var          serializer = new JsonSerializer();
        var          output     = serializer.Deserialize<DateTimeResponse>(new RestResponse { Content = content });
        var          expected   = DateTime.Parse("2018-10-01 14:39:00", CultureInfo.InvariantCulture);

        Assert.NotNull(output);
        Assert.Equal(DateTimeKind.Utc, output.CreatedOn.Kind);

        Assert.Equal(
            expected.ToString(CultureInfo.InvariantCulture),
            output.CreatedOn.ToString(CultureInfo.InvariantCulture)
        );
    }

    [Fact]
    public void Can_Deserialize_DateTime() {
        var payload = GetPayLoad<DateTimeTestStructure>("datetimes.json");

        Assert.Equal(
            new DateTime(2011, 6, 30, 8, 15, 46, 929, DateTimeKind.Utc),
            payload.DateTime
        );
    }

    [Fact]
    public void Can_Deserialize_DateTime_With_DateTimeStyles() {
        var item0 = new DateTime(2010, 2, 8, 11, 11, 11, DateTimeKind.Local);
        var item1 = new DateTime(2011, 2, 8, 11, 11, 11, DateTimeKind.Utc);
        var item2 = new DateTime(2012, 2, 8, 11, 11, 11, DateTimeKind.Unspecified);

        var data = new JsonObject { ["Items"] = new JsonArray { item0, item1, item2, "/Date(1309421746929+0000)/" } };

        var serializer = new JsonSerializer();
        var response   = new RestResponse { Content = data.ToString() };
        var p          = serializer.Deserialize<GenericWithList<DateTime>>(response);

        Assert.NotEqual(item0.Kind, p.Items[0].Kind);
        Assert.Equal(item1.Kind, p.Items[1].Kind);
        Assert.Equal(DateTimeKind.Utc, p.Items[2].Kind);
        Assert.Equal(DateTimeKind.Utc, p.Items[3].Kind);
    }

    [Fact]
    public void Can_Deserialize_DateTimeOffset() {
        var payload = GetPayLoad<DateTimeTestStructure>("datetimes.json");

        Assert.Equal(
            new DateTime(2011, 6, 30, 8, 15, 46, 929, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm:ss.fff"),
            payload.DateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss.fff")
        );
    }

    [Fact]
    public void Can_Deserialize_NewDateTime() {
        var payload = GetPayLoad<NewDateTimeTestStructure>("newdatetimes.json");

        Assert.Equal(
            new DateTime(2011, 6, 30, 8, 15, 46, 929, DateTimeKind.Utc),
            payload.DateTime
        );
    }

    [Fact]
    public void Can_Deserialize_Negative_NewDateTime() {
        var payload = GetPayLoad<NewDateTimeTestStructure>("newdatetimes.json");

        Assert.Equal(
            new DateTime(1969, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc),
            payload.DateTimeNegative
        );
    }

    [Fact]
    public void Can_Deserialize_Decimal_With_Four_Zeros_After_Floating_Point() {
        const string json       = "{\"Value\":0.00005557}";
        var          response   = new RestResponse { Content = json };
        var          serializer = new JsonSerializer();
        var          result     = serializer.Deserialize<DecimalNumber>(response);

        Assert.Equal(.00005557m, result.Value);
    }

    [Fact]
    public void Can_Deserialize_Dictionary_of_Lists() {
        var doc        = File.ReadAllText(Path.Combine(CurrentPath, "SampleData", "jsondictionary.json"));
        var serializer = new JsonSerializer { RootElement = "response" };
        var output     = serializer.Deserialize<EmployeeTracker>(new RestResponse { Content = doc });

        Assert.NotEmpty(output.EmployeesMail);
        Assert.NotEmpty(output.EmployeesTime);
        Assert.NotEmpty(output.EmployeesPay);
    }

    [Fact]
    public void Can_Deserialize_Dictionary_with_Null() {
        var doc        = File.ReadAllText(Path.Combine(CurrentPath, "SampleData", "jsondictionary_null.json"));
        var serializer = new JsonSerializer { RootElement = "response" };

        IDictionary<string, object> output =
            serializer.Deserialize<Dictionary<string, object>>(new RestResponse { Content = doc });

        var dictionary = (IDictionary<string, object>)output["SomeDictionary"];
        Assert.Equal("abra", dictionary["NonNull"]);
        Assert.Null(dictionary["Null"]);
    }

    [Fact]
    public void Can_Deserialize_Dot_Field() {
        var data       = File.ReadAllText(Path.Combine(CurrentPath, "SampleData", "bearertoken.json"));
        var response   = new RestResponse { Content = data };
        var serializer = new JsonSerializer();
        var output     = serializer.Deserialize<BearerToken>(response);

        var expectedIssued =
            DateTimeOffset.ParseExact("Mon, 14 Oct 2013 06:53:32 GMT", "r", CultureInfo.InvariantCulture);

        var expectedExpires =
            DateTimeOffset.ParseExact("Mon, 28 Oct 2013 06:53:32 GMT", "r", CultureInfo.InvariantCulture);

        Assert.Equal("boQtj0SCGz2GFGz[...]", output.AccessToken);
        Assert.Equal("bearer", output.TokenType);
        Assert.Equal(1209599L, output.ExpiresIn);
        Assert.Equal("Alice", output.UserName);
        Assert.Equal(expectedIssued, output.Issued);
        Assert.Equal(expectedExpires, output.Expires);
    }

    [Fact]
    public void Can_Deserialize_Elements_to_Nullable_Values() {
        var serializer = new JsonSerializer();

        var output = serializer.Deserialize<NullableValues>(
            new RestResponse
                { Content = JsonData.CreateJsonWithoutEmptyValues }
        );

        Assert.NotNull(output.Id);
        Assert.NotNull(output.StartDate);
        Assert.NotNull(output.UniqueId);

        Assert.Equal(123, output.Id);
        Assert.NotNull(output.StartDate);

        Assert.Equal(
            new DateTime(2010, 2, 21, 9, 35, 00, DateTimeKind.Utc),
            output.StartDate.Value
        );
        Assert.Equal(new Guid(JsonData.GUID_STRING), output.UniqueId);
    }

    [Fact]
    public void Can_Deserialize_Empty_Elements_to_Nullable_Values() {
        var serializer = new JsonSerializer();

        var output =
            serializer.Deserialize<NullableValues>(new RestResponse { Content = JsonData.JsonWithEmptyValues });

        Assert.Null(output.Id);
        Assert.Null(output.StartDate);
        Assert.Null(output.UniqueId);
    }

    [Fact]
    public void Can_Deserialize_Exponential_Notation() {
        const string content    = "{ \"Value\": 4.8e-04 }";
        var          serializer = new JsonSerializer();
        var          output     = serializer.Deserialize<DecimalNumber>(new RestResponse { Content = content });
        var          expected   = decimal.Parse("4.8e-04", NumberStyles.Float, CultureInfo.InvariantCulture);

        Assert.NotNull(output);
        Assert.Equal(expected, output.Value);
    }

    [Fact]
    public void Can_Deserialize_From_Root_Element() {
        var doc        = File.ReadAllText(Path.Combine(CurrentPath, "SampleData", "sojson.json"));
        var serializer = new JsonSerializer { RootElement = "User" };
        var output     = serializer.Deserialize<SoUser>(new RestResponse { Content = doc });

        Assert.Equal("John Sheehan", output.DisplayName);
    }

    [Fact]
    public void Can_Deserialize_Generic_List_of_DateTime() {
        var item1 = new DateTime(2010, 2, 8, 11, 11, 11);
        var item2 = item1.AddSeconds(12345);
        var data  = new JsonObject { ["Items"] = new JsonArray { item1.ToString("u"), item2.ToString("u") } };

        var serializer = new JsonSerializer();
        var response   = new RestResponse { Content = data.ToString() };
        var p          = serializer.Deserialize<GenericWithList<DateTime>>(response);

        Assert.Equal(2, p.Items.Count);
        Assert.Equal(item1, p.Items[0]);
        Assert.Equal(item2, p.Items[1]);
    }

    [Fact]
    public void Can_Deserialize_Generic_Members() {
        var doc        = File.ReadAllText(Path.Combine(CurrentPath, "SampleData", "GenericWithList.json"));
        var serializer = new JsonSerializer();

        var output =
            serializer.Deserialize<Generic<GenericWithList<Foe>>>(new RestResponse { Content = doc });

        Assert.Equal("Foe sho", output.Data.Items[0].Nickname);
    }

    [Fact]
    public void Can_Deserialize_Guid_String_Fields() {
        var doc        = new JsonObject { ["Guid"] = JsonData.GUID_STRING };
        var serializer = new JsonSerializer();
        var response   = new RestResponse { Content = doc.ToString() };
        var p          = serializer.Deserialize<PersonForJson>(response);

        Assert.Equal(new Guid(JsonData.GUID_STRING), p.Guid);
    }

    [Fact]
    public void Can_Deserialize_IEnumerable_of_Simple_Types() {
        const string content    = "{\"numbers\":[1,2,3,4,5]}";
        var          serializer = new JsonSerializer { RootElement = "numbers" };
        var          output     = serializer.Deserialize<IEnumerable<int>>(new RestResponse { Content = content }).ToArray();

        Assert.NotEmpty(output);
        Assert.True(output.Length == 5);
    }

    [Fact]
    public void Can_Deserialize_IList_of_Simple_Types() {
        const string content    = "{\"numbers\":[1,2,3,4,5]}";
        var          serializer = new JsonSerializer { RootElement = "numbers" };
        var          output     = serializer.Deserialize<IList<int>>(new RestResponse { Content = content });

        Assert.NotEmpty(output);
        Assert.True(output.Count == 5);
    }

    [Fact]
    public void Can_Deserialize_Int_to_Bool() {
        var doc = new JsonObject { ["IsCool"] = 1 };

        var serializer = new JsonSerializer();
        var response   = new RestResponse { Content = doc.ToString() };
        var p          = serializer.Deserialize<PersonForJson>(response);

        Assert.True(p.IsCool);
    }

    [Fact]
    public void Can_Deserialize_Into_Struct() {
        const string content    = "{\"one\":\"oneOneOne\", \"two\":\"twoTwoTwo\", \"three\":3}";
        var          serializer = new JsonSerializer();
        var          output     = serializer.Deserialize<SimpleStruct>(new RestResponse { Content = content });

        Assert.Equal("oneOneOne", output.One);
        Assert.Equal("twoTwoTwo", output.Two);
        Assert.Equal(3, output.Three);
    }

    [Fact]
    public void Can_Deserialize_Iso_Json_Dates() {
        var doc        = JsonData.CreateIsoDateJson();
        var serializer = new JsonSerializer();
        var response   = new RestResponse { Content = doc };
        var bd         = serializer.Deserialize<Birthdate>(response);

        Assert.Equal(new DateTime(1910, 9, 25, 9, 30, 25, DateTimeKind.Utc), bd.Value);
    }

    [Fact]
    public void Can_Deserialize_Iso8601DateTimeLocal() {
        var payload = GetPayLoad<Iso8601DateTimeTestStructure>("iso8601datetimes.json");

        Assert.Equal(
            new DateTime(2012, 7, 19, 10, 23, 25, DateTimeKind.Utc),
            payload.DateTimeLocal
        );
    }

    [Fact]
    public void Can_Deserialize_Iso8601DateTimeWithOffset() {
        var payload = GetPayLoad<Iso8601DateTimeTestStructure>("iso8601datetimes.json");

        Assert.Equal(
            new DateTime(2012, 7, 19, 10, 23, 25, 544, DateTimeKind.Utc),
            payload.DateTimeWithOffset.ToUniversalTime()
        );
    }

    [Fact]
    public void Can_Deserialize_Iso8601DateTimeZulu() {
        var payload = GetPayLoad<Iso8601DateTimeTestStructure>("iso8601datetimes.json");

        Assert.Equal(
            new DateTime(2012, 7, 19, 10, 23, 25, 544, DateTimeKind.Utc),
            payload.DateTimeUtc.ToUniversalTime()
        );
    }

    [Fact]
    public void Can_Deserialize_Json_Using_DeserializeAs_Attribute() {
        const string content =
            "{\"sid\":\"asdasdasdasdasdasdasda\",\"friendlyName\":\"VeryNiceName\",\"oddballPropertyName\":\"blahblah\"}";
        var serializer = new JsonSerializer { RootElement = "users" };
        var output     = serializer.Deserialize<Oddball>(new RestResponse { Content = content });

        Assert.NotNull(output);
        Assert.Equal("blahblah", output.GoodPropertyName);
    }

    [Fact]
    public void Can_Deserialize_JsonNet_Dates() {
        var person = GetPayLoad<PersonForJson>("person.json");

        Assert.Equal(
            new DateTime(2011, 6, 30, 8, 15, 46, 929, DateTimeKind.Utc),
            person.StartDate
        );
    }

    [Fact]
    public void Can_Deserialize_List_of_Guid() {
        var id1  = new Guid("b0e5c11f-e944-478c-aadd-753b956d0c8c");
        var id2  = new Guid("809399fa-21c4-4dca-8dcd-34cb697fbca0");
        var data = new JsonObject { ["Ids"] = new JsonArray { id1, id2 } };

        var serializer = new JsonSerializer();
        var response   = new RestResponse { Content = data.ToString() };
        var p          = serializer.Deserialize<GuidList>(response);

        Assert.Equal(2, p.Ids.Count);
        Assert.Equal(id1, p.Ids[0]);
        Assert.Equal(id2, p.Ids[1]);
    }

    [Fact]
    public void Can_Deserialize_Lists_of_Simple_Types() {
        var doc        = File.ReadAllText(Path.Combine(CurrentPath, "SampleData", "jsonlists.json"));
        var serializer = new JsonSerializer();
        var output     = serializer.Deserialize<JsonLists>(new RestResponse { Content = doc });

        Assert.NotEmpty(output.Names);
        Assert.NotEmpty(output.Numbers);
    }

    [Fact]
    public void Can_Deserialize_Names_With_Double_Uppercase() {
        var doc        = JsonData.CreateJsonWithDoubleUppercase();
        var serializer = new JsonSerializer();
        var response   = new RestResponse { Content = doc };
        var p          = serializer.Deserialize<PersonForJson>(response);

        Assert.Equal(435, p.PersonId);
    }

    [Fact]
    public void Can_Deserialize_Names_With_Dashes_With_Default_Root() {
        var doc        = JsonData.CreateJsonWithDashes();
        var serializer = new JsonSerializer();
        var response   = new RestResponse { Content = doc };
        var p          = serializer.Deserialize<PersonForJson>(response);

        Assert.Equal("John Sheehan", p.Name);
        //Assert.Equal(new DateTime(2009, 9, 25, 0, 6, 1, DateTimeKind.Utc), p.StartDate);
        Assert.Equal(28, p.Age);
        Assert.Equal(long.MaxValue, p.BigNumber);
        Assert.Equal(99.9999m, p.Percent);
        Assert.False(p.IsCool);
        Assert.Equal(new Uri("http://example.com", UriKind.RelativeOrAbsolute), p.Url);
        Assert.Equal(new Uri("/foo/bar", UriKind.RelativeOrAbsolute), p.UrlPath);
        Assert.NotNull(p.Friends);
        Assert.Equal(10, p.Friends.Count);
        Assert.NotNull(p.BestFriend);
        Assert.Equal("The Fonz", p.BestFriend.Name);
        Assert.Equal(1952, p.BestFriend.Since);
        Assert.NotEmpty(p.Foes);
        Assert.Equal("Foe 1", p.Foes["dict1"].Nickname);
        Assert.Equal("Foe 2", p.Foes["dict2"].Nickname);
    }

    [Fact]
    public void Can_Deserialize_Names_With_Dashes_With_Default_Root_Alternative_Culture() {
        using (new CultureChange(AlternativeCulture)) Can_Deserialize_Names_With_Dashes_With_Default_Root();
    }

    [Fact]
    public void Can_Deserialize_Names_With_Underscore_Prefix() {
        var data       = File.ReadAllText(Path.Combine(CurrentPath, "SampleData", "underscore_prefix.json"));
        var response   = new RestResponse { Content       = data };
        var serializer = new JsonSerializer { RootElement = "User" };
        var output     = serializer.Deserialize<SoUser>(response);

        Assert.Equal("John Sheehan", output.DisplayName);
        Assert.Equal(1786, output.Id);
    }

    [Fact]
    public void Can_Deserialize_Names_With_Underscores_With_Default_Root() {
        var doc        = JsonData.CreateJsonWithUnderscores();
        var serializer = new JsonSerializer();
        var response   = new RestResponse { Content = doc };
        var p          = serializer.Deserialize<PersonForJson>(response);

        Assert.Equal("John Sheehan", p.Name);
        Assert.Equal(new DateTime(2009, 9, 25, 0, 6, 1), p.StartDate);
        Assert.Equal(28, p.Age);
        Assert.Equal(long.MaxValue, p.BigNumber);
        Assert.Equal(99.9999m, p.Percent);
        Assert.False(p.IsCool);
        Assert.Equal(new Uri("http://example.com", UriKind.RelativeOrAbsolute), p.Url);
        Assert.Equal(new Uri("/foo/bar", UriKind.RelativeOrAbsolute), p.UrlPath);
        Assert.NotNull(p.Friends);
        Assert.Equal(10, p.Friends.Count);
        Assert.NotNull(p.BestFriend);
        Assert.Equal("The Fonz", p.BestFriend.Name);
        Assert.Equal(1952, p.BestFriend.Since);
        Assert.NotEmpty(p.Foes);
        Assert.Equal("Foe 1", p.Foes["dict1"].Nickname);
        Assert.Equal("Foe 2", p.Foes["dict2"].Nickname);
    }

    [Fact]
    public void Can_Deserialize_Names_With_Underscores_With_Default_Root_Alternative_Culture() {
        using (new CultureChange(AlternativeCulture)) Can_Deserialize_Names_With_Underscores_With_Default_Root();
    }

    [Fact]
    public void Can_Deserialize_Null_Elements_to_Nullable_Values() {
        var serializer = new JsonSerializer();

        var output =
            serializer.Deserialize<NullableValues>(new RestResponse { Content = JsonData.JsonWithNullValues });

        Assert.Null(output.Id);
        Assert.Null(output.StartDate);
        Assert.Null(output.UniqueId);
    }

    [Fact]
    public void Can_Deserialize_Nullable_DateTime_With_Null() {
        var payload = GetPayLoad<DateTimeTestStructure>("datetimes.json");

        Assert.Null(payload.NullableDateTimeWithNull);
    }

    [Fact]
    public void Can_Deserialize_Nullable_DateTime_With_Value() {
        var payload = GetPayLoad<DateTimeTestStructure>("datetimes.json");

        Assert.NotNull(payload.NullableDateTimeWithValue);

        Assert.Equal(
            new DateTime(2011, 6, 30, 8, 15, 46, 929, DateTimeKind.Utc),
            payload.NullableDateTimeWithValue.Value
        );
    }

    [Fact]
    public void Can_Deserialize_Nullable_DateTimeOffset_With_Null() {
        var payload = GetPayLoad<DateTimeTestStructure>("datetimes.json");

        Assert.Null(payload.NullableDateTimeOffsetWithNull);
    }

    [Fact]
    public void Can_Deserialize_Nullable_DateTimeOffset_With_Value() {
        var payload = GetPayLoad<DateTimeTestStructure>("datetimes.json");

        Assert.NotNull(payload.NullableDateTimeOffsetWithValue);

        Assert.Equal(
            new DateTime(2011, 6, 30, 8, 15, 46, 929, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm:ss.fff"),
            payload.NullableDateTimeOffsetWithValue.Value.ToString("yyyy-MM-dd HH:mm:ss.fff")
        );
    }

    [Fact]
    public void Can_Deserialize_Object_Type_Property_With_Primitive_Vale() {
        var payload = GetPayLoad<ObjectProperties>("objectproperty.json");

        Assert.Equal(42L, payload.ObjectProperty);
    }

    [Fact]
    public void Can_Deserialize_Plain_Values() {
        const string json       = "\"c02bdd1e-cce3-4b9c-8473-165e6e93b92a\"";
        var          response   = new RestResponse { Content = json };
        var          serializer = new JsonSerializer();
        var          result     = serializer.Deserialize<Guid>(response);

        Assert.Equal(result, new Guid("c02bdd1e-cce3-4b9c-8473-165e6e93b92a"));
    }

    [Fact]
    public void Can_Deserialize_Quoted_Primitive() {
        var doc = new JsonObject { ["Age"] = "28" };

        var serializer = new JsonSerializer();
        var response   = new RestResponse { Content = doc.ToString() };
        var p          = serializer.Deserialize<PersonForJson>(response);

        Assert.Equal(28, p.Age);
    }

    [Fact]
    public void Can_Deserialize_Root_Json_Array_To_Inherited_List() {
        var data       = File.ReadAllText(Path.Combine(CurrentPath, "SampleData", "jsonarray.json"));
        var response   = new RestResponse { Content = data };
        var serializer = new JsonSerializer();
        var output     = serializer.Deserialize<StatusList>(response);

        Assert.Equal(4, output.Count);
    }

    [Fact]
    public void Can_Deserialize_Root_Json_Array_To_List() {
        var data       = File.ReadAllText(Path.Combine(CurrentPath, "SampleData", "jsonarray.json"));
        var response   = new RestResponse { Content = data };
        var serializer = new JsonSerializer();
        var output     = serializer.Deserialize<List<status>>(response);

        Assert.Equal(4, output.Count);
    }

    [Fact]
    public void Can_Deserialize_Select_Tokens() {
        var data       = File.ReadAllText(Path.Combine(CurrentPath, "SampleData", "jsonarray.json"));
        var response   = new RestResponse { Content = data };
        var serializer = new JsonSerializer();
        var output     = serializer.Deserialize<StatusComplexList>(response);

        Assert.Equal(4, output.Count);
    }

    [Fact]
    public void Can_Deserialize_Simple_Generic_List_Given_Item_Without_Array() {
        const string content    = "{\"users\":\"johnsheehan\"}";
        var          serializer = new JsonSerializer { RootElement = "users" };
        var          output     = serializer.Deserialize<List<string>>(new RestResponse { Content = content });

        Assert.True(output.SequenceEqual(new[] { "johnsheehan" }));
    }

    [Fact]
    public void Can_Deserialize_Simple_Generic_List_Given_Toplevel_Item_Without_Array() {
        const string content    = "\"johnsheehan\"";
        var          serializer = new JsonSerializer();
        var          output     = serializer.Deserialize<List<string>>(new RestResponse { Content = content });

        Assert.True(output.SequenceEqual(new[] { "johnsheehan" }));
    }

    [Fact]
    public void Can_Deserialize_Simple_Generic_List_of_Simple_Types() {
        const string content    = "{\"users\":[\"johnsheehan\",\"jagregory\",\"drusellers\",\"structuremap\"]}";
        var          serializer = new JsonSerializer { RootElement = "users" };
        var          output     = serializer.Deserialize<List<string>>(new RestResponse { Content = content });

        Assert.NotEmpty(output);
    }

    [Fact]
    public void Can_Deserialize_Simple_Generic_List_of_Simple_Types_With_Nulls() {
        const string content    = "{\"users\":[\"johnsheehan\",\"jagregory\",null,\"drusellers\",\"structuremap\"]}";
        var          serializer = new JsonSerializer { RootElement = "users" };
        var          output     = serializer.Deserialize<List<string>>(new RestResponse { Content = content });

        Assert.NotEmpty(output);
        Assert.Null(output[2]);
        Assert.Equal(5, output.Count);
    }

    [Fact]
    public void Can_Deserialize_TimeSpan() {
        var payload = GetPayLoad<TimeSpanTestStructure>("timespans.json");

        Assert.Equal(new TimeSpan(468006), payload.Tick);
        Assert.Equal(new TimeSpan(0, 0, 0, 0, 125), payload.Millisecond);
        Assert.Equal(new TimeSpan(0, 0, 8), payload.Second);
        Assert.Equal(new TimeSpan(0, 55, 2), payload.Minute);
        Assert.Equal(new TimeSpan(21, 30, 7), payload.Hour);
        Assert.Null(payload.NullableWithoutValue);
        Assert.NotNull(payload.NullableWithValue);
        Assert.Equal(new TimeSpan(21, 30, 7), payload.NullableWithValue.Value);
        Assert.Equal(new TimeSpan(0, 0, 10), payload.IsoSecond);
        Assert.Equal(new TimeSpan(0, 3, 23), payload.IsoMinute);
        Assert.Equal(new TimeSpan(5, 4, 9), payload.IsoHour);
        Assert.Equal(new TimeSpan(1, 19, 27, 13), payload.IsoDay);
        // 2 months + 4 days = 64 days
        Assert.Equal(new TimeSpan(64, 3, 14, 19), payload.IsoMonth);
        // 1 year = 365 days
        Assert.Equal(new TimeSpan(365, 9, 27, 48), payload.IsoYear);
    }

    [Fact]
    public void Can_Deserialize_To_Dictionary_Int_Object() {
        var doc        = File.ReadAllText(Path.Combine(CurrentPath, "SampleData", "jsondictionary_KeysType.json"));
        var serializer = new JsonSerializer();

        var output =
            serializer.Deserialize<Dictionary<int, object>>(new RestResponse { Content = doc });

        Assert.Equal(2, output.Keys.Count);

        var firstKeysVal = output.FirstOrDefault().Value;

        Assert.IsAssignableFrom<IDictionary>(firstKeysVal);
    }

    [Fact]
    public void Can_Deserialize_To_Dictionary_String_Object() {
        var doc        = File.ReadAllText(Path.Combine(CurrentPath, "SampleData", "jsondictionary.json"));
        var serializer = new JsonSerializer();

        var output =
            serializer.Deserialize<Dictionary<string, object>>(new RestResponse { Content = doc });

        Assert.Equal(3, output.Keys.Count);

        var firstKeysVal = output.FirstOrDefault().Value;

        Assert.IsAssignableFrom<IDictionary>(firstKeysVal);
    }

    [Fact]
    public void Can_Deserialize_To_Dictionary_String_String() {
        var serializer = new JsonSerializer();
        var response   = new RestResponse { Content = JsonData.JsonStringDictionary };
        var bd         = serializer.Deserialize<Dictionary<string, string>>(response);

        Assert.Equal("Thing1", bd["Thing1"]);
        Assert.Equal("Thing2", bd["Thing2"]);
        Assert.Equal("ThingRed", bd["ThingRed"]);
        Assert.Equal("ThingBlue", bd["ThingBlue"]);
    }

    [Fact]
    public void Can_Deserialize_To_Dictionary_String_String_With_Dynamic_Values() {
        var serializer = new JsonSerializer();
        var response   = new RestResponse { Content = JsonData.DynamicJsonStringDictionary };
        var bd         = serializer.Deserialize<Dictionary<string, string>>(response);

        Assert.Equal("[\"Value1\",\"Value2\"]", bd["Thing1"]);
        Assert.Equal("Thing2", bd["Thing2"]);
        Assert.Equal("{\"Name\":\"ThingRed\",\"Color\":\"Red\"}", bd["ThingRed"]);
        Assert.Equal("{\"Name\":\"ThingBlue\",\"Color\":\"Blue\"}", bd["ThingBlue"]);
    }

    [Fact]
    public void Can_Deserialize_Unix_Json_Dates() {
        var serializer = new JsonSerializer();
        var response   = new RestResponse { Content = JsonData.UnixDateJson };
        var bd         = serializer.Deserialize<Birthdate>(response);

        Assert.Equal(new DateTime(2011, 6, 30, 8, 15, 46, DateTimeKind.Utc), bd.Value);
    }

    [Fact]
    public void Can_Deserialize_Unix_Json_Millisecond_Dates() {
        var serializer = new JsonSerializer();
        var response   = new RestResponse { Content = JsonData.UnixDateMillisecondsJson };
        var bd         = serializer.Deserialize<Birthdate>(response);

        Assert.Equal(new DateTime(2011, 6, 30, 8, 15, 46, DateTimeKind.Utc), bd.Value);
    }

    [Fact]
    public void Can_Deserialize_Various_Enum_Types() {
        var data       = File.ReadAllText(Path.Combine(CurrentPath, "SampleData", "jsonenumtypes.json"));
        var response   = new RestResponse { Content = data };
        var serializer = new JsonSerializer();
        var output     = serializer.Deserialize<JsonEnumTypesTestStructure>(response);

        Assert.Equal(ByteEnum.EnumMin, output.ByteEnumType);
        Assert.Equal(SByteEnum.EnumMin, output.SByteEnumType);
        Assert.Equal(ShortEnum.EnumMin, output.ShortEnumType);
        Assert.Equal(UShortEnum.EnumMin, output.UShortEnumType);
        Assert.Equal(IntEnum.EnumMin, output.IntEnumType);
        Assert.Equal(UIntEnum.EnumMin, output.UIntEnumType);
        Assert.Equal(LongEnum.EnumMin, output.LongEnumType);
        Assert.Equal(ULongEnum.EnumMin, output.ULongEnumType);
    }

    [Fact]
    public void Can_Deserialize_Various_Enum_Values() {
        var data       = File.ReadAllText(Path.Combine(CurrentPath, "SampleData", "jsonenums.json"));
        var response   = new RestResponse { Content = data };
        var serializer = new JsonSerializer();
        var output     = serializer.Deserialize<JsonEnumsTestStructure>(response);

        Assert.Equal(Disposition.Friendly, output.Upper);
        Assert.Equal(Disposition.Friendly, output.Lower);
        Assert.Equal(Disposition.SoSo, output.CamelCased);
        Assert.Equal(Disposition.SoSo, output.Underscores);
        Assert.Equal(Disposition.SoSo, output.LowerUnderscores);
        Assert.Equal(Disposition.SoSo, output.Dashes);
        Assert.Equal(Disposition.SoSo, output.LowerDashes);
        Assert.Equal(Disposition.SoSo, output.Integer);
    }

    [Fact]
    public void Can_Deserialize_With_Default_Root() {
        var doc        = JsonData.CreateJson();
        var serializer = new JsonSerializer();
        var response   = new RestResponse { Content = doc };
        var p          = serializer.Deserialize<PersonForJson>(response);

        Assert.Equal("John Sheehan", p.Name);
        Assert.Equal(new DateTime(2009, 9, 25, 0, 6, 1, DateTimeKind.Utc), p.StartDate);
        Assert.Equal(28, p.Age);
        Assert.Equal(long.MaxValue, p.BigNumber);
        Assert.Equal(99.9999m, p.Percent);
        Assert.False(p.IsCool);
        Assert.Equal(new Uri("http://example.com", UriKind.RelativeOrAbsolute), p.Url);
        Assert.Equal(new Uri("/foo/bar", UriKind.RelativeOrAbsolute), p.UrlPath);
        Assert.Equal(Guid.Empty, p.EmptyGuid);
        Assert.Equal(new Guid(JsonData.GUID_STRING), p.Guid);
        Assert.Equal(Order.Third, p.Order);
        Assert.Equal(Disposition.SoSo, p.Disposition);
        Assert.NotNull(p.Friends);
        Assert.Equal(10, p.Friends.Count);
        Assert.NotNull(p.BestFriend);
        Assert.Equal("The Fonz", p.BestFriend.Name);
        Assert.Equal(1952, p.BestFriend.Since);
        Assert.NotEmpty(p.Foes);
        Assert.Equal("Foe 1", p.Foes["dict1"].Nickname);
        Assert.Equal("Foe 2", p.Foes["dict2"].Nickname);
    }

    [Fact]
    public void Can_Deserialize_With_Default_Root_Alternative_Culture() {
        using (new CultureChange(AlternativeCulture)) Can_Deserialize_With_Default_Root();
    }

    [Fact]
    public void Deserialization_Of_Undefined_Int_Value_Returns_Enum_Default() {
        const string data = @"{ ""Integer"" : 1024 }";

        var response   = new RestResponse { Content = data };
        var serializer = new JsonSerializer();
        var result     = serializer.Deserialize<JsonEnumsTestStructure>(response);

        Assert.Equal(Disposition.Friendly, result.Integer);
    }

    [Fact]
    public void Ignore_Protected_Property_That_Exists_In_Data() {
        var doc        = JsonData.CreateJson();
        var serializer = new JsonSerializer();
        var response   = new RestResponse { Content = doc };
        var p          = serializer.Deserialize<PersonForJson>(response);

        Assert.Null(p.IgnoreProxy);
    }

    [Fact]
    public void Ignore_ReadOnly_Property_That_Exists_In_Data() {
        var doc        = JsonData.CreateJson();
        var response   = new RestResponse { Content = doc };
        var serializer = new JsonSerializer();
        var p          = serializer.Deserialize<PersonForJson>(response);

        Assert.Null(p.ReadOnlyProxy);
    }

    [Fact]
    public void Serialize_Json_Does_Not_Double_Escape() {
        var preformattedString = "{ \"name\" : \"value\" }";
        var expectedSlashCount = preformattedString.Count(x => x == '\\');

        var serializer       = new JsonSerializer();
        var result           = serializer.Serialize(preformattedString);
        var actualSlashCount = result.Count(x => x == '\\');

        Assert.Equal(preformattedString, result);
        Assert.Equal(expectedSlashCount, actualSlashCount);
    }

    [Fact]
    public void Serialize_Json_Returns_Same_Json() {
        var preformattedString = "{ \"name\" : \"value\" } ";

        var serializer = new JsonSerializer();
        var result     = serializer.Serialize(preformattedString);

        Assert.Equal(preformattedString, result);
    }

    [Fact]
    public void Serialize_Json_Returns_Same_Json_Array() {
        var preformattedString = "[{ \"name\" : \"value\" }]";

        var serializer = new JsonSerializer();
        var result     = serializer.Serialize(preformattedString);

        Assert.Equal(preformattedString, result);
    }
}