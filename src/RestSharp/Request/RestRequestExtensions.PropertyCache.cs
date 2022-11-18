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

public static partial class RestRequestExtensions {
    static partial class PropertyCache<T> where T : class {
        static readonly IReadOnlyCollection<Populator> Populators =
            typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(Populator.From)
                .ToArray();

        internal static IEnumerable<Parameter> GetParameters(T entity, params string[] includedProperties) {
            if (includedProperties.Length == 0) {
                return GetParameters(entity);
            }

            Array.Sort(includedProperties); // Otherwise binary search is unsafe.

            var populators = Populators.Where(populator => Array.BinarySearch(includedProperties, populator.PropertyName) >= 0);
            return GetParameters(entity, populators);
        }
        internal static IEnumerable<Parameter> GetParameters(T entity) => GetParameters(entity, Populators);

        static IEnumerable<Parameter> GetParameters(T entity, IEnumerable<Populator> populators) {
            var parameters = new List<Parameter>(capacity: Populators.Count);

            foreach (var populator in populators) {
                populator.Populate(entity, parameters);
            }

            return parameters;
        }
    }
}
