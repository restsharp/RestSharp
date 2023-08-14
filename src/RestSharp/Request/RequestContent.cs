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

using System.Net.Http.Headers;
using System.Runtime.Serialization;
using RestSharp.Extensions;
using static RestSharp.KnownHeaders;

// ReSharper disable InvertIf
// ReSharper disable SuggestBaseTypeForParameter

namespace RestSharp;

class RequestContent : IDisposable {
    readonly RestClient           _client;
    readonly RestRequest          _request;
    readonly List<Stream>         _streams = new();
    readonly ParametersCollection _parameters;

    HttpContent? Content { get; set; }

    public RequestContent(RestClient client, RestRequest request) {
        _client     = client;
        _request    = request;
        _parameters = new RequestParameters(_request.Parameters.Union(_client.DefaultParameters));
    }

    public HttpContent BuildContent() {
        var postParameters       = _parameters.GetContentParameters(_request.Method).ToArray();
        var postParametersExists = postParameters.Length > 0;
        var bodyParametersExists = _request.TryGetBodyParameter(out var bodyParameter);
        var filesExists          = _request.Files.Any();

        if (_request.HasFiles()                      ||
            BodyShouldBeMultipartForm(bodyParameter) ||
            filesExists                              ||
            _request.AlwaysMultipartFormData) {
            Content = CreateMultipartFormDataContent();
        }

        if (filesExists) AddFiles();

        if (bodyParametersExists) AddBody(postParametersExists, bodyParameter!);

        if (postParametersExists) AddPostParameters(postParameters);

        AddHeaders();

        return Content!;
    }

    void AddFiles() {
        // File uploading without multipart/form-data
        if (_request is { AlwaysSingleFileAsContent: true, Files.Count: 1 }) {
            var fileParameter = _request.Files.First();
            Content?.Dispose();
            Content = ToStreamContent(fileParameter);
            return;
        }

        var mpContent = Content as MultipartFormDataContent;
        foreach (var fileParameter in _request.Files) mpContent!.Add(ToStreamContent(fileParameter));
    }

    StreamContent ToStreamContent(FileParameter fileParameter) {
        var stream = fileParameter.GetFile();
        _streams.Add(stream);
        var streamContent = new StreamContent(stream);

        streamContent.Headers.ContentType = fileParameter.ContentType.AsMediaTypeHeaderValue;

        var dispositionHeader = fileParameter.Options.DisableFilenameEncoding
            ? ContentDispositionHeaderValue.Parse($"form-data; name=\"{fileParameter.Name}\"; filename=\"{fileParameter.FileName}\"")
            : new ContentDispositionHeaderValue("form-data") { Name = $"\"{fileParameter.Name}\"", FileName = $"\"{fileParameter.FileName}\"" };
        if (!fileParameter.Options.DisableFileNameStar) dispositionHeader.FileNameStar = fileParameter.FileName;
        streamContent.Headers.ContentDisposition = dispositionHeader;

        return streamContent;
    }

    HttpContent Serialize(BodyParameter body) {
        return body.DataFormat switch {
            DataFormat.None   => new StringContent(body.Value!.ToString()!, _client.Options.Encoding, body.ContentType.Value),
            DataFormat.Binary => GetBinary(),
            _                 => GetSerialized()
        };

        HttpContent GetBinary() {
            var byteContent = new ByteArrayContent((body.Value as byte[])!);
            byteContent.Headers.ContentType = body.ContentType.AsMediaTypeHeaderValue;

            if (body.ContentEncoding != null) {
                byteContent.Headers.ContentEncoding.Clear();
                byteContent.Headers.ContentEncoding.Add(body.ContentEncoding);
            }

            return byteContent;
        }

        HttpContent GetSerialized() {
            var serializer = _client.Serializers.GetSerializer(body.DataFormat);
            var content    = serializer.Serialize(body);

            if (content == null) throw new SerializationException("Request body serialized to null");

            var contentType = body.ContentType.Or(serializer.Serializer.ContentType);

            return new StringContent(content, _client.Options.Encoding, contentType.Value);
        }
    }

