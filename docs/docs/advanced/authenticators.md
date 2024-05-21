# Authenticators

RestSharp includes authenticators for basic HTTP, OAuth1 and token-based (JWT and OAuth2). 

There are two ways to set the authenticator: client-wide or per-request.

Set the client-wide authenticator by assigning the `Authenticator` property of `RestClientOptions`:

```csharp
var options = new RestClientOptions("http://example.com") {
    Authenticator = new HttpBasicAuthenticator("username", "password")
};
var client = new RestClient(options);
```

To set the authenticator per-request, assign the `Authenticator` property of `RestRequest`:

```csharp
var request = new RestRequest("/api/users/me") {
    Authenticator = new HttpBasicAuthenticator("username", "password")
};
var response = await client.ExecuteAsync(request, cancellationToken);
```

## Basic Authentication

The `HttpBasicAuthenticator` allows you pass a username and password as a basic `Authorization` header using a base64 encoded string.

```csharp
var options = new RestClientOptions("http://example.com") {
    Authenticator = new HttpBasicAuthenticator("username", "password")
};
var client = new RestClient(options);
```

## OAuth1

For OAuth1 authentication the `OAuth1Authenticator` class provides static methods to help generate an OAuth authenticator.

### Request token

This method requires a `consumerKey` and `consumerSecret` to authenticate.

```csharp
var options = new RestClientOptions("http://example.com") {
    Authenticator = OAuth1Authenticator.ForRequestToken(consumerKey, consumerSecret)
};
var client = new RestClient(options);
```

### Access token

This method retrieves an access token when provided `consumerKey`, `consumerSecret`, `oauthToken`, and `oauthTokenSecret`.

```csharp
var authenticator = OAuth1Authenticator.ForAccessToken(
    consumerKey, consumerSecret, oauthToken, oauthTokenSecret
);
var options = new RestClientOptions("http://example.com") {
    Authenticator = authenticator
};
var client = new RestClient(options);
```

This method also includes an optional parameter to specify the `OAuthSignatureMethod`.
```csharp
var authenticator = OAuth1Authenticator.ForAccessToken(
    consumerKey, consumerSecret, oauthToken, oauthTokenSecret, 
    OAuthSignatureMethod.PlainText
);
```

### 0-legged OAuth

The same access token authenticator can be used in 0-legged OAuth scenarios by providing `null` for the `consumerSecret`.

```csharp
var authenticator = OAuth1Authenticator.ForAccessToken(
    consumerKey, null, oauthToken, oauthTokenSecret
);
```

## OAuth2

RestSharp has two very simple authenticators to send the access token as part of the request.

`OAuth2UriQueryParameterAuthenticator` accepts the access token as the only constructor argument, and it will send the provided token as a query parameter `oauth_token`.

`OAuth2AuthorizationRequestHeaderAuthenticator` has two constructors. One only accepts a single argument, which is the access token. The other constructor also allows you to specify the token type. The authenticator will then add an `Authorization` header using the specified token type or `OAuth` as the default token type, and the token itself.

For example:

```csharp
var authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(
    token, "Bearer"
);
var options = new RestClientOptions("http://example.com") {
    Authenticator = authenticator
};
var client = new RestClient(options);
```

The code above will tell RestSharp to send the bearer token with each request as a header. Essentially, the code above does the same as the sample for `JwtAuthenticator` below.

As those authenticators don't do much to get the token itself, you might be interested in looking at our [sample OAuth2 authenticator](usage.md#authenticator), which requests the token on its own.

## JWT

The JWT authentication can be supported by using `JwtAuthenticator`. It is a very simple class that can be constructed like this:

```csharp
var authenticator = new JwtAuthenticator(myToken);
var options = new RestClientOptions("http://example.com") {
    Authenticator = authenticator
};
var client = new RestClient(options);
```

For each request, it will add an `Authorization` header with the value `Bearer <your token>`.

As you might need to refresh the token from, you can use the `SetBearerToken` method to update the token.

## Custom Authenticator

You can write your own implementation by implementing `IAuthenticator` and 
registering it with your RestClient:

```csharp
var authenticator = new SuperAuthenticator(); // implements IAuthenticator
var options = new RestClientOptions("http://example.com") {
    Authenticator = authenticator
};
var client = new RestClient(options);
```

The `Authenticate` method is the very first thing called upon calling `RestClient.Execute` or `RestClient.Execute<T>`. 
It gets the `RestRequest` currently being executed giving you access to  every part of the request data (headers, parameters, etc.)

You can find an example of a custom authenticator that fetches and uses an OAuth2 bearer token [here](usage.md#authenticator).
