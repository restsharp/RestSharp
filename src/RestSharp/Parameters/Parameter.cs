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

namespace RestSharp; 

/// <summary>
/// Parameter container for REST requests
/// </summary>
public record Parameter {
    public Parameter(string? name, object? value, ParameterType type, bool encode = true) {
        if (type != ParameterType.RequestBody)
            Ensure.NotEmpty(name, nameof(name));
        else {
            Ensure.NotNull(value, nameof(value));
        }

        Name   = name;
        Value  = type != ParameterType.UrlSegment ? value : value?.ToString()?.Replace("%2F", "/").Replace("%2f", "/");
        Type   = type == ParameterType.QueryStringWithoutEncode ? ParameterType.QueryString : type;
        Encode = type != ParameterType.QueryStringWithoutEncode && encode;
    }

    public Parameter(string name, object value, string contentType, ParameterType type, bool encode = true) : this(name, value, type, encode)
        => ContentType = contentType;

    /// <summary>
    /// Name of the parameter
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Value of the parameter
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Type of the parameter
    /// </summary>
    public ParameterType Type { get; set; }

    /// <summary>
    /// Body parameter data type
    /// </summary>
    public DataFormat DataFormat { get; set; } = DataFormat.None;

    /// <summary>
    /// MIME content type of the parameter
    /// </summary>
    public string? ContentType { get; set; }

    internal bool Encode { get; }

    /// <summary>
    /// Return a human-readable representation of this parameter
    /// </summary>
    /// <returns>String</returns>
    public override string ToString() => $"{Name}={Value}";
}

public record XmlParameter : Parameter {
    public XmlParameter(string name, object value, string? xmlNamespace = null, string contentType = Serializers.ContentType.Xml) 
        : base(name, value, ParameterType.RequestBody) {
        XmlNamespace = xmlNamespace;
        DataFormat   = DataFormat.Xml;
        ContentType  = contentType;
    }

    public string? XmlNamespace { get; }
}

public record JsonParameter : Parameter {
    public JsonParameter(string name, object value, string contentType = Serializers.ContentType.Json) 
        : base(name, value, ParameterType.RequestBody) {
        DataFormat  = DataFormat.Json;
        ContentType = contentType;
    }
}