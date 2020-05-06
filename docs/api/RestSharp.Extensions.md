# Namespace: RestSharp.Extensions
## Class `MiscExtensions`

Extension method overload!

### Inheritance
↳ `object`
### Syntax
```csharp
public static class MiscExtensions
```

### Method `SaveAs(Byte[], String)`

Save a byte array to a file

#### Syntax
```csharp
[Obsolete("This method will be removed soon. If you use it, please copy the code to your project.")]
public static void SaveAs(this byte[] input, string path)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`input` | `byte[]` | Bytes to save
`path` | `string` | Full path to save file to



### Method `ReadAsBytes(Stream)`

Read a stream into a byte array

#### Syntax
```csharp
[Obsolete("This method will be removed soon. If you use it, please copy the code to your project.")]
public static byte[] ReadAsBytes(this Stream input)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`input` | `System.IO.Stream` | Stream to read

#### Returns
Type | Description
--- | ---
`byte[]` | byte[]



### Method `CopyTo(Stream, Stream)`

Copies bytes from one stream to another

#### Syntax
```csharp
[Obsolete("This method will be removed soon. If you use it, please copy the code to your project.")]
public static void CopyTo(this Stream input, Stream output)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`input` | `System.IO.Stream` | The input stream.
`output` | `System.IO.Stream` | The output stream.



### Method `AsString(Byte[], String)`

Converts a byte array to a string, using its byte order mark to convert it to the right encoding.
http://www.shrinkrays.net/code-snippets/csharp/an-extension-method-for-converting-a-byte-array-to-a-string.aspx

#### Syntax
```csharp
[Obsolete("This method will be removed soon. If you use it, please copy the code to your project.")]
public static string AsString(this byte[] buffer, string encoding)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`buffer` | `byte[]` | An array of bytes to convert
`encoding` | `string` | Content encoding. Will fallback to UTF8 if not a valid encoding.

#### Returns
Type | Description
--- | ---
`string` | The byte as a string.



### Method `AsString(Byte[])`

Converts a byte array to a string, using its byte order mark to convert it to the right encoding.
http://www.shrinkrays.net/code-snippets/csharp/an-extension-method-for-converting-a-byte-array-to-a-string.aspx

#### Syntax
```csharp
[Obsolete("This method will be removed soon. If you use it, please copy the code to your project.")]
public static string AsString(this byte[] buffer)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`buffer` | `byte[]` | An array of bytes to convert

#### Returns
Type | Description
--- | ---
`string` | The byte as a string using UTF8.



## Class `ReflectionExtensions`

Reflection extensions

### Inheritance
↳ `object`
### Syntax
```csharp
public static class ReflectionExtensions
```

### Method `GetAttribute<T>(MemberInfo)`

Retrieve an attribute from a member (property)

#### Syntax
```csharp
public static T GetAttribute<T>(this MemberInfo prop)
    where T : Attribute
```
#### Generic parameters
Name | Description
--- | ---
`T` | Type of attribute to retrieve

#### Parameters
Name | Type | Description
--- | --- | ---
`prop` | `System.Reflection.MemberInfo` | Member to retrieve attribute from

#### Returns
Type | Description
--- | ---
`T` | 



### Method `GetAttribute<T>(Type)`

Retrieve an attribute from a type

#### Syntax
```csharp
public static T GetAttribute<T>(this Type type)
    where T : Attribute
```
#### Generic parameters
Name | Description
--- | ---
`T` | Type of attribute to retrieve

#### Parameters
Name | Type | Description
--- | --- | ---
`type` | `System.Type` | Type to retrieve attribute from

#### Returns
Type | Description
--- | ---
`T` | 



### Method `IsSubclassOfRawGeneric(Type, Type)`

Checks a type to see if it derives from a raw generic (e.g. List[[]])

#### Syntax
```csharp
public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`toCheck` | `System.Type` | 
`generic` | `System.Type` | 

#### Returns
Type | Description
--- | ---
`bool` | 



### Method `ChangeType(Object, TypeInfo)`

#### Syntax
```csharp
[Obsolete("This method will be removed soon. If you use it, please copy the code to your project.")]
public static object ChangeType(this object source, TypeInfo newType)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`source` | `object` | 
`newType` | `System.Reflection.TypeInfo` | 

#### Returns
Type | Description
--- | ---
`object` | 



### Method `ChangeType(Object, Type, CultureInfo)`

#### Syntax
```csharp
public static object ChangeType(this object source, Type newType, CultureInfo culture)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`source` | `object` | 
`newType` | `System.Type` | 
`culture` | `System.Globalization.CultureInfo` | 

