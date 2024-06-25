---
title: Configuration
description: Learn how to configure RestClient for non-trivial use cases.
sidebar_position: 1
---

# Configuring RestClient

This page describes how to create and configure `RestClient`.

## Basic configuration

The primary `RestClient` constructor accepts an instance of `RestClientOptions`. Most of the time, default option values don't need to be changed. However, in some cases, you'd want to configure the client differently, so you'd need to change some of the options in your code. The constructor also contains a few optional parameters for additional configuration that is not covered by client options. Here's the constructor signature:

```csharp
public RestClient(
    RestClientOptions       options,
    ConfigureHeaders?       configureDefaultHeaders = null,
    ConfigureSerialization? configureSerialization  = null,
    bool                    useClientFactory        = false
)
```

Constructor parameters are:

| Name                    | Description                                                                                                                                                 | Mandatory |
|-------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------|-----------|
| options                 | Client options                                                                                                                                              | Yes       |
| configureDefaultHeaders | Function to configure headers. Allows to configure default headers for `HttpClient`. Most of the time you'd prefer using `client.AddDefaultHeader` instead. | No        |
| configureSerialization  | Function to configure client serializers with non-default options or to use a different serializer ([learn more](serialization.md))                         | No        |
| useClientFactory        | Instructs the client to use `SimpleFactory` ([learn more](../usage/usage.md#simple-factory)) to get an `HttpClient` instance                                | No        |

Here's an example of how to create a client using client options:

```csharp
var options = new RestClientOptions("https://localhost:5000/api") {
    DisableCharset = true
};
var client = new RestClient(options);
```

When you only need to set the base URL, you can use a simplified constructor:

```csharp
var client = new RestClient("https://localhost:5000/api");
```

The simplified constructor will create an instance of client options and set the base URL provided as the constructor argument.

Finally, you can override properties of default options using a configuration function. Here's the constructor signature that supports this method:

```csharp
public RestClient(
    ConfigureRestClient?    configureRestClient     = null,
    ConfigureHeaders?       configureDefaultHeaders = null,
    ConfigureSerialization? configureSerialization  = null,
    bool                    useClientFactory        = false
)
```

For example:

```csharp
var client = new RestClient(options => {
    options.BaseUrl = new Url("https://localhost:5000/api"),
    options.DisableCharset = true
});
```

You can also provide the base URL as a constructor argument like this:

```csharp
var client = new RestClient("https://localhost:5000/api", options => {
    options.DisableCharset = true
});
```

## Using custom HttpClient

By default, RestSharp creates an instance of `HttpClient` configured using the client options, and keeps it during the lifetime of the client. When the `RestClient` instance gets disposed, it also disposes the `HttpClient` instance.

There might be a case when you need to provide your own `HttpClient`. For example, you would want to use `HttpClient` created by HTTP client factory. RestSharp allows you to do it by using additional constructors. These constructors are:

```csharp
// Create a client using an existing HttpClient and RestClientOptions (optional)
public RestClient(
    HttpClient              httpClient,
    RestClientOptions?      options,
    bool                    disposeHttpClient      = false,
    ConfigureSerialization? configureSerialization = null
)

// Create a client using an existing HttpClient and optional RestClient configuration function
public RestClient(
    HttpClient              httpClient,
    bool                    disposeHttpClient      = false,
    ConfigureRestClient?    configureRestClient    = null,
    ConfigureSerialization? configureSerialization = null
)
```

The `disposeHttpClient` argument tells the client to dispose `HttpClient` when the client itself gets disposed. It's set to `false` by default as when the `HttpClient` is provided from the outside, it should normally be disposed on the outside as well.

## Using custom message handler

Unless you use an external instance of `HttpClient`, the `RestClient` creates one when being constructed, and it will use the default HTTP message handler, configured using `RestClientOptions`. Normally, you'd get a `SocketHttpHandler` with modern .NET, and `WinHttpHandler` with .NET Framework.

There might be a case when you need to configure the HTTP message handler. For example, you want to add a delegating message handler. RestSharp allows you to do it by using additional constructors. There's one constructor that allows you to pass the custom `HttpMessageHandler`:

```csharp
public RestClient(
    HttpMessageHandler      handler,
    bool                    disposeHandler         = true,
    ConfigureRestClient?    configureRestClient    = null,
    ConfigureSerialization? configureSerialization = null
)
```

This constructor will create a new `HttpClient` instance using the provided message handler. As RestSharp will dispose the `HttpClient` instance when the `RestClient` instance gets disposed, the handler will be disposed as well. If you want to change that and keep the handler, set the `disposeHandler` parameter to `false`.

:::note
When using a custom message handler, RestSharp **will not** configure it with client options, which are normally used to configure the handler created by RestSharp.
:::

Another way to customize the message handler is to allow RestSharp to create a handler, but then configure it, or wrap it in a delegating handler. It can be done by using the `RestClientOptions.ConfigureMessageHandler` property. It can be set to a function that receives the handler created by RestSharp and returned either the same handler with different settings, or a new handler.

For example, if you want to use `MockHttp` and its handler for testing, you can do it like this:

```csharp
var mockHttp = new MockHttpMessageHandler();
// Configure the MockHttp handler to do the checks
...

var options = new RestClientOptions(Url) {
    ConfigureMessageHandler = _ => mockHttp
};
using var client = new RestClient(options);
```

In this example, we are reassigning the handler to MockHttp, so the handler created by RestSharp isn't used. In other cases you want to use delegating handlers as middleware, so you'd pass the handler created by RestSharp to the delegating handler:

```csharp
var options = new RestClientOptions(Url) {
    ConfigureMessageHandler = handler => new MyDelegatingHandler(handler)
};
using var client = new RestClient(options);
```

## Client options

RestSharp allows configuring `RestClient` using client options, as mentioned at the beginning of this page. Below, you find more details about available options.

| Option                                       | Description                                                                                                                                                                                                                                                                                       |
|----------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `BaseUrl`                                    | Client base URL. It can also be provided as the `RestClientOptions` constructor argument.                                                                                                                                                                                                         |
| `ConfigureMessageHandler`                    | Configures the HTTP message handler (see above).                                                                                                                                                                                                                                                  |
| `CalculateResponseStatus`                    | Function to calculate a different response status from `HttpResponseMessage`. By default, the request is considered as complete if it returns a successful status code or 404.                                                                                                                    |
| `Authenticator`                              | Client-level authenticator. Read more about authenticators [here](authenticators.md).                                                                                                                                                                                                             |
| `Interceptors`                               | A collector of interceptors. Read more about interceptors [here](interceptors.md).                                                                                                                                                                                                                |
| `Credentials`                                | Instance of `ICredentials` used for NTLM or Kerberos authentication. Not supported in browsers.                                                                                                                                                                                                   |
| `UseDefaultCredentials`                      | Whether to use default OS credentials for NTLM or Kerberos authentication. Not supported in browsers.                                                                                                                                                                                             |
| `DisableCharset`                             | When set to `true`, the `Content-Type` header won't have the `charset` portion. Some older web servers don't understand the `charset` portion in the header and fail to process the request.                                                                                                      |
| `AutomaticDecompression`                     | Allows customizing supported decompression methods. Default is `All` except for .NET Framework that only support `GZip`. Not supported in browsers.                                                                                                                                               |
| `MaxRedirects`                               | The number of redirects to follow. Not supported in browsers.                                                                                                                                                                                                                                     |
| `ClientCertificates`                         | A collection of X.509 client certificates to be used for authentication. Not supported in browsers.                                                                                                                                                                                               |
| `Proxy`                                      | Can be used if the client needs to use an explicit, non-default proxy. Not supported in browsers, on iOS and tvOS.                                                                                                                                                                                |
| `CachePolicy`                                | Shortcut for setting the default value for `Cache-Control` header.                                                                                                                                                                                                                                |
| `FollowRedirects`                            | Instructs the client to follow redirects. Default is `true`.                                                                                                                                                                                                                                      |
| `Expect100Continue`                          | Gets or sets a value that indicates if the `Expect` header for an HTTP request contains `Continue`.                                                                                                                                                                                               |
| `UserAgent`                                  | Allows overriding the default value for `User-Agent` header, which is `RestSharp/{version}`.                                                                                                                                                                                                      |
| `PreAuthenticate`                            | Gets or sets a value that indicates whether the client sends an `Authorization` header with the request. Not supported in browsers.                                                                                                                                                               |
| `RemoteCertificateValidationCallback`        | Custom function to validate the server certificate. Normally, it's used when the server uses a certificate that isn't trusted by default.                                                                                                                                                         |
| `BaseHost`                                   | Value for the `Host` header sent with each request.                                                                                                                                                                                                                                               |
| `CookieContainer`                            | Custom cookie container that will be shared among all calls made by the client. Normally not required as RestSharp handles cookies without using a client-level cookie container.                                                                                                                 |
| `MaxTimeout`                                 | Client-level timeout in milliseconds. If the request timeout is also set, this value isn't used.                                                                                                                                                                                                  |
| `Encoding`                                   | Default request encoding. Override it only if you don't use UTF-8.                                                                                                                                                                                                                                |
| `ThrowOnDeserializationError`                | Forces the client to throw if it fails to deserialize the response. Remember that not all deserialization issues forces the serializer to throw. Default is `false`, so the client will return a `RestResponse` with deserialization exception details. Only relevant for `Execute...` functions. |
| `FailOnDeserializationError`                 | When set to `true`, if the client fails to deserialize the response, the response object will have status `Failed`, although the HTTP calls might have been successful. Default is `true`.                                                                                                        |
| `ThrowOnAnyError`                            | When set to `true`, the client will re-throw any exception from `HttpClient`. Default is `false`. Only applies for `Execute...` functions.                                                                                                                                                        |
| `AllowMultipleDefaultParametersWithSameName` | By default, adding parameters with the same name is not allowed. You can override this behaviour by setting this property to `true`.                                                                                                                                                              |
| `Encode`                                     | A function to encode URLs, the default is a custom RestSharp function based on `Uri.EscapeDataString()`. Set it if you need a different way to do the encoding.                                                                                                                                   |
| `EncodeQuery`                                | A function to encode URL query parameters. The default is the same function as for `Encode` property.                                                                                                                                                                                             |

Some of the options are used by RestSharp code, but some are only used to configure the `HttpMessageHandler`. These options are:
- `Credentials`
- `UseDefaultCredentials`
- `AutomaticDecompression`
- `PreAuthenticate`
- `MaxRedirects`
- `RemoteCertificateValidationCallback`
- `ClientCertificates`
- `FollowRedirects`
- `Proxy`

:::note
If setting these options to non-default values produce no desirable effect, check if your framework and platform supports them. RestSharp doesn't change behaviour based on values of those options.
:::

The `IRestClient` interface exposes the `Options` property, so any option can be inspected at runtime. However, RestSharp converts the options object provided to the client constructor to an immutable object. Therefore, no client option can be changed after the client is instantiated. It's because changing client options at runtime can produce issues in concurrent environments, effectively rendering the client as not thread-safe. Apart from that, changing the options that are used to create the message handler would require re-creating the handler, and also `HttpClient`, which should not be done at runtime.

## Configuring requests

Client options apply to all requests made by the client. Sometimes, you want to fine-tune particular requests, so they execute with custom configuration. It's possible to do using properties of `RestRequest`, described below.

| Name                         | Description                                                                                                                                                                                                                                                                                                       |
|------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `AlwaysMultipartFormData`    | When set to `true`, the request will be sent as a multipart form, even though it's not required. By default, RestSharp only sends requests with multiple attachments as multipart forms. Default is `false`.                                                                                                      |
| `AlwaysSingleFileAsContent`  | When set to true, the request with file attachment will not be sent as a multipart form, but as plain content. Default is `false`. It cannot be set to `true` when `AlwaysMultipartFormData` is set to `true`, or when the request has `POST` parameters.                                                         |
| `MultipartFormQuoteBoundary` | Default is `true`, which means that the form boundary string will be wrapped in quotes. If the server has an issue with that, setting this to `false` will remove quotes around the boundary.                                                                                                                     |
| `FormBoundary`               | Allows specifying a custom multipart form boundary instead of using the default random string.                                                                                                                                                                                                                    |
| `RequestParameters`          | Collection of request parameters. Normally, you won't need to use it as parameters are added to the request using `Add...` functions.                                                                                                                                                                             |
| `CookieContainer`            | Custom request-level cookie container. Default is `null`. You can still set request cookies using `AddCookie` and get response cookies from the response object without using cooking container.                                                                                                                  |
| `Authenticator`              | Overrides the client-level authenticator.                                                                                                                                                                                                                                                                         |
| `Files`                      | Collection of file parameters, read-only. Use `AddFile` for adding files to the request.                                                                                                                                                                                                                          |
| `Method`                     | Request HTTP method, default is `GET`. Only needed when using `Execute` or `ExecuteAsync` as other functions like `ExecutePostAsync` will override the request method.                                                                                                                                            |
| `TImeout`                    | Overrides the client-level timeout.                                                                                                                                                                                                                                                                               |
| `Resource`                   | Resource part of the remote endpoint URL. For example, when using the client-level base URL `https://localhost:5000/api` and `Resource` set to `weather`, the request will be sent to `https://localhost:5000/api/weather`. It can container resource placeholders to be used in combination with `AddUrlSegment` |
| `RequestFormat`              | Identifies the request as JSON, XML, binary, or none. Rarely used because the client will set the request format based on the body type if functions like `AddJsonBody` or `AddXmlBody` are used.                                                                                                                 |
| `RootElement`                | Used by the default deserializers to determine where to start deserializing from. Only supported for XML responses. Does not apply to requests.                                                                                                                                                                   |
| `OnBeforeDeserialization`    | **Obsolete** A function to be called before the response is deserializer. Allows changing the content before calling the deserializer. Use [interceptors](interceptors.md) instead.                                                                                                                               |
| `OnBeforeRequest`            | **Obsolete** A function to be called right before the request is executed by `HttpClient`. It receives an instance of `HttpRequestMessage`. Use [interceptors](interceptors.md) instead.                                                                                                                          |
| `OnAfterRequest`             | **Obsolete** A function to be called right after the request is executed by `HttpClient`. It receives an instance of `HttpResponseMessage`. Use [interceptors](interceptors.md) instead.                                                                                                                          |
| `Attempts`                   | When the request is being resent to retry, the property value increases by one.                                                                                                                                                                                                                                   |
| `CompletionOption`           | Instructs the client on when it should consider the request to be completed. The default is `ResponseContentRead`. It is automatically changed to `ResponseHeadersRead` when using async download functions or streaming.                                                                                         |
| `CachePolicy`                | Overrides the client cache policy.                                                                                                                                                                                                                                                                                |
| `ResponseWriter`             | Allows custom handling of the response stream. The function gets the raw response stream and returns another stream or `null`. Cannot be used in combination with `AdvancedResponseWriter`.                                                                                                                       |
| `AdvancedResponseWriter`     | Allows custom handling of the response. The function gets an instance of `HttpResponseMessage` and an instance of `RestRequest`. It must return an instance of `RestResponse`, so it effectively overrides RestSharp default functionality for creating responses.                                                |
| `Interceptors`               | Allows adding interceptors to the request. Both client-level and request-level interceptors will be called.                                                                                                                                                                                                       |

The table below contains all configuration properties of `RestRequest`. To learn more about adding request parameters, check the [usage page](../usage/usage.md#create-a-request) section about creating requests with parameters.
