namespace RestSharp.Tests;

public class ParameterValidationTests {
    [Fact]
    public void RestRequest_AlwaysMultipartFormData_IsAllowed() {
        var request = new RestRequest { AlwaysMultipartFormData = true };
        request.ValidateParameters();
    }

    [Fact]
    public void RestRequest_AlwaysSingleFileAsContent_IsAllowed() {
        var request = new RestRequest { AlwaysSingleFileAsContent = true };
        request.ValidateParameters();
    }

    [Fact]
    public void RestRequest_AlwaysSingleFileAsContent_And_AlwaysMultipartFormData_IsNotAllowed() {
        var request = new RestRequest {
            AlwaysSingleFileAsContent = true,
            AlwaysMultipartFormData   = true
        };
        Assert.Throws<ArgumentException>(() => request.ValidateParameters());
    }

    [Fact]
    public void RestRequest_AlwaysSingleFileAsContent_And_PostParameters_IsNotAllowed() {
        var request = new RestRequest {
            Method                    = Method.Post,
            AlwaysSingleFileAsContent = true,
        };

        request.AddParameter("name", "value", ParameterType.GetOrPost);
        Assert.Throws<ArgumentException>(() => request.ValidateParameters());
    }

    [Fact]
    public void RestRequest_AlwaysSingleFileAsContent_And_BodyParameters_IsNotAllowed() {
        var request = new RestRequest { AlwaysSingleFileAsContent = true, };
        request.AddParameter("name", "value", ParameterType.RequestBody);
        Assert.Throws<ArgumentException>(() => request.ValidateParameters());
    }
}