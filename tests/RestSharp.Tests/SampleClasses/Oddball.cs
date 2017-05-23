namespace RestSharp.Tests.SampleClasses
{
    [Deserializers.DeserializeAs(Name = "oddballRootName")]
    public class Oddball
    {
        public string Sid { get; set; }

        public string FriendlyName { get; set; }

        [Deserializers.DeserializeAs(Name = "oddballPropertyName")]
        public string GoodPropertyName { get; set; }
    }
}
