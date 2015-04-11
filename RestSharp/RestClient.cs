﻿#region License
//   Copyright 2010 John Sheehan
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using RestSharp.Deserializers;
using RestSharp.Extensions;

namespace RestSharp
{
    using RestSharp.Authenticators;

    /// <summary>
    /// Client to translate RestRequests into Http requests and process response result
    /// </summary>
    public partial class RestClient : IRestClient
    {
        // silverlight friendly way to get current version
#if PocketPC
        static readonly Version version = Assembly.GetExecutingAssembly().GetName().Version;
#else
        private static readonly Version version = new AssemblyName(Assembly.GetExecutingAssembly().FullName).Version;
#endif
        public IHttpFactory HttpFactory = new SimpleFactory<Http>();


        /// <summary>
        /// Maximum number of redirects to follow if FollowRedirects is true
        /// </summary>
        public int? MaxRedirects { get; set; }

#if FRAMEWORK
        /// <summary>
        /// X509CertificateCollection to be sent with request
        /// </summary>
        public X509CertificateCollection ClientCertificates { get; set; }

        /// <summary>
        /// Proxy to use for requests made by this client instance.
        /// Passed on to underlying WebRequest if set.
        /// </summary>
        public IWebProxy Proxy { get; set; }
#endif

        /// <summary>
        /// Default is true. Determine whether or not requests that result in 
        /// HTTP status codes of 3xx should follow returned redirect
        /// </summary>
        public bool FollowRedirects { get; set; }

#if !PocketPC
        /// <summary>
        /// The CookieContainer used for requests made by this client instance
        /// </summary>
        public CookieContainer CookieContainer { get; set; }
#endif

        /// <summary>
        /// UserAgent to use for requests made by this client instance
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Timeout in milliseconds to use for requests made by this client instance
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// The number of milliseconds before the writing or reading times out.
        /// </summary>
        public int ReadWriteTimeout { get; set; }

        /// <summary>
        /// Whether to invoke async callbacks using the SynchronizationContext.Current captured when invoked
        /// </summary>
        public bool UseSynchronizationContext { get; set; }

        /// <summary>
        /// Authenticator to use for requests made by this client instance
        /// </summary>
        public IAuthenticator Authenticator { get; set; }

        /// <summary>
        /// Combined with Request.Resource to construct URL for request
        /// Should include scheme and domain without trailing slash.
        /// </summary>
        /// <example>
        /// client.BaseUrl = new Uri("http://example.com");
        /// </example>
        public virtual Uri BaseUrl { get; set; }

        private Encoding encoding = Encoding.UTF8;

        public Encoding Encoding { get { return this.encoding; } set { this.encoding = value; } }

        public bool PreAuthenticate { get; set; }

        /// <summary>
        /// Default constructor that registers default content handlers
        /// </summary>
        public RestClient()
        {
#if WINDOWS_PHONE
            this.UseSynchronizationContext = true;
#endif
            this.ContentHandlers = new Dictionary<string, IDeserializer>();
            this.AcceptTypes = new List<string>();
            this.DefaultParameters = new List<Parameter>();

            // register default handlers
            this.AddHandler("application/json", new JsonDeserializer());
            this.AddHandler("application/xml", new XmlDeserializer());
            this.AddHandler("text/json", new JsonDeserializer());
            this.AddHandler("text/x-json", new JsonDeserializer());
            this.AddHandler("text/javascript", new JsonDeserializer());
            this.AddHandler("text/xml", new XmlDeserializer());
            this.AddHandler("*", new XmlDeserializer());

            this.FollowRedirects = true;
        }

        /// <summary>
        /// Sets the BaseUrl property for requests made by this client instance
        /// </summary>
        /// <param name="baseUrl"></param>
        public RestClient(Uri baseUrl) : this()
        {
            this.BaseUrl = baseUrl;
        }

        /// <summary>
        /// Sets the BaseUrl property for requests made by this client instance
        /// </summary>
        /// <param name="baseUrl"></param>
        public RestClient(string baseUrl) : this()
        {
            if (String.IsNullOrEmpty(baseUrl))
                throw new ArgumentNullException("baseUrl");

            this.BaseUrl = new Uri(baseUrl);
        }

        private IDictionary<string, IDeserializer> ContentHandlers { get; set; }

        private IList<string> AcceptTypes { get; set; }

