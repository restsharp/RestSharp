using System.Runtime.Serialization;

namespace RestSharp.Authenticators.OAuth
{
    [DataContract]
    internal class WebParameter : WebPair
    {
        public WebParameter(string name, string value)
            : base(name, value) { }
    }
}
