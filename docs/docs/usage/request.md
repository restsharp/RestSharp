---
sidebar_position: 4
title: Preparing requests
---

## Create a request

Before making a request using `RestClient`, you need to create a request instance:

```csharp
var request = new RestRequest(resource); // resource is the sub-path of the client base path
```

The default request type is `GET` and you can override it by setting the `Method` property. You can also set the method using the constructor overload:

```csharp
var request = new RestRequest(resource, Method.Post);
```

After you've created a `RestRequest`, you can add parameters to it. Below, you can find all the parameter types supported by RestSharp.

## Request headers

Adds the header parameter as an HTTP header that is sent along with the request. The header name is the parameter's name and the header value is the value.

You can use one of the following request methods to add a header parameter:

```csharp
AddHeader(string name, string value);
AddHeader<T>(string name, T value);           // value will be converted to string
AddOrUpdateHeader(string name, string value); // replaces the header if it already exists
```

For example:

```csharp
var request = new RestRequest("/path").AddHeader("X-Key", someKey);
```

You can also add header parameters to the client, and they will be added to every request made by the client. This is useful for adding authentication headers, for example.

```csharp
client.AddDefaultHeader(string name, string value);
```

:::warning Avoid setting Content-Type header
RestSharp will use the correct content type by default. Avoid adding the `Content-Type` header manually to your requests unless you are absolutely sure it is required. You can add a custom content type to the [body parameter](#request-body) itself.
:::

## Get or Post parameters

The default RestSharp parameter type is `GetOrPostParameter`. You can add `GetOrPost` parameter to the request using the `AddParameter` function:

```csharp
request
    .AddParameter("name1", "value1")
    .AddParameter("name2", "value2");
```

`GetOrPost` behaves differently based on the HTTP method. If you execute a `GET` call, RestSharp will append the parameters to the URL in the form `url?name1=value1&name2=value2`.

On a `POST` or `PUT` requests, it depends on whether you have files attached to a request.
If not, the parameters will be sent as the body of the request in the form `name1=value1&name2=value2`. Also, the request will be sent as `application/x-www-form-urlencoded`.

In both cases, name and value will automatically be URL-encoded, unless specified otherwise:

```csharp
request.AddParameter("name", "Væ üé", false); // don't encode the value
```

If you have files, RestSharp will send a `multipart/form-data` request. Your parameters will be part of this request in the form:

```
Content-Type: text/plain; charset=utf-8
Content-Disposition: form-data; name="parameterName"

ParameterValue
```

Sometimes, you need to override the default content type for the parameter when making a multipart form call. It's possible to do by setting the `ContentType` property of the parameter object. As an example, the code below will create a POST parameter with JSON value, and set the appropriate content type:

```csharp
var parameter = new GetOrPostParameter("someJson", "{\"attributeFormat\":\"pdf\"}") {
    ContentType = "application/json"
};
request.AddParameter(parameter);
```

When the request is set to use multipart content, the parameter will be sent as part of the request with the specified content type:

```
Content-Type: application/json; charset=utf-8
Content-Disposition: form-data; name="someJson"

{"attributeFormat":"pdf"}
```

You can also add `GetOrPost` parameter as a default parameter to the client. This will add the parameter to every request made by the client.

```csharp
client.AddDefaultParameter("foo", "bar");
```

It will work the same way as request parameters, except that it will be added to every request.

## Query string

`QueryString` works like `GetOrPost`, except that it always appends the parameters to the url in the form `url?name1=value1&name2=value2`, regardless of the request method.

Example:

```csharp
var client = new RestClient("https://search.me");
var request = new RestRequest("search")
    .AddParameter("foo", "bar");
var response = await client.GetAsync<SearchResponse>(request);
```

It will send a `GET` request to `https://search.me/search?foo=bar`.

For `POST`-style requests you need to add the query string parameter explicitly:

```csharp
request.AddQueryParameter("foo", "bar");
```

In some cases, you might need to prevent RestSharp from encoding the query string parameter.
To do so, set the `encode` argument to `false` when adding the parameter:

```csharp
request.AddQueryParameter("foo", "bar/fox", false);
```

You can also add a query string parameter as a default parameter to the client. This will add the parameter to every request made by the client.

```csharp
client.AddDefaultQueryParameter("foo", "bar");
```

The line above will result in all the requests made by that client instance to have `foo=bar` in the query string for all the requests made by that client.

## Using AddObject

You can avoid calling `AddParameter` multiple times if you collect all the parameters in an object, and then use `AddObject`.
For example, this code:

```csharp
var params = new {
    status = 1,
    priority = "high",
    ids = new [] { "123", "456" }
};
request.AddObject(params);
```

is equivalent to:

```csharp
request.AddParameter("status", 1);
request.AddParameter("priority", "high");
request.AddParameter("ids", "123,456");
```

Remember that `AddObject` only works if your properties have primitive types. It also works with collections of primitive types as shown above.

If you need to override the property name or format, you can do it using the `RequestProperty` attribute. For example:

```csharp
public class RequestModel {
    // override the name and the format
    [RequestProperty(Name = "from_date", Format = "d")]
    public DateTime FromDate { get; set; }
}

// add it to the request
request.AddObject(new RequestModel { FromDate = DateTime.Now });
```

In this case, the request will get a GET or POST parameter named `from_date` and its value would be the current date in short date format.

## Using AddObjectStatic

Request function `AddObjectStatic<T>(...)` allows using pre-compiled expressions for getting property values. Compared to `AddObject` that uses reflections for each call, `AddObjectStatic` caches functions to retrieve properties from an object of type `T`, so it works much faster.

You can instruct `AddObjectStatic` to use custom parameter names and formats, as well as supply the list of properties than need to be used as parameters. The last option could be useful if the type `T` has properties that don't need to be sent with HTTP call.

To use custom parameter name or format, use the `RequestProperty` attribute. For example:

```csharp
class TestObject {
    [RequestProperty(Name = "some_data")]
    public string SomeData { get; set; }

    [RequestProperty(Format = "d")]
    public DateTime SomeDate { get; set; }

    [RequestProperty(Name = "dates", Format = "d")]
    public DateTime[] DatesArray { get; set; }

    public int        Plain      { get; set; }
    public DateTime[] PlainArray { get; set; }
}
```

## URL segment parameter

Unlike `GetOrPost`, URL segment parameter replaces placeholder values in the request URL:

```csharp
var request = new RestRequest("health/{entity}/status")
    .AddUrlSegment("entity", "s2");
```

When the request executes, RestSharp will try to match any `{placeholder}` with a parameter of that name (without the `{}`) and replace it with the value. So the above code results in `health/s2/status` being the URL.

You can also add `UrlSegment` parameter as a default parameter to the client. This will add the parameter to every request made by the client.

```csharp
client.AddDefaultUrlSegment("foo", "bar");
```

## Cookies

You can add cookies to a request using the `AddCookie` method:

```csharp
request.AddCookie("foo", "bar");
```

RestSharp will add cookies from the request as cookie headers and then extract the matching cookies from the response. You can observe and extract response cookies using the `RestResponse.Cookies` properties, which has the `CookieCollection` type.

However, the usage of a default URL segment parameter is questionable as you can just include the parameter value to the base URL of the client. There is, however, a `CookieContainer` instance on the request level. You can either assign the pre-populated container to `request.CookieContainer`, or let the container be created by the request when you call `AddCookie`. Still, the container is only used to extract all the cookies from it and create cookie headers for the request instead of using the container directly. It's because the cookie container is normally configured on the `HttpClientHandler` level and cookies are shared between requests made by the same client. In most of the cases this behaviour can be harmful.

If your use case requires sharing cookies between requests made by the client instance, you can use the client-level `CookieContainer`, which you must provide as the options' property. You can add cookies to the container using the container API. No response cookies, however, would be auto-added to the container, but you can do it in code by getting cookies from the `Cookes` property of the response and adding them to the client-level container available via `IRestClient.Options.CookieContainer` property.

## Request Body

RestSharp supports multiple ways to add a request body:
- `AddJsonBody` for JSON payloads
- `AddXmlBody` for XML payloads
- `AddStringBody` for pre-serialized payloads

We recommend using `AddJsonBody` or `AddXmlBody` methods instead of `AddParameter` with type `BodyParameter`. Those methods will set the proper request type and do the serialization work for you.

When you make a `POST`, `PUT` or `PATCH` request and added `GetOrPost` [parameters](#get-or-post-parameters), RestSharp will send them as a URL-encoded form request body by default. When a request also has files, it will send a `multipart/form-data` request. You can also instruct RestSharp to send the body as `multipart/form-data` by setting the `AlwaysMultipartFormData` property to `true`.

You can specify a custom body content type if necessary. The `contentType` argument is available in all the overloads that add a request body.

It is not possible to add client-level default body parameters.

### String body

If you have a pre-serialized payload like a JSON string, you can use `AddStringBody` to add it as a body parameter. You need to specify the content type, so the remote endpoint knows what to do with the request body. For example:

```csharp
const json = "{ data: { foo: \"bar\" } }";
request.AddStringBody(json, ContentType.Json);
```

### JSON body

When you call `AddJsonBody`, it does the following for you:

- Instructs the RestClient to serialize the object parameter as JSON when making a request
- Sets the content type to `application/json`
- Sets the internal data type of the request body to `DataType.Json`

Here is the example:

```csharp
var param = new MyClass { IntData = 1, StringData = "test123" };
request.AddJsonBody(param);
```

It is possible to override the default content type by supplying the `contentType` argument. For example:

```csharp
request.AddJsonBody(param, "text/x-json");
```

If you use a pre-serialized string with `AddJsonBody`, it will be sent as-is. The `AddJsonBody` will detect if the parameter is a string and will add it as a string body with JSON content type.
Essentially, it means that top-level strings won't be serialized as JSON when you use `AddJsonBody`. To overcome this issue, you can use an overload of `AddJsonBody`, which allows you to tell RestSharp to serialize the string as JSON:

```csharp
const string payload = @"
""requestBody"": { 
    ""content"": { 
        ""application/json"": { 
            ""schema"": { 
                ""type"": ""string"" 
            } 
        } 
    } 
},";
request.AddJsonBody(payload, forceSerialize: true); // the string will be serialized
request.AddJsonBody(payload); // the string will NOT be serialized and will be sent as-is
```

### XML body

When you call `AddXmlBody`, it does the following for you:

- Instructs the RestClient to serialize the object parameter as XML when making a request
- Sets the content type to `application/xml`
- Sets the internal data type of the request body to `DataType.Xml`

:::warning
Do not send XML string to `AddXmlBody`; it won't work!
:::

## Uploading files

To add a file to the request you can use the `RestRequest` function called `AddFile`. The main function accepts the `FileParameter` argument:

```csharp
request.AddFile(fileParameter);
```

You can instantiate the file parameter using `FileParameter.Create` that accepts a bytes array, or `FileParameter.FromFile`, which will load the file from disk.

There are also extension functions that wrap the creation of `FileParameter` inside:

```csharp
// Adds a file from disk
AddFile(parameterName, filePath, contentType);

// Adds an array of bytes
AddFile(parameterName, bytes, fileName, contentType);

// Adds a stream returned by the getFile function
AddFile(parameterName, getFile, fileName, contentType);
```

Remember that `AddFile` will set all the necessary headers, so please don't try to set content headers manually.

You can also provide file upload options to the `AddFile` call. The options are:
- `DisableFilenameEncoding` (default `false`): if set to `true`, RestSharp will not encode the file name in the `Content-Disposition` header
- `DisableFilenameStar` (default `true`): if set to `true`, RestSharp will not add the `filename*` parameter to the `Content-Disposition` header

Example of using the options:

```csharp
var options = new FileParameterOptions {
    DisableFilenameEncoding = true,
    DisableFilenameStar = false
};
request.AddFile("file", filePath, options: options);
```

The options specified in the snippet above usually help when you upload files with non-ASCII characters in their names.
