# Namespace: RestSharp.Serialization
## Class `ContentType`

### Inheritance
↳ `object`
### Syntax
```csharp
public static class ContentType
```

### Field `Json`

#### Syntax
```csharp
public static string Json = "application/json"
```


### Field `Xml`

#### Syntax
```csharp
public static string Xml = "application/xml"
```


### Field `FromDataFormat`

#### Syntax
```csharp
public static Dictionary<DataFormat, string> FromDataFormat
```


### Field `JsonAccept`

#### Syntax
```csharp
public static string[] JsonAccept
```


### Field `XmlAccept`

#### Syntax
```csharp
public static string[] XmlAccept
```


## Interface `IRestSerializer`

### Syntax
```csharp
public interface IRestSerializer : ISerializer, IDeserializer
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Property `SupportedContentTypes`

#### Syntax
```csharp
string[] SupportedContentTypes { get; }
```


### Property `DataFormat`

#### Syntax
```csharp
DataFormat DataFormat { get; }
```


### Method `Serialize(Parameter)`

#### Syntax
```csharp
string Serialize(Parameter parameter)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`parameter` | `RestSharp.Parameter` | 

#### Returns
Type | Description
--- | ---
`string` | 



## Interface `IWithRootElement`

### Syntax
```csharp
public interface IWithRootElement
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Property `RootElement`

#### Syntax
```csharp
string RootElement { get; set; }
```

