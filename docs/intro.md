---
title: Quick start
---

## Introduction

::: warning
RestSharp v107 changes the library API surface and its behaviour significantly. We advise looking at [v107](/v107/) docs to understand how to migrate to the latest version of RestSharp.
:::

The main purpose of RestSharp is to make synchronous and asynchronous calls to remote resources over HTTP. As the name suggests, the main audience of RestSharp are developers who use REST APIs. However, RestSharp can call any API over HTTP, as long as you have the resource URI and request parameters that you want to send comply with W3C HTTP standards.

One of the main challenges of using HTTP APIs for .NET developers is to work with requests and responses of different kinds and translate them to complex C# types. RestSharp can take care of serializing the request body to JSON or XML and deserialize the response. It can also form a valid request URI based on different parameter kinds - path, query, form or body.

## Getting Started

Before you can use RestSharp in your application, you need to add the NuGet package. You can do it using your IDE or the command line:

```
dotnet add package RestSharp
```

### Basic Usage

If you only have a few number of one-off requests to make to an API, you can use RestSharp like so:

```csharp
using RestSharp;
using RestSharp.Authenticators;

var client = new RestClient("https://api.twitter.com/1.1") {
    Authenticator = new HttpBasicAuthenticator("username", "password")
};
var request = new RestRequest("statuses/home_timeline.json");
var response = await client.GetAsync(request, cancellationToken);
```

It will return a `RestResponse` back, which contains all the information returned from the remote server.
You have access to the headers, content, HTTP status and more.

We recommend using the generic overloads like `Get<T>` to automatically deserialize the response into .NET classes.

For example:

```csharp
using RestSharp;
using RestSharp.Authenticators;

var client = new RestClient("https://api.twitter.com/1.1");
client.Authenticator = new HttpBasicAuthenticator("username", "password");

var request = new RestRequest("statuses/home_timeline.json", DataFormat.Json);

var timeline = await client.GetAsync<HomeTimeline>(request, cancellationToken);
```

The most important difference, however, that async methods that are named after HTTP methods return the `Task<T>` instead of `Task<IRestResponse<T>>`. Because it means that you won't get an error response if the request fails, those methods
throw an exception.

All `ExecuteAsync` overloads, however, behave in the same way as `Execute` and return the `IRestResponse` or `IRestResponse<T>`.

Read [here](error-handling.md) about how RestSharp handles exceptions.

### Content type

RestSharp supports sending XML or JSON body as part of the request. To add a body to the request, simply call `AddJsonBody` or `AddXmlBody` method of the `IRestRequest` instance.

There is no need to set the `Content-Type` or add the `DataFormat` parameter to the request when using those methods, RestSharp will do it for you.

RestSharp will also handle both XML and JSON responses and perform all necessary deserialization tasks, depending on the server response type. Therefore, you only need to add the `Accept` header if you want to deserialize the response manually.

For example, only you'd only need these lines to make a request with JSON body:

```csharp
var request = new RestRequest("address/update").AddJsonBody(updatedAddress);
var response = await client.PostAsync<AddressUpdateResponse>(request);
```

Read more about serialization and deserialization [here](serialization.md).

### Response

When you use `ExecuteAsync`, you get an instance of `RestResponse` back that has the `Content` property, which contains the response as string. You can find other useful properties there, like `StatusCode`, `ContentType` and so on. If the request wasn't successful, you'd get a response back with `IsSuccessful` property set to `false` and the error explained in the `ErrorException` and `ErrorMessage` properties.

When using typed `ExecuteAsync<T>`, you get an instance of `RestResponse<T>` back, which is identical to `RestResponse` but also contains the `T Data` property with the deserialized response.

None of `ExecuteAsync` overloads throw if the remote server returns an error. You can inspect the response and find the status code, error message, and, potentially, an exception.

Extensions like `GetAsync<T>` will not return the whole `RestResponse<T>` but just a deserialized response. These extensions will throw an exception if the remote server returns an error. The exception will tell you what status code was returned by the server.
