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

using System.Globalization;

namespace RestSharp.Extensions;

static class ObjectExtensions {
    /// <summary>
    /// Converts a value to its string representation using the specified culture for IFormattable types.
    /// </summary>
    /// <typeparam name="T">The type of value to convert</typeparam>
    /// <param name="value">The value to convert</param>
    /// <param name="culture">The culture to use for formatting. If null, uses the current culture.</param>
    /// <returns>String representation using the specified culture, or null if value is null</returns>
    internal static string? ToStringWithCulture<T>(this T value, CultureInfo? culture) => value switch {
        null           => null,
        IFormattable f => f.ToString(null, culture),
        _              => value.ToString()
    };
}
