---
sidebar_position: 3
title: Creating the client
---

## Constructors

A RestSharp client can be instantiated by one of its constructors. Two most commonly used constructors are:

#### Only specify the base URL

You can create an instance of `RestClient` with only a single parameter: the base URL. Even that isn't required as base URL can be left empty. In that case, you'd need to specify the absolute path for each call. When the base URL is set, you can use both relative and absolute path.

```csharp
// Creates a client with default options to call a given base URL
var client = new RestClient("https://localhost:5000");
```

#### Provide client options

The most common way to create a client is to use the constructor with options. The options object has the type of `RestClientOptions`.
Here's an example of how to create a client using the same base path as in the previous sample, but with a couple additional settings:

```csharp
// Creates a client using the options object
var options = new RestClientOptions("https://localhost:5000") {
    MaxTimeout = 1000
};
var client = new RestClient(options);
```

#### Advanced configuration

RestSharp can be configured with more tweaks, including default request options, how it should handle responses, how serialization works, etc. You can also provide your own instance of `HttpClient` or `HttpMessageHandler`.

Read more about the advanced configuration of RestSharp on a [dedicated page](../advanced/configuration.md).

## Simple factory

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
