# Getting Started

Before you can use RestSharp in your application, you need to add the NuGet package. You can do it using your IDE or the command line:

```
dotnet add package RestSharp
```

## Basic Usage

If you only have a few number of one-off requests to make to an API, you can use RestSharp like so:

```csharp
using RestSharp;
using RestSharp.Authenticators;

var client = new RestClient("https://api.twitter.com/1.1");
client.Authenticator = new HttpBasicAuthenticator("username", "password");

var request = new RestRequest("statuses/home_timeline.json", DataFormat.Json);

var response = client.Get(request);
```

`IRestResponse` contains all the information returned from the remote server. 
You have access to the headers, content, HTTP status and more. 

We recommend using the generic overloads like `Get<T>` to automatically deserialize the response into .NET classes. 

## Asynchronous Calls

All synchronous methods have their asynchronous siblings, suffixed with `Async`.

So, instead of `Get<T>` that returns `T` or `Execute<T>`, which returns `IRestResponse<T>`,
you can use `GetAsync<T>` and `ExecuteAsync<T>`. The arguments set is usually identical.
You can optionally supply the cancellation token, which by default is set to `CancellationToken.None`.

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

Read [here](../usage/exceptions.md) about how RestSharp handles exceptions.

## Content type

RestSharp supports sending XML or JSON body as part of the request. To add a body to the request, simply call `AddJsonBody` or `AddXmlBody` method of the `IRestRequest` instance.

There is no need to set the `Content-Type` or add the `DataFormat` parameter to the request when using those methods, RestSharp will do it for you.

RestSharp will also handle both XML and JSON responses and perform all necessary deserialization tasks, depending on th server response type. Therefore, you only need to add the `Accept` header if you want to deserialize the response manually.

For example, only you'd only need these lines to make a request with JSON body:

```csharp
var request = new RestRequest("address/update")
    .AddJsonBody(updatedAddress);
var response = await client.PostAsync<AddressUpdateResponse>(request);
```

## Response

When you use `Execute` or `ExecuteAsync`, you get an instance of `IRestResponse` back that has the `Content` property, which contains the response as string. You can find other useful properties there, like `StatusCode`, `ContentType` and so on. If the request wasn't successful, you'd get a response back with `IsSuccessful` property set to `false` and the error explained in the `ErrorException` and `ErrorMessage` properties.

When using typed `Execute<T>` or `ExecuteAsync<T>`, you get an instance of `IRestResponse<T>` back, which is identical to `IRestResponse` but also contains the `T Data` property with the deserialized response.

Extensions like `Get<T>` and `GetAsync<T>` will not return the whole `IRestResponse<T>` but just a deserialized response. You might get `null` back if something goes wrong, and it can be hard to understand the issue. Therefore, when using typed extension methods, we suggest setting `IRestClient.ThrowOnAnyError` property to `true`. By doing that, you tell RestSharp to throw an exception when something goes wrong. You can then wrap the call in a `try`/`catch` block and handle the exception accordingly. To know more about how RestSharp deals with exceptions, please refer to the [Error handling](../usage/exceptions.md) page.
