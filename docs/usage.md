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

## Request Parameters

After you've created a `RestRequest`, you can add parameters to it.
Here is a Description of the 5 currently supported types and their behavior when using the default IHttp implementation.

### Http Header

Adds the parameter as an HTTP header that is sent along with the request. The header name is the name of the parameter and the header value is the value.

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

If this parameter is set, its value will be sent as the body of the request. *Only one* `RequestBody` parameter is accepted - the first one.

The name of the parameter will be used as the `Content-Type` header for the request.

`RequestBody` does not work on `GET` or `HEAD` Requests, as they do not send a body.

If you have `GetOrPost` parameters as well, they will overwrite the `RequestBody` - RestSharp will not combine them, but it will instead throw the `RequestBody` parameter away.

We recommend using `AddJsonBody` or `AddXmlBody` methods instead of `AddParameter` with type `BodyParameter`. Those methods will set the proper request type and do the serialization work for you.

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

Do not set content type headers or send JSON string or some sort of `JObject` instance to `AddJsonBody`, it won't work!

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

Do not set content type headers or send XML string to `AddXmlBody`, it won't work!

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

## Serialization

RestSharp has JSON and XML serializers built in. 

:::tip
The default behavior of RestSharp is to swallow deserialization errors and return `null` in the `Data`
property of the response. Read more about it in the [Error Handling](#error-handling).
:::

### JSON

The default JSON serializer uses `System.Text.Json`, which is a part of .NET since .NET 6. For earlier versions, it is added as a dependency. There are also a few serializers provided as additional packages.

### XML

The default XML serializer is `DotNetXmlSerializer`, which uses `System.Xml.Serialization` library from .NET.

### NewtonsoftJson (aka Json.Net)

The `NewtonsoftJson` package is the most popular JSON serializer for .NET. It handles all possible scenarios and is very configurable. Such a flexibility comes with the cost of performance. If you need speed, keep the default JSON serializer.

RestSharp support Json.Net serializer via a separate package. You can install it
from NuGet:

```
dotnet add package RestSharp.Serializers.NewtonsoftJson
```

Use the extension method provided by the package to configure the client:

```csharp
client.UseNewtonsoftJson();
```

The serializer configures some options by default:

```csharp
JsonSerializerSettings DefaultSettings = new JsonSerializerSettings {
    ContractResolver     = new CamelCasePropertyNamesContractResolver(),
    DefaultValueHandling = DefaultValueHandling.Include,
    TypeNameHandling     = TypeNameHandling.None,
    NullValueHandling    = NullValueHandling.Ignore,
    Formatting           = Formatting.None,
    ConstructorHandling  = ConstructorHandling.AllowNonPublicDefaultConstructor
};
```

If you need to use different settings, you can supply your instance of
`JsonSerializerSettings` as a parameter for the extension method.

### Custom

You can also implement your custom serializer. To support both serialization and
deserialization, you must implement the `IRestSerializer` interface.

Here is an example of a custom serializer that uses `System.Text.Json`:

```csharp
public class SimpleJsonSerializer : IRestSerializer {
    public string Serialize(object obj) => JsonSerializer.Serialize(obj);

    public string Serialize(Parameter bodyParameter) => Serialize(bodyParameter.Value);

    public T Deserialize<T>(IRestResponse response) => JsonSerializer.Deserialize<T>(response.Content);

    public string[] SupportedContentTypes { get; } = {
        "application/json", "text/json", "text/x-json", "text/javascript", "*+json"
    };

    public string ContentType { get; set; } = "application/json";

    public DataFormat DataFormat { get; } = DataFormat.Json;
}
```

The value of the `SupportedContentTypes` property will be used to match the
serializer with the response `Content-Type` headers.

The `ContentType` property will be used when making a request so the
server knows how to handle the payload.

## Error handling

If there is a network transport error (network is down, failed DNS lookup, etc), or any kind of server error (except 404), `RestResponse.ResponseStatus` will be set to `ResponseStatus.Error`, otherwise it will be `ResponseStatus.Completed`.

If an API returns a 404, `ResponseStatus` will still be `Completed`. If you need access to the HTTP status code returned you will find it at `RestResponse.StatusCode`.
The `Status` property is an indicator of completion independent of the API error handling.

Normally, RestSharp doesn't throw an exception if the request fails.

However, it is possible to configure RestSharp to throw in different situations, when it normally doesn't throw
in favour of giving you the error as a property.

| Property                      | Behavior                                                                                                                                                                                                                                                                                         |
|-------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `FailOnDeserializationError`  | Changes the default behavior when failed deserialization results in a successful response with an empty `Data` property of the response. Setting this property to `true` will tell RestSharp to consider failed deserialization as an error and set the `ResponseStatus` to `Error` accordingly. |
| `ThrowOnDeserializationError` | Changes the default behavior when failed deserialization results in empty `Data` property of the response. Setting this property to `true` will tell RestSharp to throw when deserialization fails.                                                                                              |
| `ThrowOnAnyError`             | Setting this property to `true` changes the default behavior and forces RestSharp to throw if any errors occurs when making a request or during deserialization.                                                                                                                                 |

Those properties are available for the `RestClient` instance and will be used for all request made with that instance.

::: warning
Please be aware that deserialization failures will only work if the serializer throws an exception when deserializing the response.
Many serializers don't throw by default, and just return a `null` result. RestSharp is unable to figure out why `null` is returned, so it won't fail in this case.
Check the serializer documentation to find out if it can be configured to throw on deserialization error.
:::

There are also slight differences on how different overloads handle exceptions.

Asynchronous generic methods `GetAsync<T>`, `PostAsync<T>` and so on, which aren't a part of `RestClient` interface (those methods are extension methods) return `Task<T>`. It means that there's no `RestResponse` to set the response status to error. We decided to throw an exception when such a request fails. It is a trade-off between the API consistency and usability of the library. Usually, you only need the content of `RestResponse` instance to diagnose issues and most of the time the exception would tell you what's wrong. 

Below you can find how different extensions deal with errors. Note that functions, which don't throw by default, will throw exceptions when `ThrowOnAnyError` is set to `true`.

| Function              | Throws on errors |
|:----------------------|:-----------------|
| `ExecuteAsync`        | No |
| `ExecuteGetAsync`     | No |
| `ExecuteGetAsync<T>`  | No |
| `ExecutePostAsync`    | No |
| `ExecutePutAsync`     | No |
| `ExecuteGetAsync<T>`  | No |
| `ExecutePostAsync<T>` | No |
| `ExecutePutAsync<T>`  | No |
| `GetAsync`            | Yes |
| `GetAsync<T>`         | Yes |
| `PostAsync`           | Yes |
| `PostAsync<T>`        | Yes |
| `PatchAsync`          | Yes |
| `PatchAsync<T>`       | Yes |
| `DeleteAsync`         | Yes |
| `DeleteAsync<T>`      | Yes |
| `OptionsAsync`        | Yes |
| `OptionsAsync<T>`     | Yes |
| `HeadAsync`           | Yes |
| `HeadAsync<T>`        | Yes |

In addition, all the functions for JSON requests, like `GetJsonAsync` and `PostJsonAsyn` throw an exception if the HTTP call fails.