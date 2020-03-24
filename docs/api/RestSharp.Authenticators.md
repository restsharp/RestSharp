# Namespace: RestSharp.Authenticators
## Class `AuthenticatorBase`

### Inheritance
↳ `object`

### Inherited members

### Syntax
```csharp
public abstract class AuthenticatorBase : IAuthenticator
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Constructor `AuthenticatorBase(String)`

#### Syntax
```csharp
protected AuthenticatorBase(string token)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`token` | `string` | 



### Property `Token`

#### Syntax
```csharp
protected string Token { get; }
```
#### Property value
Type | Description
--- | ---
`string` | 



### Method `GetAuthenticationParameter(String)`

#### Syntax
```csharp
protected abstract Parameter GetAuthenticationParameter(string accessToken)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`accessToken` | `string` | 

#### Returns
Type | Description
--- | ---
`RestSharp.Parameter` | 



### Method `Authenticate(IRestClient, IRestRequest)`

#### Syntax
```csharp
public void Authenticate(IRestClient client, IRestRequest request)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`client` | `RestSharp.IRestClient` | 
`request` | `RestSharp.IRestRequest` | 



## Class `HttpBasicAuthenticator`

Allows &quot;basic access authentication&quot; for HTTP requests.

### Remarks

Encoding can be specified depending on what your server expect (see https://stackoverflow.com/a/7243567).
UTF-8 is used by default but some servers might expect ISO-8859-1 encoding.

### Inheritance
↳ `object`<br>&nbsp;&nbsp;↳ `RestSharp.Authenticators.AuthenticatorBase`

### Inherited members

### Syntax
```csharp
public class HttpBasicAuthenticator : AuthenticatorBase, IAuthenticator
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Constructor `HttpBasicAuthenticator(String, String)`

#### Syntax
```csharp
public HttpBasicAuthenticator(string username, string password)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`username` | `string` | 
`password` | `string` | 



### Constructor `HttpBasicAuthenticator(String, String, Encoding)`

#### Syntax
```csharp
public HttpBasicAuthenticator(string username, string password, Encoding encoding)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`username` | `string` | 
`password` | `string` | 
`encoding` | `System.Text.Encoding` | 



### Method `GetAuthenticationParameter(String)`

#### Syntax
```csharp
protected override Parameter GetAuthenticationParameter(string accessToken)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`accessToken` | `string` | 

#### Returns
Type | Description
--- | ---
`RestSharp.Parameter` | 



## Interface `IAuthenticator`


### Inherited members

### Syntax
```csharp
public interface IAuthenticator
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Method `Authenticate(IRestClient, IRestRequest)`

#### Syntax
```csharp
void Authenticate(IRestClient client, IRestRequest request)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`client` | `RestSharp.IRestClient` | 
`request` | `RestSharp.IRestRequest` | 



## Class `JwtAuthenticator`

JSON WEB TOKEN (JWT) Authenticator class.
<remarks>https://tools.ietf.org/html/draft-ietf-oauth-json-web-token</remarks>

### Inheritance
↳ `object`

### Inherited members

### Syntax
```csharp
public class JwtAuthenticator : IAuthenticator
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Constructor `JwtAuthenticator(String)`

#### Syntax
```csharp
public JwtAuthenticator(string accessToken)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`accessToken` | `string` | 



### Method `SetBearerToken(String)`

Set the new bearer token so the request gets the new header value

#### Syntax
```csharp
public void SetBearerToken(string accessToken)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`accessToken` | `string` | 



### Method `Authenticate(IRestClient, IRestRequest)`

#### Syntax
```csharp
public void Authenticate(IRestClient client, IRestRequest request)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`client` | `RestSharp.IRestClient` | 
`request` | `RestSharp.IRestRequest` | 



## Class `NtlmAuthenticator`

Tries to Authenticate with the credentials of the currently logged in user, or impersonate a user

### Inheritance
↳ `object`

### Inherited members

### Syntax
```csharp
public class NtlmAuthenticator : IAuthenticator
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Constructor `NtlmAuthenticator()`

Authenticate with the credentials of the currently logged in user

#### Syntax
```csharp
public NtlmAuthenticator()
```


### Constructor `NtlmAuthenticator(String, String)`

Authenticate by impersonation

#### Syntax
```csharp
public NtlmAuthenticator(string username, string password)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`username` | `string` | 
`password` | `string` | 



### Constructor `NtlmAuthenticator(ICredentials)`

Authenticate by impersonation, using an existing <code>ICredentials</code> instance

#### Syntax
```csharp
public NtlmAuthenticator(ICredentials credentials)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`credentials` | `ICredentials` | 



### Method `Authenticate(IRestClient, IRestRequest)`

#### Syntax
```csharp
public void Authenticate(IRestClient client, IRestRequest request)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`client` | `RestSharp.IRestClient` | 
`request` | `RestSharp.IRestRequest` | 



## Class `OAuth1Authenticator`

### See also
[RFC: The OAuth 1.0 Protocol](http://tools.ietf.org/html/rfc5849)
### Inheritance
↳ `object`

### Inherited members

### Syntax
```csharp
public class OAuth1Authenticator : IAuthenticator
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Property `Realm`

#### Syntax
```csharp
public virtual string Realm { get; set; }
```
#### Property value
Type | Description
--- | ---
`string` | 



### Property `ParameterHandling`

#### Syntax
```csharp
public virtual OAuthParameterHandling ParameterHandling { get; set; }
```
#### Property value
Type | Description
--- | ---
`RestSharp.Authenticators.OAuth.OAuthParameterHandling` | 



### Property `SignatureMethod`

#### Syntax
```csharp
public virtual OAuthSignatureMethod SignatureMethod { get; set; }
```
#### Property value
Type | Description
--- | ---
`RestSharp.Authenticators.OAuth.OAuthSignatureMethod` | 



### Property `SignatureTreatment`

#### Syntax
```csharp
public virtual OAuthSignatureTreatment SignatureTreatment { get; set; }
```
#### Property value
Type | Description
--- | ---
`RestSharp.Authenticators.OAuth.OAuthSignatureTreatment` | 



### Method `Authenticate(IRestClient, IRestRequest)`

#### Syntax
```csharp
public void Authenticate(IRestClient client, IRestRequest request)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`client` | `RestSharp.IRestClient` | 
`request` | `RestSharp.IRestRequest` | 



### Method `ForRequestToken(String, String, OAuthSignatureMethod)`

#### Syntax
```csharp
public static OAuth1Authenticator ForRequestToken(string consumerKey, string consumerSecret, OAuthSignatureMethod signatureMethod = OAuthSignatureMethod.HmacSha1)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`consumerKey` | `string` | 
`consumerSecret` | `string` | 
`signatureMethod` | `RestSharp.Authenticators.OAuth.OAuthSignatureMethod` | 

#### Returns
Type | Description
--- | ---
`RestSharp.Authenticators.OAuth1Authenticator` | 



### Method `ForRequestToken(String, String, String)`

#### Syntax
```csharp
public static OAuth1Authenticator ForRequestToken(string consumerKey, string consumerSecret, string callbackUrl)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`consumerKey` | `string` | 
`consumerSecret` | `string` | 
`callbackUrl` | `string` | 

#### Returns
Type | Description
--- | ---
`RestSharp.Authenticators.OAuth1Authenticator` | 



### Method `ForAccessToken(String, String, String, String, OAuthSignatureMethod)`

#### Syntax
```csharp
public static OAuth1Authenticator ForAccessToken(string consumerKey, string consumerSecret, string token, string tokenSecret, OAuthSignatureMethod signatureMethod = OAuthSignatureMethod.HmacSha1)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`consumerKey` | `string` | 
`consumerSecret` | `string` | 
`token` | `string` | 
`tokenSecret` | `string` | 
`signatureMethod` | `RestSharp.Authenticators.OAuth.OAuthSignatureMethod` | 

#### Returns
Type | Description
--- | ---
`RestSharp.Authenticators.OAuth1Authenticator` | 



### Method `ForAccessToken(String, String, String, String, String)`

#### Syntax
```csharp
public static OAuth1Authenticator ForAccessToken(string consumerKey, string consumerSecret, string token, string tokenSecret, string verifier)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`consumerKey` | `string` | 
`consumerSecret` | `string` | 
`token` | `string` | 
`tokenSecret` | `string` | 
`verifier` | `string` | 

#### Returns
Type | Description
--- | ---
`RestSharp.Authenticators.OAuth1Authenticator` | 



### Method `ForAccessTokenRefresh(String, String, String, String, String)`



#### Syntax
```csharp
public static OAuth1Authenticator ForAccessTokenRefresh(string consumerKey, string consumerSecret, string token, string tokenSecret, string sessionHandle)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`consumerKey` | `string` | 
`consumerSecret` | `string` | 
`token` | `string` | 
`tokenSecret` | `string` | 
`sessionHandle` | `string` | 

#### Returns
Type | Description
--- | ---
`RestSharp.Authenticators.OAuth1Authenticator` | 



### Method `ForAccessTokenRefresh(String, String, String, String, String, String)`



#### Syntax
```csharp
public static OAuth1Authenticator ForAccessTokenRefresh(string consumerKey, string consumerSecret, string token, string tokenSecret, string verifier, string sessionHandle)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`consumerKey` | `string` | 
`consumerSecret` | `string` | 
`token` | `string` | 
`tokenSecret` | `string` | 
`verifier` | `string` | 
`sessionHandle` | `string` | 

#### Returns
Type | Description
--- | ---
`RestSharp.Authenticators.OAuth1Authenticator` | 



### Method `ForClientAuthentication(String, String, String, String, OAuthSignatureMethod)`



#### Syntax
```csharp
public static OAuth1Authenticator ForClientAuthentication(string consumerKey, string consumerSecret, string username, string password, OAuthSignatureMethod signatureMethod = OAuthSignatureMethod.HmacSha1)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`consumerKey` | `string` | 
`consumerSecret` | `string` | 
`username` | `string` | 
`password` | `string` | 
`signatureMethod` | `RestSharp.Authenticators.OAuth.OAuthSignatureMethod` | 

#### Returns
Type | Description
--- | ---
`RestSharp.Authenticators.OAuth1Authenticator` | 



### Method `ForProtectedResource(String, String, String, String, OAuthSignatureMethod)`



#### Syntax
```csharp
public static OAuth1Authenticator ForProtectedResource(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, OAuthSignatureMethod signatureMethod = OAuthSignatureMethod.HmacSha1)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`consumerKey` | `string` | 
`consumerSecret` | `string` | 
`accessToken` | `string` | 
`accessTokenSecret` | `string` | 
`signatureMethod` | `RestSharp.Authenticators.OAuth.OAuthSignatureMethod` | 

#### Returns
Type | Description
--- | ---
`RestSharp.Authenticators.OAuth1Authenticator` | 



## Class `OAuth2Authenticator`

Base class for OAuth 2 Authenticators.

### Remarks

Since there are many ways to authenticate in OAuth2,
this is used as a base class to differentiate between
other authenticators.
Any other OAuth2 authenticators must derive from this
abstract class.

### Inheritance
↳ `object`

### Inherited members

### Syntax
```csharp
[Obsolete("Check the OAuth2 authenticators implementation on how to use the AuthenticatorBase instead")]
public abstract class OAuth2Authenticator : IAuthenticator
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Constructor `OAuth2Authenticator(String)`

#### Syntax
```csharp
protected OAuth2Authenticator(string accessToken)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`accessToken` | `string` | 



### Property `AccessToken`

Gets the access token.

#### Syntax
```csharp
public string AccessToken { get; }
```
#### Property value
Type | Description
--- | ---
`string` | 



### Method `Authenticate(IRestClient, IRestRequest)`

#### Syntax
```csharp
public void Authenticate(IRestClient client, IRestRequest request)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`client` | `RestSharp.IRestClient` | 
`request` | `RestSharp.IRestRequest` | 



### Method `GetAuthenticationParameter(String)`

#### Syntax
```csharp
protected abstract Parameter GetAuthenticationParameter(string accessToken)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`accessToken` | `string` | 

#### Returns
Type | Description
--- | ---
`RestSharp.Parameter` | 



## Class `OAuth2AuthorizationRequestHeaderAuthenticator`

The OAuth 2 authenticator using the authorization request header field.

### Remarks

Based on http://tools.ietf.org/html/draft-ietf-oauth-v2-10#section-5.1.1

### Inheritance
↳ `object`<br>&nbsp;&nbsp;↳ `RestSharp.Authenticators.AuthenticatorBase`

### Inherited members

### Syntax
```csharp
public class OAuth2AuthorizationRequestHeaderAuthenticator : AuthenticatorBase, IAuthenticator
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Constructor `OAuth2AuthorizationRequestHeaderAuthenticator(String)`

#### Syntax
```csharp
public OAuth2AuthorizationRequestHeaderAuthenticator(string accessToken)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`accessToken` | `string` | 



### Constructor `OAuth2AuthorizationRequestHeaderAuthenticator(String, String)`

#### Syntax
```csharp
public OAuth2AuthorizationRequestHeaderAuthenticator(string accessToken, string tokenType)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`accessToken` | `string` | 
`tokenType` | `string` | 



### Method `GetAuthenticationParameter(String)`

#### Syntax
```csharp
protected override Parameter GetAuthenticationParameter(string accessToken)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`accessToken` | `string` | 

#### Returns
Type | Description
--- | ---
`RestSharp.Parameter` | 



## Class `OAuth2UriQueryParameterAuthenticator`

The OAuth 2 authenticator using URI query parameter.

### Remarks

Based on http://tools.ietf.org/html/draft-ietf-oauth-v2-10#section-5.1.2

### Inheritance
↳ `object`<br>&nbsp;&nbsp;↳ `RestSharp.Authenticators.AuthenticatorBase`

### Inherited members

### Syntax
```csharp
public class OAuth2UriQueryParameterAuthenticator : AuthenticatorBase, IAuthenticator
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Constructor `OAuth2UriQueryParameterAuthenticator(String)`

#### Syntax
```csharp
public OAuth2UriQueryParameterAuthenticator(string accessToken)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`accessToken` | `string` | 



### Method `GetAuthenticationParameter(String)`

#### Syntax
```csharp
protected override Parameter GetAuthenticationParameter(string accessToken)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`accessToken` | `string` | 

#### Returns
Type | Description
--- | ---
`RestSharp.Parameter` | 



## Class `SimpleAuthenticator`

### Inheritance
↳ `object`

### Inherited members

### Syntax
```csharp
public class SimpleAuthenticator : IAuthenticator
```

### Extension methods
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Reflection.TypeInfo)`
-  `RestSharp.Extensions.ReflectionExtensions.ChangeType(object, System.Type, System.Globalization.CultureInfo)`
### Constructor `SimpleAuthenticator(String, String, String, String)`

#### Syntax
```csharp
public SimpleAuthenticator(string usernameKey, string username, string passwordKey, string password)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`usernameKey` | `string` | 
`username` | `string` | 
`passwordKey` | `string` | 
`password` | `string` | 



### Method `Authenticate(IRestClient, IRestRequest)`

#### Syntax
```csharp
public void Authenticate(IRestClient client, IRestRequest request)
```
#### Parameters
Name | Type | Description
--- | --- | ---
`client` | `RestSharp.IRestClient` | 
`request` | `RestSharp.IRestRequest` | 


