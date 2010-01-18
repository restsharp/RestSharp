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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

using RestSharp.Extensions;

namespace RestSharp.Deserializers
{
	public class JsonDeserializer : IDeserializer
	{
		public string RootElement { get; set; }
		public string Namespace { get; set; }
		public DateFormat DateFormat { get; set; }

		public T Deserialize<T>(string content) where T : new() {
			var target = new T();

			if (target is IList) {
				var objType = target.GetType();
				JArray json = JArray.Parse(content);
				target = (T)BuildList(objType, json.Root.Children());
			}
			else {
				JObject json = JObject.Parse(content);
				JToken root = json.Root;

				if (RootElement.HasValue())
					root = json[RootElement];

				Map(target, root);
			}

			return target;
		}

		private void Map(object x, JToken json) {
			var objType = x.GetType();
			var props = objType.GetProperties().Where(p => p.CanWrite).ToList();

			foreach (var prop in props) {
				var type = prop.PropertyType;

				var name = prop.Name;
				var value = json[name];
				var actualName = name;

				if (value == null) {
					// try camel cased name
					actualName = name.ToCamelCase();
					value = json[actualName];
				}

				if (value == null) {
					// try lower cased name
					actualName = name.ToLower();
					value = json[actualName];
				}

				if (value == null) {
					// try name with underscores
					actualName = name.AddUnderscores();
					value = json[actualName];
				}

				if (value == null) {
					// try name with underscores with lower case
					actualName = name.AddUnderscores().ToLower();
					value = json[actualName];
				}

				if (value == null || value.Type == JTokenType.Null)
					continue;

				if (type.IsPrimitive) {
					// no primitives can contain quotes so we can safely remove them
					// allows converting a json value like {"index": "1"} to an int
					var tmpVal = value.ToString().Replace("\"", string.Empty);
					prop.SetValue(x, Convert.ChangeType(tmpVal, type), null);
				}
				else if (type == typeof(string)) {
					string raw = value.ToString();
					// remove leading and trailing "
					prop.SetValue(x, raw.Substring(1, raw.Length - 2), null);
				}
				else if (type == typeof(DateTime)) {
					var dt = value.ToString().ParseJsonDate();
					prop.SetValue(x, dt, null);
				}
				else if (type == typeof(Decimal)) {
					var dec = Decimal.Parse(value.ToString());
					prop.SetValue(x, dec, null);
				}
				else if (type.IsGenericType) {
					var genericTypeDef = type.GetGenericTypeDefinition();
					if (genericTypeDef == typeof(List<>)) {
						var list = BuildList(type, value.Children());
						prop.SetValue(x, list, null);
					}
					else if (genericTypeDef == typeof(Dictionary<,>)) {
						var keyType = type.GetGenericArguments()[0];

						// only supports Dict<string, T>()
						if (keyType == typeof(string)) {
							var dict = BuildDictionary(type, value.Children());
							prop.SetValue(x, dict, null);
						}
					}
				}
				else {
					// nested property classes
					var item = CreateAndMap(type, json[actualName]);
					prop.SetValue(x, item, null);
				}
			}
		}

		private object CreateAndMap(Type type, JToken element) {
			object instance = null;
			if (type.IsGenericType) {
				var genericTypeDef = type.GetGenericTypeDefinition();
				if (genericTypeDef == typeof(Dictionary<,>)) {
					instance = BuildDictionary(type, element.Children());
				}
				else if (genericTypeDef == typeof(List<>)) {
					instance = BuildList(type, element.Children());
				}
			}
			else {
				instance = Activator.CreateInstance(type);
				Map(instance, element);
			}
			return instance;
		}

		private IDictionary BuildDictionary(Type type, JEnumerable<JToken> elements) {
			var dict = (IDictionary)Activator.CreateInstance(type);
			var valueType = type.GetGenericArguments()[1];
			foreach (JProperty child in elements) {
				var key = child.Name;
				var item = CreateAndMap(valueType, child.Value);
				dict.Add(key, item);
			}
			return dict;
		}

		private IList BuildList(Type type, JEnumerable<JToken> elements) {
			var list = (IList)Activator.CreateInstance(type);
			var itemType = type.GetGenericArguments()[0];

			foreach (var element in elements) {
				var item = CreateAndMap(itemType, element);
				list.Add(item);
			}
			return list;
		}
	}
}