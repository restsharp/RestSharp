using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestSharp.Extensions;

namespace RestSharp.Tests.Integrated.Server.Handlers;

[ApiController]
public class UploadController : ControllerBase {
    [HttpPost]
    [Route("upload")]
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public async Task<UploadResponse> Upload([FromForm] FormFile formFile) {
        var assetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
        var file      = formFile.File;

        await using var stream = file.OpenReadStream();

        var received = await stream.ReadAsBytes(default);
        var expected = await System.IO.File.ReadAllBytesAsync(Path.Combine(assetPath, file.FileName));

        var response = new UploadResponse(file.FileName, file.Length, received.SequenceEqual(expected));
        return response;
    }
}

public class FormFile {
    public IFormFile File { get; set; } = null!;
}
