namespace RestSharp
{
    public class RequestBody
    {
        public string ContentType { get; }
        public string Name { get; }
        public object Value { get; }

        public RequestBody(string contentType, string name, object value)
        {
            ContentType = contentType;
            Name        = name;
            Value       = value;
        }
    }
}