        /// <summary>
        /// Parameters included with every request made with this instance of RestClient
        /// If specified in both client and request, the request wins
        /// </summary>
        public IList<Parameter> DefaultParameters { get; private set; }

        /// <summary>
        /// Registers a content handler to process response content
        /// </summary>
        /// <param name="contentType">MIME content type of the response content</param>
        /// <param name="deserializer">Deserializer to use to process content</param>
        public void AddHandler(string contentType, IDeserializer deserializer)
        {
            this.ContentHandlers[contentType] = deserializer;

            if (contentType != "*")
            {
                this.AcceptTypes.Add(contentType);
                // add Accept header based on registered deserializers
                var accepts = string.Join(", ", AcceptTypes.ToArray());

                this.RemoveDefaultParameter("Accept");
                this.AddDefaultParameter("Accept", accepts, ParameterType.HttpHeader);
            }
        }

        /// <summary>
        /// Remove a content handler for the specified MIME content type
        /// </summary>
        /// <param name="contentType">MIME content type to remove</param>
        public void RemoveHandler(string contentType)
        {
            this.ContentHandlers.Remove(contentType);
            this.AcceptTypes.Remove(contentType);
            this.RemoveDefaultParameter("Accept");
        }

        /// <summary>
        /// Remove all content handlers
        /// </summary>
        public void ClearHandlers()
        {
            this.ContentHandlers.Clear();
            this.AcceptTypes.Clear();
            this.RemoveDefaultParameter("Accept");
        }

        /// <summary>
        /// Retrieve the handler for the specified MIME content type
        /// </summary>
        /// <param name="contentType">MIME content type to retrieve</param>
        /// <returns>IDeserializer instance</returns>
        private IDeserializer GetHandler(string contentType)
        {
            if (contentType == null)
                throw new ArgumentNullException("contentType");

            if (string.IsNullOrEmpty(contentType) && this.ContentHandlers.ContainsKey("*"))
            {
                return ContentHandlers["*"];
            }

            var semicolonIndex = contentType.IndexOf(';');

            if (semicolonIndex > -1)
                contentType = contentType.Substring(0, semicolonIndex);

            IDeserializer handler = null;

            if (this.ContentHandlers.ContainsKey(contentType))
            {
                handler = this.ContentHandlers[contentType];
            }
            else if (this.ContentHandlers.ContainsKey("*"))
            {
                handler = this.ContentHandlers["*"];
            }

            return handler;
        }

        private void AuthenticateIfNeeded(RestClient client, IRestRequest request)
        {
            if (Authenticator != null)
            {
                this.Authenticator.Authenticate(client, request);
            }
        }

        /// <summary>
        /// Assembles URL to call based on parameters, method and resource
        /// </summary>
        /// <param name="request">RestRequest to execute</param>
        /// <returns>Assembled System.Uri</returns>
        public Uri BuildUri(IRestRequest request)
        {
            if (this.BaseUrl == null)
                throw new NullReferenceException("RestClient must contain a value for BaseUrl");

            var assembled = request.Resource;
            var urlParms = request.Parameters.Where(p => p.Type == ParameterType.UrlSegment);
            var builder = new UriBuilder(this.BaseUrl);

            foreach (var p in urlParms)
            {
                if (p.Value == null)
                {
                    throw new ArgumentException(
                        string.Format("Cannot build uri when url segment parameter '{0}' value is null.", p.Name),
                        "request");
                }

                if (!string.IsNullOrEmpty(assembled))
                    assembled = assembled.Replace("{" + p.Name + "}", p.Value.ToString().UrlEncode());

                builder.Path = builder.Path.UrlDecode().Replace("{" + p.Name + "}", p.Value.ToString().UrlEncode());
            }

            this.BaseUrl = new Uri(builder.ToString());

            if (!string.IsNullOrEmpty(assembled) && assembled.StartsWith("/"))
            {
                assembled = assembled.Substring(1);
            }

            if (this.BaseUrl != null && !string.IsNullOrEmpty(this.BaseUrl.AbsoluteUri))
            {
                if (!this.BaseUrl.AbsoluteUri.EndsWith("/") && !string.IsNullOrEmpty(assembled))
                    assembled = string.Concat("/", assembled);

                assembled = string.IsNullOrEmpty(assembled)
                    ? this.BaseUrl.AbsoluteUri
                    : string.Format("{0}{1}", this.BaseUrl, assembled);
            }

            IEnumerable<Parameter> parameters;

            if (request.Method != Method.POST && request.Method != Method.PUT && request.Method != Method.PATCH)
            {
                parameters = request.Parameters.Where(
                    p => p.Type == ParameterType.GetOrPost || p.Type == ParameterType.QueryString).ToList();
            }
            else
            {
                parameters = request.Parameters.Where(p => p.Type == ParameterType.QueryString).ToList();
            }

            if (!parameters.Any())
                return new Uri(assembled);

            // build and attach querystring
            var data = EncodeParameters(parameters);
            var separator = assembled.Contains("?") ? "&" : "?";

            assembled = string.Concat(assembled, separator, data);

            return new Uri(assembled);
        }

