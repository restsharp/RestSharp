---
sidebar_position: 5
title: Making calls
---

## Executing requests

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
Task<RestResponse> ExecuteDeleteAsync(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse> ExecuteHeadAsync(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse> ExecuteOptionsAsync(RestRequest request, CancellationToken cancellationToken)
```

When using any of those methods, you will get the response content as string in `response.Content`.

RestSharp can deserialize the response for you. To use that feature, use one of the generic overloads:

```csharp
Task<RestResponse<T>> ExecuteAsync<T>(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse<T>> ExecuteGetAsync<T>(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse<T>> ExecutePostAsync<T>(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse<T>> ExecutePutAsync<T>(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse<T>> ExecuteDeleteAsync<T>(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse<T>> ExecuteHeadAsync<T>(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse<T>> ExecuteOptionsAsync<T>(RestRequest request, CancellationToken cancellationToken)
```

:::note Beware of errors
All the overloads with names starting with `Execute` don't throw an exception if the server returns an error. Read more about it [here](../advanced/error-handling.md).
It allows you to inspect responses and handle remote server errors gracefully. Overloads without `Execute` prefix throw exceptions in case of any error, so you'd need to ensure to handle exceptions properly.
:::

If you just need a deserialized response, you can use one of the extensions:

```csharp
Task<T> GetAsync<T>(RestRequest request, CancellationToken cancellationToken)
Task<T> PostAsync<T>(RestRequest request, CancellationToken cancellationToken)
Task<T> PutAsync<T>(RestRequest request, CancellationToken cancellationToken)
Task<T> PatchAsync<T>(RestRequest request, CancellationToken cancellationToken)
Task<T> DeleteAsync<T>(RestRequest request, CancellationToken cancellationToken)
Task<T> HeadAsync<T>(RestRequest request, CancellationToken cancellationToken)
Task<T> OptionsAsync<T>(RestRequest request, CancellationToken cancellationToken)
```

Those extensions will throw an exception if the server returns an error, as there's no other way to float the error back to the caller.

The `IRestClient` interface also has extensions for making requests without deserialization, which throw an exception if the server returns an error even if the client is configured to not throw exceptions.

```csharp
Task<RestResponse> GetAsync(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse> PostAsync(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse> PutAsync(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse> PatchAsync(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse> DeleteAsync(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse> HeadAsync(RestRequest request, CancellationToken cancellationToken)
Task<RestResponse> OptionsAsync(RestRequest request, CancellationToken cancellationToken)
``` 

### Sync calls

The preferred way for making requests is to execute them asynchronously as HTTP calls are IO-bound operations.
If you are unable to make async calls, all the functions about have sync overloads, which have the same names without `Async` suffix.
For example, for making a sync `GET` call you can use `ExecuteGet(request)` or `Get<Response>`, etc.

## Requests without body

Some HTTP methods don't suppose to be used with request body. For those methods, RestSharp supports making simplified calls without using `RestRequest`. All you need is to provide the resource path as a string.

For example, you can make a `DELETE` call like this:

```csharp
var response = await client.ExecuteDeleteAsync($"order/delete/{orderId}", cancellationToken);
```

Similarly, you can make `GET` calls with or without deserialization of the response using `ExecuteGetAsync(resource)`, `GetAsync(resource)`, `ExecuteGetAsync<TResponse>(resource)`, and `GetAsync<TResponse>(resource)` (see below).

## JSON requests

RestSharp provides an easier API for making calls to endpoints that accept and return JSON.

### GET calls

To make a simple `GET` call and get a deserialized JSON response with a pre-formed resource string, use this:

```csharp
var response = await client.GetAsync<TResponse>("endpoint?foo=bar", cancellationToken);
```

:::note
In v111, `GetJsonAsync<T>` is renamed to `GetAsync<T>`.
:::

You can also use a more advanced extension that uses an object to compose the resource string:

```csharp
var client = new RestClient("https://example.org");
var args = new {
    id = "123",
    foo = "bar"
};
// Will make a call to https://example.org/endpoint/123?foo=bar
var response = await client.GetAsync<TResponse>("endpoint/{id}", args, cancellationToken);
```

It will search for the URL segment parameters matching any of the object properties and replace them with values. All the other properties will be used as query parameters.

One note about `GetAsync<T>` is that it will deserialize the response with any supported content type, not only JSON.

### POST calls

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

## Downloading binary data

There are two functions that allow you to download binary data from the remote API.

First, there's `DownloadDataAsync`, which returns `Task<byte[]`. It will read the binary response to the end, and return the whole binary content as a byte array. It works well for downloading smaller files.

For larger responses, you can use `DownloadStreamAsync` that returns `Task<Stream>`. This function allows you to open a stream reader and asynchronously stream large responses to memory or disk.

## JSON streaming

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

