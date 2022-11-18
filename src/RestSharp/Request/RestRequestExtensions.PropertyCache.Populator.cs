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

using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RestSharp;

public static partial class RestRequestExtensions {
    static partial class PropertyCache<T> where T : class {
        sealed partial class Populator {
            internal string PropertyName { get; }
            readonly Action<T, ICollection<Parameter>> _populate;

            private Populator(string propertyName, Action<T, ICollection<Parameter>> populate) {
                PropertyName = propertyName;
                _populate = populate;
            }

            internal void Populate(T entity, ICollection<Parameter> parameters) => _populate(entity, parameters);

            internal static Populator From(PropertyInfo property) {
                var entity = Expression.Parameter(typeof(T));
                var callGetter = Expression.Call(entity, property.GetGetMethod()!);

                Expression convertGetterReturnToObject =
                    property.PropertyType.IsValueType ?
                    Expression.Convert(callGetter, typeof(object)) :
                    callGetter;

                var getObject = Expression.Lambda<Func<T, object>>(convertGetterReturnToObject, entity).Compile();

                var populate = GetPopulate(getObject, property);

                return new Populator(property.Name, populate);
            }

            static Action<T, ICollection<Parameter>> GetPopulate(Func<T, IFormattable> getFormattable, RequestProperty requestProperty) => (model, parameters) => Populate(getFormattable(model), requestProperty, parameters);

            static Action<T, ICollection<Parameter>> GetPopulate(Func<T, IConvertible> getConvertible, RequestProperty requestProperty) => (model, parameters) => Populate(getConvertible(model), requestProperty, parameters);

            static Action<T, ICollection<Parameter>> GetPopulate(Func<T, IEnumerable<IFormattable>> getFormattables, RequestProperty requestProperty) => requestProperty.ArrayQueryType switch {
                RequestArrayQueryType.CommaSeparated => (model, parameters) => PopulateCsv(getFormattables(model), requestProperty, parameters),
                RequestArrayQueryType.ArrayParameters => GetPopulateArray(getFormattables, requestProperty),
                _ => (_, _) => { }
            };

            static Action<T, ICollection<Parameter>> GetPopulate(Func<T, IEnumerable<IConvertible>> getConvertibles, RequestProperty requestProperty) => requestProperty.ArrayQueryType switch {
                RequestArrayQueryType.CommaSeparated => (entity, parameters) => PopulateCsv(getConvertibles(entity), requestProperty, parameters),
                RequestArrayQueryType.ArrayParameters => GetPopulateArray(getConvertibles, requestProperty),
                _ => (_, _) => { }
            };

            static Action<T, ICollection<Parameter>> GetPopulate(Func<T, IEnumerable> getEnumerable, RequestProperty requestProperty) => requestProperty.ArrayQueryType switch {
                RequestArrayQueryType.CommaSeparated => (entity, parameters) => PopulateCsv(getEnumerable(entity), requestProperty, parameters),
                RequestArrayQueryType.ArrayParameters => GetPopulateArray(getEnumerable, requestProperty),
                _ => (_, _) => { }
            };

            static Action<T, ICollection<Parameter>> GetPopulate(Func<T, object> getObject, RequestProperty requestProperty) => requestProperty.ArrayQueryType switch {
                RequestArrayQueryType.CommaSeparated => (entity, parameters) => PopulateCsv(getObject(entity), requestProperty, parameters),
                RequestArrayQueryType.ArrayParameters => (entity, parameters) => PopulateArray(getObject(entity), requestProperty, parameters),
                _ => (_, _) => { }
            };

            static Action<T, ICollection<Parameter>> GetPopulate(Func<T, object> getObject, PropertyInfo property) {
                var requestProperty = RequestProperty.From(property);

                return property.PropertyType switch {
                    var formattableType when typeof(IFormattable).IsAssignableFrom(formattableType) => GetPopulate(entity => Unsafe.As<IFormattable>(getObject(entity))!, requestProperty),
                    var convertibleType when typeof(IConvertible).IsAssignableFrom(convertibleType) => GetPopulate(entity => Unsafe.As<IConvertible>(getObject(entity))!, requestProperty),
                    var enumerableType when typeof(IEnumerable).IsAssignableFrom(enumerableType) => GetPopulateUnknown(entity => Unsafe.As<IEnumerable>(getObject(entity))!, requestProperty),
                    var otherType => GetPopulate(getObject, requestProperty)
                };
            }

