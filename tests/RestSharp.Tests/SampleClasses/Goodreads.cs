using System.Collections.Generic;

namespace RestSharp.Tests.SampleClasses
{
    public class GoodReadsReviewCollection
    {
        public int Start { get; set; }

        public int End { get; set; }

        public int Total { get; set; }

        public List<GoodReadsReview> Reviews { get; set; }
    }

    public class GoodReadsReview
    {
        public string Id { get; set; }

        public GoodReadsBook Book { get; set; }
    }

    public class GoodReadsBook
    {
        public string Isbn { get; set; }
    }
}
