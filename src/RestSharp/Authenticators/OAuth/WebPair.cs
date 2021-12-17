﻿//   Copyright © 2009-2021 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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

namespace RestSharp.Authenticators.OAuth;

class WebPair {
    public WebPair(string name, string value, bool encode = false) {
        Name     = name;
        Value    = value;
        WebValue = encode ? OAuthTools.UrlEncodeRelaxed(value) : value;
        Encode   = encode;
    }

    public string Name     { get; }
    public string Value    { get; }
    public string WebValue { get; }
    public bool   Encode   { get; }

    internal static WebPairComparer Comparer { get; } = new();

    internal class WebPairComparer : IComparer<WebPair> {
        public int Compare(WebPair? x, WebPair? y) {
            var compareName = string.CompareOrdinal(x?.Name, y?.Name);

            return compareName != 0 ? compareName : string.CompareOrdinal(x?.Value, y?.Value);
        }
    }
}