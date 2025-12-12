//   Copyright (c) .NET Foundation and Contributors
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System.Text;

namespace RestSharp.Authenticators;

/// <summary>
/// Allows "basic access authentication" for HTTP requests.
/// </summary>
/// <remarks>
/// Encoding can be specified depending on what your server expect (see https://stackoverflow.com/a/7243567).
/// UTF-8 is used by default but some servers might expect ISO-8859-1 encoding.
/// </remarks>
[PublicAPI]
public class HttpBasicAuthenticator(string username, string password, Encoding encoding)
    : AuthenticatorBase(GetHeader(username, password, encoding)) {
    public HttpBasicAuthenticator(string username, string password) : this(username, password, Encoding.UTF8) { }

    static string GetHeader(string username, string password, Encoding encoding)
        => Convert.ToBase64String(encoding.GetBytes($"{username}:{password}"));

    // return ;
    protected override ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
        => new(new HeaderParameter(KnownHeaders.Authorization, $"Basic {accessToken}"));
}