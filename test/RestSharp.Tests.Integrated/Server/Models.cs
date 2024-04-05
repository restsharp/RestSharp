namespace RestSharp.Tests.Integrated.Server;

record TestServerResponse(string Name, string Value);

public record UploadResponse(string FileName, long Length, bool Equal);

public record SuccessResponse(string Message);

public class TestResponse {
    public string Message { get; set; } = null!;
}