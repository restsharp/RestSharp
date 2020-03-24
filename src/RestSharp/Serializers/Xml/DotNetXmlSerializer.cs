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

using System.IO;
using System.Text;
using System.Xml.Serialization;
using RestSharp.Serialization.Xml;

namespace RestSharp.Serializers
{
    /// <summary>
    ///     Wrapper for System.Xml.Serialization.XmlSerializer.
    /// </summary>
    public class DotNetXmlSerializer : IXmlSerializer
    {
        /// <summary>
        ///     Default constructor, does not specify namespace
        /// </summary>
        public DotNetXmlSerializer()
        {
            ContentType = Serialization.ContentType.Xml;
            Encoding    = Encoding.UTF8;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Specify the namespaced to be used when serializing
        /// </summary>
        /// <param name="namespace">XML namespace</param>
        public DotNetXmlSerializer(string @namespace)
            : this()
            => Namespace = @namespace;

        /// <summary>
        ///     Encoding for serialized content
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        ///     Serialize the object as XML
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>XML as string</returns>
        public string Serialize(object obj)
        {
            var ns = new XmlSerializerNamespaces();

            ns.Add(string.Empty, Namespace);

            var serializer = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            var writer     = new EncodingStringWriter(Encoding);

            serializer.Serialize(writer, obj, ns);

            return writer.ToString();
        }

        /// <summary>
        ///     Name of the root element to use when serializing
        /// </summary>
        public string RootElement { get; set; }

        /// <summary>
        ///     XML namespace to use when serializing
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        ///     Format string to use when serializing dates
        /// </summary>
        public string DateFormat { get; set; }

        /// <summary>
        ///     Content type for serialized content
        /// </summary>
        public string ContentType { get; set; }

        class EncodingStringWriter : StringWriter
        {
            // Need to subclass StringWriter in order to override Encoding
            public EncodingStringWriter(Encoding encoding) => Encoding = encoding;

            public override Encoding Encoding { get; }
        }
    }
}