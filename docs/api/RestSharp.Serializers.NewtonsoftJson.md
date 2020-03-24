# Namespace: RestSharp.Serializers.NewtonsoftJson
## Class JsonNetSerializer

### Inheritance
↳ IRestSerializer
 ↳ System.Object

### Inherited members

## Syntax
```csharp
public class JsonNetSerializer : IRestSerializer
```

### Extension methods
object.ChangeType(System.Reflection.TypeInfo)
object.ChangeType(System.Type, System.Globalization.CultureInfo)
DefaultSettings | 
### Constructor JsonNetSerializer()

Create the new serializer that uses Json.Net with default settings

### Syntax
```csharp
public JsonNetSerializer()
```


### Constructor JsonNetSerializer(JsonSerializerSettings)

Create the new serializer that uses Json.Net with custom settings

### Syntax
```csharp
public JsonNetSerializer(JsonSerializerSettings settings)
```
#### Parameters
Name | Type | Description
--- | --- | ---
settings | JsonSerializerSettings | Json.Net serializer settings



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
public string Serialize(Parameter bodyParameter)
```
#### Parameters
Name | Type | Description
--- | --- | ---
bodyParameter | Parameter | 

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

### Method UseNewtonsoftJson(IRestClient)

Use Json.Net serializer with default settings

### Syntax
```csharp
public static IRestClient UseNewtonsoftJson(this IRestClient client)
```
#### Parameters
Name | Type | Description
--- | --- | ---
client | IRestClient | 

#### Returns
Type | Description
--- | ---
IRestClient | 



### Method UseNewtonsoftJson(IRestClient, JsonSerializerSettings)

Use Json.Net serializer with custom settings

### Syntax
```csharp
public static IRestClient UseNewtonsoftJson(this IRestClient client, JsonSerializerSettings settings)
```
#### Parameters
Name | Type | Description
--- | --- | ---
client | IRestClient | 
settings | JsonSerializerSettings | Json.Net serializer settings

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

### Method UseNewtonsoftJson(IRestRequest)

### Syntax
```csharp
public static IRestRequest UseNewtonsoftJson(this IRestRequest request)
```
#### Parameters
Name | Type | Description
--- | --- | ---
request | IRestRequest | 

#### Returns
Type | Description
--- | ---
IRestRequest | 



### Method UseNewtonsoftJson(IRestRequest, JsonSerializerSettings)

### Syntax
```csharp
public static IRestRequest UseNewtonsoftJson(this IRestRequest request, JsonSerializerSettings settings)
```
#### Parameters
Name | Type | Description
--- | --- | ---
request | IRestRequest | 
settings | JsonSerializerSettings | 

#### Returns
Type | Description
--- | ---
IRestRequest | 


