# About

The `RestSharp.Extensions.DependencyInjection` library provides integration with `Microsoft.Extensions.DependencyInjection` components as well as integrates with `IHttpClientFactory`.

# How to use

Use the extension method provided by the package to configure the client:

```csharp
// Add a default client with no base URL, with default options
services.AddRestClient(); 

// Add a client with a base URL
services.AddRestClient(new Uri("https://example.com")); 

// Add a client with a base URL and custom options
services.AddRestClient(options => 
{
    options.BaseUrl = new Uri("https://example.com");
    options.Timeout = TimeSpan.FromSeconds(30);
}); 
```

When the above registrations are used, the `IRestClient` interface can be injected into any class.

In addition, the package supports registering named clients:

```csharp
services.AddRestClient("my-client", options => 
{
    options.BaseUrl = new Uri("https://example.com");
    options.Timeout = TimeSpan.FromSeconds(30);
}); 
```

When the above registrations are used, resolving the client instance should be done using the `IRestClientFactory`:

```csharp
public class MyClass(IRestClientFactory restClientFactory)
{
    IRestClient client = restClientFactory.CreateClient("my-client");

    // Use the client in your code
}
```