---
title: RestSharp Next (v107)
---

## RestSharp v107

The latest version of RestSharp is v107. It's a major upgrade, which contains quite a few breaking changes.

The most important change is that RestSharp stop using the legacy `HttpWebRequest` class, and uses well-known 'HttpClient' instead.
This move solves lots of issues, like hanging connections due to improper `HttpClient` instance cache, updated protocols support, and many other problems.

Another big change is that `SimpleJson` is retired completely from the code base. Instead, RestSharp uses `JsonSerializer` from the `System.Text.Json` package, which is the default serializer for ASP.NET Core.

Finally, most of the interfaces are now gone.

## Brief migration guide

### RestClient and options

The `IRestClient` interface is deprecated. You will be using the `RestClient` class instance.

Most of the client options are moved to `RestClientOptions`. If you can't find the option you used to set on `IRestClient`, check the options, it's probably there.

This is how you can instantiate the client using the simplest possible way:

```csharp
var client = new RestClient("https://api.myorg.com");
```

For customizing the client, use `RestClientOptions`:

```csharp
var options = new RestClientOptions("https://api.myorg.com") {
    ThrowOnAnyError = true,
    Timeout = 1000
};
var client = new RestClient(options);
```

You can still change serializers and add default parameters to the client.

### Making requests

The `IRestRequest` interface is deprecated. You will be using the `RestRequest` class instance.

You can still create a request as before:

```csharp
var request = new RestRequest();
```

Adding parameters hasn't change much, except you cannot add cookie parameters to the request. It's because cookies are added to the `HttpMessageHandler` cookie container, which is not accessible inside the request class.

```csharp
var request = new RestRequest()
    .AddQueryParameter("foo", "bar")
    .AddJsonBody(someObject);
```

Quite a few options previously available via `IRestRequest` are now in `RestClientOptions`. It's also because changing those options forced us to use a different HTTP message handler, and it caused hanging connections, etc.

When you got a request instance, you can make a call:

```csharp
var request = new RestRequest()
    .AddQueryParameter("foo", "bar")
    .AddJsonBody(someObject);
var response = await client.PostAsync<MyResponse>(request, cancellationToken);
```

All the synchronous methods are gone. If you absolutely must call without using `async` and `await`, use `GetAwaiter().GetResult()` blocking call.

The `IRestResponse` interface is deprecated. You get an instance of `RestResponse` or `RestResponse<T>` in return.

You can also use a simplified API for making POST and PUT requests:

```csharp
var request = new MyRequest { Data = "foo" };
var response = await client.PostAsync<MyRequest, MyResponse>(request, cancellationToken);
// response will be of type TResponse
```

This way you avoid instantiating `RestRequest` explicitly.

### Using your own HttpClient

`RestClient` class has two constructors, which accept either `HttpClient` or `HttpMessageHandler` instance.

This way you can use a pre-configured `HttpClient` or `HttpMessageHandler`, customized for your needs.

### Default serializers

For JSON, RestSharp will use `JsonSerializer` from the `System.Text.Json` package. This package is now referenced by default, and it is the only dependency of the RestSharp NuGet package.

The `Utf8` serializer package is deprecated as the package is not being updated.

For XML requests and responses RestSharp uses `DotNetXmlSerializer` and `DotNetXmlDeserializer`.
Previously used default `XmlSerializer`, `XmlDeserializer`, and `XmlAttrobuteDeserializer` are moved to a separate package `RestSharp.Serializers.Xml`.

### NTLM authentication

The `NtlmAuthenticator` is deprecated.

NTLM authenticator was doing nothing more than telling `WebRequest` to use certain credentials. Now with RestSharp, the preferred way would be to set the `Credentials` or `UseDefaultCredentials` property in `RestClientOptions`.

The reason to remove it was that all other authenticators use `AuthenticatorBase`, which must return a parameter. In general, any authenticator is given a request before its made, so it can do something with it. NTLM doesn't work this way, it needs some settings to be provided for `HttpClientHandler`, which is set up before the `HttpClient` instance is created, and it happens once per RestClient instance, and it cannot be changed per request.

### Delegating handlers

You can easily build your own request/response pipeline, as you would with `HttpClient`. RestClient will create an `HttpMessageHandler` instance for its own use, using the options provided. You can, of course, provide your own instance of `HttpMessageHandler` as `RestSharpClient` has a constructor that accepts a custom handler and uses it to create an `HttpClient` instance. However, you'll be on your own with the handler configuration in this case.

