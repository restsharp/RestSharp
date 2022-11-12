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

using RestSharp.Extensions;
using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RestSharp;

public static partial class RestRequestExtensions {
    static partial class TypeCache<T> where T : class {
#if NET5_0_OR_GREATER
        const char CsvSeparator = ',';
#else
        const string CsvSeparator = ",";
#endif
        const string ArrayBrackets = "[]";

        static readonly IReadOnlyDictionary<string, Func<T, ParameterProperty>> NameToGetter =
            typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(property => {
                    var requestProperty =
                        property.GetAttribute<RequestPropertyAttribute>() ??
                        new RequestPropertyAttribute();

                    return new {
                        property.Name,
                        // The getter can only be null if it is non-public,
                        // but we are already filtering out non-public properties
                        // through the binding flags, so it's fine.
                        GetMethod = property.GetGetMethod()!,
                        RequestProperty = requestProperty
                    };
                })
#if NET5_0_OR_GREATER
                // We must filter out ref structs otherwise they will
                // break LINQ expressions apart once we compile them.
                .Where(property => !property.GetMethod.ReturnType.IsByRefLike)
#endif
                .ToDictionary(
                    property => property.Name,
                    property => CompileGetter(
                        property.GetMethod,
                        property.RequestProperty.Name ?? property.Name,
                        property.RequestProperty));

        internal static IEnumerable<Parameter> GetParameters(T obj, params string[] includedProperties) =>
            includedProperties.Length == 0 ?
            GetParameters(obj) :
            GetParameters(obj, includedProperties.Select(property => NameToGetter[property]));

        internal static IEnumerable<Parameter> GetParameters(T obj) =>
            GetParameters(obj, NameToGetter.Values);

        static IEnumerable<Parameter> GetParameters(T obj, IEnumerable<Func<T, ParameterProperty>> getters) =>
            getters
                .Select(getParameter => getParameter(obj))
                .SelectMany(property => property.Values.Select(value => new GetOrPostParameter(property.Name, value)));

        static Func<T, ParameterProperty> CompileGetter(MethodInfo getMethod, string propertyName, RequestPropertyAttribute requestProperty) {
            var modelParameter = Expression.Parameter(typeof(T));

            var callGetter = Expression.Call(modelParameter, getMethod);

            return getMethod.ReturnType switch {
                var @return when typeof(IFormattable).IsAssignableFrom(@return) =>
                    @return.IsValueType ?
                    GetFormattableGetter(modelParameter, Expression.Convert(callGetter, typeof(IFormattable)), propertyName, requestProperty) :
                    GetFormattableGetter(modelParameter, callGetter, propertyName, requestProperty),
                var @return when typeof(IConvertible).IsAssignableFrom(@return) =>
                    @return.IsValueType ?
                    GetConvertibleGetter(modelParameter, Expression.Convert(callGetter, typeof(IConvertible)), propertyName) :
                    GetConvertibleGetter(modelParameter, callGetter, propertyName),
                var @return when typeof(IEnumerable).IsAssignableFrom(@return) =>
                    @return.IsValueType ?
                    GetEnumerableGetter(modelParameter, Expression.Convert(callGetter, typeof(IEnumerable)), propertyName, requestProperty, @return) :
                    GetEnumerableGetter(modelParameter, callGetter, propertyName, requestProperty, @return),
                var @return =>
                    @return.IsValueType ?
                    GetObjectGetter(modelParameter, Expression.Convert(callGetter, typeof(object)), propertyName, requestProperty, @return) :
                    GetObjectGetter(modelParameter, callGetter, propertyName, requestProperty, @return)
            };
        }

        static Func<T, ParameterProperty> GetFormattableGetter(ParameterExpression modelParameter, Expression callGetter, string propertyName, RequestPropertyAttribute requestProperty) {
            var getFormattable = Expression.Lambda<Func<T, IFormattable>>(callGetter, modelParameter).Compile();

            return model => new ParameterProperty(propertyName, new[] {
                getFormattable(model).ToString(requestProperty.Format, null)
            });
        }

        static Func<T, ParameterProperty> GetConvertibleGetter(ParameterExpression modelParameter, Expression callGetter, string propertyName) {
            var getFormattable = Expression.Lambda<Func<T, IConvertible>>(callGetter, modelParameter).Compile();

            return model => new ParameterProperty(propertyName, new[] {
                getFormattable(model).ToString(null)
            });
        }

        static Func<T, ParameterProperty> GetEnumerableGetter(ParameterExpression modelParameter, Expression callGetter, string propertyName, RequestPropertyAttribute requestProperty, Type @return) {

            var getEnumerable = Expression.Lambda<Func<T, IEnumerable>>(callGetter, modelParameter).Compile();

            var getValues = GetGetFormattedStringValues(getEnumerable, requestProperty, @return);

            var actualPropertyName =
                requestProperty.ArrayQueryType == RequestArrayQueryType.ArrayParameters ?
                $"{propertyName}{ArrayBrackets}" :
                propertyName;

            return model => new ParameterProperty(actualPropertyName, getValues(model));
        }

