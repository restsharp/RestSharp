using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp.Extensions;

namespace RestSharp
{
    public class HttpConverter : IHttpConverter
    {
        public IHttpRequest ConvertTo(IRestClient restClient, IRestRequest request)
        {
            var http = new HttpRequest();
            
            MergeClientProperties(restClient, request);

            http.AlwaysMultipartFormData = request.AlwaysMultipartFormData;
            http.UseDefaultCredentials = request.UseDefaultCredentials;
            http.ResponseWriter = request.ResponseWriter;

            http.CookieContainer = request.CookieContainer;

            if (request.Proxy != null)
            {
                http.Proxy = request.Proxy;
            }

            http.Url = RestSharp.UriBuilder.Build(restClient.BaseUrl, request);

            http.UserAgent = restClient.UserAgent;

            if (request.Timeout> 0)
            {
                http.Timeout = request.Timeout;
            }

            http.MaxAutomaticRedirects = request.MaxAutomaticRedirects;

            //http.FollowRedirects = FollowRedirects;

            //if (ClientCertificates != null)
            //{
            //    this.ClientCertificates = ClientCertificates;
            //}

            //http.MaxRedirects = MaxRedirects;

            if (request.Credentials != null)
            {
                http.Credentials = request.Credentials;
            }

            var headers = request.Parameters
                            .Where(p => p.Type == ParameterType.HttpHeader)
                            .Select(p => new HttpHeader() { Name = p.Name, Value = new List<string>() { p.Value.ToString() } });

            foreach (var header in headers)
            {
                http.Headers.Add(header);
            }

            var cookies = request.Parameters
                            .Where(p => p.Type == ParameterType.HttpHeader)
                            .Select(p => new HttpCookie() { Name = p.Name, Value = p.Value.ToString() });
            
            foreach (var cookie in cookies)
            {
                http.Cookies.Add(cookie);
            }

            var @params = request.Parameters
                                .Where(p => p.Type == ParameterType.GetOrPost && p.Value != null)
                                .Select(p => new KeyValuePair<string, string>(p.Name, p.Value.ToString()));

            foreach (var parameter in @params)
            {
                http.Parameters.Add(new KeyValuePair<string, string>(parameter.Key, parameter.Value));
            }

            foreach (var file in request.Files)
            {
                http.Files.Add(new HttpFile { Name = file.Name, ContentType = file.ContentType, Data = file.Data, FileName = file.FileName, ContentLength = file.ContentLength });
            }

            var body = request.Parameters.Where(p => p.Type == ParameterType.RequestBody).FirstOrDefault();

            if (body != null)
            {
                object val = body.Value;
                if (val is byte[])
                    http.RequestBodyBytes = (byte[])val;
                else
                    http.RequestBody = body.Value.ToString();
                http.RequestContentType = body.Name;
            }

            return http;
        }

        private void MergeClientProperties(IRestClient restClient, IRestRequest restRequest)
        {
            // move RestClient.DefaultParameters into Request.Parameters
            foreach (var p in restClient.DefaultParameters)
            {
                if (restRequest.Parameters.Any(p2 => p2.Name == p.Name && p2.Type == p.Type))
                {
                    continue;
                }

                restRequest.AddParameter(p);
            }

            // Add Accept header based on registered deserializers if none has been set by the caller.
            if (restRequest.Parameters.All(p2 => p2.Name.ToLowerInvariant() != "accept"))
            {
                var accepts = string.Join(", ", restClient.DefaultAcceptTypes.ToArray());
                restRequest.AddParameter("Accept", accepts, ParameterType.HttpHeader);
            }

            if (restClient.FollowRedirects && restClient.MaxRedirects.HasValue)
            {
                restRequest.MaxAutomaticRedirects = restClient.MaxRedirects.Value;
            }

            restRequest.Proxy = restClient.Proxy;

            restRequest.CookieContainer = restClient.CookieContainer;

            //overrides the RestRequest timeout
            if (restClient.Timeout.HasValue)
            {
                restRequest.Timeout = restClient.Timeout.Value;
            }

            if (restClient.Authenticator != null)
            {
                restClient.Authenticator.Authenticate(restRequest);
            }

            //set uri
        }


        public IRestResponse ConvertFrom(IHttpResponse httpResponse)
        {
            var restResponse = new RestResponse();
            restResponse.Content = httpResponse.Content;
            restResponse.ContentEncoding = httpResponse.ContentEncoding;
            restResponse.ContentLength = httpResponse.ContentLength;
            restResponse.ContentType = httpResponse.ContentType;
            restResponse.ErrorException = httpResponse.ErrorException;
            restResponse.ErrorMessage = httpResponse.ErrorMessage;
            restResponse.RawBytes = httpResponse.RawBytes;
            restResponse.ResponseStatus = httpResponse.ResponseStatus;
            restResponse.ResponseUri = httpResponse.ResponseUri;
            restResponse.Server = httpResponse.Server;
            restResponse.StatusCode = httpResponse.StatusCode;
            restResponse.StatusDescription = httpResponse.StatusDescription;

            foreach (var header in httpResponse.Headers)
            {
                restResponse.Headers.Add(new Parameter { Name = header.Name, Value = header.Value, Type = ParameterType.HttpHeader });
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
    }
}
