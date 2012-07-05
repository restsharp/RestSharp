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
using System.Xml.Linq;

using RestSharp.Extensions;
using System.Globalization;
using System.Xml;

namespace RestSharp.Deserializers
{
	public class XmlDeserializer : IDeserializer
	{
		public string RootElement { get; set; }
		public string Namespace { get; set; }
		public string DateFormat { get; set; }
		public CultureInfo Culture { get; set; }

		public XmlDeserializer()
		{
			Culture = CultureInfo.InvariantCulture;
		}

		public T Deserialize<T>(IRestResponse response)
		{
			if (string.IsNullOrEmpty( response.Content ))
				return default(T);

			var doc = XDocument.Parse(response.Content);
			var root = doc.Root;
			if (RootElement.HasValue() && doc.Root != null)
			{
				root = doc.Root.Element(RootElement.AsNamespaced(Namespace));
			}

			// autodetect xml namespace
			if (!Namespace.HasValue())
			{
				RemoveNamespace(doc);
			}

			var x = Activator.CreateInstance<T>();
			var objType = x.GetType();

			if (objType.IsSubclassOfRawGeneric(typeof(List<>)))
			{
				x = (T)HandleListDerivative(x, root, objType.Name, objType);
			}
			else
			{
				Map(x, root);
			}

			return x;
		}

		void RemoveNamespace(XDocument xdoc)
		{
			foreach (XElement e in xdoc.Root.DescendantsAndSelf())
			{
				if (e.Name.Namespace != XNamespace.None)
				{
					e.Name = XNamespace.None.GetName(e.Name.LocalName);
				}
				if (e.Attributes().Any(a => a.IsNamespaceDeclaration || a.Name.Namespace != XNamespace.None))
				{
					e.ReplaceAttributes(e.Attributes().Select(a => a.IsNamespaceDeclaration ? null : a.Name.Namespace != XNamespace.None ? new XAttribute(XNamespace.None.GetName(a.Name.LocalName), a.Value) : a));
				}
			}
		}

		private void Map(object x, XElement root)
		{
			var objType = x.GetType();
			var props = objType.GetProperties();

			foreach (var prop in props)
			{
				var type = prop.PropertyType;

				if (!type.IsPublic || !prop.CanWrite)
					continue;

				var name = prop.Name.AsNamespaced(Namespace);
				var value = GetValueFromXml(root, name);

				if (value == null)
				{
					// special case for inline list items
					if (type.IsGenericType)
					{
						var genericType = type.GetGenericArguments()[0];
						var first = GetElementByName(root, genericType.Name);
						var list = (IList)Activator.CreateInstance(type);

						if (first != null)
						{
							var elements = root.Elements(first.Name);
							PopulateListFromElements(genericType, elements, list);
						}

						prop.SetValue(x, list, null);
					}
					continue;
				}

				// check for nullable and extract underlying type
				if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				{
                    // if the value is empty, set the property to null...
                    if (value == null || String.IsNullOrEmpty(value.ToString()))
                    {
                        prop.SetValue(x, null, null);
                        continue;
                    }
                    type = type.GetGenericArguments()[0];
				}

				if (type == typeof(bool))
				{
					var toConvert = value.ToString().ToLower();
					prop.SetValue(x, XmlConvert.ToBoolean(toConvert), null);
				}
				else if (type.IsPrimitive)
				{
					prop.SetValue(x, value.ChangeType(type, Culture), null);
				}
				else if (type.IsEnum)
				{
					var converted = type.FindEnumValue(value.ToString(), Culture);
					prop.SetValue(x, converted, null);
				}
				else if (type == typeof(Uri))
				{
					var uri = new Uri(value.ToString(), UriKind.RelativeOrAbsolute);
					prop.SetValue(x, uri, null);
				}
				else if (type == typeof(string))
				{
					prop.SetValue(x, value, null);
				}
				else if (type == typeof(DateTime))
				{
					if (DateFormat.HasValue())
					{
						value = DateTime.ParseExact(value.ToString(), DateFormat, Culture);
					}
					else
					{
						value = DateTime.Parse(value.ToString(), Culture);
					}

					prop.SetValue(x, value, null);
				}
				else if (type == typeof(Decimal))
				{
					value = Decimal.Parse(value.ToString(), Culture);
					prop.SetValue(x, value, null);
				}
				else if (type == typeof(Guid))
				{
					var raw = value.ToString();
					value = string.IsNullOrEmpty(raw) ? Guid.Empty : new Guid(value.ToString());
					prop.SetValue(x, value, null);
                }
                else if (type == typeof(TimeSpan))
                {
                    var timeSpan = XmlConvert.ToTimeSpan(value.ToString());
                    prop.SetValue(x, timeSpan, null);
                }
				else if (type.IsGenericType)
				{
					var t = type.GetGenericArguments()[0];
					var list = (IList)Activator.CreateInstance(type);

					var container = GetElementByName(root, prop.Name.AsNamespaced(Namespace));

					if (container.HasElements)
					{
						var first = container.Elements().FirstOrDefault();
						var elements = container.Elements(first.Name);
						PopulateListFromElements(t, elements, list);
					}

					prop.SetValue(x, list, null);
				}
				else if (type.IsSubclassOfRawGeneric(typeof(List<>)))
				{
					// handles classes that derive from List<T>
					// e.g. a collection that also has attributes
					var list = HandleListDerivative(x, root, prop.Name, type);
					prop.SetValue(x, list, null);
				}
				else
				{
					// nested property classes
					if (root != null)
					{
						var element = GetElementByName(root, name);
						if (element != null)
						{
							var item = CreateAndMap(type, element);
							prop.SetValue(x, item, null);
						}
					}
				}
			}
		}

