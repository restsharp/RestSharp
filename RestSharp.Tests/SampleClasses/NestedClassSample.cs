using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace RestSharp.Tests.SampleClasses
{
    public class NestedClassSample
    {
        [JsonProperty(PropertyName = "identifier")]
        public int Id { get; set; }

    }
}
