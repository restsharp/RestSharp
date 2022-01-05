﻿//   Copyright © 2009-2021 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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
public abstract record Parameter(string? Name, object? Value, ParameterType Type, bool Encode) {
    /// <summary>
    /// MIME content type of the parameter
    /// </summary>
    public string? ContentType { get; protected init; }

    /// <summary>
    /// Return a human-readable representation of this parameter
    /// </summary>
    /// <returns>String</returns>
    public override string ToString() => $"{Name}={Value}";

    public static Parameter CreateParameter(string? name, object value, ParameterType type, bool encode = true)
        => type switch {
            ParameterType.GetOrPost   => new GetOrPostParameter(name!, value, encode),
            ParameterType.UrlSegment  => new UrlSegmentParameter(name!, value, encode),
            ParameterType.HttpHeader  => new HeaderParameter(name, value),
            ParameterType.RequestBody => new BodyParameter(name, value, Serializers.ContentType.Plain),
            ParameterType.QueryString => new QueryParameter(name!, value, encode),
            _                         => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
}

public record NamedParameter : Parameter {
    protected NamedParameter(string name, object? value, ParameterType type, bool encode = true)
        : base(Ensure.NotEmptyString(name, nameof(name)), value, type, encode) { }
}