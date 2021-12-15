namespace RestSharp.Serializers.Xml.Tests.SampleClasses.DeserializeAsTest; 

public class NodeWithAttributeAndValue {
    [DeserializeAs(Name = "attribute-value", Attribute = true)]
    public string AttributeValue { get; set; }
}

public class SingleNode {
    [DeserializeAs(Name = "node-value")]
    public string Node { get; set; }
}