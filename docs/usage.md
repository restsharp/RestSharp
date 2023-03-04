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

Adds the parameter as an HTTP header that is sent along with the request. The header name is the parameter's name and the header value is the value.

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

#### AddJsonBody

When you call `AddJsonBody`, it does the following for you:

- Instructs the RestClient to serialize the object parameter as JSON when making a request
- Sets the content type to `application/json`
- Sets the internal data type of the request body to `DataType.Json`

::: warning
Do not send JSON string or some sort of `JObject` instance to `AddJsonBody`; it won't work! Use `AddStringBody` instead.
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
