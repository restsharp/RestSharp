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

namespace RestSharp.Serializers;

public delegate bool SupportsContentType(string contentType);

public static class ContentType {
    public const string Json   = "application/json";
    public const string Xml    = "application/xml";
    public const string Plain  = "text/plain";
    public const string Binary = "application/octet-stream";
    public const string GZip   = "application/x-gzip";

    public static readonly Dictionary<DataFormat, string> FromDataFormat =
        new() {
            { DataFormat.Xml, Xml },
            { DataFormat.Json, Json }, 
            { DataFormat.None, Plain }, 
            { DataFormat.Binary, Binary }
        };

    public static readonly string[] JsonAccept = {
        "application/json", "text/json", "text/x-json", "text/javascript"
    };

    public static readonly string[] XmlAccept = {
        "application/xml", "text/xml"
    };
}