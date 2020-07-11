# Request Parameters

After you've created a `RestRequest`, you can add parameters to it.
Here is a Description of the 5 currently supported types and their behavior when using the default IHttp implementation.

## Cookie

Adds the parameter to the list of cookies that are sent along with the request. The cookie name is the name of the parameter and the value is the `Value.ToString()` you passed in.

## Http Header

Adds the parameter as an HTTP header that is sent along with the request. The header name is the name of the parameter and the header value is the value.

Note that there are some restricted headers that may behave differently or that are simply ignored. Please look at the `_restrictedHeaderActions` dictionary in `Http.cs` to see which headers are special and how they behave.

## Get or Post

This behaves differently based on the method. If you execute a GET call, RestSharp will append the parameters to the Url in the form `url?name1=value1&name2=value2`.

On a POST or PUT Requests, it depends on whether you have files attached to a Request.
If not, the Parameters will be sent as the body of the request in the form `name1=value1&name2=value2`. Also, the request will be sent as `application/x-www-form-urlencoded`.

In both cases, name and value will automatically be url-encoded.

If you have files, RestSharp will send a `multipart/form-data` request. Your parameters will be part of this request in the form:

```
Content-Disposition: form-data; name="parameterName"

ParameterValue
```

## Url Segment

Unlike `GetOrPost`, this `ParameterType` replaces placeholder values in the `RequestUrl`:

```csharp
var request = new RestRequest("health/{entity}/status")
    .AddUrlSegment("entity", "s2");
```

When the request executes, RestSharp will try to match any `{placeholder}` with a parameter of that name (without the `{}`) and replace it with the value. So the above code results in `health/s2/status` being the url.

## Request Body

If this parameter is set, its value will be sent as the body of the request. *Only one* `RequestBody` parameter is accepted - the first one.

The name of the parameter will be used as the `Content-Type` header for the request.

`RequestBody` does not work on `GET` or `HEAD` Requests, as they do not send a body.

If you have `GetOrPost` parameters as well, they will overwrite the `RequestBody` - RestSharp will not combine them, but it will instead throw the `RequestBody` parameter away.

We recommend using `AddJsonBody` or `AddXmlBody` methods instead of `AddParameter` with type `BodyParameter`. Those methods will set the proper request type and do the serialization work for you.

### AddJsonBody

When you call `AddJsonBody`, it does the following for you:

 - Instructs the RestClient to serialize the object parameter as JSON when making a request
 - Sets the content type to `application/json`
 - Sets the internal data type of the request body to `DataType.Json`

Do not set content type headers or send JSON string or some sort of `JObject` instance to `AddJsonBody`, it won't work!

Here is the example:

```csharp
var param = new MyClass { IntData = 1, StringData = "test123" };
request.AddJsonBody(param);
```

### AddXmlBody

When you call `AddXmlBody`, it does the following for you:

 - Instructs the RestClient to serialize the object parameter as XML when making a request
 - Sets the content type to `application/xml`
 - Sets the internal data type of the request body to `DataType.Xml`

Do not set content type headers or send XML string to `AddXmlBody`, it won't work!

## Query String

This works like `GetOrPost`, except that it always appends the parameters to the url in the form `url?name1=value1&name2=value2`, regardless of the request method. 

Example:

```csharp
var client = new RestClient("https://search.me");
var request = new RestRequest("search")
    .AddParameter("foo", "bar");
var response = await client.GetAsync<SearchResponse>(request);
```

It will send a `GET` request to `https://search.me/search?foo=bar")`.

You can also specify the query string parameter type explicitly:

```csharp
request.AddParameter("foo", "bar", RequestType.QueryString);
```

In some cases you might need to prevent RestSharp from encoding the query string parameter. To do so, use the `QueryStringWithoutEncode` parameter type.
