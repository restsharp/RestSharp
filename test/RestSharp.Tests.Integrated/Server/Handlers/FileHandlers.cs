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
    public async Task<IActionResult> Upload([FromForm] FormFile formFile, [FromQuery] bool checkFile = true) {
        var file = formFile.File;

        if (!checkFile) {
            return Ok(new UploadResponse(file.FileName, file.Length, true));
        }

        var assetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");

        await using var stream = file.OpenReadStream();

        var received = await stream.ReadAsBytes(default);

        try {
            var expected = await System.IO.File.ReadAllBytesAsync(Path.Combine(assetPath, file.FileName));
            var response = new UploadResponse(file.FileName, file.Length, received.SequenceEqual(expected));
            return Ok(response);
        }
        catch (Exception e) {
            return BadRequest(new { Message = e.Message, Filename = file.FileName });
        }
    }
}

public class FormFile {
    public IFormFile File { get; set; } = null!;
}
