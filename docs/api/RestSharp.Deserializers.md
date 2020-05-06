# Namespace: RestSharp.Deserializers
## Class `DeserializeAsAttribute`

Allows control how class and property names and values are deserialized by XmlAttributeDeserializer

### Inheritance
↳ `object`<br>&nbsp;&nbsp;↳ `System.Attribute`
### Syntax
```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = false)]
public sealed class DeserializeAsAttribute : Attribute
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Property `Name`

The name to use for the serialized element

#### Syntax
```csharp
public string Name { get; set; }
```


### Property `Attribute`

Sets if the property to Deserialize is an Attribute or Element (Default: false)

#### Syntax
```csharp
public bool Attribute { get; set; }
```


### Property `Content`

Sets if the property to Deserialize is a content of current Element (Default: false)

#### Syntax
```csharp
public bool Content { get; set; }
```


## Interface `IDeserializer`

### Syntax
```csharp
public interface IDeserializer
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Method `Deserialize<T>(IRestResponse)`

#### Syntax
```csharp
T Deserialize<T>(IRestResponse response)
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



## Class `DotNetXmlDeserializer`

Wrapper for System.Xml.Serialization.XmlSerializer.

### Inheritance
↳ `object`
### Syntax
```csharp
public class DotNetXmlDeserializer : IXmlDeserializer, IDeserializer, IWithRootElement
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Property `Encoding`

Encoding for serialized content

#### Syntax
```csharp
public Encoding Encoding { get; set; }
```


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

#### Syntax
```csharp
public string DateFormat { get; set; }
```


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



## Class `XmlDeserializer`

### Inheritance
↳ `object`
### Syntax
```csharp
public class XmlDeserializer : IXmlDeserializer, IDeserializer, IWithRootElement
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Constructor `XmlDeserializer()`

#### Syntax
```csharp
public XmlDeserializer()
```


### Property `Culture`

#### Syntax
```csharp
public CultureInfo Culture { get; set; }
```


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


### Method `Deserialize<T>(IRestResponse)`

#### Syntax
```csharp
public virtual T Deserialize<T>(IRestResponse response)
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



### Method `Map(Object, XElement)`

#### Syntax
```csharp
protected virtual object Map(object x, XElement root)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`x` | `object` | 
`root` | `XElement` | 

#### Returns
Type | Description
--- | ---
`object` | 



### Method `CreateAndMap(Type, XElement)`

#### Syntax
```csharp
protected virtual object CreateAndMap(Type t, XElement element)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`t` | `System.Type` | 
`element` | `XElement` | 

#### Returns
Type | Description
--- | ---
`object` | 



### Method `GetValueFromXml(XElement, XName, PropertyInfo, Boolean)`

#### Syntax
```csharp
protected virtual object GetValueFromXml(XElement root, XName name, PropertyInfo prop, bool useExactName)
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



### Method `GetElementByName(XElement, XName)`

#### Syntax
```csharp
protected virtual XElement GetElementByName(XElement root, XName name)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`root` | `XElement` | 
`name` | `XName` | 

#### Returns
Type | Description
--- | ---
`XElement` | 



### Method `GetAttributeByName(XElement, XName, Boolean)`

#### Syntax
```csharp
protected virtual XAttribute GetAttributeByName(XElement root, XName name, bool useExactName)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`root` | `XElement` | 
`name` | `XName` | 
`useExactName` | `bool` | 

#### Returns
Type | Description
--- | ---
`XAttribute` | 


