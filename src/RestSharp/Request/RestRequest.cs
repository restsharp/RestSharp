//  Copyright (c) .NET Foundation and Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Net;
using RestSharp.Extensions;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace RestSharp;

/// <summary>
/// Container for data used to make requests
/// </summary>
public class RestRequest {
    readonly Func<HttpResponseMessage, RestResponse>? _advancedResponseHandler;
    readonly Func<Stream, Stream?>?                   _responseWriter;

    /// <summary>
    /// Default constructor
    /// </summary>
    public RestRequest() => Method = Method.Get;

    /// <summary>
    /// Constructor for a rest request to a relative resource URL and optional method
    /// </summary>
    /// <param name="resource">Resource to use</param>
    /// <param name="method">Method to use (defaults to Method.Get></param>
    public RestRequest(string? resource, Method method = Method.Get) : this() {
        Resource = resource ?? "";
        Method   = method;

        if (string.IsNullOrWhiteSpace(resource)) return;

        var queryStringStart = Resource.IndexOf('?');

        if (queryStringStart >= 0 && Resource.IndexOf('=') > queryStringStart) {
            var queryParams = ParseQuery(Resource.Substring(queryStringStart + 1));
            Resource = Resource.Substring(0, queryStringStart);

            foreach (var param in queryParams)
                this.AddQueryParameter(param.Key, param.Value, false);
        }

        static IEnumerable<KeyValuePair<string, string>> ParseQuery(string query)
            => query.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(
                    x => {
                        var position = x.IndexOf('=');

                        return position > 0
                            ? new KeyValuePair<string, string>(x.Substring(0, position), x.Substring(position + 1))
                            : new KeyValuePair<string, string>(x, string.Empty);
                    }
                );
    }

    /// <summary>
    /// Constructor for a rest request to a specific resource Uri and optional method
    /// </summary>
    /// <param name="resource">Resource Uri to use</param>
    /// <param name="method">Method to use (defaults to Method.Get></param>
    public RestRequest(Uri resource, Method method = Method.Get)
        : this(resource.IsAbsoluteUri ? resource.AbsoluteUri : resource.OriginalString, method) { }

    readonly List<FileParameter> _files = new();

    /// <summary>
    /// Always send a multipart/form-data request - even when no Files are present.
    /// </summary>
    public bool AlwaysMultipartFormData { get; set; }
    
    /// <summary>
    /// When set to true, parameters in a multipart form data requests will be enclosed in
    /// quotation marks. Default is false. Enable it if the remote endpoint requires parameters
    /// to be in quotes (for example, FreshDesk API). 
    /// </summary>
    public bool MultipartFormQuoteParameters { get; set; }
    
    public string? FormBoundary { get; set; }

    /// <summary>
    /// Container of all HTTP parameters to be passed with the request.
    /// See AddParameter() for explanation of the types of parameters that can be passed
    /// </summary>
    public ParametersCollection Parameters { get; } = new();

    /// <summary>
    /// Optional cookie container to use for the request. If not set, cookies are not passed.
    /// </summary>
    public CookieContainer? CookieContainer { get; set; }

    /// <summary>
    /// Container of all the files to be uploaded with the request.
    /// </summary>
    public IReadOnlyCollection<FileParameter> Files => _files.AsReadOnly();

    /// <summary>
    /// Determines what HTTP method to use for this request. Supported methods: GET, POST, PUT, DELETE, HEAD, OPTIONS
    /// Default is GET
    /// </summary>
    public Method Method { get; set; }

    /// <summary>
    /// Custom request timeout
    /// </summary>
    public int Timeout { get; set; }

    /// <summary>
    /// The Resource URL to make the request against.
    /// Tokens are substituted with UrlSegment parameters and match by name.
    /// Should not include the scheme or domain. Do not include leading slash.
    /// Combined with RestClient.BaseUrl to assemble final URL:
    /// {BaseUrl}/{Resource} (BaseUrl is scheme + domain, e.g. http://example.com)
    /// </summary>
    /// <example>
    /// // example for url token replacement
    /// request.Resource = "Products/{ProductId}";
    /// request.AddParameter("ProductId", 123, ParameterType.UrlSegment);
    /// </example>
    public string Resource { get; set; } = "";

    /// <summary>
    /// Serializer to use when writing request bodies.
    /// </summary>
    public DataFormat RequestFormat { get; set; }

    /// <summary>
    /// Used by the default deserializers to determine where to start deserializing from.
    /// Can be used to skip container or root elements that do not have corresponding deserialzation targets.
    /// </summary>
    public string? RootElement { get; set; }

    /// <summary>
    /// When supplied, the function will be called before calling the deserializer
    /// </summary>
    public Action<RestResponse>? OnBeforeDeserialization { get; set; }

    /// <summary>
    /// When supplied, the function will be called before making a request
    /// </summary>
    public Func<HttpRequestMessage, ValueTask>? OnBeforeRequest { get; set; }

    /// <summary>
    /// When supplied, the function will be called after the request is complete
    /// </summary>
    public Func<HttpResponseMessage, ValueTask>? OnAfterRequest { get; set; }

    internal void IncreaseNumAttempts() => Attempts++;

    /// <summary>
    /// How many attempts were made to send this Request
    /// </summary>
    /// <remarks>
    /// This number is incremented each time the RestClient sends the request.
    /// </remarks>
    public int Attempts { get; private set; }

    /// <summary>
    /// Completion option for <seealso cref="HttpClient"/>
    /// </summary>
    public HttpCompletionOption CompletionOption { get; set; } = HttpCompletionOption.ResponseContentRead;

    /// <summary>
    /// Set this to write response to Stream rather than reading into memory.
    /// </summary>
    public Func<Stream, Stream?>? ResponseWriter {
        get => _responseWriter;
        init {
            if (AdvancedResponseWriter != null)
                throw new ArgumentException(
                    "AdvancedResponseWriter is not null. Only one response writer can be used."
                );

            _responseWriter = value;
        }
    }

    /// <summary>
    /// Set this to handle the response stream yourself, based on the response details
    /// </summary>
    public Func<HttpResponseMessage, RestResponse>? AdvancedResponseWriter {
        get => _advancedResponseHandler;
        init {
            if (ResponseWriter != null)
                throw new ArgumentException("ResponseWriter is not null. Only one response writer can be used.");

            _advancedResponseHandler = value;
        }
    }

    /// <summary>
    /// Adds a parameter object to the request parameters
    /// </summary>
    /// <param name="parameter">Parameter to add</param>
    /// <returns></returns>
    public RestRequest AddParameter(Parameter parameter) => this.With(x => x.Parameters.AddParameter(parameter));

    /// <summary>
    /// Removes a parameter object from the request parameters
    /// </summary>
    /// <param name="parameter">Parameter to remove</param>
    public RestRequest RemoveParameter(Parameter parameter) {
        Parameters.RemoveParameter(parameter);
        return this;
    }

    internal RestRequest AddFile(FileParameter file) => this.With(x => x._files.Add(file));
}