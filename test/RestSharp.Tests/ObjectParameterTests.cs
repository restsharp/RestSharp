namespace RestSharp.Tests;

public class ObjectParameterTests {
    [Fact]
    public void Can_Add_Object_With_IntegerArray_property() {
        var request = new RestRequest();
        request.AddObject(new { Items = new[] { 2, 3, 4 } });
    }
}