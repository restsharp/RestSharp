﻿using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using RestSharp.Serialization.Xml;

namespace RestSharp.Deserializers
{
    /// <summary>
    ///     Wrapper for System.Xml.Serialization.XmlSerializer.
    /// </summary>
    public class DotNetXmlDeserializer : IXmlDeserializer
    {
        public bool UseBytes { get; } = false;


        /// <summary>
        ///     Encoding for serialized content
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        /// <summary>
        ///     Name of the root element to use when serializing
        /// </summary>
        public string RootElement { get; set; }

        /// <summary>
        ///     XML namespace to use when serializing
        /// </summary>
        public string Namespace { get; set; }

        public string DateFormat { get; set; }

        public T Deserialize<T>(IRestResponse response)
            => this.Deserialize<T>(response.Content);

        public T Deserialize<T>(string payload)
        {
            if (string.IsNullOrEmpty(payload)) return default;

            using (var stream = new MemoryStream(Encoding.GetBytes(payload)))
            {
                var serializer = new XmlSerializer(typeof(T));

                return (T)serializer.Deserialize(stream);
            }
        }

        public T DeserializeFromBytes<T>(byte[] payload)
            => throw new NotSupportedException("Deserialize DotNetXml from byte[] array is not supported!");
    }
}