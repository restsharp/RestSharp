# Namespace: RestSharp.Serializers
## Interface `ISerializer`


### Inherited members

### Syntax
```csharp
public interface ISerializer
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Property `ContentType`

#### Syntax
```csharp
string ContentType { get; set; }
```
#### Property value
Type | Description
--- | ---
`string` | 



### Method `Serialize(Object)`

#### Syntax
```csharp
string Serialize(object obj)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`obj` | `object` | 

#### Returns
Type | Description
--- | ---
`string` | 



## Class `SerializeAsAttribute`

Allows control how class and property names and values are serialized by XmlSerializer
Currently not supported with the JsonSerializer
When specified at the property level the class-level specification is overridden

### Inheritance
↳ `object`<br>&nbsp;&nbsp;↳ `System.Attribute`

### Inherited members

### Syntax
```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = false)]
public sealed class SerializeAsAttribute : Attribute
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Constructor `SerializeAsAttribute()`

#### Syntax
```csharp
public SerializeAsAttribute()
```


### Property `Name`

The name to use for the serialized element

#### Syntax
```csharp
public string Name { get; set; }
```
#### Property value
Type | Description
--- | ---
`string` | 



### Property `Attribute`

Sets the value to be serialized as an Attribute instead of an Element

#### Syntax
```csharp
public bool Attribute { get; set; }
```
#### Property value
Type | Description
--- | ---
`bool` | 



### Property `Content`

Sets the value to be serialized as text content of current Element instead of an new Element

#### Syntax
```csharp
public bool Content { get; set; }
```
#### Property value
Type | Description
--- | ---
`bool` | 



### Property `Culture`

The culture to use when serializing

#### Syntax
```csharp
public CultureInfo Culture { get; set; }
```
#### Property value
Type | Description
--- | ---
`System.Globalization.CultureInfo` | 



### Property `NameStyle`

Transforms the casing of the name based on the selected value.

#### Syntax
```csharp
public NameStyle NameStyle { get; set; }
```
#### Property value
Type | Description
--- | ---
`RestSharp.Serializers.NameStyle` | 



### Property `Index`

The order to serialize the element. Default is int.MaxValue.

#### Syntax
```csharp
public int Index { get; set; }
```
#### Property value
Type | Description
--- | ---
`int` | 



### Method `TransformName(String)`

Called by the attribute when NameStyle is speficied

#### Syntax
```csharp
public string TransformName(string input)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`input` | `string` | The string to transform

#### Returns
Type | Description
--- | ---
`string` | String



## Enum `NameStyle`

Options for transforming casing of element names

### Syntax
```csharp
public enum NameStyle
```

### Fields
Name | Description
--- | ---
AsIs | 
CamelCase | 
LowerCase | 
PascalCase | 
## Class `DotNetXmlSerializer`

Wrapper for System.Xml.Serialization.XmlSerializer.

### Inheritance
↳ `object`

### Inherited members

### Syntax
```csharp
public class DotNetXmlSerializer : IXmlSerializer, ISerializer, IWithRootElement
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Constructor `DotNetXmlSerializer()`

Default constructor, does not specify namespace

#### Syntax
```csharp
public DotNetXmlSerializer()
```


### Constructor `DotNetXmlSerializer(String)`

Specify the namespaced to be used when serializing

#### Syntax
```csharp
public DotNetXmlSerializer(string namespace)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`namespace` | `string` | XML namespace



### Property `Encoding`

Encoding for serialized content

#### Syntax
```csharp
public Encoding Encoding { get; set; }
```
#### Property value
Type | Description
--- | ---
`System.Text.Encoding` | 



### Method `Serialize(Object)`

Serialize the object as XML

#### Syntax
```csharp
public string Serialize(object obj)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`obj` | `object` | Object to serialize

#### Returns
Type | Description
--- | ---
`string` | XML as string



### Property `RootElement`

Name of the root element to use when serializing

#### Syntax
```csharp
public string RootElement { get; set; }
```
#### Property value
Type | Description
--- | ---
`string` | 



### Property `Namespace`

XML namespace to use when serializing

#### Syntax
```csharp
public string Namespace { get; set; }
```
#### Property value
Type | Description
--- | ---
`string` | 



### Property `DateFormat`

Format string to use when serializing dates

#### Syntax
```csharp
public string DateFormat { get; set; }
```
#### Property value
Type | Description
--- | ---
`string` | 



### Property `ContentType`

Content type for serialized content

#### Syntax
```csharp
public string ContentType { get; set; }
```
#### Property value
Type | Description
--- | ---
`string` | 



## Class `XmlSerializer`

Default XML Serializer

### Inheritance
↳ `object`

### Inherited members

### Syntax
```csharp
public class XmlSerializer : IXmlSerializer, ISerializer, IWithRootElement
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Constructor `XmlSerializer()`

Default constructor, does not specify namespace

#### Syntax
```csharp
public XmlSerializer()
```


### Constructor `XmlSerializer(String)`

Specify the namespaced to be used when serializing

#### Syntax
```csharp
public XmlSerializer(string namespace)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`namespace` | `string` | XML namespace



### Method `Serialize(Object)`

Serialize the object as XML

#### Syntax
```csharp
public string Serialize(object obj)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`obj` | `object` | Object to serialize

#### Returns
Type | Description
--- | ---
`string` | XML as string



### Property `RootElement`

Name of the root element to use when serializing

#### Syntax
```csharp
public string RootElement { get; set; }
```
#### Property value
Type | Description
--- | ---
`string` | 



### Property `Namespace`

XML namespace to use when serializing

#### Syntax
```csharp
public string Namespace { get; set; }
```
#### Property value
Type | Description
--- | ---
`string` | 



### Property `DateFormat`

Format string to use when serializing dates

#### Syntax
```csharp
public string DateFormat { get; set; }
```
#### Property value
Type | Description
--- | ---
`string` | 



### Property `ContentType`

Content type for serialized content

#### Syntax
```csharp
public string ContentType { get; set; }
```
#### Property value
Type | Description
--- | ---
`string` | 


