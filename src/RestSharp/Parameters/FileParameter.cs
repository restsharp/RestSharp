//   Copyright (c) .NET Foundation and Contributors
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

namespace RestSharp;

/// <summary>
/// Container for files to be uploaded with requests
/// </summary>
[PublicAPI]
public record FileParameter {
    /// <summary>
    /// Name of the parameter
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Name of the file to use when uploading
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// MIME content type of file
    /// </summary>
    public string? ContentType { get; }

    /// <summary>
    /// Provides raw data for file
    /// </summary>
    public Func<Stream> GetFile { get; }

    public FileParameterOptions Options { get; }

    FileParameter(string name, string fileName, Func<Stream> getFile, string? contentType, FileParameterOptions options) {
        Name        = name;
        FileName    = fileName;
        GetFile     = getFile;
        Options     = options;
        ContentType = contentType ?? Serializers.ContentType.Binary;
    }

    /// <summary>
    /// Creates a file parameter from an array of bytes.
    /// </summary>
    /// <param name="name">The parameter name to use in the request.</param>
    /// <param name="data">The data to use as the file's contents.</param>
    /// <param name="filename">The filename to use in the request.</param>
    /// <param name="contentType">The content type to use in the request.</param>
    /// <param name="options">File parameter options</param>
    /// <returns>The <see cref="FileParameter" /></returns>
    public static FileParameter Create(string name, byte[] data, string filename, string? contentType = null, FileParameterOptions? options = null) {
        return new FileParameter(name, filename, GetFile, contentType, options ?? new FileParameterOptions());

        Stream GetFile() {
            var stream = new MemoryStream();
            stream.Write(data, 0, data.Length);
            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }

    /// <summary>
    /// Creates a file parameter from an array of bytes.
    /// </summary>
    /// <param name="name">The parameter name to use in the request.</param>
    /// <param name="getFile">Delegate that will be called with the request stream so you can write to it..</param>
    /// <param name="fileName">The filename to use in the request.</param>
    /// <param name="contentType">Optional: parameter content type, default is "application/g-zip"</param>
    /// <param name="options">File parameter options</param>
    /// <returns>The <see cref="FileParameter" /> using the default content type.</returns>
    public static FileParameter Create(
        string                name,
        Func<Stream>          getFile,
        string                fileName,
        string?               contentType = null,
        FileParameterOptions? options     = null
    )
        => new(name, fileName, getFile, contentType ?? Serializers.ContentType.Binary, options ?? new FileParameterOptions());

    public static FileParameter FromFile(string fullPath, string? name = null, string? contentType = null, FileParameterOptions? options = null) {
        if (!File.Exists(Ensure.NotEmptyString(fullPath, nameof(fullPath)))) throw new FileNotFoundException("File not found", fullPath);

        var fileName      = Path.GetFileName(fullPath);
        var parameterName = name ?? fileName;

        return new FileParameter(parameterName, fileName, GetFile, contentType, options ?? new FileParameterOptions());

        Stream GetFile() => File.OpenRead(fullPath);
    }
}

[PublicAPI]
public class FileParameterOptions {
    public bool DisableFileNameStar     { get; set; } = true;
    public bool DisableFilenameEncoding { get; set; }
}
