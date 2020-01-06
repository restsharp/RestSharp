# Recommended Usage

RestSharp works best as the foundation for a proxy class for your API. Here are a couple of examples from the <a href="http://github.com/twilio/twilio-csharp">Twilio</a> library.

Create a class to contain your API proxy implementation with an `Execute` method for funneling all requests to the API. 
This allows you to set commonly-used parameters and other settings (like authentication) shared across requests. 
Because an account ID and secret key are required for every request you are required to pass those two values when 
creating a new instance of the proxy. 

::: warning
Note that exceptions from `Execute` are not thrown but are available in the `ErrorException` property.
:::

```csharp
// TwilioApi.cs
public class TwilioApi 
{
    const string BaseUrl = "https://api.twilio.com/2008-08-01";

    readonly IRestClient _client;

    string _accountSid;

    public TwilioApi(string accountSid, string secretKey) 
    {
        _client = new RestClient(BaseUrl);
        _client.Authenticator = new HttpBasicAuthenticator(accountSid, secretKey);
        _accountSid = accountSid;
    }

    public T Execute<T>(RestRequest request) where T : new()
    {
        request.AddParameter("AccountSid", _accountSid, ParameterType.UrlSegment); // used on every request
        var response = _client.Execute<T>(request);

        if (response.ErrorException != null)
        {
            const string message = "Error retrieving response.  Check inner details for more info.";
            var twilioException = new Exception(message, response.ErrorException);
            throw twilioException;
        }
        return response.Data;
    }

}
```

Next, define a class that maps to the data returned by the API.

```csharp
// Call.cs
public class Call
{
    public string Sid { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
    public string CallSegmentSid { get; set; }
    public string AccountSid { get; set; }
    public string Called { get; set; }
    public string Caller { get; set; }
    public string PhoneNumberSid { get; set; }
    public int Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Duration { get; set; }
    public decimal Price { get; set; }
    public int Flags { get; set; }
}
```

Then add a method to query the API for the details of a specific Call resource.

```csharp
// TwilioApi.cs, GetCall method of TwilioApi class
public Call GetCall(string callSid) 
{
    var request = new RestRequest("Accounts/{AccountSid}/Calls/{CallSid}");
    request.RootElement = "Call";

    request.AddParameter("CallSid", callSid, ParameterType.UrlSegment);

    return Execute<Call>(request);
}
```

There's some magic here that RestSharp takes care of so you don't have to.

The API returns XML, which is automatically detected and deserialized to the Call object using the default `XmlDeserializer`.
By default a RestRequest is made via a GET HTTP request. You can change this by setting the `Method` property of `RestRequest` 
or specifying the method in the constructor when creating an instance (covered below).
Parameters of type `UrlSegment` have their values injected into the URL based on a matching token name existing in the Resource property value. 
`AccountSid` is set in `TwilioApi.Execute` because it is common to every request.
We specify the name of the root element to start deserializing from. In this case, the XML returned is `<Response><Call>...</Call></Response>` and since the Response element itself does not contain any information relevant to our model, we start the deserializing one step down the tree.

You can also make POST (and PUT/DELETE/HEAD/OPTIONS) requests:

```csharp
// TwilioApi.cs, method of TwilioApi class
public Call InitiateOutboundCall(CallOptions options) 
{
    Require.Argument("Caller", options.Caller);
    Require.Argument("Called", options.Called);
    Require.Argument("Url", options.Url);

    var request = new RestRequest("Accounts/{AccountSid}/Calls", Method.POST);
    request.RootElement = "Calls";

    request.AddParameter("Caller", options.Caller);
    request.AddParameter("Called", options.Called);
    request.AddParameter("Url", options.Url);

    if (options.Method.HasValue) request.AddParameter("Method", options.Method);
    if (options.SendDigits.HasValue()) request.AddParameter("SendDigits", options.SendDigits);
    if (options.IfMachine.HasValue) request.AddParameter("IfMachine", options.IfMachine.Value);
    if (options.Timeout.HasValue) request.AddParameter("Timeout", options.Timeout.Value);

    return Execute<Call>(request);
}
```

This example also demonstrates RestSharp's lightweight validation helpers. 
These helpers allow you to verify before making the request that the values submitted are valid. 
Read more about Validation here.

All of the values added via AddParameter in this example will be submitted as a standard encoded form, 
similar to a form submission made via a web page. If this were a GET-style request (GET/DELETE/OPTIONS/HEAD), 
the parameter values would be submitted via the query string instead. You can also add header and cookie 
parameters with `AddParameter`. To add all properties for an object as parameters, use `AddObject`. 
To add a file for upload, use `AddFile` (the request will be sent as a multipart encoded form). 
To include a request body like XML or JSON, use `AddXmlBody` or `AddJsonBody`.


