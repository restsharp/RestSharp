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

## OAuth1

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
