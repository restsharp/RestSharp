# Namespace: RestSharp.Serializers.Utf8Json
## Class RestClientExtensions

### Inheritance
↳ object

### Inherited members
object.GetType()
object.MemberwiseClone()
object.ToString()
object.Equals(object?)
object.Equals(object?, object?)
object.ReferenceEquals(object?, object?)
object.GetHashCode()
## Syntax
```csharp
public static class RestClientExtensions
```

### Method UseUtf8Json(IRestClient)

Use Utf8Json serializer with default formatter resolver

### Syntax
```csharp
public static IRestClient UseUtf8Json(this IRestClient client)
```
#### Parameters
Name | Type | Description
--- | --- | ---
client | IRestClient | 

#### Returns
Type | Description
--- | ---
IRestClient | 



### Method UseUtf8Json(IRestClient, IJsonFormatterResolver)

Use Utf8Json serializer with custom formatter resolver

### Syntax
```csharp
public static IRestClient UseUtf8Json(this IRestClient client, IJsonFormatterResolver resolver)
```
#### Parameters
Name | Type | Description
--- | --- | ---
client | IRestClient | 
resolver | IJsonFormatterResolver | Utf8Json deserialization formatter resolver

#### Returns
Type | Description
--- | ---
IRestClient | 



## Class RestRequestExtensions

### Inheritance
↳ object

### Inherited members
object.GetType()
object.MemberwiseClone()
object.ToString()
object.Equals(object?)
object.Equals(object?, object?)
object.ReferenceEquals(object?, object?)
object.GetHashCode()
## Syntax
```csharp
public static class RestRequestExtensions
```

### Method UseUtf8Json(IRestRequest)

### Syntax
```csharp
public static IRestRequest UseUtf8Json(this IRestRequest request)
```
#### Parameters
Name | Type | Description
--- | --- | ---
request | IRestRequest | 

#### Returns
Type | Description
--- | ---
IRestRequest | 



### Method UseNewtonsoftJson(IRestRequest, IJsonFormatterResolver)

### Syntax
```csharp
public static IRestRequest UseNewtonsoftJson(this IRestRequest request, IJsonFormatterResolver resolver)
```
#### Parameters
Name | Type | Description
--- | --- | ---
request | IRestRequest | 
resolver | IJsonFormatterResolver | 

#### Returns
Type | Description
--- | ---
IRestRequest | 



## Class Utf8JsonSerializer

### Inheritance
↳ IRestSerializer
 ↳ System.Object

### Inherited members

## Syntax
```csharp
public class Utf8JsonSerializer : IRestSerializer
```

### Extension methods
object.ChangeType(System.Reflection.TypeInfo)
object.ChangeType(System.Type, System.Globalization.CultureInfo)
### Constructor Utf8JsonSerializer(IJsonFormatterResolver)

### Syntax
```csharp
public Utf8JsonSerializer(IJsonFormatterResolver resolver = null)
```
#### Parameters
Name | Type | Description
--- | --- | ---
resolver | IJsonFormatterResolver | 



### Method Serialize(Object)

### Syntax
```csharp
public string Serialize(object obj)
```
#### Parameters
Name | Type | Description
--- | --- | ---
obj | object | 

#### Returns
Type | Description
--- | ---
string | 



### Method Serialize(Parameter)

### Syntax
```csharp
public string Serialize(Parameter parameter)
```
#### Parameters
Name | Type | Description
--- | --- | ---
parameter | Parameter | 

#### Returns
Type | Description
--- | ---
string | 



### Method Deserialize&lt;T&rt;(IRestResponse)

### Syntax
```csharp
public T Deserialize&lt;T&rt;(IRestResponse response)
```
#### Generic parameters
Name | Description
--- | ---
T | 

#### Parameters
Name | Type | Description
--- | --- | ---
response | IRestResponse | 

#### Returns
Type | Description
--- | ---
T | 



### Property SupportedContentTypes

### Syntax
```csharp
public string[] SupportedContentTypes { get; }
```
#### Property value
Type | Description
--- | ---
string[] | 



### Property ContentType

### Syntax
```csharp
public string ContentType { get; set; }
```
#### Property value
Type | Description
--- | ---
string | 



### Property DataFormat

### Syntax
```csharp
public DataFormat DataFormat { get; }
```
#### Property value
Type | Description
--- | ---
DataFormat | 


