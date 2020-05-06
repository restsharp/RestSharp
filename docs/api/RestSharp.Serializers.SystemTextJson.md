---
title: RestSharp.Serializers.SystemTextJson
---

# Assembly: RestSharp.Serializers.SystemTextJson
## Namespace: RestSharp.Serializers.SystemTextJson
### Class `RestClientExtensions`

#### Inheritance
↳ `object`
#### Syntax
```csharp
public static class RestClientExtensions
```

#### Method `UseSystemTextJson(IRestClient)`

Use System.Text.Json serializer with default settings

##### Syntax
```csharp
public static IRestClient UseSystemTextJson(this IRestClient client)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`client` | `IRestClient` | 

##### Returns
Type | Description
--- | ---
`IRestClient` | 



#### Method `UseSystemTextJson(IRestClient, JsonSerializerOptions)`

Use System.Text.Json serializer with custom settings

##### Syntax
```csharp
public static IRestClient UseSystemTextJson(this IRestClient client, JsonSerializerOptions options)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`client` | `IRestClient` | 
`options` | `JsonSerializerOptions` | System.Text.Json serializer options

##### Returns
Type | Description
--- | ---
`IRestClient` | 



### Class `RestRequestExtensions`

#### Inheritance
↳ `object`
#### Syntax
```csharp
public static class RestRequestExtensions
```

#### Method `UseSystemTextJson(IRestRequest)`

##### Syntax
```csharp
public static IRestRequest UseSystemTextJson(this IRestRequest request)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`request` | `IRestRequest` | 

##### Returns
Type | Description
--- | ---
`IRestRequest` | 



#### Method `UseSystemTextJson(IRestRequest, JsonSerializerOptions)`

##### Syntax
```csharp
public static IRestRequest UseSystemTextJson(this IRestRequest request, JsonSerializerOptions options)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`request` | `IRestRequest` | 
`options` | `JsonSerializerOptions` | 

##### Returns
Type | Description
--- | ---
`IRestRequest` | 



### Class `SystemTextJsonSerializer`

#### Inheritance
↳ `IRestSerializer`<br>&nbsp;&nbsp;↳ `System.Object`
#### Syntax
```csharp
public class SystemTextJsonSerializer : IRestSerializer
```

#### Constructor `SystemTextJsonSerializer()`

Create the new serializer that uses System.Text.Json.JsonSerializer with default settings

##### Syntax
```csharp
public SystemTextJsonSerializer()
```


#### Constructor `SystemTextJsonSerializer(JsonSerializerOptions)`

Create the new serializer that uses System.Text.Json.JsonSerializer with custom settings

##### Syntax
```csharp
public SystemTextJsonSerializer(JsonSerializerOptions options)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`options` | `JsonSerializerOptions` | Json serializer settings



#### Method `Serialize(Object)`

##### Syntax
```csharp
public string Serialize(object obj)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`obj` | `object` | 

##### Returns
Type | Description
--- | ---
`string` | 



#### Method `Serialize(Parameter)`

##### Syntax
```csharp
public string Serialize(Parameter bodyParameter)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`bodyParameter` | `Parameter` | 

##### Returns
Type | Description
--- | ---
`string` | 



#### Method `Deserialize<T>(IRestResponse)`

##### Syntax
```csharp
public T Deserialize<T>(IRestResponse response)
```
##### Generic parameters
Name | Description
--- | ---
`T` | 

##### Parameters
Name | Type | Description
--- | --- | ---
`response` | `IRestResponse` | 

##### Returns
Type | Description
--- | ---
`T` | 



#### Property `SupportedContentTypes`

##### Syntax
```csharp
public string[] SupportedContentTypes { get; }
```


#### Property `ContentType`

##### Syntax
```csharp
public string ContentType { get; set; }
```


#### Property `DataFormat`

##### Syntax
```csharp
public DataFormat DataFormat { get; }
```

