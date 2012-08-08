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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RestSharp.Deserializers;
using RestSharp.Tests.SampleClasses;
using Xunit;

namespace RestSharp.Tests
{
	public class JsonTests
	{
		private const string AlternativeCulture = "pt-PT";

		private const string GuidString = "AC1FC4BC-087A-4242-B8EE-C53EBE9887A5";

		[Fact]
		public void Can_Deserialize_4sq_Json_With_Root_Element_Specified()
		{
			var doc = File.ReadAllText(Path.Combine("SampleData", "4sq.txt"));

			var json = new JsonDeserializer();
			json.RootElement = "response";

			var output = json.Deserialize<VenuesResponse>(new RestResponse { Content = doc });

			Assert.NotEmpty(output.Groups);
		}

		[Fact]
		public void Can_Deserialize_Lists_of_Simple_Types()
		{
			var doc = File.ReadAllText(Path.Combine("SampleData", "jsonlists.txt"));
			var json = new JsonDeserializer ();

			var output = json.Deserialize<JsonLists> (new RestResponse { Content = doc });

			Assert.NotEmpty (output.Names);
			Assert.NotEmpty (output.Numbers);
		}

		[Fact]
		public void Can_Deserialize_Simple_Generic_List_of_Simple_Types()
		{
			const string content = "{\"users\":[\"johnsheehan\",\"jagregory\",\"drusellers\",\"structuremap\"]}";
			var json = new JsonDeserializer {RootElement = "users"};

			var output = json.Deserialize<List<string>>(new RestResponse {Content = content});

			Assert.NotEmpty(output);
		}

		[Fact]
		public void Can_Deserialize_Simple_Generic_List_of_Simple_Types_With_Nulls ()
		{
			const string content = "{\"users\":[\"johnsheehan\",\"jagregory\",null,\"drusellers\",\"structuremap\"]}";
			var json = new JsonDeserializer { RootElement = "users" };

			var output = json.Deserialize<List<string>> (new RestResponse { Content = content });

			Assert.NotEmpty (output);
			Assert.Equal (null, output[2]);
			Assert.Equal (5, output.Count);
		}

		[Fact]
		public void Can_Deserialize_Simple_Generic_List_Given_Item_Without_Array ()
		{
			const string content = "{\"users\":\"johnsheehan\"}";
			var json = new JsonDeserializer { RootElement = "users" };

			var output = json.Deserialize<List<string>> (new RestResponse { Content = content });

			Assert.True (output.SequenceEqual (new[] { "johnsheehan" }));
		}

		[Fact]
		public void Can_Deserialize_Simple_Generic_List_Given_Toplevel_Item_Without_Array ()
		{
			const string content = "\"johnsheehan\"";
			var json = new JsonDeserializer ();

			var output = json.Deserialize<List<string>> (new RestResponse { Content = content });

			Assert.True (output.SequenceEqual (new[] { "johnsheehan" }));
		}

		[Fact]
		public void Can_Deserialize_From_Root_Element()
		{
			var doc = File.ReadAllText(Path.Combine("SampleData", "sojson.txt"));

			var json = new JsonDeserializer();
			json.RootElement = "User";

			var output = json.Deserialize<SOUser>(new RestResponse { Content = doc });
			Assert.Equal("John Sheehan", output.DisplayName);
		}

		[Fact]
		public void Can_Deserialize_Generic_Members()
		{
			var doc = File.ReadAllText(Path.Combine("SampleData", "GenericWithList.txt"));
			var json = new JsonDeserializer();

			var output = json.Deserialize<Generic<GenericWithList<Foe>>>(new RestResponse { Content = doc });
			Assert.Equal("Foe sho", output.Data.Items[0].Nickname);
		}

