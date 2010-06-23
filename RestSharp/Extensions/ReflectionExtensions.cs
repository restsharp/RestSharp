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
using System.Reflection;

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
		public static T GetAttribute<T>(this MemberInfo prop) where T : Attribute {
			return Attribute.GetCustomAttribute(prop, typeof(T)) as T;
		}

		/// <summary>
		/// Retrieve an attribute from a type
		/// </summary>
		/// <typeparam name="T">Type of attribute to retrieve</typeparam>
		/// <param name="type">Type to retrieve attribute from</param>
		/// <returns></returns>
		public static T GetAttribute<T>(this Type type) where T : Attribute {
			return Attribute.GetCustomAttribute(type, typeof(T)) as T;
		}

		/// <summary>
		/// Checks a type to see if it derives from a raw generic (e.g. List[[]])
		/// </summary>
		/// <param name="toCheck"></param>
		/// <param name="generic"></param>
		/// <returns></returns>
		public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic) {
			while (toCheck != typeof(object)) {
				var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
				if (generic == cur) {
					return true;
				}
				toCheck = toCheck.BaseType;
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
	}
}
