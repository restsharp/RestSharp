# RestSharp - Simple .NET REST Client 

[![Build status](https://ci.appveyor.com/api/projects/status/5vdwwducje0miayf?svg=true)](https://ci.appveyor.com/project/hallem/restsharp)

[![Join the chat at https://gitter.im/RestSharp/RestSharp](https://badges.gitter.im/RestSharp/RestSharp.svg)](https://gitter.im/RestSharp/RestSharp?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

### [Official Site][1] - [@RestSharp][2]  

### License: Apache License 2.0

### Note on RestSharp.Signed

The `RestSharp` package is now signed so there is no need to install `RestSharp.Signed`, which is obsolete from v160.0.0.

### Features

* Assemblies for .NET 4.5.2 and .NET Standard 2.0
* Easy installation using [NuGet](http://nuget.org/packages/RestSharp) for most .NET flavors (signed)
* Automatic XML and JSON deserialization
* Supports custom serialization and deserialization via ISerializer and IDeserializer
* Fuzzy element name matching ('product_id' in XML/JSON will match C# property named 'ProductId')
* Automatic detection of type of content returned
* GET, POST, PUT, PATCH, HEAD, OPTIONS, DELETE, COPY supported
* Other non-standard HTTP methods also supported
* OAuth 1, OAuth 2, Basic, NTLM and Parameter-based Authenticators included
* Supports custom authentication schemes via IAuthenticator
* Multi-part form/file uploads

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
await client.ExecuteAsync(request);

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
