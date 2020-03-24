# Error Handling

If there is a network transport error (network is down, failed DNS lookup, etc), `RestResponse.ResponseStatus` will be set to `ResponseStatus.Error`, otherwise it will be `ResponseStatus.Completed`. 

If an API returns a 404, `ResponseStatus` will still be `Completed`. If you need access to the HTTP status code returned you will find it at `RestResponse.StatusCode`. 
The `Status` property is an indicator of completion independent of the API error handling.

Normally, RestSharp doesn't throw an exception if the request fails.

However, it is possible to configure RestSharp to throw in different situations, when it normally doesn't throw
in favour of giving you the error as a property.

| Property        | Behavior           |
| ------------- |-------------|
| `FailOnDeserializationError` | Changes the default behavior when failed deserialization results in a successful response with an empty `Data` property of the response. Setting this property to `true` will tell RestSharp to consider failed deserialization as an error and set the `ResponseStatus` to `Error` accordingly. |
| `ThrowOnDeserializationError` | Changes the default behavior when failed deserialization results in empty `Data` property of the response. Setting this property to `true` will tell RestSharp to throw when deserialization fails. |
| `ThrowOnAnyError` | Setting this property to `true` changes the default behavior and forces RestSharp to throw if any errors occurs when making a request or during deserialization.     |

Those properties are available for the `IRestClient` instance and will be used for all request made with that instance.

There are also slight differences on how different overloads handle exceptions.

Asynchronous generic methods `GetAsync<T>`, `PostAsync<T>` and so on, which aren't a part of `IRestClient` interface
(those methods are extension methods) return `Task<T>`. It means that there's no `IRestResponse` to set the response status to error.
We decided to throw an exception when such a request fails. It is a trade-off between the API
consistency and usability of the library. Usually, you only need the content of `RestResponse` instance to diagnose issues
and most of the time the exception would tell you what's wrong. 
