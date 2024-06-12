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

namespace RestSharp.Serializers.Xml;

public class XmlRestSerializer(IXmlSerializer serializer, IXmlDeserializer deserializer) : IRestSerializer {
    public XmlRestSerializer() : this(new DotNetXmlSerializer(), new DotNetXmlDeserializer()) { }

    public ISerializer         Serializer           => serializer;
    public IDeserializer       Deserializer         => deserializer;
    public string[]            AcceptedContentTypes => ContentType.XmlAccept;
    public SupportsContentType SupportsContentType  => contentType => contentType.Value.EndsWith("xml", StringComparison.InvariantCultureIgnoreCase);

    public DataFormat DataFormat => DataFormat.Xml;

    public string? Serialize(Parameter parameter) {
        if (parameter is not XmlParameter xmlParameter)
            throw new ArgumentException("Supplied parameter is not an XML parameter", nameof(parameter));

        if (parameter.Value == null)
            throw new ArgumentNullException(nameof(parameter), "Parameter value is null");

        var savedNamespace = serializer.Namespace;
        serializer.Namespace = xmlParameter.XmlNamespace ?? savedNamespace;

        var result = serializer.Serialize(parameter.Value);

        serializer.Namespace = savedNamespace;

        return result;
    }

    public XmlRestSerializer WithXmlSerializer(IXmlSerializer xmlSerializer) {
        serializer = xmlSerializer;
        return this;
    }

    public XmlRestSerializer WithXmlDeserializer(IXmlDeserializer xmlDeserializer) {
        deserializer = xmlDeserializer;
        return this;
    }
}