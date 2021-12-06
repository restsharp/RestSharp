namespace RestSharp.Tests; 

public class SimpleJsonTests {
    [Fact]
    public void EscapeToJavascriptString_should_not_double_escape() {
        var preformattedString = "{ \"name\" : \"value\" }";
        var expectedSlashCount = preformattedString.Count(x => x == '\\');

        var result           = SimpleJson.EscapeToJavascriptString(preformattedString);
        var actualSlashCount = result.Count(x => x == '\\');

        Assert.Equal(expectedSlashCount, actualSlashCount);
    }

    [Fact]
    public void SerializeObject_should_not_assume_strings_wrapped_in_curly_braces_are_json() {
        var objectWithCurlyString = new { Name = "{value}" };

        var result = SimpleJson.SerializeObject(objectWithCurlyString);

        Assert.Equal("{\"Name\":\"{value}\"}", result);
    }
}