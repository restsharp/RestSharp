using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using RestSharp.Serialization;
using RestSharp.Serializers;

namespace RestSharp
{
    internal static class RestRequestExtensions
    {
        internal static void SerializeRequestBody(
            this IRestRequest request,
            IDictionary<DataFormat, IRestSerializer> restSerializers,
            params ISerializer[] serializers
        )
        {
            var body = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody);
            if (body == null) return;

            if (body.DataFormat == DataFormat.None)
            {
                 request.Body = new RequestBody(body.ContentType, body.Name, body.Value);
                 return;
            }

            var contentType       = body.ContentType ?? ContentType.FromDataFormat[body.DataFormat];
            var requestSerializer = serializers.FirstOrDefault(x => x != null && x.ContentType == contentType);

            if (requestSerializer != null)
            {
                request.Body = new RequestBody(
                    requestSerializer.ContentType, requestSerializer.ContentType,
                    requestSerializer.Serialize(body.Value)
                );
                return;
            }

            if (!restSerializers.TryGetValue(body.DataFormat, out var serializer))
                throw new InvalidDataContractException(
                    $"Can't find serializer for content type {body.DataFormat}"
                );

            request.Body = new RequestBody(serializer.ContentType, serializer.ContentType, serializer.Serialize(body));
        }

        internal static void AddBody(this IHttp http, RequestBody requestBody)
        {
            // Only add the body if there aren't any files to make it a multipart form request
            // If there are files or AlwaysMultipartFormData = true, then add the body to the HTTP Parameters
            if (requestBody.Value == null) return;

            http.RequestContentType = string.IsNullOrWhiteSpace(requestBody.Name)
                ? requestBody.ContentType
                : requestBody.Name;

            if (!http.AlwaysMultipartFormData && !http.Files.Any())
            {
                var val = requestBody.Value;

                if (val is byte[] bytes)
                    http.RequestBodyBytes = bytes;
                else
                    http.RequestBody = requestBody.Value.ToString();
            }
            else
            {
                http.Parameters.Add(
                    new HttpParameter
                    {
                        Name        = requestBody.Name,
                        Value       = requestBody.Value.ToString(),
                        ContentType = requestBody.ContentType
                    }
                );
            }
        }
    }
}