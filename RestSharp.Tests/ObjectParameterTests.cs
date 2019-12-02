using NUnit.Framework;

namespace RestSharp.Tests
{
    public class ObjectParameterTests
    {
        [Test]
        public void Can_Add_Object_With_IntegerArray_property()
        {
            var request = new RestRequest();

            Assert.DoesNotThrow(() => request.AddObject(new {Items = new[] {2, 3, 4}}));
        }
    }
}