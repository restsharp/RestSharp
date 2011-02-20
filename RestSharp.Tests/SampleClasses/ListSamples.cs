using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp.Deserializers;

namespace RestSharp.Tests.SampleClasses
{
	public class InlineListSample
	{
		public int Count { get; set; }
		public List<image> images { get; set; }
		public List<Image> Images { get; set; }
	}

    public class InlineListChildNameMapingSample
    {
        public int Count { get; set; }
        [DeserializeAs(CollectionItemName = "image")]
        public List<NamedImage> Images { get; set; }
    }

	public class NestedListSample
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

    public class NamedImage
    {
        public string Src { get; set; }
        public string Value { get; set; }
    }
}
