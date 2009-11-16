using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Newtonsoft.Json.Linq;

using RestSharp.Deserializers;

namespace RestSharp.Tests
{
	public class JsonTests
	{
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
		public void Can_Deserialize_Names_With_Underscores_On_Default_Root() {
		}

		private static string CreateJson() {
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
