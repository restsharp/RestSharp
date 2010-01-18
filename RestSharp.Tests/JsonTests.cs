#region License
//   Copyright 2009 John Sheehan
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
using Newtonsoft.Json.Linq;
using RestSharp.Deserializers;
using Xunit;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using RestSharp.Tests.SampleClasses;
using System.Collections.Generic;

namespace RestSharp.Tests
{
	public class JsonTests
	{
		[Fact]
		public void Can_Deserialize_Root_Json_Array_To_List() {
			var data = File.ReadAllText(@"..\..\SampleData\jsonarray.txt");
			var json = new JsonDeserializer();
			var output = json.Deserialize<List<status>>(data);
			Assert.Equal(4, output.Count);
		}

		[Fact]
		public void Can_Deserialize_Quoted_Primitive() {
			var doc = new JObject();
			doc["Age"] = "28";

			var d = new JsonDeserializer();
			var p = d.Deserialize<PersonForJson>(doc.ToString());

			Assert.Equal(28, p.Age);
		}

		[Fact]
		public void Can_Deserialize_With_Default_Root() {
			var doc = CreateJson();
			var d = new JsonDeserializer();
			var p = d.Deserialize<PersonForJson>(doc);

			Assert.Equal("John Sheehan", p.Name);
			Assert.Equal(new DateTime(2009, 9, 25, 0, 6, 1), p.StartDate);
			Assert.Equal(28, p.Age);
			Assert.Equal(long.MaxValue, p.BigNumber);
			Assert.Equal(99.9999m, p.Percent);
			Assert.Equal(false, p.IsCool);

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
		public void Can_Deserialize_Names_With_Underscores_With_Default_Root() {
			var doc = CreateJsonWithUnderscores();
			var d = new JsonDeserializer();
			var p = d.Deserialize<PersonForJson>(doc);

			Assert.Equal("John Sheehan", p.Name);
			Assert.Equal(new DateTime(2009, 9, 25, 0, 6, 1), p.StartDate);
			Assert.Equal(28, p.Age);
			Assert.Equal(long.MaxValue, p.BigNumber);
			Assert.Equal(99.9999m, p.Percent);
			Assert.Equal(false, p.IsCool);

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
		public void Ignore_Protected_Property_That_Exists_In_Data() {
			var doc = CreateJson();
			var d = new JsonDeserializer();
			var p = d.Deserialize<PersonForJson>(doc);

			Assert.Null(p.IgnoreProxy);
		}

		[Fact]
		public void Ignore_ReadOnly_Property_That_Exists_In_Data() {
			var doc = CreateJson();
			var d = new JsonDeserializer();
			var p = d.Deserialize<PersonForJson>(doc);

			Assert.Null(p.ReadOnlyProxy);
		}

		[Fact]
		public void Can_Deserialize_Iso_Json_Dates() {
			var doc = CreateIsoDateJson();
			var d = new JsonDeserializer();
			var bd = d.Deserialize<Birthdate>(doc);

			Assert.Equal(new DateTime(1910, 9, 25, 9, 30, 25, DateTimeKind.Utc), bd.Value);
		}

		[Fact]
		public void Can_Deserialize_JScript_Json_Dates() {
			var doc = CreateJScriptDateJson();
			var d = new JsonDeserializer();
			var bd = d.Deserialize<Birthdate>(doc);

			Assert.Equal(new DateTime(1910, 9, 25, 9, 30, 25, DateTimeKind.Utc), bd.Value);
		}

		private string CreateJsonWithUnderscores() {
			var doc = new JObject();
			doc["name"] = "John Sheehan";
			doc["start_date"] = new DateTime(2009, 9, 25, 0, 6, 1);
			doc["age"] = 28;
			doc["percent"] = 99.9999m;
			doc["big_number"] = long.MaxValue;
			doc["is_cool"] = false;
			doc["ignore"] = "dummy";
			doc["read_only"] = "dummy";

			doc["best_friend"] = new JObject(
									new JProperty("name", "The Fonz"),
									new JProperty("since", 1952)
								);

			var friendsArray = new JArray();
			for (int i = 0; i < 10; i++) {
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

		private string CreateIsoDateJson() {
			var bd = new Birthdate();
			bd.Value = new DateTime(1910, 9, 25, 9, 30, 25, DateTimeKind.Utc);

			return JsonConvert.SerializeObject(bd, new IsoDateTimeConverter());
		}

		private string CreateJScriptDateJson() {
			var bd = new Birthdate();
			bd.Value = new DateTime(1910, 9, 25, 9, 30, 25, DateTimeKind.Utc);

			return JsonConvert.SerializeObject(bd, new JavaScriptDateTimeConverter());
		}

		private string CreateJson() {
			var doc = new JObject();
			doc["Name"] = "John Sheehan";
			doc["StartDate"] = new DateTime(2009, 9, 25, 0, 6, 1);
			doc["Age"] = 28;
			doc["Percent"] = 99.9999m;
			doc["BigNumber"] = long.MaxValue;
			doc["IsCool"] = false;
			doc["Ignore"] = "dummy";
			doc["ReadOnly"] = "dummy";

			doc["BestFriend"] = new JObject(
									new JProperty("Name", "The Fonz"),
									new JProperty("Since", 1952)
								);

			var friendsArray = new JArray();
			for (int i = 0; i < 10; i++) {
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
	}
}
