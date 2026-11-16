# Authenticators

RestSharp includes authenticators for basic HTTP, OAuth1 and token-based (JWT and OAuth2). 

There are two ways to set the authenticator: client-wide or per-request.

Set the client-wide authenticator by assigning the `Authenticator` property of `RestClientOptions`:

```csharp
var options = new RestClientOptions("https://example.com") {
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

## Basic authentication

The `HttpBasicAuthenticator` allows you pass a username and password as a basic `Authorization` header using a base64 encoded string.

```csharp
var options = new RestClientOptions("https://example.com") {
    Authenticator = new HttpBasicAuthenticator("username", "password")
};
var client = new RestClient(options);
```

## OAuth1

For OAuth1 authentication the `OAuth1Authenticator` class provides static methods to help generate an OAuth authenticator.
OAuth1 authenticator will add the necessary OAuth parameters to the request, including signature.  

The authenticator will use `HMAC SHA1` to create a signature by default. 
Each static function to create the authenticator allows you to override the default and use another method to generate the signature.

### Request token

Getting a temporary request token is the usual first step in the 3-legged OAuth1 flow. 
Use `OAuth1Authenticator.ForRequestToken` function to get the request token authenticator.
This method requires a `consumerKey` and `consumerSecret` to authenticate.

```csharp
var options = new RestClientOptions("https://api.twitter.com") {
    Authenticator = OAuth1Authenticator.ForRequestToken(consumerKey, consumerSecret)
};
var client = new RestClient(options);
var request = new RestRequest("oauth/request_token");
```

The response should contain the token and the token secret, which can then be used to complete the authorization process.
If you need to provide the callback URL, assign the `CallbackUrl` property of the authenticator to the callback destination.

### Access token

Getting an access token is the usual third step in the 3-legged OAuth1 flow.
This method retrieves an access token when provided `consumerKey`, `consumerSecret`, `oauthToken`, and `oauthTokenSecret`. 
If you don't have a token for this call, you need to make a call to get the request token as described above.

```csharp
var authenticator = OAuth1Authenticator.ForAccessToken(
    consumerKey, consumerSecret, oauthToken, oauthTokenSecret
);
var options = new RestClientOptions("https://api.twitter.com") {
    Authenticator = authenticator
};
var client = new RestClient(options);
var request = new RestRequest("oauth/access_token");
```

If the second step in 3-leg OAuth1 flow returned a verifier value, you can use another overload of `ForAccessToken`:

```csharp
var authenticator = OAuth1Authenticator.ForAccessToken(
    consumerKey, consumerSecret, oauthToken, oauthTokenSecret, verifier
);
```

The response should contain the access token that can be used to make calls to protected resources.

For refreshing access tokens, use one of the two overloads of `ForAccessToken` that accept `sessionHandle`.

### Protected resource

When the access token is available, use `ForProtectedResource` function to get the authenticator for accessing protected resources.

```csharp
var authenticator = OAuth1Authenticator.ForAccessToken(
    consumerKey, consumerSecret, accessToken, accessTokenSecret
);
var options = new RestClientOptions("https://api.twitter.com/1.1") {
    Authenticator = authenticator
};
var client = new RestClient(options);
var request = new RestRequest("statuses/update.json", Method.Post)
        .AddParameter("status", "Hello Ladies + Gentlemen, a signed OAuth request!")
        .AddParameter("include_entities", "true");
```

### xAuth

xAuth is a simplified version of OAuth1. It allows sending the username and password as `x_auth_username` and `x_auth_password` request parameters and directly get the access token. xAuth is not widely supported, but RestSharp still allows using it.

Create an xAuth authenticator using `OAuth1Authenticator.ForClientAuthentication` function:

```csharp
var authenticator = OAuth1Authenticator.ForClientAuthentication(
    consumerKey, consumerSecret, username, password
);
```

### 0-legged OAuth

The access token authenticator can be used in 0-legged OAuth scenarios by providing `null` for the `consumerSecret`.

```csharp
var authenticator = OAuth1Authenticator.ForAccessToken(
    consumerKey, null, oauthToken, oauthTokenSecret
);
```

## OAuth2

RestSharp provides OAuth2 authenticators at two levels: **token lifecycle authenticators** that handle the full flow (obtaining, caching, and refreshing tokens automatically), and **simple authenticators** that just stamp a pre-obtained token onto requests.

### Token lifecycle authenticators

These authenticators manage tokens end-to-end. They use their own internal `HttpClient` for token endpoint calls, so there's no circular dependency with the `RestClient` they're attached to. All are thread-safe for concurrent use.

#### Client credentials

Use `OAuth2ClientCredentialsAuthenticator` for machine-to-machine flows. It POSTs `grant_type=client_credentials` to your token endpoint, caches the token, and refreshes it automatically before it expires.

```csharp
var request = new OAuth2TokenRequest(
    "https://auth.example.com/oauth2/token",
    "my-client-id",
    "my-client-secret"
) {
    Scope = "api.read api.write"
};

