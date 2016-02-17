#region License

//   Copyright 2010 John Sheehan
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using RestSharp.Deserializers;
using RestSharp.Tests.SampleClasses;

namespace RestSharp.Tests
{
    [TestFixture]
    public class JsonTests
    {
        private const string ALTERNATIVE_CULTURE = "pt-PT";

        private const string GUID_STRING = "AC1FC4BC-087A-4242-B8EE-C53EBE9887A5";

        [Test]
        public void Can_Deserialize_Exponential_Notation()
        {
            const string content = "{ \"Value\": 4.8e-04 }";
            JsonDeserializer json = new JsonDeserializer();
            DecimalNumber output = json.Deserialize<DecimalNumber>(new RestResponse { Content = content });
            decimal expected = decimal.Parse("4.8e-04", NumberStyles.Float, CultureInfo.InvariantCulture);

            Assert.NotNull(output);
            Assert.AreEqual(expected, output.Value);
        }

        [Test]
        public void Can_Deserialize_Into_Struct()
        {
            const string content = "{\"one\":\"oneOneOne\", \"two\":\"twoTwoTwo\", \"three\":3}";
            JsonDeserializer json = new JsonDeserializer();
            SimpleStruct output = json.Deserialize<SimpleStruct>(new RestResponse { Content = content });

            Assert.NotNull(output);
            Assert.AreEqual("oneOneOne", output.One);
            Assert.AreEqual("twoTwoTwo", output.Two);
            Assert.AreEqual(3, output.Three);
        }

        [Test]
        public void Can_Deserialize_Select_Tokens()
        {
            string data = File.ReadAllText(Path.Combine("SampleData", "jsonarray.txt"));
            RestResponse response = new RestResponse { Content = data };
            JsonDeserializer json = new JsonDeserializer();
            StatusComplexList output = json.Deserialize<StatusComplexList>(response);

            Assert.AreEqual(4, output.Count);
        }

        [Test]
        public void Can_Deserialize_4sq_Json_With_Root_Element_Specified()
        {
            string doc = File.ReadAllText(Path.Combine("SampleData", "4sq.txt"));
            JsonDeserializer json = new JsonDeserializer { RootElement = "response" };
            VenuesResponse output = json.Deserialize<VenuesResponse>(new RestResponse { Content = doc });

            Assert.IsNotEmpty(output.Groups);
        }

        [Test]
        public void Can_Deserialize_IEnumerable_of_Simple_Types()
        {
            const string content = "{\"numbers\":[1,2,3,4,5]}";
            JsonDeserializer json = new JsonDeserializer { RootElement = "numbers" };
            var output = json.Deserialize<IEnumerable<int>>(new RestResponse { Content = content });

            Assert.IsNotEmpty(output);
            Assert.IsTrue(output.Count() == 5);
        }

        [Test]
        public void Can_Deserialize_Lists_of_Simple_Types()
        {
            string doc = File.ReadAllText(Path.Combine("SampleData", "jsonlists.txt"));
            JsonDeserializer json = new JsonDeserializer();
            JsonLists output = json.Deserialize<JsonLists>(new RestResponse { Content = doc });

            Assert.IsNotEmpty(output.Names);
            Assert.IsNotEmpty(output.Numbers);
        }

        [Test]
        public void Can_Deserialize_Simple_Generic_List_of_Simple_Types()
        {
            const string content = "{\"users\":[\"johnsheehan\",\"jagregory\",\"drusellers\",\"structuremap\"]}";
            JsonDeserializer json = new JsonDeserializer { RootElement = "users" };
            List<string> output = json.Deserialize<List<string>>(new RestResponse { Content = content });

            Assert.IsNotEmpty(output);
        }

        [Test]
        public void Can_Deserialize_Simple_Generic_List_of_Simple_Types_With_Nulls()
        {
            const string content = "{\"users\":[\"johnsheehan\",\"jagregory\",null,\"drusellers\",\"structuremap\"]}";
            JsonDeserializer json = new JsonDeserializer { RootElement = "users" };
            List<string> output = json.Deserialize<List<string>>(new RestResponse { Content = content });

            Assert.IsNotEmpty(output);
            Assert.AreEqual(null, output[2]);
            Assert.AreEqual(5, output.Count);
        }

        [Test]
        public void Can_Deserialize_Simple_Generic_List_Given_Item_Without_Array()
        {
            const string content = "{\"users\":\"johnsheehan\"}";
            JsonDeserializer json = new JsonDeserializer { RootElement = "users" };
            List<string> output = json.Deserialize<List<string>>(new RestResponse { Content = content });

            Assert.True(output.SequenceEqual(new[] { "johnsheehan" }));
        }

        [Test]
        public void Can_Deserialize_Simple_Generic_List_Given_Toplevel_Item_Without_Array()
        {
            const string content = "\"johnsheehan\"";
            JsonDeserializer json = new JsonDeserializer();
            List<string> output = json.Deserialize<List<string>>(new RestResponse { Content = content });

            Assert.True(output.SequenceEqual(new[] { "johnsheehan" }));
        }

