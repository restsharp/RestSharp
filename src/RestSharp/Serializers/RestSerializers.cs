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

public class RestSerializers(Dictionary<DataFormat, SerializerRecord> records) {
    [PublicAPI]
    public IReadOnlyDictionary<DataFormat, SerializerRecord> Serializers { get; } = new ReadOnlyDictionary<DataFormat, SerializerRecord>(records);

    public RestSerializers(SerializerConfig config) : this(config.Serializers) { }

    public IRestSerializer GetSerializer(DataFormat dataFormat)
        => Serializers.TryGetValue(dataFormat, out var value)
            ? value.GetSerializer()
            : throw new InvalidOperationException($"Unable to find a serializer for {dataFormat}");

    internal string[] GetAcceptedContentTypes() => Serializers.SelectMany(x => x.Value.AcceptedContentTypes).Distinct().ToArray();

    internal async ValueTask<RestResponse<T>> Deserialize<T>(RestRequest request, RestResponse raw, ReadOnlyRestClientOptions options, CancellationToken cancellationToken) {
        var response = RestResponse<T>.FromResponse(raw);

        try {
            await OnBeforeDeserialization(raw, cancellationToken).ConfigureAwait(false);
#pragma warning disable CS0618 // Type or member is obsolete
            request.OnBeforeDeserialization?.Invoke(raw);
#pragma warning restore CS0618 // Type or member is obsolete
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
   
    static async ValueTask OnBeforeDeserialization(RestResponse response, CancellationToken cancellationToken) {
        if (response.Request.Interceptors == null) return;

        foreach (var interceptor in response.Request.Interceptors) {
            await interceptor.BeforeDeserialization(response, cancellationToken).ConfigureAwait(false);
        }
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

        if (deserializer is not IXmlDeserializer xml || response.Request is not RestXmlRequest xmlRequest)
            return deserializer != null ? deserializer.Deserialize<T>(response) : default;

        if (xmlRequest.XmlNamespace.IsNotEmpty()) xml.Namespace = xmlRequest.XmlNamespace!;

        if (xml is IWithDateFormat withDateFormat && xmlRequest.DateFormat.IsNotEmpty()) withDateFormat.DateFormat = xmlRequest.DateFormat!;

        return deserializer.Deserialize<T>(response);
    }

    IDeserializer? GetContentDeserializer(RestResponseBase response) {
        if (string.IsNullOrWhiteSpace(response.Content)) return null;

        var contentType = response.ContentType ?? DetectContentType()?.Value;

        if (contentType == null) {
            Serializers.TryGetValue(response.Request.RequestFormat, out var serializerByRequestFormat);
            return serializerByRequestFormat?.GetSerializer().Deserializer;
        }

        var serializer = Serializers.Values.FirstOrDefault(x => x.SupportsContentType(contentType));

        // ReSharper disable once InvertIf
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
