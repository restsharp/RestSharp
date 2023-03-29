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
    public static Uri BuildUri(this IRestClient client, RestRequest request) {
        DoBuildUriValidations(client, request);

        var (uri, resource) = client.Options.BaseUrl.GetUrlSegmentParamsValues(
            request.Resource,
            client.Options.Encode,
            request.Parameters,
            client.DefaultParameters
        );
        var mergedUri = uri.MergeBaseUrlAndResource(resource);

        var finalUri = mergedUri.ApplyQueryStringParamsValuesToUri(
            request.Method,
            client.Options.Encoding,
            client.Options.EncodeQuery,
            request.Parameters,
            client.DefaultParameters
        );
        return finalUri;
    }

    static void DoBuildUriValidations(IRestClient client, RestRequest request) {
        if (client.Options.BaseUrl == null && !request.Resource.ToLowerInvariant().StartsWith("http"))
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "Request resource doesn't contain a valid scheme for an empty base URL of the client"
            );
    }

    public static Uri BuildUriWithoutQueryParameters(this IRestClient client, RestRequest request) {
        DoBuildUriValidations(client, request);
        var (uri, resource) = client.Options.BaseUrl.GetUrlSegmentParamsValues(request.Resource, client.Options.Encode, request.Parameters, client.DefaultParameters);
        return uri.MergeBaseUrlAndResource(resource);
    }
}
