# RestSharp Release Notes

Newer release notes can be found on the [Releases](https://github.com/restsharp/RestSharp/releases) page.

# 106.10

* Added a new package `RestSharp.Serializers.NewtonsoftJson`
* Added a new package `RestSharp.Serializers.Utf8Json`
* Added a new package `RestSharp.Serializers.SystemTextJson`
* New documentation website
* Added `ThrowOnAnyError` property that will tell RestSharp to throw instead of creating an error response
* Fixed the error response propagation for async calls

# 106.9

* Fixed the relative URI issue (thanks @maratoss)
* Added `AddDefaultHeaders` extension method to `RestClient` that accepts multiple headers (thanks @Kerl1310)
* Added `AddHeaders` method to `RestRequest` that accepts multiple headers (thanks @abailey7)
* Fixed the crash on `null` body parameter names (thanks to @ofirZelig, @mitcht and @kd5ziy)
* Fixed the exception when `Encoding` is set to null. Wasn't a bug but the exception was popping up in the debugger
* `IList` properties marked with `[SerializeAs(Content = true)]` attribute can be serialized as parent's XML tag content (thanks @partyz0ne)
* Better handling for the case a unicode character gets between chunks (thanks @stukalin)

# 106.6

* Fixed some new platform unsupported exceptions
* Fixed OAuth regression issues
* Moved serialization to the client
* Added `UseSerializer` to `IRestClient` to specify the client-level (de)serializer
* Added `UseDotNetXmlSerializer` extension to `IRestClient`

# 106.5
* Wrapped proxy discovery in try-catch for the platform unsupported exception
* Fixed DateTime deserialization with millisecond UTC conflict
* Fixed HttpMethod is not overridden in `RestClient.Sync.Execute(IRestRequest request, Method httpMethod)`
* Split `ISerializer` to `IJsonSerializer` and `IXmlSerializer` to avoid the namespace confusion
* Fixed double escape bug in SimpleJson

# 106.4.1
* Fixed the wrong HTTP method used in `Post` extension method
* Custom content type for multipart requests

# 106.4.0

* Added the XML documentation file to the NuGet package
* Fixed the issue with `AddBody` overrides the XML namespace with an empty string
* Fixed the issue when combining query parameters and JSON body caused an incorrect content type
* Marked `MethodTaskAsync<T>` extensions as obsolete (where `Method` is `Get`, `Post`, etc)
* Added new extensions for `MethodAsync<T>` (where `Method` is the same as above) that will return the result or throw an exception. Obsolete methods still don't throw and return an empty instance.
* You can now add query string parameters without encoding them
* Fixed the issue with query string parameters in combination with OAuth1

# 106.1.0

* Fixed ignoring the DeserializeAsAttribute for list properties
* Fixed the proxy issue on .NET Core
* Fixed Uri builder when the Resource is an absolute Uri
* Add RSA-SHA1 signing capability
* Add ability to customize the Host header

# 106.0.1

* Added support of .NET Standard 2.0, enabling development for .NET Core 2.0
* Support for .NET 3.5, .NET 4.0, Silverlight, Windows Phone and Monotouch is removed
* Several smaller fixes

To see all commits for this version, [click here](https://github.com/RestSharp/RestSharp/compare/106.0.1...105.2.3).

## 105.2.2

* Added nuget targets for windows phone 8 and 8.1, monotouch10, monoandroid10, xamarin.ios10, net46
* Fixed the silverlight target to be sl5
* Added all projects to the solution
* Cleaned up and consolidated the build and packaging scripts
* **Code clean-up and namespace patching may cause breaking changes**

To see all commits for this version, [click here](https://github.com/RestSharp/RestSharp/compare/105.2.1...105.2.2).

## 105.2.1

* **Code clean-up and namespace patching may cause breaking changes**

To see all commits for this version, [click here](https://github.com/RestSharp/RestSharp/compare/105.1.0...105.2.1).

## 105.1.0

To see all commits for this version, [click here](https://github.com/RestSharp/RestSharp/compare/105.0...105.1.0).

## 105.0.1

To see all commits for this version, [click here](https://github.com/RestSharp/RestSharp/compare/105.0...105.0.1).

### Bug Fixes

* Reverted changes to parameter encoding

## 105.0.0

To see all commits for this version, [click here](https://github.com/RestSharp/RestSharp/compare/104.5.0...105.0).

### New Features/Improvements

* Converted the BaseUrl to be a URI rather than a string **(potential breaking change)**
* Updated the SimpleJson package to the latest version **(potential breaking change)**
* Converted the use of tabs to spaces
* Added support for the DeserializeAs attribute on XML
* Added ability to deserialize into structs
* Added additional methods on RestRequest
  * `IRestRequest.AddJsonBody`
  * `IRestRequest.AddXmlBody`
  * `IRestRequest.AddQueryParameter`
* Added support for multi-part form request to allow both a request body and files

### Bug Fixes

* Fixed potential Null Reference Exceptions on the parameters in RestClient (ToString usage)

## 104.5.0

To see all commits for this version, [click here](https://github.com/RestSharp/RestSharp/compare/104.4.0...104.5.0).

## 104.4.0

To see all commits for this version, [click here](https://github.com/RestSharp/RestSharp/compare/104.3.3...104.4.0).

## 104.3.3

To see all commits for this version, [click here](https://github.com/RestSharp/RestSharp/compare/104.2...104.3.3).

### New Features/Improvements

* Support for query string parameters on POST requests
* Deserialize an integer to a bool property
* Enable Task extensions for Monotouch and Monodroid platforms
* Support for deserializing a dictionary of lists

### Bug Fixes

* Fixed regression that prevented deserializing requests when non-protocol errors occurred
* Properly URL encode strings longer than 32766 characters

## 104.2

To see all commits for this version, [click here](https://github.com/RestSharp/RestSharp/compare/104.1...104.2).

### New Features

* Allow specifying the body of a `PUT` or `POST` to be specified as a byte array.
* Added `ExecuteAsync` overloads that return `Task<T>`
* Improved handling of nullable types
* Support `DateTimeOffset` to `XmlDeserializer`

### Bug Fixes

* Crash if an XML attribute contains empty string
* Adding array of int to request
* Support XAuth parameters for OAuth parameter handling
* Memory leak around handling of Accepts header
* `ConfigureProxy` was not being called for async request
* Serialization for classes with `IList` properties
* Exception when executing async requests on multiple threads with one `RestClient`
* ResponseStatus.Aborted was not being set if request was aborted
* ClientCertificate threw `NotImplementedException` on Mono
* Fix decimal parsing for small decimal values

## 104.1

* Fixed bug where ExecuteAsync sometimes doesn't send data

## 104.0

* Fixed Windows Phone and Silverlight to use culture when calling Convert.ChangeType() (thanks trydis)
* Added support for non-standard HTTP methods (thanks jhoerr)  
  New API methods include:
  * `IRestClient.ExecuteAsyncGet()`
  * `IRestClient.ExecuteAsyncPost()`
  * `IRestClient.ExecuteAsyncGet<T>()`
  * `IRestClient.ExecuteAsyncPost<T>()`
  
  See [groups discussion](https://groups.google.com/forum/?fromgroups=#!topic/restsharp/FCLGE5By7AU) for more info

* Resolved an xAuth support issue in the OAuth1Authenticator (thanks artema)
* Change AddDefaultParameter methods to be extension methods (thanks haacked)  
  Added `RestClientExtensions.AddDefaultParameter()` with 4 overloads. See pull request [#311](https://github.com/restsharp/RestSharp/pull/311) for more info

* Adding support for deserializing enums from integer representations (thanks dontjee)

## 103.4

* Version bump to fix assembly versioning

## 103.3

* Added in the check for it being generic before calling GetGenericType Definition() (thanks nicwise)
* Add support for deserializing properties with an underscore prefix (thanks psampaio)
* BaseUrl is now virtual (thanks Haacked)
* Fixed List<T> json deserialization, when T was a something like DateTime, Timespan, Guid, etc. (thanks PedroLamas)
* Improve support for parsing iso8601 dates (thanks friism)

## 103.2

### New Features

* Allow deserializing a single item into a List<T> field, for JSON that only uses a list when there's more than one item for a given field (thanks petejohanson)
* Extended NtlmAuthenticator so that it can also impersonate a user (thanks kleinron)
* Added support for mapping JSON objects to Dictionary<string, string=""> (thanks petejohanson)
* Added ability to set Host and Date when built for .NET 4.0 (thanks lukebakken)
* Allow deserializing lists with null in them. Should resolve pull request (thanks petejohanson)
* Add support for deserializing JSON to subclasses of List<T> (thanks abaybuzskiy)

### Bugs fixed
* Fixed invalid OAuth1 signature for GET request (thanks trilobyte)
* Added some missing OAuth files to the .NET4 and Silverlight projects (thanks PedroLamas)
* Removed unused NewtonsoftJsonMonoTouch.dll and Newtonsoft.Json.MonoDroid.dll binaries (thanks attilah)
* Fixed various issues with MonoTouch/Droid ports (thanks attilah)
* Add ability to set Host and Date when built for .NET 4.0 (thanks lukebakken)
* Fixed XmlDeserializer issue not handling lowercase + dash root elements in some cases
* Fixed an issue where RestResponse.Request was not populated (thanks mattleibow)
* Don't crash on captive networks that intercept SSL (thanks aroben)

## 103.1

* #267 Added CLS Compliance
* #263 Fixed InvalidCastException 
* #218 Handles connection failures better
* #231 OAuth now complies with rfc3986 url hex encoding

## 103.0 - Remove dependency on Json.NET

* Remove WP7.0 support (7.1 Mango remains).

## 102.7

* Updating Json.NET to 4.0.8, misc fixes

## 102.6 

* Updating Json.NET reference to 4.0.5
