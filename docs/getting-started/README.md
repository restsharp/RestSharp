# Getting Started

First, add the NuGet package to your project:

```
dotnet add package RestSharp
```

## Basic Usage

If you only have a small number of one-off requests to make to an API, you can use RestSharp like so:

```csharp
using RestSharp;
using RestSharp.Authenticators;

var client = new RestClient("https://api.twitter.com/1.1");
client.Authenticator = new HttpBasicAuthenticator("username", "password");

var request = new RestRequest("statuses/home_timeline.json", DataFormat.Json);

var response = client.Get(request);
```

`IRestResponse` contains all of the information returned from the remote server. 
You have access to the headers, content, HTTP status and more. 

It is recommended that you use the generic overloads like `Get<T>` to automatically deserialize the response into .NET classes. 

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

The most important difference, however, that async methods that are named after
HTTP methods return the `Task<T>` instead of `Task<IRestResponse<T>>`. Because it
means that you won't get an error response if the request fails, those methods
throw an exception.

All `ExecuteAsync` overloads, however, behave in the same way as `Execute` and return
the `IRestResponse` or `IRestResponse<T>`.

Read [here](../usage/exceptions.md) about how RestSharp handles exceptions.

