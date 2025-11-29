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

using System.Diagnostics;

namespace RestSharp;

/// <summary>
/// Parameter container for REST requests
/// </summary>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}()}}")]
public abstract record Parameter {
    /// <summary>
    /// Parameter container for REST requests
    /// </summary>
    protected Parameter(string? name, object? value, ParameterType type, bool encode) {
        Name   = name;
        Value  = value;
        Type   = type;
        Encode = encode;
    }

    /// <summary>
    /// Content type of the parameter. Normally applies to the body parameter, or POST parameter in multipart requests.
    /// </summary>
    public ContentType ContentType { get; set; } = ContentType.Undefined;

    /// <summary>
    /// Parameter name
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Parameter value
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Parameter type
    /// </summary>
    public ParameterType Type { get; }

    /// <summary>
    /// Indicates if the parameter value should be encoded or not.
    /// </summary>
    public bool Encode { get; }

    /// <summary>
    /// Return a human-readable representation of this parameter
    /// </summary>
    /// <returns>String</returns>
    public sealed override string ToString() => Value == null ? $"{Name}" : $"{Name}={ValueString}";

    protected virtual string ValueString => Value?.ToString() ?? "null";

    /// <summary>
    /// Creates a parameter object of based on the type
    /// </summary>
    /// <param name="name">Parameter name</param>
    /// <param name="value">Parameter value</param>
    /// <param name="type">Parameter type</param>
    /// <param name="encode">Indicates if the parameter value should be encoded</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static Parameter CreateParameter(string? name, object? value, ParameterType type, bool encode = true)
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        => type switch {
            ParameterType.GetOrPost   => new GetOrPostParameter(Ensure.NotEmptyString(name, nameof(name)), value?.ToString(), encode),
            ParameterType.UrlSegment  => new UrlSegmentParameter(Ensure.NotEmptyString(name, nameof(name)), value?.ToString()!, encode),
            ParameterType.HttpHeader  => new HeaderParameter(name!, value?.ToString()!),
            ParameterType.QueryString => new QueryParameter(Ensure.NotEmptyString(name, nameof(name)), value?.ToString(), encode),
            _                         => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

    [PublicAPI]
    public void Deconstruct(out string? name, out object? value, out ParameterType type, out bool encode) {
        name   = Name;
        value  = Value;
        type   = Type;
        encode = Encode;
    }

    /// <summary>
    /// Assists with debugging by displaying in the debugger output
    /// </summary>
    /// <returns></returns>
    [UsedImplicitly]
    protected string DebuggerDisplay() => $"{GetType().Name.Replace("Parameter", "")} {ToString()}";
}

public record NamedParameter : Parameter {
    protected NamedParameter(string name, object? value, ParameterType type, bool encode = true)
        : base(Ensure.NotEmptyString(name, nameof(name)), value, type, encode) { }
}