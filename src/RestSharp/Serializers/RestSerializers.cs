//  Copyright (c) .NET Foundation and Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System.Collections.ObjectModel;
using RestSharp.Extensions;
using RestSharp.Serializers.Xml;

namespace RestSharp.Serializers;

public class RestSerializers {
    public IReadOnlyDictionary<DataFormat, SerializerRecord> Serializers { get; }

    public RestSerializers(Dictionary<DataFormat, SerializerRecord> records)
        => Serializers = new ReadOnlyDictionary<DataFormat, SerializerRecord>(records);

    public RestSerializers(SerializerConfig config) : this(config.Serializers) { }

    public IRestSerializer GetSerializer(DataFormat dataFormat)
        => Serializers.TryGetValue(dataFormat, out var value)
            ? value.GetSerializer()
            : throw new InvalidOperationException($"Unable to find a serializer for {dataFormat}");

    internal string[] GetAcceptedContentTypes() => Serializers.SelectMany(x => x.Value.AcceptedContentTypes).Distinct().ToArray();

    internal RestResponse<T> Deserialize<T>(RestRequest request, RestResponse raw, ReadOnlyRestClientOptions options) {
        var response = RestResponse<T>.FromResponse(raw);

        try {
            request.OnBeforeDeserialization?.Invoke(raw);
            response.Data = DeserializeContent<T>(raw);
        }
        catch (Exception ex) {
            if (options.ThrowOnAnyError) throw;

            if (options.FailOnDeserializationError || options.ThrowOnDeserializationError) response.ResponseStatus = ResponseStatus.Error;

            response.AddException(ex);

            if (options.ThrowOnDeserializationError) throw new DeserializationException(response, ex);
        }

        return response;
    }
   

    /// <summary>
    /// Deserialize the response content into the specified type
    /// </summary>
    /// <param name="response">Response instance</param>
    /// <typeparam name="T">Deserialized model type</typeparam>
    /// <returns></returns>
    [PublicAPI]
    public T? DeserializeContent<T>(RestResponse response) {
        // Only attempt to deserialize if the request has not errored due
        // to a transport or framework exception.  HTTP errors should attempt to
        // be deserialized
        if (response.Content == null) {
            return default;
        }

        // Only continue if there is a handler defined else there is no way to deserialize the data.
        // This can happen when a request returns for example a 404 page instead of the requested JSON/XML resource
        var deserializer = GetContentDeserializer(response);

        if (deserializer is IXmlDeserializer xml && response.Request is RestXmlRequest xmlRequest) {
            if (xmlRequest.XmlNamespace.IsNotEmpty()) xml.Namespace = xmlRequest.XmlNamespace!;

            if (xml is IWithDateFormat withDateFormat && xmlRequest.DateFormat.IsNotEmpty()) withDateFormat.DateFormat = xmlRequest.DateFormat!;
        }

        return deserializer != null ? deserializer.Deserialize<T>(response) : default;
    }

    IDeserializer? GetContentDeserializer(RestResponseBase response) {
        if (string.IsNullOrWhiteSpace(response.Content)) return null;

        var contentType = response.ContentType ?? DetectContentType()?.Value;

        if (contentType == null) {
            Serializers.TryGetValue(response.Request.RequestFormat, out var serializerByRequestFormat);
            return serializerByRequestFormat?.GetSerializer().Deserializer;
        }

        var serializer = Serializers.Values.FirstOrDefault(x => x.SupportsContentType(contentType));

        if (serializer == null) {
            var detectedType = DetectContentType()?.Value;

            if (detectedType != null && detectedType != contentType)
            {
                serializer = Serializers.Values.FirstOrDefault(x => x.SupportsContentType(detectedType));
            }
        }

        return serializer?.GetSerializer().Deserializer;

        ContentType? DetectContentType()
            => response.Content![0] switch {
                '<'        => ContentType.Xml,
                '{' or '[' => ContentType.Json,
                _          => null
            };
    }
}
