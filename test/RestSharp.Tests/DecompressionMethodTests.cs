using System.Net;

namespace RestSharp.Tests; 

public class DecompressionMethodTests {
    [Fact]
    public void ShouldDecompressionMethodsContainsDefaultValues() {
        var restRequest = new RestRequest();

        Assert.True(restRequest.AllowedDecompressionMethods.Contains(DecompressionMethods.None));
        Assert.True(restRequest.AllowedDecompressionMethods.Contains(DecompressionMethods.Deflate));
        Assert.True(restRequest.AllowedDecompressionMethods.Contains(DecompressionMethods.GZip));
    }

    [Fact]
    public void ShouldDecompressionMethodsNotEmptyOrNull() {
        var restRequest = new RestRequest();

        Assert.NotNull(restRequest.AllowedDecompressionMethods);
        Assert.NotEmpty(restRequest.AllowedDecompressionMethods);
    }
}