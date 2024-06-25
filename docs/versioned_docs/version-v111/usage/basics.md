---
sidebar_position: 2
---

# RestSharp basics

This page describes some of the essential properties and features of RestSharp.

## What RestSharp does

Essentially, RestSharp is a wrapper around `HttpClient` that allows you to do the following:
- Add default parameters of any kind (not just headers) to the client, once
- Add parameters of any kind to each request (query, URL segment, form, attachment, serialized body, header) in a straightforward way
- Serialize the payload to JSON or XML if necessary
- Set the correct content headers (content type, disposition, length, etc.)
- Handle the remote endpoint response
- Deserialize the response from JSON or XML if necessary

## API client

The best way to call an external HTTP API is to create a typed client, which encapsulates RestSharp calls and doesn't expose the `RestClient` instance in public.

You can find an example of a Twitter API client on the [Example](example.md) page.


## Handling responses

All `Execute{Method}Async` functions return an instance of `RestResponse`. Similarly, `Execute{Method}Async<T>` return a generic instance of `RestResponse<T>` where `T` is the response object type.

Response object contains the following properties:

| Property                 | Type                                                | Description                                                                                                                  |
|--------------------------|-----------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------|
| `Request`                | `RestRequest`                                       | Request instance that was used to get the response.                                                                          |
| `ContentType`            | `string?`                                           | Response content type. `Null` if response has no content.                                                                    |
| `ContentLength`          | `long?`                                             | Response content length. `Null` if response has no content.                                                                  |
| `ContentEncoding`        | `ICollection<string>`                               | Content encoding collection. Empty if response has no content.                                                               |
| `Content`                | `string?`                                           | Response content as string. `Null` if response has no content.                                                               |
| `IsSuccessfulStatusCode` | `bool`                                              | Indicates if response was successful, so no errors were reported by the server. Note that `404` response code means success. |
| `ResponseStatus`         | `None`, `Completed`, `Error`, `TimedOut`, `Aborted` | Response completion status. Note that completed responses might still return errors.                                         |
| `IsSuccessful`           | `bool`                                              | `True` when `IsSuccessfulStatusCode` is `true` and `ResponseStatus` is `Completed`.                                          |
| `StatusDescription`      | `string?`                                           | Response status description, if available.                                                                                   |
| `RawBytes`               | `byte[]?`                                           | Response content as byte array. `Null` if response has no content.                                                           |
| `ResponseUri`            | `Uri?`                                              | URI of the response, which might be different from request URI in case of redirects.                                         |
| `Server`                 | `string?`                                           | Server header value of the response.                                                                                         |
| `Cookies`                | `CookieCollection?`                                 | Collection of cookies received with the response, if any.                                                                    |
| `Headers`                | Collection of `HeaderParameter`                     | Response headers.                                                                                                            |
| `ContentHeaders`         | Collection of `HeaderParameter`                     | Response content headers.                                                                                                    |
| `ErrorMessage`           | `string?`                                           | Transport or another non-HTTP error generated while attempting request.                                                      |
| `ErrorException`         | `Exception?`                                        | Exception thrown when executing the request, if any.                                                                         |
| `Version`                | `Version?`                                          | HTTP protocol version of the request.                                                                                        |
| `RootElement`            | `string?`                                           | Root element of the serialized response content, only works if deserializer supports it.                                     |

In addition, `RestResponse<T>` has one additional property:

| Property | Type | Description                                                                                                                                               |
|----------|------|-----------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Data`   | `T?` | Deserialized response object. `Null` if there's no content in the response, deserializer failed to understand the response content, or if request failed. |

### JSON streaming APIs

For HTTP API endpoints that stream the response data (like [Twitter search stream](https://developer.twitter.com/en/docs/twitter-api/tweets/filtered-stream/api-reference/get-tweets-search-stream)) you can use RestSharp with `StreamJsonAsync<T>`, which returns an `IAsyncEnumerable<T>`:

```csharp
public async IAsyncEnumerable<SearchResponse> SearchStream(
    [EnumeratorCancellation] CancellationToken cancellationToken = default
) {
    var response = _client.StreamJsonAsync<TwitterSingleObject<SearchResponse>>(
        "tweets/search/stream", cancellationToken
    );

    await foreach (var item in response.WithCancellation(cancellationToken)) {
        yield return item.Data;
    }
}
```

The main limitation of this function is that it expects each JSON object to be returned as a single line. It is unable to parse the response by combining multiple lines into a JSON string.

### Uploading files

To add a file to the request you can use the `RestRequest` function called `AddFile`. The main function accepts the `FileParameter` argument:

```csharp
request.AddFile(fileParameter);
```

You can instantiate the file parameter using `FileParameter.Create` that accepts a bytes array, or `FileParameter.FromFile`, which will load the file from disk.

There are also extension functions that wrap the creation of `FileParameter` inside:

```csharp
// Adds a file from disk
AddFile(parameterName, filePath, contentType);

