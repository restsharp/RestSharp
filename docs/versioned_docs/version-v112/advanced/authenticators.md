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

RestSharp has two very simple authenticators to send the access token as part of the request.

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

As those authenticators don't do much to get the token itself, you might be interested in looking at our [sample OAuth2 authenticator](../usage/example.md#authenticator), which requests the token on its own.

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

You can find an example of a custom authenticator that fetches and uses an OAuth2 bearer token [here](../usage/example.md#authenticator).