        [Test]
        public void Can_Deserialize_From_Root_Element()
        {
            string doc = File.ReadAllText(Path.Combine("SampleData", "sojson.txt"));
            JsonDeserializer json = new JsonDeserializer { RootElement = "User" };
            SoUser output = json.Deserialize<SoUser>(new RestResponse { Content = doc });

            Assert.AreEqual("John Sheehan", output.DisplayName);
        }

        [Test]
        public void Can_Deserialize_To_Dictionary_String_Object()
        {
            string doc = File.ReadAllText(Path.Combine("SampleData", "jsondictionary.txt"));
            JsonDeserializer json = new JsonDeserializer();
            Dictionary<string, object> output =
                json.Deserialize<Dictionary<string, object>>(new RestResponse { Content = doc });

            Assert.AreEqual(output.Keys.Count, 3);

            object firstKeysVal = output.FirstOrDefault().Value;

            Assert.IsInstanceOf<IDictionary>(firstKeysVal);
        }

        [Test]
        public void Can_Deserialize_To_Dictionary_Int_Object()
        {
            string doc = File.ReadAllText(Path.Combine("SampleData", "jsondictionary_KeysType.txt"));
            JsonDeserializer json = new JsonDeserializer();
            Dictionary<int, object> output =
                json.Deserialize<Dictionary<int, object>>(new RestResponse { Content = doc });

            Assert.AreEqual(output.Keys.Count, 2);

            object firstKeysVal = output.FirstOrDefault().Value;

            Assert.IsInstanceOf<IDictionary>(firstKeysVal);
        }

        [Test]
        public void Can_Deserialize_Generic_Members()
        {
            string doc = File.ReadAllText(Path.Combine("SampleData", "GenericWithList.txt"));
            JsonDeserializer json = new JsonDeserializer();
            Generic<GenericWithList<Foe>> output =
                json.Deserialize<Generic<GenericWithList<Foe>>>(new RestResponse { Content = doc });

            Assert.AreEqual("Foe sho", output.Data.Items[0].Nickname);
        }

        [Test]
        public void Can_Deserialize_List_of_Guid()
        {
            Guid id1 = new Guid("b0e5c11f-e944-478c-aadd-753b956d0c8c");
            Guid id2 = new Guid("809399fa-21c4-4dca-8dcd-34cb697fbca0");
            JsonObject data = new JsonObject();

            data["Ids"] = new JsonArray { id1, id2 };

            JsonDeserializer d = new JsonDeserializer();
            RestResponse response = new RestResponse { Content = data.ToString()  };
            GuidList p = d.Deserialize<GuidList>(response);

            Assert.AreEqual(2, p.Ids.Count);
            Assert.AreEqual(id1, p.Ids[0]);
            Assert.AreEqual(id2, p.Ids[1]);
        }

        [Test]
        public void Can_Deserialize_Generic_List_of_DateTime()
        {
            DateTime item1 = new DateTime(2010, 2, 8, 11, 11, 11);
            DateTime item2 = item1.AddSeconds(12345);
            JsonObject data = new JsonObject();

            data["Items"] = new JsonArray
                            {
                                item1.ToString("u"),
                                item2.ToString("u")
                            };

            JsonDeserializer d = new JsonDeserializer();
            RestResponse response = new RestResponse { Content = data.ToString() };
            GenericWithList<DateTime> p = d.Deserialize<GenericWithList<DateTime>>(response);

            Assert.AreEqual(2, p.Items.Count);
            Assert.AreEqual(item1, p.Items[0]);
            Assert.AreEqual(item2, p.Items[1]);
        }

        [Test]
        public void Can_Deserialize_DateTime_With_DateTimeStyles()
        {
            DateTime item0 = new DateTime(2010, 2, 8, 11, 11, 11, DateTimeKind.Local);
            DateTime item1 = new DateTime(2011, 2, 8, 11, 11, 11, DateTimeKind.Utc);
            DateTime item2 = new DateTime(2012, 2, 8, 11, 11, 11, DateTimeKind.Unspecified);
            JsonObject data = new JsonObject();

            data["Items"] = new JsonArray
                            {
                                item0.ToString(),
                                item1.ToString(),
                                item2.ToString(),
                                "/Date(1309421746929+0000)/"
                            };

            JsonDeserializer d = new JsonDeserializer();
            RestResponse response = new RestResponse { Content = data.ToString() };
            GenericWithList<DateTime> p = d.Deserialize<GenericWithList<DateTime>>(response);

            Assert.AreNotEqual(item0.Kind, p.Items[0].Kind);
            Assert.AreEqual(item1.Kind, p.Items[1].Kind);
            Assert.AreEqual(DateTimeKind.Utc, p.Items[2].Kind);
            Assert.AreEqual(DateTimeKind.Utc, p.Items[3].Kind);
        }

        [Test]
        public void Can_Deserialize_Null_Elements_to_Nullable_Values()
        {
            string doc = CreateJsonWithNullValues();
            JsonDeserializer json = new JsonDeserializer();
            NullableValues output = json.Deserialize<NullableValues>(new RestResponse { Content = doc });

            Assert.Null(output.Id);
            Assert.Null(output.StartDate);
            Assert.Null(output.UniqueId);
        }

