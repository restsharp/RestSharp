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

namespace RestSharp.Extensions;

/// <summary>
/// Extension method overload!
/// </summary>
static class MiscExtensions {
    /// <summary>
    /// Read a stream into a byte array
    /// </summary>
    /// <param name="input">Stream to read</param>
    /// <param name="cancellationToken"></param>
    /// <returns>byte[]</returns>
    public static async Task<byte[]> ReadAsBytes(this Stream input, CancellationToken cancellationToken) {
        var buffer = new byte[16 * 1024];

        using var ms = new MemoryStream();

        int read;
#if NETSTANDARD
        while ((read = await input.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
#else
        while ((read = await input.ReadAsync(buffer, cancellationToken)) > 0)
#endif
            ms.Write(buffer, 0, read);

        return ms.ToArray();
    }

    internal static IEnumerable<(string Name, string? Value)> GetProperties(this object obj, params string[] includedProperties) {
        // automatically create parameters from object props
        var type  = obj.GetType();
        var props = type.GetProperties();

        foreach (var prop in props) {
            if (!IsAllowedProperty(prop.Name))
                continue;

            var val = prop.GetValue(obj, null);

            if (val == null)
                continue;

            var propType = prop.PropertyType;

            if (propType.IsArray) {
                var elementType = propType.GetElementType();
                var array       = (Array)val;

                if (array.Length > 0 && elementType != null) {
                    // convert the array to an array of strings
                    var values = array.Cast<object>().Select(item => item.ToString());
                    yield return (prop.Name, string.Join(",", values));

                    continue;
                }
            }

            yield return (prop.Name, val.ToString());
        }

        bool IsAllowedProperty(string propertyName)
            => includedProperties.Length == 0 || includedProperties.Length > 0 && includedProperties.Contains(propertyName);
    }
}