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

namespace RestSharp; 

public static class HttpContentExtensions {
    public static string GetFormBoundary(this HttpContent content) {
        var contentType = content.Headers.ContentType.ToString();
        var index       = contentType.IndexOf("boundary=", StringComparison.Ordinal);
        return index > 0 ? GetFormBoundary(contentType, index) : "";
    } 
    
    static string GetFormBoundary(string headerValue, int index) {
        var part = headerValue.Substring(index);
        return part.Substring(10, 36);
    }
}