//  Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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

using System.Net.Http.Headers;
using System.Runtime.Serialization;
using RestSharp.Extensions;
using static RestSharp.KnownHeaders;

namespace RestSharp;

class RequestContent : IDisposable {
    readonly RestClient   _client;
    readonly RestRequest  _request;
    readonly List<Stream> _streams = new();

    internal static readonly string[] ContentHeaders = { ContentType, ContentEncoding, ContentDisposition };

    HttpContent? Content { get; set; }

    public RequestContent(RestClient client, RestRequest request) {
        _client  = client;
        _request = request;
    }

    public HttpContent BuildContent() {
        AddFiles();
        AddBody();
        AddPostParameters();
        AddHeaders();
        return Content!;
    }

    void AddFiles() {
        if (!_request.HasFiles() && !_request.AlwaysMultipartFormData) return;

        var mpContent = new MultipartFormDataContent();

        foreach (var file in _request.Files) {
            var stream = file.GetFile();
            _streams.Add(stream);
            var fileContent = new StreamContent(stream);

            if (file.ContentType != null)
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
            mpContent.Add(fileContent, file.Name, file.FileName);
        }

        Content = mpContent;
    }

    HttpContent Serialize(Parameter body) {
        return body.DataFormat switch {
            DataFormat.None => new StringContent(body.Value!.ToString()!, _client.Options.Encoding, body.ContentType),
            _               => GetSerialized()
        };

        HttpContent GetSerialized() {
            if (!_client.Serializers.TryGetValue(body.DataFormat, out var serializer))
                throw new InvalidDataContractException(
                    $"Can't find serializer for content type {body.DataFormat}"
                );

            var content = serializer.Serialize(body);

            if (content == null)
                throw new SerializationException("Request body serialized to null");

            return new StringContent(
                content,
                _client.Options.Encoding,
                body.ContentType ?? serializer.ContentType
            );
        }
    }

    static bool BodyShouldBeMultipartForm(Parameter bodyParameter) {
        var bodyContentType = bodyParameter.ContentType ?? bodyParameter.Name;
        return bodyParameter.Name.IsNotEmpty() && bodyParameter.Name != bodyContentType;
    }

    void AddBody() {
        if (!_request.TryGetBodyParameter(out var bodyParameter)) return;

        var bodyContent = Serialize(bodyParameter!);

        // we need to send the body
        if (_request.HasPostParameters() || _request.HasFiles() || BodyShouldBeMultipartForm(bodyParameter!)) {
            // here we must use multipart form data
            var mpContent = Content as MultipartFormDataContent ?? new MultipartFormDataContent();

            if (bodyParameter!.Name.IsEmpty())
                mpContent.Add(bodyContent);
            else
                mpContent.Add(bodyContent, bodyParameter.Name!);
            Content = mpContent;
        }
        else {
            // we don't have parameters, only the body
            Content = bodyContent;
        }
    }

    void AddPostParameters() {
        var postParameters = _request.GetPostParameters();
        if (postParameters.Length <= 0) return;

        // it's a form
        if (Content is MultipartFormDataContent mpContent) {
            // we got the multipart form already instantiated, just add parameters to it
            foreach (var postParameter in postParameters) {
                mpContent.Add(
                    new StringContent(postParameter.Value!.ToString()!, _client.Options.Encoding, postParameter.ContentType),
                    postParameter.Name!
                );
            }
        }
        else {
            // we should not have anything else except the parameters, so we send them as form URL encoded
            var formContent = new FormUrlEncodedContent(
                _request.Parameters
                    .Where(x => x.Type == ParameterType.GetOrPost)
                    .Select(x => new KeyValuePair<string, string>(x.Name!, x.Value!.ToString()!))!
            );
            Content = formContent;
        }
    }

    void AddHeaders() {
        _request.Parameters
            .Where(x => x.Type == ParameterType.HttpHeader && ContentHeaders.Contains(x.Name))
            .ForEach(AddHeader);

        void AddHeader(Parameter parameter) {
            var parameterStringValue = parameter.Value!.ToString();

            var value = parameter.Name switch {
                ContentType => GetContentTypeHeader(Ensure.NotNull(parameterStringValue, nameof(parameter))),
                _           => parameterStringValue
            };
            var pName = Ensure.NotNull(parameter.Name, nameof(parameter.Name));
            Content!.Headers.Remove(pName);
            Content!.Headers.TryAddWithoutValidation(pName, value);
        }
    }

    string GetContentTypeHeader(string contentType) {
        if (Content == null)
            throw new InvalidRequestException("Content type headers should not be used when there's no body in the request");

        var boundary = Content.GetFormBoundary();
        return boundary.IsEmpty() ? contentType : $"{contentType}; boundary=\"{boundary}\"";
    }

    public void Dispose() => _streams.ForEach(x => x.Dispose());
}