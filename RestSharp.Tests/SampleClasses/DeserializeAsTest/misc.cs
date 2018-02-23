using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Tests.SampleClasses.DeserializeAsTest
{
    public class NodeWithAttributeAndValue
    {
        [DeserializeAs(Name = "attribute-value", Attribute = true)]
        public string AttributeValue { get; set; }
    }

    public class SingleNode
    {
        [DeserializeAs(Name = "node-value")]
        public string Node { get; set; }
    }
}
