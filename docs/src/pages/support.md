---
id: "support"
---

# Get help

Got issues, questions, suggestions? Please read this page carefully to understand how you can get help working with RestSharp.

:::tip
You can also support maintainers and motivate them by contributing via [GitHub Sponsors](https://github.com/sponsors/restsharp).
:::

## Questions

The most effective way to resolve questions about using RestSharp is StackOverflow.

RestSharp has a large user base. Tens of thousands of projects and hundreds of thousands of developers
use RestSharp on a daily basis. So, asking questions on **StackOverflow** with [restsharp](https://stackoverflow.com/questions/tagged/restsharp) tag
would most definitely lead you to a solution.

:::warning
Please do not use GitHub issues to ask question about using RestSharp.
:::

## Discussions

We have a [mail list](http://groups.google.com/group/restsharp) at Google Groups dedicated to discussions about
using RestSharp, feature proposals and similar topics. It is perfectly fine to
ask questions about using RestSharp at that group too.

Please check the group and engage with the community if you feel a need
to discuss things that you struggle with or want to improve.

## Bugs and issues

The last resort to get help when you experience some unexpected behaviour,
a crash or anything else that you consider a bug, is submitting an issue
at our GitHub repository.

:::warning
**Do not ignore our contribution guidelines**, otherwise you risk your issue to be
closed without being considered. Respect the maintainers, be specific and provide
as many details about the issue as you can.
:::

Ensure you provide the following in the issue:
 - Expected behaviour
 - Actual behaviour
 - Why do you think it is an issue, not a misunderstanding
 - How the issue can be reproduced: a repository or at least a code snippet
 - If RestSharp unexpectedly throws an exception, provide the stack trace
 
## Contributing

Although issues are considered as contributions, we strongly suggest helping
the community by solving issues that you experienced by submitting a pull request.

Here are contribution guidelines:

 - Make each pull request atomic and exclusive; don't send pull requests for a laundry list of changes.
 - Even better, commit in small manageable chunks.
 - Use the supplied `.editorconfig` file to format the code.
 - Any change must be accompanied by a unit test covering the change.
 - New tests are preferred to use FluentAssertions.
 - No regions.
 - No licence header for tested.
 - Code must build for .NET Standard 2.0, .NET 5, and .NET 6.
 - Test must run on .NET 6.
 - Use `autocrlf=true` (`git config --global core.autocrlf true`)
 

## Common issues

Before opening an issue on GitHub, please check the list of known issues below.

### Content type

One of the mistakes developers make when using RestSharp is setting the `Content-Type` header manually.
Remember that in most of the usual scenarios setting the content type header manually is not required, and it might be harmful.

RestSharp sets the content type header automatically based on the request type. 
You might want to override the request body content type, but the best way to do it is to supply the content type to the body parameter itself.
Functions for adding the request body to the request have overloads, which accept content type. For example

```csharp
request.AddStringBody(jsonString, ContentType.Json);
```

### Setting the User Agent

Setting the user agent on the request won't work when you use `AddHeader`.

Instead, please use the `RestClientOptions.UserAgent` property.

### Empty response

We regularly get issues where developers complain that their requests get executed
and they get a proper raw response, but the `RestResponse<T>` instance doesn't
have a deserialized object set.

In other situations, the raw response is also empty.

All those issues are caused by the design choice to swallow exceptions
that occur when RestSharp makes the request and processes the response. Instead,
RestSharp produces so-called _error response_.

You can check the response status to find out if there are any errors.
The following properties can tell you about those errors:

- `IsSuccessful`
- `ResponseStatus`
- `StatusCode`
- `ErrorMessage`
- `ErrorException`

It could be that the request was executed and you got `200 OK` status
code back and some content, but the typed `Data` property is empty.

In that case, you probably got deserialization issues. By default, RestSharp will just return an empty (`null`) result in the `Data` property.
Deserialization errors can be also populated to the error response. To do that,
set the `client.FailOnDeserializationError` property to `true`.

It is also possible to force RestSharp to throw an exception.

If you set `client.ThrowOnDeserializationError`, RestSharp will throw a `DeserializationException`
when the serializer throws. The exception has the internal exception and the response.

Finally, by setting `ThrowOnAnyError` you can force RestSharp to re-throw any
exception that happens when making the request and processing the response.
