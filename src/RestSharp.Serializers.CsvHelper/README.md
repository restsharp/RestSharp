# About

The `RestSharp.Serializers.CsvHelper` library provides a CSV serializer for RestSharp. It is based on the
`CsvHelper` library.

# How to use

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
    configureSerialization: s => s.UseCsvHelper(new CsvConfiguration(CultureInfo.InvariantCulture) {...})
);
```
