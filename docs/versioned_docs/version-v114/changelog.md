---
title: What's new
description: List of changes for the current major version
sidebar_position: 1
---

# Changelog

For release notes of previous versions, please check the [Releases page](https://github.com/restsharp/RestSharp/releases) in RestSharp GitHub repository.

Changes between major versions are documented in the documentation for each version on this website.

## v114.0

### Breaking changes

* **`IAuthenticator` interface signature changed** — `Authenticate` now accepts an optional `CancellationToken` parameter. All custom authenticator implementations must be recompiled. ([#2362](https://github.com/restsharp/RestSharp/pull/2362))
* **`FollowRedirects` and `MaxRedirects` removed from `ReadOnlyRestClientOptions`** — these properties are now excluded from the generated immutable wrapper. Use `RedirectOptions` instead. Code accessing these properties on `ReadOnlyRestClientOptions` must be updated. ([#2360](https://github.com/restsharp/RestSharp/pull/2360))
* **Extension method signatures changed** — `AddParameter<T>`, `AddOrUpdateParameter<T>`, `AddHeader<T>`, `AddOrUpdateHeader<T>`, `AddQueryParameter<T>`, and `AddUrlSegment<T>` now have an additional optional `CultureInfo? culture` parameter. Assemblies compiled against v113 must be recompiled. ([#2354](https://github.com/restsharp/RestSharp/pull/2354))

### New features

* **OAuth2 token lifecycle authenticators** — new `OAuth2ClientCredentialsAuthenticator`, `OAuth2RefreshTokenAuthenticator`, and `OAuth2TokenAuthenticator` that handle obtaining, caching, and refreshing tokens automatically. ([#2362](https://github.com/restsharp/RestSharp/pull/2362))
* **Custom redirect handling with `RedirectOptions`** — RestSharp now manages redirects internally instead of delegating to `HttpClient`, fixing lost `Set-Cookie` headers on redirects. New `RedirectOptions` class provides fine-grained control over redirect behavior. ([#2360](https://github.com/restsharp/RestSharp/pull/2360))
* **`MergedParameters` on `RestResponse`** — provides a combined view of request and default parameters at execution time, useful for logging and debugging. ([#2349](https://github.com/restsharp/RestSharp/pull/2349))
* **Restored `AddCookie(name, value)` overload** — the simple two-parameter form defers domain resolution to execution time. ([#2351](https://github.com/restsharp/RestSharp/pull/2351))

### Behavior changes

* **`MultipartFormQuoteParameters` now defaults to `true`** — multipart form parameter names are now quoted per RFC 7578. ([#2357](https://github.com/restsharp/RestSharp/pull/2357))
* **`InvariantCulture` used for parameter formatting** — all generic parameter methods now format values using `CultureInfo.InvariantCulture` by default. Pass a `CultureInfo` explicitly if locale-specific formatting is needed. ([#2354](https://github.com/restsharp/RestSharp/pull/2354))
* **Improved `ErrorMessage` for timeouts** — now shows `"The request timed out."` instead of `"A task was canceled."`. ([#2356](https://github.com/restsharp/RestSharp/pull/2356))
* **`ErrorMessage` surfaces root cause** — uses `GetBaseException().Message` to show the actual error instead of generic wrapper messages. ([#2352](https://github.com/restsharp/RestSharp/pull/2352))
* **`HttpClient.DefaultRequestHeaders` no longer modified** — `Expect100Continue` is now set per-request. Safe to share an `HttpClient` across multiple `RestClient` instances. ([#2363](https://github.com/restsharp/RestSharp/pull/2363))

### Bug fixes

* Fix `ConfigureAwait(false)` missing on several `await` calls, preventing deadlocks in sync-over-async scenarios. ([#2367](https://github.com/restsharp/RestSharp/pull/2367))
* Fix `ResponseUri` returning original URL instead of redirect target when `FollowRedirects=false`. ([#2350](https://github.com/restsharp/RestSharp/pull/2350))
* Fix default parameter merging bugs (multi-value dedup, request mutation, `BuildUriString` without execute). ([#2349](https://github.com/restsharp/RestSharp/pull/2349))
* Fix OAuth1 double-encoding of RFC 3986 special characters in URL paths. ([#2341](https://github.com/restsharp/RestSharp/pull/2341))
* Fix pipe character encoding when `AddQueryParameter` is used with `encode=false`. ([#2345](https://github.com/restsharp/RestSharp/pull/2345))
* Fix `XmlDeserializer` when XML uses same tag name in nested elements. ([#2339](https://github.com/restsharp/RestSharp/pull/2339))
* Fix credential/`UseDefaultCredentials` property order on `HttpClientHandler`. ([#2353](https://github.com/restsharp/RestSharp/pull/2353))
* Fix URL escaping on .NET Framework 4.6.2. ([#2327](https://github.com/restsharp/RestSharp/pull/2327))
