---
sidebar_position: 7
title: Dependency Injection
---

RestSharp integrates with the Microsoft [Dependency Injection](ttps://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) container and `IHttpClientFactory` via a separate package `RestSharp.Extensions.DependencyInjection`.

You can register RestClient in the container as a transient dependency, and it will use `IHttpClientFactory` via `IRestClientFactory` for creating HTTP clients and message handlers that are managed by the factory. This approach offers the following benefits:

* Provides a central location for naming and configuring logical RestClient instances. For example, a client named `github` could be registered and configured to access GitHub. A default client can be registered for general access.
* Manages the pooling and lifetime of underlying `HttpClientMessageHandler` instances. Automatic management avoids common DNS problems that occur when manually managing RestClient lifetime.

Examples below are for a basic ASP.NET Core bootstrap with `WebApplicationBuilder`.

## Default client

Register `IHttpClientFactory`, `IRestClientFactory`, and `IRestClient` by calling `AddRestClient` in `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRestClient();
```

Then, you can get `IRestClient` injected to your services:

```csharp
public class GetReposModel(IRestClient client) : PageModel {
    public IEnumerable<GitHubBranch>? GitHubBranches { get; set; }

    public async Task OnGet() {
        var request = new RestRequest("https://api.github.com/repos/RestSharp/RestSharp/branches");
        var response = await client.ExecuteGetAsync<IEnumerable<GitHubBranch>>(request);

        if (response.IsSuccessful) {
            GitHubBranches = response.Data;
        }
    }
}
```

It's possible to provide client options when registering the default client using one of the extensions:

```csharp
// Specify the base URL
builder.Services.AddRestClient(new Uri("https://api.github.com"));

// Provide custom options
var options = new RestClientOptions("https://api.github.com") {
    Timeout = TimeSpan.FromSeconds(5);
}
builder.Services.AddRestClient(options);
```

## Named client

Named clients are needed when you need multiple clients to be configured differently, which are then used in different parts of the application. For example, you need one client to call the GitHub API, and another client to call the Twilio API. Registering clients using meaningful names allows to retrieve them from `IRestClientFactory`.

For example, a GitHub-specific client can be registered with `github` name:

```csharp
var options = new RestClientOptions("https://api.github.com") {
    Timeout = TimeSpan.FromSeconds(5);
}
builder.Services.AddRestClient("github", options);
```

Then, an application component can use the client factory to get the client instance:

```csharp
public class GetReposModel(IRestClientFactory factory) : PageModel {
    public IEnumerable<GitHubBranch>? GitHubBranches { get; set; }

    public async Task OnGet() {
        var request = new RestRequest("/repos/RestSharp/RestSharp/branches");
        var client = factory.CreateClient("github");
        var response = await client.ExecuteGetAsync<IEnumerable<GitHubBranch>>(request);

        if (response.IsSuccessful) {
            GitHubBranches = response.Data;
        }
    }
}
```

As the default client, named clients can be registered with a base URL and with custom options. The most versatile extension allows configuring anything at named client registration:

```csharp
builder.Services.AddRestClient(
    "my-client",
    options => {
        options.BaseUrl = new Uri("https://api.github.com");
        options.Timeout = TimeSpan.FromSeconds(1);
        options.Authenticator = new GitHubAuthenticator(builder.Configuration["GitHub:ApiToken"]);
    },
    serialization => serialization.UseNewtonsoftJson()
);
```