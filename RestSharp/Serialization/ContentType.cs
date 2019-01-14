using System.Collections.Generic;

namespace RestSharp.Serialization
{
    public static class ContentType
    {
        public const string Json = "application/json";

        public const string Xml = "application/xml";

        public static Dictionary<DataFormat, string> FromDataFormat =
            new Dictionary<DataFormat, string>
            {
                {DataFormat.Xml, Xml},
                {DataFormat.Json, Json}
            };
    }
}