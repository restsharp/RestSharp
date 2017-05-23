using System.Collections.Generic;

namespace RestSharp.Tests.SampleClasses
{
    public class SimpleTypesListSample
    {
        public List<string> Names { get; set; }

        public List<int> Numbers { get; set; }
    }

    public class InlineListSample
    {
        public int Count { get; set; }

        public List<image> images { get; set; }

        public List<Image> Images { get; set; }
    }

    public class NestedListSample
    {
        public List<image> images { get; set; }

        public List<Image> Images { get; set; }
    }

    public class EmptyListSample
    {
        public List<image> images { get; set; }

        public List<Image> Images { get; set; }
    }

    public class Image
    {
        public string Src { get; set; }

        public string Value { get; set; }
    }

    public class image
    {
        public string Src { get; set; }

        public string Value { get; set; }
    }
}
