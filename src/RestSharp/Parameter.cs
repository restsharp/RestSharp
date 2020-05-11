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
using JetBrains.Annotations;
using RestSharp.Validation;

namespace RestSharp
{
    /// <summary>
    ///     Parameter container for REST requests
    /// </summary>
    public class Parameter : IEquatable<Parameter>
    {
        public Parameter(string name, object value, ParameterType type)
        {
            if (type != ParameterType.RequestBody)
                Ensure.NotEmpty(name, nameof(name));

            Name  = name;
            Value = type != ParameterType.UrlSegment ? value : value?.ToString().Replace("%2F", "/").Replace("%2f", "/");
            Type  = type;
        }

        public Parameter(string name, object value, string contentType, ParameterType type) : this(name, value, type) => ContentType = contentType;

        /// <summary>
        ///     Name of the parameter
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        ///     Value of the parameter
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        ///     Type of the parameter
        /// </summary>
        public ParameterType Type { get; set; }

        /// <summary>
        ///     Body parameter data type
        /// </summary>
        public DataFormat DataFormat { get; set; } = DataFormat.None;

        /// <summary>
        ///     MIME content type of the parameter
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        ///     Return a human-readable representation of this parameter
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"{Name}={Value}";

        public bool Equals(Parameter other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Name == other.Name
                && Equals(Value, other.Value)
                && Type        == other.Type
                && DataFormat  == other.DataFormat
                && ContentType == other.ContentType;
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
                hashCode = (hashCode * 397) ^ (int) DataFormat;
                hashCode = (hashCode * 397) ^ (ContentType != null ? ContentType.GetHashCode() : 0);
                return hashCode;
            }
        }
        // ReSharper enable NonReadonlyMemberInGetHashCode
    }

    public class XmlParameter : Parameter
    {
        public XmlParameter(string name, object value, string? xmlNamespace = null) : base(name, value, ParameterType.RequestBody)
        {
            XmlNamespace = xmlNamespace;
            DataFormat   = DataFormat.Xml;
            ContentType  = Serialization.ContentType.Xml;
        }

        public string? XmlNamespace { get; }
    }

    public class JsonParameter : Parameter
    {
        public JsonParameter(string name, object value) : base(name, value, ParameterType.RequestBody)
        {
            DataFormat  = DataFormat.Json;
            ContentType = Serialization.ContentType.Json;
        }

        public JsonParameter(string name, object value, string contentType) : base(name, value, ParameterType.RequestBody)
        {
            DataFormat  = DataFormat.Json;
            ContentType = contentType ?? Serialization.ContentType.Json;
        }
    }
}