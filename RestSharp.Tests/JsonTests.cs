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
using System.Globalization;
using System.IO;
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
		private const string GuidString = "AC1FC4BC-087A-4242-B8EE-C53EBE9887A5";

		[Fact]
		public void Can_Deserialize_Lists_of_Simple_Types()
		{
			var doc = File.ReadAllText(@"SampleData\jsonlists.txt");
			var json = new JsonDeserializer();

			var output = json.Deserialize<JsonLists>(new RestResponse { Content = doc });

			Assert.NotEmpty(output.Names);
			Assert.NotEmpty(output.Numbers);
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
		public void Can_Deserialize_From_Root_Element()
		{
			var doc = File.ReadAllText(@"SampleData\sojson.txt");

			var json = new JsonDeserializer();
			json.RootElement = "User";

			var output = json.Deserialize<SOUser>(new RestResponse { Content = doc });
			Assert.Equal("John Sheehan", output.DisplayName);
		}

		[Fact]
		public void Can_Deserialize_Generic_Members()
		{
			var doc = File.ReadAllText(@"SampleData\GenericWithList.txt");
			var json = new JsonDeserializer();

			var output = json.Deserialize<Generic<GenericWithList<Foe>>>(new RestResponse { Content = doc });
			Assert.Equal("Foe sho", output.Data.Items[0].Nickname);
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
			Assert.Equal(new DateTime(2010, 2, 21, 9, 35, 00), output.StartDate);
			Assert.Equal(new Guid(GuidString), output.UniqueId);
		}

		[Fact]
		public void Can_Deserialize_Custom_Formatted_Date()
		{
			var format = "dd yyyy MMM, hh:mm ss tt";
			var date = new DateTime(2010, 2, 8, 11, 11, 11);

			var formatted = new
			{
				StartDate = date.ToString(format, CultureInfo.InvariantCulture)
			};

			var data = JsonConvert.SerializeObject(formatted);
			var response = new RestResponse { Content = data };

			var json = new JsonDeserializer { DateFormat = format };

			var output = json.Deserialize<PersonForJson>(response);

			Assert.Equal(date, output.StartDate);
		}

		[Fact]
		public void Can_Deserialize_Root_Json_Array_To_List()
		{
			var data = File.ReadAllText(@"SampleData\jsonarray.txt");
			var response = new RestResponse { Content = data };
			var json = new JsonDeserializer();
			var output = json.Deserialize<List<status>>(response);
			Assert.Equal(4, output.Count);
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
		public void Can_Deserialize_Names_With_Dashes_With_Default_Root()
		{
			var doc = CreateJsonWithDashes();
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
		public void Can_Deserialize_Iso_Json_Dates()
		{
			var doc = CreateIsoDateJson();
			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc };
			var bd = d.Deserialize<Birthdate>(response);

			Assert.Equal(new DateTime(1910, 9, 25, 9, 30, 25, DateTimeKind.Utc), bd.Value);
		}

		[Fact]
		public void Can_Deserialize_JScript_Json_Dates()
		{
			var doc = CreateJScriptDateJson();
			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc };
			var bd = d.Deserialize<Birthdate>(response);

			Assert.Equal(new DateTime(1910, 9, 25, 9, 30, 25, DateTimeKind.Utc), bd.Value);
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

		private string CreateJScriptDateJson()
		{
			var bd = new Birthdate();
			bd.Value = new DateTime(1910, 9, 25, 9, 30, 25, DateTimeKind.Utc);

			return JsonConvert.SerializeObject(bd, new JavaScriptDateTimeConverter());
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
			doc["Order"] = "Third";

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
			doc["StartDate"] = new DateTime(2010, 2, 21, 9, 35, 00);
			doc["UniqueId"] = new Guid(GuidString).ToString();

			return doc.ToString();
		}
	}
}
