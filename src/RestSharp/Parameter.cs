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
        public string? Name { get; protected set; }

        /// <summary>
        /// Value of the parameter
        /// </summary>
        public object? Value { get; protected set; }

        /// <summary>
        /// Type of the parameter
        /// </summary>
        public abstract ParameterType Type { get;}

        protected Parameter(object value) => Value = value;
        protected Parameter(string name, object value) : this(value)
        {
            Ensure.NotEmpty(name, nameof(name));
            Name = name;
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
        public override ParameterType Type { get; } = ParameterType.Cookie;
        internal CookieParameter(string name, string value) : base(name, value) {}
    }

    public class HttpHeaderParameter : Parameter
    {
        public override ParameterType Type { get; } = ParameterType.HttpHeader;
        internal HttpHeaderParameter(string name, string value) : base(name, value) {}
    }

    public class GetOrPostParameter : Parameter
    {
        public virtual string? ContentType { get; }
        public override ParameterType Type { get; } = ParameterType.GetOrPost;

        internal GetOrPostParameter(string name, object value, string? contentType = null) : base(name, value) => ContentType = contentType;
    }

    public class QueryStringParameter : GetOrPostParameter
    {
        public override ParameterType Type { get; } = ParameterType.QueryString;

        internal QueryStringParameter(string name, string value, bool encode) : base(name, value)
            => Type = encode ? ParameterType.QueryString : ParameterType.QueryStringWithoutEncode;
    }

    public class UrlSegmentParameter : Parameter
    {
        public override ParameterType Type { get; } = ParameterType.UrlSegment;
        internal UrlSegmentParameter(string name, string value) : base(name, value)
        {
            Value = value?.Replace("%2F", "/").Replace("%2f", "/");
        }
    }

    public class BodyParameter : Parameter
    {
        public virtual string? ContentType { get; }
        public virtual DataFormat DataFormat { get; } = DataFormat.None;
        public override ParameterType Type { get; } = ParameterType.RequestBody;

        internal BodyParameter(string name, object value) : base(value) => Name = name;
        internal BodyParameter(string name, object value, string? contentType) : this(name, value) => ContentType = contentType;

        public bool Equals(BodyParameter other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return base.Equals(other)
                && DataFormat == other.DataFormat
                && ContentType == other.ContentType;
        }

        public override bool Equals(object obj)
            => !ReferenceEquals(null, obj)
               && (ReferenceEquals(this, obj) || obj.GetType() == this.GetType() && Equals((BodyParameter) obj));

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();

                hashCode = (hashCode * 397) ^ (int) DataFormat;
                hashCode = (hashCode * 397) ^ (ContentType != null ? ContentType.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public class XmlBodyParameter : BodyParameter
    {
        public override DataFormat DataFormat { get; } = DataFormat.Xml;
        public override string ContentType { get; } = Serialization.ContentType.Xml;
        public string? XmlNamespace { get; }

        internal XmlBodyParameter(object value, string? xmlNamespace = null) : base(string.Empty, value) => XmlNamespace = xmlNamespace;

        public bool Equals(XmlBodyParameter other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return base.Equals(other)
                   && XmlNamespace == other.XmlNamespace;
        }

        public override bool Equals(object obj)
            => !ReferenceEquals(null, obj)
               && (ReferenceEquals(this, obj) || obj.GetType() == this.GetType() && Equals((XmlBodyParameter) obj));

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (XmlNamespace != null ? XmlNamespace.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public class JsonBodyParameter : BodyParameter
    {
        public override DataFormat DataFormat { get; } = DataFormat.Json;
        public override string ContentType { get; } = Serialization.ContentType.Json;

        internal JsonBodyParameter(object value, string? contentType = null) : base(contentType ?? string.Empty, value) => ContentType = contentType ?? Serialization.ContentType.Json;

        public override bool Equals(object obj)
            => !ReferenceEquals(null, obj)
               && (ReferenceEquals(this, obj) || obj.GetType() == this.GetType() && Equals((JsonBodyParameter) obj));
    }

    public static class ParameterFactory
    {
        public static BodyParameter CreateBodyParameter(string name, object value, string? contentType = null) => new BodyParameter(name, value, contentType);
        public static XmlBodyParameter CreateXmlBody(object value, string? xmlNamespace = null) => new XmlBodyParameter(value, xmlNamespace);
        public static JsonBodyParameter CreateJsonBody(object value, string? contentType = null) => new JsonBodyParameter(value, contentType);
        public static UrlSegmentParameter CreateUrlSegment(string name, string value) => new UrlSegmentParameter(name, value);
        public static UrlSegmentParameter CreateUrlSegment(string name, object value) => new UrlSegmentParameter(name, $"{value}");
        public static QueryStringParameter CreateQueryString(string name, string value, bool encode = true) => new QueryStringParameter(name, value, encode);
        public static QueryStringParameter CreateQueryString(string name, object value, bool encode = true) => CreateQueryString(name, value.ToString(), encode);
        public static GetOrPostParameter CreateGetOrPost(string name, object value, string? contentType = null) => new GetOrPostParameter(name, value, contentType);
        public static CookieParameter CreateCookie(string name, string value) => new CookieParameter(name, value);
        public static CookieParameter CreateCookie(string name, object value) => CreateCookie(name, value.ToString());
        public static HttpHeaderParameter CreateHttpHeader(string name, string value) => new HttpHeaderParameter(name, value);
        public static HttpHeaderParameter CreateHttpHeader(string name, object value) => CreateHttpHeader(name, value.ToString());

        public static Parameter Create(string name, object value, ParameterType type)=> type switch
        {
            ParameterType.Cookie => CreateCookie(name, value),
            ParameterType.UrlSegment => CreateUrlSegment(name, value),
            ParameterType.HttpHeader => CreateHttpHeader(name, value),
            ParameterType.RequestBody => CreateBodyParameter(name, value),
            ParameterType.QueryString => CreateQueryString(name, value),
            ParameterType.QueryStringWithoutEncode => CreateQueryString(name, value, false),
            _ => CreateGetOrPost(name, value)
        };
        public static Parameter Create(string name, object value, string contentType, ParameterType type) => type switch
        {
            ParameterType.Cookie => CreateCookie(name, value),
            ParameterType.UrlSegment => CreateUrlSegment(name, value),
            ParameterType.HttpHeader => CreateHttpHeader(name, value),
            ParameterType.RequestBody => CreateBodyParameter(name, value, contentType),
            ParameterType.QueryString => CreateQueryString(name, value),
            ParameterType.QueryStringWithoutEncode => CreateQueryString(name, value, false),
            _ => CreateGetOrPost(name, value, contentType)
        };
    }
}