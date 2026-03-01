---
sidebar_position: 1
---

# Example

RestSharp works best as the foundation for a proxy class for your API. Each API would most probably require different settings for `RestClient`. Hence, a dedicated API class (and its interface) gives you sound isolation between different `RestClient` instances and make them testable.

For example, let's look at a simple Twitter API v2 client, which uses OAuth2 machine-to-machine authentication. For it to work, you would need to have access to the Twitter Developers portal, a project, and an approved application inside the project with OAuth2 enabled.

## Client model

Before implementing an API client, we need to have a model for it. The model includes an abstraction for the client, which has functions for the API calls we are interested to implement. In addition, the client model would include the necessary request and response models. Usually those are simple classes or records without logic, which are often referred to as DTOs (data transfer objects).

This example starts with a single function that retrieves one Twitter user. Lets being by defining the API client interface:

```csharp
public interface ITwitterClient {
    Task<TwitterUser> GetUser(string user);
}
```

As the function returns a `TwitterUser` instance, we need to define it as a model:

```csharp
public record TwitterUser(string Id, string Name, string Username);
```

## Client implementation

When that is done, we can implement the interface and add all the necessary code blocks to get a working API client.

The client class needs the following:
- A constructor for passing API credentials
- A wrapped `RestClient` instance with the Twitter API base URI pre-configured
- An authenticator to support authorizing the client using Twitter OAuth2 authentication
- The actual function to get the user (to implement the `ITwitterClient` interface)

Creating an authenticator is described [below](#authenticator).

Here's how the client implementation could look like:

```csharp
public class TwitterClient : ITwitterClient, IDisposable {
    readonly RestClient _client;

    public TwitterClient(string apiKey, string apiKeySecret) {
        var tokenRequest = new OAuth2TokenRequest(
            "https://api.twitter.com/oauth2/token",
            apiKey,
            apiKeySecret
        );
        var options = new RestClientOptions("https://api.twitter.com/2") {
            Authenticator = new OAuth2ClientCredentialsAuthenticator(tokenRequest)
        };
        _client = new RestClient(options);
    }

    public async Task<TwitterUser> GetUser(string user) {
        var response = await _client.GetAsync<TwitterSingleObject<TwitterUser>>(
            "users/by/username/{user}",
            new { user }
        );
        return response!.Data;
    }

    record TwitterSingleObject<T>(T Data);

    public void Dispose() {
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }
}
```

It is also possible to use ASP.NET Core Options for configuring the client, instead of passing the credentials as strings. For example, we can add a class for Twitter client options, and use it in a constructor:

```csharp
public class TwitterClientOptions(string ApiKey, string ApiSecret);

public TwitterClient(IOptions<TwitterClientOptions> options) {
    var opt = new RestClientOptions("https://api.twitter.com/2");
    _client = new RestClient(options);
}
```

Then, you can register and configure the client using ASP.NET Core dependency injection container.

Notice the client constructor already configures the `OAuth2ClientCredentialsAuthenticator`. The authenticator setup is described in the next section.

## Authenticator

Before we can call the API itself, we need to get a bearer token. Twitter exposes an endpoint `https://api.twitter.com/oauth2/token`. As it follows the standard OAuth2 client credentials convention, we can use the built-in `OAuth2ClientCredentialsAuthenticator`:

```csharp
var tokenRequest = new OAuth2TokenRequest(
    "https://api.twitter.com/oauth2/token",
    apiKey,
    apiKeySecret
);

var options = new RestClientOptions("https://api.twitter.com/2") {
    Authenticator = new OAuth2ClientCredentialsAuthenticator(tokenRequest)
};
```

The authenticator will automatically obtain a token on the first request, cache it, and refresh it when it expires. It uses its own `HttpClient` internally for token endpoint calls, so there's no circular dependency with the `RestClient`.

For more details on the available OAuth2 authenticators (including refresh token flows and custom token providers), see [Authenticators](../advanced/authenticators.md#oauth2).

## Final words

This page demonstrates how an API client can be implemented as a typed, configurable client with its own interface. Usage of the client in applications is not covered here as different application types and target frameworks have their own idiomatic ways to use HTTP clients.