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
// 

using System.Text;
using RestSharp.Extensions;

namespace RestSharp;

static class UriExtensions {
    public static Uri MergeBaseUrlAndResource(this Uri? baseUrl, string? resource) {
        var assembled = resource;

        if (assembled.IsNotEmpty() && assembled!.StartsWith("/")) assembled = assembled.Substring(1);

        if (baseUrl == null || baseUrl.AbsoluteUri.IsEmpty()) {
            return assembled.IsNotEmpty()
                ? new Uri(assembled!)
                : throw new ArgumentException("Both BaseUrl and Resource are empty", nameof(resource));
        }

        var usingBaseUri = baseUrl.AbsoluteUri.EndsWith("/") || assembled.IsEmpty() ? baseUrl : new Uri(baseUrl.AbsoluteUri + "/");

        return assembled != null ? new Uri(usingBaseUri, assembled) : baseUrl;
    }

    public static Uri ApplyQueryStringParamsValuesToUri(
        this Uri                       mergedUri,
        Method                         method,
        Encoding                       encoding,
        Func<string, Encoding, string> encodeQuery,
        params ParametersCollection[]  parametersCollections
    ) {
        var parameters = parametersCollections.SelectMany(x => x.GetQueryParameters(method)).ToList();

        if (parameters.Count == 0) return mergedUri;

        var uri       = mergedUri.AbsoluteUri;
        var separator = uri.Contains('?') ? "&" : "?";

        return new Uri(string.Concat(uri, separator, EncodeParameters()));

        string EncodeParameters() => string.Join("&", parameters.Select(EncodeParameter).ToArray());

        string GetString(string name, string? value, Func<string, string>? encode) {
            var val = encode != null && value != null ? encode(value) : value;
            return val == null ? name : $"{name}={val}";
        }

        string EncodeParameter(Parameter parameter)
            => !parameter.Encode
                ? GetString(parameter.Name!, parameter.Value?.ToString(), null)
                : GetString(encodeQuery(parameter.Name!, encoding), parameter.Value?.ToString(), x => encodeQuery(x, encoding));
    }

    public static UrlSegmentParamsValues GetUrlSegmentParamsValues(
        this Uri?                     baseUri,
        string                        resource,
        Func<string, string>          encode,
        params ParametersCollection[] parametersCollections
    ) {
        var assembled = baseUri == null ? "" : resource;
        var baseUrl   = baseUri ?? new Uri(resource);

        var hasResource = !assembled.IsEmpty();

        var parameters = parametersCollections.SelectMany(x => x.GetParameters(ParameterType.UrlSegment));

        var builder = new UriBuilder(baseUrl);

        foreach (var parameter in parameters) {
            var paramPlaceHolder = $"{{{parameter.Name}}}";
            var paramValue       = parameter.Encode ? encode(parameter.Value!.ToString()!) : parameter.Value!.ToString();

            if (hasResource) assembled = assembled.Replace(paramPlaceHolder, paramValue);

            builder.Path = builder.Path.UrlDecode().Replace(paramPlaceHolder, paramValue);
        }

        return new UrlSegmentParamsValues(builder.Uri, assembled);
    }
}

record UrlSegmentParamsValues(Uri Uri, string Resource);
