namespace RestSharp.Tests.Serializers.Xml.SampleClasses;

public class VenuesResponse {
    public List<Group> Groups { get; set; }
}

public class Group {
    public string Name { get; set; }
}