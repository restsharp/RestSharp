namespace RestSharp.Tests.Integrated.Server;

record TestServerResponse(string Name, string Value);

public record UploadResponse(string FileName, long Length, bool Equal);