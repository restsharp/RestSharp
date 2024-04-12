// Copyright (c) .NET Foundation and Contributors
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

namespace RestSharp;

public static partial class RestRequestExtensions {
    /// <summary>
    /// Adds a body parameter to the request
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="obj">Object to be used as the request body, or string for plain content</param>
    /// <param name="contentType">Optional: content type</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Thrown if request body type cannot be resolved</exception>
    /// <remarks>This method will try to figure out the right content type based on the request data format and the provided content type</remarks>
    public static RestRequest AddBody(this RestRequest request, object obj, ContentType? contentType = null) {
        if (contentType == null) {
            return request.RequestFormat switch {
                DataFormat.Json   => request.AddJsonBody(obj, contentType),
                DataFormat.Xml    => request.AddXmlBody(obj, contentType),
                DataFormat.Binary => request.AddParameter(new BodyParameter("", obj, ContentType.Binary)),
                _                 => request.AddParameter(new BodyParameter("", obj.ToString()!, ContentType.Plain))
            };
        }

        return
            obj is string str                  ? request.AddStringBody(str, contentType) :
            obj is byte[] bytes                ? request.AddParameter(new BodyParameter("", bytes, contentType, DataFormat.Binary)) :
            contentType.Value.Contains("xml")  ? request.AddXmlBody(obj, contentType) :
            contentType.Value.Contains("json") ? request.AddJsonBody(obj, contentType) :
                                                 throw new ArgumentException("Non-string body found with unsupported content type", nameof(obj));
    }

    /// <summary>
    /// Adds a string body and figures out the content type from the data format specified. You can, for example, add a JSON string
    /// using this method as request body, using DataFormat.Json/>
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="body">String body</param>
    /// <param name="dataFormat"><see cref="DataFormat"/> for the content</param>
    /// <returns></returns>
    public static RestRequest AddStringBody(this RestRequest request, string body, DataFormat dataFormat) {
        var contentType = ContentType.FromDataFormat(dataFormat);
        request.RequestFormat = dataFormat;
        return request.AddParameter(new BodyParameter("", body, contentType));
    }

    /// <summary>
    /// Adds a string body to the request using the specified content type.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="body">String body</param>
    /// <param name="contentType">Content type of the body</param>
    /// <returns></returns>
    public static RestRequest AddStringBody(this RestRequest request, string body, ContentType contentType)
        => request.AddParameter(new BodyParameter(body, Ensure.NotNull(contentType, nameof(contentType))));

    /// <summary>
    /// Adds a JSON body parameter to the request from a string
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="forceSerialize">Force serialize the top-level string</param>
    /// <param name="contentType">Optional: content type. Default is ContentType.Json</param>
    /// <param name="jsonString">JSON string to be used as a body</param>
    /// <returns></returns>
    public static RestRequest AddJsonBody(this RestRequest request, string jsonString, bool forceSerialize, ContentType? contentType = null) {
        request.RequestFormat = DataFormat.Json;

        return !forceSerialize
            ? request.AddStringBody(jsonString, DataFormat.Json)
            : request.AddParameter(new JsonParameter(jsonString, contentType));
    }

    /// <summary>
    /// Adds a JSON body parameter to the request
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="obj">Object that will be serialized to JSON</param>
    /// <param name="contentType">Optional: content type. Default is ContentType.Json</param>
    /// <returns></returns>
    public static RestRequest AddJsonBody<T>(this RestRequest request, T obj, ContentType? contentType = null) where T : class {
        request.RequestFormat = DataFormat.Json;

        return obj is string str
            ? request.AddStringBody(str, DataFormat.Json)
            : request.AddParameter(new JsonParameter(obj, contentType));
    }

    /// <summary>
    /// Adds an XML body parameter to the request
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="obj">Object that will be serialized to XML</param>
    /// <param name="contentType">Optional: content type. Default is ContentType.Xml</param>
    /// <param name="xmlNamespace">Optional: XML namespace</param>
    /// <returns></returns>
    public static RestRequest AddXmlBody<T>(this RestRequest request, T obj, ContentType? contentType = null, string xmlNamespace = "")
        where T : class {
        request.RequestFormat = DataFormat.Xml;

        return obj is string str
            ? request.AddStringBody(str, DataFormat.Xml)
            : request.AddParameter(new XmlParameter(obj, xmlNamespace, contentType));
    }
    static RestRequest AddBodyParameter(this RestRequest request, string? name, object value)
        => name != null && name.Contains('/')
            ? request.AddBody(value, name)
            : request.AddParameter(new BodyParameter(name, value, ContentType.Plain));
}