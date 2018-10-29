
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
        /// If the object is already a serialized string returns that value
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>JSON as String</returns>
        public string Serialize(object obj)
        {
	        if (IsSerializedString(obj, out var serializedString))
	        {
		        return serializedString;
	        }

            return SimpleJson.SerializeObject(obj);
        }

		/// <summary>
		/// Determines if the object is already a serialized string.
		/// </summary>
	    private static bool IsSerializedString(object obj, out string serializedString)
	    {
		    if( obj is string value )
		    {
			    string trimmed = value.Trim();

			    if( ( trimmed.StartsWith( "{" ) && trimmed.EndsWith( "}" ) ) 
				    || ( trimmed.StartsWith( "[{" ) && trimmed.EndsWith( "}]" ) ) )
			    {
				    serializedString = value;
				    return true;
			    }
		    }

		    serializedString = null;
		    return false;
	    }

        /// <summary>
        /// Content type for serialized content
        /// </summary>
        public string ContentType { get; set; }
    }
}
