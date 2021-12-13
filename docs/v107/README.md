---
title: RestSharp Next (v107)
---

## RestSharp v107

The next version of RestSharp is v107. It's a major upgrade, which contains quite a few breaking changes.

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

The `IRestResponse` interface is deprecated. You get an instance of `RestRequest` or `RestRequest<T>` in return.

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

### Recommended usage

`RestClient` should be thread-safe. It holds an instance of `HttpClient` and `HttpMessageHandler` inside.
Do not instantiate the client for a single call, otherwise you get issues with hanging connections and connection pooling won't be possible.

Do use use case-specific typed API clients. Use a single instance of `RestClient` internally in such an API client for making calls.

Do not use one instance of `RestClient` across different API clients.

### Presumably solved issues

The next RestSharp version presumably solves the following issues:
- Connection pool starvation
- Hanging open TCP connections
- Improper handling of async calls
- Various `SimpleJson` serialization quirks
- HTTP/2 support
- Intermediate certificate issue
- Uploading large files (use file parameters with `Stream`)
- Downloading large files (use `DownloadFileStreanAsync`)
