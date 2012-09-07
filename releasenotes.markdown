# RestSharp Release Notes

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