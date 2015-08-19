namespace RestSharp {
  public partial class Http : RestSharp.IHttp, RestSharp.IHttpFactory {
    public Http() { }
    public bool AlwaysMultipartFormData { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(bool); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Net.CookieContainer CookieContainer { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Net.CookieContainer); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Collections.Generic.IList<RestSharp.HttpCookie> Cookies { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Collections.Generic.IList<RestSharp.HttpCookie>); } }
    public System.Net.ICredentials Credentials { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Net.ICredentials); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Text.Encoding Encoding { get { return default(System.Text.Encoding); } set { } }
    public System.Collections.Generic.IList<RestSharp.HttpFile> Files { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Collections.Generic.IList<RestSharp.HttpFile>); } }
    public bool FollowRedirects { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(bool); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    protected bool HasBody { get { return default(bool); } }
    protected bool HasCookies { get { return default(bool); } }
    protected bool HasFiles { get { return default(bool); } }
    protected bool HasParameters { get { return default(bool); } }
    public System.Collections.Generic.IList<RestSharp.HttpHeader> Headers { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Collections.Generic.IList<RestSharp.HttpHeader>); } }
    public System.Collections.Generic.IList<RestSharp.HttpParameter> Parameters { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Collections.Generic.IList<RestSharp.HttpParameter>); } }
    public bool PreAuthenticate { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(bool); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public int ReadWriteTimeout { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(int); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string RequestBody { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Byte[] RequestBodyBytes { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Byte[]); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string RequestContentType { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Action<System.IO.Stream> ResponseWriter { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Action<System.IO.Stream>); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public int Timeout { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(int); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Uri Url { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Uri); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public bool UseDefaultCredentials { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(bool); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string UserAgent { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Net.HttpWebRequest AsGetAsync(System.Action<RestSharp.HttpResponse> action, string httpMethod) { return default(System.Net.HttpWebRequest); }
    public System.Net.HttpWebRequest AsPostAsync(System.Action<RestSharp.HttpResponse> action, string httpMethod) { return default(System.Net.HttpWebRequest); }
    public RestSharp.IHttp Create() { return default(RestSharp.IHttp); }
    public System.Net.HttpWebRequest DeleteAsync(System.Action<RestSharp.HttpResponse> action) { return default(System.Net.HttpWebRequest); }
    public System.Net.HttpWebRequest GetAsync(System.Action<RestSharp.HttpResponse> action) { return default(System.Net.HttpWebRequest); }
    public System.Net.HttpWebRequest HeadAsync(System.Action<RestSharp.HttpResponse> action) { return default(System.Net.HttpWebRequest); }
    public System.Net.HttpWebRequest MergeAsync(System.Action<RestSharp.HttpResponse> action) { return default(System.Net.HttpWebRequest); }
    public System.Net.HttpWebRequest OptionsAsync(System.Action<RestSharp.HttpResponse> action) { return default(System.Net.HttpWebRequest); }
    public System.Net.HttpWebRequest PatchAsync(System.Action<RestSharp.HttpResponse> action) { return default(System.Net.HttpWebRequest); }
    public System.Net.HttpWebRequest PostAsync(System.Action<RestSharp.HttpResponse> action) { return default(System.Net.HttpWebRequest); }
    public System.Net.HttpWebRequest PutAsync(System.Action<RestSharp.HttpResponse> action) { return default(System.Net.HttpWebRequest); }
  }
  public partial class HttpBasicAuthenticator : RestSharp.IAuthenticator {
    public HttpBasicAuthenticator(string username, string password) { }
    public void Authenticate(RestSharp.IRestClient client, RestSharp.IRestRequest request) { }
  }
  public partial interface IAuthenticator {
    void Authenticate(RestSharp.IRestClient client, RestSharp.IRestRequest request);
  }
  public abstract partial class OAuth2Authenticator : RestSharp.IAuthenticator {
    protected OAuth2Authenticator(string accessToken) { }
    public string AccessToken { get { return default(string); } }
    public abstract void Authenticate(RestSharp.IRestClient client, RestSharp.IRestRequest request);
  }
  public partial class OAuth2AuthorizationRequestHeaderAuthenticator : RestSharp.OAuth2Authenticator {
    public OAuth2AuthorizationRequestHeaderAuthenticator(string accessToken) : base (default(string)) { }
    public OAuth2AuthorizationRequestHeaderAuthenticator(string accessToken, string tokenType) : base (default(string)) { }
    public override void Authenticate(RestSharp.IRestClient client, RestSharp.IRestRequest request) { }
  }
  public partial class OAuth2UriQueryParameterAuthenticator : RestSharp.OAuth2Authenticator {
    public OAuth2UriQueryParameterAuthenticator(string accessToken) : base (default(string)) { }
    public override void Authenticate(RestSharp.IRestClient client, RestSharp.IRestRequest request) { }
  }
  public partial class RestClient : RestSharp.IRestClient {
    public RestSharp.IHttpFactory HttpFactory;
    public RestClient() { }
    public RestClient(string baseUrl) { }
    public RestClient(System.Uri baseUrl) { }
    public RestSharp.IAuthenticator Authenticator { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(RestSharp.IAuthenticator); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public virtual System.Uri BaseUrl { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Uri); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Net.CookieContainer CookieContainer { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Net.CookieContainer); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Collections.Generic.IList<RestSharp.Parameter> DefaultParameters { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Collections.Generic.IList<RestSharp.Parameter>); } }
    public System.Text.Encoding Encoding { get { return default(System.Text.Encoding); } set { } }
    public bool FollowRedirects { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(bool); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Nullable<System.Int32> MaxRedirects { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Nullable<System.Int32>); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public bool PreAuthenticate { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(bool); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public int ReadWriteTimeout { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(int); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public int Timeout { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(int); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string UserAgent { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public bool UseSynchronizationContext { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(bool); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public void AddHandler(string contentType, RestSharp.Deserializers.IDeserializer deserializer) { }
    public System.Uri BuildUri(RestSharp.IRestRequest request) { return default(System.Uri); }
    public void ClearHandlers() { }
    public virtual RestSharp.RestRequestAsyncHandle ExecuteAsync(RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse, RestSharp.RestRequestAsyncHandle> callback) { return default(RestSharp.RestRequestAsyncHandle); }
    public virtual RestSharp.RestRequestAsyncHandle ExecuteAsync<T>(RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse<T>, RestSharp.RestRequestAsyncHandle> callback) { return default(RestSharp.RestRequestAsyncHandle); }
    public virtual RestSharp.RestRequestAsyncHandle ExecuteAsyncGet(RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse, RestSharp.RestRequestAsyncHandle> callback, string httpMethod) { return default(RestSharp.RestRequestAsyncHandle); }
    public virtual RestSharp.RestRequestAsyncHandle ExecuteAsyncGet<T>(RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse<T>, RestSharp.RestRequestAsyncHandle> callback, string httpMethod) { return default(RestSharp.RestRequestAsyncHandle); }
    public virtual RestSharp.RestRequestAsyncHandle ExecuteAsyncPost(RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse, RestSharp.RestRequestAsyncHandle> callback, string httpMethod) { return default(RestSharp.RestRequestAsyncHandle); }
    public virtual RestSharp.RestRequestAsyncHandle ExecuteAsyncPost<T>(RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse<T>, RestSharp.RestRequestAsyncHandle> callback, string httpMethod) { return default(RestSharp.RestRequestAsyncHandle); }
    public virtual System.Threading.Tasks.Task<RestSharp.IRestResponse> ExecuteGetTaskAsync(RestSharp.IRestRequest request) { return default(System.Threading.Tasks.Task<RestSharp.IRestResponse>); }
    public virtual System.Threading.Tasks.Task<RestSharp.IRestResponse> ExecuteGetTaskAsync(RestSharp.IRestRequest request, System.Threading.CancellationToken token) { return default(System.Threading.Tasks.Task<RestSharp.IRestResponse>); }
    public virtual System.Threading.Tasks.Task<RestSharp.IRestResponse<T>> ExecuteGetTaskAsync<T>(RestSharp.IRestRequest request) { return default(System.Threading.Tasks.Task<RestSharp.IRestResponse<T>>); }
    public virtual System.Threading.Tasks.Task<RestSharp.IRestResponse<T>> ExecuteGetTaskAsync<T>(RestSharp.IRestRequest request, System.Threading.CancellationToken token) { return default(System.Threading.Tasks.Task<RestSharp.IRestResponse<T>>); }
    public virtual System.Threading.Tasks.Task<RestSharp.IRestResponse> ExecutePostTaskAsync(RestSharp.IRestRequest request) { return default(System.Threading.Tasks.Task<RestSharp.IRestResponse>); }
    public virtual System.Threading.Tasks.Task<RestSharp.IRestResponse> ExecutePostTaskAsync(RestSharp.IRestRequest request, System.Threading.CancellationToken token) { return default(System.Threading.Tasks.Task<RestSharp.IRestResponse>); }
    public virtual System.Threading.Tasks.Task<RestSharp.IRestResponse<T>> ExecutePostTaskAsync<T>(RestSharp.IRestRequest request) { return default(System.Threading.Tasks.Task<RestSharp.IRestResponse<T>>); }
    public virtual System.Threading.Tasks.Task<RestSharp.IRestResponse<T>> ExecutePostTaskAsync<T>(RestSharp.IRestRequest request, System.Threading.CancellationToken token) { return default(System.Threading.Tasks.Task<RestSharp.IRestResponse<T>>); }
    public virtual System.Threading.Tasks.Task<RestSharp.IRestResponse> ExecuteTaskAsync(RestSharp.IRestRequest request) { return default(System.Threading.Tasks.Task<RestSharp.IRestResponse>); }
    public virtual System.Threading.Tasks.Task<RestSharp.IRestResponse> ExecuteTaskAsync(RestSharp.IRestRequest request, System.Threading.CancellationToken token) { return default(System.Threading.Tasks.Task<RestSharp.IRestResponse>); }
    public virtual System.Threading.Tasks.Task<RestSharp.IRestResponse<T>> ExecuteTaskAsync<T>(RestSharp.IRestRequest request) { return default(System.Threading.Tasks.Task<RestSharp.IRestResponse<T>>); }
    public virtual System.Threading.Tasks.Task<RestSharp.IRestResponse<T>> ExecuteTaskAsync<T>(RestSharp.IRestRequest request, System.Threading.CancellationToken token) { return default(System.Threading.Tasks.Task<RestSharp.IRestResponse<T>>); }
    public void RemoveHandler(string contentType) { }
  }
  public partial class RestRequest : RestSharp.IRestRequest {
    public RestRequest() { }
    public RestRequest(RestSharp.Method method) { }
    public RestRequest(string resource) { }
    public RestRequest(string resource, RestSharp.Method method) { }
    public RestRequest(System.Uri resource) { }
    public RestRequest(System.Uri resource, RestSharp.Method method) { }
    public bool AlwaysMultipartFormData { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(bool); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public int Attempts { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(int); } }
    public System.Net.ICredentials Credentials { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Net.ICredentials); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string DateFormat { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Collections.Generic.List<RestSharp.FileParameter> Files { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Collections.Generic.List<RestSharp.FileParameter>); } }
    public RestSharp.Serializers.ISerializer JsonSerializer { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(RestSharp.Serializers.ISerializer); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public RestSharp.Method Method { get { return default(RestSharp.Method); } set { } }
    public System.Action<RestSharp.IRestResponse> OnBeforeDeserialization { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Action<RestSharp.IRestResponse>); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Collections.Generic.List<RestSharp.Parameter> Parameters { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Collections.Generic.List<RestSharp.Parameter>); } }
    public int ReadWriteTimeout { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(int); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public RestSharp.DataFormat RequestFormat { get { return default(RestSharp.DataFormat); } set { } }
    public string Resource { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Action<System.IO.Stream> ResponseWriter { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Action<System.IO.Stream>); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string RootElement { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public int Timeout { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(int); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public bool UseDefaultCredentials { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(bool); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public object UserState { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(object); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string XmlNamespace { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public RestSharp.Serializers.ISerializer XmlSerializer { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(RestSharp.Serializers.ISerializer); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public RestSharp.IRestRequest AddBody(object obj) { return default(RestSharp.IRestRequest); }
    public RestSharp.IRestRequest AddBody(object obj, string xmlNamespace) { return default(RestSharp.IRestRequest); }
    public RestSharp.IRestRequest AddCookie(string name, string value) { return default(RestSharp.IRestRequest); }
    public RestSharp.IRestRequest AddFile(string name, System.Action<System.IO.Stream> writer, string fileName) { return default(RestSharp.IRestRequest); }
    public RestSharp.IRestRequest AddFile(string name, System.Action<System.IO.Stream> writer, string fileName, string contentType) { return default(RestSharp.IRestRequest); }
    public RestSharp.IRestRequest AddFile(string name, System.Byte[] bytes, string fileName) { return default(RestSharp.IRestRequest); }
    public RestSharp.IRestRequest AddFile(string name, System.Byte[] bytes, string fileName, string contentType) { return default(RestSharp.IRestRequest); }
    public RestSharp.IRestRequest AddFile(string name, string path) { return default(RestSharp.IRestRequest); }
    public RestSharp.IRestRequest AddParameter(string name, object value, string contentType, ParameterType type) { return default(RestSharp.IRestRequest); }
    public RestSharp.IRestRequest AddHeader(string name, string value) { return default(RestSharp.IRestRequest); }
    public RestSharp.IRestRequest AddJsonBody(object obj) { return default(RestSharp.IRestRequest); }
    public RestSharp.IRestRequest AddObject(object obj) { return default(RestSharp.IRestRequest); }
    public RestSharp.IRestRequest AddObject(object obj, params System.String[] includedProperties) { return default(RestSharp.IRestRequest); }
    public RestSharp.IRestRequest AddParameter(RestSharp.Parameter p) { return default(RestSharp.IRestRequest); }
    public RestSharp.IRestRequest AddParameter(string name, object value) { return default(RestSharp.IRestRequest); }
    public RestSharp.IRestRequest AddParameter(string name, object value, RestSharp.ParameterType type) { return default(RestSharp.IRestRequest); }
    public RestSharp.IRestRequest AddQueryParameter(string name, string value) { return default(RestSharp.IRestRequest); }
    public RestSharp.IRestRequest AddUrlSegment(string name, string value) { return default(RestSharp.IRestRequest); }
    public RestSharp.IRestRequest AddXmlBody(object obj) { return default(RestSharp.IRestRequest); }
    public RestSharp.IRestRequest AddXmlBody(object obj, string xmlNamespace) { return default(RestSharp.IRestRequest); }
    public void IncreaseNumAttempts() { }
  }
  public partial class SimpleAuthenticator : RestSharp.IAuthenticator {
    public SimpleAuthenticator(string usernameKey, string username, string passwordKey, string password) { }
    public void Authenticate(RestSharp.IRestClient client, RestSharp.IRestRequest request) { }
  }
}
namespace RestSharp.Authenticators {
  public partial class OAuth1Authenticator : RestSharp.IAuthenticator {
    public OAuth1Authenticator() { }
    public virtual RestSharp.Authenticators.OAuth.OAuthParameterHandling ParameterHandling { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(RestSharp.Authenticators.OAuth.OAuthParameterHandling); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public virtual string Realm { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public virtual RestSharp.Authenticators.OAuth.OAuthSignatureMethod SignatureMethod { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(RestSharp.Authenticators.OAuth.OAuthSignatureMethod); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public virtual RestSharp.Authenticators.OAuth.OAuthSignatureTreatment SignatureTreatment { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(RestSharp.Authenticators.OAuth.OAuthSignatureTreatment); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public void Authenticate(RestSharp.IRestClient client, RestSharp.IRestRequest request) { }
    public static RestSharp.Authenticators.OAuth1Authenticator ForAccessToken(string consumerKey, string consumerSecret, string token, string tokenSecret) { return default(RestSharp.Authenticators.OAuth1Authenticator); }
    public static RestSharp.Authenticators.OAuth1Authenticator ForAccessToken(string consumerKey, string consumerSecret, string token, string tokenSecret, string verifier) { return default(RestSharp.Authenticators.OAuth1Authenticator); }
    public static RestSharp.Authenticators.OAuth1Authenticator ForAccessTokenRefresh(string consumerKey, string consumerSecret, string token, string tokenSecret, string sessionHandle) { return default(RestSharp.Authenticators.OAuth1Authenticator); }
    public static RestSharp.Authenticators.OAuth1Authenticator ForAccessTokenRefresh(string consumerKey, string consumerSecret, string token, string tokenSecret, string verifier, string sessionHandle) { return default(RestSharp.Authenticators.OAuth1Authenticator); }
    public static RestSharp.Authenticators.OAuth1Authenticator ForClientAuthentication(string consumerKey, string consumerSecret, string username, string password) { return default(RestSharp.Authenticators.OAuth1Authenticator); }
    public static RestSharp.Authenticators.OAuth1Authenticator ForProtectedResource(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret) { return default(RestSharp.Authenticators.OAuth1Authenticator); }
    public static RestSharp.Authenticators.OAuth1Authenticator ForRequestToken(string consumerKey, string consumerSecret) { return default(RestSharp.Authenticators.OAuth1Authenticator); }
    public static RestSharp.Authenticators.OAuth1Authenticator ForRequestToken(string consumerKey, string consumerSecret, string callbackUrl) { return default(RestSharp.Authenticators.OAuth1Authenticator); }
  }
}
namespace RestSharp.Authenticators.OAuth {
  public enum OAuthParameterHandling {
    HttpAuthorizationHeader = 0,
    UrlOrPostParameters = 1,
  }
  public enum OAuthSignatureMethod {
    HmacSha1 = 0,
    PlainText = 1,
    RsaSha1 = 2,
  }
  public enum OAuthSignatureTreatment {
    Escaped = 0,
    Unescaped = 1,
  }
  public enum OAuthType {
    AccessToken = 1,
    ClientAuthentication = 3,
    ProtectedResource = 2,
    RequestToken = 0,
  }
  public partial class OAuthWebQueryInfo {
    public OAuthWebQueryInfo() { }
    public virtual string Callback { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public virtual string ClientMode { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public virtual string ClientPassword { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public virtual string ClientUsername { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public virtual string ConsumerKey { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public virtual string Nonce { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public virtual RestSharp.Authenticators.OAuth.OAuthParameterHandling ParameterHandling { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(RestSharp.Authenticators.OAuth.OAuthParameterHandling); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public virtual string Signature { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public virtual string SignatureMethod { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public virtual RestSharp.Authenticators.OAuth.OAuthSignatureTreatment SignatureTreatment { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(RestSharp.Authenticators.OAuth.OAuthSignatureTreatment); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public virtual string Timestamp { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public virtual string Token { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public virtual string UserAgent { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public virtual string Verifier { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public virtual string Version { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public virtual string WebMethod { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
  }
}
namespace RestSharp.Deserializers {
  public partial class JsonDeserializer : RestSharp.Deserializers.IDeserializer {
    public JsonDeserializer() { }
    public System.Globalization.CultureInfo Culture { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Globalization.CultureInfo); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string DateFormat { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Namespace { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string RootElement { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public T Deserialize<T>(RestSharp.IRestResponse response) { return default(T); }
  }
  public partial class XmlDeserializer : RestSharp.Deserializers.IDeserializer {
    public XmlDeserializer() { }
    public System.Globalization.CultureInfo Culture { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Globalization.CultureInfo); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string DateFormat { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Namespace { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string RootElement { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    protected virtual object CreateAndMap(System.Type t, System.Xml.Linq.XElement element) { return default(object); }
    public virtual T Deserialize<T>(RestSharp.IRestResponse response) { return default(T); }
    protected virtual System.Xml.Linq.XAttribute GetAttributeByName(System.Xml.Linq.XElement root, System.Xml.Linq.XName name) { return default(System.Xml.Linq.XAttribute); }
    protected virtual System.Xml.Linq.XElement GetElementByName(System.Xml.Linq.XElement root, System.Xml.Linq.XName name) { return default(System.Xml.Linq.XElement); }
    protected virtual object GetValueFromXml(System.Xml.Linq.XElement root, System.Xml.Linq.XName name, System.Reflection.PropertyInfo prop) { return default(object); }
    protected virtual object Map(object x, System.Xml.Linq.XElement root) { return default(object); }
  }
}
namespace RestSharp.Extensions {
  public static partial class MiscExtensions {
    public static string AsString(this System.Byte[] buffer) { return default(string); }
    public static void CopyTo(this System.IO.Stream input, System.IO.Stream output) { }
    public static System.Byte[] ReadAsBytes(this System.IO.Stream input) { return default(System.Byte[]); }
  }
  public static partial class ReflectionExtensions {
    public static object ChangeType(this object source, System.Type newType) { return default(object); }
    public static object ChangeType(this object source, System.Type newType, System.Globalization.CultureInfo culture) { return default(object); }
    public static object FindEnumValue(this System.Type type, string value, System.Globalization.CultureInfo culture) { return default(object); }
    public static T GetAttribute<T>(this System.Reflection.MemberInfo prop) where T : System.Attribute { return default(T); }
    public static T GetAttribute<T>(this System.Type type) where T : System.Attribute { return default(T); }
    public static bool IsSubclassOfRawGeneric(this System.Type toCheck, System.Type generic) { return default(bool); }
  }
  public static partial class ResponseStatusExtensions {
    public static System.Net.WebException ToWebException(this RestSharp.ResponseStatus responseStatus) { return default(System.Net.WebException); }
  }
  public static partial class StringExtensions {
    public static string AddDashes(this string pascalCasedWord) { return default(string); }
    public static string AddSpaces(this string pascalCasedWord) { return default(string); }
    public static string AddUnderscorePrefix(this string pascalCasedWord) { return default(string); }
    public static string AddUnderscores(this string pascalCasedWord) { return default(string); }
    public static System.Collections.Generic.IEnumerable<System.String> GetNameVariants(this string name, System.Globalization.CultureInfo culture) { return default(System.Collections.Generic.IEnumerable<System.String>); }
    public static bool HasValue(this string input) { return default(bool); }
    public static string HtmlDecode(this string input) { return default(string); }
    public static string HtmlEncode(this string input) { return default(string); }
    public static bool IsUpperCase(this string inputString) { return default(bool); }
    public static string MakeInitialLowerCase(this string word) { return default(string); }
    public static bool Matches(this string input, string pattern) { return default(bool); }
    public static System.DateTime ParseJsonDate(this string input, System.Globalization.CultureInfo culture) { return default(System.DateTime); }
    public static string RemoveSurroundingQuotes(this string input) { return default(string); }
    public static string RemoveUnderscoresAndDashes(this string input) { return default(string); }
    public static string ToCamelCase(this string lowercaseAndUnderscoredWord, System.Globalization.CultureInfo culture) { return default(string); }
    public static string ToPascalCase(this string text, bool removeUnderscores, System.Globalization.CultureInfo culture) { return default(string); }
    public static string ToPascalCase(this string lowercaseAndUnderscoredWord, System.Globalization.CultureInfo culture) { return default(string); }
    public static string UrlDecode(this string input) { return default(string); }
    public static string UrlEncode(this string input) { return default(string); }
  }
  public static partial class XmlExtensions {
    public static System.Xml.Linq.XName AsNamespaced(this string name, string @namespace) { return default(System.Xml.Linq.XName); }
  }
}
namespace RestSharp.Serializers {
  public partial class XmlSerializer : RestSharp.Serializers.ISerializer {
    public XmlSerializer() { }
    public XmlSerializer(string @namespace) { }
    public string ContentType { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string DateFormat { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Namespace { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string RootElement { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Serialize(object obj) { return default(string); }
  }
}
