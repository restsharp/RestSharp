namespace RestSharp.Serializers
{
    public interface IXmlSerializer : ISerializer
    {
        string RootElement { get; set; }

        string Namespace { get; set; }

        string DateFormat { get; set; }
    }
}
