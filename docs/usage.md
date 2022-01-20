---
title: Usage
---

## Recommended usage

RestSharp works best as the foundation for a proxy class for your API. Each API would most probably require different settings for `RestClient`, so a dedicated API class (and its interface) gives you a nice isolation between different `RestClient` instances, and make them testable.

Essentially, RestSharp is a wrapper around `HttpClient` that allows you to do the following:
- Add default parameters of any kind (not just headers) to the client, once
- Add parameters of any kind to each request (query, URL segment, form, attachment, serialized body, header) in a straightforward way
- Serialize the payload to JSON or XML if necessary
- Set the correct content headers (content type, disposition, length, etc)
- Handle the remote endpoint response
- Deserialize the response from JSON or XML if necessary

As an example, let's look at a simple Twitter API v2 client, which uses OAuth2 machine-to-machine authentication. For it to work, you would need to have access to Twitter Developers portal, an a project, and an approved application inside the project with OAuth2 enabled.

### Authenticator

Before we can make any call to the API itself, we need to get a bearer token. Twitter exposes an endpoint `https://api.twitter.com/oauth2/token`. As it follows the OAuth2 conventions, the code can be used to create an authenticator for some other vendors.

First, we need a model for deserializing the token endpoint response. OAuth2 uses snake case for property naming, so we need to decorate model properties with `JsonPropertyName` attribute:

```csharp
record TokenResponse {
    [JsonPropertyName("token_type")]
    public string TokenType { get; init; }
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; }
}
```

Next, we create the authenticator itself. It needs the API key and API key secret for calling the token endpoint using basic HTTP authentication. In addition, we can extend the list of parameters with the base URL, so it can be converted to a more generic OAuth2 authenticator.

The easiest way to create an authenticator is to inherit is from the `AuthanticatorBase` base class:

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
        var token = string.IsNullOrEmpty(Token) ? await GetToken() : Token;
        return new HeaderParameter(KnownHeaders.Authorization, token);
    }
}
```

During the first call made by the client using the authenticator, it will find out that the `Token` property is empty. It will then call the `GetToken` function to get the token once, and then will reuse the token going forwards.

Now, we need to include the `GetToken` function to the class:

```csharp
async Task<string> GetToken() {
    var options = new RestClientOptions(_baseUrl);
    using var client = new RestClient(options) {
        Authenticator = new HttpBasicAuthenticator(_clientId, _clientSecret),
    };

    var request = new RestRequest("oauth2/token")
        .AddParameter("grant_type", "client_credentials");
    var response = await client.PostAsync<TokenResponse>(request);
    return $"{response!.TokenType} {response!.AccessToken}";
}
```

As we need to make a call to the token endpoint, we need our own, short-lived instance of `RestClient`. Unlike the actual Twitter client, it will use the `HttpBasicAuthenticator` to send API key and secret as username and password. The client is then gets disposed as we only use it once.

Here we add a POST parameter `grant_type` with `client_credentials` as its value. At the moment, it's the only supported value.

The POST request will use the `application/x-www-form-urlencoded` content type by default.

### API client

Now, we can start creating the API client itself. Here we start with a single function that retrieves one Twitter user. Let's being by defining the API client interface:

```csharp
public interface ITwitterClient {
    Task<TwitterUser> GetUser(string user);
}
```

As the function returns a `TwitterUser` instance, we need to define it as a model:

```csharp
public record TwitterUser(string Id, string Name, string Username);
```

When that is done, we can implement the interface and add all the necessary code blocks to get a working API client.

The client class needs the following:
- A constructor, which accepts API credentials to be passed to the authenticator
- A wrapped `RestClient` instance with Twitter API base URI pre-configured
- The `TwitterAuthenticator` that we created previously as the client authenticator
- The actual function to get the user

```csharp
public class TwitterClient : ITwitterClient, IDisposable {
    readonly RestClient _client;

    public TwitterClient(string apiKey, string apiKeySecret) {
        var options = new RestClientOptions("https://api.twitter.com/2");

        _client = new RestClient(options) {
            Authenticator = new TwitterAuthenticator("https://api.twitter.com", apiKey, apiKeySecret)
        };
    }

