//  Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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

//  Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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

using RestSharp.Serialization;
using Utf8Json;
using Utf8Json.Resolvers;

namespace RestSharp.Serializers.Utf8Json
{
    public class Utf8JsonSerializer : IRestSerializer
    {
        public Utf8JsonSerializer(IJsonFormatterResolver resolver = null) => Resolver = resolver ?? StandardResolver.AllowPrivateExcludeNullCamelCase;

        IJsonFormatterResolver Resolver { get; }

        public string Serialize(object obj) => JsonSerializer.NonGeneric.ToJsonString(obj, Resolver);

        public string Serialize(Parameter parameter) => Serialize(parameter.Value);

        public T Deserialize<T>(IRestResponse response) => JsonSerializer.Deserialize<T>(response.RawBytes, Resolver);

        public string[] SupportedContentTypes { get; } =
        {
            "application/json", "text/json", "text/x-json", "text/javascript", "*+json"
        };

        public string ContentType { get; set; } = "application/json";

        public DataFormat DataFormat { get; } = DataFormat.Json;
    }
}