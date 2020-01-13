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

var timeline = await client.Get<HomeTimeline>(request, cancellationToken);
```

## Note About Error Handling

Normally, RestSharp doesn't throw an exception if the request fails.

However, it is possible to configure RestSharp to throw in different situations, when it normally doesn't throw
in favour of giving you the error as a property.

[TODO] - add exception handling

If there is a network transport error (network is down, failed DNS lookup, etc), `RestResponse.ResponseStatus` will be set to `ResponseStatus.Error`, otherwise it will be `ResponseStatus.Completed`. 

If an API returns a 404, `ResponseStatus` will still be `Completed`. If you need access to the HTTP status code returned you will find it at `RestResponse.StatusCode`. 
The `Status` property is an indicator of completion independent of the API error handling.
