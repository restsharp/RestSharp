---
sidebar_position: 6
title: Handling responses
---

All `Execute{Method}Async` functions return an instance of `RestResponse`. Similarly, `Execute{Method}Async<T>` return a generic instance of `RestResponse<T>` where `T` is the response object type.

Response object contains the following properties:

| Property                 | Type                                                | Description                                                                                                                  |
|--------------------------|-----------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------|
| `Request`                | `RestRequest`                                       | Request instance that was used to get the response.                                                                          |
| `ContentType`            | `string?`                                           | Response content type. `Null` if response has no content.                                                                    |
| `ContentLength`          | `long?`                                             | Response content length. `Null` if response has no content.                                                                  |
| `ContentEncoding`        | `ICollection<string>`                               | Content encoding collection. Empty if response has no content.                                                               |
| `Content`                | `string?`                                           | Response content as string. `Null` if response has no content.                                                               |
| `IsSuccessfulStatusCode` | `bool`                                              | Indicates if response was successful, so no errors were reported by the server. Note that `404` response code means success. |
| `ResponseStatus`         | `None`, `Completed`, `Error`, `TimedOut`, `Aborted` | Response completion status. Note that completed responses might still return errors.                                         |
| `IsSuccessful`           | `bool`                                              | `True` when `IsSuccessfulStatusCode` is `true` and `ResponseStatus` is `Completed`.                                          |
| `StatusDescription`      | `string?`                                           | Response status description, if available.                                                                                   |
| `RawBytes`               | `byte[]?`                                           | Response content as byte array. `Null` if response has no content.                                                           |
| `ResponseUri`            | `Uri?`                                              | URI of the response, which might be different from request URI in case of redirects.                                         |
| `Server`                 | `string?`                                           | Server header value of the response.                                                                                         |
| `Cookies`                | `CookieCollection?`                                 | Collection of cookies received with the response, if any.                                                                    |
| `Headers`                | Collection of `HeaderParameter`                     | Response headers.                                                                                                            |
| `ContentHeaders`         | Collection of `HeaderParameter`                     | Response content headers.                                                                                                    |
| `ErrorMessage`           | `string?`                                           | Transport or another non-HTTP error generated while attempting request.                                                      |
| `ErrorException`         | `Exception?`                                        | Exception thrown when executing the request, if any.                                                                         |
| `Version`                | `Version?`                                          | HTTP protocol version of the request.                                                                                        |
| `RootElement`            | `string?`                                           | Root element of the serialized response content, only works if deserializer supports it.                                     |

In addition, `RestResponse<T>` has one additional property:

| Property | Type | Description                                                                                                                                               |
|----------|------|-----------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Data`   | `T?` | Deserialized response object. `Null` if there's no content in the response, deserializer failed to understand the response content, or if request failed. |
