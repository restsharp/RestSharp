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
// 

using System.Linq.Expressions;

namespace RestSharp;

public static class HttpContentExtensions {
    static readonly Func<MultipartContent, string> GetBoundary = GetFieldAccessor<MultipartContent, string>("_boundary");
    
    public static string GetFormBoundary(this MultipartFormDataContent content) {
        return GetBoundary(content);
        // var contentType = content.Headers.ContentType?.ToString();
        // var index       = contentType?.IndexOf("boundary=", StringComparison.Ordinal) ?? 0;
        // return index > 0 ? GetFormBoundary(contentType!, index) : "";
    }

    static string GetFormBoundary(string headerValue, int index) {
        var part = headerValue.Substring(index);
        return part.Substring(10, 36);
    }

    static Func<T, TReturn> GetFieldAccessor<T, TReturn>(string fieldName) {
        var param    = Expression.Parameter(typeof(T), "arg");
        var member   = Expression.Field(param, fieldName);
        var lambda   = Expression.Lambda(typeof(Func<T, TReturn>), member, param);
        var compiled = (Func<T, TReturn>)lambda.Compile();
        return compiled;
    }
}