#### Returns
Type | Description
--- | ---
`object` | 



### Method `FindEnumValue(Type, String, CultureInfo)`

Find a value from a System.Enum by trying several possible variants
of the string value of the enum.

#### Syntax
```csharp
public static object FindEnumValue(this Type type, string value, CultureInfo culture)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`type` | `System.Type` | Type of enum
`value` | `string` | Value for which to search
`culture` | `System.Globalization.CultureInfo` | The culture used to calculate the name variants

#### Returns
Type | Description
--- | ---
`object` | 



## Class `ResponseExtensions`

### Inheritance
↳ `object`
### Syntax
```csharp
public static class ResponseExtensions
```

### Method `ToAsyncResponse<T>(IRestResponse)`

#### Syntax
```csharp
[Obsolete("This method will be removed soon. If you use it, please copy the code to your project.")]
public static IRestResponse<T> ToAsyncResponse<T>(this IRestResponse response)
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
`RestSharp.IRestResponse<T>` | 



## Class `ResponseStatusExtensions`

### Inheritance
↳ `object`
### Syntax
```csharp
public static class ResponseStatusExtensions
```

### Method `ToWebException(ResponseStatus)`

#### Syntax
```csharp
public static WebException ToWebException(this ResponseStatus responseStatus)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`responseStatus` | `RestSharp.ResponseStatus` | 

#### Returns
Type | Description
--- | ---
`WebException` | 



## Class `RSACryptoServiceProviderExtensions`

### Inheritance
↳ `object`
### Syntax
```csharp
public static class RSACryptoServiceProviderExtensions
```

### Method `FromXmlString2(RSACryptoServiceProvider, String)`

Imports the specified XML String into the crypto service provider

#### Remarks

.NET Core 2.0 doesn&apos;t provide an implementation of RSACryptoServiceProvider.FromXmlString/ToXmlString, so we have
to do it ourselves.
Source: https://gist.github.com/Jargon64/5b172c452827e15b21882f1d76a94be4/

#### Syntax
```csharp
public static void FromXmlString2(this RSACryptoServiceProvider rsa, string xmlString)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`rsa` | `RSACryptoServiceProvider` | 
`xmlString` | `string` | 



## Class `StringExtensions`

### Inheritance
↳ `object`
### Syntax
```csharp
public static class StringExtensions
```

### Method `UrlDecode(String)`

#### Syntax
```csharp
public static string UrlDecode(this string input)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`input` | `string` | 

#### Returns
Type | Description
--- | ---
`string` | 



### Method `UrlEncode(String)`

Uses Uri.EscapeDataString() based on recommendations on MSDN
http://blogs.msdn.com/b/yangxind/archive/2006/11/09/don-t-use-net-system-uri-unescapedatastring-in-url-decoding.aspx

#### Syntax
```csharp
public static string UrlEncode(this string input)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`input` | `string` | 

#### Returns
Type | Description
--- | ---
`string` | 



### Method `UrlEncode(String, Encoding)`

#### Syntax
```csharp
public static string UrlEncode(this string input, Encoding encoding)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`input` | `string` | 
`encoding` | `System.Text.Encoding` | 

#### Returns
Type | Description
--- | ---
`string` | 



### Method `HasValue(String)`

Check that a string is not null or empty

#### Syntax
```csharp
public static bool HasValue(this string input)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`input` | `string` | String to check

#### Returns
Type | Description
--- | ---
`bool` | bool



### Method `RemoveUnderscoresAndDashes(String)`

Remove underscores from a string

#### Syntax
```csharp
public static string RemoveUnderscoresAndDashes(this string input)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`input` | `string` | String to process

#### Returns
Type | Description
--- | ---
`string` | string



### Method `ParseJsonDate(String, CultureInfo)`

Parses most common JSON date formats

#### Syntax
```csharp
public static DateTime ParseJsonDate(this string input, CultureInfo culture)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`input` | `string` | JSON value to parse
`culture` | `System.Globalization.CultureInfo` | 

#### Returns
Type | Description
--- | ---
`System.DateTime` | DateTime



### Method `ToPascalCase(String, CultureInfo)`

Converts a string to pascal case

