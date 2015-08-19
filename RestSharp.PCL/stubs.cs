namespace RestSharp {
  public enum DataFormat {
    Json = 0,
    Xml = 1,
  }
  [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, Size=1)]
  public partial struct DateFormat {
    public const string Iso8601 = "s";
    public const string RoundTrip = "u";
  }
  public partial class FileParameter {
    public FileParameter() { }
    public long ContentLength { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(long); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string ContentType { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string FileName { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Name { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Action<System.IO.Stream> Writer { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Action<System.IO.Stream>); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public static RestSharp.FileParameter Create(string name, System.Byte[] data, string filename) { return default(RestSharp.FileParameter); }
    public static RestSharp.FileParameter Create(string name, System.Byte[] data, string filename, string contentType) { return default(RestSharp.FileParameter); }
  }
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
  public partial class HttpCookie {
    public HttpCookie() { }
    public string Comment { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Uri CommentUri { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Uri); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public bool Discard { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(bool); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Domain { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public bool Expired { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(bool); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.DateTime Expires { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.DateTime); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public bool HttpOnly { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(bool); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Name { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Path { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Port { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public bool Secure { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(bool); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.DateTime TimeStamp { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.DateTime); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Value { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public int Version { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(int); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
  }
  public partial class HttpFile {
    public HttpFile() { }
    public long ContentLength { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(long); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string ContentType { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string FileName { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Name { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Action<System.IO.Stream> Writer { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Action<System.IO.Stream>); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
  }
  public partial class HttpHeader {
    public HttpHeader() { }
    public string Name { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Value { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
  }
  public partial class HttpParameter {
    public HttpParameter() { }
    public string Name { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Value { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
  }
  public partial class HttpResponse : RestSharp.IHttpResponse {
    public HttpResponse() { }
    public string Content { get { return default(string); } }
    public string ContentEncoding { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public long ContentLength { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(long); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string ContentType { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Collections.Generic.IList<RestSharp.HttpCookie> Cookies { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Collections.Generic.IList<RestSharp.HttpCookie>); } }
    public System.Exception ErrorException { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Exception); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string ErrorMessage { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Collections.Generic.IList<RestSharp.HttpHeader> Headers { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Collections.Generic.IList<RestSharp.HttpHeader>); } }
    public System.Byte[] RawBytes { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Byte[]); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public RestSharp.ResponseStatus ResponseStatus { get { return default(RestSharp.ResponseStatus); } set { } }
    public System.Uri ResponseUri { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Uri); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Server { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Net.HttpStatusCode StatusCode { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Net.HttpStatusCode); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string StatusDescription { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
  }
  public partial interface IAuthenticator {
    void Authenticate(RestSharp.IRestClient client, RestSharp.IRestRequest request);
  }
  public partial interface IHttp {
    bool AlwaysMultipartFormData { get; set; }
    System.Net.CookieContainer CookieContainer { get; set; }
    System.Collections.Generic.IList<RestSharp.HttpCookie> Cookies { get; }
    System.Net.ICredentials Credentials { get; set; }
    System.Text.Encoding Encoding { get; set; }
    System.Collections.Generic.IList<RestSharp.HttpFile> Files { get; }
    bool FollowRedirects { get; set; }
    System.Collections.Generic.IList<RestSharp.HttpHeader> Headers { get; }
    System.Collections.Generic.IList<RestSharp.HttpParameter> Parameters { get; }
    bool PreAuthenticate { get; set; }
    int ReadWriteTimeout { get; set; }
    string RequestBody { get; set; }
    System.Byte[] RequestBodyBytes { get; set; }
    string RequestContentType { get; set; }
    System.Action<System.IO.Stream> ResponseWriter { get; set; }
    int Timeout { get; set; }
    System.Uri Url { get; set; }
    bool UseDefaultCredentials { get; set; }
    string UserAgent { get; set; }
    System.Net.HttpWebRequest AsGetAsync(System.Action<RestSharp.HttpResponse> action, string httpMethod);
    System.Net.HttpWebRequest AsPostAsync(System.Action<RestSharp.HttpResponse> action, string httpMethod);
    System.Net.HttpWebRequest DeleteAsync(System.Action<RestSharp.HttpResponse> action);
    System.Net.HttpWebRequest GetAsync(System.Action<RestSharp.HttpResponse> action);
    System.Net.HttpWebRequest HeadAsync(System.Action<RestSharp.HttpResponse> action);
    System.Net.HttpWebRequest MergeAsync(System.Action<RestSharp.HttpResponse> action);
    System.Net.HttpWebRequest OptionsAsync(System.Action<RestSharp.HttpResponse> action);
    System.Net.HttpWebRequest PatchAsync(System.Action<RestSharp.HttpResponse> action);
    System.Net.HttpWebRequest PostAsync(System.Action<RestSharp.HttpResponse> action);
    System.Net.HttpWebRequest PutAsync(System.Action<RestSharp.HttpResponse> action);
  }
  public partial interface IHttpFactory {
    RestSharp.IHttp Create();
  }
  public partial interface IHttpResponse {
    string Content { get; }
    string ContentEncoding { get; set; }
    long ContentLength { get; set; }
    string ContentType { get; set; }
    System.Collections.Generic.IList<RestSharp.HttpCookie> Cookies { get; }
    System.Exception ErrorException { get; set; }
    string ErrorMessage { get; set; }
    System.Collections.Generic.IList<RestSharp.HttpHeader> Headers { get; }
    System.Byte[] RawBytes { get; set; }
    RestSharp.ResponseStatus ResponseStatus { get; set; }
    System.Uri ResponseUri { get; set; }
    string Server { get; set; }
    System.Net.HttpStatusCode StatusCode { get; set; }
    string StatusDescription { get; set; }
  }
  [System.CodeDom.Compiler.GeneratedCodeAttribute("simple-json", "1.0.0")]
  public partial interface IJsonSerializerStrategy {
    object DeserializeObject(object value, System.Type type);
    bool TrySerializeNonPrimitiveObject(object input, out object output);
  }
  public partial interface IRestClient {
    RestSharp.IAuthenticator Authenticator { get; set; }
    System.Uri BaseUrl { get; set; }
    System.Net.CookieContainer CookieContainer { get; set; }
    System.Collections.Generic.IList<RestSharp.Parameter> DefaultParameters { get; }
    System.Text.Encoding Encoding { get; set; }
    bool PreAuthenticate { get; set; }
    int ReadWriteTimeout { get; set; }
    int Timeout { get; set; }
    string UserAgent { get; set; }
    bool UseSynchronizationContext { get; set; }
    System.Uri BuildUri(RestSharp.IRestRequest request);
    RestSharp.RestRequestAsyncHandle ExecuteAsync(RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse, RestSharp.RestRequestAsyncHandle> callback);
    RestSharp.RestRequestAsyncHandle ExecuteAsync<T>(RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse<T>, RestSharp.RestRequestAsyncHandle> callback);
    RestSharp.RestRequestAsyncHandle ExecuteAsyncGet(RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse, RestSharp.RestRequestAsyncHandle> callback, string httpMethod);
    RestSharp.RestRequestAsyncHandle ExecuteAsyncGet<T>(RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse<T>, RestSharp.RestRequestAsyncHandle> callback, string httpMethod);
    RestSharp.RestRequestAsyncHandle ExecuteAsyncPost(RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse, RestSharp.RestRequestAsyncHandle> callback, string httpMethod);
    RestSharp.RestRequestAsyncHandle ExecuteAsyncPost<T>(RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse<T>, RestSharp.RestRequestAsyncHandle> callback, string httpMethod);
    System.Threading.Tasks.Task<RestSharp.IRestResponse> ExecuteGetTaskAsync(RestSharp.IRestRequest request);
    System.Threading.Tasks.Task<RestSharp.IRestResponse> ExecuteGetTaskAsync(RestSharp.IRestRequest request, System.Threading.CancellationToken token);
    System.Threading.Tasks.Task<RestSharp.IRestResponse<T>> ExecuteGetTaskAsync<T>(RestSharp.IRestRequest request);
    System.Threading.Tasks.Task<RestSharp.IRestResponse<T>> ExecuteGetTaskAsync<T>(RestSharp.IRestRequest request, System.Threading.CancellationToken token);
    System.Threading.Tasks.Task<RestSharp.IRestResponse> ExecutePostTaskAsync(RestSharp.IRestRequest request);
    System.Threading.Tasks.Task<RestSharp.IRestResponse> ExecutePostTaskAsync(RestSharp.IRestRequest request, System.Threading.CancellationToken token);
    System.Threading.Tasks.Task<RestSharp.IRestResponse<T>> ExecutePostTaskAsync<T>(RestSharp.IRestRequest request);
    System.Threading.Tasks.Task<RestSharp.IRestResponse<T>> ExecutePostTaskAsync<T>(RestSharp.IRestRequest request, System.Threading.CancellationToken token);
    System.Threading.Tasks.Task<RestSharp.IRestResponse> ExecuteTaskAsync(RestSharp.IRestRequest request);
    System.Threading.Tasks.Task<RestSharp.IRestResponse> ExecuteTaskAsync(RestSharp.IRestRequest request, System.Threading.CancellationToken token);
    System.Threading.Tasks.Task<RestSharp.IRestResponse<T>> ExecuteTaskAsync<T>(RestSharp.IRestRequest request);
    System.Threading.Tasks.Task<RestSharp.IRestResponse<T>> ExecuteTaskAsync<T>(RestSharp.IRestRequest request, System.Threading.CancellationToken token);
  }
  public partial interface IRestRequest {
    bool AlwaysMultipartFormData { get; set; }
    int Attempts { get; }
    System.Net.ICredentials Credentials { get; set; }
    string DateFormat { get; set; }
    System.Collections.Generic.List<RestSharp.FileParameter> Files { get; }
    RestSharp.Serializers.ISerializer JsonSerializer { get; set; }
    RestSharp.Method Method { get; set; }
    System.Action<RestSharp.IRestResponse> OnBeforeDeserialization { get; set; }
    System.Collections.Generic.List<RestSharp.Parameter> Parameters { get; }
    int ReadWriteTimeout { get; set; }
    RestSharp.DataFormat RequestFormat { get; set; }
    string Resource { get; set; }
    System.Action<System.IO.Stream> ResponseWriter { get; set; }
    string RootElement { get; set; }
    int Timeout { get; set; }
    bool UseDefaultCredentials { get; set; }
    string XmlNamespace { get; set; }
    RestSharp.Serializers.ISerializer XmlSerializer { get; set; }
    RestSharp.IRestRequest AddBody(object obj);
    RestSharp.IRestRequest AddBody(object obj, string xmlNamespace);
    RestSharp.IRestRequest AddCookie(string name, string value);
    RestSharp.IRestRequest AddHeader(string name, string value);
    RestSharp.IRestRequest AddJsonBody(object obj);
    RestSharp.IRestRequest AddObject(object obj);
    RestSharp.IRestRequest AddObject(object obj, params System.String[] includedProperties);
    RestSharp.IRestRequest AddParameter(RestSharp.Parameter p);
    RestSharp.IRestRequest AddParameter(string name, object value);
    RestSharp.IRestRequest AddParameter(string name, object value, RestSharp.ParameterType type);
    RestSharp.IRestRequest AddQueryParameter(string name, string value);
    RestSharp.IRestRequest AddUrlSegment(string name, string value);
    RestSharp.IRestRequest AddXmlBody(object obj);
    RestSharp.IRestRequest AddXmlBody(object obj, string xmlNamespace);
    void IncreaseNumAttempts();
  }
  public partial interface IRestResponse {
    string Content { get; set; }
    string ContentEncoding { get; set; }
    long ContentLength { get; set; }
    string ContentType { get; set; }
    System.Collections.Generic.IList<RestSharp.RestResponseCookie> Cookies { get; }
    System.Exception ErrorException { get; set; }
    string ErrorMessage { get; set; }
    System.Collections.Generic.IList<RestSharp.Parameter> Headers { get; }
    System.Byte[] RawBytes { get; set; }
    RestSharp.IRestRequest Request { get; set; }
    RestSharp.ResponseStatus ResponseStatus { get; set; }
    System.Uri ResponseUri { get; set; }
    string Server { get; set; }
    System.Net.HttpStatusCode StatusCode { get; set; }
    string StatusDescription { get; set; }
  }
  public partial interface IRestResponse<T> : RestSharp.IRestResponse {
    T Data { get; set; }
  }
  [System.CodeDom.Compiler.GeneratedCodeAttribute("simple-json", "1.0.0")]
  [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
  public partial class JsonArray : System.Collections.Generic.List<System.Object> {
    public JsonArray() { }
    public JsonArray(int capacity) { }
    public override string ToString() { return default(string); }
  }
  [System.CodeDom.Compiler.GeneratedCodeAttribute("simple-json", "1.0.0")]
  [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
  public partial class JsonObject : System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<System.String, System.Object>>, System.Collections.Generic.IDictionary<System.String, System.Object>, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.String, System.Object>>, System.Collections.IEnumerable {
    public JsonObject() { }
    public JsonObject(System.Collections.Generic.IEqualityComparer<System.String> comparer) { }
    public int Count { get { return default(int); } }
    public bool IsReadOnly { get { return default(bool); } }
    public object this[int index] { get { return default(object); } }
    public object this[string key] { get { return default(object); } set { } }
    public System.Collections.Generic.ICollection<System.String> Keys { get { return default(System.Collections.Generic.ICollection<System.String>); } }
    public System.Collections.Generic.ICollection<System.Object> Values { get { return default(System.Collections.Generic.ICollection<System.Object>); } }
    public void Add(System.Collections.Generic.KeyValuePair<System.String, System.Object> item) { }
    public void Add(string key, object value) { }
    public void Clear() { }
    public bool Contains(System.Collections.Generic.KeyValuePair<System.String, System.Object> item) { return default(bool); }
    public bool ContainsKey(string key) { return default(bool); }
    public void CopyTo(System.Collections.Generic.KeyValuePair<System.String, System.Object>[] array, int arrayIndex) { }
    public System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<System.String, System.Object>> GetEnumerator() { return default(System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<System.String, System.Object>>); }
    public bool Remove(System.Collections.Generic.KeyValuePair<System.String, System.Object> item) { return default(bool); }
    public bool Remove(string key) { return default(bool); }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return default(System.Collections.IEnumerator); }
    public override string ToString() { return default(string); }
    public bool TryGetValue(string key, out object value) { value = default(object); return default(bool); }
  }
  public enum Method {
    DELETE = 3,
    GET = 0,
    HEAD = 4,
    MERGE = 7,
    OPTIONS = 5,
    PATCH = 6,
    POST = 1,
    PUT = 2,
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
  public partial class Parameter {
    public Parameter() { }
    public string Name { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public RestSharp.ParameterType Type { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(RestSharp.ParameterType); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public object Value { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(object); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public override string ToString() { return default(string); }
  }
  public enum ParameterType {
    Cookie = 0,
    GetOrPost = 1,
    HttpHeader = 3,
    QueryString = 5,
    RequestBody = 4,
    UrlSegment = 2,
  }
  [System.CodeDom.Compiler.GeneratedCodeAttribute("simple-json", "1.0.0")]
  public partial class PocoJsonSerializerStrategy : RestSharp.IJsonSerializerStrategy {
    public PocoJsonSerializerStrategy() { }
    public virtual object DeserializeObject(object value, System.Type type) { return default(object); }
    protected virtual string MapClrMemberNameToJsonFieldName(string clrPropertyName) { return default(string); }
    protected virtual object SerializeEnum(System.Enum p) { return default(object); }
    protected virtual bool TrySerializeKnownTypes(object input, out object output) { output = default(object); return default(bool); }
    public virtual bool TrySerializeNonPrimitiveObject(object input, out object output) { output = default(object); return default(bool); }
    protected virtual bool TrySerializeUnknownTypes(object input, out object output) { output = default(object); return default(bool); }
  }
  public enum ResponseStatus {
    Aborted = 4,
    Completed = 1,
    Error = 2,
    None = 0,
    TimedOut = 3,
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
  public static partial class RestClientExtensions {
    public static void AddDefaultHeader(this RestSharp.IRestClient restClient, string name, string value) { }
    public static void AddDefaultParameter(this RestSharp.IRestClient restClient, RestSharp.Parameter p) { }
    public static void AddDefaultParameter(this RestSharp.IRestClient restClient, string name, object value) { }
    public static void AddDefaultParameter(this RestSharp.IRestClient restClient, string name, object value, RestSharp.ParameterType type) { }
    public static void AddDefaultUrlSegment(this RestSharp.IRestClient restClient, string name, string value) { }
    public static RestSharp.RestRequestAsyncHandle DeleteAsync(this RestSharp.IRestClient client, RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse, RestSharp.RestRequestAsyncHandle> callback) { return default(RestSharp.RestRequestAsyncHandle); }
    public static RestSharp.RestRequestAsyncHandle DeleteAsync<T>(this RestSharp.IRestClient client, RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse<T>, RestSharp.RestRequestAsyncHandle> callback) where T : new() { return default(RestSharp.RestRequestAsyncHandle); }
    public static System.Threading.Tasks.Task<T> DeleteTaskAsync<T>(this RestSharp.IRestClient client, RestSharp.IRestRequest request) where T : new() { return default(System.Threading.Tasks.Task<T>); }
    public static RestSharp.RestRequestAsyncHandle ExecuteAsync(this RestSharp.IRestClient client, RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse> callback) { return default(RestSharp.RestRequestAsyncHandle); }
    public static RestSharp.RestRequestAsyncHandle ExecuteAsync<T>(this RestSharp.IRestClient client, RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse<T>> callback) where T : new() { return default(RestSharp.RestRequestAsyncHandle); }
    public static RestSharp.RestRequestAsyncHandle GetAsync(this RestSharp.IRestClient client, RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse, RestSharp.RestRequestAsyncHandle> callback) { return default(RestSharp.RestRequestAsyncHandle); }
    public static RestSharp.RestRequestAsyncHandle GetAsync<T>(this RestSharp.IRestClient client, RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse<T>, RestSharp.RestRequestAsyncHandle> callback) where T : new() { return default(RestSharp.RestRequestAsyncHandle); }
    public static System.Threading.Tasks.Task<T> GetTaskAsync<T>(this RestSharp.IRestClient client, RestSharp.IRestRequest request) where T : new() { return default(System.Threading.Tasks.Task<T>); }
    public static RestSharp.RestRequestAsyncHandle HeadAsync(this RestSharp.IRestClient client, RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse, RestSharp.RestRequestAsyncHandle> callback) { return default(RestSharp.RestRequestAsyncHandle); }
    public static RestSharp.RestRequestAsyncHandle HeadAsync<T>(this RestSharp.IRestClient client, RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse<T>, RestSharp.RestRequestAsyncHandle> callback) where T : new() { return default(RestSharp.RestRequestAsyncHandle); }
    public static System.Threading.Tasks.Task<T> HeadTaskAsync<T>(this RestSharp.IRestClient client, RestSharp.IRestRequest request) where T : new() { return default(System.Threading.Tasks.Task<T>); }
    public static RestSharp.RestRequestAsyncHandle OptionsAsync(this RestSharp.IRestClient client, RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse, RestSharp.RestRequestAsyncHandle> callback) { return default(RestSharp.RestRequestAsyncHandle); }
    public static RestSharp.RestRequestAsyncHandle OptionsAsync<T>(this RestSharp.IRestClient client, RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse<T>, RestSharp.RestRequestAsyncHandle> callback) where T : new() { return default(RestSharp.RestRequestAsyncHandle); }
    public static System.Threading.Tasks.Task<T> OptionsTaskAsync<T>(this RestSharp.IRestClient client, RestSharp.IRestRequest request) where T : new() { return default(System.Threading.Tasks.Task<T>); }
    public static RestSharp.RestRequestAsyncHandle PatchAsync(this RestSharp.IRestClient client, RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse, RestSharp.RestRequestAsyncHandle> callback) { return default(RestSharp.RestRequestAsyncHandle); }
    public static RestSharp.RestRequestAsyncHandle PatchAsync<T>(this RestSharp.IRestClient client, RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse<T>, RestSharp.RestRequestAsyncHandle> callback) where T : new() { return default(RestSharp.RestRequestAsyncHandle); }
    public static System.Threading.Tasks.Task<T> PatchTaskAsync<T>(this RestSharp.IRestClient client, RestSharp.IRestRequest request) where T : new() { return default(System.Threading.Tasks.Task<T>); }
    public static RestSharp.RestRequestAsyncHandle PostAsync(this RestSharp.IRestClient client, RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse, RestSharp.RestRequestAsyncHandle> callback) { return default(RestSharp.RestRequestAsyncHandle); }
    public static RestSharp.RestRequestAsyncHandle PostAsync<T>(this RestSharp.IRestClient client, RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse<T>, RestSharp.RestRequestAsyncHandle> callback) where T : new() { return default(RestSharp.RestRequestAsyncHandle); }
    public static System.Threading.Tasks.Task<T> PostTaskAsync<T>(this RestSharp.IRestClient client, RestSharp.IRestRequest request) where T : new() { return default(System.Threading.Tasks.Task<T>); }
    public static RestSharp.RestRequestAsyncHandle PutAsync(this RestSharp.IRestClient client, RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse, RestSharp.RestRequestAsyncHandle> callback) { return default(RestSharp.RestRequestAsyncHandle); }
    public static RestSharp.RestRequestAsyncHandle PutAsync<T>(this RestSharp.IRestClient client, RestSharp.IRestRequest request, System.Action<RestSharp.IRestResponse<T>, RestSharp.RestRequestAsyncHandle> callback) where T : new() { return default(RestSharp.RestRequestAsyncHandle); }
    public static System.Threading.Tasks.Task<T> PutTaskAsync<T>(this RestSharp.IRestClient client, RestSharp.IRestRequest request) where T : new() { return default(System.Threading.Tasks.Task<T>); }
    public static void RemoveDefaultParameter(this RestSharp.IRestClient restClient, string name) { }
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
  public partial class RestRequestAsyncHandle {
    public System.Net.HttpWebRequest WebRequest;
    public RestRequestAsyncHandle() { }
    public RestRequestAsyncHandle(System.Net.HttpWebRequest webRequest) { }
    public void Abort() { }
  }
  public partial class RestResponse : RestSharp.RestResponseBase, RestSharp.IRestResponse {
    public RestResponse() { }
  }
  public partial class RestResponse<T> : RestSharp.RestResponseBase, RestSharp.IRestResponse, RestSharp.IRestResponse<T> {
    public RestResponse() { }
    public T Data { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(T); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public static explicit operator RestSharp.RestResponse<T> (RestSharp.RestResponse response) { return default(RestSharp.RestResponse<T>); }
  }
  public abstract partial class RestResponseBase {
    public RestResponseBase() { }
    public string Content { get { return default(string); } set { } }
    public string ContentEncoding { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public long ContentLength { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(long); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string ContentType { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Collections.Generic.IList<RestSharp.RestResponseCookie> Cookies { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Collections.Generic.IList<RestSharp.RestResponseCookie>); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]protected internal set { } }
    public System.Exception ErrorException { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Exception); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string ErrorMessage { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Collections.Generic.IList<RestSharp.Parameter> Headers { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Collections.Generic.IList<RestSharp.Parameter>); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]protected internal set { } }
    public System.Byte[] RawBytes { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Byte[]); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public RestSharp.IRestRequest Request { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(RestSharp.IRestRequest); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public RestSharp.ResponseStatus ResponseStatus { get { return default(RestSharp.ResponseStatus); } set { } }
    public System.Uri ResponseUri { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Uri); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Server { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Net.HttpStatusCode StatusCode { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Net.HttpStatusCode); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string StatusDescription { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
  }
  public partial class RestResponseCookie {
    public RestResponseCookie() { }
    public string Comment { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Uri CommentUri { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Uri); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public bool Discard { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(bool); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Domain { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public bool Expired { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(bool); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.DateTime Expires { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.DateTime); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public bool HttpOnly { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(bool); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Name { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Path { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Port { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public bool Secure { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(bool); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.DateTime TimeStamp { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.DateTime); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Value { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public int Version { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(int); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
  }
  public partial class SimpleAuthenticator : RestSharp.IAuthenticator {
    public SimpleAuthenticator(string usernameKey, string username, string passwordKey, string password) { }
    public void Authenticate(RestSharp.IRestClient client, RestSharp.IRestRequest request) { }
  }
  public partial class SimpleFactory<T> : RestSharp.IHttpFactory where T : RestSharp.IHttp, new() {
    public SimpleFactory() { }
    public RestSharp.IHttp Create() { return default(RestSharp.IHttp); }
  }
  [System.CodeDom.Compiler.GeneratedCodeAttribute("simple-json", "1.0.0")]
  public static partial class SimpleJson {
    public static RestSharp.IJsonSerializerStrategy CurrentJsonSerializerStrategy { get { return default(RestSharp.IJsonSerializerStrategy); } set { } }
    [System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(2))]
    public static RestSharp.PocoJsonSerializerStrategy PocoJsonSerializerStrategy { get { return default(RestSharp.PocoJsonSerializerStrategy); } }
    public static object DeserializeObject(string json) { return default(object); }
    public static object DeserializeObject(string json, System.Type type) { return default(object); }
    public static object DeserializeObject(string json, System.Type type, RestSharp.IJsonSerializerStrategy jsonSerializerStrategy) { return default(object); }
    public static T DeserializeObject<T>(string json) { return default(T); }
    public static T DeserializeObject<T>(string json, RestSharp.IJsonSerializerStrategy jsonSerializerStrategy) { return default(T); }
    public static string EscapeToJavascriptString(string jsonString) { return default(string); }
    public static string SerializeObject(object json) { return default(string); }
    public static string SerializeObject(object json, RestSharp.IJsonSerializerStrategy jsonSerializerStrategy) { return default(string); }
    public static bool TryDeserializeObject(string json, out object obj) { obj = default(object); return default(bool); }
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
  [System.AttributeUsageAttribute((System.AttributeTargets)(132), Inherited=false, AllowMultiple=false)]
  public sealed partial class DeserializeAsAttribute : System.Attribute {
    public DeserializeAsAttribute() { }
    public bool Attribute { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(bool); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Name { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
  }
  public partial class DotNetXmlDeserializer : RestSharp.Deserializers.IDeserializer {
    public DotNetXmlDeserializer() { }
    public string DateFormat { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Namespace { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string RootElement { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public T Deserialize<T>(RestSharp.IRestResponse response) { return default(T); }
  }
  public partial interface IDeserializer {
    string DateFormat { get; set; }
    string Namespace { get; set; }
    string RootElement { get; set; }
    T Deserialize<T>(RestSharp.IRestResponse response);
  }
  public partial class JsonDeserializer : RestSharp.Deserializers.IDeserializer {
    public JsonDeserializer() { }
    public System.Globalization.CultureInfo Culture { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Globalization.CultureInfo); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string DateFormat { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Namespace { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string RootElement { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public T Deserialize<T>(RestSharp.IRestResponse response) { return default(T); }
  }
  public partial class XmlAttributeDeserializer : RestSharp.Deserializers.XmlDeserializer {
    public XmlAttributeDeserializer() { }
    protected override object GetValueFromXml(System.Xml.Linq.XElement root, System.Xml.Linq.XName name, System.Reflection.PropertyInfo prop) { return default(object); }
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
  public static partial class ResponseExtensions {
    public static RestSharp.IRestResponse<T> toAsyncResponse<T>(this RestSharp.IRestResponse response) { return default(RestSharp.IRestResponse<T>); }
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
  public partial class DotNetXmlSerializer : RestSharp.Serializers.ISerializer {
    public DotNetXmlSerializer() { }
    public DotNetXmlSerializer(string @namespace) { }
    public string ContentType { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string DateFormat { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Text.Encoding Encoding { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Text.Encoding); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Namespace { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string RootElement { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Serialize(object obj) { return default(string); }
  }
  public partial interface ISerializer {
    string ContentType { get; set; }
    string DateFormat { get; set; }
    string Namespace { get; set; }
    string RootElement { get; set; }
    string Serialize(object obj);
  }
  public partial class JsonSerializer : RestSharp.Serializers.ISerializer {
    public JsonSerializer() { }
    public string ContentType { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string DateFormat { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Namespace { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string RootElement { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Serialize(object obj) { return default(string); }
  }
  public enum NameStyle {
    AsIs = 0,
    CamelCase = 1,
    LowerCase = 2,
    PascalCase = 3,
  }
  [System.AttributeUsageAttribute((System.AttributeTargets)(132), Inherited=false, AllowMultiple=false)]
  public sealed partial class SerializeAsAttribute : System.Attribute {
    public SerializeAsAttribute() { }
    public bool Attribute { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(bool); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public System.Globalization.CultureInfo Culture { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Globalization.CultureInfo); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public int Index { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(int); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string Name { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public RestSharp.Serializers.NameStyle NameStyle { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(RestSharp.Serializers.NameStyle); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    public string TransformName(string input) { return default(string); }
  }
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
namespace RestSharp.Validation {
  public partial class Require {
    public Require() { }
    public static void Argument(string argumentName, object argumentValue) { }
  }
  public partial class Validate {
    public Validate() { }
    public static void IsBetween(int value, int min, int max) { }
    public static void IsValidLength(string value, int maxSize) { }
  }
}