		[Fact]
		public void Can_Deserialize_List_of_Guid()
		{
			Guid ID1 = new Guid("b0e5c11f-e944-478c-aadd-753b956d0c8c");
			Guid ID2 = new Guid("809399fa-21c4-4dca-8dcd-34cb697fbca0");
			var data = new JObject();
			data["Ids"] = new JArray(ID1, ID2);

			var d = new JsonDeserializer();
			var response = new RestResponse { Content = data.ToString() };
			var p = d.Deserialize<GuidList>(response);

			Assert.Equal(2, p.Ids.Count);
			Assert.Equal(ID1, p.Ids[0]);
			Assert.Equal(ID2, p.Ids[1]);
		}
		
		[Fact]
		public void Can_Deserialize_Generic_List_of_DateTime()
		{
			DateTime Item1 = new DateTime(2010, 2, 8, 11, 11, 11);
			DateTime Item2 = Item1.AddSeconds(12345);
			var data = new JObject();
			data["Items"] = new JArray(Item1.ToString("u"), Item2.ToString("u"));

			var d = new JsonDeserializer();
			var response = new RestResponse { Content = data.ToString() };
			var p = d.Deserialize<GenericWithList<DateTime>>(response);

			Assert.Equal(2, p.Items.Count);
			Assert.Equal(Item1, p.Items[0]);
			Assert.Equal(Item2, p.Items[1]);
		}

		[Fact]
		public void Can_Deserialize_Empty_Elements_to_Nullable_Values()
		{
			var doc = CreateJsonWithNullValues();

			var json = new JsonDeserializer();
			var output = json.Deserialize<NullableValues>(new RestResponse { Content = doc });

			Assert.Null(output.Id);
			Assert.Null(output.StartDate);
			Assert.Null(output.UniqueId);
		}

		[Fact]
		public void Can_Deserialize_Elements_to_Nullable_Values()
		{
			var doc = CreateJsonWithoutEmptyValues();

			var json = new JsonDeserializer();
			var output = json.Deserialize<NullableValues>(new RestResponse { Content = doc });

			Assert.NotNull(output.Id);
			Assert.NotNull(output.StartDate);
			Assert.NotNull(output.UniqueId);

			Assert.Equal(123, output.Id);
			Assert.NotNull(output.StartDate);
			Assert.Equal(
				new DateTime(2010, 2, 21, 9, 35, 00, DateTimeKind.Utc),
				output.StartDate.Value);
			Assert.Equal(new Guid(GuidString), output.UniqueId);
		}

		[Fact]
		public void Can_Deserialize_Custom_Formatted_Date()
		{
			 var culture = CultureInfo.InvariantCulture;
			var format = "dd yyyy MMM, hh:mm ss tt";
			var date = new DateTime(2010, 2, 8, 11, 11, 11);

			var formatted = new
			{
				StartDate = date.ToString(format, culture)
			};

			var data = JsonConvert.SerializeObject(formatted);
			var response = new RestResponse { Content = data };

			var json = new JsonDeserializer { DateFormat = format, Culture = culture };

			var output = json.Deserialize<PersonForJson>(response);

			Assert.Equal(date, output.StartDate);
		}

		[Fact]
		public void Can_Deserialize_Root_Json_Array_To_List()
		{
			var data = File.ReadAllText(Path.Combine("SampleData", "jsonarray.txt"));
			var response = new RestResponse { Content = data };
			var json = new JsonDeserializer();
			var output = json.Deserialize<List<status>>(response);
			Assert.Equal(4, output.Count);
		}

		[Fact]
		public void Can_Deserialize_Root_Json_Array_To_Inherited_List()
		{
			var data = File.ReadAllText(Path.Combine("SampleData", "jsonarray.txt"));
			var response = new RestResponse { Content = data };
			var json = new JsonDeserializer();
			var output = json.Deserialize<StatusList>(response);
			Assert.Equal(4, output.Count);
		}

		[Fact]
		public void Can_Deserialize_Various_Enum_Values ()
		{
			var data = File.ReadAllText (Path.Combine ("SampleData", "jsonenums.txt"));
			var response = new RestResponse { Content = data };
			var json = new JsonDeserializer ();
			var output = json.Deserialize<JsonEnumsTestStructure>(response);

			Assert.Equal(Disposition.Friendly,output.Upper);
			Assert.Equal(Disposition.Friendly,output.Lower);
			Assert.Equal(Disposition.SoSo,output.CamelCased);
			Assert.Equal(Disposition.SoSo,output.Underscores);
			Assert.Equal(Disposition.SoSo,output.LowerUnderscores);
			Assert.Equal(Disposition.SoSo,output.Dashes);
			Assert.Equal(Disposition.SoSo,output.LowerDashes);
			Assert.Equal(Disposition.SoSo,output.Integer);
		}