            static Action<T, ICollection<Parameter>> GetPopulateUnknown(Func<T, IEnumerable> getEnumerable, RequestProperty requestProperty) {

                if (GetSingleEnumeratedTypeOrNull(requestProperty.Type) is not { } enumeratedType) {
                    return GetPopulateKnown(getEnumerable, requestProperty);
                }

                return enumeratedType switch {
                    var formattableEnumeratedType when typeof(IFormattable).IsAssignableFrom(formattableEnumeratedType) => GetPopulate(GetEnumerableOf<IFormattable>(getEnumerable, formattableEnumeratedType), requestProperty),
                    var convertibleEnumeratedType when typeof(IConvertible).IsAssignableFrom(convertibleEnumeratedType) => GetPopulate(GetEnumerableOf<IConvertible>(getEnumerable, convertibleEnumeratedType), requestProperty),
                    var otherEnumeratedType => GetPopulate(getEnumerable, requestProperty)
                };
            }

            static Action<T, ICollection<Parameter>> GetPopulateKnown(Func<T, IEnumerable> getEnumerable, RequestProperty requestProperty) => requestProperty.ArrayQueryType switch {
                RequestArrayQueryType.CommaSeparated => (entity, parameters) => PopulateCsvUnknown(getEnumerable(entity), requestProperty, parameters),
                RequestArrayQueryType.ArrayParameters => GetPopulateArray(getEnumerable, requestProperty),
                _ => (_, _) => { }
            };

            static Action<T, ICollection<Parameter>> GetPopulateArray(Func<T, IEnumerable<IFormattable>> getFormattables, RequestProperty requestProperty) =>
                GetPopulateArray(getFormattables, formattable => GetStringValue(formattable, requestProperty), requestProperty);

            static Action<T, ICollection<Parameter>> GetPopulateArray(Func<T, IEnumerable<IConvertible>> getConvertibles, RequestProperty requestProperty) =>
                GetPopulateArray(getConvertibles, GetStringValue, requestProperty);

            static Action<T, ICollection<Parameter>> GetPopulateArray<V>(Func<T, IEnumerable<V>> getEnumerable, Func<V, string?> toString, RequestProperty requestProperty) where V : class {
                var newRequestProperty = requestProperty with { Name = $"{requestProperty.Name}[]" };
                return (entity, parameters) => PopulateArray(getEnumerable(entity), toString, newRequestProperty, parameters);
            }

            static Action<T, ICollection<Parameter>> GetPopulateArray(Func<T, IEnumerable> getEnumerable, RequestProperty requestProperty) {
                var newRequestProperty = requestProperty with { Name = $"{requestProperty.Name}[]" };
                return (entity, parameters) => PopulateArray(getEnumerable(entity), newRequestProperty, parameters);
            }

            static void Populate(IFormattable formattable, RequestProperty requestProperty, ICollection<Parameter> parameters) => Populate(GetStringValue(formattable, requestProperty), requestProperty, parameters);

            static void Populate(IConvertible convertible, RequestProperty requestProperty, ICollection<Parameter> parameters) => Populate(GetStringValue(convertible), requestProperty, parameters);

            static void Populate(object @object, RequestProperty requestProperty, ICollection<Parameter> parameters) => Populate(GetKnownStringValue(@object), requestProperty, parameters);

            static void Populate(string? stringValue, RequestProperty requestProperty, ICollection<Parameter> parameters) {
                var parameter = new GetOrPostParameter(requestProperty.Name, stringValue);
                parameters.Add(parameter);
            }

