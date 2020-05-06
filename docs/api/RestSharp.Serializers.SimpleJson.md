---
title: RestSharp.Serializers.SimpleJson
---

# Assembly: RestSharp.Serializers.SimpleJson
## Namespace: RestSharp
### Class `JsonArray`

Represents the json array.

#### Inheritance
↳ `object`<br>&nbsp;&nbsp;↳ `System.Collections.Generic.List<object>`
#### Syntax
```csharp
[EditorBrowsable(EditorBrowsableState.Never)]
[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
public class JsonArray : List<object>, IList<object>, ICollection<object>, IList, ICollection, IReadOnlyList<object>, IReadOnlyCollection<object>, IEnumerable<object>, IEnumerable
```

#### Constructor `JsonArray()`

##### Syntax
```csharp
public JsonArray()
```


#### Constructor `JsonArray(Int32)`

##### Syntax
```csharp
public JsonArray(int capacity)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`capacity` | `int` | 



#### Method `ToString()`

The json representation of the array.

##### Syntax
```csharp
public override string ToString()
```
##### Returns
Type | Description
--- | ---
`string` | The json representation of the array.



### Class `JsonObject`

Represents the json object.

#### Inheritance
↳ `DynamicObject`<br>&nbsp;&nbsp;↳ `System.Object`
#### Syntax
```csharp
[EditorBrowsable(EditorBrowsableState.Never)]
[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
public class JsonObject : DynamicObject, IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
```

#### Constructor `JsonObject()`

##### Syntax
```csharp
public JsonObject()
```


#### Constructor `JsonObject(IEqualityComparer<String>)`

##### Syntax
```csharp
public JsonObject(IEqualityComparer<string> comparer)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`comparer` | `System.Collections.Generic.IEqualityComparer<string>` | 



#### Property `Item[Int32]`

##### Syntax
```csharp
public object this[int index] { get; }
```
##### Parameters
Name | Type | Description
--- | --- | ---
`index` | `int` | 



#### Method `Add(String, Object)`

Adds the specified key.

##### Syntax
```csharp
public void Add(string key, object value)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`key` | `string` | The key.
`value` | `object` | The value.



#### Method `ContainsKey(String)`

Determines whether the specified key contains key.

##### Syntax
```csharp
public bool ContainsKey(string key)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`key` | `string` | The key.

##### Returns
Type | Description
--- | ---
`bool` | 
<code>true</code> if the specified key contains key; otherwise, <code>false</code>.




#### Property `Keys`

Gets the keys.

##### Syntax
```csharp
public ICollection<string> Keys { get; }
```


#### Method `Remove(String)`

Removes the specified key.

##### Syntax
```csharp
public bool Remove(string key)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`key` | `string` | The key.

##### Returns
Type | Description
--- | ---
`bool` | 



#### Method `TryGetValue(String, out Object)`

Tries the get value.

##### Syntax
```csharp
public bool TryGetValue(string key, out object value)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`key` | `string` | The key.
`value` | `object` | The value.

##### Returns
Type | Description
--- | ---
`bool` | 



#### Property `Values`

Gets the values.

##### Syntax
```csharp
public ICollection<object> Values { get; }
```


#### Property `Item[String]`

##### Syntax
```csharp
public object this[string key] { get; set; }
```
##### Parameters
Name | Type | Description
--- | --- | ---
`key` | `string` | 



#### Method `Add(KeyValuePair<String, Object>)`

Adds the specified item.

##### Syntax
```csharp
public void Add(KeyValuePair<string, object> item)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`item` | `System.Collections.Generic.KeyValuePair<string, object>` | The item.



#### Method `Clear()`

Clears this instance.

##### Syntax
```csharp
public void Clear()
```


#### Method `Contains(KeyValuePair<String, Object>)`

Determines whether [contains] [the specified item].

##### Syntax
```csharp
public bool Contains(KeyValuePair<string, object> item)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`item` | `System.Collections.Generic.KeyValuePair<string, object>` | The item.

##### Returns
Type | Description
--- | ---
`bool` | 
<code>true</code> if [contains] [the specified item]; otherwise, <code>false</code>.




#### Method `CopyTo(KeyValuePair<String, Object>[], Int32)`

Copies to.

##### Syntax
```csharp
public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`array` | `System.Collections.Generic.KeyValuePair<string, object>[]` | The array.
`arrayIndex` | `int` | Index of the array.



#### Property `Count`

Gets the count.

##### Syntax
```csharp
public int Count { get; }
```


#### Property `IsReadOnly`

Gets a value indicating whether this instance is read only.

##### Syntax
```csharp
public bool IsReadOnly { get; }
```


#### Method `Remove(KeyValuePair<String, Object>)`

Removes the specified item.

##### Syntax
```csharp
public bool Remove(KeyValuePair<string, object> item)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`item` | `System.Collections.Generic.KeyValuePair<string, object>` | The item.

##### Returns
Type | Description
--- | ---
`bool` | 



#### Method `GetEnumerator()`

Gets the enumerator.

##### Syntax
```csharp
public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
```
##### Returns
Type | Description
--- | ---
`System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, object>>` | 



#### Method `IEnumerable.GetEnumerator()`

##### Syntax
```csharp
IEnumerator IEnumerable.GetEnumerator()
```
##### Returns
Type | Description
--- | ---
`System.Collections.IEnumerator` | 



#### Method `ToString()`

##### Syntax
```csharp
public override string ToString()
```
##### Returns
Type | Description
--- | ---
`string` | 



#### Method `TryConvert(ConvertBinder, out Object)`

##### Syntax
```csharp
public override bool TryConvert(ConvertBinder binder, out object result)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`binder` | `ConvertBinder` | 
`result` | `object` | 

##### Returns
Type | Description
--- | ---
`bool` | 



#### Method `TryDeleteMember(DeleteMemberBinder)`

Provides the implementation for operations that delete an object member. This method is not intended for use in C# or Visual Basic.

##### Syntax
```csharp
public override bool TryDeleteMember(DeleteMemberBinder binder)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`binder` | `DeleteMemberBinder` | Provides information about the deletion.

##### Returns
Type | Description
--- | ---
`bool` | 
Alwasy returns true.




#### Method `TryGetIndex(GetIndexBinder, Object[], out Object)`

##### Syntax
```csharp
public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`binder` | `GetIndexBinder` | 
`indexes` | `object[]` | 
`result` | `object` | 

##### Returns
Type | Description
--- | ---
`bool` | 



#### Method `TryGetMember(GetMemberBinder, out Object)`

##### Syntax
```csharp
public override bool TryGetMember(GetMemberBinder binder, out object result)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`binder` | `GetMemberBinder` | 
`result` | `object` | 

##### Returns
Type | Description
--- | ---
`bool` | 



#### Method `TrySetIndex(SetIndexBinder, Object[], Object)`

##### Syntax
```csharp
public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`binder` | `SetIndexBinder` | 
`indexes` | `object[]` | 
`value` | `object` | 

##### Returns
Type | Description
--- | ---
`bool` | 



#### Method `TrySetMember(SetMemberBinder, Object)`

##### Syntax
```csharp
public override bool TrySetMember(SetMemberBinder binder, object value)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`binder` | `SetMemberBinder` | 
`value` | `object` | 

##### Returns
Type | Description
--- | ---
`bool` | 



#### Method `GetDynamicMemberNames()`

Returns the enumeration of all dynamic member names.

##### Syntax
```csharp
public override IEnumerable<string> GetDynamicMemberNames()
```
##### Returns
Type | Description
--- | ---
`System.Collections.Generic.IEnumerable<string>` | 
A sequence that contains dynamic member names.




### Class `SimpleJson`

This class encodes and decodes JSON strings.
Spec. details, see http://www.json.org/

JSON uses Arrays and Objects. These correspond here to the datatypes JsonArray(IList&lt;object>) and JsonObject(IDictionary&lt;string,object>).
All numbers are parsed to doubles.

#### Inheritance
↳ `object`
#### Syntax
```csharp
public static class SimpleJson
```

#### Method `DeserializeObject(String)`

Parses the string json into a value

##### Syntax
```csharp
public static object DeserializeObject(string json)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`json` | `string` | A JSON string.

##### Returns
Type | Description
--- | ---
`object` | An IList&lt;object>, a IDictionary&lt;string,object>, a double, a string, null, true, or false



#### Method `DeserializeObject(Char[])`

Parses the char array json into a value

##### Syntax
```csharp
public static object DeserializeObject(char[] json)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`json` | `char[]` | A JSON char array.

##### Returns
Type | Description
--- | ---
`object` | An IList&lt;object>, a IDictionary&lt;string,object>, a double, a string, null, true, or false



#### Method `TryDeserializeObject(Char[], out Object)`

Try parsing the json string into a value.

##### Syntax
```csharp
[SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate", Justification = "Need to support .NET 2")]
public static bool TryDeserializeObject(char[] json, out object obj)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`json` | `char[]` | 
A JSON string.

`obj` | `object` | 
The object.


##### Returns
Type | Description
--- | ---
`bool` | 
Returns true if successfull otherwise false.




#### Method `TryDeserializeObject(String, out Object)`

Try parsing the json string into a value.

##### Syntax
```csharp
[SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate", Justification = "Need to support .NET 2")]
public static bool TryDeserializeObject(string json, out object obj)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`json` | `string` | 
A JSON string.

`obj` | `object` | 
The object.


##### Returns
Type | Description
--- | ---
`bool` | 
Returns true if successfull otherwise false.




#### Method `DeserializeObject(String, Type, IJsonSerializerStrategy)`

##### Syntax
```csharp
public static object DeserializeObject(string json, Type type, IJsonSerializerStrategy jsonSerializerStrategy)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`json` | `string` | 
`type` | `System.Type` | 
`jsonSerializerStrategy` | `RestSharp.IJsonSerializerStrategy` | 

##### Returns
Type | Description
--- | ---
`object` | 



#### Method `DeserializeObject(Char[], Type, IJsonSerializerStrategy)`

##### Syntax
```csharp
public static object DeserializeObject(char[] json, Type type, IJsonSerializerStrategy jsonSerializerStrategy)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`json` | `char[]` | 
`type` | `System.Type` | 
`jsonSerializerStrategy` | `RestSharp.IJsonSerializerStrategy` | 

##### Returns
Type | Description
--- | ---
`object` | 



#### Method `DeserializeObject(String, Type)`

##### Syntax
```csharp
public static object DeserializeObject(string json, Type type)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`json` | `string` | 
`type` | `System.Type` | 

##### Returns
Type | Description
--- | ---
`object` | 



#### Method `DeserializeObject<T>(String, IJsonSerializerStrategy)`

##### Syntax
```csharp
public static T DeserializeObject<T>(string json, IJsonSerializerStrategy jsonSerializerStrategy)
```
##### Generic parameters
Name | Description
--- | ---
`T` | 

##### Parameters
Name | Type | Description
--- | --- | ---
`json` | `string` | 
`jsonSerializerStrategy` | `RestSharp.IJsonSerializerStrategy` | 

##### Returns
Type | Description
--- | ---
`T` | 



#### Method `DeserializeObject<T>(String)`

##### Syntax
```csharp
public static T DeserializeObject<T>(string json)
```
##### Generic parameters
Name | Description
--- | ---
`T` | 

##### Parameters
Name | Type | Description
--- | --- | ---
`json` | `string` | 

##### Returns
Type | Description
--- | ---
`T` | 



#### Method `SerializeObject(Object, IJsonSerializerStrategy)`

Converts a IDictionary&lt;string,object> / IList&lt;object> object into a JSON string

##### Syntax
```csharp
public static string SerializeObject(object json, IJsonSerializerStrategy jsonSerializerStrategy)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`json` | `object` | A IDictionary&lt;string,object> / IList&lt;object>
`jsonSerializerStrategy` | `RestSharp.IJsonSerializerStrategy` | Serializer strategy to use

##### Returns
Type | Description
--- | ---
`string` | A JSON encoded string, or null if object &apos;json&apos; is not serializable



#### Method `SerializeObject(Object)`

##### Syntax
```csharp
public static string SerializeObject(object json)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`json` | `object` | 

##### Returns
Type | Description
--- | ---
`string` | 



#### Method `EscapeToJavascriptString(String)`

##### Syntax
```csharp
public static string EscapeToJavascriptString(string jsonString)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`jsonString` | `string` | 

##### Returns
Type | Description
--- | ---
`string` | 



#### Property `CurrentJsonSerializerStrategy`

##### Syntax
```csharp
public static IJsonSerializerStrategy CurrentJsonSerializerStrategy { get; set; }
```


#### Property `PocoJsonSerializerStrategy`

##### Syntax
```csharp
[EditorBrowsable(EditorBrowsableState.Advanced)]
public static PocoJsonSerializerStrategy PocoJsonSerializerStrategy { get; }
```


### Interface `IJsonSerializerStrategy`

#### Syntax
```csharp
public interface IJsonSerializerStrategy
```

#### Method `TrySerializeNonPrimitiveObject(Object, out Object)`

##### Syntax
```csharp
[SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate", Justification = "Need to support .NET 2")]
bool TrySerializeNonPrimitiveObject(object input, out object output)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`input` | `object` | 
`output` | `object` | 

##### Returns
Type | Description
--- | ---
`bool` | 



#### Method `DeserializeObject(Object, Type)`

##### Syntax
```csharp
object DeserializeObject(object value, Type type)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`value` | `object` | 
`type` | `System.Type` | 

##### Returns
Type | Description
--- | ---
`object` | 



### Class `PocoJsonSerializerStrategy`

#### Inheritance
↳ `object`
#### Syntax
```csharp
public class PocoJsonSerializerStrategy : IJsonSerializerStrategy
```

#### Constructor `PocoJsonSerializerStrategy()`

##### Syntax
```csharp
public PocoJsonSerializerStrategy()
```


#### Method `MapClrMemberNameToJsonFieldName(String)`

##### Syntax
```csharp
protected virtual string MapClrMemberNameToJsonFieldName(string clrFieldName)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`clrFieldName` | `string` | 

##### Returns
Type | Description
--- | ---
`string` | 



#### Method `TrySerializeNonPrimitiveObject(Object, out Object)`

##### Syntax
```csharp
public virtual bool TrySerializeNonPrimitiveObject(object input, out object output)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`input` | `object` | 
`output` | `object` | 

##### Returns
Type | Description
--- | ---
`bool` | 



#### Method `DeserializeObject(Object, Type)`

##### Syntax
```csharp
[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
public virtual object DeserializeObject(object value, Type type)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`value` | `object` | 
`type` | `System.Type` | 

##### Returns
Type | Description
--- | ---
`object` | 



#### Method `SerializeEnum(Enum)`

##### Syntax
```csharp
protected virtual object SerializeEnum(Enum p)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`p` | `System.Enum` | 

##### Returns
Type | Description
--- | ---
`object` | 



#### Method `TrySerializeKnownTypes(Object, out Object)`

##### Syntax
```csharp
[SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate", Justification = "Need to support .NET 2")]
protected virtual bool TrySerializeKnownTypes(object input, out object output)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`input` | `object` | 
`output` | `object` | 

##### Returns
Type | Description
--- | ---
`bool` | 



#### Method `TrySerializeUnknownTypes(Object, out Object)`

##### Syntax
```csharp
[SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate", Justification = "Need to support .NET 2")]
protected virtual bool TrySerializeUnknownTypes(object input, out object output)
```
##### Parameters
Name | Type | Description
--- | --- | ---
`input` | `object` | 
`output` | `object` | 

##### Returns
Type | Description
--- | ---
`bool` | 


