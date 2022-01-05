namespace RestSharp.Tests;

public class ObjectParameterTests {
    [Fact]
    public void Can_Add_Object_With_IntegerArray_property() {
        var request = new RestRequest();
        var items   = new[] { 2, 3, 4 };
        request.AddObject(new { Items = items });
        request.Parameters.First().Should().Be(new GetOrPostParameter("Items", string.Join(",", items)));
    }
}