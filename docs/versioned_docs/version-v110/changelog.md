---
title: What's new
description: List of changes for the current major version
sidebar_position: 1
---

# Changelog

For release notes of previous versions, please check the [Releases page](https://github.com/restsharp/RestSharp/releases) in RestSharp GitHub repository.

## What's Changed
* Added default parameters to the request. They got missing somehow.
* Consider the boundary quotes request option value.
* Made `BuildUrl` an extension so it can be used publicly.
* Added client-level cookie container.

## Breaking change

The `IRestClient` interface signature is different, so any non-standard implementations need to adopt the changes.

To keep `DefaultParameters` thread-safe, it got a new type `DefaultParameters`, and request property `Parameters` has a dedicated type `RequestParameter`. Code-wise the change is non-breaking as the signatures are the same, but v110 is not binary compatible with previous versions. The difference is that `DefaultParameters` collection wraps all its mutations in a lock.

**Full Changelog**: https://github.com/restsharp/RestSharp/compare/109.0.1...110.0.0

