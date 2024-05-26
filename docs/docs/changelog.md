---
title: What's new
description: List of changes per RestSharp version.
sidebar_position: 2
---

# Changelog

This changelog is only maintained since v111. For release notes of previous versions, please check the [Releases page](https://github.com/restsharp/RestSharp/releases) in RestSharp GitHub repository.

Only the most important or breaking changes are listed there. All other changes can be found in each release on GitHub.

## v111

* Added [interceptors](advanced/interceptors.md).
* As interceptors provide a better way to interject the request and response execution flow, request properties `OnBeforeRequest`, `OnBeforeDeserialization` and `OnAfterRequest` are marked obsolete and will be removed in future versions.
* **Breaking change.** Client option `MaxTimeout` renamed to `Timeout` and changed type to `Timespan` for clarity. It doesn't configure the `HttpClient` timeout anymore. Instead, the same method is used for client and request level timeouts with cancellation tokens.
* **Breaking change.** Request option `Timeout` changed type to `Timespan` for clarity.
* Added .NET 8 target.
* Support uploading files as content without multipart form.
* Added `CacheControl` options to client and requests.
* Allow using `AddJsonBody` to serialize top-level strings.
