# Namespace: RestSharp.Serialization
## Class `ContentType`

### Inheritance
↳ `object`

### Inherited members

### Syntax
```csharp
public static class ContentType
```

Json | 
Xml | 
FromDataFormat | 
JsonAccept | 
XmlAccept | 
## Interface `IRestSerializer`


### Inherited members

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
#### Property value
Type | Description
--- | ---
`string[]` | 



### Property `DataFormat`

#### Syntax
```csharp
DataFormat DataFormat { get; }
```
#### Property value
Type | Description
--- | ---
`RestSharp.DataFormat` | 



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


### Inherited members

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
#### Property value
Type | Description
--- | ---
`string` | 


