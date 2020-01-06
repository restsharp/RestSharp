using System;
using System.Linq;
using RestSharp.Tests.SampleClasses;

namespace RestSharp.Tests.TestData
{
    public static class JsonData
    {
        public const string GUID_STRING = "AC1FC4BC-087A-4242-B8EE-C53EBE9887A5";

        public static string JsonWithNullValues =
            new JsonObject {["Id"] = null, ["StartDate"] = null, ["UniqueId"] = null}.ToString();

        public static string JsonWithEmptyValues =
            new JsonObject {["Id"] = "", ["StartDate"] = "", ["UniqueId"] = ""}.ToString();

        public static string CreateJsonWithoutEmptyValues =
            new JsonObject
            {
                ["Id"]        = 123,
                ["StartDate"] = new DateTime(2010, 2, 21, 9, 35, 00, DateTimeKind.Utc),
                ["UniqueId"]  = new Guid(GUID_STRING).ToString()
            }.ToString();

        public static string JsonStringDictionary =
            new JsonObject
            {
                ["Thing1"] = "Thing1", ["Thing2"] = "Thing2", ["ThingRed"] = "ThingRed", ["ThingBlue"] = "ThingBlue"
            }.ToString();

        public static string DynamicJsonStringDictionary =
            new JsonObject
            {
                ["Thing1"]    = new JsonArray {"Value1", "Value2"},
                ["Thing2"]    = "Thing2",
                ["ThingRed"]  = new JsonObject {{"Name", "ThingRed"}, {"Color", "Red"}},
                ["ThingBlue"] = new JsonObject {{"Name", "ThingBlue"}, {"Color", "Blue"}}
            }.ToString();

        public static readonly string UnixDateJson             = new JsonObject {["Value"] = 1309421746}.ToString();
        public static readonly string UnixDateMillisecondsJson = new JsonObject {["Value"] = 1309421746000}.ToString();

        public static string CreateJsonWithUnderscores()
        {
            var doc = new JsonObject
            {
                ["name"]        = "John Sheehan",
                ["start_date"]  = new DateTime(2009, 9, 25, 0, 6, 1, DateTimeKind.Utc),
                ["age"]         = 28,
                ["percent"]     = 99.9999m,
                ["big_number"]  = long.MaxValue,
                ["is_cool"]     = false,
                ["ignore"]      = "dummy",
                ["read_only"]   = "dummy",
                ["url"]         = "http://example.com",
                ["url_path"]    = "/foo/bar",
                ["best_friend"] = new JsonObject {{"name", "The Fonz"}, {"since", 1952}}
            };

            var friendsArray = new JsonArray();

            friendsArray.AddRange(
                Enumerable.Range(0, 10)
                    .Select(
                        i =>
                            new JsonObject
                            {
                                {"name", "Friend"           + i},
                                {"since", DateTime.Now.Year - i}
                            }
                    )
            );

            doc["friends"] = friendsArray;

            doc["foes"] =
                new JsonObject
                {
                    {"dict1", new JsonObject {{"nickname", "Foe 1"}}},
                    {"dict2", new JsonObject {{"nickname", "Foe 2"}}}
                };

            return doc.ToString();
        }

        public static string CreateJsonWithDashes()
        {
            var doc = new JsonObject
            {
                ["name"]        = "John Sheehan",
                ["start-date"]  = new DateTime(2009, 9, 25, 0, 6, 1, DateTimeKind.Utc),
                ["age"]         = 28,
                ["percent"]     = 99.9999m,
                ["big-number"]  = long.MaxValue,
                ["is-cool"]     = false,
                ["ignore"]      = "dummy",
                ["read-only"]   = "dummy",
                ["url"]         = "http://example.com",
                ["url-path"]    = "/foo/bar",
                ["best-friend"] = new JsonObject {{"name", "The Fonz"}, {"since", 1952}}
            };

            var friendsArray = new JsonArray();

            friendsArray.AddRange(
                Enumerable.Range(0, 10)
                    .Select(
                        i =>
                            new JsonObject
                            {
                                {"name", "Friend"           + i},
                                {"since", DateTime.Now.Year - i}
                            }
                    )
            );

            doc["friends"] = friendsArray;

            doc["foes"] =
                new JsonObject
                {
                    {"dict1", new JsonObject {{"nickname", "Foe 1"}}},
                    {"dict2", new JsonObject {{"nickname", "Foe 2"}}}
                };

            return doc.ToString();
        }

        public static string CreateJson()
        {
            var doc = new JsonObject
            {
                ["Name"]        = "John Sheehan",
                ["StartDate"]   = new DateTime(2009, 9, 25, 0, 6, 1, DateTimeKind.Utc),
                ["Age"]         = 28,
                ["Percent"]     = 99.9999m,
                ["BigNumber"]   = long.MaxValue,
                ["IsCool"]      = false,
                ["Ignore"]      = "dummy",
                ["ReadOnly"]    = "dummy",
                ["Url"]         = "http://example.com",
                ["UrlPath"]     = "/foo/bar",
                ["Order"]       = "third",
                ["Disposition"] = "so_so",
                ["Guid"]        = new Guid(GUID_STRING).ToString(),
                ["EmptyGuid"]   = "",
                ["BestFriend"]  = new JsonObject {{"Name", "The Fonz"}, {"Since", 1952}}
            };

            var friendsArray = new JsonArray();

            for (var i = 0; i < 10; i++)
                friendsArray.Add(
                    new JsonObject
                    {
                        {"Name", "Friend"           + i},
                        {"Since", DateTime.Now.Year - i}
                    }
                );

            doc["Friends"] = friendsArray;

            var foesArray = new JsonObject
            {
                {"dict1", new JsonObject {{"nickname", "Foe 1"}}},
                {"dict2", new JsonObject {{"nickname", "Foe 2"}}}
            };

            doc["Foes"] = foesArray;

            return doc.ToString();
        }

        public static string CreateIsoDateJson()
            => SimpleJson.SerializeObject(
                new Birthdate
                {
                    Value = new DateTime(1910, 9, 25, 9, 30, 25, DateTimeKind.Utc)
                }
            );
    }
}