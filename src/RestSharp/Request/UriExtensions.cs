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

using RestSharp.Extensions;

namespace RestSharp;

static class UriExtensions {
    public static Uri MergeBaseUrlAndResource(this Uri? baseUrl, string? resource) {
        var assembled = resource;

        if (assembled.IsNotEmpty() && assembled.StartsWith("/")) assembled = assembled.Substring(1);

        if (baseUrl == null || baseUrl.AbsoluteUri.IsEmpty()) {
            return assembled.IsNotEmpty()
                ? new Uri(assembled)
                : throw new ArgumentException("Both BaseUrl and Resource are empty", nameof(resource));
        }

        var usingBaseUri = baseUrl.AbsoluteUri.EndsWith("/") || assembled.IsEmpty() ? baseUrl : new Uri(baseUrl.AbsoluteUri + "/");

        return assembled != null ? new Uri(usingBaseUri, assembled) : baseUrl;
    }

    public static Uri AddQueryString(this Uri uri, string? query) {
        if (query == null) return uri;

        var absoluteUri       = uri.AbsoluteUri;
        var separator = absoluteUri.Contains('?') ? "&" : "?";

        return new Uri($"{absoluteUri}{separator}{query}");
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

        var parameters = parametersCollections.SelectMany(x => x.GetParameters<UrlSegmentParameter>());

        var builder = new UriBuilder(baseUrl);

        foreach (var parameter in parameters) {
            var paramPlaceHolder = $"{{{parameter.Name}}}";
            var value            = Ensure.NotNull(parameter.Value!.ToString(), $"URL segment parameter {parameter.Name} value");
            var paramValue       = parameter.Encode ? encode(value) : value;

            if (hasResource) assembled = assembled.Replace(paramPlaceHolder, paramValue);

            builder.Path = builder.Path.UrlDecode().Replace(paramPlaceHolder, paramValue);
        }

        return new UrlSegmentParamsValues(builder.Uri, assembled);
    }
}

record UrlSegmentParamsValues(Uri Uri, string Resource);
