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

// ReSharper disable InvertIf

using RestSharp.Extensions;

namespace RestSharp;

class RequestHeaders : ParametersCollection<HeaderParameter> {
    public RequestHeaders AddHeaders(ParametersCollection parameters) {
        Parameters.AddRange(parameters.GetParameters<HeaderParameter>());
        return this;
    }

    // Add Accept header based on registered deserializers if the caller has set none.
    public RequestHeaders AddAcceptHeader(string[] acceptedContentTypes) {
        if (TryFind(KnownHeaders.Accept) == null) {
            var accepts = acceptedContentTypes.JoinToString(", ");
            Parameters.Add(new(KnownHeaders.Accept, accepts));
        }

        return this;
    }

    // Add Cookie header from the cookie container
    public RequestHeaders AddCookieHeaders(Uri uri, CookieContainer? cookieContainer) {
        if (cookieContainer == null) return this;

        var cookies = cookieContainer.GetCookieHeader(uri);

        if (string.IsNullOrWhiteSpace(cookies)) return this;

        var newCookies = SplitHeader(cookies);
        var existing   = GetParameters<HeaderParameter>().FirstOrDefault(x => x.Name == KnownHeaders.Cookie);

        if (existing?.Value != null) {
            newCookies = newCookies.Union(SplitHeader(existing.Value!));
        }

        Parameters.Add(new(KnownHeaders.Cookie, string.Join("; ", newCookies)));

        return this;

        IEnumerable<string> SplitHeader(string header) => header.Split(';').Select(x => x.Trim());
    }
}
