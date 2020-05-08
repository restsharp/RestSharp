# Serialization

RestSharp has JSON and XML serializers built in without any additional packages
or configuration. There are also a few JSON serializers provided as additional packages.

:::tip
The default behavior of RestSharp is to swallow deserialization errors and return `null` in the `Data`
property of the response. Read more about it in the [Error Handling](exceptions.md).
:::

## Default Serializers

RestSharp core package includes a few default serializers for both JSON and XML formats.

### JSON

The default JSON serializer uses the forked version of `SimpleJson`. It is very simplistic and
doesn't handle advanced scenarios in many cases. We do not plan to fix or add new features
to the default JSON serializer, since it handles a lot of cases already and when you need
to handle more complex objects, please consider using alternative JSON serializers mentioned below.

There's a [known issue](https://github.com/restsharp/RestSharp/issues/1433) that SimpleJson doesn't use the UTC time zone when the regular .NET date format
is used (`yyyy-MM-ddTHH:mm:ssZ`). As suggested in the issue, it can be solved by setting the
date format explicitly for SimpleJson:

```csharp
client.UseSerializer(
    () => new JsonSerializer { DateFormat = "yyyy-MM-ddTHH:mm:ss.FFFFFFFZ" }
);
```

### XML

You can use either the default XML serializer or the `DotNetXmlSerializer`, which uses `System.Xml.Serialization` library
from .NET. To use the `DotNetXmlSerializer` you need to configure the REST client instance:

```csharp
client.UseDotNetXmlSerializer();
```

## NewtonsoftJson (aka Json.Net)

The `NewtonsoftJson` package is the most popular JSON serializer for .NET.
It handles all possible scenarios and is very configurable. Such a flexibility
comes with the cost of performance. If you need something faster, please check
`Utf8Json` or `System.Text.Json` serializers (below).

RestSharp support Json.Net serializer via a separate package. You can install it
from NuGet:

```
dotnet add package RestSharp.Serializers.NewtonsoftJson
```

Use the extension method provided by the package to configure the client:

```csharp
client.UseNewtonsoftJson();
```

The serializer configures some options by default:

```csharp
JsonSerializerSettings DefaultSettings = new JsonSerializerSettings
{
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

## Utf8Json

The 'Utf8Json' package is known to be the fastest JSON serializer for .NET.

RestSharp supports `Utf8Json` serializer via a separate package. You can install it
from NuGet:

```
dotnet add package RestSharp.Serializers.Utf8Json
```

Configure your REST client using the extension method:

```csharp
client.UseUtf8Json();
``` 

When the extension method is called without parameters, it will configure
the default options:

 - Allow private properties
 - Exclude null values
 - Use camel-case

If you need to use different options, you can provide the instance of
`IJsonFormatterResolver` as a parameter for the extension method.

:::warning
Keep in mind that this serializer is case-sensitive by default.
:::

## System.Text.Json

Microsoft included the new JSON serializer package `System.Text.Json` together with .NET Core 3.
It is a small and fast serializer that is used in the WebApi version for .NET Core 3
and beyond by default. The package is also available for .NET Standard 2.0 and .NET Framework 4.6.1 and higher.

RestSharp supports `System.Text.Json` serializer via a separate package. You can install it
from NuGet:

```
dotnet add package RestSharp.Serializers.SystemTextJson
```

Configure your REST client using the extension method:

```csharp
client.UseSystemTextJson();
``` 

The serializer will use default options, unless you provide your
own instance of `JsonSerializerOptions` to the extension method.

:::warning
Keep in mind that this serializer is case-sensitive by default.
:::

## Custom

You can also implement your custom serializer. To support both serialization and
deserialization, you must implement the `IRestSerializer` interface.

Here is an example of a custom serializer that uses `System.Text.Json`:

```csharp
public class SimpleJsonSerializer : IRestSerializer
{
    public string Serialize(object obj) => JsonSerializer.Serialize(obj);

    public string Serialize(Parameter bodyParameter) => Serialize(bodyParameter.Value);

    public T Deserialize<T>(IRestResponse response) => JsonSerializer.Deserialize<T>(response.Content);

    public string[] SupportedContentTypes { get; } =
    {
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
