# About

This package is a custom XML serializer for RestSharp. It is based on the original XML serializer that was part of RestSharp but was removed in version 107.0.0.

# How to use

The default XML serializer in RestSharp is `DotNetXmlSerializer`, which uses `System.Xml.Serialization` library from .
NET.

In previous versions of RestSharp, the default XML serializer was a custom RestSharp XML serializer. To make the 
code library size smaller, the custom serializer was removed from RestSharp.

You can add it back if necessary by installing the `RestSharp.Serializers.Xml` package and adding it to the client:

```csharp
var client = new RestClient(
    options, 
    configureSerialization: s => s.UseXmlSerializer()
);
```

As before, you can supply three optional arguments for a custom namespace, custom root element, and if you want to use `SerializeAs` and `DeserializeAs` attributed.
