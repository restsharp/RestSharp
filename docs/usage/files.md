# Working with Files

Here's an example that will use a `Stream` to avoid memory buffering of request content. Useful when retrieving large amounts of data that you will be immediately writing to disk.

```csharp
var tempFile = Path.GetTempFileName();
using var writer = File.OpenWrite(tempFile);

var client = new RestClient(baseUrl);
var request = new RestRequest("Assets/LargeFile.7z");
request.ResponseWriter = responseStream =>
{
    using (responseStream)
    {
        responseStream.CopyTo(writer);
    }
};
var response = client.DownloadData(request);
```