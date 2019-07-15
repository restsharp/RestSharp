# RestSharp - Simple .NET REST Client 

[![Build status](https://ci.appveyor.com/api/projects/status/5vdwwducje0miayf?svg=true)](https://ci.appveyor.com/project/hallem/restsharp)

[![Join the chat at https://gitter.im/RestSharp/RestSharp](https://badges.gitter.im/RestSharp/RestSharp.svg)](https://gitter.im/RestSharp/RestSharp?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

### [Official Site][1] - [@RestSharp][2] - [Google Group](https://groups.google.com/forum/#!forum/restsharp)

### License: Apache License 2.0

### Note on RestSharp.Signed

The `RestSharp` package is now signed so there is no need to install `RestSharp.Signed`, which is obsolete from v160.0.0.

### Note on JSON serialization

Some time ago, we have decided to get rid of the reference to `Newtonsoft.Json` package.
The intentions were good, we thought that the `SimpleJson` library would be a good replacement that can be embedded to the library itself,
so we don't need to have any external references.

However, as many good intentions, that change created more issues than it solved. The number of issues on GitHub that are
related to JSON (de)serialization is growing and `SimpleJson` is long abandoned. We faced a choice to start maintaining
`SimpleJson` ourselves or use something else.

Since as per today almost every .NET project has a direct or indirect reference to `Newtonsoft.Json`, we decided to bring it back as a dependency
and get rid of `SimpleJson`. This will be done in RestSharp v107, the next major version.

To prepare for this change, we made quite a few changes in how serialization works in RestSharp. Before, objects were serialized
when added to the `RestRequest` by using one of the `AddBody` methods. That made it impossible to assign a custom
serializer on the client level, so it should have been done for each request. In v106.6 body parameter is serialized just
before executing the request. Delaying the serialization allowed us to add the client-level serializer.

It is still possible to assign custom (de)serializer per request, as before. In addition to that, you can
use the new method `IRestClient.UseSerializer(IRestSerializer restSerializer)`. The `IRestSerializer` interface
has methods for serialization and deserialization. Default serializers are the same as before.

From v106.6.2 you can use `Newtonsoft.Json` for the `RestClient` by using code from this [snippet](https://gist.github.com/alexeyzimarev/c00b79c11c8cce6f6208454f7933ad24).

In addition to that, you can change the default XML serialization to use the `System.Xml` serializer, also known
as `DotNetXmlSerializer` and `DotNetXmlDeserializer`. In v106.6.2, you can simply write:

```csharp
client.UseDotNetXmlSerializer();
```

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

var request = new RestRequest("resource/{id}");
request.AddParameter("name", "value"); // adds to POST or URL querystring based on Method
request.AddUrlSegment("id", "123"); // replaces matching token in request.Resource

// add parameters for all properties on an object
request.AddObject(@object);

// or just whitelisted properties
request.AddObject(object, "PersonId", "Name", ...);

// easily add HTTP Headers
request.AddHeader("header", "value");

// add files to upload (works with compatible verbs)
request.AddFile("file", path);

// execute the request
var response = client.Post(request);
var content = response.Content; // raw content as string

// or automatically deserialize result
// return content type is sniffed but can be explicitly set via RestClient.AddHandler();
var response2 = client.Post<Person>(request);
var name = response2.Data.Name;

// or download and save file to disk
client.DownloadData(request).SaveAs(path);

// easy async support
await client.ExecuteTaskAsync(request);

// async with deserialization
var asyncHandle = client.PostAsync<Person>(request, response => {
    Console.WriteLine(response.Data.Name);
});

// abort the request on demand
asyncHandle.Abort();
```
 
  [1]: http://restsharp.org
  [2]: http://twitter.com/RestSharp
  [3]: http://groups.google.com/group/RestSharp
