using System.Collections.Generic;

namespace RestSharp.Serialization
{
    public static class ContentType
    {
        public const string Json = "application/json";

        public const string Xml = "application/xml";

        public static readonly Dictionary<DataFormat, string> FromDataFormat =
            new Dictionary<DataFormat, string>
            {
                {DataFormat.Xml, Xml},
                {DataFormat.Json, Json}
            };

        public static readonly string[] JsonAccept =
        {
            "application/json", "text/json", "text/x-json", "text/javascript", "*+json"
        };

        public static readonly string[] XmlAccept =
        {
            "application/xml", "text/xml", "*+xml", "*"
        };
    }
}