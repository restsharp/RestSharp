---
title: Usage
---

## Recommended usage

RestSharp works best as the foundation for a proxy class for your API. Each API would most probably require different settings for `RestClient`. Hence, a dedicated API class (and its interface) gives you sound isolation between different `RestClient` instances and make them testable.

Essentially, RestSharp is a wrapper around `HttpClient` that allows you to do the following:
- Add default parameters of any kind (not just headers) to the client, once
- Add parameters of any kind to each request (query, URL segment, form, attachment, serialized body, header) in a straightforward way
- Serialize the payload to JSON or XML if necessary
- Set the correct content headers (content type, disposition, length, etc.)
- Handle the remote endpoint response
- Deserialize the response from JSON or XML if necessary

For example, let's look at a simple Twitter API v2 client, which uses OAuth2 machine-to-machine authentication. For it to work, you would need to have access to the Twitter Developers portal, a project, and an approved application inside the project with OAuth2 enabled.

### Authenticator

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

As we need to make a call to the token endpoint, we need our own short-lived instance of `RestClient`. Unlike the actual Twitter client, it will use the `HttpBasicAuthenticator` to send the API key and secret as the username and password. The client then gets disposed as we only use it once.

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
- A constructor, which accepts API credentials to pass to the authenticator
- A wrapped `RestClient` instance with the Twitter API base URI pre-configured
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

The code above includes a couple of things that go beyond the "basics", and so we won't cover them here:
- The API client class needs to be disposable, so that it can dispose of the wrapped `HttpClient` instance
- Twitter API returns wrapped models. In this case, we use the `TwitterSingleObject` wrapper. In other methods, you'd need a similar object with `T[] Data` to accept collections

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

### Simple factory

Another way to create the client instance is to use a simple client factory. The factory will use the `BaseUrl` property of the client options to cache `HttpClient` instances. Every distinct base URL will get its own `HttpClient` instance. Other options don't affect the caching. Therefore, if you use different options for the same base URL, you'll get the same `HttpClient` instance, which will not be configured with the new options. Options that aren't applied _after_ the first client instance is created are:

* `Credentials`
* `UseDefaultCredentials`
* `AutomaticDecompression`
* `PreAuthenticate`
* `FollowRedirects`
* `RemoteCertificateValidationCallback`
* `ClientCertificates`
* `MaxRedirects`
* `MaxTimeout`
* `UserAgent`
* `Expect100Continue`

Constructor parameters to configure the `HttpMessageHandler` and default `HttpClient` headers configuration are also ignored for the cached instance as the factory only configures the handler once.

You need to set the `useClientFactory` parameter to `true` in the `RestClient` constructor to enable the factory.

```csharp
var client = new RestClient("https://api.twitter.com/2", true);
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

### Headers

Adds the header parameter as an HTTP header that is sent along with the request. The header name is the parameter's name and the header value is the value.

You can use one of the following request methods to add a header parameter:

```csharp
AddHeader(string name, string value);
AddHeader<T>(string name, T value);           // value will be converted to string
AddOrUpdateHeader(string name, string value); // replaces the header if it already exists
```

You can also add header parameters to the client, and they will be added to every request made by the client. This is useful for adding authentication headers, for example.

```csharp
client.AddDefaultHeader(string name, string value);
```

::: warning Content-Type
RestSharp will use the correct content type by default. Avoid adding the `Content-Type` header manually to your requests unless you are absolutely sure it is required. You can add a custom content type to the [body parameter](#request-body) itself.
:::

### Get or Post

`GetOrPost` behaves differently based on the method. If you execute a GET call, RestSharp will append the parameters to the Url in the form `url?name1=value1&name2=value2`.

On a POST or PUT Requests, it depends on whether you have files attached to a Request.
If not, the Parameters will be sent as the body of the request in the form `name1=value1&name2=value2`. Also, the request will be sent as `application/x-www-form-urlencoded`.

In both cases, name and value will automatically be url-encoded.

If you have files, RestSharp will send a `multipart/form-data` request. Your parameters will be part of this request in the form:

```
Content-Disposition: form-data; name="parameterName"

