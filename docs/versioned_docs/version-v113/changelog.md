---
title: What's new
description: List of changes for the current major version
sidebar_position: 1
---

# Changelog

For release notes of previous versions, please check the [Releases page](https://github.com/restsharp/RestSharp/releases) in RestSharp GitHub repository.

Changes between major versions are documented in the documentation for each version on this website.

# v112.0

* Security fix for [CVE-2024-45302](https://github.com/restsharp/RestSharp/security/advisories/GHSA-4rr6-2v9v-wcpc). Header values cannot contain `CRLF`.

## v112.1

* Follow up on v112.0 security fix: remove `\t` from the list of forbidden characters in headers.

# v113.0

* Added support for .NET 9 and .NET 10
* Upgraded `System.Text.Json` to v10 for all target frameworks
* Added support for Microsoft dependency injection and HTTP client factory
* For responses with 404 (not found) status code the `IsSuccessful` is set to `false`
* When the new option `ErrorWhenUnsuccessfulStatusCode` is set to `false`, the error message and the exception won't be added to the response. Default for this option is `true` for backwards compatibility.
* When `AddUrlSegment` is called more than once with the same name, the last value will be used.
* The new package `RestSharp.Extensions.DependencyInjection` integrates RestSharp with Microsoft DI container and `IHttpClientFactory`.