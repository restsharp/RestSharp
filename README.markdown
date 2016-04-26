# RestSharp - Simple .NET REST Client [![Build status](https://ci.appveyor.com/api/projects/status/5vdwwducje0miayf?svg=true)](https://ci.appveyor.com/project/hallem/restsharp)

![](https://ci5.googleusercontent.com/proxy/LSXBIaYndN6I0nqGyXGtKk3-woLLXMUj-UPxpJz6yhn-qUv5qHiIpW-8QczKLztBwl1TYyqlTV-1T4dL3o4lWmaZoy9S15ylU8WC5n-dpKFiwYPvWzIR4EumHgzx0q3ZFVyceR6aj-WfLkFu-LGdrGS1Mm-uW-mrEV7L_8HnfIwj0ASI3Ze0sbmjWoMtEvI6mA1mNYBW3wpeZe-BaHXMbTa84tKXKkZcvJC7-Gdsa8T334auZiRLJ_G2idmmgLafAVW_-WXYbbpTyXXXtv_3C4clLDeIOJSYWK_ll2H95THh-JhVgRCTvlgjKb5I=s0-d-e1-ft#https://camo.githubusercontent.com/b5192c7e6e9b9cd446ae5221b4d86a24dcc38a82/687474703a2f2f69632e706963732e6c6976656a6f75726e616c2e636f6d2f6c6a656e2f33393031393336382f31343833302f31343833305f6f726967696e616c2e6a7067)

RestSharp is in need of more maintainers.  Specifically ones who have more experience with PCL and UWP.  If you're interested, please open an issue at [RestSharp Maintainers](https://github.com/hallem/RestSharpMaintainers/issues).

### [Official Site/Blog][1] - [@RestSharp][2]  
### License: Apache License 2.0

### Features

* Supports .NET 3.5+, Silverlight 5, Windows Phone 8, Mono, MonoTouch, Mono for Android
* Easy installation using [NuGet](http://nuget.org/packages/RestSharp) for most .NET flavors
* Supports strong naming using [NuGet](http://nuget.org/packages/RestSharpSigned) for most .NET flavors
* Automatic XML and JSON deserialization
* Supports custom serialization and deserialization via ISerializer and IDeserializer
* Fuzzy element name matching ('product_id' in XML/JSON will match C# property named 'ProductId')
* Automatic detection of type of content returned
* GET, POST, PUT, PATCH, HEAD, OPTIONS, DELETE supported
* Other non-standard HTTP methods also supported
* oAuth 1, oAuth 2, Basic, NTLM and Parameter-based Authenticators included
* Supports custom authentication schemes via IAuthenticator
* Multi-part form/file uploads
* T4 Helper to generate C# classes from an XML document

```csharp
var client = new RestClient("http://example.com");
// client.Authenticator = new HttpBasicAuthenticator(username, password);

var request = new RestRequest("resource/{id}", Method.POST);
request.AddParameter("name", "value"); // adds to POST or URL querystring based on Method
request.AddUrlSegment("id", "123"); // replaces matching token in request.Resource

// add parameters for all properties on an object
request.AddObject(object);

// or just whitelisted properties
request.AddObject(object, "PersonId", "Name", ...);

// easily add HTTP Headers
request.AddHeader("header", "value");

// add files to upload (works with compatible verbs)
request.AddFile("file", path);

// execute the request
IRestResponse response = client.Execute(request);
var content = response.Content; // raw content as string

// or automatically deserialize result
// return content type is sniffed but can be explicitly set via RestClient.AddHandler();
IRestResponse<Person> response2 = client.Execute<Person>(request);
var name = response2.Data.Name;

// or download and save file to disk
client.DownloadData(request).SaveAs(path);

// easy async support
client.ExecuteAsync(request, response => {
    Console.WriteLine(response.Content);
});

// async with deserialization
var asyncHandle = client.ExecuteAsync<Person>(request, response => {
    Console.WriteLine(response.Data.Name);
});

// abort the request on demand
asyncHandle.Abort();
```
 
  [1]: http://restsharp.org
  [2]: http://twitter.com/RestSharp
  [3]: http://groups.google.com/group/RestSharp