    public async Task<TwitterUser> GetUser(string user) {
        var response = await _client.GetJsonAsync<TwitterSingleObject<TwitterUser>>(
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

Couple of things that don't fall to the "basics" list.
- The API client class needs to be disposable, so it can dispose the wrapped `HttpClient` instance
- Twitter API returns wrapped models. In this case we use the `TwitterSingleObject` wrapper, in other methods you'd need a similar object with `T[] Data` to accept collections

You can find the full example code in [this gist](https://gist.github.com/alexeyzimarev/62d77bb25d7aa5bb4b9685461f8aabdd).

Such a client can and should be used _as a singleton_, as it's thread-safe and authentication-aware. If you make it a transient dependency, you'll keep bombarding Twitter with token requests and effectively half your request limit.

You can, for example, register it in the DI container:

```csharp
services.AddSingleton<ITwitterClient>(
    new TwitterClient(
        Configuration["Twitter:ApiKey"],
        Configuration["Twitter:ApiKeySecret"]
    )
);
```

## Create a request

Before making a request using `RestClient`, you need to create a request instance:

```csharp
var request = new RestRequest(resource); // resource is the sub-path of the client base path
```

The default request type is `GET` and you can override it by setting the `Method` property. You can also set the method using the constructor overload:

```csharp
var request = new RestRequest(resource, Method.Post);
```

After you've created a `RestRequest`, you can add parameters to it. Below, you can find all the parameter types supported by RestSharp.

### Http Header

Adds the parameter as an HTTP header that is sent along with the request. The header name is the name of the parameter and the header value is the value.

::: warning Content-Type
RestSharp will use the correct content type by default. Avoid adding the `Content-Type` header manually to your requests, unless you are absolutely sure it is required. You can add a custom content type to the [body parameter](#request-body) itself.
:::

### Get or Post

This behaves differently based on the method. If you execute a GET call, RestSharp will append the parameters to the Url in the form `url?name1=value1&name2=value2`.

On a POST or PUT Requests, it depends on whether you have files attached to a Request.
If not, the Parameters will be sent as the body of the request in the form `name1=value1&name2=value2`. Also, the request will be sent as `application/x-www-form-urlencoded`.

In both cases, name and value will automatically be url-encoded.

If you have files, RestSharp will send a `multipart/form-data` request. Your parameters will be part of this request in the form:

```
Content-Disposition: form-data; name="parameterName"

ParameterValue
```

### Url Segment

Unlike `GetOrPost`, this `ParameterType` replaces placeholder values in the `RequestUrl`:

```csharp
var request = new RestRequest("health/{entity}/status")
    .AddUrlSegment("entity", "s2");
```

When the request executes, RestSharp will try to match any `{placeholder}` with a parameter of that name (without the `{}`) and replace it with the value. So the above code results in `health/s2/status` being the url.

### Request Body

If this parameter is set, its value will be sent as the body of the request.

We recommend using `AddJsonBody` or `AddXmlBody` methods instead of `AddParameter` with type `BodyParameter`. Those methods will set the proper request type and do the serialization work for you.

#### AddStringBody

If you have a pre-serialized payload like a JSON string, you can use `AddStringBody` to add it as a body parameter. You need to specify the content type, so the remote endpoint knows what to do with the request body. For example:

```csharp
const json = "{ data: { foo: \"bar\" } }";
request.AddStringBody(json, ContentType.Json);
```

You can specify a custom body content type if necessary. The `contentType` argument is available in all the overloads that add a request body.

#### AddStringBody

If you have a pre-serialized payload like a JSON string, you can use `AddStringBody` to add it as a body parameter. You need to specify the content type, so the remote endpoint knows what to do with the request body. For example:

```csharp
const json = "{ data: { foo: \"bar\" } }";
request.AddStringBody(json, ContentType.Json);
```

#### AddJsonBody

When you call `AddJsonBody`, it does the following for you:

- Instructs the RestClient to serialize the object parameter as JSON when making a request
- Sets the content type to `application/json`
- Sets the internal data type of the request body to `DataType.Json`

::: warning
Do not send JSON string or some sort of `JObject` instance to `AddJsonBody`, it won't work! Use `AddStringBody` instead.
:::

Here is the example:

```csharp
var param = new MyClass { IntData = 1, StringData = "test123" };
request.AddJsonBody(param);
```

#### AddXmlBody

When you call `AddXmlBody`, it does the following for you:

- Instructs the RestClient to serialize the object parameter as XML when making a request
- Sets the content type to `application/xml`
- Sets the internal data type of the request body to `DataType.Xml`

::: warning
Do not send XML string to `AddXmlBody`, it won't work!
:::

### Query String

This works like `GetOrPost`, except that it always appends the parameters to the url in the form `url?name1=value1&name2=value2`, regardless of the request method.

Example:

```csharp
var client = new RestClient("https://search.me");
var request = new RestRequest("search")
    .AddParameter("foo", "bar");
var response = await client.GetAsync<SearchResponse>(request);
```

It will send a `GET` request to `https://search.me/search?foo=bar")`.

You can also specify the query string parameter type explicitly:

```csharp
request.AddParameter("foo", "bar", ParameterType.QueryString);
```

In some cases you might need to prevent RestSharp from encoding the query string parameter. To do so, use the `QueryStringWithoutEncode` parameter type.

## Making a call

When you have a `RestRequest` instance with all the parameters added to it, you are ready to make a request.

`RestClient` has a single function for this:

```csharp
public async Task<RestResponse> ExecuteAsync(
    RestRequest request, 
    CancellationToken cancellationToken = default
)
```

You can also avoid setting the request method upfront and use one of the overloads:

```csharp
Task<RestResponse> ExecuteGetAsync(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse> ExecutePostAsync(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse> ExecutePutAsync(RestRequest request, CancellationToken cancellationToken)
```

When using any of those methods, you will get the response content as string in `response.Content`.

RestSharp can deserialize the response for you. To use that feature, use one of the generic overloads:

```csharp
Task<RestResponse<T>> ExecuteAsync<T>(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse<T>> ExecuteGetAsync<T>(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse<T>> ExecutePostAsync<T>(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse<T>> ExecutePutAsync<T>(RestRequest request, CancellationToken cancellationToken)
```

All the overloads that return `RestResponse` or `RestResponse<T>` don't throw an exception if the server returns an error. Read more about it [here](error-handling.md).

If you just need a deserialized response, you can use one of the extensions:

```csharp
Task<T> GetAsync<T>(RestRequest request, CancellationToken cancellationToken)
Task<T> PostAsync<T>(RestRequest request, CancellationToken cancellationToken)
Task<T> PutAsync<T>(RestRequest request, CancellationToken cancellationToken)
Task<T> HeadAsync<T>(RestRequest request, CancellationToken cancellationToken)
Task<T> PatchAsync<T>(RestRequest request, CancellationToken cancellationToken)
Task<T> DeleteAsync<T>(RestRequest request, CancellationToken cancellationToken)
```

Those extensions will throw an exception if the server returns an error, as there's no other way to float the error back to the caller.

### JSON requests

For making a simple `GET` call and get a deserialized JSON response with a pre-formed resource string, use this:

```csharp
var response = await client.GetJsonAsync<TResponse>("endpoint?foo=bar", cancellationToken);
```

You can also use a more advance extension that uses an object to compose the resource string:

```csharp
var client = new RestClient("https://example.org");
var args = new {
    id = "123",
    foo = "bar"
};
// Will make a call to https://example.org/endpoint/123?foo=bar
var response = await client.GetJsonAsync<TResponse>("endpoint/{id}", args, cancellationToken);
```

It will search for the URL segment parameters matching any of the object properties and replace them with values. All the other properties will be used as query parameters.

Similar things are available for `POST` requests.

```csharp
var request = new CreateOrder("123", "foo", 10100);
// Will post the request object as JSON to "orders" and returns a 
// JSON response deserialized to OrderCreated  
var result = client.PostJsonAsync<CreateOrder, OrderCreated>("orders", request, cancellationToken);
```

```csharp
var request = new CreateOrder("123", "foo", 10100);
// Will post the request object as JSON to "orders" and returns a 
// status code, not expecting any response body
var statusCode = client.PostJsonAsync("orders", request, cancellationToken);
```

The same two extensions also exist for `PUT` requests (`PutJsonAsync`);
