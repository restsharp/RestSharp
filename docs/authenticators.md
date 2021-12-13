# Authenticators

RestSharp includes authenticators for basic HTTP (Authorization header), 
NTLM and parameter-based systems. 

## Basic Authentication

The `HttpBasicAuthenticator` allows you pass a username and password as a basica auth Authorization header.

```csharp
var client = new RestClient("http://example.com");
client.Authenticator = new HttpBasicAuthenticator("username", "password");
```

## OAuth1

For OAuth1 authentication the `OAuth1Authenticator` class provides static methods to help generate an OAuth authenticator.

### Request token

This method requires a `consumerKey` and `consumerSecret` to authenticate.

```csharp
var client = new RestClient("http://example.com");
client.Authenticator = OAuth1Authenticator.ForRequestToken(consumerKey, consumerSecret);
```

### Access token

This method retrieves an access token when provided `consumerKey`, `consumerSecret`, `oauthToken`, and `oauthTokenSecret`.

```csharp
client.Authenticator = OAuth1Authenticator.ForAccessToken(
                        consumerKey, consumerSecret, oauthToken,
                        oauthTokenSecret
                       );
```

This method also includes an optional parameter to specity the `OAuthSignatureMethod`.
```csharp
client.Authenticator = OAuth1Authenticator.ForAccessToken(consumerKey, 
                                                          consumerSecret, 
                                                          oauthToken, 
                                                          oauthTokenSecret, 
                                                          OAuthSignatureMethod.PlainText);
```

### 0-legged OAuth

The same access token authenticator can be used in 0-legged OAuth scenarios by providing `null` for the `consumerSecret`.
```csharp
client.Authenticator = OAuth1Authenticator.ForAccessToken(consumerKey, null, oauthToken, oauthTokenSecret);
```

## JWT

## Custom Authenticator

You can write your own implementation by implementing `IAuthenticator` and 
registering it with your RestClient:

```csharp
var client = new RestClient();
client.Authenticator = new SuperAuthenticator(); // implements IAuthenticator
```

The `Authenticate` method is the very first thing called upon calling 
`RestClient.Execute` or `RestClient.Execute<T>`. 
The `Authenticate` method is passed the `RestRequest` currently being executed giving 
you access to  every part of the request data (headers, parameters, etc.)
