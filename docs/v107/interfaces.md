| `IRestClient` member                                                                            | Where is it now?                   |
|:------------------------------------------------------------------------------------------------|:-----------------------------------|
| `CookieContainer`                                                                               | `RestClient`                       |
| `AutomaticDecompression`                                                                        | `RestClientOptions`, changed type  |
| `MaxRedirects`                                                                                  | `RestClientOptions`                |
| `UserAgent`                                                                                     | `RestClientOptions`                |
| `Timeout`                                                                                       | `RestClientOptions`, `RestRequest` |
| `Authenticator`                                                                                 | `RestClient`                       |
| `BaseUrl`                                                                                       | `RestClientOptions`                |
| `Encoding`                                                                                      | `RestClientOptions`                |
| `ThrowOnDeserializationError`                                                                   | `RestClientOptions`                |
| `FailOnDeserializationError`                                                                    | `RestClientOptions`                |
| `ThrowOnAnyError`                                                                               | `RestClientOptions`                |
| `PreAuthenticate`                                                                               | `RestClientOptions`                |
| `BaseHost`                                                                                      | `RestClientOptions`                |
| `AllowMultipleDefaultParametersWithSameName`                                                    | `RestClientOptions`                |
| `ClientCertificates`                                                                            | `RestClientOptions`                |
| `Proxy`                                                                                         | `RestClientOptions`                |
| `CachePolicy`                                                                                   | `RestClientOptions`, changed type  |
| `FollowRedirects`                                                                               | `RestClientOptions`                |
| `RemoteCertificateValidationCallback`                                                           | `RestClientOptions`                |
| `Pipelined`                                                                                     | Not supported                      |
| `UnsafeAuthenticatedConnectionSharing`                                                          | Not supported                      |
| `ConnectionGroupName`                                                                           | Not supported                      |
| `ReadWriteTimeout`                                                                              | Not supported                      |
| `UseSynchronizationContext`                                                                     | Not supported                      |
| `DefaultParameters`                                                                             | `RestClient`                       |
| `UseSerializer(Func<IRestSerializer> serializerFactory)`                                        | `RestClient`                       |
| `UseSerializer<T>()`                                                                            | `RestClient`                       |
| `Deserialize<T>(IRestResponse response)`                                                        | `RestClient`                       |
| `BuildUri(IRestRequest request)`                                                                | `RestClient`                       |
| `UseUrlEncoder(Func<string, string> encoder)`                                                   | Extension                          |
| `UseQueryEncoder(Func<string, Encoding, string> queryEncoder)`                                  | Extension                          |
| `ExecuteAsync<T>(IRestRequest request, CancellationToken cancellationToken)`                    | `RestClient`                       |
| `ExecuteAsync<T>(IRestRequest request, Method httpMethod, CancellationToken cancellationToken)` | Extension                          |
| `ExecuteAsync(IRestRequest request, Method httpMethod, CancellationToken cancellationToken)`    | Extension                          |
| `ExecuteAsync(IRestRequest request, CancellationToken cancellationToken)`                       | Extension                          |
| `ExecuteGetAsync<T>(IRestRequest request, CancellationToken cancellationToken)`                 | Extension                          |
| `ExecutePostAsync<T>(IRestRequest request, CancellationToken cancellationToken)`                | Extension                          |
| `ExecuteGetAsync(IRestRequest request, CancellationToken cancellationToken)`                    | Extension                          |
| `ExecutePostAsync(IRestRequest request, CancellationToken cancellationToken)`                   | Extension                          |
| `Execute(IRestRequest request)`                                                                 | Deprecated                         |
| `Execute(IRestRequest request, Method httpMethod)`                                              | Deprecated                         |
| `Execute<T>(IRestRequest request)`                                                              | Deprecated                         |
| `Execute<T>(IRestRequest request, Method httpMethod)`                                           | Deprecated                         |
| `DownloadData(IRestRequest request)`                                                            | Deprecated                         |
| `ExecuteAsGet(IRestRequest request, string httpMethod)`                                         | Deprecated                         |
| `ExecuteAsPost(IRestRequest request, string httpMethod)`                                        | Deprecated                         |
| `ExecuteAsGet<T>(IRestRequest request, string httpMethod)`                                      | Deprecated                         |
| `ExecuteAsPost<T>(IRestRequest request, string httpMethod)`                                     | Deprecated                         |
| `BuildUriWithoutQueryParameters(IRestRequest request)`                                          | Removed                            |
| `ConfigureWebRequest(Action<HttpWebRequest> configurator)`                                      | Removed                            |
| `AddHandler(string contentType, Func<IDeserializer> deserializerFactory)`                       | Removed                            |
| `RemoveHandler(string contentType)`                                                             | Removed                            |
| `ClearHandlers()`                                                                               | Removed                            |

