using Microsoft.AspNetCore.Http;

namespace RestSharp.Tests.Integrated.Server; 

record TestServerResponse(string Name, string Value);

record UploadRequest(string Filename, IFormFile File);

public record UploadResponse(string FileName, long Length, bool Equal);

record ContentResponse(string Content);