        private static string EncodeParameters(IEnumerable<Parameter> parameters)
        {
#if !PocketPC
            return string.Join("&", parameters.Select(EncodeParameter).ToArray());
#else
            return string.Join("&", parameters.Select(x => EncodeParameter(x)).ToArray());
#endif
        }

        private static string EncodeParameter(Parameter parameter)
        {
            return parameter.Value == null
                ? string.Concat(parameter.Name.UrlEncode(), "=")
                : string.Concat(parameter.Name.UrlEncode(), "=", parameter.Value.ToString().UrlEncode());
        }

        private void ConfigureHttp(IRestRequest request, IHttp http)
        {
            http.Encoding = this.Encoding;
            http.AlwaysMultipartFormData = request.AlwaysMultipartFormData;
#if !PocketPC
            http.UseDefaultCredentials = request.UseDefaultCredentials;
#endif
            http.ResponseWriter = request.ResponseWriter;
#if !PocketPC
            http.CookieContainer = CookieContainer;
#endif
            // move RestClient.DefaultParameters into Request.Parameters
            foreach (var p in DefaultParameters)
            {
                if (request.Parameters.Any(p2 => p2.Name == p.Name && p2.Type == p.Type))
                {
                    continue;
                }

                request.AddParameter(p);
            }

            // Add Accept header based on registered deserializers if none has been set by the caller.
#if PocketPC
            if (request.Parameters.All(p2 => p2.Name.ToLower() != "accept"))
#else
            if (request.Parameters.All(p2 => p2.Name.ToLowerInvariant() != "accept"))
#endif
            {
                var accepts = string.Join(", ", AcceptTypes.ToArray());
                request.AddParameter("Accept", accepts, ParameterType.HttpHeader);
            }

            http.Url = BuildUri(request);
            http.PreAuthenticate = this.PreAuthenticate;

            var userAgent = UserAgent ?? http.UserAgent;

            http.UserAgent = userAgent.HasValue() ? userAgent : "RestSharp/" + version;

            var timeout = request.Timeout > 0 ? request.Timeout : Timeout;

            if (timeout > 0)
            {
                http.Timeout = timeout;
            }

            var readWriteTimeout = request.ReadWriteTimeout > 0 ? request.ReadWriteTimeout : ReadWriteTimeout;

            if (readWriteTimeout > 0)
            {
                http.ReadWriteTimeout = readWriteTimeout;
            }

#if !SILVERLIGHT
            http.FollowRedirects = FollowRedirects;
#endif

#if FRAMEWORK
            if (ClientCertificates != null)
            {
                http.ClientCertificates = this.ClientCertificates;
            }

            http.MaxRedirects = MaxRedirects;
#endif

            if (request.Credentials != null)
            {
                http.Credentials = request.Credentials;
            }

            var headers = from p in request.Parameters
                          where p.Type == ParameterType.HttpHeader
                          select new HttpHeader
                                 {
                                     Name = p.Name,
                                     Value = Convert.ToString(p.Value)
                                 };

            foreach (var header in headers)
            {
                http.Headers.Add(header);
            }

            var cookies = from p in request.Parameters
                          where p.Type == ParameterType.Cookie
                          select new HttpCookie
                                 {
                                     Name = p.Name,
                                     Value = Convert.ToString(p.Value)
                                 };

            foreach (var cookie in cookies)
            {
                http.Cookies.Add(cookie);
            }

            var @params = from p in request.Parameters
                          where p.Type == ParameterType.GetOrPost
                                && p.Value != null
                          select new HttpParameter
                                 {
                                     Name = p.Name,
                                     Value = Convert.ToString(p.Value)
                                 };

            foreach (var parameter in @params)
            {
                http.Parameters.Add(parameter);
            }

            foreach (var file in request.Files)
            {
                http.Files.Add(new HttpFile
                               {
                                   Name = file.Name,
                                   ContentType = file.ContentType,
                                   Writer = file.Writer,
                                   FileName = file.FileName,
                                   ContentLength = file.ContentLength
                               });
            }

            var body = (from p in request.Parameters
                        where p.Type == ParameterType.RequestBody
                        select p).FirstOrDefault();

            // Only add the body if there aren't any files to make it a multipart form request
            // If there are files, then add the body to the HTTP Parameters
            if (body != null)
            {
                http.RequestContentType = body.Name;

                if (!http.Files.Any())
                {
                    object val = body.Value;

                    if (val is byte[])
                        http.RequestBodyBytes = (byte[])val;
                    else
                        http.RequestBody = Convert.ToString(body.Value);
                }
                else
                {
                    http.Parameters.Add(new HttpParameter
                                        {
                                            Name = body.Name,
                                            Value = Convert.ToString(body.Value)
                                        });
                }
            }
#if FRAMEWORK
            this.ConfigureProxy(http);
#endif
        }

#if FRAMEWORK
        private void ConfigureProxy(IHttp http)
        {
            if (Proxy != null)
            {
                http.Proxy = Proxy;
            }
        }
#endif

