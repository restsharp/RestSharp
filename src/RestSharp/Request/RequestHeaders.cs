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

using System.Net;

namespace RestSharp;

class RequestHeaders {
    public ParametersCollection Parameters { get; } = new();

    public RequestHeaders AddHeaders(ParametersCollection parameters) {
        Parameters.AddParameters(parameters.GetParameters<HeaderParameter>());
        return this;
    }

    // Add Accept header based on registered deserializers if none has been set by the caller.
    public RequestHeaders AddAcceptHeader(string[] acceptedContentTypes) {
        if (Parameters.TryFind(KnownHeaders.Accept) == null) {
            var accepts = string.Join(", ", acceptedContentTypes);
            Parameters.AddParameter(new HeaderParameter(KnownHeaders.Accept, accepts));
        }

        return this;
    }

    // Add Cookie header from the cookie container
    public RequestHeaders AddCookieHeaders(CookieContainer cookieContainer, Uri uri) {
        var cookies = cookieContainer.GetCookieHeader(uri);
        if (cookies.Length > 0) {
            Parameters.AddParameter(new HeaderParameter(KnownHeaders.Cookie, cookies));
        }
        return this;
    }
}