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

using System.Net.Http.Headers;

namespace RestSharp;

public delegate bool SupportsContentType(ContentType contentType);

public class ContentType : IEquatable<ContentType> {
    ContentType(string contentType) {
        var ct = Ensure.NotEmptyString(contentType, nameof(contentType));
        if (!ct.Contains('/')) throw new ArgumentException("Invalid content type string", nameof(contentType));

        _value = ct;
    }

    public static readonly ContentType Json           = "application/json";
    public static readonly ContentType Xml            = "application/xml";
    public static readonly ContentType Plain          = "text/plain";
    public static readonly ContentType Binary         = "application/octet-stream";
    public static readonly ContentType GZip           = "application/x-gzip";
    public static readonly ContentType FormUrlEncoded = "application/x-www-form-urlencoded";
    public static readonly ContentType Undefined      = "undefined/undefined";

    public string Value => _value == Undefined._value ? Plain._value : _value;

    public static ContentType FromDataFormat(DataFormat dataFormat) => DataFormatMap[dataFormat];

    public override string ToString() => Value;

    public static implicit operator ContentType(string? contentType) => contentType == null ? Undefined : new(contentType);

    public static implicit operator string(ContentType contentType) => contentType.Value;

    public ContentType Or(ContentType? contentType) => Equals(Undefined) ? contentType ?? Plain : this;

    public string OrValue(string? contentType) => Equals(Undefined) ? contentType ?? Plain.Value : Value;

    public MediaTypeHeaderValue AsMediaTypeHeaderValue => MediaTypeHeaderValue.Parse(Value);

    static readonly Dictionary<DataFormat, ContentType> DataFormatMap =
        new() {
            { DataFormat.Xml, Xml },
            { DataFormat.Json, Json },
            { DataFormat.None, Plain },
            { DataFormat.Binary, Binary }
        };

    public static readonly string[] JsonAccept = {
        "application/json", "text/json", "text/x-json", "text/javascript"
    };

    public static readonly string[] XmlAccept = {
        "application/xml", "text/xml"
    };

    readonly string _value;

    public bool Equals(ContentType? other) {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return _value == other._value;
    }

    public override bool Equals(object? obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((ContentType)obj);
    }

    public override int GetHashCode() => _value.GetHashCode();
}
