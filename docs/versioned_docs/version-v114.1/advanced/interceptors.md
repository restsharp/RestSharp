---
title: Interceptors
---

## Intercepting requests and responses

Interceptors are a powerful feature of RestSharp that allows you to modify requests and responses before they are sent or received. You can use interceptors to add headers, modify the request body, or even cancel the request. You can also use interceptors to modify the response before it is returned to the caller.

### Implementing an interceptor

To implement an interceptor, you need to create a class that inherits the `Interceptor` base class. The base class implements all interceptor methods as virtual, so you can override them in your derived class.

Methods that you can override are:
- `BeforeRequest(RestRequest request, CancellationToken cancellationToken)`
- `AfterRequest(RestResponse response, CancellationToken cancellationToken)`
- `BeforeHttpRequest(HttpRequestMessage requestMessage, CancellationToken cancellationToken)`
- `AfterHttpRequest(HttpResponseMessage responseMessage, CancellationToken cancellationToken)`
- `BeforeDeserialization(RestResponse response, CancellationToken cancellationToken)`

All those functions must return a `ValueTask` instance.

Here's an example of an interceptor that adds a header to a request:

```csharp
// This interceptor adds a header to the request
// You'd not normally use this interceptor, as RestSharp already has a method 
// to add headers to the request
class HeaderInterceptor(string headerName, string headerValue) : Interceptors.Interceptor {
    public override ValueTask BeforeHttpRequest(HttpRequestMessage requestMessage, CancellationToken cancellationToken) {
        requestMessage.Headers.Add(headerName, headerValue);
        return ValueTask.CompletedTask;
    }
}
```

Because interceptor functions return `ValueTask`, you can use `async` and `await` inside them.

### Using an interceptor

It's possible to add as many interceptors as you want, both to the client and to the request. The interceptors are executed in the order they were added.

Adding interceptors to the client is done via the client options:

```csharp
var options = new RestClientOptions("https://api.example.com") {
    Interceptors = [new HeaderInterceptor("Authorization", token)]
};
var client = new RestClient(options);
```

When you add an interceptor to the client, it will be executed for every request made by that client.

You can also add an interceptor to a specific request:

```csharp
var request = new RestRequest("resource") {
    Interceptors = [new HeaderInterceptor("Authorization", token)]
};
```

In this case, the interceptor will only be executed for that specific request.

### Deprecation notice

Interceptors aim to replace the existing request hooks available in RestSharp prior to version 111.0. Those hooks are marked with `Obsolete` attribute and will be removed in the future. If you are using those hooks, we recommend migrating to interceptors as soon as possible.

To make the migration easier, RestSharp provides a class called `CompatibilityInterceptor`. It has properties for the hooks available in RestSharp 110.0 and earlier. You can use it to migrate your code to interceptors without changing the existing logic.

For example, a code that uses `OnBeforeRequest` hook:

```csharp
var request = new RestRequest("success");
request.OnBeforeDeserialization += _ => throw new Exception(exceptionMessage);
```

Can be migrated to interceptors like this:

```csharp
var request = new RestRequest("success") {
    Interceptors = [new CompatibilityInterceptor {
        OnBeforeDeserialization = _ => throw new Exception(exceptionMessage)
    }]
};
```