        static Func<T, IEnumerable<string?>> GetGetFormattedStringValues(Func<T, IEnumerable> getEnumerable, RequestPropertyAttribute requestProperty, Type @return) {

            var getStringValues = GetGetRawStringValues(getEnumerable, requestProperty, @return);

            return requestProperty.ArrayQueryType switch {
                RequestArrayQueryType.CommaSeparated => model => new[] { string.Join(CsvSeparator, getStringValues(model)) },
                RequestArrayQueryType.ArrayParameters => getStringValues,
                _ => model => Enumerable.Empty<string>()
            };
        }

        static Func<T, IEnumerable<string?>> GetGetRawStringValues(Func<T, IEnumerable> getEnumerable, RequestPropertyAttribute requestProperty, Type @return) {
            var enumerableInterfaces =
                @return
                    .GetInterfaces()
                    .Append(@return) // In case `@return` is `IEnumerable<>`: https://stackoverflow.com/a/63794667
                    .Where(
                        @interface =>
                        @interface.IsGenericType &&
                        @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .ToArray();

            if (enumerableInterfaces.Length != 1) {
                return requestProperty.ArrayQueryType switch {
                    RequestArrayQueryType.CommaSeparated => model => getEnumerable(model).Cast<object>().Select(value => string.Join(CsvSeparator, GetStringValues(value, requestProperty))),
                    RequestArrayQueryType.ArrayParameters => model => getEnumerable(model).Cast<object>().SelectMany(value => GetStringValues(value, requestProperty)),
                    _ => model => Enumerable.Empty<string>()
                };
            }

            return enumerableInterfaces[0].GetGenericArguments()[0] switch {
                var enumerated when typeof(IFormattable).IsAssignableFrom(enumerated) =>
                    enumerated.IsValueType ?
                    model => getEnumerable(model).Cast<IFormattable>().Select(formattable => formattable.ToString(requestProperty.Format, null)) :
                    model => Unsafe.As<IEnumerable<IFormattable>>(getEnumerable(model)).Select(formattable => formattable.ToString(requestProperty.Format, null)),
                var enumerated when typeof(IConvertible).IsAssignableFrom(enumerated) =>
                    enumerated.IsValueType ?
                    model => getEnumerable(model).Cast<IConvertible>().Select(formattable => formattable.ToString(null)) :
                    model => Unsafe.As<IEnumerable<IConvertible>>(getEnumerable(model)).Select(formattable => formattable.ToString(null)),

                var enumerated =>
                    requestProperty.ArrayQueryType switch {
                        RequestArrayQueryType.CommaSeparated =>
                            enumerated.IsValueType ?
                            model => new[] { string.Join(CsvSeparator, getEnumerable(model).Cast<object>().Select(value => string.Join(CsvSeparator, GetStringValues(value, requestProperty)))) } :
                            model => new[] { string.Join(CsvSeparator, Unsafe.As<IEnumerable<object>>(getEnumerable(model)).Select(value => string.Join(CsvSeparator, GetStringValues(value, requestProperty)))) },
                        RequestArrayQueryType.ArrayParameters =>
                            enumerated.IsValueType ?
                            model => getEnumerable(model).Cast<object>().SelectMany(value => GetStringValues(value, requestProperty)) :
                            model => Unsafe.As<IEnumerable<object>>(getEnumerable(model)).SelectMany(value => GetStringValues(value, requestProperty)),
                        _ => model => Enumerable.Empty<string>()
                    }
            };
        }

        static Func<T, ParameterProperty> GetObjectGetter(ParameterExpression modelParameter, Expression callGetter, string propertyName, RequestPropertyAttribute requestProperty, Type @return) {
            var getObject = Expression.Lambda<Func<T, object>>(callGetter, modelParameter).Compile();

            Func<object, string> getPropertyName =
                requestProperty.ArrayQueryType == RequestArrayQueryType.ArrayParameters ?
                @object => @object is IEnumerable ? $"{propertyName}{ArrayBrackets}" : propertyName :
                _ => propertyName;

            if (@return == typeof(object)) {
                return model => {
                    var @object = getObject(model);

                    var values = GetStringValues(@object, requestProperty);

                    return new ParameterProperty(getPropertyName(@object), values);
                };
            }

            var getterConverter = TypeDescriptor.GetConverter(@return);
            return model => new ParameterProperty(propertyName, new[] {
                getterConverter.ConvertToString(getObject(model))
            });
        }

        static IEnumerable<string?> GetStringValues(object @object, RequestPropertyAttribute requestProperty) => @object switch {
            IFormattable formattable => new[] { formattable.ToString(requestProperty.Format, null) },
            IConvertible convertible => new[] { convertible.ToString(null) },
            IEnumerable enumerable => requestProperty.ArrayQueryType switch {
                RequestArrayQueryType.CommaSeparated => new[] { string.Join(CsvSeparator, enumerable.Cast<object>().Select(value => string.Join(CsvSeparator, GetStringValues(value, requestProperty)))) },
                RequestArrayQueryType.ArrayParameters => enumerable.Cast<object>().SelectMany(value => GetStringValues(value, requestProperty)),
                _ => Enumerable.Empty<string>()
            },
            _ => new[] { TypeDescriptor.GetConverter(@object).ConvertToString(@object) }
        };
    }
}