    static bool BodyShouldBeMultipartForm(BodyParameter? bodyParameter) {
        if (bodyParameter == null) return false;

        var bodyContentType = bodyParameter.ContentType.OrValue(bodyParameter.Name);
        return bodyParameter.Name.IsNotEmpty() && bodyParameter.Name != bodyContentType;
    }

    string GetOrSetFormBoundary() => _request.FormBoundary ?? (_request.FormBoundary = Guid.NewGuid().ToString());

    MultipartFormDataContent CreateMultipartFormDataContent() {
        var boundary    = GetOrSetFormBoundary();
        var mpContent   = new MultipartFormDataContent(boundary);
        var contentType = new MediaTypeHeaderValue("multipart/form-data");
        contentType.Parameters.Add(new NameValueHeaderValue(nameof(boundary), GetBoundary(boundary, _request.MultipartFormQuoteBoundary)));
        mpContent.Headers.ContentType = contentType;
        return mpContent;
    }

    void AddBody(bool hasPostParameters, BodyParameter bodyParameter) {
        var bodyContent = Serialize(bodyParameter);

        // we need to send the body
        if (hasPostParameters || _request.HasFiles() || BodyShouldBeMultipartForm(bodyParameter) || _request.AlwaysMultipartFormData) {
            // here we must use multipart form data
            var mpContent = Content as MultipartFormDataContent ?? CreateMultipartFormDataContent();
            var ct        = bodyContent.Headers.ContentType?.MediaType;
            var name      = bodyParameter.Name.IsEmpty() ? ct : bodyParameter.Name;

            if (name.IsEmpty())
                mpContent.Add(bodyContent);
            else
                mpContent.Add(bodyContent, name);
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

    void AddPostParameters(GetOrPostParameter[] postParameters) {
        if (postParameters.Length == 0) return;

        if (Content is MultipartFormDataContent mpContent) {
            // we got the multipart form already instantiated, just add parameters to it
            foreach (var postParameter in postParameters) {
                var parameterName = postParameter.Name!;

                mpContent.Add(
                    new StringContent(postParameter.Value?.ToString() ?? string.Empty, _client.Options.Encoding, postParameter.ContentType.Value),
                    _request.MultipartFormQuoteParameters ? $"\"{parameterName}\"" : parameterName
                );
            }
        }
        else {
            var encodedItems   = postParameters.Select(x => $"{x.Name!.UrlEncode()}={x.Value?.ToString()!.UrlEncode() ?? string.Empty}");
            var encodedContent = new StringContent(encodedItems.JoinToString("&"), _client.Options.Encoding, ContentType.FormUrlEncoded.Value);

            if (_client.Options.DisableCharset) {
                encodedContent.Headers.ContentType!.CharSet = "";
            }

            Content = encodedContent;
        }
    }

    static string GetBoundary(string boundary, bool quote) => quote ? $"\"{boundary}\"" : boundary;

    void AddHeaders() {
        var contentHeaders = _parameters
            .GetParameters<HeaderParameter>()
            .Where(x => IsContentHeader(x.Name!))
            .ToArray();

        if (contentHeaders.Length > 0 && Content == null) {
            // We need some content to add content headers to it, so if necessary, we'll add empty content
            Content = new StringContent("");
        }

        contentHeaders.ForEach(AddHeader);

        void AddHeader(HeaderParameter parameter) {
            var parameterStringValue = parameter.Value!.ToString();

            var value = parameter.Name switch {
                KnownHeaders.ContentType => GetContentTypeHeader(Ensure.NotNull(parameterStringValue, nameof(parameter))),
                _                        => parameterStringValue
            };
            var pName = Ensure.NotNull(parameter.Name, nameof(parameter.Name));
            ReplaceHeader(pName, value);
        }

        string GetContentTypeHeader(string contentType)
            => Content is MultipartFormDataContent
                ? $"{contentType}; boundary={GetBoundary(GetOrSetFormBoundary(), _request.MultipartFormQuoteBoundary)}"
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
