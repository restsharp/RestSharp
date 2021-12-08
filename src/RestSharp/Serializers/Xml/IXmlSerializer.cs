namespace RestSharp.Serializers.Xml; 

public interface IXmlSerializer : ISerializer, IWithRootElement {
    string Namespace { get; set; }

    string DateFormat { get; set; }
}