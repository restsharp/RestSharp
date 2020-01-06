# Getting Started

Add the NuGet package to your project:

```
dotnet add package RestSharp
```

Create the `IRestClient` instance in advance:

```csharp
var client = new RestClient("http://example.com");
```

Find a place in your code where you want to make a REST call, create a request and get the client to execute it:

```csharp
var request = new RestRequest("resource/{id}")
    .AddParameter("name", "value") // adds to POST or URL querystring based on Method
    .AddUrlSegment("id", "123");   // replaces matching token in request.Resource
```

Execute the request:

```csharp
var response = client.Post(request);
```
