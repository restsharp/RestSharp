//   Copyright © 2009-2021 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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

using System.Text.Json;

namespace RestSharp.Serializers.Json; 

[PublicAPI]
public static class RestClientExtensions {
    /// <summary>
    /// Use System.Text.Json serializer with default settings
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    public static RestClient UseSystemTextJson(this RestClient client) => client.UseSerializer(() => new SystemTextJsonSerializer());

    /// <summary>
    /// Use System.Text.Json serializer with custom settings
    /// </summary>
    /// <param name="client"></param>
    /// <param name="options">System.Text.Json serializer options</param>
    /// <returns></returns>
    public static RestClient UseSystemTextJson(this RestClient client, JsonSerializerOptions options)
        => client.UseSerializer(() => new SystemTextJsonSerializer(options));
}