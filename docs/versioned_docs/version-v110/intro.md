---
sidebar_position: 2
title: Quick start
---

## Introduction

:::warning
RestSharp v107+ changes the library API surface and its behaviour significantly. We advise looking at [migration](/migration) docs to understand how to migrate to the latest version of RestSharp.
:::

The main purpose of RestSharp is to make synchronous and asynchronous calls to remote resources over HTTP. As the name suggests, the main audience of RestSharp are developers who use REST APIs. However, RestSharp can call any API over HTTP, as long as you have the resource URI and request parameters that you want to send comply with W3C HTTP standards.

One of the main challenges of using HTTP APIs for .NET developers is to work with requests and responses of different kinds and translate them to complex C# types. RestSharp can take care of serializing the request body to JSON or XML and deserialize the response. It can also form a valid request URI based on different parameter kinds: path, query, form or body.

## Getting Started

Before you can use RestSharp in your application, you need to add the NuGet package. You can do it using your IDE or the command line:

```
dotnet add package RestSharp
```

### Basic Usage

If you only have a small number of one-off API requests to perform, you can use RestSharp like this:

```csharp
using RestSharp;
using RestSharp.Authenticators;

var options = new RestClientOptions("https://api.twitter.com/1.1") {
    Authenticator = new HttpBasicAuthenticator("username", "password")
};
var client = new RestClient(options);
var request = new RestRequest("statuses/home_timeline.json");
// The cancellation token comes from the caller. You can still make a call without it.
var response = await client.GetAsync(request, cancellationToken);
```

It will return a `RestResponse` back, which contains all the information returned from the remote server.
You have access to the headers, content, HTTP status and more.

You can also use generic overloads like `Get<T>` to automatically deserialize the response into a .NET class.

For example:

```csharp
using RestSharp;
using RestSharp.Authenticators;

var options = new RestClientOptions("https://api.twitter.com/1.1") {
    Authenticator = new HttpBasicAuthenticator("username", "password")
};
var client = new RestClient(options);

var request = new RestRequest("statuses/home_timeline.json");

// The cancellation token comes from the caller. You can still make a call without it.
var timeline = await client.GetAsync<HomeTimeline>(request, cancellationToken);
```

Both snippets above use the `GetAsync` extension, which is a wrapper about `ExecuteGetAsync`, which, in turn, is a wrapper around `ExecuteAsync`.
All `ExecuteAsync` overloads and return the `RestResponse` or `RestResponse<T>`.

The most important difference is that async methods named after HTTP methods (like `GetAsync` or `PostAsync`) return `Task<T>` instead of `Task<RestResponse<T>>`. It means that you won't get an error response if the request fails as those methods throw an exception for unsuccessful HTTP calls. For keeping the API consistent, non-generic functions like `GetAsync` or `PostAsync` also throw an exception if the request fails, although they return the `Task<RestResponse>`.

Read [here](advanced/error-handling.md) about how RestSharp handles exceptions.

RestSharp also offers simple ways to call APIs that accept and return JSON payloads. You can use the `GetJsonAsync` and `PostJsonAsync` extension methods, which will automatically serialize the request body to JSON and deserialize the response to the specified type.

```csharp
var client = new RestClient(options);
var timeline = await client.GetJsonAsync<HomeTimeline>("statuses/home_timeline.json", cancellationToken);
```

Read [here](usage/usage.md#json-requests) about making JSON calls without preparing a request object.

### Content type

RestSharp supports sending XML or JSON body as part of the request. To add a body to the request, simply call `AddJsonBody` or `AddXmlBody` method of the `RestRequest` object.

There is no need to set the `Content-Type` or add the `DataFormat` parameter to the request when using those methods, RestSharp will do it for you.

RestSharp will also handle both XML and JSON responses and perform all necessary deserialization tasks, depending on the server response type. Therefore, you only need to add the `Accept` header if you want to deserialize the response manually.

For example, only you'd only need these lines to make a request with JSON body:

```csharp
var request = new RestRequest("address/update").AddJsonBody(updatedAddress);
var response = await client.PostAsync<AddressUpdateResponse>(request);
```

It's also possible to make the same call using `PostAsync` shorter syntax:

```csharp
var response = await PostJsonAsync<AddressUpdateRequest, AddressUpdateResponse>(
    "address/update", request, cancellationToken
);
```

Read more about serialization and deserialization [here](advanced/serialization.md).

### Response

When you use `ExecuteAsync`, you get an instance of `RestResponse` back. The response object has the `Content` property, which contains the response as string. You can find other useful properties there, like `StatusCode`, `ContentType` and so on. If the request wasn't successful, you'd get a response back with `IsSuccessful` property set to `false` and the error explained in the `ErrorException` and `ErrorMessage` properties.

When using typed `ExecuteAsync<T>`, you get an instance of `RestResponse<T>` back, which is identical to `RestResponse` but also contains the `T Data` property with the deserialized response.

None of `ExecuteAsync` overloads throw if the remote server returns an error. You can inspect the response and find the status code, error message, and, potentially, an exception.

Extensions like `GetAsync<T>` will not return the whole `RestResponse<T>` but just a deserialized response. These extensions will throw an exception if the remote server returns an error. The exception details contain the status code returned by the server.
