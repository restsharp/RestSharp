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

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace RestSharp;

public static class KnownHeaders {
    public const string Authorization      = "Authorization";
    public const string Accept             = "Accept";
    public const string Allow              = "Allow";
    public const string Expires            = "Expires";
    public const string ContentDisposition = "Content-Disposition";
    public const string ContentEncoding    = "Content-Encoding";
    public const string ContentLanguage    = "Content-Language";
    public const string ContentLength      = "Content-Length";
    public const string ContentLocation    = "Content-Location";
    public const string ContentRange       = "Content-Range";
    public const string ContentType        = "Content-Type";
    public const string Cookie             = "Cookie";
    public const string LastModified       = "Last-Modified";
    public const string ContentMD5         = "Content-MD5";
    public const string Host               = "Host";

    internal static readonly string[] ContentHeaders = {
        Allow, Expires, ContentDisposition, ContentEncoding, ContentLanguage, ContentLength, ContentLocation, ContentRange, ContentType, ContentMD5,
        LastModified
    };

    static readonly HashSet<string> ContentHeadersHash = new(ContentHeaders.Select(x => x.ToLower()));

    internal static bool IsContentHeader(string key) => ContentHeadersHash.Contains(key.ToLower());
}
