//   Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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

using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace RestSharp.Extensions
{
    /// <summary>
    ///     Reflection extensions
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        ///     Retrieve an attribute from a member (property)
        /// </summary>
        /// <typeparam name="T">Type of attribute to retrieve</typeparam>
        /// <param name="prop">Member to retrieve attribute from</param>
        /// <returns></returns>
        public static T GetAttribute<T>(this MemberInfo prop) where T : Attribute => Attribute.GetCustomAttribute(prop, typeof(T)) as T;

        /// <summary>
        ///     Retrieve an attribute from a type
        /// </summary>
        /// <typeparam name="T">Type of attribute to retrieve</typeparam>
        /// <param name="type">Type to retrieve attribute from</param>
        /// <returns></returns>
        public static T GetAttribute<T>(this Type type) where T : Attribute => Attribute.GetCustomAttribute(type, typeof(T)) as T;

        /// <summary>
        ///     Checks a type to see if it derives from a raw generic (e.g. List[[]])
        /// </summary>
        /// <param name="toCheck"></param>
        /// <param name="generic"></param>
        /// <returns></returns>
        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.GetTypeInfo().IsGenericType
                    ? toCheck.GetGenericTypeDefinition()
                    : toCheck;

                if (generic == cur) return true;

                toCheck = toCheck.GetTypeInfo().BaseType;
            }

            return false;
        }

        internal static object ChangeType(this object source, Type newType) => Convert.ChangeType(source, newType);

        /// <summary>
        ///     Find a value from a System.Enum by trying several possible variants
        ///     of the string value of the enum.
        /// </summary>
        /// <param name="type">Type of enum</param>
        /// <param name="value">Value for which to search</param>
        /// <param name="culture">The culture used to calculate the name variants</param>
        /// <returns></returns>
        public static object FindEnumValue(this Type type, string value, CultureInfo culture)
        {
            var caseInsensitiveComparer = StringComparer.Create(culture, true);

            var ret = Enum.GetValues(type)
                .Cast<Enum>()
                .FirstOrDefault(
                    v => v.ToString()
                        .GetNameVariants(culture)
                        .Contains(value, caseInsensitiveComparer)
                );

            if (ret != null) return ret;

            var enumValueAsUnderlyingType = Convert.ChangeType(value, Enum.GetUnderlyingType(type), culture);

            if (enumValueAsUnderlyingType != null && Enum.IsDefined(type, enumValueAsUnderlyingType))
                ret = (Enum) Enum.ToObject(type, enumValueAsUnderlyingType);

            return ret;
        }
    }
}