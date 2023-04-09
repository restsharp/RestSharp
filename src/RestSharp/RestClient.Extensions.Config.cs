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

namespace RestSharp;

public static partial class RestClientExtensions {
    [PublicAPI]
    public static RestResponse<T> Deserialize<T>(this IRestClient client, RestResponse response)
        => client.Serializers.Deserialize<T>(response.Request, response, client.Options);

    [Obsolete("Set the RestClientOptions.Encode property instead")]
    public static RestClient UseUrlEncoder(this RestClient client, Func<string, string> encoder)
        => throw new NotImplementedException("Set the RestClientOptions.Encode property instead");

    [Obsolete("Set the RestClientOptions.EncodeQuery property instead")]
    public static RestClient UseQueryEncoder(this RestClient client, Func<string, Encoding, string> queryEncoder)
        => throw new NotImplementedException("Set the RestClientOptions.EncodeQuery property instead");

    [Obsolete("Set the RestClientOptions.Authenticator property instead")]
    public static RestClient UseAuthenticator(this RestClient client, IAuthenticator authenticator)
        => throw new NotImplementedException("Set the RestClientOptions.Authenticator property instead");
}
