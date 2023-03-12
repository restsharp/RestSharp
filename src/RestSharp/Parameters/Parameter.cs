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

namespace RestSharp;

/// <summary>
/// Parameter container for REST requests
/// </summary>
public abstract record Parameter(string? Name, object? Value, ParameterType Type, bool Encode) {
    /// <summary>
    /// MIME content type of the parameter
    /// </summary>
    public ContentType ContentType { get; protected init; } = ContentType.Undefined;

    /// <summary>
    /// Return a human-readable representation of this parameter
    /// </summary>
    /// <returns>String</returns>
    public override string ToString() => $"{Name}={Value}";

    public static Parameter CreateParameter(string? name, object? value, ParameterType type, bool encode = true)
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        => type switch {
            ParameterType.GetOrPost   => new GetOrPostParameter(Ensure.NotEmptyString(name, nameof(name)), value?.ToString(), encode),
            ParameterType.UrlSegment  => new UrlSegmentParameter(Ensure.NotEmptyString(name, nameof(name)), value?.ToString()!, encode),
            ParameterType.HttpHeader  => new HeaderParameter(name, value?.ToString()),
            ParameterType.QueryString => new QueryParameter(Ensure.NotEmptyString(name, nameof(name)), value?.ToString(), encode),
            _                         => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
}

public record NamedParameter : Parameter {
    protected NamedParameter(string name, object? value, ParameterType type, bool encode = true)
        : base(Ensure.NotEmptyString(name, nameof(name)), value, type, encode) { }
}