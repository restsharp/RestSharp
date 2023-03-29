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

namespace RestSharp;

public static class BuildUriExtensions {
    /// <summary>
    /// Builds the URI for the request
    /// </summary>
    /// <param name="client">Client instance</param>
    /// <param name="request">Request instance</param>
    /// <returns></returns>
    public static Uri BuildUri(this IRestClient client, RestRequest request) {
        DoBuildUriValidations(client, request);

        var (uri, resource) = client.Options.BaseUrl.GetUrlSegmentParamsValues(
            request.Resource,
            client.Options.Encode,
            request.Parameters,
            client.DefaultParameters
        );
        var mergedUri = uri.MergeBaseUrlAndResource(resource);
        var query     = client.GetRequestQuery(request);
        return mergedUri.AddQueryString(query);
    }

    /// <summary>
    /// Builds the URI for the request without query parameters.
    /// </summary>
    /// <param name="client">Client instance</param>
    /// <param name="request">Request instance</param>
    /// <returns></returns>
    public static Uri BuildUriWithoutQueryParameters(this IRestClient client, RestRequest request) {
        DoBuildUriValidations(client, request);

        var (uri, resource) = client.Options.BaseUrl.GetUrlSegmentParamsValues(
            request.Resource,
            client.Options.Encode,
            request.Parameters,
            client.DefaultParameters
        );
        return uri.MergeBaseUrlAndResource(resource);
    }

    /// <summary>
    /// Gets the query string for the request.
    /// </summary>
    /// <param name="client">Client instance</param>
    /// <param name="request">Request instance</param>
    /// <returns></returns>
    [PublicAPI]
    public static string? GetRequestQuery(this IRestClient client, RestRequest request) {
        var parametersCollections = new ParametersCollection[] { request.Parameters, client.DefaultParameters };

        var parameters = parametersCollections.SelectMany(x => x.GetQueryParameters(request.Method)).ToList();

        return parameters.Count == 0 ? null : string.Join("&", parameters.Select(EncodeParameter).ToArray());

        string GetString(string name, string? value, Func<string, string>? encode) {
            var val = encode != null && value != null ? encode(value) : value;
            return val == null ? name : $"{name}={val}";
        }

        string EncodeParameter(Parameter parameter)
            => !parameter.Encode
                ? GetString(parameter.Name!, parameter.Value?.ToString(), null)
                : GetString(
                    client.Options.EncodeQuery(parameter.Name!, client.Options.Encoding),
                    parameter.Value?.ToString(),
                    x => client.Options.EncodeQuery(x, client.Options.Encoding)
                );
    }

    static void DoBuildUriValidations(IRestClient client, RestRequest request) {
        if (client.Options.BaseUrl == null && !request.Resource.ToLowerInvariant().StartsWith("http"))
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "Request resource doesn't contain a valid scheme for an empty base URL of the client"
            );
    }
}
