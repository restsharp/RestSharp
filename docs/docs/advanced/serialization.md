# Serialization

One of the most common reasons to choose RestSharp over plain `HttpClient` is its rich build-in serialization support. RestSharp allows adding complex objects as request body to be serialized when making a call to an API endpoint, and deserializing the response to a given .NET type. RestSharp supports JSON and XML serialization and deserialization by default. In addition, you can use a CSV serializer or write your own.

In contrast to `System.Net.Http.Json` package that contains `HttpClient` extensions to make `GET` or `POST` calls using JSON, RestSharp support JSON responses for all HTTP methods, not just for `GET`.

## Configuration

:::tip
The default behavior of RestSharp is to swallow deserialization errors and return `null` in the `Data`
property of the response. Read more about it in the [Error Handling](error-handling.md).
:::

You can tell RestSharp to use a custom serializer by using the `configureSerialization` constructor parameter:

```csharp
var client = new RestClient(
    options, 
    configureSerialization: s => s.UseSerializer(() => new CustomSerializer());
);
```

All RestSharp serializers implement the `IRestSerializer` interface. Among other things, the interface requires implementing the `AcceptedContentTypes` property, which must return a collection of content types supported by the serializer. Being configured to use certain serializers, RestSharp populates the `Accept` header accordingly, so it doesn't need to be set manually.

When making a call, RestSharp sets the request content type according to the request body type. For example, when you use `AddJsonBody`, the content type is set to `application/json`. Normally, you won't need to set the `Content-Type` header manually. If you need to set a custom content type for a JSON call, you can use the optional `contentType` argument of `AddJsonBody`, for example:

```csharp
request.AddJsonBody(data, "text/json");
```

## JSON

The default JSON serializer uses `System.Text.Json`, which is a part of .NET since .NET 6. For earlier versions, it is added as a dependency. There are also a few serializers provided as additional packages.

By default, RestSharp will use `JsonSerializerDefaults.Web` configuration. If necessary, you can specify your own options:

```csharp
var client = new RestClient(
    options, 
    configureSerialization: s => s.UseSystemTextJson(new JsonSerializerOptions {...})
);
```

## XML

The default XML serializer is `DotNetXmlSerializer`, which uses `System.Xml.Serialization` library from .NET.

In previous versions of RestSharp, the default XML serializer was a custom RestSharp XML serializer. To make the code library size smaller, that serializer is now available as a separate package [`RestSharp.Serializers.Xml`](https://www.nuget.org/packages/RestSharp.Serializers.Xml).
You can add it back if necessary by installing the package and adding it to the client:

```csharp
var client = new RestClient(
    options, 
    configureSerialization: s => s.UseXmlSerializer()
);
```

As before, you can supply three optional arguments for a custom namespace, custom root element, and if you want to use `SerializeAs` and `DeserializeAs` attributed.

## NewtonsoftJson (aka Json.Net)

The `NewtonsoftJson` package is the most popular JSON serializer for .NET. It handles all possible scenarios and is very configurable. Such a flexibility comes with the cost of performance. If you need speed, keep the default JSON serializer.

RestSharp support Json.Net serializer via a separate package [`RestSharp.Serializers.NewtonsoftJson`](https://www.nuget.org/packages/RestSharp.Serializers.NewtonsoftJson).

:::warning
Please note that `RestSharp.Newtonsoft.Json` package is not provided by RestSharp, is marked as obsolete on NuGet, and no longer supported by its creator.
:::

Use the extension method provided by the package to configure the client:

```csharp
var client = new RestClient(
    options, 
    configureSerialization: s => s.UseNewtonsoftJson()
);
```

The serializer configures some options by default:

```csharp
JsonSerializerSettings DefaultSettings = new JsonSerializerSettings {
    ContractResolver     = new CamelCasePropertyNamesContractResolver(),
    DefaultValueHandling = DefaultValueHandling.Include,
    TypeNameHandling     = TypeNameHandling.None,
    NullValueHandling    = NullValueHandling.Ignore,
    Formatting           = Formatting.None,
    ConstructorHandling  = ConstructorHandling.AllowNonPublicDefaultConstructor
};
```

If you need to use different settings, you can supply your instance of
`JsonSerializerSettings` as a parameter for the extension method.

## CSV

A separate package `RestSharp.Serializers.CsvHelper` provides a CSV serializer for RestSharp. It is based on the 
`CsvHelper` library.

Use the extension method provided by the package to configure the client:

```csharp
var client = new RestClient(
    options, 
    configureSerialization: s => s.UseCsvHelper()
);
```

You can also supply your instance of `CsvConfiguration` as a parameter for the extension method.

```csharp
var client = new RestClient(
    options, 
    configureSerialization: s => s.UseCsvHelper(
        new CsvConfiguration(CultureInfo.InvariantCulture) {...}
    )
);
```

## Custom

You can also implement your custom serializer. To support both serialization and
deserialization, you must implement the `IRestSerializer` interface.

Here is an example of a custom serializer that uses `System.Text.Json`:

```csharp
public class SimpleJsonSerializer : IRestSerializer {
    public string Serialize(object obj) => JsonSerializer.Serialize(obj);

    public string Serialize(Parameter bodyParameter) => Serialize(bodyParameter.Value);

    public T Deserialize<T>(IRestResponse response) => JsonSerializer.Deserialize<T>(response.Content);

    public string[] SupportedContentTypes { get; } = {
        "application/json", "text/json", "text/x-json", "text/javascript", "*+json"
    };

    public string ContentType { get; set; } = "application/json";

    public DataFormat DataFormat { get; } = DataFormat.Json;
}
```

The value of the `SupportedContentTypes` property will be used to match the
serializer with the response `Content-Type` headers.

The `ContentType` property will be used when making a request so the
server knows how to handle the payload.
