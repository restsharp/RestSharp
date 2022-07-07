//  Copyright © 2009-2021 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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

using System.Net;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using RestSharp.Extensions;
using static RestSharp.KnownHeaders;

// ReSharper disable InvertIf
// ReSharper disable SuggestBaseTypeForParameter

namespace RestSharp;

class RequestContent : IDisposable {
    readonly RestClient   _client;
    readonly RestRequest  _request;
    readonly List<Stream> _streams = new();

    HttpContent? Content { get; set; }

    public RequestContent(RestClient client, RestRequest request) {
        _client  = client;
        _request = request;
    }

    public HttpContent BuildContent() {
        AddFiles();
        var postParameters = _request.Parameters.GetContentParameters(_request.Method);
        AddBody(!postParameters.IsEmpty());
        AddPostParameters(postParameters);
        AddHeaders();

        return Content!;
    }

    void AddFiles() {
        if (!_request.HasFiles() && !_request.AlwaysMultipartFormData) return;

        var mpContent = new MultipartFormDataContent(GetOrSetFormBoundary());

        foreach (var file in _request.Files) {
            var stream = file.GetFile();
            _streams.Add(stream);
            var fileContent = new StreamContent(stream);

            if (file.ContentType != null) fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);

            var dispositionHeader = file.Options.DisableFilenameEncoding
                ? ContentDispositionHeaderValue.Parse($"form-data; name=\"{file.Name}\"; filename=\"{file.FileName}\"")
                : new ContentDispositionHeaderValue("form-data") {
                    Name     = $"\"{file.Name}\"",
                    FileName = $"\"{file.FileName}\""
                };
            if (!file.Options.DisableFileNameStar) dispositionHeader.FileNameStar = file.FileName;
            fileContent.Headers.ContentDisposition = dispositionHeader;

            mpContent.Add(fileContent);
        }

        Content = mpContent;
    }

    HttpContent Serialize(BodyParameter body) {
        return body.DataFormat switch {
            DataFormat.None   => new StringContent(body.Value!.ToString()!, _client.Options.Encoding, body.ContentType),
            DataFormat.Binary => GetBinary(),
            _                 => GetSerialized()
        };

        HttpContent GetBinary() {
            var byteContent = new ByteArrayContent((body.Value as byte[])!);
            byteContent.Headers.ContentType = MediaTypeHeaderValue.Parse(body.ContentType);

            if (body.ContentEncoding != null) {
                byteContent.Headers.ContentEncoding.Clear();
                byteContent.Headers.ContentEncoding.Add(body.ContentEncoding);
            }

            return byteContent;
        }

        HttpContent GetSerialized() {
            if (!_client.Serializers.TryGetValue(body.DataFormat, out var serializerRecord))
                throw new InvalidDataContractException(
                    $"Can't find serializer for content type {body.DataFormat}"
                );

            var serializer = serializerRecord.GetSerializer();

            var content = serializer.Serialize(body);

            if (content == null) throw new SerializationException("Request body serialized to null");

            return new StringContent(
                content,
                _client.Options.Encoding,
                body.ContentType ?? serializer.Serializer.ContentType
            );
        }
    }

    static bool BodyShouldBeMultipartForm(BodyParameter bodyParameter) {
        var bodyContentType = bodyParameter.ContentType ?? bodyParameter.Name;
        return bodyParameter.Name.IsNotEmpty() && bodyParameter.Name != bodyContentType;
    }

    string GetOrSetFormBoundary() => _request.FormBoundary ?? (_request.FormBoundary = Guid.NewGuid().ToString());

    void AddBody(bool hasPostParameters) {
        if (!_request.TryGetBodyParameter(out var bodyParameter)) return;

        var bodyContent = Serialize(bodyParameter!);

        // we need to send the body
        if (hasPostParameters || _request.HasFiles() || BodyShouldBeMultipartForm(bodyParameter!) || _request.AlwaysMultipartFormData) {
            // here we must use multipart form data
            var mpContent = Content as MultipartFormDataContent ?? new MultipartFormDataContent(GetOrSetFormBoundary());
            var ct        = bodyContent.Headers.ContentType?.MediaType;
            var name      = bodyParameter!.Name.IsEmpty() ? ct : bodyParameter.Name;

            if (name.IsEmpty())
                mpContent.Add(bodyContent);
            else
                mpContent.Add(bodyContent, name!);
            Content = mpContent;
        }
        else {
            // we don't have parameters, only the body
            Content = bodyContent;
        }

        if (_client.Options.DisableCharset) {
            Content.Headers.ContentType!.CharSet = "";
        }
    }

    void AddPostParameters(ParametersCollection? postParameters) {
        if (postParameters.IsEmpty()) return;

        if (Content is MultipartFormDataContent mpContent) {
            // we got the multipart form already instantiated, just add parameters to it
            foreach (var postParameter in postParameters!) {
                var parameterName = postParameter.Name!;

                mpContent.Add(
                    new StringContent(postParameter.Value!.ToString()!, _client.Options.Encoding, postParameter.ContentType),
                    _request.MultipartFormQuoteParameters ? $"\"{parameterName}\"" : parameterName
                );
            }
        }
        else {
#if NETCORE
            // We should not have anything else except the parameters, so we send them as form URL encoded.
            var formContent = new FormUrlEncodedContent(
                _request.Parameters
                    .Where(x => x.Type == ParameterType.GetOrPost)
                    .Select(x => new KeyValuePair<string, string>(x.Name!, x.Value!.ToString()!))!
            );
            Content = formContent;
#else
            // However due to bugs in HttpClient FormUrlEncodedContent (see https://github.com/restsharp/RestSharp/issues/1814) we
            // do the encoding ourselves using WebUtility.UrlEncode instead.
            var formData = _request.Parameters
                .Where(x => x.Type == ParameterType.GetOrPost)
                .Select(x => new KeyValuePair<string, string>(x.Name!, x.Value!.ToString()!))!;
            var encodedItems   = formData.Select(i => $"{WebUtility.UrlEncode(i.Key)}={WebUtility.UrlEncode(i.Value)}" /*.Replace("%20", "+")*/);
            var encodedContent = new StringContent(string.Join("&", encodedItems), null, "application/x-www-form-urlencoded");

            Content = encodedContent;
#endif
        }
    }

    void AddHeaders() {
        var contentHeaders = _request.Parameters
            .Where(x => x.Type == ParameterType.HttpHeader && IsContentHeader(x.Name!))
            .ToArray();

        if (contentHeaders.Length > 0 && Content == null) {
            // We need some content to add content headers to it, so if necessary, we'll add empty content
            Content = new StringContent("");
        }

        contentHeaders.ForEach(AddHeader);

        void AddHeader(Parameter parameter) {
            var parameterStringValue = parameter.Value!.ToString();

            var value = parameter.Name switch {
                ContentType => GetContentTypeHeader(Ensure.NotNull(parameterStringValue, nameof(parameter))),
                _           => parameterStringValue
            };
            var pName = Ensure.NotNull(parameter.Name, nameof(parameter.Name));
            ReplaceHeader(pName, value);
        }

        string GetContentTypeHeader(string contentType)
            => Content is MultipartFormDataContent
                ? $"{contentType}; boundary=\"{GetOrSetFormBoundary()}\""
                : contentType;
    }

    void ReplaceHeader(string name, string? value) {
        Content!.Headers.Remove(name);
        Content!.Headers.TryAddWithoutValidation(name, value);
    }

    public void Dispose() {
        _streams.ForEach(x => x.Dispose());
        Content?.Dispose();
    }
}
