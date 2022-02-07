using System.Net;

namespace RestSharp.Tests;

public class RestContentTests {
    [Fact]
    public void RestContent_CaseInsensitiveHeaders() {
        var myContentType = "application/x-custom";
        var request = new RestRequest("resource");
        request.AddHeader("coNteNt-TypE", myContentType);
        var client = new RestClient();
        var content = new RequestContent(client, request);

        var httpContent = content.BuildContent();

        Assert.Equal(myContentType, httpContent.Headers.ContentType.MediaType);
    }
}
