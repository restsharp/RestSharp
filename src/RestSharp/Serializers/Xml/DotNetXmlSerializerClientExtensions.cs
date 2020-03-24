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

using System.Text;
using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace RestSharp.Serialization.Xml
{
    public static class DotNetXmlSerializerClientExtensions
    {
        public static IRestClient UseDotNetXmlSerializer(
            this IRestClient restClient,
            string xmlNamespace = null,
            Encoding encoding = null
        )
        {
            var xmlSerializer                                 = new DotNetXmlSerializer();
            if (xmlNamespace != null) xmlSerializer.Namespace = xmlNamespace;
            if (encoding     != null) xmlSerializer.Encoding  = encoding;

            var xmlDeserializer                            = new DotNetXmlDeserializer();
            if (encoding != null) xmlDeserializer.Encoding = encoding;

            var serializer = new XmlRestSerializer()
                .WithXmlSerializer(xmlSerializer)
                .WithXmlDeserializer(xmlDeserializer);

            return restClient.UseSerializer(() => serializer);
        }
    }
}