// Adds an array of bytes
AddFile(parameterName, bytes, fileName, contentType);

// Adds a stream returned by the getFile function
AddFile(parameterName, getFile, fileName, contentType);
```

Remember that `AddFile` will set all the necessary headers, so please don't try to set content headers manually.

You can also provide file upload options to the `AddFile` call. The options are:
- `DisableFilenameEncoding` (default `false`): if set to `true`, RestSharp will not encode the file name in the `Content-Disposition` header
- `DisableFilenameStar` (default `true`): if set to `true`, RestSharp will not add the `filename*` parameter to the `Content-Disposition` header

Example of using the options:

```csharp
var options = new FileParameterOptions {
    DisableFilenameEncoding = true,
    DisableFilenameStar = false
};
request.AddFile("file", filePath, options: options);
```

The options specified in the snippet above usually help when you upload files with non-ASCII characters in their names.

### Downloading binary data

There are two functions that allow you to download binary data from the remote API.

First, there's `DownloadDataAsync`, which returns `Task<byte[]`. It will read the binary response to the end, and return the whole binary content as a byte array. It works well for downloading smaller files.

For larger responses, you can use `DownloadStreamAsync` that returns `Task<Stream>`. This function allows you to open a stream reader and asynchronously stream large responses to memory or disk.


## Reusing HttpClient

RestSharp uses `HttpClient` internally to make HTTP requests. It's possible to reuse the same `HttpClient` instance for multiple `RestClient` instances. This is useful when you want to share the same connection pool between multiple `RestClient` instances.

One way of doing it is to use `RestClient` constructors that accept an instance of `HttpClient` or `HttpMessageHandler` as an argument. Note that in that case not all the options provided via `RestClientOptions` will be used. Here is the list of options that will work:

- `BaseAddress` will be used to set the base address of the `HttpClient` instance if base address is not set there already.
- `MaxTimeout`
- `UserAgent` will be added to the `RestClient.DefaultParameters` list as a HTTP header. This will be added to each request made by the `RestClient`, and the `HttpClient` instance will not be modified. This is to allow the `HttpClient` instance to be reused for scenarios where different `User-Agent` headers are required.
- `Expect100Continue`

Another option is to use a simple HTTP client factory as described [above](#simple-factory). 

## Blazor support

Inside a Blazor webassembly app, you can make requests to external API endpoints. Microsoft examples show how to do it with `HttpClient`, and it's also possible to use RestSharp for the same purpose.

You need to remember that webassembly has some platform-specific limitations. Therefore, you won't be able to instantiate `RestClient` using all of its constructors. In fact, you can only use `RestClient` constructors that accept `HttpClient` or `HttpMessageHandler` as an argument. If you use the default parameterless constructor, it will call the option-based constructor with default options. The options-based constructor will attempt to create an `HttpMessageHandler` instance using the options provided, and it will fail with Blazor, as some of those options throw thw "Unsupported platform" exception.

Here is an example how to register the `RestClient` instance globally as a singleton:

```csharp
builder.Services.AddSingleton(new RestClient(new HttpClient()));
```

Then, on a page you can inject the instance:

```html
@page "/fetchdata"
@using RestSharp
@inject RestClient _restClient
```

And then use it:

```csharp
@code {
    private WeatherForecast[]? forecasts;

    protected override async Task OnInitializedAsync() {
        forecasts = await _restClient.GetJsonAsync<WeatherForecast[]>("http://localhost:5104/weather");
    }

    public class WeatherForecast {
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public string? Summary { get; set; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
```

In this case, the call will be made to a WebAPI server hosted at `http://localhost:5104/weather`. Remember that if the WebAPI server is not hosting the webassembly itself, it needs to have a CORS policy configured to allow the webassembly origin to access the API endpoint from the browser.
