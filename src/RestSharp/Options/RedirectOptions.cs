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

/// <summary>
/// Options for controlling redirect behavior when RestSharp handles redirects.
/// </summary>
public class RedirectOptions {
    /// <summary>
    /// Whether to follow redirects. Default is true.
    /// </summary>
    public bool FollowRedirects { get; set; } = true;

    /// <summary>
    /// Whether to follow redirects from HTTPS to HTTP (insecure). Default is false.
    /// </summary>
    public bool FollowRedirectsToInsecure { get; set; }

    /// <summary>
    /// Whether to forward request headers on redirect. Default is true.
    /// </summary>
    public bool ForwardHeaders { get; set; } = true;

    /// <summary>
    /// Whether to forward the Authorization header on redirect. Default is false.
    /// </summary>
    public bool ForwardAuthorization { get; set; }

    /// <summary>
    /// Whether to forward cookies on redirect. Default is true.
    /// Cookies from Set-Cookie headers are always stored in the CookieContainer regardless of this setting.
    /// </summary>
    public bool ForwardCookies { get; set; } = true;

    /// <summary>
    /// Whether to forward the request body on redirect when the HTTP verb is preserved. Default is true.
    /// Body is always dropped when the verb changes to GET.
    /// </summary>
    public bool ForwardBody { get; set; } = true;

    /// <summary>
    /// Whether to forward original query string parameters on redirect. Default is true.
    /// </summary>
    public bool ForwardQuery { get; set; } = true;

    /// <summary>
    /// Maximum number of redirects to follow. Default is 50.
    /// </summary>
    public int MaxRedirects { get; set; } = 50;

    /// <summary>
    /// HTTP status codes that are considered redirects.
    /// </summary>
    public IReadOnlyList<HttpStatusCode> RedirectStatusCodes { get; set; } = [
        HttpStatusCode.MovedPermanently,  // 301
        HttpStatusCode.Found,             // 302
        HttpStatusCode.SeeOther,          // 303
        HttpStatusCode.TemporaryRedirect, // 307
        (HttpStatusCode)308,              // 308 Permanent Redirect
    ];
}
