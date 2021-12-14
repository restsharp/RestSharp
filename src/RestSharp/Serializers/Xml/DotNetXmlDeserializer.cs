using System.Text;

namespace RestSharp.Serializers.Xml; 

/// <summary>
/// Wrapper for System.Xml.Serialization.XmlSerializer.
/// </summary>
public class DotNetXmlDeserializer : IXmlDeserializer {
    /// <summary>
    /// Encoding for serialized content
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;
    
    /// <summary>
    /// Name of the root element to use when serializing
    /// </summary>
    public string? RootElement { get; set; }

    /// <summary>
    /// XML namespace to use when serializing
    /// </summary>
    public string? Namespace { get; set; }

    public string? DateFormat { get; set; }

    public T? Deserialize<T>(RestResponse response) {
        if (string.IsNullOrEmpty(response.Content)) return default;

        using var stream = new MemoryStream(Encoding.GetBytes(response.Content!));

        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));

        return (T?)serializer.Deserialize(stream);
    }
}