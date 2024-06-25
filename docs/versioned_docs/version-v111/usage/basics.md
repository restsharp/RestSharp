---
sidebar_position: 2
---

# RestSharp basics

This page describes some of the essential properties and features of RestSharp.

## What RestSharp does

Essentially, RestSharp is a wrapper around `HttpClient` that allows you to do the following:
- Add default parameters of any kind (not just headers) to the client, once
- Add parameters of any kind to each request (query, URL segment, form, attachment, serialized body, header) in a straightforward way
- Serialize the payload to JSON or XML if necessary
- Set the correct content headers (content type, disposition, length, etc.)
- Handle the remote endpoint response
- Deserialize the response from JSON or XML if necessary

## API client

The best way to call an external HTTP API is to create a typed client, which encapsulates RestSharp calls and doesn't expose the `RestClient` instance in public.

You can find an example of a Twitter API client on the [Example](example.md) page.
