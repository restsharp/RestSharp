# RestSharp - Simple .NET REST Client 

RestSharp is a lightweight HTTP client library. It's a wrapper around `HttpClient`, not a full-fledged client on its own.

What RestSharp adds to `HttpClient`:
- Default parameters of any kind, not just headers
- Add a parameter of any kind to requests, like query, URL segment, header, cookie, or body
- Multiple ways to add a request body, including JSON, XML, and form data
- Built-in serialization and deserilization of JSON and XML

## RestSharp vNext

Finally, RestSharp has moved to `HttpClient`. We also deprecated the following:
- SimpleJson in favour of `System.Text.Json.JsonSerialzer`
- `IRestClient`, `IRestRequest`, and `IRestResponse` in favour of implementing classes
- Everything `Http` and `IHttp` as those are just wrappers

Most of the client and some of the request options are now in `RestClientOptions`.

Check [v107+ docs](https://restsharp.dev/v107) for more information.

| :boom:  Interfaces rage!   |
|:---------------------------|
| Before you start to rage in public about interfaces that are useful for unit-testing HTTP calls,<br>please read [this page](https://restsharp.dev/v107/#mocking). |

## Builds and Packages

### Build

| | |
|-|-|
| dev | [![](https://img.shields.io/github/workflow/status/restsharp/RestSharp/Build%20and%20deploy)](https://github.com/restsharp/RestSharp/actions?query=workflow%3A%22Build+and+deploy%22) |

### Nuget

| | |
|-|-|
| downloads | ![](https://img.shields.io/nuget/dt/RestSharp) |
| stable | [![](https://img.shields.io/nuget/v/RestSharp)](https://www.nuget.org/packages/RestSharp) |
| preview | ![](https://img.shields.io/nuget/vpre/RestSharp) |

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behavior in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

## Support

RestSharp is an open-source project with a single maintainer. Do not expect your issue to be resolved unless it concerns a large group of RestSharp users.
The best way to resolve your issue is to fix it yourself. Fork the repository and submit a pull request.
You can also motivate the maintainer by sponsoring this project.

### Contribute

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on the process for reporting issues and submitting pull requests.

### Get help

Read the docs: [Official Site][1]

Ask a question on StackOverflow with the tag `restsharp`.
 
Find RestSharp on Twitter: [@RestSharp][2]

## Community

### .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).

### Code Contributors

This project exists thanks to all the people who contribute.
<a href="https://github.com/restsharp/RestSharp/graphs/contributors"><img src="https://opencollective.com/RestSharp/contributors.svg?width=890&button=false" /></a>

### Financial Contributors

Become a financial contributor and help us sustain our community. [[Contribute](https://opencollective.com/RestSharp/contribute)]

#### Individuals

<a href="https://opencollective.com/RestSharp"><img src="https://opencollective.com/RestSharp/individuals.svg?width=890"></a>

#### Organizations

Support this project with your organization. Your logo will show up here with a link to your website. [[Contribute](https://opencollective.com/RestSharp/contribute)]

<a href="https://opencollective.com/RestSharp/organization/0/website"><img src="https://opencollective.com/RestSharp/organization/0/avatar.svg"></a>
<a href="https://opencollective.com/RestSharp/organization/1/website"><img src="https://opencollective.com/RestSharp/organization/1/avatar.svg"></a>
<a href="https://opencollective.com/RestSharp/organization/2/website"><img src="https://opencollective.com/RestSharp/organization/2/avatar.svg"></a>
<a href="https://opencollective.com/RestSharp/organization/3/website"><img src="https://opencollective.com/RestSharp/organization/3/avatar.svg"></a>
<a href="https://opencollective.com/RestSharp/organization/4/website"><img src="https://opencollective.com/RestSharp/organization/4/avatar.svg"></a>
<a href="https://opencollective.com/RestSharp/organization/5/website"><img src="https://opencollective.com/RestSharp/organization/5/avatar.svg"></a>
<a href="https://opencollective.com/RestSharp/organization/6/website"><img src="https://opencollective.com/RestSharp/organization/6/avatar.svg"></a>
<a href="https://opencollective.com/RestSharp/organization/7/website"><img src="https://opencollective.com/RestSharp/organization/7/avatar.svg"></a>
<a href="https://opencollective.com/RestSharp/organization/8/website"><img src="https://opencollective.com/RestSharp/organization/8/avatar.svg"></a>
<a href="https://opencollective.com/RestSharp/organization/9/website"><img src="https://opencollective.com/RestSharp/organization/9/avatar.svg"></a>

### License: Apache License 2.0

  [1]: https://restsharp.dev
  [2]: https://twitter.com/RestSharp
  [3]: https://github.com/restsharp/RestSharp/issues