        [Test]
        public void Can_Deserialize_Empty_Elements_to_Nullable_Values()
        {
            string doc = CreateJsonWithEmptyValues();
            JsonDeserializer json = new JsonDeserializer();
            NullableValues output = json.Deserialize<NullableValues>(new RestResponse { Content = doc });

            Assert.Null(output.Id);
            Assert.Null(output.StartDate);
            Assert.Null(output.UniqueId);
        }

        [Test]
        public void Can_Deserialize_Elements_to_Nullable_Values()
        {
            string doc = CreateJsonWithoutEmptyValues();
            JsonDeserializer json = new JsonDeserializer();
            NullableValues output = json.Deserialize<NullableValues>(new RestResponse { Content = doc });

            Assert.NotNull(output.Id);
            Assert.NotNull(output.StartDate);
            Assert.NotNull(output.UniqueId);

            Assert.AreEqual(123, output.Id);
            Assert.NotNull(output.StartDate);
            Assert.AreEqual(
                new DateTime(2010, 2, 21, 9, 35, 00, DateTimeKind.Utc),
                output.StartDate.Value);
            Assert.AreEqual(new Guid(GUID_STRING), output.UniqueId);
        }

        [Test]
        public void Can_Deserialize_Json_Using_DeserializeAs_Attribute()
        {
            const string content = "{\"sid\":\"asdasdasdasdasdasdasda\",\"friendlyName\":\"VeryNiceName\",\"oddballPropertyName\":\"blahblah\"}";
            JsonDeserializer json = new JsonDeserializer { RootElement = "users" };
            Oddball output = json.Deserialize<Oddball>(new RestResponse { Content = content });

            Assert.NotNull(output);
            Assert.AreEqual("blahblah", output.GoodPropertyName);
        }

        [Test]
        public void Can_Deserialize_Custom_Formatted_Date()
        {
            CultureInfo culture = CultureInfo.InvariantCulture;
            const string format = "dd yyyy MMM, hh:mm ss tt";
            DateTime date = new DateTime(2010, 2, 8, 11, 11, 11);
            var formatted = new { StartDate = date.ToString(format, culture) };
            string data = SimpleJson.SerializeObject(formatted);
            RestResponse response = new RestResponse { Content = data };
            JsonDeserializer json = new JsonDeserializer
                                    {
                                        DateFormat = format,
                                        Culture = culture
                                    };
            PersonForJson output = json.Deserialize<PersonForJson>(response);

            Assert.AreEqual(date, output.StartDate);
        }

        [Test]
        public void Can_Deserialize_Root_Json_Array_To_List()
        {
            string data = File.ReadAllText(Path.Combine("SampleData", "jsonarray.txt"));
            RestResponse response = new RestResponse { Content = data };
            JsonDeserializer json = new JsonDeserializer();
            List<status> output = json.Deserialize<List<status>>(response);

            Assert.AreEqual(4, output.Count);
        }

        [Test]
        public void Can_Deserialize_Root_Json_Array_To_Inherited_List()
        {
            string data = File.ReadAllText(Path.Combine("SampleData", "jsonarray.txt"));
            RestResponse response = new RestResponse { Content = data };
            JsonDeserializer json = new JsonDeserializer();
            StatusList output = json.Deserialize<StatusList>(response);

            Assert.AreEqual(4, output.Count);
        }

        [Test]
        public void Can_Deserialize_Various_Enum_Values()
        {
            string data = File.ReadAllText(Path.Combine("SampleData", "jsonenums.txt"));
            RestResponse response = new RestResponse { Content = data };
            JsonDeserializer json = new JsonDeserializer();
            JsonEnumsTestStructure output = json.Deserialize<JsonEnumsTestStructure>(response);

            Assert.AreEqual(Disposition.Friendly, output.Upper);
            Assert.AreEqual(Disposition.Friendly, output.Lower);
            Assert.AreEqual(Disposition.SoSo, output.CamelCased);
            Assert.AreEqual(Disposition.SoSo, output.Underscores);
            Assert.AreEqual(Disposition.SoSo, output.LowerUnderscores);
            Assert.AreEqual(Disposition.SoSo, output.Dashes);
            Assert.AreEqual(Disposition.SoSo, output.LowerDashes);
            Assert.AreEqual(Disposition.SoSo, output.Integer);
        }

        [Test]
        public void Can_Deserialize_Various_Enum_Types()
        {
            string data = File.ReadAllText(Path.Combine("SampleData", "jsonenumtypes.txt"));
            RestResponse response = new RestResponse { Content = data };
            JsonDeserializer json = new JsonDeserializer();
            JsonEnumTypesTestStructure output = json.Deserialize<JsonEnumTypesTestStructure>(response);

            Assert.AreEqual(ByteEnum.EnumMin, output.ByteEnumType);
            Assert.AreEqual(SByteEnum.EnumMin, output.SByteEnumType);
            Assert.AreEqual(ShortEnum.EnumMin, output.ShortEnumType);
            Assert.AreEqual(UShortEnum.EnumMin, output.UShortEnumType);
            Assert.AreEqual(IntEnum.EnumMin, output.IntEnumType);
            Assert.AreEqual(UIntEnum.EnumMin, output.UIntEnumType);
            Assert.AreEqual(LongEnum.EnumMin, output.LongEnumType);
            Assert.AreEqual(ULongEnum.EnumMin, output.ULongEnumType);
        }

