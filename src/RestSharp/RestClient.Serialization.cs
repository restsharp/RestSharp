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

using RestSharp.Extensions;
using RestSharp.Serializers;
using RestSharp.Serializers.Json;
using RestSharp.Serializers.Xml;

// ReSharper disable VirtualMemberCallInConstructor
#pragma warning disable 618

namespace RestSharp;

public partial class RestClient {
    internal Dictionary<DataFormat, SerializerRecord> Serializers { get; } = new();

    /// <summary>
    /// Replace the default serializer with a custom one
    /// </summary>
    /// <param name="serializerFactory">Function that returns the serializer instance</param>
    public RestClient UseSerializer(Func<IRestSerializer> serializerFactory) {
        var instance = serializerFactory();

        Serializers[instance.DataFormat] = new SerializerRecord(
            instance.DataFormat,
            instance.AcceptedContentTypes,
            instance.SupportsContentType,
            serializerFactory
        );
        AssignAcceptedContentTypes();
        return this;
    }

    public void UseDefaultSerializers() => UseSerializer<SystemTextJsonSerializer>().UseSerializer<XmlRestSerializer>();

    /// <summary>
    /// Replace the default serializer with a custom one
    /// </summary>
    /// <typeparam name="T">The type that implements <see cref="IRestSerializer"/></typeparam>
    /// <returns></returns>
    public RestClient UseSerializer<T>() where T : class, IRestSerializer, new() => UseSerializer(() => new T());

    internal RestResponse<T> Deserialize<T>(RestRequest request, RestResponse raw) {
        var response = RestResponse<T>.FromResponse(raw);

        try {
            request.OnBeforeDeserialization?.Invoke(raw);

            // Only attempt to deserialize if the request has not errored due
            // to a transport or framework exception.  HTTP errors should attempt to
            // be deserialized
            if (response.Content != null) {
                // Only continue if there is a handler defined else there is no way to deserialize the data.
                // This can happen when a request returns for example a 404 page instead of the requested JSON/XML resource
                var handler = GetContentDeserializer(raw, request.RequestFormat);

                if (handler is IXmlDeserializer xml && request is RestXmlRequest xmlRequest) {
                    if (xmlRequest.XmlNamespace.IsNotEmpty()) xml.Namespace = xmlRequest.XmlNamespace!;

                    if (xml is IWithDateFormat withDateFormat && xmlRequest.DateFormat.IsNotEmpty())
                        withDateFormat.DateFormat = xmlRequest.DateFormat!;
                }

                if (handler != null) response.Data = handler.Deserialize<T>(raw);
            }
        }
        catch (Exception ex) {
            if (Options.ThrowOnAnyError) throw;

            if (Options.FailOnDeserializationError || Options.ThrowOnDeserializationError) response.ResponseStatus = ResponseStatus.Error;

            response.ErrorMessage   = ex.Message;
            response.ErrorException = ex;

            if (Options.ThrowOnDeserializationError) throw new DeserializationException(response, ex);
        }

        response.Request = request;

        return response;
    }

    IDeserializer? GetContentDeserializer(RestResponseBase response, DataFormat requestFormat) {
        var contentType = response.ContentType ?? DetectContentType();
        if (contentType == null) return null;

        var serializer = Serializers.Values.FirstOrDefault(x => x.SupportsContentType(contentType));
        var factory    = serializer ?? (Serializers.ContainsKey(requestFormat) ? Serializers[requestFormat] : null);
        return factory?.GetSerializer().Deserializer;
        
        string? DetectContentType()
            => response.Content!.StartsWith("<") ? ContentType.Xml 
                : response.Content.StartsWith("{") || response.Content.StartsWith("[") ? ContentType.Json : null;
    }
}