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

// ReSharper disable InconsistentNaming

namespace RestSharp;

static partial class PropertyCache<T> where T : class {
    sealed partial class Populator {
        /// <summary>
        /// Gets the name of the property this populator represents.
        /// </summary>
        /// <remarks>
        /// This corresponds to the actual property name and not the name
        /// determined by <see cref="RequestPropertyAttribute.Name"/>
        /// </remarks>
        internal string PropertyName { get; }
        readonly Action<T, ICollection<Parameter>> _populate;

        Populator(string propertyName, Action<T, ICollection<Parameter>> populate) {
            PropertyName = propertyName;
            _populate    = populate;
        }

        /// <summary>
        /// Populates the provided parameters collection
        /// </summary>
        /// <param name="entity">The object to get parameters from</param>
        /// <param name="parameters">The parameters collection to populate</param>
        internal void Populate(T entity, ICollection<Parameter> parameters) => _populate(entity, parameters);

        /// <summary>
        /// Creates a new populator instance from the provided property
        /// </summary>
        /// <param name="property">A public instance property from the <typeparamref name="T"/> type</param>
        /// <returns></returns>
        internal static Populator From(PropertyInfo property) {
            var entity = Expression.Parameter(typeof(T));
            var callGetter = Expression.Call(entity, property.GetGetMethod()!);

            Expression convertGetterReturnToObject =
                property.PropertyType.IsValueType
                    ?
                    // Values types are not automatically boxed in LINQ expressions.
                    // This would throw an exception.
                    Expression.Convert(callGetter, typeof(object))
                    :
                    // Avoid unnecessary cast to object if property is already a reference type.
                    callGetter;

            // This compiles roughly to: `(T entity) => (object)entity.get_Property()`,
            // where `.GetProperty()` is the getter. The reason we use LINQ expressions
            // instead of direct calls to the MethodInfo instance is for an increase in
            // performance. We can then leverage our knowledge of the type parameter provided.

            var getObject = Expression.Lambda<Func<T, object>>(convertGetterReturnToObject, entity).Compile();

            var populate = GetPopulate(property);

            // Skip null property values so a DTO with unset optional properties doesn't throw.
            // This matches the reflection-based AddObject.
            return new(
                property.Name,
                (model, parameters) => {
                    var value = getObject(model);
                    if (value is null) return;
                    populate(value, parameters);
                }
            );
        }

        static Action<object, ICollection<Parameter>> GetPopulate(Func<object, IFormattable> getFormattable, RequestProperty requestProperty)
            => (value, parameters) => Populate(getFormattable(value), requestProperty, parameters);

        static Action<object, ICollection<Parameter>> GetPopulate(Func<object, IConvertible> getConvertible, RequestProperty requestProperty)
            => (value, parameters) => Populate(getConvertible(value), requestProperty, parameters);

        static Action<object, ICollection<Parameter>> GetPopulate(Func<object, IEnumerable<IFormattable>> getFormattables, RequestProperty requestProperty)
            => requestProperty.ArrayQueryType switch {
                RequestArrayQueryType.CommaSeparated => (value, parameters) => PopulateCsv(getFormattables(value), requestProperty, parameters),
                RequestArrayQueryType.ArrayParameters => GetPopulateArray(getFormattables, requestProperty),
                _ => (_, _) => { }
            }; // Here we avoid the cost of checking if the format is CSV or Array every time by caching the result of this evaluation.

        static Action<object, ICollection<Parameter>> GetPopulate(Func<object, IEnumerable<IConvertible>> getConvertibles, RequestProperty requestProperty)
            => requestProperty.ArrayQueryType switch {
                RequestArrayQueryType.CommaSeparated => (value, parameters) => PopulateCsv(getConvertibles(value), requestProperty, parameters),
                RequestArrayQueryType.ArrayParameters => GetPopulateArray(getConvertibles, requestProperty),
                _ => (_, _) => { }
            }; // Here we avoid the cost of checking if the format is CSV or Array every time by caching the result of this evaluation.

