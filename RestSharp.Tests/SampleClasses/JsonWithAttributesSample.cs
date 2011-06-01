using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace RestSharp.Tests.SampleClasses
{
    public class JsonWithAttributesSample
    {
        [JsonProperty(PropertyName = "id", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int ClientId { get; set; }

        [JsonProperty(PropertyName = "name", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string ClientName { get; set; }

        [JsonProperty(PropertyName = "hourly_rate", Required = Required.Default,
            NullValueHandling = NullValueHandling.Ignore)]
        public double HourlyRate { get; set; }

    }
}