        [Test]
        public void Deserialization_Of_Undefined_Int_Value_Returns_Enum_Default()
        {
            const string data = @"{ ""Integer"" : 1024 }";
            RestResponse response = new RestResponse { Content = data };
            JsonDeserializer json = new JsonDeserializer();
            JsonEnumsTestStructure result = json.Deserialize<JsonEnumsTestStructure>(response);

            Assert.AreEqual(Disposition.Friendly, result.Integer);
        }

        [Test]
        public void Can_Deserialize_Guid_String_Fields()
        {
            JsonObject doc = new JsonObject();

            doc["Guid"] = GUID_STRING;

            JsonDeserializer d = new JsonDeserializer();
            RestResponse response = new RestResponse { Content = doc.ToString() };
            PersonForJson p = d.Deserialize<PersonForJson>(response);

            Assert.AreEqual(new Guid(GUID_STRING), p.Guid);
        }

        [Test]
        public void Can_Deserialize_Quoted_Primitive()
        {
            JsonObject doc = new JsonObject();

            doc["Age"] = "28";

            JsonDeserializer d = new JsonDeserializer();
            RestResponse response = new RestResponse { Content = doc.ToString() };
            PersonForJson p = d.Deserialize<PersonForJson>(response);

            Assert.AreEqual(28, p.Age);
        }

        [Test]
        public void Can_Deserialize_Int_to_Bool()
        {
            JsonObject doc = new JsonObject();

            doc["IsCool"] = 1;

            JsonDeserializer d = new JsonDeserializer();
            RestResponse response = new RestResponse { Content = doc.ToString() };
            PersonForJson p = d.Deserialize<PersonForJson>(response);

            Assert.True(p.IsCool);
        }

        [Test]
        public void Can_Deserialize_With_Default_Root()
        {
            string doc = CreateJson();
            JsonDeserializer d = new JsonDeserializer();
            RestResponse response = new RestResponse { Content = doc };
            PersonForJson p = d.Deserialize<PersonForJson>(response);

            Assert.AreEqual("John Sheehan", p.Name);
            Assert.AreEqual(new DateTime(2009, 9, 25, 0, 6, 1, DateTimeKind.Utc), p.StartDate);
            Assert.AreEqual(28, p.Age);
            Assert.AreEqual(long.MaxValue, p.BigNumber);
            Assert.AreEqual(99.9999m, p.Percent);
            Assert.AreEqual(false, p.IsCool);
            Assert.AreEqual(new Uri("http://example.com", UriKind.RelativeOrAbsolute), p.Url);
            Assert.AreEqual(new Uri("/foo/bar", UriKind.RelativeOrAbsolute), p.UrlPath);
            Assert.AreEqual(Guid.Empty, p.EmptyGuid);
            Assert.AreEqual(new Guid(GUID_STRING), p.Guid);
            Assert.AreEqual(Order.Third, p.Order);
            Assert.AreEqual(Disposition.SoSo, p.Disposition);
            Assert.NotNull(p.Friends);
            Assert.AreEqual(10, p.Friends.Count);
            Assert.NotNull(p.BestFriend);
            Assert.AreEqual("The Fonz", p.BestFriend.Name);
            Assert.AreEqual(1952, p.BestFriend.Since);
            Assert.IsNotEmpty(p.Foes);
            Assert.AreEqual("Foe 1", p.Foes["dict1"].Nickname);
            Assert.AreEqual("Foe 2", p.Foes["dict2"].Nickname);
        }

        [Test]
        public void Can_Deserialize_With_Default_Root_Alternative_Culture()
        {
            using (new CultureChange(ALTERNATIVE_CULTURE))
            {
                this.Can_Deserialize_With_Default_Root();
            }
        }

        [Test]
        public void Can_Deserialize_Names_With_Underscore_Prefix()
        {
            string data = File.ReadAllText(Path.Combine("SampleData", "underscore_prefix.txt"));
            RestResponse response = new RestResponse { Content = data };
            JsonDeserializer json = new JsonDeserializer { RootElement = "User" };
            SoUser output = json.Deserialize<SoUser>(response);

            Assert.AreEqual("John Sheehan", output.DisplayName);
            Assert.AreEqual(1786, output.Id);
        }

        [Test]
        public void Can_Deserialize_Names_With_Underscores_With_Default_Root()
        {
            string doc = CreateJsonWithUnderscores();
            JsonDeserializer d = new JsonDeserializer();
            RestResponse response = new RestResponse { Content = doc };
            PersonForJson p = d.Deserialize<PersonForJson>(response);

            Assert.AreEqual("John Sheehan", p.Name);
            Assert.AreEqual(new DateTime(2009, 9, 25, 0, 6, 1), p.StartDate);
            Assert.AreEqual(28, p.Age);
            Assert.AreEqual(long.MaxValue, p.BigNumber);
            Assert.AreEqual(99.9999m, p.Percent);
            Assert.AreEqual(false, p.IsCool);
            Assert.AreEqual(new Uri("http://example.com", UriKind.RelativeOrAbsolute), p.Url);
            Assert.AreEqual(new Uri("/foo/bar", UriKind.RelativeOrAbsolute), p.UrlPath);
            Assert.NotNull(p.Friends);
            Assert.AreEqual(10, p.Friends.Count);
            Assert.NotNull(p.BestFriend);
            Assert.AreEqual("The Fonz", p.BestFriend.Name);
            Assert.AreEqual(1952, p.BestFriend.Since);
            Assert.IsNotEmpty(p.Foes);
            Assert.AreEqual("Foe 1", p.Foes["dict1"].Nickname);
            Assert.AreEqual("Foe 2", p.Foes["dict2"].Nickname);
        }