        static Action<object, ICollection<Parameter>> GetPopulate(Func<object, IEnumerable> getEnumerable, RequestProperty requestProperty)
            => requestProperty.ArrayQueryType switch {
                RequestArrayQueryType.CommaSeparated => (value, parameters) => PopulateCsv(getEnumerable(value), requestProperty, parameters),
                RequestArrayQueryType.ArrayParameters => GetPopulateArray(getEnumerable, requestProperty),
                _ => (_, _) => { }
            }; // Here we avoid the cost of checking if the format is CSV or Array every time by caching the result of this evaluation.

        static Action<object, ICollection<Parameter>> GetPopulate(Func<object, object> getObject, RequestProperty requestProperty)
            => requestProperty.ArrayQueryType switch {
                RequestArrayQueryType.CommaSeparated => (value, parameters) => PopulateCsv(getObject(value), requestProperty, parameters),
                RequestArrayQueryType.ArrayParameters => (value, parameters) => PopulateArray(getObject(value), requestProperty, parameters),
                _ => (_, _) => { }
            }; // Here we avoid the cost of checking if the format is CSV or Array every time by caching the result of this evaluation.

        static Action<object, ICollection<Parameter>> GetPopulate(PropertyInfo property) {
            var requestProperty = RequestProperty.From(property);

            // We need to use different conversion mechanisms for each return type. Simply calling `.ToString()`
            // on every returned object would not take into account special cases like custom formatting, enumeration etc.
            // Unchecked casts here are safe because the property value is boxed if needed.
            return property.PropertyType switch {
                var formattableType when typeof(IFormattable).IsAssignableFrom(formattableType) => GetPopulate(
                    value => Unsafe.As<IFormattable>(value),
                    requestProperty
                ),
                var convertibleType when typeof(IConvertible).IsAssignableFrom(convertibleType) => GetPopulate(
                    value => Unsafe.As<IConvertible>(value),
                    requestProperty
                ),
                var enumerableType when typeof(IEnumerable).IsAssignableFrom(enumerableType) => GetPopulateUnknown(
                    value => Unsafe.As<IEnumerable>(value),
                    requestProperty
                ),
                // At this point we're not necessarily sure we can just treat this as a bare object
                // and use its type converter. Even though the property itself returns an object,
                // the object returned itself may need to be treated in a special way, so we check
                // it as we go.
                _ => GetPopulate(static value => value, requestProperty)
            };
        }

        static Action<object, ICollection<Parameter>> GetPopulateUnknown(Func<object, IEnumerable> getEnumerable, RequestProperty requestProperty) {
            if (GetSingleEnumeratedTypeOrNull(requestProperty.Type) is not { } enumeratedType) {
                // Means we're dealing with a legacy, untyped enumerable instance.
                // We can just convert it into an enumerable of objects and delegate
                // conversion to string to the type converter of each enumerated item.
                return GetPopulateKnown(getEnumerable, requestProperty);
            }

            return enumeratedType switch {
                _ when typeof(IFormattable).IsAssignableFrom(enumeratedType) => GetPopulate(
                    GetEnumerableOf<IFormattable>(getEnumerable, enumeratedType),
                    requestProperty
                ),
                _ when typeof(IConvertible).IsAssignableFrom(enumeratedType) => GetPopulate(
                    GetEnumerableOf<IConvertible>(getEnumerable, enumeratedType),
                    requestProperty
                ),
                // At this point we're not necessarily sure we can just treat this as an enumerable of objects.
                // Since we know the actual enumerable may be a typed `IEnumerable<>` enumerating a type we're
                // interested in, we do further checks to ensure the correct conversion to string is applied.
                _ => GetPopulate(getEnumerable, requestProperty)
            };
        }

