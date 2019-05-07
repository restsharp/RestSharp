using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp.Serialization.Xml;

namespace RestSharp.IntegrationTests.SampleDeserializers
{
    class CustomDeserializer : IXmlDeserializer
    {
        public T Deserialize<T>(IRestResponse response)
        {
            return default(T);
        }

        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
    }
}
