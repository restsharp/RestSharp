# Introduction

The main purpose of RestSharp is to make synchronous and asynchronous calls to remote resources over HTTP. As the name suggests, the main audience of RestSharp are developers tha use REST APIs. However, RestSharp can call any API over HTTP (but not HTTP/2), as long as you have the resource URI and request parameters that you want to send comply with W3C HTTP standards.

One of the main challenges of using HTTP APIs for .NET developers is to work with requests and responses of different kinds and translate them to complex C# types. RestSharp can take care of serializing the request body to JSON or XML and deserialize the response. It can also form a valid request URI based on different parameter kinds - path, query, form or body.

Check the [Getting started](getting-started.md) page to learn about using RestSharp in your application. 