        static Action<object, ICollection<Parameter>> GetPopulateKnown(Func<object, IEnumerable> getEnumerable, RequestProperty requestProperty)
            => requestProperty.ArrayQueryType switch {
                RequestArrayQueryType.CommaSeparated => (value, parameters) => PopulateCsvUnknown(
                    getEnumerable(value),
                    requestProperty,
                    parameters
                ),
                RequestArrayQueryType.ArrayParameters => GetPopulateArray(getEnumerable, requestProperty),
                _ => (_, _) => { }
            }; // Here we avoid the cost of checking if the format is CSV or Array every time by caching the result of this evaluation.

        static Action<object, ICollection<Parameter>> GetPopulateArray(
            Func<object, IEnumerable<IFormattable>> getFormattables,
            RequestProperty requestProperty
        )
            => GetPopulateArray(getFormattables, formattable => GetStringValue(formattable, requestProperty), requestProperty);

        static Action<object, ICollection<Parameter>> GetPopulateArray(
            Func<object, IEnumerable<IConvertible>> getConvertibles,
            RequestProperty requestProperty
        )
            => GetPopulateArray(getConvertibles, GetStringValue, requestProperty);

        static Action<object, ICollection<Parameter>> GetPopulateArray<V>(
            Func<object, IEnumerable<V>> getEnumerable,
            Func<V, string?> toString,
            RequestProperty requestProperty
        ) where V : class {
            // We do this to avoid recreating request property on each iteration.
            var newRequestProperty = requestProperty with { Name = $"{requestProperty.Name}[]" };
            return (value, parameters) => PopulateArray(getEnumerable(value), toString, newRequestProperty, parameters);
        }

        static Action<object, ICollection<Parameter>> GetPopulateArray(Func<object, IEnumerable> getEnumerable, RequestProperty requestProperty) {
            // We do this to avoid recreating request property on each iteration.
            var newRequestProperty = requestProperty with { Name = $"{requestProperty.Name}[]" };
            return (value, parameters) => PopulateArray(getEnumerable(value), newRequestProperty, parameters);
        }

        static void Populate(IFormattable formattable, RequestProperty requestProperty, ICollection<Parameter> parameters)
            => Populate(GetStringValue(formattable, requestProperty), requestProperty, parameters);

        static void Populate(IConvertible convertible, RequestProperty requestProperty, ICollection<Parameter> parameters)
            => Populate(GetStringValue(convertible), requestProperty, parameters);

        static void Populate(object @object, RequestProperty requestProperty, ICollection<Parameter> parameters)
            => Populate(GetStringValueKnown(@object), requestProperty, parameters);

        static void Populate(string? stringValue, RequestProperty requestProperty, ICollection<Parameter> parameters) {
            var parameter = new GetOrPostParameter(requestProperty.Name, stringValue);
            parameters.Add(parameter);
        }

        static void PopulateCsv(IEnumerable<IFormattable> formattables, RequestProperty requestProperty, ICollection<Parameter> parameters)
            => PopulateCsv(formattables, formattable => GetStringValue(formattable, requestProperty), requestProperty, parameters);

        static void PopulateCsv(IEnumerable<IConvertible> convertibles, RequestProperty requestProperty, ICollection<Parameter> parameters)
            => PopulateCsv(convertibles, GetStringValue, requestProperty, parameters);

        static void PopulateCsv(IEnumerable<object> objects, RequestProperty requestProperty, ICollection<Parameter> parameters)
            => PopulateCsv(objects, @object => GetStringValueUnknown(@object, requestProperty), requestProperty, parameters);

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

        static void PopulateCsv<V>(
            IEnumerable<V> enumerable,
            Func<V, string?> toString,
            RequestProperty requestProperty,
            ICollection<Parameter> parameters
        ) where V : class {
#if NETCOREAPP2_0_OR_GREATER
            const char csvSeparator = ',';
#else
            const string csvSeparator = ",";
#endif
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
                    // At this point it's safe to assume we can delegate
                    // to the type converter.
                    Populate(@object, requestProperty, parameters);
                    break;
            }
        }

