using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using RestSharp.Serialization;
using RestSharp.Serialization.Xml;
using RestSharp.Serializers;

namespace RestSharp
{
    internal static class RestRequestExtensions
    {
        internal static void AddBody(this IHttp http, IEnumerable<Parameter> parameters)
        {
            var body = parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody);
            if (body == null) return;
            
            http.AddBody(body.ContentType, body.Value);
        }

        internal static void AddBody(this IHttp http, BodyParameter bodyParameter,
            IDictionary<DataFormat, IRestSerializer> serializers)
        {
            if (bodyParameter == null) return;

            if (!serializers.TryGetValue(bodyParameter.ParameterType, out var serializer))
            {
                throw new InvalidDataContractException($"Can't find serializer for content type {bodyParameter.ParameterType}");
            }
            
            http.AddBody(serializer.ContentType, serializer.Serialize(bodyParameter));
        }

        internal static void AddBody(this IHttp http, string contentType, object value)
        {
            // Only add the body if there aren't any files to make it a multipart form request
            // If there are files or AlwaysMultipartFormData = true, then add the body to the HTTP Parameters
            if (value == null) return;

            http.RequestContentType = contentType;

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
                http.Parameters.Add(new HttpParameter
                {
                    Name = contentType,
                    Value = value.ToString(),
                    ContentType = contentType
                });
            }
        }
    }
}