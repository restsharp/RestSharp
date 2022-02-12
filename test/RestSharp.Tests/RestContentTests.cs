namespace RestSharp.Tests;

public class RestContentTests {
    [Fact]
    public void RestContent_CaseInsensitiveHeaders() {
        const string myContentType = "application/x-custom";

        var request = new RestRequest("resource").AddHeader("coNteNt-TypE", myContentType);
        var content = new RequestContent(new RestClient(), request);

        var httpContent = content.BuildContent();

        httpContent.Headers.ContentType!.MediaType.Should().Be(myContentType);
    }
}
