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

using System.Text;
using System.Xml.Serialization;

namespace RestSharp.Serializers.Xml;

/// <summary>
/// Wrapper for System.Xml.Serialization.XmlSerializer.
/// </summary>
public class DotNetXmlSerializer : IXmlSerializer {
    /// <summary>
    /// Default constructor, does not specify namespace
    /// </summary>
    public DotNetXmlSerializer() {
        ContentType = ContentType.Xml;
        Encoding    = Encoding.UTF8;
    }

    /// <inheritdoc />
    /// <summary>
    /// Specify the namespaced to be used when serializing
    /// </summary>
    /// <param name="namespace">XML namespace</param>
    [PublicAPI]
    public DotNetXmlSerializer(string @namespace) : this() => Namespace = @namespace;

    /// <summary>
    /// Encoding for serialized content
    /// </summary>
    public Encoding Encoding { get; set; }

    /// <summary>
    /// Serialize the object as XML
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <returns>XML as string</returns>
    public string Serialize(object obj) {
        var ns = new XmlSerializerNamespaces();

        ns.Add(string.Empty, Namespace!);

        var serializer = GetXmlSerializer(obj.GetType(), RootElement);
        var writer     = new EncodingStringWriter(Encoding);

        serializer.Serialize(writer, obj, ns);

        return writer.ToString();
    }

    /// <summary>
    /// Name of the root element to use when serializing
    /// </summary>
    public string? RootElement { get; set; }

    /// <summary>
    /// XML namespace to use when serializing
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Content type for serialized content
    /// </summary>
    public ContentType ContentType { get; set; }

    static readonly Dictionary<(Type, string?), XmlSerializer> Cache     = new();
    static readonly ReaderWriterLockSlim                       CacheLock = new();

    static XmlSerializer GetXmlSerializer(Type type, string? rootElement) {
        XmlSerializer? serializer = null;

        var key = (type, rootElement);

        CacheLock.EnterReadLock();

        try {
            if (Cache.ContainsKey(key)) {
                serializer = Cache[key];
            }
        }
        finally {
            CacheLock.ExitReadLock();
        }

        if (serializer != null) {
            return serializer;
        }

        CacheLock.EnterWriteLock();

        try {
            // check again for a cached instance, because between the EnterWriteLock
            // and the last check, some other thread could have added an instance
            if (!Cache.ContainsKey(key)) {
                var root = rootElement == null ? null : new XmlRootAttribute(rootElement);

                Cache[key] = new XmlSerializer(type, root);
            }

            serializer = Cache[key];
        }
        finally {
            CacheLock.ExitWriteLock();
        }

        return serializer;
    }

    class EncodingStringWriter : StringWriter {
        // Need to subclass StringWriter in order to override Encoding
        public EncodingStringWriter(Encoding encoding) => Encoding = encoding;

        public override Encoding Encoding { get; }
    }
}
