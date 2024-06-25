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
        var options = new RestClientOptions("https://api.twitter.com/2");
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

Right now, the client won't really work as Twitter API requires authentication. It's covered in the next section.

## Authenticator

Before we can call the API itself, we need to get a bearer token. Twitter exposes an endpoint `https://api.twitter.com/oauth2/token`. As it follows the OAuth2 conventions, the code can be used to create an authenticator for some other vendors.

First, we need a model for deserializing the token endpoint response. OAuth2 uses snake case for property naming, so we need to decorate model properties with `JsonPropertyName` attribute:

```csharp
record TokenResponse {
    [JsonPropertyName("token_type")]
    public string TokenType { get; init; }
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; }
}
```

Next, we create the authenticator itself. It needs the API key and API key secret to call the token endpoint using basic HTTP authentication. In addition, we can extend the list of parameters with the base URL to convert it to a more generic OAuth2 authenticator.

The easiest way to create an authenticator is to inherit from the `AuthenticatorBase` base class:

```csharp
public class TwitterAuthenticator : AuthenticatorBase {
    readonly string _baseUrl;
    readonly string _clientId;
    readonly string _clientSecret;

    public TwitterAuthenticator(string baseUrl, string clientId, string clientSecret) : base("") {
        _baseUrl      = baseUrl;
        _clientId     = clientId;
        _clientSecret = clientSecret;
    }

    protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken) {
        Token = string.IsNullOrEmpty(Token) ? await GetToken() : Token;
        return new HeaderParameter(KnownHeaders.Authorization, Token);
    }
}
```

During the first call made by the client using the authenticator, it will find out that the `Token` property is empty. It will then call the `GetToken` function to get the token once and reuse the token going forward.

Now, we need to implement the `GetToken` function in the class:

```csharp
async Task<string> GetToken() {
    var options = new RestClientOptions(_baseUrl){
        Authenticator = new HttpBasicAuthenticator(_clientId, _clientSecret),
    };
    using var client = new RestClient(options);

    var request = new RestRequest("oauth2/token")
        .AddParameter("grant_type", "client_credentials");
    var response = await client.PostAsync<TokenResponse>(request);
    return $"{response!.TokenType} {response!.AccessToken}";
}
```

As we need to make a call to the token endpoint, we need our own short-lived instance of `RestClient`. Unlike the actual Twitter client, it will use the `HttpBasicAuthenticator` to send the API key and secret as the username and password. The client then gets disposed as we only use it once.

Here we add a POST parameter `grant_type` with `client_credentials` as its value. At the moment, it's the only supported value.

The POST request will use the `application/x-www-form-urlencoded` content type by default.

::: note
Sample code provided on this page is a production code. For example, the authenticator might produce undesired side effect when multiple requests are made at the same time when the token hasn't been obtained yet. It can be solved rather than simply using semaphores or synchronized invocation.
:::

## Final words

This page demonstrates how an API client can be implemented as a typed, configurable client with its own interface. Usage of the client in applications is not covered here as different application types and target frameworks have their own idiomatic ways to use HTTP clients.