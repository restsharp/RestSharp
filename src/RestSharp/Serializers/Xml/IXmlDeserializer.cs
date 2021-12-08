namespace RestSharp.Serializers.Xml; 

public interface IXmlDeserializer : IDeserializer, IWithRootElement {
    string Namespace { get; set; }

    string DateFormat { get; set; }
}