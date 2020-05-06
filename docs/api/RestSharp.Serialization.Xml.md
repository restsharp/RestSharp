# Namespace: RestSharp.Serialization.Xml
## Class `DotNetXmlSerializerClientExtensions`

### Inheritance
↳ `object`
### Syntax
```csharp
public static class DotNetXmlSerializerClientExtensions
```

### Method `UseDotNetXmlSerializer(IRestClient, String, Encoding)`

#### Syntax
```csharp
public static IRestClient UseDotNetXmlSerializer(this IRestClient restClient, string xmlNamespace = null, Encoding encoding = null)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`restClient` | `RestSharp.IRestClient` | 
`xmlNamespace` | `string` | 
`encoding` | `System.Text.Encoding` | 

#### Returns
Type | Description
--- | ---
`RestSharp.IRestClient` | 



## Interface `IXmlDeserializer`

### Syntax
```csharp
public interface IXmlDeserializer : IDeserializer, IWithRootElement
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Property `Namespace`

#### Syntax
```csharp
string Namespace { get; set; }
```


### Property `DateFormat`

#### Syntax
```csharp
string DateFormat { get; set; }
```


## Interface `IXmlSerializer`

### Syntax
```csharp
public interface IXmlSerializer : ISerializer, IWithRootElement
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Property `Namespace`

#### Syntax
```csharp
string Namespace { get; set; }
```


### Property `DateFormat`

#### Syntax
```csharp
string DateFormat { get; set; }
```


## Class `XmlAttributeDeserializer`

### Inheritance
↳ `object`<br>&nbsp;&nbsp;↳ `RestSharp.Deserializers.XmlDeserializer`
### Syntax
```csharp
public class XmlAttributeDeserializer : XmlDeserializer, IXmlDeserializer, IDeserializer, IWithRootElement
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Method `GetValueFromXml(XElement, XName, PropertyInfo, Boolean)`

#### Syntax
```csharp
protected override object GetValueFromXml(XElement root, XName name, PropertyInfo prop, bool useExactName)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`root` | `XElement` | 
`name` | `XName` | 
`prop` | `System.Reflection.PropertyInfo` | 
`useExactName` | `bool` | 

#### Returns
Type | Description
--- | ---
`object` | 



## Class `XmlRestSerializer`

### Inheritance
↳ `object`
### Syntax
```csharp
public class XmlRestSerializer : IRestSerializer, IXmlSerializer, ISerializer, IXmlDeserializer, IDeserializer, IWithRootElement
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Property `SupportedContentTypes`

#### Syntax
```csharp
public string[] SupportedContentTypes { get; }
```


### Property `DataFormat`

#### Syntax
```csharp
public DataFormat DataFormat { get; }
```


### Property `ContentType`

#### Syntax
```csharp
public string ContentType { get; set; }
```


### Method `Serialize(Object)`

#### Syntax
```csharp
public string Serialize(object obj)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`obj` | `object` | 

#### Returns
Type | Description
--- | ---
`string` | 



### Method `Deserialize<T>(IRestResponse)`

#### Syntax
```csharp
public T Deserialize<T>(IRestResponse response)
```
#### Generic parameters
Name | Description
--- | ---
`T` | 

#### Parameters
Name | Type | Description
--- | --- | ---
`response` | `RestSharp.IRestResponse` | 

#### Returns
Type | Description
--- | ---
`T` | 



### Method `Serialize(Parameter)`

#### Syntax
```csharp
public string Serialize(Parameter parameter)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`parameter` | `RestSharp.Parameter` | 

#### Returns
Type | Description
--- | ---
`string` | 



### Property `RootElement`

#### Syntax
```csharp
public string RootElement { get; set; }
```


### Property `Namespace`

#### Syntax
```csharp
public string Namespace { get; set; }
```


### Property `DateFormat`

#### Syntax
```csharp
public string DateFormat { get; set; }
```


### Method `WithOptions(XmlSerilizationOptions)`

#### Syntax
```csharp
public XmlRestSerializer WithOptions(XmlSerilizationOptions options)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`options` | `RestSharp.Serialization.Xml.XmlSerilizationOptions` | 

#### Returns
Type | Description
--- | ---
`RestSharp.Serialization.Xml.XmlRestSerializer` | 



### Method `WithXmlSerializer<T>(XmlSerilizationOptions)`

#### Syntax
```csharp
public XmlRestSerializer WithXmlSerializer<T>(XmlSerilizationOptions options = null)
    where T : IXmlSerializer, new()
```
#### Generic parameters
Name | Description
--- | ---
`T` | 

#### Parameters
Name | Type | Description
--- | --- | ---
`options` | `RestSharp.Serialization.Xml.XmlSerilizationOptions` | 

#### Returns
Type | Description
--- | ---
`RestSharp.Serialization.Xml.XmlRestSerializer` | 



### Method `WithXmlSerializer(IXmlSerializer)`

#### Syntax
```csharp
public XmlRestSerializer WithXmlSerializer(IXmlSerializer xmlSerializer)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`xmlSerializer` | `RestSharp.Serialization.Xml.IXmlSerializer` | 

#### Returns
Type | Description
--- | ---
`RestSharp.Serialization.Xml.XmlRestSerializer` | 



### Method `WithXmlDeserialzier<T>(XmlSerilizationOptions)`

#### Syntax
```csharp
public XmlRestSerializer WithXmlDeserialzier<T>(XmlSerilizationOptions options = null)
    where T : IXmlDeserializer, new()
```
#### Generic parameters
Name | Description
--- | ---
`T` | 

#### Parameters
Name | Type | Description
--- | --- | ---
`options` | `RestSharp.Serialization.Xml.XmlSerilizationOptions` | 

#### Returns
Type | Description
--- | ---
`RestSharp.Serialization.Xml.XmlRestSerializer` | 



### Method `WithXmlDeserializer(IXmlDeserializer)`

#### Syntax
```csharp
public XmlRestSerializer WithXmlDeserializer(IXmlDeserializer xmlDeserializer)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`xmlDeserializer` | `RestSharp.Serialization.Xml.IXmlDeserializer` | 

#### Returns
Type | Description
--- | ---
`RestSharp.Serialization.Xml.XmlRestSerializer` | 



## Class `XmlSerilizationOptions`

### Inheritance
↳ `object`
### Syntax
```csharp
public class XmlSerilizationOptions
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Property `RootElement`

Name of the root element to use when serializing

#### Syntax
```csharp
public string RootElement { get; set; }
```


### Property `Namespace`

XML namespace to use when serializing

#### Syntax
```csharp
public string Namespace { get; set; }
```


### Property `DateFormat`

Format string to use when serializing dates

#### Syntax
```csharp
public string DateFormat { get; set; }
```


### Property `Culture`

#### Syntax
```csharp
public CultureInfo Culture { get; set; }
```


### Property `Default`

#### Syntax
```csharp
public static XmlSerilizationOptions Default { get; }
```