        [Test]
        public void Can_Deserialize_Names_With_Underscores_With_Default_Root_Alternative_Culture()
        {
            using (new CultureChange(ALTERNATIVE_CULTURE))
            {
                this.Can_Deserialize_Names_With_Underscores_With_Default_Root();
            }
        }

        [Test]
        public void Can_Deserialize_Names_With_Dashes_With_Default_Root()
        {
            string doc = CreateJsonWithDashes();
            JsonDeserializer d = new JsonDeserializer();
            RestResponse response = new RestResponse { Content = doc };
            PersonForJson p = d.Deserialize<PersonForJson>(response);

            Assert.AreEqual("John Sheehan", p.Name);
            //Assert.AreEqual(new DateTime(2009, 9, 25, 0, 6, 1, DateTimeKind.Utc), p.StartDate);
            Assert.AreEqual(28, p.Age);
            Assert.AreEqual(long.MaxValue, p.BigNumber);
            Assert.AreEqual(99.9999m, p.Percent);
            Assert.AreEqual(false, p.IsCool);
            Assert.AreEqual(new Uri("http://example.com", UriKind.RelativeOrAbsolute), p.Url);
            Assert.AreEqual(new Uri("/foo/bar", UriKind.RelativeOrAbsolute), p.UrlPath);
            Assert.NotNull(p.Friends);
            Assert.AreEqual(10, p.Friends.Count);
            Assert.NotNull(p.BestFriend);
            Assert.AreEqual("The Fonz", p.BestFriend.Name);
            Assert.AreEqual(1952, p.BestFriend.Since);
            Assert.IsNotEmpty(p.Foes);
            Assert.AreEqual("Foe 1", p.Foes["dict1"].Nickname);
            Assert.AreEqual("Foe 2", p.Foes["dict2"].Nickname);
        }

        [Test]
        public void Can_Deserialize_Names_With_Dashes_With_Default_Root_Alternative_Culture()
        {
            using (new CultureChange(ALTERNATIVE_CULTURE))
            {
                this.Can_Deserialize_Names_With_Dashes_With_Default_Root();
            }
        }

        [Test]
        public void Ignore_Protected_Property_That_Exists_In_Data()
        {
            string doc = CreateJson();
            JsonDeserializer d = new JsonDeserializer();
            RestResponse response = new RestResponse { Content = doc };
            PersonForJson p = d.Deserialize<PersonForJson>(response);

            Assert.Null(p.IgnoreProxy);
        }

        [Test]
        public void Ignore_ReadOnly_Property_That_Exists_In_Data()
        {
            string doc = CreateJson();
            RestResponse response = new RestResponse { Content = doc };
            JsonDeserializer d = new JsonDeserializer();
            PersonForJson p = d.Deserialize<PersonForJson>(response);

            Assert.Null(p.ReadOnlyProxy);
        }