            static void PopulateCsv(IEnumerable<IFormattable> formattables, RequestProperty requestProperty, ICollection<Parameter> parameters) =>
                PopulateCsv(formattables, formattable => GetStringValue(formattable, requestProperty), requestProperty, parameters);

            static void PopulateCsv(IEnumerable<IConvertible> convertibles, RequestProperty requestProperty, ICollection<Parameter> parameters) =>
                PopulateCsv(convertibles, GetStringValue, requestProperty, parameters);

            static void PopulateCsv(IEnumerable<object> objects, RequestProperty requestProperty, ICollection<Parameter> parameters) =>
                PopulateCsv(objects, @object => GetUnknownStringValue(@object, requestProperty), requestProperty, parameters);

            static void PopulateCsv(IEnumerable enumerable, RequestProperty requestProperty, ICollection<Parameter> parameters) {
                switch (enumerable) {
                    case IEnumerable<IFormattable> formattables:
                        PopulateCsv(formattables, requestProperty, parameters);
                        break;
                    case IEnumerable<IConvertible> convertibles:
                        PopulateCsv(convertibles, requestProperty, parameters);
                        break;
                    case IEnumerable<object> objects:
                        PopulateCsv(objects, requestProperty, parameters);
                        break;
                    default:
                        PopulateCsvUnknown(enumerable, requestProperty, parameters);
                        break;
                }
            }


            static void PopulateCsv<V>(IEnumerable<V> enumerable, Func<V, string?> toString, RequestProperty requestProperty, ICollection<Parameter> parameters) where V : class {
                const string csvSeparator = ",";
                var formattedStrings = enumerable.Select(toString);
                var csv = string.Join(csvSeparator, formattedStrings);
                Populate(csv, requestProperty, parameters);
            }

            static void PopulateCsv(object @object, RequestProperty requestProperty, ICollection<Parameter> parameters) {
                switch (@object) {
                    case IFormattable formattable:
                        Populate(formattable, requestProperty, parameters);
                        break;
                    case IConvertible convertible:
                        Populate(convertible, requestProperty, parameters);
                        break;
                    case IEnumerable enumerable:
                        PopulateCsv(enumerable, requestProperty, parameters);
                        break;
                    default:
                        Populate(@object, requestProperty, parameters);
                        break;
                }
            }

            static void PopulateCsvUnknown(IEnumerable enumerable, RequestProperty requestProperty, ICollection<Parameter> parameters) {

                if (GetSingleEnumeratedTypeOrNull(enumerable.GetType()) is not { } enumeratedType) {
                    PopulateCsvKnown(enumerable, requestProperty, parameters);
                    return;
                }

                switch (enumeratedType) {
                    case var _ when typeof(IFormattable).IsAssignableFrom(enumeratedType):
                        PopulateCsv(enumerable.Cast<IFormattable>(), requestProperty, parameters);
                        break;
                    case var _ when typeof(IConvertible).IsAssignableFrom(enumeratedType):
                        PopulateCsv(enumerable.Cast<IConvertible>(), requestProperty, parameters);
                        break;
                    default:
                        PopulateCsvKnown(enumerable, requestProperty, parameters);
                        break;
                }
            }

            static void PopulateCsvKnown(IEnumerable enumerable, RequestProperty requestProperty, ICollection<Parameter> parameters) => PopulateCsv(enumerable.Cast<object>(), requestProperty, parameters);

            static void PopulateArray(IEnumerable<IFormattable> formattables, RequestProperty requestProperty, ICollection<Parameter> parameters) =>
                PopulateArray(formattables, formattable => GetStringValue(formattable, requestProperty), requestProperty, parameters);

            static void PopulateArray(IEnumerable<IConvertible> convertibles, RequestProperty requestProperty, ICollection<Parameter> parameters) =>
                PopulateArray(convertibles, GetStringValue, requestProperty, parameters);

            static void PopulateArray(IEnumerable<object> objects, RequestProperty requestProperty, ICollection<Parameter> parameters) =>
                PopulateArray(objects, @object => GetUnknownStringValue(@object, requestProperty), requestProperty, parameters);