		private void PopulateListFromElements(Type t, IEnumerable<XElement> elements, IList list)
		{
			foreach (var element in elements)
			{
				var item = CreateAndMap(t, element);
				list.Add(item);
			}
		}

		private object HandleListDerivative(object x, XElement root, string propName, Type type)
		{
			Type t;

			if (type.IsGenericType)
			{
				t = type.GetGenericArguments()[0];
			}
			else
			{
				t = type.BaseType.GetGenericArguments()[0];
			}


			var list = (IList)Activator.CreateInstance(type);

			var elements = root.Descendants(t.Name.AsNamespaced(Namespace));
			
			var name = t.Name;

			if (!elements.Any())
			{
				var lowerName = name.ToLower().AsNamespaced(Namespace);
				elements = root.Descendants(lowerName);
			}

			if (!elements.Any())
			{
				var camelName = name.ToCamelCase(Culture).AsNamespaced(Namespace);
				elements = root.Descendants(camelName);
			}

			if (!elements.Any())
			{
				elements = root.Descendants().Where(e => e.Name.LocalName.RemoveUnderscoresAndDashes() == name);
			}

			if (!elements.Any())
			{
				var lowerName = name.ToLower().AsNamespaced(Namespace);
				elements = root.Descendants().Where(e => e.Name.LocalName.RemoveUnderscoresAndDashes() == lowerName);
			}

			PopulateListFromElements(t, elements, list);

			// get properties too, not just list items
			// only if this isn't a generic type
			if (!type.IsGenericType)
			{
				Map(list, root.Element(propName.AsNamespaced(Namespace)) ?? root); // when using RootElement, the heirarchy is different
			}

			return list;
		}

		private object CreateAndMap(Type t, XElement element)
		{
			object item;
			if (t == typeof(String))
			{
				item = element.Value;
			}
			else if (t.IsPrimitive)
			{
				item = element.Value.ChangeType(t, Culture);
			}
			else
			{
				item = Activator.CreateInstance(t);
				Map(item, element);
			}

			return item;
		}

		private object GetValueFromXml(XElement root, XName name)
		{
			object val = null;

			if (root != null)
			{
				var element = GetElementByName(root, name);
				if (element == null)
				{
					var attribute = GetAttributeByName(root, name);
					if (attribute != null)
					{
						val = attribute.Value;
					}
				}
				else
				{
					if (!element.IsEmpty || element.HasElements || element.HasAttributes)
					{
						val = element.Value;
					}
				}
			}

			return val;
		}

		private XElement GetElementByName(XElement root, XName name)
		{
			var lowerName = name.LocalName.ToLower().AsNamespaced(name.NamespaceName);
			var camelName = name.LocalName.ToCamelCase(Culture).AsNamespaced(name.NamespaceName);

			if (root.Element(name) != null)
			{
				return root.Element(name);
			}

			if (root.Element(lowerName) != null)
			{
				return root.Element(lowerName);
			}

			if (root.Element(camelName) != null)
			{
				return root.Element(camelName);
			}

			if (name == "Value".AsNamespaced(name.NamespaceName))
			{
				return root;
			}

			// try looking for element that matches sanitized property name (Order by depth)
			var element = root.Descendants()
				.OrderBy(d => d.Ancestors().Count())
				.FirstOrDefault(d => d.Name.LocalName.RemoveUnderscoresAndDashes() == name.LocalName) 
				?? root.Descendants()
				.OrderBy(d => d.Ancestors().Count())
				.FirstOrDefault(d => d.Name.LocalName.RemoveUnderscoresAndDashes() == name.LocalName.ToLower());

			if (element != null)
			{
				return element;
			}

			return null;
		}

		private XAttribute GetAttributeByName(XElement root, XName name)
		{
			var lowerName = name.LocalName.ToLower().AsNamespaced(name.NamespaceName);
			var camelName = name.LocalName.ToCamelCase(Culture).AsNamespaced(name.NamespaceName);

			if (root.Attribute(name) != null)
			{
				return root.Attribute(name);
			}

			if (root.Attribute(lowerName) != null)
			{
				return root.Attribute(lowerName);
			}

			if (root.Attribute(camelName) != null)
			{
				return root.Attribute(camelName);
			}

			// try looking for element that matches sanitized property name
			var element = root.Attributes().FirstOrDefault(d => d.Name.LocalName.RemoveUnderscoresAndDashes() == name.LocalName);
			if (element != null)
			{
				return element;
			}

			return null;
		}
	}
}
