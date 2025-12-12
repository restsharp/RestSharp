# Error handling

If there is a network transport error (network is down, failed DNS lookup, etc.), or any kind of server error (except 404), `RestResponse.ResponseStatus` will be set to `ResponseStatus.Error`, otherwise it will be `ResponseStatus.Completed`.

If an API returns a 404, `ResponseStatus` will still be `Completed`. If you need access to the HTTP status code returned, you will find it at `RestResponse.StatusCode`.
The `Status` property is an indicator of completion independent of the API error handling.

Normally, RestSharp doesn't throw an exception if the request fails.

However, it is possible to configure RestSharp to throw in different situations when it normally doesn't throw
in favor of giving you the error as a property.

| Property                      | Behavior                                                                                                                                                                                                                                                                                         |
|-------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `FailOnDeserializationError`  | Changes the default behavior when failed deserialization results in a successful response with an empty `Data` property of the response. Setting this property to `true` will tell RestSharp to consider failed deserialization as an error and set the `ResponseStatus` to `Error` accordingly. |
| `ThrowOnDeserializationError` | Changes the default behavior when failed deserialization results in empty `Data` property of the response. Setting this property to `true` will tell RestSharp to throw when deserialization fails.                                                                                              |
| `ThrowOnAnyError`             | Setting this property to `true` changes the default behavior and forces RestSharp to throw if any errors occurs when making a request or during deserialization.                                                                                                                                 |

Those properties are available for the `RestClientOptions` and will be used for all request made with the client instance.

For example, you can configure the client to throw an exception if any error occurs when making a request or when a request returns a non-successful HTTP status code:

```csharp
var options = new RestClientOptions(url) {
    ThrowOnAnyError = true
};
var client = new RestClient(options);
var request = new RestRequest("resource/{id}").AddUrlSegment("id", 123);

// 👇 will throw if the request fails
var deserialized = await client.GetAsync<ResponseModel>(request);

// 👇 will NOT throw if the request fails, inspect the response to find out what happened
var response = await client.ExecuteGetAsync<ResponseModel>(request);
```

:::warning
Please be aware that deserialization failures will only work if the serializer throws an exception when deserializing the response.
Many serializers don't throw by default, and just return a `null` result. RestSharp is unable to figure out why `null` is returned, so it won't fail in this case.
Check the serializer documentation to find out if it can be configured to throw on deserialization error.
:::

There are also slight differences on how different overloads handle exceptions.

Asynchronous generic methods `GetAsync<T>`, `PostAsync<T>` and so on, which aren't a part of `RestClient` API (those methods are extension methods) return `Task<T>`. It means that there's no `RestResponse` to set the response status to error. We decided to throw an exception when such a request fails. It is a trade-off between the API consistency and usability of the library. Usually, you only need the content of `RestResponse` instance to diagnose issues and most of the time the exception would tell you what's wrong.

Below, you can find how different extensions deal with errors. Note that functions, which don't throw by default, will throw exceptions when `ThrowOnAnyError` is set to `true`.

| Function              | Throws on errors |
|:----------------------|:-----------------|
| `ExecuteAsync`        | No               |
| `ExecuteGetAsync`     | No               |
| `ExecuteGetAsync<T>`  | No               |
| `ExecutePostAsync`    | No               |
| `ExecutePostAsync<T>` | No               |
| `ExecutePutAsync`     | No               |
| `ExecutePutAsync<T>`  | No               |
| `GetAsync`            | Yes              |
| `GetAsync<T>`         | Yes              |
| `PostAsync`           | Yes              |
| `PostAsync<T>`        | Yes              |
| `PatchAsync`          | Yes              |
| `PatchAsync<T>`       | Yes              |
| `DeleteAsync`         | Yes              |
| `DeleteAsync<T>`      | Yes              |
| `OptionsAsync`        | Yes              |
| `OptionsAsync<T>`     | Yes              |
| `HeadAsync`           | Yes              |
| `HeadAsync<T>`        | Yes              |

In addition, all the functions for JSON requests, like `GetJsonAsync` and `PostJsonAsync` throw an exception if the HTTP call fails.
