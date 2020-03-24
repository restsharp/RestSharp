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

namespace RestSharp.Deserializers
{
    /// <summary>
    ///     Wrapper for System.Xml.Serialization.XmlSerializer.
    /// </summary>
    public class DotNetXmlDeserializer : IXmlDeserializer
    {
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
        {
            if (string.IsNullOrEmpty(response.Content)) return default;

            using (var stream = new MemoryStream(Encoding.GetBytes(response.Content)))
            {
                var serializer = new XmlSerializer(typeof(T));

                return (T) serializer.Deserialize(stream);
            }
        }
    }
}