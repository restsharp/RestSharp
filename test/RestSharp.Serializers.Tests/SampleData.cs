using System.Collections.Generic;

namespace RestSharp.Serializers.Tests
{
    public class TestClass
    {
        public string SimpleString { get; set; }
        public int SimpleInt { get; set; }
        public List<Subclass> List { get; set; }
        public Subclass Sub { get; set; }

        public class Subclass
        {
            public string Thing { get; set; }
            public int AnotherThing { get; set; }
        }
    }
}