var options = new RestClientOptions("https://api.example.com") {
    Authenticator = new OAuth2ClientCredentialsAuthenticator(request)
};
using var client = new RestClient(options);
```

The authenticator will obtain a token on the first request and reuse it until it expires. The `ExpiryBuffer` property (default 30 seconds) controls how far in advance of actual expiry the token is considered stale.

#### Refresh token

Use `OAuth2RefreshTokenAuthenticator` when you already have an access token and refresh token (e.g., from an authorization code flow). It uses the initial access token until it expires, then automatically refreshes using the `refresh_token` grant type.

```csharp
var request = new OAuth2TokenRequest(
    "https://auth.example.com/oauth2/token",
    "my-client-id",
    "my-client-secret"
) {
    OnTokenRefreshed = response => {
        // Persist the new tokens to your storage
        SaveTokens(response.AccessToken, response.RefreshToken);
    }
};

var options = new RestClientOptions("https://api.example.com") {
    Authenticator = new OAuth2RefreshTokenAuthenticator(
        request,
        accessToken: "current-access-token",
        refreshToken: "current-refresh-token",
        expiresAt: DateTimeOffset.UtcNow.AddMinutes(30)
    )
};
using var client = new RestClient(options);
```

If the server rotates refresh tokens, the authenticator will automatically use the new refresh token for subsequent refreshes. The `OnTokenRefreshed` callback fires every time a new token is obtained, so you can persist the updated tokens.

#### Custom token provider

Use `OAuth2TokenAuthenticator` when you have a non-standard token flow or want full control over how tokens are obtained. Provide an async delegate that returns an `OAuth2Token`:

```csharp
var options = new RestClientOptions("https://api.example.com") {
    Authenticator = new OAuth2TokenAuthenticator(async cancellationToken => {
        var token = await myCustomTokenService.GetTokenAsync(cancellationToken);
        return new OAuth2Token(token.Value, token.ExpiresAt);
    })
};
using var client = new RestClient(options);
```

The authenticator caches the result and re-invokes your delegate when the token expires.

#### Bringing your own HttpClient

By default, the token lifecycle authenticators create their own `HttpClient` for token endpoint calls (and dispose it when the authenticator is disposed). If you need to customize it (e.g., for proxy settings or mTLS), pass your own:

```csharp
var request = new OAuth2TokenRequest(
    "https://auth.example.com/oauth2/token",
    "my-client-id",
    "my-client-secret"
) {
    HttpClient = myCustomHttpClient // not disposed by the authenticator
};
```

### Simple authenticators

If you manage tokens yourself and just need to stamp them onto requests, use these simpler authenticators.

`OAuth2UriQueryParameterAuthenticator` accepts the access token as the only constructor argument, and it will send the provided token as a query parameter `oauth_token`.

`OAuth2AuthorizationRequestHeaderAuthenticator` has two constructors. One only accepts a single argument, which is the access token. The other constructor also allows you to specify the token type. The authenticator will then add an `Authorization` header using the specified token type or `OAuth` as the default token type, and the token itself.

For example:

```csharp
var authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(
    token, "Bearer"
);
var options = new RestClientOptions("https://example.com") {
    Authenticator = authenticator
};
var client = new RestClient(options);
```

The code above will tell RestSharp to send the bearer token with each request as a header. Essentially, the code above does the same as the sample for `JwtAuthenticator` below.

## JWT

The JWT authentication can be supported by using `JwtAuthenticator`. It is a very simple class that can be constructed like this:

```csharp
var authenticator = new JwtAuthenticator(myToken);
var options = new RestClientOptions("https://example.com") {
    Authenticator = authenticator
};
var client = new RestClient(options);
```

For each request, it will add an `Authorization` header with the value `Bearer <your token>`.

As you might need to refresh the token from, you can use the `SetBearerToken` method to update the token.

## Custom authenticator

You can write your own implementation by implementing `IAuthenticator` and 
registering it with your RestClient:

```csharp
var authenticator = new SuperAuthenticator(); // implements IAuthenticator
var options = new RestClientOptions("https://example.com") {
    Authenticator = authenticator
};
var client = new RestClient(options);
```

The `Authenticate` method is the very first thing called upon calling `RestClient.Execute` or `RestClient.Execute<T>`. 
It gets the `RestRequest` currently being executed giving you access to every part of the request data (headers, parameters, etc.)

You can find an example of using the built-in OAuth2 authenticator in a typed API client [here](../usage/example.md#authenticator).
