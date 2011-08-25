#region License
//   Copyright 2010 John Sheehan
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

namespace RestSharp.Serializers
{
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;

    /// <summary>
    /// Wrapper for System.Xml.Serialization.XmlSerializer.
    /// </summary>
    public class DotNetXmlSerializer : ISerializer
    {
        /// <summary>
		/// Default constructor, does not specify namespace
		/// </summary>
        public DotNetXmlSerializer()
        {
            this.ContentType = "application/xml";
            this.Encoding = Encoding.UTF8;
		}

        /// <summary>
		/// Specify the namespaced to be used when serializing
		/// </summary>
		/// <param name="namespace">XML namespace</param>
        public DotNetXmlSerializer(string @namespace) : this()
        {
			Namespace = @namespace;
		}

        /// <summary>
        /// Serialize the object as XML
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>XML as string</returns>
        public string Serialize(object obj)
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, this.Namespace);
            var serializer = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            var writer = new EncodingStringWriter(this.Encoding);
            serializer.Serialize(writer, obj, ns);

            return writer.ToString();
        }

        /// <summary>
        /// Name of the root element to use when serializing
        /// </summary>
        public string RootElement { get; set; }

        /// <summary>
        /// XML namespace to use when serializing
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Format string to use when serializing dates
        /// </summary>
        public string DateFormat { get; set; }

        /// <summary>
        /// Content type for serialized content
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Encoding for serialized content
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Need to subclass StringWriter in order to override Encoding
        /// </summary>
        private class EncodingStringWriter : StringWriter
        {
            private readonly Encoding encoding;

            public EncodingStringWriter(Encoding encoding)
            {
                this.encoding = encoding;
            }

            public override Encoding Encoding
            {
                get
                {
                    return this.encoding;
                }
            }
        }
    }
}