namespace RestSharp.Tests.SampleClasses
{
    internal class FooWithArrayProperty
    {
        public string Name { get; set; }

        public Id[] Ids { get; set; }
    }

    internal class Id
    {
        public int Value { get; set; }
    }
}