        private static RestResponse ConvertToRestResponse(IRestRequest request, HttpResponse httpResponse)
        {
            var restResponse = new RestResponse
                               {
                                   Content = httpResponse.Content,
                                   ContentEncoding = httpResponse.ContentEncoding,
                                   ContentLength = httpResponse.ContentLength,
                                   ContentType = httpResponse.ContentType,
                                   ErrorException = httpResponse.ErrorException,
                                   ErrorMessage = httpResponse.ErrorMessage,
                                   RawBytes = httpResponse.RawBytes,
                                   ResponseStatus = httpResponse.ResponseStatus,
                                   ResponseUri = httpResponse.ResponseUri,
                                   Server = httpResponse.Server,
                                   StatusCode = httpResponse.StatusCode,
                                   StatusDescription = httpResponse.StatusDescription,
                                   Request = request
                               };

            foreach (var header in httpResponse.Headers)
            {
                restResponse.Headers.Add(new Parameter
                                         {
                                             Name = header.Name,
                                             Value = header.Value,
                                             Type = ParameterType.HttpHeader
                                         });
            }

            foreach (var cookie in httpResponse.Cookies)
            {
                restResponse.Cookies.Add(new RestResponseCookie
                                         {
                                             Comment = cookie.Comment,
                                             CommentUri = cookie.CommentUri,
                                             Discard = cookie.Discard,
                                             Domain = cookie.Domain,
                                             Expired = cookie.Expired,
                                             Expires = cookie.Expires,
                                             HttpOnly = cookie.HttpOnly,
                                             Name = cookie.Name,
                                             Path = cookie.Path,
                                             Port = cookie.Port,
                                             Secure = cookie.Secure,
                                             TimeStamp = cookie.TimeStamp,
                                             Value = cookie.Value,
                                             Version = cookie.Version
                                         });
            }

            return restResponse;
        }

        private IRestResponse<T> Deserialize<T>(IRestRequest request, IRestResponse raw)
        {
            request.OnBeforeDeserialization(raw);

            IRestResponse<T> response = new RestResponse<T>();

            try
            {
                response = raw.ToAsyncResponse<T>();
                response.Request = request;

                // Only attempt to deserialize if the request has not errored due
                // to a transport or framework exception.  HTTP errors should attempt to 
                // be deserialized 
                if (response.ErrorException == null)
                {
                    IDeserializer handler = this.GetHandler(raw.ContentType);

                    // Only continue if there is a handler defined else there is no way to deserialize the data.
                    // This can happen when a request returns for example a 404 page instead of the requested JSON/XML resource
                    if (handler != null)
                    {
                        handler.RootElement = request.RootElement;
                        handler.DateFormat = request.DateFormat;
                        handler.Namespace = request.XmlNamespace;

                        response.Data = handler.Deserialize<T>(raw);
                    }
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus = ResponseStatus.Error;
                response.ErrorMessage = ex.Message;
                response.ErrorException = ex;
            }

            return response;
        }
    }
}
