namespace RestSharp
{
    public class BodyParameter
    {
        public object Value { get; }
        public DataFormat ParameterType { get; }
        public string XmlNamespace { get; }

        public BodyParameter(object value)
        {
            Value = value;
            ParameterType = DataFormat.Json;
        }

        public BodyParameter(object value, string xmlNamespace)
        {
            Value = value;
            XmlNamespace = xmlNamespace;
            ParameterType = DataFormat.Xml;
        }
    }
}