ParameterValue
```

You can also add `GetOrPost` parameter as a default parameter to the client. This will add the parameter to every request made by the client.

```csharp
client.AddDefaultParameter("foo", "bar");
```

It will work the same way as request parameters, except that it will be added to every request.

#### AddObject

You can avoid calling `AddParameter` multiple times if you collect all the parameters in an object, and then use `AddObject`.
For example, this code:

```csharp
var params = new {
    status = 1,
    priority = "high",
    ids = new [] { "123", "456" }
};
request.AddObject(params);
```

is equivalent to:

```csharp
request.AddParameter("status", 1);
request.AddParameter("priority", "high");
request.AddParameter("ids", "123,456");
```

Remember that `AddObject` only works if your properties have primitive types. It also works with collections of primitive types as shown above.

If you need to override the property name or format, you can do it using the `RequestProperty` attribute. For example:

```csharp
public class RequestModel {
    // override the name and the format
    [RequestAttribute(Name = "from_date", Format = "d")]
    public DateTime FromDate { get; set; }
}

// add it to the request
request.AddObject(new RequestModel { FromDate = DateTime.Now });
```

In this case, the request will get a GET or POST parameter named `from_date` and its value would be the current date in short date format.

### Url Segment

Unlike `GetOrPost`, this `ParameterType` replaces placeholder values in the `RequestUrl`:

```csharp
var request = new RestRequest("health/{entity}/status")
    .AddUrlSegment("entity", "s2");
```

When the request executes, RestSharp will try to match any `{placeholder}` with a parameter of that name (without the `{}`) and replace it with the value. So the above code results in `health/s2/status` being the url.

You can also add `UrlSegment` parameter as a default parameter to the client. This will add the parameter to every request made by the client.

```csharp
client.AddDefaultUrlSegment("foo", "bar");
```

### Cookies

You can add cookies to a request using the `AddCookie` method:

```csharp
request.AddCookie("foo", "bar");
```

RestSharp will add cookies from the request as cookie headers and then extract the matching cookies from the response. You can observe and extract response cookies using the `RestResponse.Cookies` properties, which has the `CookieCollection` type.

However, the usage of a default URL segment parameter is questionable as you can just include the parameter value to the base URL of the client. There is, however, a `CookieContainer` instance on the request level. You can either assign the pre-populated container to `request.CookieContainer`, or let the container be created by the request when you call `AddCookie`. Still, the container is only used to extract all the cookies from it and create cookie headers for the request instead of using the container directly. It's because the cookie container is normally configured on the `HttpClientHandler` level and cookies are shared between requests made by the same client. In most of the cases this behaviour can be harmful.

If your use case requires sharing cookies between requests made by the client instance, you can use the client-level `CookieContainer`, which you must provide as the options' property. You can add cookies to the container using the container API. No response cookies, however, would be auto-added to the container, but you can do it in code by getting cookies from the `Cookes` property of the response and adding them to the client-level container available via `IRestClient.Options.CookieContainer` property.

### Request Body

RestSharp supports multiple ways to add a request body:
- `AddJsonBody` for JSON payloads
- `AddXmlBody` for XML payloads
- `AddStringBody` for pre-serialized payloads

We recommend using `AddJsonBody` or `AddXmlBody` methods instead of `AddParameter` with type `BodyParameter`. Those methods will set the proper request type and do the serialization work for you.

When you make a `POST`, `PUT` or `PATCH` request and added `GetOrPost` [parameters](#get-or-post), RestSharp will send them as a URL-encoded form request body by default. When a request also has files, it will send a `multipart/form-data` request. You can also instruct RestSharp to send the body as `multipart/form-data` by setting the `AlwaysMultipartFormData` property to `true`. 

It is not possible to add client-level default body parameters.

#### AddStringBody

If you have a pre-serialized payload like a JSON string, you can use `AddStringBody` to add it as a body parameter. You need to specify the content type, so the remote endpoint knows what to do with the request body. For example:

```csharp
const json = "{ data: { foo: \"bar\" } }";
request.AddStringBody(json, ContentType.Json);
```

You can specify a custom body content type if necessary. The `contentType` argument is available in all the overloads that add a request body.

#### AddJsonBody

When you call `AddJsonBody`, it does the following for you:

- Instructs the RestClient to serialize the object parameter as JSON when making a request
- Sets the content type to `application/json`
- Sets the internal data type of the request body to `DataType.Json`

Here is the example:

```csharp
var param = new MyClass { IntData = 1, StringData = "test123" };
request.AddJsonBody(param);
```

It is possible to override the default content type by supplying the `contentType` argument. For example:

```csharp
request.AddJsonBody(param, "text/x-json");
```

If you use a pre-serialized string with `AddJsonBody`, it will be sent as-is. The `AddJsonBody` will detect if the parameter is a string and will add it as a string body with JSON content type.
Essentially, it means that top-level strings won't be serialized as JSON when you use `AddJsonBody`. To overcome this issue, you can use an overload of `AddJsonBody`, which allows you to tell RestSharp to serialize the string as JSON:

```csharp
const string payload = @"
""requestBody"": { 
    ""content"": { 
        ""application/json"": { 
            ""schema"": { 
                ""type"": ""string"" 
            } 
        } 
    } 
},";
request.AddJsonBody(payload, forceSerialize: true); // the string will be serialized
request.AddJsonBody(payload);       // the string will NOT be serialized and will be sent as-is
```

#### AddXmlBody

When you call `AddXmlBody`, it does the following for you:

- Instructs the RestClient to serialize the object parameter as XML when making a request
- Sets the content type to `application/xml`
- Sets the internal data type of the request body to `DataType.Xml`

::: warning
Do not send XML string to `AddXmlBody`; it won't work!
:::

### Query String

`QueryString` works like `GetOrPost`, except that it always appends the parameters to the url in the form `url?name1=value1&name2=value2`, regardless of the request method.

Example:

```csharp
var client = new RestClient("https://search.me");
var request = new RestRequest("search")
    .AddParameter("foo", "bar");
