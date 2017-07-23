using System.Collections.Generic;

namespace RestSharp.Tests.SampleClasses
{
    public class VenuesResponse
    {
        public List<Group> Groups { get; set; }
    }

    public class Group
    {
        public string Name { get; set; }
    }
}
