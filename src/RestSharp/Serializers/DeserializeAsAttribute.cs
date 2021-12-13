namespace RestSharp.Serializers;

/// <summary>
/// Allows control how class and property names and values are deserialized by XmlAttributeDeserializer
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = false)]
public sealed class DeserializeAsAttribute : Attribute {
    /// <summary>
    /// The name to use for the serialized element
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Sets if the property to Deserialize is an Attribute or Element (Default: false)
    /// </summary>
    public bool Attribute { get; set; }

    /// <summary>
    /// Sets if the property to Deserialize is a content of current Element (Default: false)
    /// </summary>
    public bool Content { get; set; }
}