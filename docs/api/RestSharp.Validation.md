# Namespace: RestSharp.Validation
## Class `Ensure`

### Inheritance
↳ `object`

### Inherited members

### Syntax
```csharp
public static class Ensure
```

### Method `NotNull(Object, String)`

#### Syntax
```csharp
public static void NotNull(object parameter, string name)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`parameter` | `object` | 
`name` | `string` | 



### Method `NotEmpty(String, String)`

#### Syntax
```csharp
public static void NotEmpty(string parameter, string name)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`parameter` | `string` | 
`name` | `string` | 



## Class `Require`

Helper methods for validating required values

### Inheritance
↳ `object`

### Inherited members

### Syntax
```csharp
public class Require
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Method `Argument(String, Object)`

Require a parameter to not be null

#### Syntax
```csharp
[Obsolete("This method will be removed soon. If you use it, please copy the code to your project.")]
public static void Argument(string argumentName, object argumentValue)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`argumentName` | `string` | Name of the parameter
`argumentValue` | `object` | Value of the parameter



## Class `Validate`

Helper methods for validating values

### Inheritance
↳ `object`

### Inherited members

### Syntax
```csharp
public class Validate
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Method `IsBetween(Int32, Int32, Int32)`

Validate an integer value is between the specified values (exclusive of min/max)

#### Syntax
```csharp
[Obsolete("This method will be removed soon. If you use it, please copy the code to your project.")]
public static void IsBetween(int value, int min, int max)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`value` | `int` | Value to validate
`min` | `int` | Exclusive minimum value
`max` | `int` | Exclusive maximum value



### Method `IsValidLength(String, Int32)`

Validate a string length

#### Syntax
```csharp
[Obsolete("This method will be removed soon. If you use it, please copy the code to your project.")]
public static void IsValidLength(string value, int maxSize)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`value` | `string` | String to be validated
`maxSize` | `int` | Maximum length of the string


