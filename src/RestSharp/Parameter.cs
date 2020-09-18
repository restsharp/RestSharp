#region License

//   Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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

#endregion

using System;
using RestSharp.Validation;

namespace RestSharp
{
    /// <summary>
    /// Parameter container for REST requests
    /// </summary>
    public abstract class Parameter : IEquatable<Parameter>
    {
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
        public abstract ParameterType Type { get;}

        public Parameter(string name, object value)
        {
            Name  = name;
            Value = value;
        }

        /// <summary>
        /// Return a human-readable representation of this parameter
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"{Name}={Value}";

        public bool Equals(Parameter other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Name == other.Name
                   && Equals(Value, other.Value)
                   && Type == other.Type;
        }

        public override bool Equals(object obj)
            => !ReferenceEquals(null, obj)
                && (ReferenceEquals(this, obj) || obj.GetType() == this.GetType() && Equals((Parameter) obj));

        // ReSharper disable NonReadonlyMemberInGetHashCode
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name != null ? Name.GetHashCode() : 0;

                hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Type;
                return hashCode;
            }
        }
        // ReSharper enable NonReadonlyMemberInGetHashCode
    }

    public class CookieParameter : Parameter
    {
        public CookieParameter(string name, string value) : base(name, value) => Ensure.NotEmpty(name, nameof(name));
        public override ParameterType Type { get; } = ParameterType.Cookie;
    }

    public class GetOrPostParameter : Parameter
    {
        public override ParameterType Type { get; } = ParameterType.GetOrPost;
        public GetOrPostParameter(string name, object value) : base(name, value) => Ensure.NotEmpty(name, nameof(name));
    }

    public class HttpHeaderParameter : Parameter
    {
        public override ParameterType Type { get; } = ParameterType.HttpHeader;
        public HttpHeaderParameter(string name, object value) : base(name, value) => Ensure.NotEmpty(name, nameof(name));
    }

    public class QueryStringParameter : Parameter
    {
        public override ParameterType Type { get; } = ParameterType.QueryString;

        public QueryStringParameter(string name, object value) : base(name, value)
        {
            Ensure.NotEmpty(name, nameof(name));
            Type = ParameterType.QueryString;

        }
        public QueryStringParameter(string name, object value, bool encode) : this(name, value)
        {
            if (!encode) Type = ParameterType.QueryStringWithoutEncode;
        }
    }

    public class UrlSegmentParameter : Parameter
    {
        public override ParameterType Type { get; } = ParameterType.UrlSegment;
        public UrlSegmentParameter(string name, object value) : base(name, value)
        {
            Ensure.NotEmpty(name, nameof(name));
            Value = value?.ToString().Replace("%2F", "/").Replace("%2f", "/");
        }
    }

    public class BodyParameter : Parameter
    {
        public virtual string? ContentType { get; }
        public virtual DataFormat DataFormat { get; } = DataFormat.None;
        public override ParameterType Type { get; } = ParameterType.RequestBody;

        protected BodyParameter(string name, object value) : base(name, value) {}
        public BodyParameter(string name, object value, string? contentType) : this(name, value) => ContentType = contentType;
    }

    public class XmlBodyParameter : BodyParameter
    {
        public override DataFormat DataFormat { get; } = DataFormat.Xml;
        public override string ContentType { get; } = Serialization.ContentType.Xml;
        public string? XmlNamespace { get; }

        public XmlBodyParameter(string name, object value, string? xmlNamespace = null) : base(name, value) => XmlNamespace = xmlNamespace;
    }


    public class JsonBodyParameter : BodyParameter
    {
        public override DataFormat DataFormat { get; } = DataFormat.Json;
        public override string ContentType { get; } = Serialization.ContentType.Json;

        public JsonBodyParameter(string name, object value) : base(name, value) {}
        public JsonBodyParameter(string name, object value, string contentType) : base(name, value) => ContentType = contentType ?? Serialization.ContentType.Json;
    }
}