﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using RestSharp.Extensions;
using System.Globalization;

namespace RestSharp.Deserializers
{
	public class JsonDeserializer : IDeserializer
	{
		public string RootElement { get; set; }
		public string Namespace { get; set; }
		public string DateFormat { get; set; }
		public CultureInfo Culture { get; set; }

		public JsonDeserializer()
		{
			Culture = CultureInfo.InvariantCulture;
		}

		public T Deserialize<T>(IRestResponse response)
		{
			var target = Activator.CreateInstance<T>();

			if (target is IList)
			{
				var objType = target.GetType();

				if (RootElement.HasValue())
				{
					var root = FindRoot(response.Content);
					target = (T)BuildList(objType, root);
				}
				else
				{
					var data = SimpleJson.DeserializeObject(response.Content);
					target = (T)BuildList(objType, data);
				}
			}
			else if (target is IDictionary)
			{
				var root = FindRoot(response.Content);
				target = (T)BuildDictionary(target.GetType(), root);
			}
			else
			{
				var root = FindRoot(response.Content);
				Map(target, (IDictionary<string, object>)root);
			}

			return target;
		}

		private object FindRoot(string content)
		{
			var data = (IDictionary<string, object>)SimpleJson.DeserializeObject(content);
			if (RootElement.HasValue() && data.ContainsKey(RootElement))
			{
				return data[RootElement];
			}
			return data;
		}

		private void Map(object target, IDictionary<string, object> data)
		{
			var objType = target.GetType();
			var props = objType.GetProperties().Where(p => p.CanWrite).ToList();

			foreach (var prop in props)
			{
				var type = prop.PropertyType;

				var name = prop.Name;
				var actualName = name.GetNameVariants(Culture).FirstOrDefault(n => data.ContainsKey(n));
				var value = actualName != null ? data[actualName] : null;

				if (value == null) continue;

                var stringValue = Convert.ToString(value, Culture);

				// check for nullable and extract underlying type
				if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				{
					type = type.GetGenericArguments()[0];
				}

				if (type.IsPrimitive)
				{
					// no primitives can contain quotes so we can safely remove them
					// allows converting a json value like {"index": "1"} to an int
					var tmpVal = stringValue.Replace("\"", string.Empty);
					prop.SetValue(target, tmpVal.ChangeType(type, Culture), null);
				}
				else if (type.IsEnum)
				{
					var converted = type.FindEnumValue(stringValue, Culture);
					prop.SetValue(target, converted, null);
				}
				else if (type == typeof(Uri))
				{
					var uri = new Uri(stringValue, UriKind.RelativeOrAbsolute);
					prop.SetValue(target, uri, null);
				}
				else if (type == typeof(string))
				{
					prop.SetValue(target, stringValue, null);
				}
				else if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
				{
					DateTime dt;
					if (DateFormat.HasValue())
					{
						dt = DateTime.ParseExact(stringValue, DateFormat, Culture);
					}
					else
					{
						// try parsing instead
						dt = stringValue.ParseJsonDate(Culture);
					}

					if (type == typeof(DateTime))
					{
						prop.SetValue(target, dt, null);
					}
					else if (type == typeof(DateTimeOffset))
					{
						prop.SetValue(target, (DateTimeOffset)dt, null);
					}
				}
				else if (type == typeof(Decimal))
				{
					var dec = Decimal.Parse(stringValue, Culture);
					prop.SetValue(target, dec, null);
				}
				else if (type == typeof(Guid))
				{
					var guid = string.IsNullOrEmpty(stringValue) ? Guid.Empty : new Guid(stringValue);
					prop.SetValue(target, guid, null);
                }
                else if (type == typeof(TimeSpan))
                {
                    var timeSpan = TimeSpan.Parse(stringValue);
                    prop.SetValue(target, timeSpan, null);
                }
				else if (type.IsGenericType)
				{
					var genericTypeDef = type.GetGenericTypeDefinition();
					if (genericTypeDef == typeof(List<>))
					{
						var list = BuildList(type, value);
						prop.SetValue(target, list, null);
					}
					else if (genericTypeDef == typeof(Dictionary<,>))
					{
						var keyType = type.GetGenericArguments()[0];

						// only supports Dict<string, T>()
						if (keyType == typeof(string))
						{
							var dict = BuildDictionary(type, value);
							prop.SetValue(target, dict, null);
						}
					}
					else
					{
						// nested property classes
						var item = CreateAndMap(type, data[actualName]);
						prop.SetValue(target, item, null);
					}
				}
				else
				{
					// nested property classes
					var item = CreateAndMap(type, data[actualName]);
					prop.SetValue(target, item, null);
				}
			}


		}

		private IDictionary BuildDictionary(Type type, object parent)
		{
			var dict = (IDictionary)Activator.CreateInstance(type);
			var valueType = type.GetGenericArguments()[1];
			foreach (var child in (IDictionary<string, object>)parent)
			{
				var key = child.Key;
				var item = CreateAndMap(valueType, child.Value);
				dict.Add(key, item);
			}

			return dict;
		}

		private IList BuildList(Type type, object parent)
		{
			var list = (IList)Activator.CreateInstance(type);
			var itemType = type.GetGenericArguments()[0];

			foreach (var element in (IList)parent)
			{
				if (itemType.IsPrimitive)
				{
					var value = element.ToString();
					list.Add(value.ChangeType(itemType, Culture));
				}
				else if (itemType == typeof(string))
				{
					if (element != null)
					{
						list.Add(element.ToString());
					}
					else
					{
						list.Add(element);
					}
				}
				else
				{
					var item = CreateAndMap(itemType, element);
					list.Add(item);
				}
			}
			return list;
		}

		private object CreateAndMap(Type type, object element)
		{
			object instance = null;
			if (type.IsGenericType)
			{
				var genericTypeDef = type.GetGenericTypeDefinition();
				if (genericTypeDef == typeof(Dictionary<,>))
				{
					instance = BuildDictionary(type, element);
				}
				else if (genericTypeDef == typeof(List<>))
				{
					instance = BuildList(type, element);
				}
				else if (type == typeof(string))
				{
					instance = (string)element;
				}
				else
				{
					instance = Activator.CreateInstance(type);
					Map(instance, (IDictionary<string, object>)element);
				}
			}
			else if (type == typeof(string))
			{
				instance = (string)element;
			}
			else
			{
				instance = Activator.CreateInstance(type);
				var data = (IDictionary<string, object>)element;
				Map(instance, data);
			}
			return instance;
		}

	}
}