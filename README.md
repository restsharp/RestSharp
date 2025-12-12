# RestSharp - Simple .NET REST Client 

![](https://img.shields.io/nuget/dt/RestSharp) [![](https://img.shields.io/nuget/v/RestSharp)](https://www.nuget.org/packages/RestSharp) [![](https://img.shields.io/nuget/vpre/RestSharp)](https://www.nuget.org/packages/RestSharp#versions-body-tab)

RestSharp is a lightweight HTTP API client library. It's a wrapper around `HttpClient`, not a full-fledged client on 
its own.

What RestSharp adds to `HttpClient`:
- Default parameters of any kind, not just headers
- Add a parameter of any kind to requests, like query, URL segment, header, cookie, or body
- Multiple ways to add a request body, including JSON, XML, URL-encoded form data, multipart form data with and 
  without files
- Built-in serialization and deserilization of JSON, XML, and CSV, as well as the ability to add custom serializers
- Rich support for authentication

## Compatibility note

RestSharp 107 was a major release that brings a lot of changes. We've removed a lot of legacy code and added new 
features. Finally, RestSharp has moved to `HttpClient`. We also deprecated the following:
- SimpleJson in favour of `System.Text.Json.JsonSerialzer`
- `IRestRequest`, and `IRestResponse` in favour of implementing classes
- Everything `Http` and `IHttp` as those are just wrappers
- Client configuration moved to `RestClientOptions` to make the client thread-safe
- `IRestClient` interface surface substantially reduced

Most of the client and some of the request options are now in `RestClientOptions`.

Check [v107+ docs](https://restsharp.dev/v107) for more information.

## Packages

| Package                                | What it's for                                                                        |
|----------------------------------------|--------------------------------------------------------------------------------------|
| `RestSharp`                            | The core library, including `System.Text.Json` serializer and basical XML serializer |
| `RestSharp.Serializers.NewtonsoftJson` | Use `Newtonsoft.Json` as a JSON serializer                                           |
| `RestSharp.Serializers.Xml`            | Use custom RestSharp XML serializer for XML                                          |
| `RestSharp.Serializers.CsvHelper`      | Use `CsvHelper` as a CSV serializer                                                  |

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behavior in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

**Users violated the aforementioned code of conduct will be blocked.**

## Support

RestSharp is an open-source project with a single maintainer. Do not expect your issue to be resolved unless it concerns a large group of RestSharp users.
The best way to resolve your issue is to fix it yourself. Fork the repository and submit a pull request.
You can also motivate the maintainer by sponsoring this project.

### Contribute

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on the process for reporting issues and submitting pull requests.

### Get help

Read the docs: [Official Site][1]

Ask a question on StackOverflow with the tag `restsharp`.
 
Join RestSharp Discord server: [![Discord](https://img.shields.io/discord/1224723555053207612?label=Discord)](https://discord.gg/NdpzHZ2qep)

Find RestSharp on Twitter: [@RestSharp][2]

## Community

### .NET Foundation

This project is a part of the [.NET Foundation](https://dotnetfoundation.org).

### Code Contributors

This project exists thanks to all the people who contribute.
[<img src="https://opencollective.com/RestSharp/contributors.svg?width=890&button=false">](https://github.com/restsharp/RestSharp/graphs/contributors)

### Financial Contributors

Become a financial contributor and help us sustain our community. [Contribute](https://github.com/sponsors/restsharp)

## License

[Apache License 2.0](https://github.com/restsharp/RestSharp/blob/dev/LICENSE.txt)

  [1]: https://restsharp.dev
  [2]: https://twitter.com/RestSharp
  [3]: https://github.com/restsharp/RestSharp/issues
