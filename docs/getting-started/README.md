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

See below about how RestSharp handles exceptions.

## Note About Error Handling

If there is a network transport error (network is down, failed DNS lookup, etc), `RestResponse.ResponseStatus` will be set to `ResponseStatus.Error`, otherwise it will be `ResponseStatus.Completed`. 

If an API returns a 404, `ResponseStatus` will still be `Completed`. If you need access to the HTTP status code returned you will find it at `RestResponse.StatusCode`. 
The `Status` property is an indicator of completion independent of the API error handling.

Normally, RestSharp doesn't throw an exception if the request fails.

However, it is possible to configure RestSharp to throw in different situations, when it normally doesn't throw
in favour of giving you the error as a property.

| Property        | Behavior           |
| ------------- |:-------------:|
| FailOnDeserialization      | Changes the default behavior when failed deserialization results in a successful response with an empty `Data` property of the response. Setting this property to `true` will tell RestSharp to consider failed deserialization as an error and set the `ResponseStatus` to `Error` accordingly. |
| ThrowOnDeserialization      | Changes the default behavior when failed deserialization results in empty `Data` property of the response. Setting this property to `true` will tell RestSharp to throw when deserialization fails. |
| ThrowOnAnyError      | Setting this property to `true` changes the default behavior and forces RestSharp to throw if any errors occurs when making a request or during deserialization.     |

There are also slight differences on how different overloads handle exceptions.

Asynchronous generic methods `GetAsync<T>`, `PostAsync<T>` and so on, which aren't a part of `IRestClient` interface
(those methods are extension methods) return `Task<T>`. It means that there's no `IRestResponse` to set the response status to error.
We decided to throw an exception when such a request fails. It is a trade-off between the API
consistency and usability of the library. Usually, you only need the content of `RestResponse` instance to diagnose issues
and most of the time the exception would tell you what's wrong. 
