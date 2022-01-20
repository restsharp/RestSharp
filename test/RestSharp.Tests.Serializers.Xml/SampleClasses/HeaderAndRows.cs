using RestSharp.Serializers;

namespace RestSharp.Tests.Serializers.Xml.SampleClasses;

public class Header {
    public string Title { get; set; }
    public string Body  { get; set; }
    public string Date  { get; set; }

    [DeserializeAs(Name = "rows")]
    public List<Row> Othername { get; set; }
}

public class Row {
    public string Text1 { get; set; }
    public string Text2 { get; set; }
}