#### Syntax
```csharp
public static string ToPascalCase(this string lowercaseAndUnderscoredWord, CultureInfo culture)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`lowercaseAndUnderscoredWord` | `string` | String to convert
`culture` | `System.Globalization.CultureInfo` | 

#### Returns
Type | Description
--- | ---
`string` | string



### Method `ToPascalCase(String, Boolean, CultureInfo)`

Converts a string to pascal case with the option to remove underscores

#### Syntax
```csharp
public static string ToPascalCase(this string text, bool removeUnderscores, CultureInfo culture)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`text` | `string` | String to convert
`removeUnderscores` | `bool` | Option to remove underscores
`culture` | `System.Globalization.CultureInfo` | 

#### Returns
Type | Description
--- | ---
`string` | 



### Method `ToCamelCase(String, CultureInfo)`

Converts a string to camel case

#### Syntax
```csharp
public static string ToCamelCase(this string lowercaseAndUnderscoredWord, CultureInfo culture)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`lowercaseAndUnderscoredWord` | `string` | String to convert
`culture` | `System.Globalization.CultureInfo` | 

#### Returns
Type | Description
--- | ---
`string` | String



### Method `MakeInitialLowerCase(String)`

Convert the first letter of a string to lower case

#### Syntax
```csharp
public static string MakeInitialLowerCase(this string word)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`word` | `string` | String to convert

#### Returns
Type | Description
--- | ---
`string` | string



### Method `AddUnderscores(String)`

Add underscores to a pascal-cased string

#### Syntax
```csharp
public static string AddUnderscores(this string pascalCasedWord)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`pascalCasedWord` | `string` | String to convert

#### Returns
Type | Description
--- | ---
`string` | string



### Method `AddDashes(String)`

Add dashes to a pascal-cased string

#### Syntax
```csharp
public static string AddDashes(this string pascalCasedWord)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`pascalCasedWord` | `string` | String to convert

#### Returns
Type | Description
--- | ---
`string` | string



### Method `AddUnderscorePrefix(String)`

Add an undescore prefix to a pascasl-cased string

#### Syntax
```csharp
public static string AddUnderscorePrefix(this string pascalCasedWord)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`pascalCasedWord` | `string` | 

#### Returns
Type | Description
--- | ---
`string` | 



### Method `AddSpaces(String)`

Add spaces to a pascal-cased string

#### Syntax
```csharp
public static string AddSpaces(this string pascalCasedWord)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`pascalCasedWord` | `string` | String to convert

#### Returns
Type | Description
--- | ---
`string` | string



### Method `GetNameVariants(String, CultureInfo)`

Return possible variants of a name for name matching.

#### Syntax
```csharp
public static IEnumerable<string> GetNameVariants(this string name, CultureInfo culture)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`name` | `string` | String to convert
`culture` | `System.Globalization.CultureInfo` | The culture to use for conversion

#### Returns
Type | Description
--- | ---
`System.Collections.Generic.IEnumerable<string>` | IEnumerable&lt;string>



## Class `WebRequestExtensions`

### Inheritance
↳ `object`
### Syntax
```csharp
public static class WebRequestExtensions
```

### Method `GetRequestStreamAsync(WebRequest, CancellationToken)`

#### Syntax
```csharp
public static Task<Stream> GetRequestStreamAsync(this WebRequest webRequest, CancellationToken cancellationToken)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`webRequest` | `WebRequest` | 
`cancellationToken` | `System.Threading.CancellationToken` | 

#### Returns
Type | Description
--- | ---
`System.Threading.Tasks.Task<System.IO.Stream>` | 



### Method `GetResponseAsync(WebRequest, CancellationToken)`

#### Syntax
```csharp
public static Task<WebResponse> GetResponseAsync(this WebRequest webRequest, CancellationToken cancellationToken)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`webRequest` | `WebRequest` | 
`cancellationToken` | `System.Threading.CancellationToken` | 

#### Returns
Type | Description
--- | ---
`System.Threading.Tasks.Task<WebResponse>` | 



## Class `XmlExtensions`

XML Extension Methods

### Inheritance
↳ `object`
### Syntax
```csharp
public static class XmlExtensions
```

### Method `AsNamespaced(String, String)`

Returns the name of an element with the namespace if specified

#### Syntax
```csharp
public static XName AsNamespaced(this string name, string namespace)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`name` | `string` | Element name
`namespace` | `string` | XML Namespace

#### Returns
Type | Description
--- | ---
`XName` | 