		[Fact]
		public void Deserialization_Of_Undefined_Int_Value_Returns_Enum_Default()
		{
			const string data = @"{ ""Integer"" : 1024 }";
			var response = new RestResponse { Content = data };
			var json = new JsonDeserializer ();
			var result = json.Deserialize<JsonEnumsTestStructure>(response);
			Assert.Equal(Disposition.Friendly,result.Integer);
		}

		[Fact]
		public void Can_Deserialize_Guid_String_Fields()
		{
			var doc = new JObject();
			doc["Guid"] = GuidString;

			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc.ToString() };
			var p = d.Deserialize<PersonForJson>(response);

			Assert.Equal(new Guid(GuidString), p.Guid);
		}

		[Fact]
		public void Can_Deserialize_Quoted_Primitive()
		{
			var doc = new JObject();
			doc["Age"] = "28";

			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc.ToString() };
			var p = d.Deserialize<PersonForJson>(response);

			Assert.Equal(28, p.Age);
		}

		[Fact]
		public void Can_Deserialize_With_Default_Root()
		{
			var doc = CreateJson();
			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc };
			var p = d.Deserialize<PersonForJson>(response);

			Assert.Equal("John Sheehan", p.Name);
			Assert.Equal(new DateTime(2009, 9, 25, 0, 6, 1, DateTimeKind.Utc), p.StartDate);
			Assert.Equal(28, p.Age);
			Assert.Equal(long.MaxValue, p.BigNumber);
			Assert.Equal(99.9999m, p.Percent);
			Assert.Equal(false, p.IsCool);
			Assert.Equal(new Uri("http://example.com", UriKind.RelativeOrAbsolute), p.Url);
			Assert.Equal(new Uri("/foo/bar", UriKind.RelativeOrAbsolute), p.UrlPath);

			Assert.Equal(Guid.Empty, p.EmptyGuid);
			Assert.Equal(new Guid(GuidString), p.Guid);

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
		public void Can_Deserialize_With_Default_Root_Alternative_Culture()
		{
			using (new CultureChange(AlternativeCulture))
			{
					Can_Deserialize_With_Default_Root();
			}
		}

		[Fact]
		public void Can_Deserialize_Names_With_Underscore_Prefix()
		{
			var data = File.ReadAllText(Path.Combine("SampleData", "underscore_prefix.txt"));
			var response = new RestResponse { Content = data };
			var json = new JsonDeserializer();
			json.RootElement = "User";

			var output = json.Deserialize<SOUser>(response);

			Assert.Equal("John Sheehan", output.DisplayName);
			Assert.Equal(1786, output.Id);
		}

		[Fact]
		public void Can_Deserialize_Names_With_Underscores_With_Default_Root()
		{
			var doc = CreateJsonWithUnderscores();
			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc };
			var p = d.Deserialize<PersonForJson>(response);

			Assert.Equal("John Sheehan", p.Name);
			Assert.Equal(new DateTime(2009, 9, 25, 0, 6, 1), p.StartDate);
			Assert.Equal(28, p.Age);
			Assert.Equal(long.MaxValue, p.BigNumber);
			Assert.Equal(99.9999m, p.Percent);
			Assert.Equal(false, p.IsCool);
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
		public void Can_Deserialize_Names_With_Underscores_With_Default_Root_Alternative_Culture()
		{
			using (new CultureChange(AlternativeCulture))
			{
					Can_Deserialize_Names_With_Underscores_With_Default_Root();
			}
		}

		[Fact]
		public void Can_Deserialize_Names_With_Dashes_With_Default_Root()
		{
			var doc = CreateJsonWithDashes();
			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc };
			var p = d.Deserialize<PersonForJson>(response);

			Assert.Equal("John Sheehan", p.Name);
			//Assert.Equal(new DateTime(2009, 9, 25, 0, 6, 1, DateTimeKind.Utc), p.StartDate);
			Assert.Equal(28, p.Age);
			Assert.Equal(long.MaxValue, p.BigNumber);
			Assert.Equal(99.9999m, p.Percent);
			Assert.Equal(false, p.IsCool);
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
		public void Can_Deserialize_Names_With_Dashes_With_Default_Root_Alternative_Culture()
		{
			using (new CultureChange(AlternativeCulture))
			{
					Can_Deserialize_Names_With_Dashes_With_Default_Root();
			}
		}

		[Fact]
		public void Ignore_Protected_Property_That_Exists_In_Data()
		{
			var doc = CreateJson();
			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc };
			var p = d.Deserialize<PersonForJson>(response);

			Assert.Null(p.IgnoreProxy);
		}

		[Fact]
		public void Ignore_ReadOnly_Property_That_Exists_In_Data()
		{
			var doc = CreateJson();
			var response = new RestResponse { Content = doc };
			var d = new JsonDeserializer();
			var p = d.Deserialize<PersonForJson>(response);

			Assert.Null(p.ReadOnlyProxy);
		}

		[Fact]
		public void Can_Deserialize_TimeSpan()
		{
			var payload = GetPayLoad<TimeSpanTestStructure>("timespans.txt");

			Assert.Equal(new TimeSpan(468006), payload.Tick);
			Assert.Equal(new TimeSpan(0, 0, 0, 0, 125), payload.Millisecond);
			Assert.Equal(new TimeSpan(0, 0, 8), payload.Second);
			Assert.Equal(new TimeSpan(0, 55, 2), payload.Minute);
			Assert.Equal(new TimeSpan(21, 30, 7), payload.Hour);
			Assert.Null(payload.NullableWithoutValue);
			Assert.NotNull(payload.NullableWithValue);
			Assert.Equal(new TimeSpan(21, 30, 7), payload.NullableWithValue.Value);
		}

		[Fact]
		public void Can_Deserialize_Iso_Json_Dates()
		{
			var doc = CreateIsoDateJson();
			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc };
			var bd = d.Deserialize<Birthdate>(response);

			Assert.Equal(new DateTime(1910, 9, 25, 9, 30, 25, DateTimeKind.Utc), bd.Value);
		}

		[Fact]
		public void Can_Deserialize_Unix_Json_Dates()
		{
			var doc = CreateUnixDateJson();
			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc };
			var bd = d.Deserialize<Birthdate>(response);

			Assert.Equal(new DateTime(2011, 6, 30, 8, 15, 46, DateTimeKind.Utc), bd.Value);
		}

		[Fact]
		public void Can_Deserialize_JsonNet_Dates()
		{
			var person = GetPayLoad<PersonForJson>("person.json.txt");

			Assert.Equal(
				new DateTime(2011, 6, 30, 8, 15, 46, 929, DateTimeKind.Utc),
				person.StartDate);
		}

		[Fact]
		public void Can_Deserialize_DateTime()
		{
			var payload = GetPayLoad<DateTimeTestStructure>("datetimes.txt");

			Assert.Equal(
				new DateTime(2011, 6, 30, 8, 15, 46, 929, DateTimeKind.Utc),
				payload.DateTime);
		}

		[Fact]
		public void Can_Deserialize_Nullable_DateTime_With_Value()
		{
			var payload = GetPayLoad<DateTimeTestStructure>("datetimes.txt");

			Assert.NotNull(payload.NullableDateTimeWithValue);
			Assert.Equal(
				new DateTime(2011, 6, 30, 8, 15, 46, 929, DateTimeKind.Utc),
				payload.NullableDateTimeWithValue.Value);
		}

		[Fact]
		public void Can_Deserialize_Nullable_DateTime_With_Null()
		{
			var payload = GetPayLoad<DateTimeTestStructure>("datetimes.txt");

			Assert.Null(payload.NullableDateTimeWithNull);
		}

		[Fact]
		public void Can_Deserialize_DateTimeOffset()
		{
			var payload = GetPayLoad<DateTimeTestStructure>("datetimes.txt");

			Assert.Equal(
				new DateTime(2011, 6, 30, 8, 15, 46, 929, DateTimeKind.Utc),
				payload.DateTimeOffset);
		}

		[Fact]
		public void Can_Deserialize_Iso8601DateTimeLocal()
		{
			var payload = GetPayLoad<Iso8601DateTimeTestStructure>("iso8601datetimes.txt");

			Assert.Equal(
				new DateTime(2012, 7, 19, 10, 23, 25, DateTimeKind.Utc),
				payload.DateTimeLocal);
		}

		[Fact]
		public void Can_Deserialize_Iso8601DateTimeZulu()
		{
			var payload = GetPayLoad<Iso8601DateTimeTestStructure>("iso8601datetimes.txt");

			Assert.Equal(
				new DateTime(2012, 7, 19, 10, 23, 25, 544, DateTimeKind.Utc),
				payload.DateTimeUtc.ToUniversalTime());
		}

		[Fact]
		public void Can_Deserialize_Iso8601DateTimeWithOffset()
		{
			var payload = GetPayLoad<Iso8601DateTimeTestStructure>("iso8601datetimes.txt");

			Assert.Equal(
				new DateTime(2012, 7, 19, 10, 23, 25, 544, DateTimeKind.Utc),
				payload.DateTimeWithOffset.ToUniversalTime());
		}

		[Fact]
		public void Can_Deserialize_Nullable_DateTimeOffset_With_Value()
		{
			var payload = GetPayLoad<DateTimeTestStructure>("datetimes.txt");

			Assert.NotNull(payload.NullableDateTimeOffsetWithValue);
			Assert.Equal(
				new DateTime(2011, 6, 30, 8, 15, 46, 929, DateTimeKind.Utc),
				payload.NullableDateTimeOffsetWithValue);
		}

		[Fact]
		public void Can_Deserialize_Nullable_DateTimeOffset_With_Null()
		{
			var payload = GetPayLoad<DateTimeTestStructure>("datetimes.txt");

			Assert.Null(payload.NullableDateTimeOffsetWithNull);
		}

		[Fact]
		public void Can_Deserialize_To_Dictionary_String_String()
		{
			var doc = CreateJsonStringDictionary();
			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc };
			var bd = d.Deserialize<Dictionary<string,string>>(response);

			Assert.Equal(bd["Thing1"], "Thing1");
			Assert.Equal(bd["Thing2"], "Thing2");
			Assert.Equal(bd["ThingRed"], "ThingRed");
			Assert.Equal(bd["ThingBlue"], "ThingBlue");
		}

		[Fact]
		public void Can_Deserialize_To_Dictionary_String_String_With_Dynamic_Values ()
		{
			var doc = CreateDynamicJsonStringDictionary ();
			var d = new JsonDeserializer ();
			var response = new RestResponse { Content = doc };
			var bd = d.Deserialize<Dictionary<string, string>> (response);

			Assert.Equal ("[\"Value1\",\"Value2\"]", bd["Thing1"]);
			Assert.Equal ("Thing2", bd["Thing2"]);
			Assert.Equal ("{\"Name\":\"ThingRed\",\"Color\":\"Red\"}", bd["ThingRed"]);
			Assert.Equal ("{\"Name\":\"ThingBlue\",\"Color\":\"Blue\"}", bd["ThingBlue"]);
		}

		private string CreateJsonWithUnderscores()
		{
			var doc = new JObject();
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

			doc["best_friend"] = new JObject(
									new JProperty("name", "The Fonz"),
									new JProperty("since", 1952)
								);

			var friendsArray = new JArray();
			for (int i = 0; i < 10; i++)
			{
				friendsArray.Add(new JObject(
									new JProperty("name", "Friend" + i),
									new JProperty("since", DateTime.Now.Year - i)
								));
			}

			doc["friends"] = friendsArray;

			var foesArray = new JObject(
								new JProperty("dict1", new JObject(new JProperty("nickname", "Foe 1"))),
								new JProperty("dict2", new JObject(new JProperty("nickname", "Foe 2")))
							);

			doc["foes"] = foesArray;

			return doc.ToString();
		}

		private string CreateJsonWithDashes()
		{
			var doc = new JObject();
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

			doc["best-friend"] = new JObject(
									new JProperty("name", "The Fonz"),
									new JProperty("since", 1952)
								);

			var friendsArray = new JArray();
			for (int i = 0; i < 10; i++)
			{
				friendsArray.Add(new JObject(
									new JProperty("name", "Friend" + i),
									new JProperty("since", DateTime.Now.Year - i)
								));
			}

			doc["friends"] = friendsArray;

			var foesArray = new JObject(
								new JProperty("dict1", new JObject(new JProperty("nickname", "Foe 1"))),
								new JProperty("dict2", new JObject(new JProperty("nickname", "Foe 2")))
							);

			doc["foes"] = foesArray;

			return doc.ToString();
		}

		private string CreateIsoDateJson()
		{
			var bd = new Birthdate();
			bd.Value = new DateTime(1910, 9, 25, 9, 30, 25, DateTimeKind.Utc);

			return JsonConvert.SerializeObject(bd, new IsoDateTimeConverter());
		}

		private string CreateUnixDateJson()
		{
			var doc = new JObject();
			doc["Value"] = 1309421746;

			return doc.ToString();
		}

		private string CreateJson()
		{
			var doc = new JObject();
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

			doc["Guid"] = new Guid(GuidString).ToString();
			doc["EmptyGuid"] = "";

			doc["BestFriend"] = new JObject(
									new JProperty("Name", "The Fonz"),
									new JProperty("Since", 1952)
								);

			var friendsArray = new JArray();
			for (int i = 0; i < 10; i++)
			{
				friendsArray.Add(new JObject(
									new JProperty("Name", "Friend" + i),
									new JProperty("Since", DateTime.Now.Year - i)
								));
			}

			doc["Friends"] = friendsArray;

			var foesArray = new JObject(
								new JProperty("dict1", new JObject(new JProperty("Nickname", "Foe 1"))),
								new JProperty("dict2", new JObject(new JProperty("Nickname", "Foe 2")))
							);

			doc["Foes"] = foesArray;

			return doc.ToString();
		}

		private string CreateJsonWithNullValues()
		{
			var doc = new JObject();
			doc["Id"] = null;
			doc["StartDate"] = null;
			doc["UniqueId"] = null;

			return doc.ToString();
		}

		private string CreateJsonWithoutEmptyValues()
		{
			var doc = new JObject();
			doc["Id"] = 123;
			doc["StartDate"] = new DateTime(2010, 2, 21, 9, 35, 00, DateTimeKind.Utc);
			doc["UniqueId"] = new Guid(GuidString).ToString();

			return doc.ToString();
		}

		public string CreateJsonStringDictionary()
		{
			var doc = new JObject();
			doc["Thing1"] = "Thing1";
			doc["Thing2"] = "Thing2";
			doc["ThingRed"] = "ThingRed";
			doc["ThingBlue"] = "ThingBlue";
			return doc.ToString();
		}

		public string CreateDynamicJsonStringDictionary ()
		{
			var doc = new JObject ();
			doc["Thing1"] = new JArray () { "Value1", "Value2" };
			doc["Thing2"] = "Thing2";
			doc["ThingRed"] = new JObject (new JProperty ("Name", "ThingRed"), new JProperty ("Color", "Red"));
			doc["ThingBlue"] = new JObject (new JProperty("Name", "ThingBlue"), new JProperty ("Color", "Blue"));
			return doc.ToString ();
		}

		private T GetPayLoad<T>(string fileName)
		{
			var doc = File.ReadAllText(Path.Combine("SampleData", fileName));
			var response = new RestResponse { Content = doc };
			var d = new JsonDeserializer();
			return d.Deserialize<T>(response);
		}
	}
}
