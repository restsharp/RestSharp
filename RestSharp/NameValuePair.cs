namespace RestSharp
{
    public class NameValuePair
    {
        public NameValuePair(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public string Value { get; }

        public bool IsEmpty => Name == null;
        
        public static NameValuePair Empty = new NameValuePair(null, null);
    }
}