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

namespace RestSharp;

public record BodyParameter : Parameter {
    public BodyParameter(string? name, object value, string contentType, DataFormat dataFormat = DataFormat.None)
        : base(name, Ensure.NotNull(value, nameof(value)), ParameterType.RequestBody, false) {
        if (dataFormat == DataFormat.Binary && value is not byte[]) {
            throw new ArgumentException("Binary data format needs a byte array as value");
        }
        ContentType = contentType;
        DataFormat  = dataFormat;
    }

    /// <summary>
    /// Body parameter data type
    /// </summary>
    public DataFormat DataFormat { get; init; } = DataFormat.None;
    
    /// <summary>
    /// Custom content encoding
    /// </summary>
    public string? ContentEncoding { get; init; }
}

public record XmlParameter : BodyParameter {
    public XmlParameter(string name, object value, string? xmlNamespace = null, string contentType = Serializers.ContentType.Xml)
        : base(name, value, contentType, DataFormat.Xml)
        => XmlNamespace = xmlNamespace;

    public string? XmlNamespace { get; }
}

public record JsonParameter : BodyParameter {
    public JsonParameter(string name, object value, string contentType = Serializers.ContentType.Json) 
        : base(name, value, contentType, DataFormat.Json) { }
}