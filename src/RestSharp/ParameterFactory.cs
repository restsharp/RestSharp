//  Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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
// 

namespace RestSharp
{
    public static class ParameterFactory
    {
        public static BodyParameter CreateBodyParameter(string name, object? value, string? contentType = null) => new BodyParameter(name, value, contentType);
        public static XmlBodyParameter CreateXmlBody(object value, string? xmlNamespace = null) => new XmlBodyParameter(value, xmlNamespace);
        public static JsonBodyParameter CreateJsonBody(object value, string? contentType = null) => new JsonBodyParameter(value, contentType);
        public static UrlSegmentParameter CreateUrlSegment(string name, string value) => new UrlSegmentParameter(name, value);
        public static UrlSegmentParameter CreateUrlSegment(string name, object? value) => new UrlSegmentParameter(name, $"{value}");
        public static QueryStringParameter CreateQueryString(string name, string value, bool encode = true) => new QueryStringParameter(name, value, encode);
        public static QueryStringParameter CreateQueryString(string name, object? value, bool encode = true) => CreateQueryString(name, value?.ToString() ?? string.Empty, encode);
        public static GetOrPostParameter CreateGetOrPost(string name, object? value) => new GetOrPostParameter(name, value);
        public static CookieParameter CreateCookie(string name, string value) => new CookieParameter(name, value);
        public static CookieParameter CreateCookie(string name, object? value) => CreateCookie(name, value?.ToString() ?? string.Empty);
        public static HttpHeaderParameter CreateHttpHeader(string name, string value) => new HttpHeaderParameter(name, value);
        public static HttpHeaderParameter CreateHttpHeader(string name, object? value) => CreateHttpHeader(name, value?.ToString() ?? string.Empty);

        public static Parameter Create(string name, object? value, ParameterType type)=> type switch
        {
            ParameterType.Cookie => CreateCookie(name, value),
            ParameterType.UrlSegment => CreateUrlSegment(name, value),
            ParameterType.HttpHeader => CreateHttpHeader(name, value),
            ParameterType.RequestBody => CreateBodyParameter(name, value),
            ParameterType.QueryString => CreateQueryString(name, value),
            ParameterType.QueryStringWithoutEncode => CreateQueryString(name, value, false),
            _ => CreateGetOrPost(name, value)
        };
        public static Parameter Create(string name, object? value, string contentType, ParameterType type) => type switch
        {
            ParameterType.Cookie => CreateCookie(name, value),
            ParameterType.UrlSegment => CreateUrlSegment(name, value),
            ParameterType.HttpHeader => CreateHttpHeader(name, value),
            ParameterType.RequestBody => CreateBodyParameter(name, value, contentType),
            ParameterType.QueryString => CreateQueryString(name, value),
            ParameterType.QueryStringWithoutEncode => CreateQueryString(name, value, false),
            _ => CreateGetOrPost(name, value)
        };
    }
}