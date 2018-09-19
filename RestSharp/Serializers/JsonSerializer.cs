
namespace RestSharp.Serializers
{
    using SimpleJson;
    /// <summary>
    /// Default JSON serializer for request bodies
    /// Doesn't currently use the SerializeAs attribute, defers to SimpleJson's attributes
    /// </summary>
    public class JsonSerializer : ISerializer
    {
        /// <summary>
        /// Default serializer
        /// </summary>
        public JsonSerializer()
        {
            ContentType = "application/json";
        }

        /// <summary>
        /// Serialize the object as JSON
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>JSON as String</returns>
        public string Serialize(object obj) => SimpleJson.SerializeObject(obj);

        /// <summary>
        /// Content type for serialized content
        /// </summary>
        public string ContentType { get; set; }
    }
}
