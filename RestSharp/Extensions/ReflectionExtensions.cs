#region License

//   Copyright 2010 John Sheehan
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

#endregion

using System;
using System.Globalization;
using System.Reflection;

#if FRAMEWORK
using System.Linq;
#endif

namespace RestSharp.Extensions
{
    /// <summary>
    /// Reflection extensions
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Retrieve an attribute from a member (property)
        /// </summary>
        /// <typeparam name="T">Type of attribute to retrieve</typeparam>
        /// <param name="prop">Member to retrieve attribute from</param>
        /// <returns></returns>
        public static T GetAttribute<T>(this MemberInfo prop) where T : Attribute
        {           
#if !WINDOWS_UWP
            return Attribute.GetCustomAttribute(prop, typeof(T)) as T;
#else
            return prop.GetCustomAttribute(typeof(T)) as T;
#endif
        }

        /// <summary>
        /// Retrieve an attribute from a type
        /// </summary>
        /// <typeparam name="T">Type of attribute to retrieve</typeparam>
        /// <param name="type">Type to retrieve attribute from</param>
        /// <returns></returns>
        public static T GetAttribute<T>(this Type type) where T : Attribute
        {
            //type.GetTypeInfo().getcu
#if !WINDOWS_UWP
            return Attribute.GetCustomAttribute(type, typeof(T)) as T;
#else
            return type.GetTypeInfo().GetCustomAttribute(type) as T;
#endif
        }

        /// <summary>
        /// Checks a type to see if it derives from a raw generic (e.g. List[[]])
        /// </summary>
        /// <param name="toCheck"></param>
        /// <param name="generic"></param>
        /// <returns></returns>
        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
#if !WINDOWS_UWP
                Type cur = toCheck.IsGenericType
                    ? toCheck.GetGenericTypeDefinition()
                    : toCheck;
#else
                Type cur = toCheck.GetTypeInfo().IsGenericType
                    ? toCheck.GetGenericTypeDefinition()
                    : toCheck;
#endif

                if (generic == cur)
                {
                    return true;
                }
#if !WINDOWS_UWP
                toCheck = toCheck.BaseType;
#else
                toCheck = toCheck.GetTypeInfo().BaseType;
#endif
            }

            return false;
        }

        public static object ChangeType(this object source, Type newType)
        {
#if FRAMEWORK
            return Convert.ChangeType(source, newType);
#else
            return Convert.ChangeType(source, newType, null);
#endif
        }

        public static object ChangeType(this object source, Type newType, CultureInfo culture)
        {
#if FRAMEWORK || SILVERLIGHT || WINDOWS_PHONE
            return Convert.ChangeType(source, newType, culture);
#else
            return Convert.ChangeType(source, newType, null);
#endif
        }

        /// <summary>
        /// Find a value from a System.Enum by trying several possible variants
        /// of the string value of the enum.
        /// </summary>
        /// <param name="type">Type of enum</param>
        /// <param name="value">Value for which to search</param>
        /// <param name="culture">The culture used to calculate the name variants</param>
        /// <returns></returns>
        public static object FindEnumValue(this Type type, string value, CultureInfo culture)
        {
#if FRAMEWORK
            Enum ret = Enum.GetValues(type)
                           .Cast<Enum>()
                           .FirstOrDefault(v => v.ToString()
                                                 .GetNameVariants(culture)
                                                 .Contains(value, StringComparer.Create(culture, true)));

            if (ret == null)
            {
                object enumValueAsUnderlyingType = Convert.ChangeType(value, Enum.GetUnderlyingType(type), culture);

                if (enumValueAsUnderlyingType != null && Enum.IsDefined(type, enumValueAsUnderlyingType))
                {
                    ret = (Enum) Enum.ToObject(type, enumValueAsUnderlyingType);
                }
            }

            return ret;
#else
            return Enum.Parse(type, value, true);
#endif
        }
    }
}
