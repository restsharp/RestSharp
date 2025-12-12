# About

This library allows using Newtonsoft.Json as a serializer for RestSharp instead of the default JSON serializer based 
on `System.Text.Json`.

# How to use

The default JSON serializer uses `System.Text.Json`, which is a part of .NET since .NET 6.

If you want to use Newtonsoft.Json, you can install the `RestSharp.Serializers.NewtonsoftJson` package and configure 
the 
client to use it:

```csharp
var client = new RestClient(
    options, 
    configureSerialization: s => s.UseNewtonsoftJson()
);
```