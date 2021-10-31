# Authenticators

RestSharp includes authenticators for basic HTTP (Authorization header), 
NTLM and parameter-based systems. 

## Using SimpleAuthenticator

The `SimpleAuthenticator` included allows you to pass a 
username and password (or API and secret key) as GET or POST 
parameters depending on the method used for the request. 
You pass it the username, password and the names of the 
parameters for each.

```csharp
var client = new RestClient("http://example.com");
client.Authenticator = new SimpleAuthenticator("username", "foo", "password", "bar");

var request = new RestRequest("resource", Method.GET);
client.Execute(request);
```

The URL generated for this request would be `http://example.com/resource?username=foo&password=bar`

Changing the above request to use a POST or PUT would send 
the values as encoded form values instead.

## Basic Authentication

The `HttpBasicAuthenticator` allows you pass a username and password as a basica auth Authorization header.

```csharp
var client = new RestClient("http://example.com");
client.Authenticator = new HttpBasicAuthenticator("username", "password");
```

## OAuth1

For OAuth1 authentication the `OAuth1Authenticator` class provides static methods to help generate an OAuth authenticator.

### For endpoints requiring a request token

This method requires a `consumerKey` and `consumerSecret` to authenticate.

```csharp
var client = new RestClient("http://example.com");
client.Authenticator = OAuth1Authenticator.ForRequestToken(consumerKey, consumerSecret);
```

### For endpoints requiring an access token

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
