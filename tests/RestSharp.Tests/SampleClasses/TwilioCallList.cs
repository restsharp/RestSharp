using System.Collections.Generic;

namespace RestSharp.Tests.SampleClasses
{
    public class TwilioCallList : List<Call>
    {
        public int Page { get; set; }

        public int NumPages { get; set; }
    }

    public class Call
    {
        public string Sid { get; set; }
    }
}