| `IRestRequest` member                                                                                  | Where is it now?                 |
|:-------------------------------------------------------------------------------------------------------|:---------------------------------|
| `AlwaysMultipartFormData`                                                                              | `RestRequest`                    |
| `JsonSerializer`                                                                                       | Deprecated                       |
| `XmlSerializer`                                                                                        | Deprecated                       |
| `AdvancedResponseWriter`                                                                               | `RestRequest`, changed signature |
| `ResponseWriter`                                                                                       | `RestRequest`, changed signature |
| `Parameters`                                                                                           | `RestRequest`                    |
| `Files`                                                                                                | `RestRequest`                    |
| `Method`                                                                                               | `RestRequest`                    |
| `Resource`                                                                                             | `RestRequest`                    |
| `RequestFormat`                                                                                        | `RestRequest`                    |
| `RootElement`                                                                                          | `RestRequest`                    |
| `DateFormat`                                                                                           | `XmlRequest`                     |
| `XmlNamespace`                                                                                         | `XmlRequest`                     |
| `Credentials`                                                                                          | Removed, use `RestClientOptions` |
| `Timeout`                                                                                              | `RestRequest`                    |
| `ReadWriteTimeout`                                                                                     | Not supported                    |
| `Attempts`                                                                                             | `RestRequest`                    |
| `UseDefaultCredentials`                                                                                | Removed, use `RestClientOptions` |
| `AllowedDecompressionMethods`                                                                          | Removed, use `RestClientOptions` |
| `OnBeforeDeserialization`                                                                              | `RestRequest`                    |
| `OnBeforeRequest`                                                                                      | `RestRequest`, changed signature |
| `Body`                                                                                                 | Removed, use `Parameters`        |
| `AddParameter(Parameter p)`                                                                            | `RestRequest`                    |
| `AddFile(string name, string path, string contentType)`                                                | Extension                        |
| `AddFile(string name, byte[] bytes, string fileName, string contentType)`                              | Extension                        |
| `AddFile(string name, Action<Stream> writer, string fileName, long contentLength, string contentType)` | Extension                        |
| `AddFileBytes(string name, byte[] bytes, string filename, string contentType)`                         | Extension `AddFile`              |
| `AddBody(object obj, string xmlNamespace)`                                                             | Deprecated                       |
| `AddBody(object obj)`                                                                                  | Extension                        |
| `AddJsonBody(object obj)`                                                                              | Extension                        |
| `AddJsonBody(object obj, string contentType)`                                                          | Extension                        |
| `AddXmlBody(object obj)`                                                                               | Extension                        |
| `AddXmlBody(object obj, string xmlNamespace)`                                                          | Extension                        |
| `AddObject(object obj, params string[] includedProperties)`                                            | Extension                        |
| `AddObject(object obj)`                                                                                | Extension                        |
| `AddParameter(string name, object value)`                                                              | Extension                        |
| `AddParameter(string name, object value, ParameterType type)`                                          | Extension                        |
| `AddParameter(string name, object value, string contentType, ParameterType type)`                      | Extension                        |
| `AddOrUpdateParameter(Parameter parameter)`                                                            | Extension                        |
| `AddOrUpdateParameters(IEnumerable<Parameter> parameters)`                                             | Extension                        |
| `AddOrUpdateParameter(string name, object value)`                                                      | Extension                        |
| `AddOrUpdateParameter(string name, object value, ParameterType type)`                                  | Extension                        |
| `AddOrUpdateParameter(string name, object value, string contentType, ParameterType type)`              | Extension                        |
| `AddHeader(string name, string value)`                                                                 | Extension                        |
| `AddOrUpdateHeader(string name, string value)`                                                         | Extension                        |
| `AddHeaders(ICollection<KeyValuePair<string, string>> headers)`                                        | Extension                        |
| `AddOrUpdateHeaders(ICollection<KeyValuePair<string, string>> headers)`                                | Extension                        |
| `AddCookie(string name, string value)`                                                                 | Extension                        |
| `AddUrlSegment(string name, string value)`                                                             | Extension                        |
| `AddUrlSegment(string name, string value, bool encode)`                                                | Extension                        |
| `AddUrlSegment(string name, object value)`                                                             | Extension                        |
| `AddQueryParameter(string name, string value)`                                                         | Extension                        |
| `AddQueryParameter(string name, string value, bool encode)`                                            | Extension                        |
| `AddDecompressionMethod(DecompressionMethods decompressionMethod)`                                     | Not supported                    |
| `IncreaseNumAttempts()`                                                                                | Made internal                    |