        static void PopulateCsvUnknown(IEnumerable enumerable, RequestProperty requestProperty, ICollection<Parameter> parameters) {
            if (GetSingleEnumeratedTypeOrNull(enumerable.GetType()) is not { } enumeratedType) {
                // Means we're dealing with a legacy, untyped enumerable instance.
                // We can just convert it into an enumerable of objects and delegate
                // conversion to string to the type converter of each enumerated item.
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

        static void PopulateCsvKnown(IEnumerable enumerable, RequestProperty requestProperty, ICollection<Parameter> parameters)
            => PopulateCsv(enumerable.Cast<object>(), requestProperty, parameters);

        static void PopulateArray(IEnumerable<IFormattable> formattables, RequestProperty requestProperty, ICollection<Parameter> parameters)
            => PopulateArray(formattables, formattable => GetStringValue(formattable, requestProperty), requestProperty, parameters);

        static void PopulateArray(IEnumerable<IConvertible> convertibles, RequestProperty requestProperty, ICollection<Parameter> parameters)
            => PopulateArray(convertibles, GetStringValue, requestProperty, parameters);

        static void PopulateArray(IEnumerable<object> objects, RequestProperty requestProperty, ICollection<Parameter> parameters)
            => PopulateArray(objects, @object => GetStringValueUnknown(@object, requestProperty), requestProperty, parameters);

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

        static void PopulateArray<V>(
            IEnumerable<V> enumerable,
            Func<V, string?> toString,
            RequestProperty requestProperty,
            ICollection<Parameter> parameters
        ) where V : class {
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
                    // We do this to avoid recreating request property on each iteration.
                    requestProperty = requestProperty with { Name = $"{requestProperty.Name}[]" };
                    PopulateArray(enumerable, requestProperty, parameters);
                    break;
                default:
                    // At this point it's safe to assume we can delegate
                    // to the type converter.
                    Populate(@object, requestProperty, parameters);
                    break;
            }
        }

        static void PopulateArrayUnknown(IEnumerable enumerable, RequestProperty requestProperty, ICollection<Parameter> parameters) {
            if (GetSingleEnumeratedTypeOrNull(enumerable.GetType()) is not { } enumeratedType) {
                // Means we're dealing with a legacy, untyped enumerable instance.
                // We can just convert it into an enumerable of objects and delegate
                // conversion to string to the type converter of each enumerated item.
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

        static void PopulateArrayKnown(IEnumerable enumerable, RequestProperty requestProperty, ICollection<Parameter> parameters)
            => PopulateArray(enumerable.Cast<object>(), requestProperty, parameters);

        static string GetStringValue(IFormattable formattable, RequestProperty requestProperty) => formattable.ToString(requestProperty.Format, null);

        static string GetStringValue(IConvertible convertible) => convertible.ToString(null);

        static string? GetStringValueKnown(object @object) => TypeDescriptor.GetConverter(@object).ConvertToString(@object);

        static string? GetStringValueUnknown(object @object, RequestProperty requestProperty)
            => @object switch {
                IFormattable formattable => GetStringValue(formattable, requestProperty),
                IConvertible convertible => GetStringValue(convertible),
                _ => GetStringValueKnown(@object)
            };

        static Func<object, IEnumerable<V>> GetEnumerableOf<V>(Func<object, IEnumerable> getEnumerable, Type enumeratedType) where V : class
            => enumeratedType.IsValueType ? value => getEnumerable(value).Cast<V>() : value => Unsafe.As<IEnumerable<V>>(getEnumerable(value));

        static Type? GetSingleEnumeratedTypeOrNull(Type enumerableType) {
            // Get all IEnumerable<> interfaces this type implements.
            var enumerableInterfaces =
                enumerableType
                    .GetInterfaces()
                    .Where(@interface => @interface.IsGenericType)
                    .Where(@interface => @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .ToArray();

            // If this type implements `IEnumerable<>` multiple times with different type parameters
            // we cannot pick which implementation to "believe", so we treat the whole thing as a bare,
            // untyped `IEnumerable`.
            return enumerableInterfaces.Length == 1 ? enumerableInterfaces[0].GetGenericArguments()[0] : null;
        }
    }
}
