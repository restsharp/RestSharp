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
	public class DynamicJsonTests
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
			var json = new JsonDeserializer { RootElement = "users" };

			var output = json.Deserialize<List<string>>(new RestResponse { Content = content });

			Assert.NotEmpty(output);
		}

		[Fact]
		public void Can_Deserialize_From_Root_Element()
		{
			var doc = File.ReadAllText(@"SampleData\sojson.txt");

			var json = new JsonDeserializer { RootElement = "User" };

			dynamic output = json.Deserialize(new RestResponse { Content = doc });
			Assert.Equal("John Sheehan", (string)output.DisplayName);
		}

		[Fact(Skip = "Bug filed http://json.codeplex.com/workitem/19663")]
		public void Can_Deserialize_Empty_Elements_to_Nullable_Values()
		{
			var doc = CreateJsonWithNullValues();

			var json = new JsonDeserializer();
			dynamic output = json.Deserialize(new RestResponse { Content = doc });

			Assert.Null(output.Id);
			Assert.Null(output.StartDate);
			Assert.Null(output.UniqueId);
		}

		[Fact]
		public void Can_Deserialize_Elements_to_Nullable_Values()
		{
			var doc = CreateJsonWithoutEmptyValues();

			var json = new JsonDeserializer();
			dynamic output = json.Deserialize(new RestResponse { Content = doc });

			Assert.NotNull(output.Id);
			Assert.NotNull(output.StartDate);
			Assert.NotNull(output.UniqueId);

			Assert.Equal(123, (int)output.Id);
			Assert.Equal(new DateTime(2010, 2, 21, 9, 35, 00), (DateTime)output.StartDate);
			Assert.Equal(GuidString.ToLower(), (string)output.UniqueId);
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
			dynamic p = d.Deserialize(response);

			Assert.Equal(GuidString, (string)p.Guid);
		}

		[Fact]
		public void Can_Deserialize_Quoted_Primitive()
		{
			var doc = new JObject();
			doc["Age"] = "28";

			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc.ToString() };
			dynamic p = d.Deserialize(response);

			Assert.Equal(28, (int)p.Age);
		}

		[Fact]
		public void Can_Deserialize_With_Default_Root()
		{
			var doc = CreateJson();
			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc };
			dynamic p = d.Deserialize(response);

			string s = p.Name;

			Assert.Equal("John Sheehan", (string)p.Name);
			Assert.Equal(new DateTime(2009, 9, 25, 0, 6, 1, DateTimeKind.Utc), (DateTime)p.StartDate);
			Assert.Equal(28, (int)p.Age);
			Assert.Equal(long.MaxValue, (long)p.BigNumber);
			Assert.Equal(99.9999M, (decimal)p.Percent);
			Assert.Equal(false, (bool)p.IsCool);
			Assert.Equal("http://example.com", (string)p.Url);
			Assert.Equal(new Uri("/foo/bar", UriKind.RelativeOrAbsolute).ToString(), (string)p.UrlPath);

			Assert.Equal(GuidString.ToLower(), (string)p.Guid);

			//Assert.Equal(Order.Third, p.Order);

			Assert.NotNull(p.Friends);
			Assert.Equal(10, p.Friends.Count);

			Assert.NotNull(p.BestFriend);
			Assert.Equal("The Fonz", (string)p.BestFriend.Name);
			Assert.Equal(1952, (int)p.BestFriend.Since);

			Assert.NotEmpty(p.Foes);
			Assert.Equal("Foe 1", (string)p.Foes.dict1.Nickname);
			Assert.Equal("Foe 2", (string)p.Foes.dict2.Nickname);
		}

		[Fact]
		public void Ignore_Protected_Property_That_Exists_In_Data()
		{
			var doc = CreateJson();
			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc };
			dynamic p = d.Deserialize(response);

			Assert.Null(p.IgnoreProxy);
		}

		[Fact]
		public void Ignore_ReadOnly_Property_That_Exists_In_Data()
		{
			var doc = CreateJson();
			var response = new RestResponse { Content = doc };
			var d = new JsonDeserializer();
			dynamic p = d.Deserialize(response);

			Assert.Null(p.ReadOnlyProxy);
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
