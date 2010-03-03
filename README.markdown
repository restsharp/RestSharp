# RestSharp - .NET REST Client That (Hopefully) Doesn't Suck  
## [http://restsharp.org][1] - [@RestSharp][2]  

## License: Apache License 2.0  

### Key Features

* Automatic XML and JSON deserialization
* Fuzzy name matching ('product_id' in XML/JSON will match property named 'ProductId')
* Automatic detection of type of content returned
* GET, POST, PUT, HEAD, OPTIONS, DELETE supported
* Basic, NTLM and Parameter-based Authenticators included
* Supports custom authentication schemes
* Multi-part form/file uploads
* T4 Helper to generate C# classes from an XML document

### Basic Usage

    var request = new RestRequest(); // GET by default
    // request.Verb = Method.GET | Method.POST | Method.PUT | Method.DELETE | Method.HEAD | Method.Options
    request.BaseUrl = "http://example.com";
    request.Action = "resource";
    request.AddParameter("name", "value");

    // add parameters for all properties on an object
    request.AddObject(object);

    // or just whitelisted properties
    request.AddObject(object, "PersonId", "Name", ...);
    
    // easily add HTTP Headers
    request.AddParameter("header", "value", ParameterType.HttpHeader);

    // supports XML/JSON request bodies
    request.RequestFormat = RequestFormat.Xml;
    request.AddBody(object);

    // add files (only works with compatible verbs)
    request.AddFile(path);
    
    var client = new RestClient();
    // client.Authenticator = new BasicAuthenticator(username, password);
    
    // get raw response
    RestResponse response = client.Execute(request);
    // response.Content : string representation of response
    
    // or automatically deserialize result
    // return content type is sniffed but can be explicitly set via RestClient.AddHandler();
    Person person = client.Execute<Person>(request);

    // or download and save file to disk
    client.DownloadData(request).SaveAs(path);

    // shortcuts for parsing xml/feeds
    client.ExecuteAsXDocument(request);
    client.ExecuteAsXmlDocument(request);
    client.ExecuteAsSyndicationFeed(request);
 
  [1]: http://restsharp.org
  [2]: http://twitter.com/RestSharp