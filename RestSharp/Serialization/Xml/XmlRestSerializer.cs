namespace RestSharp.Serialization.Xml
{
    public class XmlRestSerializer : IRestSerializer
    {
        public string Serialize(object obj)
        {
            throw new System.NotImplementedException();
        }

        public string ContentType { get; set; } = "text/xml";
        
        public T Deserialize<T>(IRestResponse response)
        {
            throw new System.NotImplementedException();
        }
    }
}