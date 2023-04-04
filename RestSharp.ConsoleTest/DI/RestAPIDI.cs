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

using Microsoft.Extensions.DependencyInjection;
using ResharperDummy.Authenticator;
using RestSharp.ConsoleTest.Interceptors;
using System.Net.Http.Headers;
using System.Text.Json;
using RestSharp.Serializers;
using RestSharp.Serializers.Json;
using WebAPIClient.Util.Utils;

namespace RestSharp.ConsoleTest.DI; 

public static class RestAPIDI {
    public static IServiceCollection AddRestAPIConfig(this IServiceCollection services) {
        services.AddSingleton<RestClient>(CreateRestClient);
        return services;
    }

    static RestClient CreateRestClient(IServiceProvider arg) {
        var client = new RestClient(
            ConfigureRestClient(),
            ConfigureHeaders,
            ConfigureSerialization,
            true
        );
        return client;
    }

    static RestClientOptions ConfigureRestClient() {
        var restClientOptions = new RestClientOptions("https://localhost:9999");
        restClientOptions.Interceptors.Add(new ErrorInterceptor());
        restClientOptions.Authenticator = new BauradarAuthenticator();
        return restClientOptions;
    }

    static void ConfigureHeaders(HttpRequestHeaders headers) {
        headers.Add("api-version", "1.0");
        headers.Add("api-key", "QmF1cmFkYXI=");
        headers.Add("DeviceName", "ResharpDummy");
        headers.Add("DeviceId", "ResharpDummy");
    }
    static void ConfigureSerialization(SerializerConfig serializerConfig) {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new CustomDateTimeConverter("dd.MM.yyyy"));
        var serializer = new SystemTextJsonSerializer(options);
        serializerConfig.UseSerializer(() => serializer);
    }
}
