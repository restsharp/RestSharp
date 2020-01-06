using System;
using RestSharp.Deserializers;

namespace RestSharp.Tests.SampleClasses
{
    public class BearerToken
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public uint ExpiresIn { get; set; }
        public string UserName { get; set; }
        [DeserializeAs(Name = ".issued")]
        public DateTimeOffset Issued { get; set; }
        [DeserializeAs(Name = ".expires")]
        public DateTimeOffset Expires { get; set; }
    }
}