            static void PopulateArray(IEnumerable enumerable, RequestProperty requestProperty, ICollection<Parameter> parameters) {
                switch (enumerable) {
                    case IEnumerable<IFormattable> formattables:
                        PopulateArray(formattables, requestProperty, parameters);
                        break;
                    case IEnumerable<IConvertible> convertibles:
                        PopulateArray(convertibles, requestProperty, parameters);
                        break;
                    case IEnumerable<object> objects:
                        PopulateArray(objects, requestProperty, parameters);
                        break;
                    default:
                        PopulateArrayUnknown(enumerable, requestProperty, parameters);
                        break;
                }
            }


            static void PopulateArray<V>(IEnumerable<V> enumerable, Func<V, string?> toString, RequestProperty requestProperty, ICollection<Parameter> parameters) where V : class {
                var values = enumerable.Select(toString);

                foreach (var value in values) {
                    Populate(value, requestProperty, parameters);
                }
            }

            static void PopulateArray(object @object, RequestProperty requestProperty, ICollection<Parameter> parameters) {
                switch (@object) {
                    case IFormattable formattable:
                        Populate(formattable, requestProperty, parameters);
                        break;
                    case IConvertible convertible:
                        Populate(convertible, requestProperty, parameters);
                        break;
                    case IEnumerable enumerable:
                        requestProperty = requestProperty with { Name = $"{requestProperty.Name}[]" };
                        PopulateArray(enumerable, requestProperty, parameters);
                        break;
                    default:
                        Populate(@object, requestProperty, parameters);
                        break;
                }
            }

            static void PopulateArrayUnknown(IEnumerable enumerable, RequestProperty requestProperty, ICollection<Parameter> parameters) {

                if (GetSingleEnumeratedTypeOrNull(enumerable.GetType()) is not { } enumeratedType) {
                    PopulateArrayKnown(enumerable, requestProperty, parameters);
                    return;
                }

                switch (enumeratedType) {
                    case var _ when typeof(IFormattable).IsAssignableFrom(enumeratedType):
                        PopulateArray(enumerable.Cast<IFormattable>(), requestProperty, parameters);
                        break;
                    case var _ when typeof(IConvertible).IsAssignableFrom(enumeratedType):
                        PopulateArray(enumerable.Cast<IConvertible>(), requestProperty, parameters);
                        break;
                    default:
                        PopulateArrayKnown(enumerable, requestProperty, parameters);
                        break;
                }
            }

            static void PopulateArrayKnown(IEnumerable enumerable, RequestProperty requestProperty, ICollection<Parameter> parameters) => PopulateArray(enumerable.Cast<object>(), requestProperty, parameters);

            static string GetStringValue(IFormattable formattable, RequestProperty requestProperty) => formattable.ToString(requestProperty.Format, null);

            static string GetStringValue(IConvertible convertible) => convertible.ToString(null);

            static string? GetKnownStringValue(object @object) => TypeDescriptor.GetConverter(@object).ConvertToString(@object);

            static string? GetUnknownStringValue(object @object, RequestProperty requestProperty) => @object switch {
                IFormattable formattable => GetStringValue(formattable, requestProperty),
                IConvertible convertible => GetStringValue(convertible),
                _ => GetKnownStringValue(@object)
            };

            static Func<T, IEnumerable<V>> GetEnumerableOf<V>(Func<T, IEnumerable> getEnumerable, Type enumeratedType) where V : class =>
                enumeratedType.IsValueType ?
                entity => getEnumerable(entity).Cast<V>() :
                entity => Unsafe.As<IEnumerable<V>>(getEnumerable(entity))!;

            static Type? GetSingleEnumeratedTypeOrNull(Type enumerableType) {
                var enumerableInterfaces =
                    enumerableType
                        .GetInterfaces()
                        .Where(@interface => @interface.IsGenericType)
                        .Where(@interface => @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        .ToArray();

                return enumerableInterfaces.Length == 1 ? enumerableInterfaces[0].GetGenericArguments()[0] : null;
            }
        }
    }
}
