using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestSharp.Extensions;

namespace RestSharp.Tests.Integrated.Server.Handlers;

public static class FileHandlers {
    public static async Task<IResult> HandleUpload(string assetPath, HttpRequest req) {
        if (!req.HasFormContentType) {
            return Results.BadRequest("It's not a form");
        }

        var form = await req.ReadFormAsync();
        var file = form.Files["file"];

        if (file is null) {
            return Results.BadRequest("File parameter 'file' is not present");
        }

        await using var stream = file.OpenReadStream();

        var received = await stream.ReadAsBytes(default);
        var expected = await File.ReadAllBytesAsync(Path.Combine(assetPath, file.FileName));

        var response = new UploadResponse(file.FileName, file.Length, received.SequenceEqual(expected));
        return Results.Json(response);
    }
}

[ApiController]
public class UploadController : ControllerBase {
    [HttpPost]
    [Route("upload")]
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
    public IFormFile File { get; set; }
}