If you want to build a _pipeline_, use [delegating handlers](https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/httpclient-message-handlers). For example, you can use `HttpTracer` to [debug your HTTP calls](https://github.com/BSiLabs/HttpTracer) like this:

```csharp
var options = new RestClientOptions(_server.Url) {
    ConfigureMessageHandler = handler => 
        new HttpTracerHandler(handler, new ConsoleLogger(), HttpMessageParts.All)
};
var client = new RestClient(options);
```

## Recommended usage

`RestClient` should be thread-safe. It holds an instance of `HttpClient` and `HttpMessageHandler` inside.
Do not instantiate the client for a single call, otherwise you get issues with hanging connections and connection pooling won't be possible.

Do create typed API clients for your use cases. Use a single instance of `RestClient` internally in such an API client for making calls.
It would be similar to using typed clients using `HttpClient`, for example:

```csharp
public class GitHubClient {
    readonly RestClient _client;

    public GitHubClient() {
        _client = new RestClient("https://api.github.com/")
            .AddDefaultHeader(KnownHeaders.Accept, "application/vnd.github.v3+json");
    }

    public Task<GitHubRepo[]> GetRepos()
        => _client.GetAsync<GitHubRepo[]>("users/aspnet/repos");
}
```

Do not use one instance of `RestClient` across different API clients.

## Presumably solved issues

The next RestSharp version presumably solves the following issues:
- Connection pool starvation
- Hanging open TCP connections
- Improper handling of async calls
- Various `SimpleJson` serialization quirks
- HTTP/2 support
- Intermediate certificate issue
- Uploading large files (use file parameters with `Stream`)
- Downloading large files (use `DownloadFileStreanAsync`)

## Deprecated interfaces

The following interfaces are removed from RestSharp:
- `IRestClient`
- `IRestRequest`
- `IRestResponse`
- `IHttp`

### Motivation

All the deprecated interfaces had only one implementation in RestSharp, so those interfaces were abstracting nothing. It is now unclear what was the purpose for adding those interfaces initially.

What about mocking it, you might ask? The answer is: what would you do if you use a plain `HttpClient` instance? It doesn't implement any interface for the same reason - there's nothing to abstract, and there's only one implementation. We don't recommend mocking `RestClient` in your tests when you are testing against APIs that are controlled by you or people in your organisation. Test your clients against the real thing, as REST calls are I/O-bound. Mocking REST calls is like mocking database calls, and lead to a lot of issues in production even if all your tests pass against mocks.

As mentioned in [Recommended usage](#recommended-usage), we advise against using `RestClient` in the application code, and advocate wrapping it inside particular API client classes. Those classes would be under your control, and you are totally free to use interfaces there. If you absolutely must mock, you can mock your interfaces instead.

### Mocking

Mocking an infrastructure component like RestSharp (or HttpClient) is not the best idea. Even if you check that all the parameters are added correctly to the request, your "unit test" will only give you a false sense of safety that your code actually works. But, you have no guarantee that the remote server will accept your request, or if you can handle the actual response correctly.

The best way to test HTTP calls is to make some, using the actual service you call. However, you might still want to check if your API client forms requests in a certain way. You might also be sure about what the remote server responds to your calls with, so you can build a set of JSON (or XML) responses, so you can simulate remote calls.

It is perfectly doable without using interfaces. As RestSharp uses `HttpClient` internally, it certainly uses `HttpMessageHandler`. Features like delegating handlers allow you to intersect the request pipeline, inspect the request, and substitute the response. You can do it yourself, or use a library like [MockHttp](https://github.com/richardszalay/mockhttp). They have an example provided in the repository README, so we have changed it for RestClient here:

```csharp
var mockHttp = new MockHttpMessageHandler();

// Setup a respond for the user api (including a wildcard in the URL)
mockHttp.When("http://localhost/api/user/*")
        .Respond("application/json", "{'name' : 'Test McGee'}"); // Respond with JSON

// Instantiate the client normally, but replace the message handler
var client = new RestClient(...) { ConfigureMessageHandler = _ => mockHttp };

var request = new RestRequest("http://localhost/api/user/1234");
var response = await client.GetAsync(request);

// No network connection required
Console.Write(response.Content); // {'name' : 'Test McGee'}
```

### Reference

Below, you can find members of `IRestClient` and `IRestRequest` with their corresponding status and location in the new API.

| `IRestClient` member                                                                            | Where is it now?                   |
|:------------------------------------------------------------------------------------------------|:-----------------------------------|
| `CookieContainer`                                                                               | `RestClient`                       |
| `AutomaticDecompression`                                                                        | `RestClientOptions`, changed type  |
| `MaxRedirects`                                                                                  | `RestClientOptions`                |
| `UserAgent`                                                                                     | `RestClientOptions`                |
| `Timeout`                                                                                       | `RestClientOptions`, `RestRequest` |
| `Authenticator`                                                                                 | `RestClient`                       |
| `BaseUrl`                                                                                       | `RestClientOptions`                |
| `Encoding`                                                                                      | `RestClientOptions`                |
| `ThrowOnDeserializationError`                                                                   | `RestClientOptions`                |
| `FailOnDeserializationError`                                                                    | `RestClientOptions`                |
| `ThrowOnAnyError`                                                                               | `RestClientOptions`                |
| `PreAuthenticate`                                                                               | `RestClientOptions`                |
| `BaseHost`                                                                                      | `RestClientOptions`                |
| `AllowMultipleDefaultParametersWithSameName`                                                    | `RestClientOptions`                |
| `ClientCertificates`                                                                            | `RestClientOptions`                |
| `Proxy`                                                                                         | `RestClientOptions`                |
| `CachePolicy`                                                                                   | `RestClientOptions`, changed type  |
| `FollowRedirects`                                                                               | `RestClientOptions`                |
| `RemoteCertificateValidationCallback`                                                           | `RestClientOptions`                |
| `Pipelined`                                                                                     | Not supported                      |
| `UnsafeAuthenticatedConnectionSharing`                                                          | Not supported                      |
| `ConnectionGroupName`                                                                           | Not supported                      |
| `ReadWriteTimeout`                                                                              | Not supported                      |
| `UseSynchronizationContext`                                                                     | Not supported                      |
| `DefaultParameters`                                                                             | `RestClient`                       |
| `UseSerializer(Func<IRestSerializer> serializerFactory)`                                        | `RestClient`                       |
| `UseSerializer<T>()`                                                                            | `RestClient`                       |
| `Deserialize<T>(IRestResponse response)`                                                        | `RestClient`                       |
| `BuildUri(IRestRequest request)`                                                                | `RestClient`                       |
| `UseUrlEncoder(Func<string, string> encoder)`                                                   | Extension                          |
| `UseQueryEncoder(Func<string, Encoding, string> queryEncoder)`                                  | Extension                          |
| `ExecuteAsync<T>(IRestRequest request, CancellationToken cancellationToken)`                    | `RestClient`                       |
| `ExecuteAsync<T>(IRestRequest request, Method httpMethod, CancellationToken cancellationToken)` | Extension                          |
| `ExecuteAsync(IRestRequest request, Method httpMethod, CancellationToken cancellationToken)`    | Extension                          |
| `ExecuteAsync(IRestRequest request, CancellationToken cancellationToken)`                       | Extension                          |
| `ExecuteGetAsync<T>(IRestRequest request, CancellationToken cancellationToken)`                 | Extension                          |
| `ExecutePostAsync<T>(IRestRequest request, CancellationToken cancellationToken)`                | Extension                          |
| `ExecuteGetAsync(IRestRequest request, CancellationToken cancellationToken)`                    | Extension                          |
| `ExecutePostAsync(IRestRequest request, CancellationToken cancellationToken)`                   | Extension                          |
| `Execute(IRestRequest request)`                                                                 | Deprecated                         |
| `Execute(IRestRequest request, Method httpMethod)`                                              | Deprecated                         |
| `Execute<T>(IRestRequest request)`                                                              | Deprecated                         |
| `Execute<T>(IRestRequest request, Method httpMethod)`                                           | Deprecated                         |
| `DownloadData(IRestRequest request)`                                                            | Deprecated                         |
| `ExecuteAsGet(IRestRequest request, string httpMethod)`                                         | Deprecated                         |
| `ExecuteAsPost(IRestRequest request, string httpMethod)`                                        | Deprecated                         |
| `ExecuteAsGet<T>(IRestRequest request, string httpMethod)`                                      | Deprecated                         |
| `ExecuteAsPost<T>(IRestRequest request, string httpMethod)`                                     | Deprecated                         |
| `BuildUriWithoutQueryParameters(IRestRequest request)`                                          | Removed                            |
| `ConfigureWebRequest(Action<HttpWebRequest> configurator)`                                      | Removed                            |
| `AddHandler(string contentType, Func<IDeserializer> deserializerFactory)`                       | Removed                            |
| `RemoveHandler(string contentType)`                                                             | Removed                            |
| `ClearHandlers()`                                                                               | Removed                            |

| `IRestRequest` member                                                                                  | Where is it now?                 |
|:-------------------------------------------------------------------------------------------------------|:---------------------------------|
| `AlwaysMultipartFormData`                                                                              | `RestRequest`                    |
| `JsonSerializer`                                                                                       | Deprecated                       |
| `XmlSerializer`                                                                                        | Deprecated                       |
| `AdvancedResponseWriter`                                                                               | `RestRequest`, changed signature |
| `ResponseWriter`                                                                                       | `RestRequest`, changed signature |
| `Parameters`                                                                                           | `RestRequest`                    |
| `Files`                                                                                                | `RestRequest`                    |
| `Method`                                                                                               | `RestRequest`                    |
| `Resource`                                                                                             | `RestRequest`                    |
| `RequestFormat`                                                                                        | `RestRequest`                    |
| `RootElement`                                                                                          | `RestRequest`                    |
| `DateFormat`                                                                                           | `XmlRequest`                     |
| `XmlNamespace`                                                                                         | `XmlRequest`                     |
| `Credentials`                                                                                          | Removed, use `RestClientOptions` |
| `Timeout`                                                                                              | `RestRequest`                    |
| `ReadWriteTimeout`                                                                                     | Not supported                    |
| `Attempts`                                                                                             | `RestRequest`                    |
| `UseDefaultCredentials`                                                                                | Removed, use `RestClientOptions` |
| `AllowedDecompressionMethods`                                                                          | Removed, use `RestClientOptions` |
| `OnBeforeDeserialization`                                                                              | `RestRequest`                    |
| `OnBeforeRequest`                                                                                      | `RestRequest`, changed signature |
| `Body`                                                                                                 | Removed, use `Parameters`        |
| `AddParameter(Parameter p)`                                                                            | `RestRequest`                    |
| `AddFile(string name, string path, string contentType)`                                                | Extension                        |
| `AddFile(string name, byte[] bytes, string fileName, string contentType)`                              | Extension                        |
| `AddFile(string name, Action<Stream> writer, string fileName, long contentLength, string contentType)` | Extension                        |
| `AddFileBytes(string name, byte[] bytes, string filename, string contentType)`                         | Extension `AddFile`              |
| `AddBody(object obj, string xmlNamespace)`                                                             | Deprecated                       |
| `AddBody(object obj)`                                                                                  | Extension                        |
| `AddJsonBody(object obj)`                                                                              | Extension                        |
| `AddJsonBody(object obj, string contentType)`                                                          | Extension                        |
| `AddXmlBody(object obj)`                                                                               | Extension                        |
| `AddXmlBody(object obj, string xmlNamespace)`                                                          | Extension                        |
| `AddObject(object obj, params string[] includedProperties)`                                            | Extension                        |
| `AddObject(object obj)`                                                                                | Extension                        |
| `AddParameter(string name, object value)`                                                              | Extension                        |
| `AddParameter(string name, object value, ParameterType type)`                                          | Extension                        |
| `AddParameter(string name, object value, string contentType, ParameterType type)`                      | Extension                        |
| `AddOrUpdateParameter(Parameter parameter)`                                                            | Extension                        |
| `AddOrUpdateParameters(IEnumerable<Parameter> parameters)`                                             | Extension                        |
| `AddOrUpdateParameter(string name, object value)`                                                      | Extension                        |
| `AddOrUpdateParameter(string name, object value, ParameterType type)`                                  | Extension                        |
| `AddOrUpdateParameter(string name, object value, string contentType, ParameterType type)`              | Extension                        |
| `AddHeader(string name, string value)`                                                                 | Extension                        |
| `AddOrUpdateHeader(string name, string value)`                                                         | Extension                        |
| `AddHeaders(ICollection<KeyValuePair<string, string>> headers)`                                        | Extension                        |
| `AddOrUpdateHeaders(ICollection<KeyValuePair<string, string>> headers)`                                | Extension                        |
| `AddCookie(string name, string value)`                                                                 | Extension                        |
| `AddUrlSegment(string name, string value)`                                                             | Extension                        |
| `AddUrlSegment(string name, string value, bool encode)`                                                | Extension                        |
| `AddUrlSegment(string name, object value)`                                                             | Extension                        |
| `AddQueryParameter(string name, string value)`                                                         | Extension                        |
| `AddQueryParameter(string name, string value, bool encode)`                                            | Extension                        |
| `AddDecompressionMethod(DecompressionMethods decompressionMethod)`                                     | Not supported                    |
| `IncreaseNumAttempts()`                                                                                | Made internal                    |
