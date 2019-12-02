using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using RestSharp.Serialization;
using RestSharp.Serializers;

namespace RestSharp
{
    internal static class RestRequestExtensions
    {
        internal static void AddBody(
            this IHttp http,
            IEnumerable<Parameter> parameters,
            IDictionary<DataFormat, IRestSerializer> restSerializers,
            params ISerializer[] serializers
        )
        {
            var body = parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody);
            if (body == null) return;

            if (body.DataFormat == DataFormat.None)
            {
                http.AddBody(body.ContentType, body.Name, body.Value);
            }
            else
            {
                var contentType       = body.ContentType ?? ContentType.FromDataFormat[body.DataFormat];
                var requestSerializer = serializers.FirstOrDefault(x => x != null && x.ContentType == contentType);

                if (requestSerializer != null)
                {
                    http.AddBody(
                        requestSerializer.ContentType, requestSerializer.ContentType,
                        requestSerializer.Serialize(body.Value)
                    );
                }
                else
                {
                    if (!restSerializers.TryGetValue(body.DataFormat, out var serializer))
                        throw new InvalidDataContractException(
                            $"Can't find serializer for content type {body.DataFormat}"
                        );

                    http.AddBody(serializer.ContentType, serializer.ContentType, serializer.Serialize(body));
                }
            }
        }

        internal static void AddBody(this IHttp http, string contentType, string name, object value)
        {
            // Only add the body if there aren't any files to make it a multipart form request
            // If there are files or AlwaysMultipartFormData = true, then add the body to the HTTP Parameters
            if (value == null) return;

            http.RequestContentType = name;

            if (!http.AlwaysMultipartFormData && !http.Files.Any())
            {
                var val = value;

                if (val is byte[] bytes)
                    http.RequestBodyBytes = bytes;
                else
                    http.RequestBody = value.ToString();
            }
            else
            {
                http.Parameters.Add(
                    new HttpParameter
                    {
                        Name        = name,
                        Value       = value.ToString(),
                        ContentType = contentType
                    }
                );
            }
        }
    }
}