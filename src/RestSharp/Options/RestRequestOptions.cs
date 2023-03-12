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

using System.Net.Http.Headers;
using RestSharp.Authenticators;

namespace RestSharp;

public class RestRequestOptions {
    /// <summary>
    /// Authenticator that will be used to populate request with necessary authentication data
    /// </summary>
    public IAuthenticator? Authenticator { get; set; }

    public CacheControlHeaderValue? CachePolicy { get; set; }

    public string? BaseHost { get; set; }
}
