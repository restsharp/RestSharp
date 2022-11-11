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
using RestSharp.Authenticators;
using RestSharp.Extensions;

namespace RestSharp;

public static partial class RestClientExtensions {
    [PublicAPI]
    public static RestResponse<T> Deserialize<T>(this RestClient client, RestResponse response) => client.Deserialize<T>(response.Request!, response);

    /// <summary>
    /// Allows to use a custom way to encode URL parameters
    /// </summary>
    /// <param name="client"></param>
    /// <param name="encoder">A delegate to encode URL parameters</param>
    /// <example>client.UseUrlEncoder(s => HttpUtility.UrlEncode(s));</example>
    /// <returns></returns>
    public static RestClient UseUrlEncoder(this RestClient client, Func<string, string> encoder) => client.With(x => x.Encode = encoder);

    /// <summary>
    /// Allows to use a custom way to encode query parameters
    /// </summary>
    /// <param name="client"></param>
    /// <param name="queryEncoder">A delegate to encode query parameters</param>
    /// <example>client.UseUrlEncoder((s, encoding) => HttpUtility.UrlEncode(s, encoding));</example>
    /// <returns></returns>
    public static RestClient UseQueryEncoder(this RestClient client, Func<string, Encoding, string> queryEncoder)
        => client.With(x => x.EncodeQuery = queryEncoder);

    public static RestClient UseAuthenticator(this RestClient client, IAuthenticator authenticator)
        => client.With(x => x.Authenticator = authenticator);
}