# RestSharp - Simple .NET Compact Framework REST Client 

### [Official Site][1] - [@RestSharp][2]  

### License: Apache License 2.0

### Features

* Supports .NET 3.5 Compact Framework
* Automatic XML and JSON deserialization
* Supports JSON.NET for serialization if required
* Supports custom serialization and deserialization via ISerializer and IDeserializer
* Fuzzy element name matching ('product_id' in XML/JSON will match C# property named 'ProductId')
* Automatic detection of type of content returned
* GET, POST, PUT, PATCH, HEAD, OPTIONS, DELETE supported
* Other non-standard HTTP methods also supported
* OAuth 1, OAuth 2, Basic, NTLM and Parameter-based Authenticators included
* Supports custom authentication schemes via IAuthenticator
* Multi-part form/file uploads
* T4 Helper to generate C# classes from an XML document

The primary difference between this version and the regular version is that this one has been customized to 
compile and support the .NET Compact Framework. Although no new development is being done on Compact Framework 
and Windows CE by Microsoft, a lot of existing Line Of Business applications and industrial handhelds still
support Windows CE. For our particular use we needed to move to REST for all future development and this project is a
stepping stone to let us bring the mobile client over to the REST platform so we can eventually migrate it more easily
to Android. Since I needed this, I figured others might need it also.

If you just want a pre-compiled version of the library, you can download the ZIP file from this repository here:

https://github.com/kendallb/RestSharp.CompactFramework/raw/master/RestSharp.CompactFramework.zip

A couple of things are not supported on the Compact Framework, like Cookies. So not everythign will work, but it works great
for most normal REST stuff. Also if you wish to use JSON.NET rather than the default simple JSON serializer, you can still download
and use JSON.NET 3.5.8 which was the last official version to support the Compact Framework.

https://github.com/JamesNK/Newtonsoft.Json/releases/tag/3.5.8

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
