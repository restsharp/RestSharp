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

using System.Reflection;

namespace RestSharp;

static partial class PropertyCache<T> where T : class {
    static readonly IReadOnlyCollection<Populator> Populators =
        typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            // We need to ensure the property does not return a ref struct
            // since reflection and LINQ expressions do not play well with
            // them. All bets are off, so let's just ignore them.
#if NETCOREAPP2_1_OR_GREATER
            .Where(property => !property.PropertyType.IsByRefLike)
#else
                // Since `IsByRefLikeAttribute` is generated at compile time, each assembly
                // may have its own definition of the attribute, so we must compare by full name
                // instead of type.
                .Where(property => !property.PropertyType.GetCustomAttributes().Select(attribute => attribute.GetType().FullName).Any(attributeName => attributeName == "System.Runtime.CompilerServices.IsByRefLikeAttribute"))
#endif
            .Select(Populator.From)
            .ToArray();

    /// <summary>
    /// Gets parameters from the provided object
    /// </summary>
    /// <param name="entity">The object from which to get the parameters</param>
    /// <param name="includedProperties">Properties to include, or nothing to include everything. The array will be sorted.</param>
    /// <returns></returns>
    internal static IEnumerable<Parameter> GetParameters(T entity, params string[] includedProperties) {
        if (includedProperties.Length == 0) {
            return GetParameters(entity);
        }

        Array.Sort(includedProperties); // Otherwise binary search is unsafe.

        // Get only populators found in `includedProperties`.

        var populators = Populators.Where(populator => Array.BinarySearch(includedProperties, populator.PropertyName) >= 0);
        return GetParameters(entity, populators);
    }

    /// <summary>
    /// Gets parameters from the provided object
    /// </summary>
    /// <param name="entity">The object from which to get the parameters</param>
    /// <returns></returns>
    internal static IEnumerable<Parameter> GetParameters(T entity) => GetParameters(entity, Populators);

    static IEnumerable<Parameter> GetParameters(T entity, IEnumerable<Populator> populators) {
        var parameters = new List<Parameter>(capacity: Populators.Count);

        foreach (var populator in populators) {
            // Each populator may return one or more parameters,
            // so they take temporary ownership of the list in order
            // to populate it with its own set of parameters.
            populator.Populate(entity, parameters);
        }

        return parameters;
    }
}