var response = await client.GetAsync<SearchResponse>(request);
```

It will send a `GET` request to `https://search.me/search?foo=bar")`.

For `POST`-style requests you need to add the query string parameter explicitly:

```csharp
request.AddQueryParameter("foo", "bar");
```

In some cases, you might need to prevent RestSharp from encoding the query string parameter. 
To do so, set the `encode` argument to `false` when adding the parameter:

```csharp
request.AddQueryParameter("foo", "bar/fox", false);
```

You can also add a query string parameter as a default parameter to the client. This will add the parameter to every request made by the client.

```csharp
client.AddDefaultQueryParameter("foo", "bar");
```

The line above will result in all the requests made by that client instance to have `foo=bar` in the query string for all the requests made by that client.

## Making a call

Once you've added all the parameters to your `RestRequest`, you are ready to make a request.

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

All the overloads with names starting with `Execute` don't throw an exception if the server returns an error. Read more about it [here](error-handling.md).

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

The `IRestClient` interface also has extensions for making requests without deserialization, which throw an exception if the server returns an error even if the client is configured to not throw exceptions.

```csharp
Task<RestResponse> GetAsync(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse> PostAsync(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse> PutAsync(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse> HeadAsync(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse> PatchAsync(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse> DeleteAsync(RestRequest request, CancellationToken cancellationToken)
``` 

### JSON requests

To make a simple `GET` call and get a deserialized JSON response with a pre-formed resource string, use this:

```csharp
var response = await client.GetJsonAsync<TResponse>("endpoint?foo=bar", cancellationToken);
```

You can also use a more advanced extension that uses an object to compose the resource string:

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

### JSON streaming APIs