        [Test]
        public void Can_Deserialize_TimeSpan()
        {
            TimeSpanTestStructure payload = GetPayLoad<TimeSpanTestStructure>("timespans.txt");

            Assert.AreEqual(new TimeSpan(468006), payload.Tick);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, 125), payload.Millisecond);
            Assert.AreEqual(new TimeSpan(0, 0, 8), payload.Second);
            Assert.AreEqual(new TimeSpan(0, 55, 2), payload.Minute);
            Assert.AreEqual(new TimeSpan(21, 30, 7), payload.Hour);
            Assert.Null(payload.NullableWithoutValue);
            Assert.NotNull(payload.NullableWithValue);
            Assert.AreEqual(new TimeSpan(21, 30, 7), payload.NullableWithValue.Value);
            Assert.AreEqual(new TimeSpan(0, 0, 10), payload.IsoSecond);
            Assert.AreEqual(new TimeSpan(0, 3, 23), payload.IsoMinute);
            Assert.AreEqual(new TimeSpan(5, 4, 9), payload.IsoHour);
            Assert.AreEqual(new TimeSpan(1, 19, 27, 13), payload.IsoDay);
            // 2 months + 4 days = 64 days
            Assert.AreEqual(new TimeSpan(64, 3, 14, 19), payload.IsoMonth);
            // 1 year = 365 days
            Assert.AreEqual(new TimeSpan(365, 9, 27, 48), payload.IsoYear);
        }

        [Test]
        public void Can_Deserialize_Iso_Json_Dates()
        {
            string doc = CreateIsoDateJson();
            JsonDeserializer d = new JsonDeserializer();
            RestResponse response = new RestResponse { Content = doc };
            Birthdate bd = d.Deserialize<Birthdate>(response);

            Assert.AreEqual(new DateTime(1910, 9, 25, 9, 30, 25, DateTimeKind.Utc), bd.Value);
        }

        [Test]
        public void Can_Deserialize_Unix_Json_Dates()
        {
            string doc = CreateUnixDateJson();
            JsonDeserializer d = new JsonDeserializer();
            RestResponse response = new RestResponse { Content = doc };
            Birthdate bd = d.Deserialize<Birthdate>(response);

            Assert.AreEqual(new DateTime(2011, 6, 30, 8, 15, 46, DateTimeKind.Utc), bd.Value);
        }

        [Test]
        public void Can_Deserialize_Unix_Json_Millisecond_Dates()
        {
            string doc = CreateUnixDateMillisecondsJson();
            JsonDeserializer d = new JsonDeserializer();
            RestResponse response = new RestResponse { Content = doc };
            Birthdate bd = d.Deserialize<Birthdate>(response);

            Assert.AreEqual(new DateTime(2011, 6, 30, 8, 15, 46, DateTimeKind.Utc), bd.Value);
        }

        [Test]
        public void Can_Deserialize_JsonNet_Dates()
        {
            PersonForJson person = GetPayLoad<PersonForJson>("person.json.txt");

            Assert.AreEqual(
                new DateTime(2011, 6, 30, 8, 15, 46, 929, DateTimeKind.Utc),
                person.StartDate);
        }

        [Test]
        public void Can_Deserialize_DateTime()
        {
            DateTimeTestStructure payload = GetPayLoad<DateTimeTestStructure>("datetimes.txt");

            Assert.AreEqual(
                new DateTime(2011, 6, 30, 8, 15, 46, 929, DateTimeKind.Utc),
                payload.DateTime);
        }

        [Test]
        public void Can_Deserialize_Nullable_DateTime_With_Value()
        {
            DateTimeTestStructure payload = GetPayLoad<DateTimeTestStructure>("datetimes.txt");

            Assert.NotNull(payload.NullableDateTimeWithValue);
            Assert.AreEqual(
                new DateTime(2011, 6, 30, 8, 15, 46, 929, DateTimeKind.Utc),
                payload.NullableDateTimeWithValue.Value);
        }

        [Test]
        public void Can_Deserialize_Nullable_DateTime_With_Null()
        {
            DateTimeTestStructure payload = GetPayLoad<DateTimeTestStructure>("datetimes.txt");

            Assert.Null(payload.NullableDateTimeWithNull);
        }

        [Test]
        public void Can_Deserialize_DateTimeOffset()
        {
            DateTimeTestStructure payload = GetPayLoad<DateTimeTestStructure>("datetimes.txt");

            Assert.AreEqual(
                new DateTime(2011, 6, 30, 8, 15, 46, 929, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm:ss.fff"),
                payload.DateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        }

        [Test]
        public void Can_Deserialize_Iso8601DateTimeLocal()
        {
            Iso8601DateTimeTestStructure payload = GetPayLoad<Iso8601DateTimeTestStructure>("iso8601datetimes.txt");

            Assert.AreEqual(
                new DateTime(2012, 7, 19, 10, 23, 25, DateTimeKind.Utc),
                payload.DateTimeLocal);
        }

        [Test]
        public void Can_Deserialize_Iso8601DateTimeZulu()
        {
            Iso8601DateTimeTestStructure payload = GetPayLoad<Iso8601DateTimeTestStructure>("iso8601datetimes.txt");

            Assert.AreEqual(
                new DateTime(2012, 7, 19, 10, 23, 25, 544, DateTimeKind.Utc),
                payload.DateTimeUtc.ToUniversalTime());
        }

        [Test]
        public void Can_Deserialize_Iso8601DateTimeWithOffset()
        {
            Iso8601DateTimeTestStructure payload = GetPayLoad<Iso8601DateTimeTestStructure>("iso8601datetimes.txt");

            Assert.AreEqual(
                new DateTime(2012, 7, 19, 10, 23, 25, 544, DateTimeKind.Utc),
                payload.DateTimeWithOffset.ToUniversalTime());
        }

        [Test]
        public void Can_Deserialize_Nullable_DateTimeOffset_With_Value()
        {
            DateTimeTestStructure payload = GetPayLoad<DateTimeTestStructure>("datetimes.txt");

            Assert.NotNull(payload.NullableDateTimeOffsetWithValue);
            Assert.AreEqual(
                new DateTime(2011, 6, 30, 8, 15, 46, 929, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm:ss.fff"),
                payload.NullableDateTimeOffsetWithValue.Value.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        }

        [Test]
        public void Can_Deserialize_Nullable_DateTimeOffset_With_Null()
        {
            DateTimeTestStructure payload = GetPayLoad<DateTimeTestStructure>("datetimes.txt");

            Assert.Null(payload.NullableDateTimeOffsetWithNull);
        }

        [Test]
        public void Can_Deserialize_To_Dictionary_String_String()
        {
            string doc = this.CreateJsonStringDictionary();
            JsonDeserializer d = new JsonDeserializer();
            RestResponse response = new RestResponse { Content = doc };
            Dictionary<string, string> bd = d.Deserialize<Dictionary<string, string>>(response);

            Assert.AreEqual(bd["Thing1"], "Thing1");
            Assert.AreEqual(bd["Thing2"], "Thing2");
            Assert.AreEqual(bd["ThingRed"], "ThingRed");
            Assert.AreEqual(bd["ThingBlue"], "ThingBlue");
        }

        [Test]
        public void Can_Deserialize_To_Dictionary_String_String_With_Dynamic_Values()
        {
            string doc = this.CreateDynamicJsonStringDictionary();
            JsonDeserializer d = new JsonDeserializer();
            RestResponse response = new RestResponse { Content = doc };
            Dictionary<string, string> bd = d.Deserialize<Dictionary<string, string>>(response);

            Assert.AreEqual("[\"Value1\",\"Value2\"]", bd["Thing1"]);
            Assert.AreEqual("Thing2", bd["Thing2"]);
            Assert.AreEqual("{\"Name\":\"ThingRed\",\"Color\":\"Red\"}", bd["ThingRed"]);
            Assert.AreEqual("{\"Name\":\"ThingBlue\",\"Color\":\"Blue\"}", bd["ThingBlue"]);
        }

        [Test]
        public void Can_Deserialize_Decimal_With_Four_Zeros_After_Floating_Point()
        {
            const string json = "{\"Value\":0.00005557}";
            RestResponse response = new RestResponse { Content = json };
            JsonDeserializer d = new JsonDeserializer();
            DecimalNumber result = d.Deserialize<DecimalNumber>(response);

            Assert.AreEqual(result.Value, .00005557m);
        }

        [Test]
        public void Can_Deserialize_Object_Type_Property_With_Primitive_Vale()
        {
            ObjectProperties payload = GetPayLoad<ObjectProperties>("objectproperty.txt");

            Assert.AreEqual(42L, payload.ObjectProperty);
        }

        [Test]
        public void Can_Deserialize_Dictionary_of_Lists()
        {
            string doc = File.ReadAllText(Path.Combine("SampleData", "jsondictionary.txt"));
            JsonDeserializer json = new JsonDeserializer { RootElement = "response" };
            EmployeeTracker output = json.Deserialize<EmployeeTracker>(new RestResponse { Content = doc });

            Assert.IsNotEmpty(output.EmployeesMail);
            Assert.IsNotEmpty(output.EmployeesTime);
            Assert.IsNotEmpty(output.EmployeesPay);
        }

        [Test]
        public void Can_Deserialize_Plain_Values()
        {
            const string json = "\"c02bdd1e-cce3-4b9c-8473-165e6e93b92a\"";
            RestResponse response = new RestResponse { Content = json };
            JsonDeserializer d = new JsonDeserializer();
            Guid result = d.Deserialize<Guid>(response);

            Assert.AreEqual(result, new Guid("c02bdd1e-cce3-4b9c-8473-165e6e93b92a"));
        }

        [Test]
        public void Can_Deserialize_Dictionary_with_Null()
        {
            string doc = File.ReadAllText(Path.Combine("SampleData", "jsondictionary_null.txt"));
            JsonDeserializer json = new JsonDeserializer { RootElement = "response" };
            IDictionary<string, object> output = json.Deserialize<Dictionary<string, object>>(new RestResponse { Content = doc });

            IDictionary<string, object> dictionary = (IDictionary<string, object>)output["SomeDictionary"];
            Assert.AreEqual("abra", dictionary["NonNull"]);
            Assert.IsNull(dictionary["Null"]);
        }

        private static string CreateJsonWithUnderscores()
        {
            JsonObject doc = new JsonObject();

            doc["name"] = "John Sheehan";
            doc["start_date"] = new DateTime(2009, 9, 25, 0, 6, 1, DateTimeKind.Utc);
            doc["age"] = 28;
            doc["percent"] = 99.9999m;
            doc["big_number"] = long.MaxValue;
            doc["is_cool"] = false;
            doc["ignore"] = "dummy";
            doc["read_only"] = "dummy";
            doc["url"] = "http://example.com";
            doc["url_path"] = "/foo/bar";
            doc["best_friend"] = new JsonObject
                                 {
                                     { "name", "The Fonz" },
                                     { "since", 1952 }
                                 };

            JsonArray friendsArray = new JsonArray();

            for (int i = 0; i < 10; i++)
            {
                friendsArray.Add(new JsonObject
                                 {
                                     { "name", "Friend" + i },
                                     { "since", DateTime.Now.Year - i }
                                 });
            }

            doc["friends"] = friendsArray;

            JsonObject foesArray = new JsonObject
                                   {
                                       { "dict1", new JsonObject { { "nickname", "Foe 1" } } },
                                       { "dict2", new JsonObject { { "nickname", "Foe 2" } } }
                                   };

            doc["foes"] = foesArray;

            return doc.ToString();
        }

        private static string CreateJsonWithDashes()
        {
            JsonObject doc = new JsonObject();

            doc["name"] = "John Sheehan";
            doc["start-date"] = new DateTime(2009, 9, 25, 0, 6, 1, DateTimeKind.Utc);
            doc["age"] = 28;
            doc["percent"] = 99.9999m;
            doc["big-number"] = long.MaxValue;
            doc["is-cool"] = false;
            doc["ignore"] = "dummy";
            doc["read-only"] = "dummy";
            doc["url"] = "http://example.com";
            doc["url-path"] = "/foo/bar";
            doc["best-friend"] = new JsonObject
                                 {
                                     { "name", "The Fonz" },
                                     { "since", 1952 }
                                 };

            JsonArray friendsArray = new JsonArray();

            for (int i = 0; i < 10; i++)
            {
                friendsArray.Add(new JsonObject
                                 {
                                     { "name", "Friend" + i },
                                     { "since", DateTime.Now.Year - i }
                                 });
            }

            doc["friends"] = friendsArray;

            JsonObject foesArray = new JsonObject
                                   {
                                       { "dict1", new JsonObject { { "nickname", "Foe 1" } } },
                                       { "dict2", new JsonObject { { "nickname", "Foe 2" } } }
                                   };

            doc["foes"] = foesArray;

            return doc.ToString();
        }

        private static string CreateIsoDateJson()
        {
            Birthdate bd = new Birthdate
                           {
                               Value = new DateTime(1910, 9, 25, 9, 30, 25, DateTimeKind.Utc)
                           };

            return SimpleJson.SerializeObject(bd);
        }

        private static string CreateUnixDateJson()
        {
            JsonObject doc = new JsonObject();

            doc["Value"] = 1309421746;

            return doc.ToString();
        }

        private static string CreateUnixDateMillisecondsJson()
        {
            JsonObject doc = new JsonObject();

            doc["Value"] = 1309421746000;

            return doc.ToString();
        }

        private static string CreateJson()
        {
            JsonObject doc = new JsonObject();

            doc["Name"] = "John Sheehan";
            doc["StartDate"] = new DateTime(2009, 9, 25, 0, 6, 1, DateTimeKind.Utc);
            doc["Age"] = 28;
            doc["Percent"] = 99.9999m;
            doc["BigNumber"] = long.MaxValue;
            doc["IsCool"] = false;
            doc["Ignore"] = "dummy";
            doc["ReadOnly"] = "dummy";
            doc["Url"] = "http://example.com";
            doc["UrlPath"] = "/foo/bar";
            doc["Order"] = "third";
            doc["Disposition"] = "so_so";
            doc["Guid"] = new Guid(GUID_STRING).ToString();
            doc["EmptyGuid"] = "";
            doc["BestFriend"] = new JsonObject
                                {
                                    { "Name", "The Fonz" },
                                    { "Since", 1952 }
                                };

            JsonArray friendsArray = new JsonArray();

            for (int i = 0; i < 10; i++)
            {
                friendsArray.Add(new JsonObject
                                 {
                                     { "Name", "Friend" + i },
                                     { "Since", DateTime.Now.Year - i }
                                 });
            }

            doc["Friends"] = friendsArray;

            JsonObject foesArray = new JsonObject
                                   {
                                       { "dict1", new JsonObject { { "nickname", "Foe 1" } } },
                                       { "dict2", new JsonObject { { "nickname", "Foe 2" } } }
                                   };

            doc["Foes"] = foesArray;

            return doc.ToString();
        }

        private static string CreateJsonWithNullValues()
        {
            JsonObject doc = new JsonObject();

            doc["Id"] = null;
            doc["StartDate"] = null;
            doc["UniqueId"] = null;

            return doc.ToString();
        }

        private static string CreateJsonWithEmptyValues()
        {
            JsonObject doc = new JsonObject();

            doc["Id"] = "";
            doc["StartDate"] = "";
            doc["UniqueId"] = "";

            return doc.ToString();
        }

        private static string CreateJsonWithoutEmptyValues()
        {
            JsonObject doc = new JsonObject();

            doc["Id"] = 123;
            doc["StartDate"] = new DateTime(2010, 2, 21, 9, 35, 00, DateTimeKind.Utc);
            doc["UniqueId"] = new Guid(GUID_STRING).ToString();

            return doc.ToString();
        }

        public string CreateJsonStringDictionary()
        {
            JsonObject doc = new JsonObject();

            doc["Thing1"] = "Thing1";
            doc["Thing2"] = "Thing2";
            doc["ThingRed"] = "ThingRed";
            doc["ThingBlue"] = "ThingBlue";

            return doc.ToString();
        }

        public string CreateDynamicJsonStringDictionary()
        {
            JsonObject doc = new JsonObject();

            doc["Thing1"] = new JsonArray
                            {
                                "Value1",
                                "Value2"
                            };
            doc["Thing2"] = "Thing2";
            doc["ThingRed"] = new JsonObject
                              {
                                  { "Name", "ThingRed" },
                                  { "Color", "Red" }
                              };
            doc["ThingBlue"] = new JsonObject
                               {
                                   { "Name", "ThingBlue" },
                                   { "Color", "Blue" }
                               };

            return doc.ToString();
        }

        private static T GetPayLoad<T>(string fileName)
        {
            string doc = File.ReadAllText(Path.Combine("SampleData", fileName));
            RestResponse response = new RestResponse { Content = doc };
            JsonDeserializer d = new JsonDeserializer();

            return d.Deserialize<T>(response);
        }
    }
}
