
namespace RestSharp.Authenticators.OAuth
{
    internal class WebPair
    {
        public WebPair(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Value { get; set; }

        public string Name { get; set; }
    }
}
