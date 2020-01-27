# Common Issues

Before opening an issue on GitHub, please check the list of known issues below.

## Connection closed with SSL

When connecting via HTTPS, you get an exception:

> The underlying connection was closed: An unexpected error occurred on a send

The exception is thrown by `WebRequest` so you need to tell the .NET Framework to
accept more certificate types than it does by default.

Adding this line somewhere in your application, where it gets called once, should solve the issue:

```csharp
ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
```

## Setting the User Agent

Due to the specifics of how `WebRequest` works, setting the user agent
on the request won't work when you use `AddHeader`.

Instead, please use the `IRestClient.UserAgent` property.

## Empty Response

We regularly get issues where developers complain that their requests get executed
and they get a proper raw response, but the `RestResponse<T>` instance doesn't 
have a deserialized object set.

In other situations, the raw response is also empty.

All those issues are caused by the design choice to swallow exceptions
that occur when RestSharp makes the request and processes the response. Instead,
RestSharp produces so-called _error response_.

You can check the response status to find out if there're any errors.
The following properties can tell you about those errors:

 - `IsSuccessful`
 - `ResponseStatus`
 - `StatusCode`
 - `ErrorMessage`
 - `ErrorException`
 
It could be that the request was executed and you got `200 OK` status
code back and some content, but the typed `Data` property is empty.

In that case, you probably got deserialization issues. By default, RestSharp will just return an empty (`null`) result in the `Data` property.
Deserialization errors can be also populated to the error response. To do that,
set the `client.FailOnDeserializationError` property to `true`.

It is also possible to force RestSharp to throw an exception.

If you set `client.ThrowOnDeserializationError`, RestSharp will throw a `DeserializationException`
when the serializer throws. The exception has the internal exception and the response.

Finally, by setting `ThrowOnAnyError` you can force RestSharp to re-throw any
exception that happens when making the request and processing the response.
