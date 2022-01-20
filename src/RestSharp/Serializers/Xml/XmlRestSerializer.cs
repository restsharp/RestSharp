//   Copyright © 2009-2021 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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

public class XmlRestSerializer : IRestSerializer {
    IXmlDeserializer _xmlDeserializer;
    IXmlSerializer   _xmlSerializer;

    public XmlRestSerializer() : this(new DotNetXmlSerializer(), new DotNetXmlDeserializer()) { }

    public XmlRestSerializer(IXmlSerializer xmlSerializer, IXmlDeserializer xmlDeserializer) {
        _xmlDeserializer = xmlDeserializer;
        _xmlSerializer   = xmlSerializer;
    }

    public ISerializer         Serializer           => _xmlSerializer;
    public IDeserializer       Deserializer         => _xmlDeserializer;
    public string[]            AcceptedContentTypes => ContentType.XmlAccept;
    public SupportsContentType SupportsContentType  => contentType => contentType.EndsWith("xml", StringComparison.InvariantCultureIgnoreCase);

    public DataFormat DataFormat => DataFormat.Xml;

    public string? Serialize(Parameter parameter) {
        if (parameter is not XmlParameter xmlParameter)
            throw new ArgumentException("Supplied parameter is not an XML parameter", nameof(parameter));

        if (parameter.Value == null)
            throw new ArgumentNullException(nameof(parameter), "Parameter value is null");

        var savedNamespace = _xmlSerializer.Namespace;
        _xmlSerializer.Namespace = xmlParameter.XmlNamespace ?? savedNamespace;

        var result = _xmlSerializer.Serialize(parameter.Value);

        _xmlSerializer.Namespace = savedNamespace;

        return result;
    }

    public XmlRestSerializer WithXmlSerializer(IXmlSerializer xmlSerializer) {
        _xmlSerializer = xmlSerializer;
        return this;
    }

    public XmlRestSerializer WithXmlDeserializer(IXmlDeserializer xmlDeserializer) {
        _xmlDeserializer = xmlDeserializer;
        return this;
    }
}