For HTTP API endpoints that stream the response data (like [Twitter search stream](https://developer.twitter.com/en/docs/twitter-api/tweets/filtered-stream/api-reference/get-tweets-search-stream)) you can use RestSharp with `StreamJsonAsync<T>`, which returns an `IAsyncEnumerable<T>`:

```csharp
public async IAsyncEnumerable<SearchResponse> SearchStream(
    [EnumeratorCancellation] CancellationToken cancellationToken = default
) {
    var response = _client.StreamJsonAsync<TwitterSingleObject<SearchResponse>>(
        "tweets/search/stream", cancellationToken
    );

    await foreach (var item in response.WithCancellation(cancellationToken)) {
        yield return item.Data;
    }
}
```

The main limitation of this function is that it expects each JSON object to be returned as a single line. It is unable to parse the response by combining multiple lines into a JSON string.

### Uploading files

To add a file to the request you can use the `RestRequest` function called `AddFile`. The main function accepts the `FileParameter` argument:

```csharp
request.AddFile(fileParameter);
```

You can instantiate the file parameter using `FileParameter.Create` that accepts a bytes array, or `FileParameter.FromFile`, which will load the file from disk.

There are also extension functions that wrap the creation of `FileParameter` inside:

```csharp
// Adds a file from disk
AddFile(parameterName, filePath, contentType);

// Adds an array of bytes
AddFile(parameterName, bytes, fileName, contentType);

// Adds a stream returned by the getFile function
AddFile(parameterName, getFile, fileName, contentType);
```

Remember that `AddFile` will set all the necessary headers, so please don't try to set content headers manually.

### Downloading binary data

There are two functions that allow you to download binary data from the remote API.

First, there's `DownloadDataAsync`, which returns `Task<byte[]`. It will read the binary response to the end, and return the whole binary content as a byte array. It works well for downloading smaller files.

For larger responses, you can use `DownloadStreamAsync` that returns `Task<Stream>`. This function allows you to open a stream reader and asynchronously stream large responses to memory or disk.


## Reusing HttpClient

RestSharp uses `HttpClient` internally to make HTTP requests. It's possible to reuse the same `HttpClient` instance for multiple `RestClient` instances. This is useful when you want to share the same connection pool between multiple `RestClient` instances.

One way of doing it is to use `RestClient` constructors that accept an instance of `HttpClient` or `HttpMessageHandler` as an argument. Note that in that case not all the options provided via `RestClientOptions` will be used. Here is the list of options that will work:

- `BaseAddress` will be used to set the base address of the `HttpClient` instance if base address is not set there already.
- `MaxTimeout`
- `UserAgent` will be set if the `User-Agent` header is not set on the `HttpClient` instance already.
- `Expect100Continue`

Another option is to use a simple HTTP client factory as described [above](#simple-factory). 

## Blazor support

Inside a Blazor webassembly app, you can make requests to external API endpoints. Microsoft examples show how to do it with `HttpClient`, and it's also possible to use RestSharp for the same purpose.

You need to remember that webassembly has some platform-specific limitations. Therefore, you won't be able to instantiate `RestClient` using all of its constructors. In fact, you can only use `RestClient` constructors that accept `HttpClient` or `HttpMessageHandler` as an argument. If you use the default parameterless constructor, it will call the option-based constructor with default options. The options-based constructor will attempt to create an `HttpMessageHandler` instance using the options provided, and it will fail with Blazor, as some of those options throw thw "Unsupported platform" exception.

Here is an example how to register the `RestClient` instance globally as a singleton:

```csharp
builder.Services.AddSingleton(new RestClient(new HttpClient()));
```

Then, on a page you can inject the instance:

```html
@page "/fetchdata"
@using RestSharp
@inject RestClient _restClient
```

And then use it:

```csharp
@code {
    private WeatherForecast[]? forecasts;

    protected override async Task OnInitializedAsync() {
        forecasts = await _restClient.GetJsonAsync<WeatherForecast[]>("http://localhost:5104/weather");
    }

    public class WeatherForecast {
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public string? Summary { get; set; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
```

In this case, the call will be made to a WebAPI server hosted at `http://localhost:5104/weather`. Remember that if the WebAPI server is not hosting the webassembly itself, it needs to have a CORS policy configured to allow the webassembly origin to access the API endpoint from the browser.
