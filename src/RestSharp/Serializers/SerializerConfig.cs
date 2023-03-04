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

using RestSharp.Serializers.Json;
using RestSharp.Serializers.Xml;

namespace RestSharp.Serializers;

public class SerializerConfig {
    internal RestClient Client { get; }

    internal Dictionary<DataFormat, SerializerRecord> Serializers { get; } = new();

    internal SerializerConfig(RestClient client) => Client = client;

    /// <summary>
    /// Replace the default serializer with a custom one
    /// </summary>
    /// <param name="serializerFactory">Function that returns the serializer instance</param>
    public SerializerConfig UseSerializer(Func<IRestSerializer> serializerFactory) {
        var instance = serializerFactory();

        Serializers[instance.DataFormat] = new SerializerRecord(
            instance.DataFormat,
            instance.AcceptedContentTypes,
            instance.SupportsContentType,
            serializerFactory
        );
        Client.AssignAcceptedContentTypes(this);
        return this;
    }

    public void UseDefaultSerializers() => UseSerializer<SystemTextJsonSerializer>().UseSerializer<XmlRestSerializer>();

    /// <summary>
    /// Replace the default serializer with a custom one
    /// </summary>
    /// <typeparam name="T">The type that implements <see cref="IRestSerializer"/></typeparam>
    /// <returns></returns>
    public SerializerConfig UseSerializer<T>() where T : class, IRestSerializer, new() => UseSerializer(() => new T());

    internal string[] GetAcceptedContentTypes() => Serializers.SelectMany(x => x.Value.AcceptedContentTypes).Distinct().ToArray();
}

public static class SerializerConfigExtensions {
    /// <summary>
    /// Sets the <see cref="SerializerConfig"/> to only use JSON
    /// </summary>
    /// <param name="config">Configuration instance to work with</param>
    /// <returns>Reference to the client instance</returns>
    public static SerializerConfig UseJson(this SerializerConfig config) {
        config.Serializers.Remove(DataFormat.Xml);
        config.Client.AssignAcceptedContentTypes(config);
        return config;
    }

    /// <summary>
    /// Sets the <see cref="SerializerConfig"/> to only use XML
    /// </summary>
    /// <param name="config">Configuration instance to work with</param>
    /// <returns>Reference to the client instance</returns>
    public static SerializerConfig UseXml(this SerializerConfig config) {
        config.Serializers.Remove(DataFormat.Json);
        config.Client.AssignAcceptedContentTypes(config);
        return config;
    }

    /// <summary>
    /// Sets the <see cref="SerializerConfig"/> to only use the passed in custom serializer
    /// </summary>
    /// <param name="config">Configuration instance to work with</param>
    /// <param name="serializerFactory">Function that returns the serializer instance</param>
    /// <returns>Reference to the client instance</returns>
    public static SerializerConfig UseOnlySerializer(this SerializerConfig config, Func<IRestSerializer> serializerFactory) {
        config.Serializers.Clear();
        config.UseSerializer(serializerFactory);
        return config;
    }
}
